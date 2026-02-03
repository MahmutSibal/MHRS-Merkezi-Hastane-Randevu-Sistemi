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

export default function AdminReportsPage() {
  const [days, setDays] = useState("30");
  const [take, setTake] = useState("10");
  const [items, setItems] = useState<TopDoctorDto[]>([]);
  const [summary, setSummary] = useState<AppointmentSummaryDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  async function load() {
    setIsLoading(true);
    setError(null);
    try {
      const [doctors, summaryResponse] = await Promise.all([
        apiJson<TopDoctorDto[]>(
          `/backend/admin/reports/top-doctors?days=${encodeURIComponent(days)}&take=${encodeURIComponent(take)}`
        ),
        apiJson<AppointmentSummaryDto>(`/backend/admin/reports/appointment-summary?days=${encodeURIComponent(days)}`),
      ]);
      setItems(doctors);
      setSummary(summaryResponse);
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
    const total = items.reduce((sum, x) => sum + x.appointmentCount, 0);
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

  const donutData = useMemo(() => ({
    labels: items.map((x) => x.doctorName),
    datasets: [
      {
        label: "Pay",
        data: items.map((x) => x.appointmentCount),
        backgroundColor: [
          "#2563eb",
          "#60a5fa",
          "#93c5fd",
          "#38bdf8",
          "#22d3ee",
          "#06b6d4",
          "#0ea5e9",
          "#0284c7",
          "#075985",
          "#1e40af",
        ].slice(0, items.length),
        borderWidth: 0,
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
      <PageHeader title="Raporlar" subtitle="En çok randevu alan doktorlar." />

      <Card>
        <div className="grid gap-3 sm:grid-cols-[1fr_1fr_auto] sm:items-end">
          <Input label="Gün" value={days} onChange={(e) => setDays(e.target.value)} />
          <Input label="Adet" value={take} onChange={(e) => setTake(e.target.value)} />
          <Button onClick={load}>Getir</Button>
        </div>
      </Card>

      {/* KPI Cards */}
      <div className="grid gap-4 sm:grid-cols-3">
        <Card title="Toplam Randevu" description={`${kpis.total}`} />
        <Card title="En Yoğun Doktor" description={kpis.topName} />
        <Card title="Doktor Sayısı" description={`${kpis.unique}`} />
      </div>

      {error ? <Card><p className="text-sm text-red-600">{error}</p></Card> : null}
      {isLoading ? <Card><p className="text-sm text-zinc-600">Yükleniyor…</p></Card> : null}

      {/* Charts */}
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
        <Card title="Randevu Payları (Donut)">
          <div className="h-64">
            <Doughnut
              data={donutData}
              options={{
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { position: "bottom" } },
              }}
            />
          </div>
        </Card>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
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
      </div>

      <Card title="Detaylı Liste">
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
