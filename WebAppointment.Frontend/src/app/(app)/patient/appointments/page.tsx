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

  // Reschedule modal state
  const [rescheduleId, setRescheduleId] = useState<string | null>(null);
  const [rescheduleDate, setRescheduleDate] = useState("");
  const [rescheduleTime, setRescheduleTime] = useState("");
  const [isRescheduling, setIsRescheduling] = useState(false);

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
    if (!confirm("Randevu iptal edilsin mi?")) return;
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

  function openReschedule(id: string) {
    setRescheduleId(id);
    setRescheduleDate("");
    setRescheduleTime("");
  }

  async function submitReschedule() {
    if (!rescheduleId || !rescheduleDate || !rescheduleTime) return;
    setIsRescheduling(true);
    setError(null);
    try {
      const isoDate = `${rescheduleDate}T${rescheduleTime}:00`;
      const offset = new Date(isoDate).getTimezoneOffset();
      const sign = offset <= 0 ? "+" : "-";
      const absH = String(Math.floor(Math.abs(offset) / 60)).padStart(2, "0");
      const absM = String(Math.abs(offset) % 60).padStart(2, "0");
      const newAppointmentDate = `${isoDate}${sign}${absH}:${absM}`;

      await apiJson<AppointmentDto>(`/backend/appointments/${rescheduleId}/reschedule`, {
        method: "PUT",
        body: JSON.stringify({ newAppointmentDate }),
      });
      toast.success("Randevu başarıyla ertelendi");
      setRescheduleId(null);
      await load();
    } catch (e) {
      const msg = e instanceof Error ? e.message : "Erteleme başarısız.";
      setError(msg);
      toast.error(msg);
    } finally {
      setIsRescheduling(false);
    }
  }

  return (
    <div className="grid gap-6">
      <PageHeader title="Randevularım" subtitle="Mevcut randevular." />
      {error ? <Card><p className="text-sm text-red-600">{error}</p></Card> : null}
      {isLoading ? <Card><p className="text-sm text-zinc-600">Yükleniyor…</p></Card> : null}

      {/* Reschedule Modal */}
      {rescheduleId && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
          <div className="w-full max-w-md rounded-2xl bg-white p-6 shadow-xl dark:bg-slate-800">
            <h3 className="mb-4 text-lg font-semibold text-slate-900 dark:text-slate-100">Randevu Ertele</h3>
            <div className="grid gap-3">
              <div>
                <label className="mb-1 block text-sm font-medium text-slate-700 dark:text-slate-300">Yeni Tarih</label>
                <input
                  type="date"
                  className="w-full rounded-lg border-2 border-slate-200 bg-white px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100"
                  value={rescheduleDate}
                  onChange={(e) => setRescheduleDate(e.target.value)}
                  min={new Date().toISOString().slice(0, 10)}
                />
              </div>
              <div>
                <label className="mb-1 block text-sm font-medium text-slate-700 dark:text-slate-300">Yeni Saat</label>
                <select
                  className="w-full rounded-lg border-2 border-slate-200 bg-white px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100"
                  value={rescheduleTime}
                  onChange={(e) => setRescheduleTime(e.target.value)}
                >
                  <option value="">Saat seçin</option>
                  {Array.from({ length: 16 }, (_, i) => {
                    const h = Math.floor(i / 2) + 9;
                    const m = (i % 2) * 30;
                    const val = `${String(h).padStart(2, "0")}:${String(m).padStart(2, "0")}`;
                    return <option key={val} value={val}>{val}</option>;
                  })}
                </select>
              </div>
            </div>
            <div className="mt-4 flex justify-end gap-2">
              <Button variant="secondary" onClick={() => setRescheduleId(null)}>Vazgeç</Button>
              <Button onClick={submitReschedule} isLoading={isRescheduling} disabled={!rescheduleDate || !rescheduleTime}>
                Ertele
              </Button>
            </div>
          </div>
        </div>
      )}

      <div className="grid gap-3">
        {items.map((a) => {
          const status = (a.status || "").toLowerCase();
          const startMs = new Date(a.appointmentDateUtc).getTime();
          const diffMs = startMs - Date.now();
          const isUpcomingEnough = diffMs > 2 * 60 * 60 * 1000;
          const canModify = (status.includes("pending") || status.includes("approved")) && isUpcomingEnough;
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
                  <Button variant="secondary" onClick={() => openReschedule(a.id)} disabled={!canModify}>
                    Ertele
                  </Button>
                  <Button variant="danger" onClick={() => cancel(a.id)} disabled={!canModify}>
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
