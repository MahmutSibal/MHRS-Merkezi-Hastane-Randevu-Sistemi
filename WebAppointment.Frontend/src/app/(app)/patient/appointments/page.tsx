"use client";

import { useEffect, useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { useToast } from "@/components/session/ToastProvider";
import { apiJson } from "@/lib/api-client";

type AppointmentDto = {
  id: string;
  userId: string;
  doctorId: number;
  doctorName: string;
  departmentName: string;
  appointmentDateUtc: string;
  status: string;
};

export default function PatientAppointmentsPage() {
  const toast = useToast();
  const [items, setItems] = useState<AppointmentDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  async function load() {
    setIsLoading(true);
    setError(null);
    try {
      const data = await apiJson<AppointmentDto[]>("/backend/appointments/my");
      setItems(data);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Yükleme başarısız.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, []);

  async function cancel(id: string) {
    if (!confirm("Randevu iptal edilsin mi?") ) return;
    setError(null);
    try {
      await apiJson<void>(`/backend/appointments/${id}/cancel`, { method: "PUT" });
      toast.success("Randevu başarıyla iptal edildi");
      await load();
    } catch (e) {
      const errorMsg = e instanceof Error ? e.message : "İptal başarısız.";
      setError(errorMsg);
      toast.error(errorMsg);
    }
  }

  return (
    <div className="grid gap-6">
      <PageHeader title="Randevularım" subtitle="Mevcut randevular." />
      {error ? <Card><p className="text-sm text-red-600">{error}</p></Card> : null}
      {isLoading ? <Card><p className="text-sm text-zinc-600">Yükleniyor…</p></Card> : null}

      <div className="grid gap-3">
        {items.map((a) => (
          <Card key={a.id}>
            <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
              <div>
                <div className="text-sm font-medium text-zinc-900">{a.departmentName} • {a.doctorName}</div>
                <div className="mt-1 text-sm text-zinc-600">{new Date(a.appointmentDateUtc).toLocaleString("tr-TR")}</div>
                <div className="mt-1 text-xs text-zinc-600">Durum: {a.status}</div>
              </div>
              <Button variant="danger" onClick={() => cancel(a.id)}>
                İptal
              </Button>
            </div>
          </Card>
        ))}
      </div>
    </div>
  );
}
