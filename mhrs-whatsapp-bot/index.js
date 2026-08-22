const wppconnect = require('@wppconnect-team/wppconnect');
const express = require('express');
const app = express();
app.use(express.json());

// .NET API'nin gelen WhatsApp yanıtlarını alacağı webhook.
// Her iki tarafta da aynı secret kullanılmalı (appsettings.json -> WhatsAppBridge:WebhookSecret).
const DOTNET_API_BASE_URL = process.env.DOTNET_API_BASE_URL || 'http://localhost:5233';
const BRIDGE_WEBHOOK_SECRET = process.env.BRIDGE_WEBHOOK_SECRET || 'CHANGE_ME_LOCAL_DEV_SECRET';

let whatsappClient;
let latestQr = null;
let connectionStatus = 'starting'; // starting | qr | connected | disconnected

// WhatsApp'ı Başlat
wppconnect.create({
    session: 'MHRS-Session',
    catchQR: (base64Qr) => {
        latestQr = base64Qr;
        connectionStatus = 'qr';
    },
    statusFind: (status) => {
        console.log('Durum:', status);
        if (status === 'isLogged' || status === 'inChat' || status === 'chatsAvailable') {
            connectionStatus = 'connected';
            latestQr = null;
        } else if (status === 'notLogged' || status === 'browserClose' || status === 'autocloseCalled') {
            connectionStatus = 'disconnected';
        }
    }
})
.then((client) => {
    whatsappClient = client;
    connectionStatus = 'connected';
    latestQr = null;
    console.log('WhatsApp Bağlantısı Hazır!');

    // Gelen mesajları .NET API'ye ilet (randevu onay/iptal yanıtları için).
    client.onMessage(async (message) => {
        if (message.isGroupMsg || message.fromMe || !message.body) {
            return;
        }

        try {
            const response = await fetch(`${DOTNET_API_BASE_URL}/api/whatsapp/inbound`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Bridge-Secret': BRIDGE_WEBHOOK_SECRET
                },
                body: JSON.stringify({ from: message.from, body: message.body })
            });

            if (!response.ok) {
                console.log('Webhook iletimi basarisiz:', response.status, await response.text());
            }
        } catch (error) {
            console.log('Webhook iletim hatasi:', error.message);
        }
    });
})
.catch((error) => console.log(error));

// Yönetim panelinin QR/bağlantı durumunu okuması için.
app.get('/status', (req, res) => res.json({ status: connectionStatus }));
app.get('/qr', (req, res) => res.json({ qr: latestQr }));

// .NET 8'in Mesaj Atacağı Endpoint
app.post('/send-message', async (req, res) => {
    const { phone, message } = req.body;
    try {
        await whatsappClient.sendText(`${phone}@c.us`, message);
        res.status(200).send({ success: true });
    } catch (error) {
        res.status(500).send({ error: error.message });
    }
});

app.listen(8080, () => console.log('Bridge API 8080 portunda çalışıyor...'));