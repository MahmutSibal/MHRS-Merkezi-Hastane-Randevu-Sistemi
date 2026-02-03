"use client";

import { useEffect, useMemo, useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { StatusBadge } from "@/components/ui/badge";
import { apiJson } from "@/lib/api-client";

type HospitalAppointmentDto = {
  id: string;
  userId: string;
  userEmail: string;
  doctorId: number;
  doctorName: string;
  departmentName: string;
  appointmentDateUtc: string;
  status: string;
};

export default function HospitalAppointmentsPage() {
  const [items, setItems] = useState<HospitalAppointmentDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [q, setQ] = useState("");
  const [statusFilter, setStatusFilter] = useState<string>("");
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");

  async function loadAppointments() {
    setIsLoading(true);
    setError(null);
    try {
      const params = new URLSearchParams();
      if (statusFilter) params.set("status", statusFilter);
      if (fromDate) params.set("fromUtc", `${fromDate}T00:00:00Z`);
      if (toDate) params.set("toUtc", `${toDate}T23:59:59Z`);
      params.set("take", "200");
      const path = `/backend/hospital/appointments${params.toString() ? `?${params}` : ""}`;
      const data = await apiJson<HospitalAppointmentDto[]>(path);
      setItems(data);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Yükleme başarısız.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadAppointments();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const filtered = useMemo(() => {
    const term = q.trim().toLowerCase();
    return items.filter((a) => {
      const matchesTerm = !term || [a.userEmail, a.doctorName, a.departmentName].some((t) => t.toLowerCase().includes(term));
      const matchesStatus = !statusFilter || (a.status || "").toLowerCase().includes(statusFilter.toLowerCase());
      return matchesTerm && matchesStatus;
    });
  }, [items, q, statusFilter]);

  return (
    <div className="grid gap-6">
      <PageHeader title="Randevular" subtitle="Hastane randevu kayıtları." />
      {error ? <Card><p className="text-sm text-red-600">{error}</p></Card> : null}
      {isLoading ? <Card><p className="text-sm text-zinc-600">Yükleniyor…</p></Card> : null}

      <Card>
        <div className="grid gap-3 sm:grid-cols-[1fr_1fr_1fr_200px] sm:items-end">
          <Input label="Ara" placeholder="Hasta, doktor veya bölüm" value={q} onChange={(e) => setQ(e.target.value)} />
          <Input label="Başlangıç" type="date" value={fromDate} onChange={(e) => setFromDate(e.target.value)} />
          <Input label="Bitiş" type="date" value={toDate} onChange={(e) => setToDate(e.target.value)} />
          <label className="block">
            <span className="mb-2 block text-sm font-medium text-slate-700 dark:text-slate-300">Durum</span>
            <select
              className="h-10 w-full rounded-lg border-2 border-slate-200 bg-white px-3 text-sm outline-none transition-all duration-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100 dark:focus:border-blue-400 dark:focus:ring-blue-900"
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
            >
              <option value="">Tümü</option>
              <option value="pending">Beklemede</option>
              <option value="approved">Onaylandı</option>
              <option value="completed">Tamamlandı</option>
              <option value="cancelled">İptal</option>
            </select>
          </label>
        </div>
        <div className="mt-3 flex justify-end">
          <button
            type="button"
            onClick={loadAppointments}
            className="inline-flex items-center rounded-lg bg-blue-600 px-4 py-2 text-sm font-semibold text-white transition hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-70"
            disabled={isLoading}
          >
            Filtreleri Uygula
          </button>
        </div>

        <div className="mt-4 overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="text-xs text-slate-600">
              <tr>
                <th className="py-2">Tarih</th>
                <th className="py-2">Durum</th>
                <th className="py-2">Hasta</th>
                <th className="py-2">Doktor</th>
                <th className="py-2">Bölüm</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((a) => (
                <tr key={a.id} className="border-t border-slate-100 hover:bg-slate-50/60 dark:border-slate-800 dark:hover:bg-slate-800/60">
                  <td className="py-2 whitespace-nowrap">{new Date(a.appointmentDateUtc).toLocaleString("tr-TR")}</td>
                  <td className="py-2"><StatusBadge status={a.status} /></td>
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
