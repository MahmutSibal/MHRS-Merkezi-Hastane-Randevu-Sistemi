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
  birthDate: string; // YYYY-MM-DD
  relation: DependentRelation | string;
};

export default function ProfilePage() {
  const toast = useToast();
  const { session, refresh } = useSession();
  const { theme, setTheme } = useTheme();

  const [currentPassword, setCurrentPassword] = useState("");
  const [newEmail, setNewEmail] = useState("");
  const [newPassword, setNewPassword] = useState("");

  const [dependents, setDependents] = useState<DependentDto[]>([]);
  const [isDependentsLoading, setIsDependentsLoading] = useState(false);
  const [isAddingDependent, setIsAddingDependent] = useState(false);
  const [dependentFullName, setDependentFullName] = useState("");
  const [dependentTckn, setDependentTckn] = useState("");
  const [dependentBirthDate, setDependentBirthDate] = useState("");
  const [dependentRelation, setDependentRelation] = useState<DependentRelation>("Child");

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
      toast.success("Yakın eklendi");
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
      // Eğer email değiştiyse, yeni email'i otomatik doldurmayı bırak.
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
      <PageHeader title="Profil" subtitle="Hesap ayarları ve tema." />

      <Card>
        <div className="grid gap-3 sm:grid-cols-2">
          <div>
            <div className="mb-2 block text-sm font-medium text-slate-700 dark:text-slate-300">E-posta</div>
            <div className="rounded-2xl border-2 border-slate-200 bg-white px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100">
              {session?.email ?? ""}
            </div>
          </div>
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
            <Input value={newEmail} onChange={(e) => setNewEmail(e.target.value)} placeholder="Yeni e-posta (opsiyonel)" />
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

      {session?.role === "Patient" ? (
        <Card>
          <div className="space-y-4">
            <div className="text-sm font-medium text-slate-700 dark:text-slate-300">Yakınlarım</div>

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
              <Input value={dependentTckn} onChange={(e) => setDependentTckn(e.target.value)} inputMode="numeric" placeholder="Yakın TCKN (11 hane)" />
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
              Not: Yakın soyadı, hasta soyadı ile aynı olmalıdır.
            </div>
          </div>
        </Card>
      ) : null}
    </div>
  );
}
