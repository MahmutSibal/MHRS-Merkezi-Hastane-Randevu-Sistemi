const wppconnect = require('@wppconnect-team/wppconnect');
const express = require('express');
const app = express();
app.use(express.json());

let whatsappClient;

// WhatsApp'ı Başlat
wppconnect.create({
    session: 'MHRS-Session',
    statusFind: (status) => console.log('Durum:', status)
})
.then((client) => {
    whatsappClient = client;
    console.log('WhatsApp Bağlantısı Hazır!');
})
.catch((error) => console.log(error));

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