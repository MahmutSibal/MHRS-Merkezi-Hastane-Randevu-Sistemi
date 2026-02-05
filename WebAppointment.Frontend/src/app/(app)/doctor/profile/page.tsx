"use client";

import { useEffect, useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useToast } from "@/components/session/ToastProvider";
import { apiJson } from "@/lib/api-client";

type DoctorProfileDto = {
  doctorId: number;
  graduationUniversity: string | null;
  experienceSummary: string | null;
  profileStatus: string;
  submittedAtUtc: string | null;
  approvedAtUtc: string | null;
};

export default function DoctorProfilePage() {
  const toast = useToast();
  const [profile, setProfile] = useState<DoctorProfileDto | null>(null);
  const [graduationUniversity, setGraduationUniversity] = useState<string>("");
  const [experienceSummary, setExperienceSummary] = useState<string>("");
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function load() {
    setIsLoading(true);
    setError(null);
    try {
      const dto = await apiJson<DoctorProfileDto>("/backend/doctor/profile/me");
      setProfile(dto);
      setGraduationUniversity(dto.graduationUniversity ?? "");
      setExperienceSummary(dto.experienceSummary ?? "");
    } catch (e) {
      setError(e instanceof Error ? e.message : "Yükleme başarısız.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, []);

  async function save() {
    setIsSaving(true);
    setError(null);
    try {
      await apiJson<void>("/backend/doctor/profile/me", {
        method: "PUT",
        body: JSON.stringify({
          graduationUniversity,
          experienceSummary,
        }),
      });
      toast.success("Bilgiler gönderildi (onay bekliyor)");
      await load();
    } catch (e) {
      const msg = e instanceof Error ? e.message : "Kaydetme başarısız.";
      setError(msg);
      toast.error(msg);
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <div className="grid gap-6">
      <PageHeader
        title="Uzmanlık Bilgileri"
        subtitle="Mezuniyet ve deneyim bilgilerinizi girin; hastane yöneticisi onayladıktan sonra hastalar görebilir."
      />

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

      {profile ? (
        <Card>
          <div className="grid gap-3">
            <div className="text-sm text-slate-600 dark:text-slate-400">
              Durum: <span className="font-medium text-slate-900 dark:text-slate-100">{profile.profileStatus}</span>
            </div>
            <Input
              value={graduationUniversity}
              onChange={(e) => setGraduationUniversity(e.target.value)}
              placeholder="Mezun olduğunuz üniversite"
            />
            <div>
              <label className="mb-2 block text-sm font-medium text-slate-700 dark:text-slate-300">Deneyim / Tecrübe</label>
              <textarea
                value={experienceSummary}
                onChange={(e) => setExperienceSummary(e.target.value)}
                rows={6}
                className="w-full rounded-lg border-2 border-slate-200 bg-white px-3 py-2 text-sm outline-none transition-all focus:border-blue-500 focus:ring-2 focus:ring-blue-200 dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100 dark:focus:border-blue-400 dark:focus:ring-blue-900"
                placeholder="Örn: 5 yıl dahiliye, 2 yıl acil servis, uzmanlık alanları…"
              />
              <p className="mt-1 text-xs text-slate-500 dark:text-slate-400">Bu bilgiler onaylandığında hastalara görünür.</p>
            </div>

            <Button onClick={save} isLoading={isSaving} disabled={!graduationUniversity.trim() || experienceSummary.trim().length < 10}>
              Onaya Gönder
            </Button>
          </div>
        </Card>
      ) : null}
    </div>
  );
}
