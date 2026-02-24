"use client";

import { useEffect, useMemo, useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { useToast } from "@/components/session/ToastProvider";
import { useSession } from "@/components/session/useSession";
import { useTheme, type Theme } from "@/components/theme/ThemeProvider";
import { apiJson } from "@/lib/api-client";

type DependentRelation = "Child" | "Mother" | "Father" | "Sister" | "Brother";
type DependentDto = {
  id: number;
  fullName: string;
  tcKimlikNo: string;
  birthDate: string;
  relation: DependentRelation | string;
};

type HealthProfile = {
  bloodType: string | null;
  allergies: string | null;
  chronicDiseases: string | null;
  medications: string | null;
  emergencyContactName: string | null;
  emergencyContactPhone: string | null;
};

const BLOOD_TYPES = ["A+", "A-", "B+", "B-", "AB+", "AB-", "0+", "0-"];

export default function ProfilePage() {
  const toast = useToast();
  const { session, refresh } = useSession();
  const { theme, setTheme } = useTheme();

  const [currentPassword, setCurrentPassword] = useState("");
  const [newEmail, setNewEmail] = useState("");
  const [newPassword, setNewPassword] = useState("");

  // Dependents
  const [dependents, setDependents] = useState<DependentDto[]>([]);
  const [isDependentsLoading, setIsDependentsLoading] = useState(false);
  const [isAddingDependent, setIsAddingDependent] = useState(false);
  const [dependentFullName, setDependentFullName] = useState("");
  const [dependentTckn, setDependentTckn] = useState("");
  const [dependentBirthDate, setDependentBirthDate] = useState("");
  const [dependentRelation, setDependentRelation] = useState<DependentRelation>("Child");

  // Health profile
  const [healthLoading, setHealthLoading] = useState(false);
  const [healthSaving, setHealthSaving] = useState(false);
  const [bloodType, setBloodType] = useState("");
  const [allergies, setAllergies] = useState("");
  const [chronicDiseases, setChronicDiseases] = useState("");
  const [medications, setMedications] = useState("");
  const [emergencyContactName, setEmergencyContactName] = useState("");
  const [emergencyContactPhone, setEmergencyContactPhone] = useState("");

  const canSave = useMemo(() => {
    const hasChange = newEmail.trim().length > 0 || newPassword.length > 0;
    return currentPassword.length > 0 && hasChange;
  }, [currentPassword, newEmail, newPassword]);

  const canAddDependent = useMemo(() => {
    return (
      dependentFullName.trim().length > 0 &&
      dependentTckn.trim().length > 0 &&
      dependentBirthDate.trim().length > 0 &&
      !!dependentRelation
    );
  }, [dependentBirthDate, dependentFullName, dependentRelation, dependentTckn]);

  async function loadDependents() {
    if (session?.role !== "Patient") return;
    setIsDependentsLoading(true);
    try {
      const list = await apiJson<DependentDto[]>("/backend/patient/dependents/me");
      setDependents(list);
    } catch {
      setDependents([]);
    } finally {
      setIsDependentsLoading(false);
    }
  }

  async function loadHealthProfile() {
    if (session?.role !== "Patient") return;
    setHealthLoading(true);
    try {
      const hp = await apiJson<HealthProfile>("/backend/patient/health-profile");
      setBloodType(hp.bloodType ?? "");
      setAllergies(hp.allergies ?? "");
      setChronicDiseases(hp.chronicDiseases ?? "");
      setMedications(hp.medications ?? "");
      setEmergencyContactName(hp.emergencyContactName ?? "");
      setEmergencyContactPhone(hp.emergencyContactPhone ?? "");
    } catch {
      // No profile yet — leave defaults
    } finally {
      setHealthLoading(false);
    }
  }

  async function saveHealthProfile() {
    setHealthSaving(true);
    try {
      await apiJson<HealthProfile>("/backend/patient/health-profile", {
        method: "PUT",
        body: JSON.stringify({
          bloodType: bloodType || null,
          allergies: allergies || null,
          chronicDiseases: chronicDiseases || null,
          medications: medications || null,
          emergencyContactName: emergencyContactName || null,
          emergencyContactPhone: emergencyContactPhone || null,
        }),
      });
      toast.success("Sağlık bilgileri kaydedildi");
    } catch (e) {
      toast.error(e instanceof Error ? e.message : "Kayıt başarısız.");
    } finally {
      setHealthSaving(false);
    }
  }

  async function addDependent() {
    setIsAddingDependent(true);
    try {
      await apiJson<DependentDto>("/backend/patient/dependents/me", {
        method: "POST",
        body: JSON.stringify({
          fullName: dependentFullName.trim(),
          tcKimlikNo: dependentTckn.trim(),
          birthDate: dependentBirthDate,
          relation: dependentRelation,
        }),
      });
      toast.success("Yakın eklendi (NVI doğrulaması yapıldı)");
      setDependentFullName("");
      setDependentTckn("");
      setDependentBirthDate("");
      setDependentRelation("Child");
      await loadDependents();
    } catch (e) {
      toast.error(e instanceof Error ? e.message : "Yakın ekleme başarısız.");
    } finally {
      setIsAddingDependent(false);
    }
  }

  useEffect(() => {
    void loadDependents();
    void loadHealthProfile();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [session?.role]);

  async function save() {
    try {
      const res = await fetch("/api/session/update-credentials", {
        method: "PATCH",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          currentPassword,
          newEmail: newEmail.trim() ? newEmail.trim() : null,
          newPassword: newPassword ? newPassword : null,
        }),
      });

      const text = await res.text();
      if (!res.ok) {
        throw new Error(text || `HTTP ${res.status}`);
      }

      toast.success("Profil güncellendi");
      setCurrentPassword("");
      setNewPassword("");
      setNewEmail("");
      await refresh();
    } catch (e) {
      toast.error(e instanceof Error ? e.message : "Güncelleme başarısız.");
    }
  }

  function onThemeChange(next: Theme) {
    setTheme(next);
    toast.success("Tema güncellendi");
  }

  return (
    <div className="grid gap-6">
      <PageHeader title="Profil" subtitle="Hesap ayarları, sağlık bilgileri ve tema." />

      <Card>
        <div className="grid gap-3 sm:grid-cols-2">
          {session?.role !== "Patient" ? (
            <div>
              <div className="mb-2 block text-sm font-medium text-slate-700 dark:text-slate-300">E-posta</div>
              <div className="rounded-2xl border-2 border-slate-200 bg-white px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100">
                {session?.email ?? ""}
              </div>
            </div>
          ) : null}
          <div>
            <div className="mb-2 block text-sm font-medium text-slate-700 dark:text-slate-300">Rol</div>
            <div className="rounded-2xl border-2 border-slate-200 bg-white px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100">
              {session?.role ?? ""}
            </div>
          </div>
        </div>
      </Card>

      <Card>
        <div className="space-y-4">
          <div className="text-sm font-medium text-slate-700 dark:text-slate-300">Hesap Bilgilerini Güncelle</div>
          <div className="grid gap-3 sm:grid-cols-3">
            <Input type="password" value={currentPassword} onChange={(e) => setCurrentPassword(e.target.value)} placeholder="Mevcut şifre" />
            {session?.role !== "Patient" ? (
              <Input value={newEmail} onChange={(e) => setNewEmail(e.target.value)} placeholder="Yeni e-posta (opsiyonel)" />
            ) : null}
            <Input type="password" value={newPassword} onChange={(e) => setNewPassword(e.target.value)} placeholder="Yeni şifre (opsiyonel)" />
          </div>
          <Button onClick={save} disabled={!canSave}>
            Kaydet
          </Button>
        </div>
      </Card>

      <Card>
        <div className="space-y-3">
          <div className="text-sm font-medium text-slate-700 dark:text-slate-300">Tema</div>
          <select
            className="w-full rounded-lg border-2 border-slate-200 bg-white px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100"
            value={theme}
            onChange={(e) => onThemeChange(e.target.value as Theme)}
          >
            <option value="system">Sistem</option>
            <option value="light">Açık</option>
            <option value="dark">Koyu</option>
          </select>
        </div>
      </Card>

      {/* Health Profile — only for patients */}
      {session?.role === "Patient" ? (
        <Card>
          <div className="space-y-4">
            <div className="flex items-center gap-2">
              <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5 text-red-500" viewBox="0 0 24 24" fill="currentColor"><path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"/></svg>
              <div className="text-sm font-medium text-slate-700 dark:text-slate-300">Sağlık Geçmişi</div>
            </div>

            {healthLoading ? (
              <div className="text-sm text-slate-500">Yükleniyor…</div>
            ) : (
              <>
                <div className="grid gap-3 sm:grid-cols-2">
                  <div>
                    <label className="mb-1 block text-xs font-medium text-slate-600 dark:text-slate-400">Kan Grubu</label>
                    <select
                      className="w-full rounded-lg border-2 border-slate-200 bg-white px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100"
                      value={bloodType}
                      onChange={(e) => setBloodType(e.target.value)}
                    >
                      <option value="">Seçiniz</option>
                      {BLOOD_TYPES.map((bt) => (
                        <option key={bt} value={bt}>{bt}</option>
                      ))}
                    </select>
                  </div>
                  <div>
                    <label className="mb-1 block text-xs font-medium text-slate-600 dark:text-slate-400">Alerjiler</label>
                    <Input value={allergies} onChange={(e) => setAllergies(e.target.value)} placeholder="Örn: Penisilin, fıstık" />
                  </div>
                  <div>
                    <label className="mb-1 block text-xs font-medium text-slate-600 dark:text-slate-400">Kronik Hastalıklar</label>
                    <Input value={chronicDiseases} onChange={(e) => setChronicDiseases(e.target.value)} placeholder="Örn: Diyabet, hipertansiyon" />
                  </div>
                  <div>
                    <label className="mb-1 block text-xs font-medium text-slate-600 dark:text-slate-400">Kullandığı İlaçlar</label>
                    <Input value={medications} onChange={(e) => setMedications(e.target.value)} placeholder="Örn: Metformin 500mg" />
                  </div>
                  <div>
                    <label className="mb-1 block text-xs font-medium text-slate-600 dark:text-slate-400">Acil Durum Kişisi</label>
                    <Input value={emergencyContactName} onChange={(e) => setEmergencyContactName(e.target.value)} placeholder="Ad Soyad" />
                  </div>
                  <div>
                    <label className="mb-1 block text-xs font-medium text-slate-600 dark:text-slate-400">Acil Durum Telefonu</label>
                    <Input value={emergencyContactPhone} onChange={(e) => setEmergencyContactPhone(e.target.value)} inputMode="tel" placeholder="05XX XXX XXXX" />
                  </div>
                </div>
                <Button onClick={saveHealthProfile} isLoading={healthSaving}>
                  Sağlık Bilgilerini Kaydet
                </Button>
              </>
            )}
          </div>
        </Card>
      ) : null}

      {/* Dependents — only for patients */}
      {session?.role === "Patient" ? (
        <Card>
          <div className="space-y-4">
            <div className="text-sm font-medium text-slate-700 dark:text-slate-300">Yakınlarım</div>
            <p className="text-xs text-slate-500 dark:text-slate-400">
              Yakın eklerken TC Kimlik No, NVI (Nüfus ve Vatandaşlık İşleri) üzerinden doğrulanır — tıpkı hasta kaydı gibi.
            </p>

            <div className="rounded-2xl border-2 border-slate-200 bg-white p-3 text-sm dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100">
              {isDependentsLoading ? (
                <div>Yükleniyor…</div>
              ) : dependents.length === 0 ? (
                <div>Henüz ekli bir yakın yok.</div>
              ) : (
                <div className="grid gap-2">
                  {dependents.map((d) => (
                    <div key={d.id} className="flex flex-col gap-1 rounded-xl border border-slate-200 bg-white px-3 py-2 dark:border-slate-700 dark:bg-slate-900">
                      <div className="font-medium text-slate-800 dark:text-slate-100">{d.fullName}</div>
                      <div className="text-xs text-slate-600 dark:text-slate-300">
                        Yakınlık: {d.relation} • Doğum Tarihi: {d.birthDate === "0001-01-01" ? "-" : d.birthDate}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>

            <div className="grid gap-3 sm:grid-cols-2">
              <Input value={dependentFullName} onChange={(e) => setDependentFullName(e.target.value)} placeholder="Yakın ad soyad" />
              <Input value={dependentTckn} onChange={(e) => setDependentTckn(e.target.value)} inputMode="numeric" placeholder="Yakın TC Kimlik No (11 hane)" />
              <Input type="date" value={dependentBirthDate} onChange={(e) => setDependentBirthDate(e.target.value)} />
              <select
                className="w-full rounded-lg border-2 border-slate-200 bg-white px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100"
                value={dependentRelation}
                onChange={(e) => setDependentRelation(e.target.value as DependentRelation)}
              >
                <option value="Child">Çocuk</option>
                <option value="Mother">Anne</option>
                <option value="Father">Baba</option>
                <option value="Sister">Kız Kardeş</option>
                <option value="Brother">Erkek Kardeş</option>
              </select>
            </div>

            <Button onClick={addDependent} isLoading={isAddingDependent} disabled={!canAddDependent}>
              Yakın Ekle
            </Button>
            <div className="text-xs text-slate-500 dark:text-slate-400">
              Not: Yakın soyadı, hasta soyadı ile aynı olmalıdır. TC Kimlik No, NVI üzerinden doğrulanır.
            </div>
          </div>
        </Card>
      ) : null}
    </div>
  );
}
