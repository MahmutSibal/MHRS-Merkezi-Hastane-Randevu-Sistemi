"use client";

import { useEffect, useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { StatusBadge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
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
  const [query, setQuery] = useState("");
  const [statusFilter, setStatusFilter] = useState("all");
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");

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

  const filteredItems = items.filter((a) => {
    const status = (a.status || "").toLowerCase();
    const matchesStatus = statusFilter === "all" || status === statusFilter;
    const matchesQuery =
      query.trim().length === 0 ||
      a.patientEmail.toLowerCase().includes(query.trim().toLowerCase());
    const startDate = new Date(a.startAtUtc);
    const matchesFrom = !fromDate || startDate >= new Date(`${fromDate}T00:00:00`);
    const matchesTo = !toDate || startDate <= new Date(`${toDate}T23:59:59`);
    return matchesStatus && matchesQuery && matchesFrom && matchesTo;
  });

  const weeklySummary = Array.from({ length: 7 }, (_, offset) => {
    const day = new Date();
    day.setDate(day.getDate() + offset);
    const key = day.toISOString().slice(0, 10);
    const count = items.filter((a) => a.startAtUtc.startsWith(key)).length;
    return { date: key, count };
  });

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

      <Card>
        <div className="grid gap-3 lg:grid-cols-[2fr_1fr_1fr_1fr]">
          <Input label="Hasta E-posta" placeholder="Ara..." value={query} onChange={(e) => setQuery(e.target.value)} />
          <label className="block">
            <span className="mb-2 block text-sm font-medium text-slate-700 dark:text-slate-300">Durum</span>
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              className="h-10 w-full rounded-2xl border-2 border-slate-200 bg-white px-3 text-sm text-slate-700 dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100"
            >
              <option value="all">Tümü</option>
              <option value="pending">Beklemede</option>
              <option value="approved">Onaylandı</option>
              <option value="completed">Tamamlandı</option>
              <option value="cancelled">İptal</option>
            </select>
          </label>
          <Input label="Başlangıç" type="date" value={fromDate} onChange={(e) => setFromDate(e.target.value)} />
          <Input label="Bitiş" type="date" value={toDate} onChange={(e) => setToDate(e.target.value)} />
        </div>
      </Card>

      <Card title="Önümüzdeki 7 Gün">
        <div className="grid gap-2 sm:grid-cols-2 lg:grid-cols-4">
          {weeklySummary.map((day) => (
            <div key={day.date} className="rounded-xl border border-slate-200 p-3 text-sm dark:border-slate-700">
              <div className="text-xs text-slate-500">{new Date(day.date).toLocaleDateString("tr-TR", { weekday: "long" })}</div>
              <div className="mt-1 text-sm font-semibold text-slate-900 dark:text-slate-100">{day.count} randevu</div>
              <div className="text-xs text-slate-500">{day.date}</div>
            </div>
          ))}
        </div>
      </Card>
      
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
        {filteredItems.map((a) => {
          const status = (a.status || "").toLowerCase();
          const canApprove = status.includes("pending");
          const canComplete = status.includes("approved");
          return (
            <Card key={a.id}>
              <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                <div>
                  <div className="flex items-center gap-2">
                    <div className="text-sm font-medium text-slate-900 dark:text-slate-100">{a.patientEmail}</div>
                    <StatusBadge status={a.status} />
                  </div>
                  <div className="mt-1 text-sm text-slate-600 dark:text-slate-400">
                    {new Date(a.startAtUtc).toLocaleString("tr-TR")} – {new Date(a.endAtUtc).toLocaleTimeString("tr-TR")}
                  </div>
                </div>
                <div className="flex gap-2">
                  {canApprove ? (
                    <Button variant="secondary" onClick={() => approve(a.id)}>
                      Onayla
                    </Button>
                  ) : null}
                  {canComplete ? (
                    <Button onClick={() => complete(a.id)}>
                      Tamamla
                    </Button>
                  ) : null}
                </div>
              </div>
            </Card>
          );
        })}
      </div>
    </div>
  );
}
