"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
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

export default function PatientAppointmentDetailPage() {
  const params = useParams<{ id: string }>();
  const id = params?.id;

  const [item, setItem] = useState<AppointmentDto | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let stale = false;
    (async () => {
      if (!id) return;
      setIsLoading(true);
      setError(null);
      try {
        const dto = await apiJson<AppointmentDto>(`/backend/appointments/${encodeURIComponent(id)}`);
        if (!stale) setItem(dto);
      } catch (e) {
        if (!stale) {
          setItem(null);
          setError(e instanceof Error ? e.message : "Randevu detayı yüklenemedi.");
        }
      } finally {
        if (!stale) setIsLoading(false);
      }
    })();
    return () => {
      stale = true;
    };
  }, [id]);

  return (
    <div className="grid gap-6">
      <PageHeader title="Randevu Detayı" subtitle="Bildirimden gelen randevu bilgisi." />

      <div className="flex items-center gap-3">
        <Link href="/patient/appointments">
          <Button variant="secondary">Randevularım</Button>
        </Link>
      </div>

      {isLoading ? (
        <Card>
          <p className="text-sm text-slate-600 dark:text-slate-400">Yükleniyor…</p>
        </Card>
      ) : error ? (
        <Card>
          <p className="text-sm font-semibold text-red-700 dark:text-red-400">Hata: {error}</p>
        </Card>
      ) : item ? (
        <Card>
          <div className="grid gap-3">
            <div className="text-sm font-medium text-slate-900 dark:text-slate-100">{item.departmentName}</div>
            <div className="text-sm text-slate-700 dark:text-slate-300">Doktor: {item.doctorName}</div>
            <div className="text-sm text-slate-700 dark:text-slate-300">Hasta: {item.dependentFullName ? item.dependentFullName : "Kendim"}</div>
            <div className="text-sm text-slate-700 dark:text-slate-300">Tarih/Saat: {new Date(item.appointmentDateUtc).toLocaleString("tr-TR")}</div>
            <div className="text-sm text-slate-700 dark:text-slate-300">Durum: {item.status}</div>
          </div>
        </Card>
      ) : (
        <Card>
          <p className="text-sm text-slate-600 dark:text-slate-400">Randevu bulunamadı.</p>
        </Card>
      )}
    </div>
  );
}
