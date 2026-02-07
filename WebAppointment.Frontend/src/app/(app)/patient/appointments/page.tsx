"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { StatusBadge } from "@/components/ui/badge";
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
  dependentId?: number | null;
  dependentFullName?: string | null;
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
    const reason = prompt("İptal gerekçesi (opsiyonel):") ?? "";
    setError(null);
    try {
      await apiJson<void>(`/backend/appointments/${id}/cancel`, {
        method: "PUT",
        body: JSON.stringify({ reason: reason.trim() ? reason.trim() : null }),
      });
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
        {items.map((a) => {
          const status = (a.status || "").toLowerCase();
          const startMs = new Date(a.appointmentDateUtc).getTime();
          const diffMs = startMs - Date.now();
          const isUpcomingEnough = diffMs > 2 * 60 * 60 * 1000;
          const canCancel = (status.includes("pending") || status.includes("approved")) && isUpcomingEnough;
          return (
            <Card key={a.id}>
              <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                <div>
                  <div className="flex items-center gap-2">
                    <div className="text-sm font-medium text-zinc-900 dark:text-slate-100">{a.departmentName} • {a.doctorName}</div>
                    <StatusBadge status={a.status} />
                  </div>
                  <div className="mt-1 text-sm text-zinc-600 dark:text-slate-400">{new Date(a.appointmentDateUtc).toLocaleString("tr-TR")}</div>
                  <div className="mt-1 text-xs text-zinc-500 dark:text-slate-400">
                    Hasta: {a.dependentFullName ? a.dependentFullName : "Kendim"}
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <Link href={`/patient/appointments/${encodeURIComponent(a.id)}`}>
                    <Button variant="secondary">Detay</Button>
                  </Link>
                  <Button variant="danger" onClick={() => cancel(a.id)} disabled={!canCancel}>
                    İptal
                  </Button>
                </div>
              </div>
            </Card>
          );
        })}
      </div>
    </div>
  );
}
