"use client";

import { useMemo, useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { useToast } from "@/components/session/ToastProvider";
import { useSession } from "@/components/session/useSession";
import { useTheme, type Theme } from "@/components/theme/ThemeProvider";

export default function ProfilePage() {
  const toast = useToast();
  const { session, refresh } = useSession();
  const { theme, setTheme } = useTheme();

  const [currentPassword, setCurrentPassword] = useState("");
  const [newEmail, setNewEmail] = useState("");
  const [newPassword, setNewPassword] = useState("");

  const canSave = useMemo(() => {
    const hasChange = newEmail.trim().length > 0 || newPassword.length > 0;
    return currentPassword.length > 0 && hasChange;
  }, [currentPassword, newEmail, newPassword]);

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
    </div>
  );
}
