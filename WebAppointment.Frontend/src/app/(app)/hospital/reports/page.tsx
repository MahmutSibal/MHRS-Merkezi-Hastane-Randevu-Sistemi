"use client";

import { useEffect, useMemo, useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { apiJson } from "@/lib/api-client";
import { Bar, Doughnut, Line } from "react-chartjs-2";
import {
  Chart as ChartJS,
  BarElement,
  CategoryScale,
  LinearScale,
  ArcElement,
  Tooltip,
  Legend,
  LineElement,
  PointElement,
} from "chart.js";

ChartJS.register(BarElement, CategoryScale, LinearScale, ArcElement, Tooltip, Legend, LineElement, PointElement);

type TopDoctorDto = { doctorId: number; doctorName: string; appointmentCount: number };
type AppointmentSummaryDto = {
  days: number;
  statusSummary: {
    pending: number;
    approved: number;
    completed: number;
    cancelled: number;
    total: number;
  };
  dailyCounts: { date: string; count: number }[];
};
type NoShowRiskAppointmentDto = {
  appointmentId: string;
  patientName: string;
  patientPhone: string;
  noShowScore: number;
  doctorName: string;
  hospitalName: string;
  startAtUtc: string;
  reminderConfirmed: boolean;
};

export default function HospitalReportsPage() {
  const [days, setDays] = useState("30");
  const [take, setTake] = useState("10");
  const [items, setItems] = useState<TopDoctorDto[]>([]);
  const [summary, setSummary] = useState<AppointmentSummaryDto | null>(null);
  const [riskItems, setRiskItems] = useState<NoShowRiskAppointmentDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  async function load() {
    setIsLoading(true);
    setError(null);
    try {
      const [doctors, summaryResponse, riskResponse] = await Promise.all([
        apiJson<TopDoctorDto[]>(
          `/backend/hospital/reports/top-doctors?days=${encodeURIComponent(days)}&take=${encodeURIComponent(take)}`
        ),
        apiJson<AppointmentSummaryDto>(`/backend/hospital/reports/appointment-summary?days=${encodeURIComponent(days)}`),
        apiJson<NoShowRiskAppointmentDto[]>(`/backend/hospital/reports/no-show-risk?days=7&minScore=40`),
      ]);
      setItems(doctors);
      setSummary(summaryResponse);
      setRiskItems(riskResponse);
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

  const kpis = useMemo(() => {
    const total = summary?.statusSummary.total ?? 0;
    const top = items[0];
    const unique = items.length;
    return { total, topName: top?.doctorName ?? "-", unique };
  }, [items, summary]);

  const barData = useMemo(() => ({
    labels: items.map((x) => x.doctorName),
    datasets: [
      {
        label: "Randevu",
        data: items.map((x) => x.appointmentCount),
        backgroundColor: "rgba(37,99,235,0.6)",
        borderRadius: 8,
      },
    ],
  }), [items]);

  const statusDonutData = useMemo(() => {
    if (!summary) {
      return { labels: [], datasets: [] };
    }
    return {
      labels: ["Beklemede", "Onaylandı", "Tamamlandı", "İptal"],
      datasets: [
        {
          label: "Durum",
          data: [
            summary.statusSummary.pending,
            summary.statusSummary.approved,
            summary.statusSummary.completed,
            summary.statusSummary.cancelled,
          ],
          backgroundColor: ["#f59e0b", "#38bdf8", "#22c55e", "#f87171"],
          borderWidth: 0,
        },
      ],
    };
  }, [summary]);

  const lineData = useMemo(() => ({
    labels: summary?.dailyCounts.map((x) => x.date) ?? [],
    datasets: [
      {
        label: "Günlük Randevu",
        data: summary?.dailyCounts.map((x) => x.count) ?? [],
        borderColor: "#2563eb",
        backgroundColor: "rgba(37,99,235,0.2)",
        tension: 0.35,
        fill: true,
      },
    ],
  }), [summary]);

  return (
    <div className="grid gap-6">
      <PageHeader title="Raporlar" subtitle="Hastane performans özeti." />

      <Card>
        <div className="grid gap-3 sm:grid-cols-[1fr_1fr_auto] sm:items-end">
          <Input label="Gün" value={days} onChange={(e) => setDays(e.target.value)} />
          <Input label="Adet" value={take} onChange={(e) => setTake(e.target.value)} />
          <Button onClick={load}>Getir</Button>
        </div>
      </Card>

      <div className="grid gap-4 sm:grid-cols-3">
        <Card title="Toplam Randevu" description={`${kpis.total}`} />
        <Card title="En Yoğun Doktor" description={kpis.topName} />
        <Card title="Doktor Sayısı" description={`${kpis.unique}`} />
      </div>

      {error ? <Card><p className="text-sm text-red-600">{error}</p></Card> : null}
      {isLoading ? <Card><p className="text-sm text-zinc-600">Yükleniyor…</p></Card> : null}

      <div className="grid gap-6 lg:grid-cols-2">
        <Card title="Randevu Dağılımı (Bar)">
          <div className="h-64">
            <Bar
              data={barData}
              options={{
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: { y: { beginAtZero: true } },
              }}
            />
          </div>
        </Card>
        <Card title="Randevu Durumları">
          <div className="h-64">
            <Doughnut
              data={statusDonutData}
              options={{
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { position: "bottom" } },
              }}
            />
          </div>
        </Card>
      </div>

      <Card title="Günlük Randevu Trendleri">
        <div className="h-64">
          <Line
            data={lineData}
            options={{
              responsive: true,
              maintainAspectRatio: false,
              plugins: { legend: { display: false } },
              scales: { y: { beginAtZero: true } },
            }}
          />
        </div>
      </Card>

      <Card title="En Yoğun Doktorlar">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="text-xs text-slate-600">
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

      <Card title="Riskli Randevular (Önümüzdeki 7 Gün)" description="Gelmeme riski yüksek, henüz onaylamamış hastalar.">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="text-xs text-slate-600">
              <tr>
                <th className="py-2">Hasta</th>
                <th className="py-2">Telefon</th>
                <th className="py-2">Doktor</th>
                <th className="py-2">Randevu</th>
                <th className="py-2">Risk Skoru</th>
                <th className="py-2">Onay</th>
              </tr>
            </thead>
            <tbody>
              {riskItems.length === 0 ? (
                <tr>
                  <td colSpan={6} className="py-3 text-center text-slate-500">Riskli randevu bulunmuyor.</td>
                </tr>
              ) : (
                riskItems.map((r) => (
                  <tr key={r.appointmentId} className="border-t border-black/5">
                    <td className="py-2">{r.patientName}</td>
                    <td className="py-2">{r.patientPhone}</td>
                    <td className="py-2">{r.doctorName}</td>
                    <td className="py-2">{new Date(r.startAtUtc).toLocaleString("tr-TR")}</td>
                    <td className="py-2">
                      <span className={r.noShowScore >= 60 ? "font-semibold text-red-600" : "font-semibold text-amber-600"}>
                        {r.noShowScore}
                      </span>
                    </td>
                    <td className="py-2">{r.reminderConfirmed ? "Onayladı" : "Bekliyor"}</td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </Card>
    </div>
  );
}
