"use client";

import { use, useEffect, useState } from "react";
import Link from "next/link";
import { Card } from "@/components/ui/card";
import { apiJson } from "@/lib/api-client";

type SmaCaseDto = {
  slug: string;
  displayName: string;
  provinceSlug: string;
  provinceName: string;
  story: string | null;
  iban: string;
  bankAccountHolderName: string;
  photoUrl: string | null;
};

export default function SmaCaseDetailPage({ params }: { params: Promise<{ slug: string }> }) {
  const { slug } = use(params);

  const [item, setItem] = useState<SmaCaseDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [copied, setCopied] = useState(false);

  useEffect(() => {
    let cancelled = false;
    apiJson<SmaCaseDto>(`/backend/sma/cases/${encodeURIComponent(slug)}`)
      .then((data) => {
        if (!cancelled) setItem(data);
      })
      .catch((e) => {
        if (!cancelled) setError(e instanceof Error ? e.message : "Kayıt bulunamadı.");
      })
      .finally(() => {
        if (!cancelled) setIsLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, [slug]);

  async function copyIban() {
    if (!item) return;
    try {
      await navigator.clipboard.writeText(item.iban);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch {
      // clipboard erişimi yoksa sessizce yok say — IBAN zaten ekranda görünür durumda
    }
  }

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <Link href="/sma" className="text-sm font-medium text-blue-600 hover:underline dark:text-blue-400">
        ← Türkiye haritasına dön
      </Link>

      {isLoading ? (
        <Card><p className="text-sm text-slate-500 dark:text-slate-400">Yükleniyor…</p></Card>
      ) : error || !item ? (
        <Card><p className="text-sm text-red-600">{error ?? "Kayıt bulunamadı."}</p></Card>
      ) : (
        <>
          <Card title={item.displayName} description={item.provinceName}>
            {item.photoUrl ? (
              <img
                src={item.photoUrl}
                alt={item.displayName}
                className="mb-4 max-h-80 w-full rounded-lg object-cover"
              />
            ) : null}
            {item.story ? (
              <p className="whitespace-pre-wrap text-slate-700 dark:text-slate-300">{item.story}</p>
            ) : null}
          </Card>

          <Card title="Bağış Bilgisi">
            <p className="mb-4 text-sm text-slate-600 dark:text-slate-400">
              Bu platform bağış toplamaz ya da iletmez. Aşağıdaki IBAN, doğrulanmış hesap sahibine
              aittir — bağışınızı kendi banka uygulamanızdan doğrudan gönderebilirsiniz.
            </p>
            <div className="rounded-lg border border-slate-200 bg-slate-50 p-4 dark:border-slate-700 dark:bg-slate-900">
              <p className="text-xs font-semibold uppercase tracking-wide text-slate-500 dark:text-slate-400">
                Hesap Sahibi
              </p>
              <p className="mb-3 font-medium text-slate-900 dark:text-white">{item.bankAccountHolderName}</p>

              <p className="text-xs font-semibold uppercase tracking-wide text-slate-500 dark:text-slate-400">
                IBAN
              </p>
              <div className="flex flex-wrap items-center gap-3">
                <p className="font-mono text-lg font-semibold text-slate-900 dark:text-white">{item.iban}</p>
                <button
                  type="button"
                  onClick={copyIban}
                  className="rounded-lg border border-slate-300 px-3 py-1.5 text-sm font-medium text-slate-700 transition hover:bg-slate-100 dark:border-slate-600 dark:text-slate-200 dark:hover:bg-slate-800"
                >
                  {copied ? "Kopyalandı ✓" : "IBAN'ı Kopyala"}
                </button>
              </div>
            </div>
          </Card>
        </>
      )}
    </div>
  );
}
