"use client";

import { useState } from "react";
import Link from "next/link";
import { Card } from "@/components/ui/card";
import { TurkeyMap } from "@/components/sma/TurkeyMap";
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

export default function SmaPage() {
  const [provinceId, setProvinceId] = useState<string | null>(null);
  const [provinceName, setProvinceName] = useState<string | null>(null);
  const [cases, setCases] = useState<SmaCaseDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function selectProvince(id: string, name: string) {
    setProvinceId(id);
    setProvinceName(name);
    setIsLoading(true);
    setError(null);
    try {
      const result = await apiJson<SmaCaseDto[]>(`/backend/sma/cases?province=${encodeURIComponent(id)}`);
      setCases(result);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Yükleme başarısız.");
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-3xl font-bold text-slate-900 dark:text-white">SMA Bağış Dizini</h1>
        <p className="mt-2 max-w-2xl text-slate-600 dark:text-slate-400">
          Bir ile tıklayarak o ildeki doğrulanmış SMA hastası vakalarını görüntüleyin. Bu platform
          bağış toplamaz veya iletmez — sadece vaka sahiplerinin IBAN bilgisini gösterir, bağışınızı
          doğrudan kendi banka uygulamanızdan gönderirsiniz.
        </p>
      </div>

      <div className="grid gap-6 lg:grid-cols-[1.3fr_1fr]">
        <Card>
          <TurkeyMap selectedProvinceId={provinceId} onSelectProvince={selectProvince} />
        </Card>

        <Card title={provinceName ? `${provinceName} — Vakalar` : "Bir il seçin"}>
          {!provinceId ? (
            <p className="text-sm text-slate-500 dark:text-slate-400">
              Haritadan bir ile tıklayarak o ildeki vakaları listeleyin.
            </p>
          ) : isLoading ? (
            <p className="text-sm text-slate-500 dark:text-slate-400">Yükleniyor…</p>
          ) : error ? (
            <p className="text-sm text-red-600">{error}</p>
          ) : cases.length === 0 ? (
            <p className="text-sm text-slate-500 dark:text-slate-400">
              Bu ilde şu an doğrulanmış bir vaka bulunmuyor.
            </p>
          ) : (
            <ul className="space-y-3">
              {cases.map((c) => (
                <li key={c.slug}>
                  <Link
                    href={`/sma/${c.slug}`}
                    className="block rounded-lg border border-slate-200 p-3 transition hover:border-blue-400 hover:bg-blue-50 dark:border-slate-700 dark:hover:bg-slate-800"
                  >
                    <p className="font-semibold text-slate-900 dark:text-white">{c.displayName}</p>
                    <p className="text-sm text-slate-500 dark:text-slate-400">Detayları görüntüle →</p>
                  </Link>
                </li>
              ))}
            </ul>
          )}
        </Card>
      </div>
    </div>
  );
}
