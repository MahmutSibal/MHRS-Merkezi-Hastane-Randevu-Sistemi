"use client";

import { useEffect, useRef, useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { apiJson } from "@/lib/api-client";

type Status = "starting" | "qr" | "connected" | "disconnected" | "unreachable";

const STATUS_LABEL: Record<Status, string> = {
  starting: "Başlatılıyor…",
  qr: "Bağlantı bekleniyor — QR kodu okutun",
  connected: "Bağlı",
  disconnected: "Bağlantı kesildi",
  unreachable: "Servise ulaşılamıyor",
};

export default function AdminWhatsAppPage() {
  const [status, setStatus] = useState<Status>("starting");
  const [qr, setQr] = useState<string | null>(null);
  const timerRef = useRef<ReturnType<typeof setInterval> | null>(null);

  async function poll() {
    try {
      const statusRes = await apiJson<{ status: Status }>("/backend/admin/whatsapp/status");
      setStatus(statusRes.status);

      if (statusRes.status === "qr") {
        const qrRes = await apiJson<{ qr: string | null }>("/backend/admin/whatsapp/qr");
        setQr(qrRes.qr);
      } else {
        setQr(null);
      }
    } catch {
      setStatus("unreachable");
      setQr(null);
    }
  }

  useEffect(() => {
    void poll();
    timerRef.current = setInterval(() => void poll(), 3000);
    return () => {
      if (timerRef.current) clearInterval(timerRef.current);
    };
  }, []);

  const badgeClass =
    status === "connected"
      ? "bg-emerald-50 text-emerald-700 dark:bg-emerald-950 dark:text-emerald-200"
      : status === "qr"
        ? "bg-amber-50 text-amber-700 dark:bg-amber-950 dark:text-amber-200"
        : "bg-red-50 text-red-700 dark:bg-red-950 dark:text-red-200";

  return (
    <div className="grid gap-6">
      <PageHeader title="WhatsApp Bağlantısı" subtitle="Hatırlatma ve doğrulama mesajlarının gönderildiği WhatsApp hattı." />

      <Card>
        <div className="flex flex-col items-center gap-4 py-6 text-center">
          <span className={`rounded-full px-4 py-1.5 text-sm font-semibold ${badgeClass}`}>
            {STATUS_LABEL[status]}
          </span>

          {status === "qr" && qr ? (
            <div className="rounded-2xl border border-slate-200 bg-white p-4 dark:border-slate-700 dark:bg-slate-900">
              <img src={qr} alt="WhatsApp QR kodu" className="h-64 w-64" />
              <p className="mt-3 max-w-xs text-sm text-slate-600 dark:text-slate-400">
                Telefonunuzda WhatsApp → Bağlı Cihazlar → Cihaz Bağla ile bu kodu okutun.
              </p>
            </div>
          ) : null}

          {status === "starting" ? (
            <p className="max-w-sm text-sm text-slate-600 dark:text-slate-400">
              Bridge servisi başlatılıyor, birkaç saniye içinde QR kodu burada görünecek.
            </p>
          ) : null}

          {status === "unreachable" ? (
            <p className="max-w-sm text-sm text-slate-600 dark:text-slate-400">
              Servise ulaşılamıyor. API başlatılırken otomatik başlatılmış olmalı — birkaç saniye
              sonra tekrar denenecek.
            </p>
          ) : null}

          {status === "connected" ? (
            <p className="max-w-sm text-sm text-slate-600 dark:text-slate-400">
              WhatsApp bağlı — hatırlatma ve doğrulama mesajları bu hat üzerinden gönderiliyor.
            </p>
          ) : null}
        </div>
      </Card>
    </div>
  );
}
