"use client";

import { useEffect, useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { LoadingSpinner } from "@/components/ui/loading-spinner";
import { useToast } from "@/components/session/ToastProvider";
import { apiJson } from "@/lib/api-client";

type DoctorAppointmentDto = {
  id: string;
  patientUserId: string;
  patientEmail: string;
  doctorId: number;
  startAtUtc: string;
  endAtUtc: string;
  status: string;
};

export default function DoctorAppointmentsPage() {
  const toast = useToast();
  const [items, setItems] = useState<DoctorAppointmentDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  async function load() {
    setIsLoading(true);
    setError(null);
    try {
      const data = await apiJson<DoctorAppointmentDto[]>("/backend/doctor/appointments/my");
      setItems(data);
    } catch (e) {
      const errorMsg = e instanceof Error ? e.message : "Yükleme başarısız.";
      setError(errorMsg);
      toast.error(errorMsg);
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, []);

  async function approve(id: string) {
    try {
      await apiJson<void>(`/backend/doctor/appointments/${id}/approve`, { method: "PUT" });
      toast.success("Randevu onaylandı");
      await load();
    } catch (e) {
      const errorMsg = e instanceof Error ? e.message : "Onaylama başarısız";
      toast.error(errorMsg);
    }
  }

  async function complete(id: string) {
    try {
      await apiJson<void>(`/backend/doctor/appointments/${id}/complete`, { method: "PUT" });
      toast.success("Randevu tamamlandı");
      await load();
    } catch (e) {
      const errorMsg = e instanceof Error ? e.message : "Tamamlama başarısız";
      toast.error(errorMsg);
    }
  }

  return (
    <div className="grid gap-6">
      <PageHeader title="Randevularım" subtitle="Onay bekleyenleri yönet." />
      
      {error ? (
        <Card>
          <div className="space-y-2">
            <p className="text-sm font-semibold text-red-700 dark:text-red-400">Hata: {error}</p>
            {error.includes("403") && (
              <p className="text-xs text-red-600 dark:text-red-300">
                Doktor profili oluşturulmamış. Lütfen sistem yöneticisine başvurun.
              </p>
            )}
          </div>
        </Card>
      ) : null}
      
      {isLoading ? <Card><p className="text-sm text-slate-600 dark:text-slate-400">Yükleniyor…</p></Card> : null}

      <div className="grid gap-3">
        {items.map((a) => (
          <Card key={a.id}>
            <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
              <div>
                <div className="text-sm font-medium text-slate-900 dark:text-slate-100">{a.patientEmail}</div>
                <div className="mt-1 text-sm text-slate-600 dark:text-slate-400">
                  {new Date(a.startAtUtc).toLocaleString("tr-TR")} – {new Date(a.endAtUtc).toLocaleTimeString("tr-TR")}
                </div>
                <div className="mt-1 text-xs text-slate-600 dark:text-slate-400">Durum: {a.status}</div>
              </div>
              <div className="flex gap-2">
                <Button variant="secondary" onClick={() => approve(a.id)}>
                  Onayla
                </Button>
                <Button onClick={() => complete(a.id)}>Tamamla</Button>
              </div>
            </div>
          </Card>
        ))}
      </div>
    </div>
  );
}
