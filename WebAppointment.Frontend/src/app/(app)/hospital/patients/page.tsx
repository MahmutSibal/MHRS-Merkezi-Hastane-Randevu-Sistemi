"use client";

import { useEffect, useMemo, useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { apiJson } from "@/lib/api-client";

type PatientDto = {
  id: number;
  userId: string;
  email: string;
  tcKimlikNo: string;
  firstName: string;
  lastName: string;
  phone: string;
};

export default function HospitalPatientsPage() {
  const [items, setItems] = useState<PatientDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [query, setQuery] = useState("");

  useEffect(() => {
    (async () => {
      setIsLoading(true);
      setError(null);
      try {
        const data = await apiJson<PatientDto[]>("/backend/hospital/patients");
        setItems(data);
      } catch (e) {
        setError(e instanceof Error ? e.message : "Yükleme başarısız.");
      } finally {
        setIsLoading(false);
      }
    })();
  }, []);

  const filtered = useMemo(() => {
    const term = query.trim().toLowerCase();
    if (!term) {
      return items;
    }
    return items.filter((p) => {
      const haystack = [p.email, p.firstName, p.lastName, p.phone, p.tcKimlikNo].join(" ").toLowerCase();
      return haystack.includes(term);
    });
  }, [items, query]);

  return (
    <div className="grid gap-6">
      <PageHeader title="Hastalar" subtitle="Hastane kapsamındaki hasta listesi." />
      {error ? <Card><p className="text-sm text-red-600">{error}</p></Card> : null}
      {isLoading ? <Card><p className="text-sm text-zinc-600">Yükleniyor…</p></Card> : null}

      <Card>
        <Input label="Ara" placeholder="E-posta, ad, telefon veya TC" value={query} onChange={(e) => setQuery(e.target.value)} />
      </Card>

      <Card title={`Toplam ${filtered.length} kayıt`}>
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="text-xs text-slate-600">
              <tr>
                <th className="py-2">Hasta</th>
                <th className="py-2">E-posta</th>
                <th className="py-2">Telefon</th>
                <th className="py-2">TC Kimlik</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((p) => (
                <tr key={p.id} className="border-t border-slate-100 hover:bg-slate-50/60 dark:border-slate-800 dark:hover:bg-slate-800/60">
                  <td className="py-2">{p.firstName} {p.lastName}</td>
                  <td className="py-2">{p.email}</td>
                  <td className="py-2">{p.phone}</td>
                  <td className="py-2">{p.tcKimlikNo}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>
    </div>
  );
}
