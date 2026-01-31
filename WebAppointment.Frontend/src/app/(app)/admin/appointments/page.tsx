"use client";

import { useEffect, useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { apiJson } from "@/lib/api-client";

type AdminAppointmentDto = {
  id: string;
  userId: string;
  userEmail: string;
  doctorId: number;
  doctorName: string;
  departmentName: string;
  appointmentDateUtc: string;
  status: string;
};

export default function AdminAppointmentsPage() {
  const [items, setItems] = useState<AdminAppointmentDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    (async () => {
      setIsLoading(true);
      setError(null);
      try {
        const data = await apiJson<AdminAppointmentDto[]>("/backend/admin/appointments");
        setItems(data);
      } catch (e) {
        setError(e instanceof Error ? e.message : "Yükleme başarısız.");
      } finally {
        setIsLoading(false);
      }
    })();
  }, []);

  return (
    <div className="grid gap-6">
      <PageHeader title="Randevular" subtitle="Tüm randevu kayıtları." />
      {error ? <Card><p className="text-sm text-red-600">{error}</p></Card> : null}
      {isLoading ? <Card><p className="text-sm text-zinc-600">Yükleniyor…</p></Card> : null}

      <Card>
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="text-xs text-zinc-600">
              <tr>
                <th className="py-2">Tarih (UTC)</th>
                <th className="py-2">Durum</th>
                <th className="py-2">Hasta</th>
                <th className="py-2">Doktor</th>
                <th className="py-2">Bölüm</th>
              </tr>
            </thead>
            <tbody>
              {items.map((a) => (
                <tr key={a.id} className="border-t border-black/5">
                  <td className="py-2 whitespace-nowrap">{new Date(a.appointmentDateUtc).toISOString()}</td>
                  <td className="py-2">{a.status}</td>
                  <td className="py-2">{a.userEmail}</td>
                  <td className="py-2">{a.doctorName}</td>
                  <td className="py-2">{a.departmentName}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>
    </div>
  );
}
