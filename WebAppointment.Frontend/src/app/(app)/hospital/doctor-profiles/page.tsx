"use client";

import { useEffect, useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { useToast } from "@/components/session/ToastProvider";
import { apiJson } from "@/lib/api-client";

type PendingDoctorProfileDto = {
  doctorId: number;
  doctorName: string;
  doctorTitle: string;
  departmentId: number;
  departmentName: string;
  graduationUniversity: string;
  experienceSummary: string;
  submittedAtUtc: string;
};

export default function HospitalDoctorProfilesPage() {
  const toast = useToast();
  const [items, setItems] = useState<PendingDoctorProfileDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  async function load() {
    setIsLoading(true);
    setError(null);
    try {
      const data = await apiJson<PendingDoctorProfileDto[]>("/backend/hospitaladmin/doctor-profiles/pending");
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

  async function approve(doctorId: number) {
    if (!confirm("Doktorun uzmanlık bilgileri onaylansın mı?")) return;
    try {
      await apiJson<void>(`/backend/hospitaladmin/doctor-profiles/${doctorId}/approve`, { method: "POST" });
      toast.success("Onaylandı");
      await load();
    } catch (e) {
      toast.error(e instanceof Error ? e.message : "Onay başarısız.");
    }
  }

  return (
    <div className="grid gap-6">
      <PageHeader title="Doktor Onayları" subtitle="Doktorların eklediği uzmanlık bilgilerini onaylayın." />

      {error ? (
        <Card>
          <p className="text-sm text-red-600">{error}</p>
        </Card>
      ) : null}

      {isLoading ? (
        <Card>
          <p className="text-sm text-zinc-600">Yükleniyor…</p>
        </Card>
      ) : null}

      {!isLoading && items.length === 0 ? (
        <Card>
          <p className="text-sm text-slate-600 dark:text-slate-400">Onay bekleyen profil yok.</p>
        </Card>
      ) : null}

      <div className="grid gap-3">
        {items.map((x) => (
          <Card key={x.doctorId}>
            <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
              <div className="space-y-2">
                <div className="text-sm font-medium text-slate-900 dark:text-slate-100">
                  {x.doctorName} – {x.doctorTitle}
                </div>
                <div className="text-xs text-slate-500 dark:text-slate-400">{x.departmentName}</div>

                <div className="text-sm">
                  <div className="font-medium">Mezuniyet</div>
                  <div className="text-slate-700 dark:text-slate-300">{x.graduationUniversity}</div>
                </div>

                <div className="text-sm">
                  <div className="font-medium">Deneyim</div>
                  <div className="whitespace-pre-wrap text-slate-700 dark:text-slate-300">{x.experienceSummary}</div>
                </div>

                <div className="text-xs text-slate-500 dark:text-slate-400">Gönderim: {new Date(x.submittedAtUtc).toLocaleString("tr-TR")}</div>
              </div>

              <div className="shrink-0">
                <Button onClick={() => approve(x.doctorId)}>Onayla</Button>
              </div>
            </div>
          </Card>
        ))}
      </div>
    </div>
  );
}
