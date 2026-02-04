"use client";

import { useEffect, useMemo, useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useToast } from "@/components/session/ToastProvider";
import { apiJson } from "@/lib/api-client";

type DoctorDto = { id: number; name: string; title: string; departmentId: number; departmentName: string; isActive: boolean; userId?: string | null };
type DepartmentDto = { id: number; name: string };

export default function HospitalDoctorsPage() {
  const toast = useToast();
  const [doctors, setDoctors] = useState<DoctorDto[]>([]);
  const [departments, setDepartments] = useState<DepartmentDto[]>([]);
  const [name, setName] = useState("");
  const [title, setTitle] = useState("");
  const [departmentId, setDepartmentId] = useState<number>(0);
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [emailError, setEmailError] = useState<string | null>(null);
  const [passwordError, setPasswordError] = useState<string | null>(null);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [editEmail, setEditEmail] = useState("");
  const [editPassword, setEditPassword] = useState("");
  const [editEmailError, setEditEmailError] = useState<string | null>(null);
  const [editPasswordError, setEditPasswordError] = useState<string | null>(null);

  async function load() {
    try {
      const [deps, docs] = await Promise.all([
        apiJson<DepartmentDto[]>("/backend/hospitaladmin/departments"),
        apiJson<DoctorDto[]>("/backend/hospitaladmin/doctors"),
      ]);
      setDepartments(deps);
      setDoctors(docs);
    } catch (e) {
      toast.error(e instanceof Error ? e.message : "Veriler yüklenemedi.");
    }
  }

  useEffect(() => { load(); }, []);

  async function create() {
    try {
      const payload: any = { name, title, departmentId };
      if (email && password) {
        payload.email = email;
        payload.password = password;
      }
      const dto = await apiJson<DoctorDto>("/backend/hospitaladmin/doctors", {
        method: "POST",
        body: JSON.stringify(payload),
      });
      toast.success("Doktor oluşturuldu");
      setName(""); setTitle(""); setDepartmentId(0); setEmail(""); setPassword(""); setEmailError(null); setPasswordError(null);
      setDoctors((prev) => [dto, ...prev]);
    } catch (e) {
      toast.error(e instanceof Error ? e.message : "Oluşturma başarısız.");
    }
  }
  // Real-time validation
  useEffect(() => {
    const e = email.trim();
    const p = password;
    if (!e && !p) { setEmailError(null); setPasswordError(null); return; }
    // RFC-like email check and no Turkish-specific letters
    const hasTurkish = e.includes("ı") || e.includes("İ");
    const basicEmailOk = /.+@.+\..+/.test(e) && !hasTurkish;
    setEmailError(basicEmailOk ? null : "Geçerli bir e-posta adresi girin (Türkçe karakter yok).");
    if (p) {
      if (p.length < 8) setPasswordError("Şifre en az 8 karakter olmalıdır.");
      else if (/^\d+$/.test(p)) setPasswordError("Şifre sadece rakamlardan oluşamaz.");
      else setPasswordError(null);
    } else {
      setPasswordError(null);
    }
  }, [email, password]);

  useEffect(() => {
    const e = editEmail.trim();
    const p = editPassword;
    if (!e && !p) { setEditEmailError(null); setEditPasswordError(null); return; }
    const hasTurkish = e.includes("ı") || e.includes("İ");
    const basicEmailOk = /.+@.+\..+/.test(e) && !hasTurkish;
    setEditEmailError(e ? (basicEmailOk ? null : "Geçerli bir e-posta adresi girin.") : null);
    if (p) {
      if (p.length < 8) setEditPasswordError("Şifre en az 8 karakter olmalıdır.");
      else if (/^\d+$/.test(p)) setEditPasswordError("Şifre sadece rakamlardan oluşamaz.");
      else setEditPasswordError(null);
    } else {
      setEditPasswordError(null);
    }
  }, [editEmail, editPassword]);

  const canCreate = useMemo(() => {
    const base = name.trim().length > 0 && title.trim().length > 0 && departmentId > 0;
    const hasCreds = email.trim().length > 0 || password.length > 0;
    if (!hasCreds) return base; // creds optional
    return base && !emailError && !passwordError;
  }, [name, title, departmentId, email, password, emailError, passwordError]);

  return (
    <div className="grid gap-6">
      <PageHeader title="Doktorlar" subtitle="Hastanenizdeki doktorları yönetin." />

      <Card>
        <div className="grid gap-3 sm:grid-cols-2">
          <Input value={name} onChange={(e) => setName(e.target.value)} placeholder="Doktor adı" />
          <Input value={title} onChange={(e) => setTitle(e.target.value)} placeholder="Unvan (örn. Uz. Dr., Prof. Dr.)" />
          <select className="rounded-lg border-2 border-slate-200 bg-white px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100" value={departmentId} onChange={(e) => setDepartmentId(Number(e.target.value))}>
            <option value={0}>Bölüm seçin...</option>
            {departments.map((d) => (
              <option key={d.id} value={d.id}>{d.name}</option>
            ))}
          </select>
          <div>
            <Input value={email} onChange={(e) => setEmail(e.target.value)} placeholder="Opsiyonel: E-posta" aria-invalid={!!emailError} />
            {emailError && <p className="mt-1 text-xs text-red-600">{emailError}</p>}
          </div>
          <div>
            <Input type="password" value={password} onChange={(e) => setPassword(e.target.value)} placeholder="Opsiyonel: Şifre" aria-invalid={!!passwordError} />
            {passwordError && <p className="mt-1 text-xs text-red-600">{passwordError}</p>}
          </div>
          <Button onClick={create} disabled={!canCreate}>Doktor Oluştur</Button>
        </div>
      </Card>

      <Card>
        <div className="space-y-2">
          <p className="text-sm text-slate-600 dark:text-slate-400">Toplam {doctors.length} doktor</p>
          <ul className="grid gap-2">
            {doctors.map((d) => (
              <li key={d.id} className="rounded-lg border border-slate-200 p-3 dark:border-slate-700">
                <div className="font-medium">{d.name} – {d.departmentName} – {d.title}</div>
                <div className="text-xs text-slate-500">{d.isActive ? "Aktif" : "Pasif"}</div>
                <div className="mt-2 flex gap-2">
                  <Button variant="outline" size="sm" onClick={() => { setEditingId(d.id); setEditEmail(""); setEditPassword(""); }}>E-posta/Şifre Düzenle</Button>
                  <Button variant="secondary" size="sm" onClick={async () => {
                    if (!confirm("Doktoru pasif duruma almak istiyor musunuz?")) return;
                    try { await apiJson(`/backend/hospitaladmin/doctors/${d.id}`, { method: "DELETE" }); toast.success("Doktor pasif duruma alındı"); setDoctors(prev => prev.map(x => x.id === d.id ? { ...x, isActive: false } : x)); } catch (e) { toast.error(e instanceof Error ? e.message : "Silme başarısız."); }
                  }}>Sil (Pasifleştir)</Button>
                </div>
                {editingId === d.id && (
                  <div className="mt-3 grid gap-2 sm:grid-cols-3">
                    <div>
                      <Input value={editEmail} onChange={(e) => setEditEmail(e.target.value)} placeholder="Yeni e-posta" aria-invalid={!!editEmailError} />
                      {editEmailError && <p className="mt-1 text-xs text-red-600">{editEmailError}</p>}
                    </div>
                    <div>
                      <Input type="password" value={editPassword} onChange={(e) => setEditPassword(e.target.value)} placeholder="Yeni şifre" aria-invalid={!!editPasswordError} />
                      {editPasswordError && <p className="mt-1 text-xs text-red-600">{editPasswordError}</p>}
                    </div>
                    <div className="flex gap-2">
                      <Button size="sm" onClick={async () => {
                        try {
                          await apiJson(`/backend/hospitaladmin/doctors/${d.id}/credentials`, { method: "PATCH", body: JSON.stringify({ email: editEmail || undefined, password: editPassword || undefined }) });
                          toast.success("Bilgiler güncellendi");
                          setEditingId(null); setEditEmail(""); setEditPassword(""); setEditEmailError(null); setEditPasswordError(null);
                        } catch (e) {
                          toast.error(e instanceof Error ? e.message : "Güncelleme başarısız.");
                        }
                      }} disabled={!!editEmailError || !!editPasswordError}>Kaydet</Button>
                      <Button variant="secondary" size="sm" onClick={() => { setEditingId(null); }}>Vazgeç</Button>
                    </div>
                  </div>
                )}
              </li>
            ))}
          </ul>
        </div>
      </Card>
    </div>
  );
}
