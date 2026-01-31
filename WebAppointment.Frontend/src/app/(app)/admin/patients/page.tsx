"use client";

import { useEffect, useMemo, useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useToast } from "@/components/session/ToastProvider";
import { apiJson } from "@/lib/api-client";

type PatientDto = {
  id: number;
  userId: string;
  email: string;
  tcKimlikNo: string;
  firstName: string;
  lastName: string;
  phone: string;
};

export default function AdminPatientsPage() {
  const toast = useToast();
  const [items, setItems] = useState<PatientDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [createForm, setCreateForm] = useState({
    email: "",
    password: "",
    tcKimlikNo: "",
    firstName: "",
    lastName: "",
    phone: "",
  });
  const [edit, setEdit] = useState<Record<number, Partial<PatientDto>>>({});

  async function load() {
    setIsLoading(true);
    setError(null);
    try {
      const data = await apiJson<PatientDto[]>("/backend/admin/patients");
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

  const sorted = useMemo(() => [...items].sort((a, b) => a.email.localeCompare(b.email, "tr")), [items]);

  async function create() {
    setError(null);
    try {
      await apiJson<PatientDto>("/backend/admin/patients", {
        method: "POST",
        body: JSON.stringify(createForm),
      });
      setCreateForm({ email: "", password: "", tcKimlikNo: "", firstName: "", lastName: "", phone: "" });
      toast.success("Hasta başarıyla eklendi");
      await load();
    } catch (e) {
      const errorMsg = e instanceof Error ? e.message : "Oluşturma başarısız.";
      setError(errorMsg);
      toast.error(errorMsg);
    }
  }

  function getEditValue(id: number, current: PatientDto): PatientDto {
    const patch = edit[id] ?? {};
    return { ...current, ...patch };
  }

  async function update(id: number, current: PatientDto) {
    setError(null);
    const v = getEditValue(id, current);
    try {
      await apiJson<PatientDto>(`/backend/admin/patients/${id}`, {
        method: "PUT",
        body: JSON.stringify({ email: v.email, firstName: v.firstName, lastName: v.lastName, phone: v.phone }),
      });
      setEdit((p) => {
        const next = { ...p };
        delete next[id];
        return next;
      });
      toast.success("Hasta başarıyla güncellendi");
      await load();
    } catch (e) {
      const errorMsg = e instanceof Error ? e.message : "Güncelleme başarısız.";
      setError(errorMsg);
      toast.error(errorMsg);
    }
  }

  async function remove(id: number) {
    if (!confirm("Silinsin mi? (Hasta soft-delete)") ) return;
    setError(null);
    try {
      await apiJson<void>(`/backend/admin/patients/${id}`, { method: "DELETE" });
      toast.success("Hasta başarıyla silindi");
      await load();
    } catch (e) {
      const errorMsg = e instanceof Error ? e.message : "Silme başarısız.";
      setError(errorMsg);
      toast.error(errorMsg);
    }
  }

  return (
    <div className="grid gap-6">
      <PageHeader title="Hastalar" subtitle="Hasta kayıtları ve güncellemeler." />

      <Card>
        <div className="grid gap-4 sm:grid-cols-2">
          <Input label="Ad" value={createForm.firstName} onChange={(e) => setCreateForm((p) => ({ ...p, firstName: e.target.value }))} />
          <Input label="Soyad" value={createForm.lastName} onChange={(e) => setCreateForm((p) => ({ ...p, lastName: e.target.value }))} />
          <Input label="TC Kimlik No" value={createForm.tcKimlikNo} onChange={(e) => setCreateForm((p) => ({ ...p, tcKimlikNo: e.target.value }))} />
          <Input label="Telefon" value={createForm.phone} onChange={(e) => setCreateForm((p) => ({ ...p, phone: e.target.value }))} />
          <Input label="E-posta" type="email" value={createForm.email} onChange={(e) => setCreateForm((p) => ({ ...p, email: e.target.value }))} />
          <Input label="Şifre" type="password" value={createForm.password} onChange={(e) => setCreateForm((p) => ({ ...p, password: e.target.value }))} />
          <div className="sm:col-span-2">
            <Button onClick={create} disabled={!createForm.email.trim() || !createForm.password.trim()}>
              Yeni Hasta Oluştur
            </Button>
          </div>
        </div>
      </Card>

      {error ? <Card><p className="text-sm text-red-600">{error}</p></Card> : null}
      {isLoading ? <Card><p className="text-sm text-zinc-600">Yükleniyor…</p></Card> : null}

      <div className="grid gap-3">
        {sorted.map((p) => {
          const v = getEditValue(p.id, p);
          return (
            <Card key={p.id}>
              <div className="grid gap-3 sm:grid-cols-2">
                <Input label={`#${p.id} E-posta`} value={v.email} onChange={(e) => setEdit((s) => ({ ...s, [p.id]: { ...s[p.id], email: e.target.value } }))} />
                <Input label="Telefon" value={v.phone} onChange={(e) => setEdit((s) => ({ ...s, [p.id]: { ...s[p.id], phone: e.target.value } }))} />
                <Input label="Ad" value={v.firstName} onChange={(e) => setEdit((s) => ({ ...s, [p.id]: { ...s[p.id], firstName: e.target.value } }))} />
                <Input label="Soyad" value={v.lastName} onChange={(e) => setEdit((s) => ({ ...s, [p.id]: { ...s[p.id], lastName: e.target.value } }))} />
              </div>
              <div className="mt-3 flex gap-2">
                <Button variant="secondary" onClick={() => update(p.id, p)}>
                  Kaydet
                </Button>
                <Button variant="danger" onClick={() => remove(p.id)}>
                  Sil
                </Button>
              </div>
              <div className="mt-2 text-xs text-zinc-600">TC: {p.tcKimlikNo} | UserId: {p.userId}</div>
            </Card>
          );
        })}
      </div>
    </div>
  );
}
