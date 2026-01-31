"use client";

import { useEffect, useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { apiJson } from "@/lib/api-client";

type TopDoctorDto = { doctorId: number; doctorName: string; appointmentCount: number };

export default function AdminReportsPage() {
  const [days, setDays] = useState("30");
  const [take, setTake] = useState("10");
  const [items, setItems] = useState<TopDoctorDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  async function load() {
    setIsLoading(true);
    setError(null);
    try {
      const data = await apiJson<TopDoctorDto[]>(
        `/backend/admin/reports/top-doctors?days=${encodeURIComponent(days)}&take=${encodeURIComponent(take)}`
      );
      setItems(data);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Yükleme başarısız.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <div className="grid gap-6">
      <PageHeader title="Raporlar" subtitle="En çok randevu alan doktorlar." />

      <Card>
        <div className="grid gap-3 sm:grid-cols-[1fr_1fr_auto] sm:items-end">
          <Input label="Gün" value={days} onChange={(e) => setDays(e.target.value)} />
          <Input label="Adet" value={take} onChange={(e) => setTake(e.target.value)} />
          <Button onClick={load}>Getir</Button>
        </div>
      </Card>

      {error ? <Card><p className="text-sm text-red-600">{error}</p></Card> : null}
      {isLoading ? <Card><p className="text-sm text-zinc-600">Yükleniyor…</p></Card> : null}

      <Card>
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="text-xs text-zinc-600">
              <tr>
                <th className="py-2">Doktor</th>
                <th className="py-2">Randevu</th>
              </tr>
            </thead>
            <tbody>
              {items.map((d) => (
                <tr key={d.doctorId} className="border-t border-black/5">
                  <td className="py-2">{d.doctorName}</td>
                  <td className="py-2">{d.appointmentCount}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>
    </div>
  );
}
