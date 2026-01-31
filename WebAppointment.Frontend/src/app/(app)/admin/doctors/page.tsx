"use client";

import { useEffect, useMemo, useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useToast } from "@/components/session/ToastProvider";
import { apiJson } from "@/lib/api-client";

type DepartmentDto = { id: number; name: string };
type DoctorDto = {
  id: number;
  name: string;
  departmentId: number;
  departmentName: string;
  isActive: boolean;
  userId: string | null;
};

export default function AdminDoctorsPage() {
  const toast = useToast();
  const [departments, setDepartments] = useState<DepartmentDto[]>([]);
  const [items, setItems] = useState<DoctorDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [createForm, setCreateForm] = useState({ name: "", departmentId: 0, email: "", password: "", userId: null });
  const [edit, setEdit] = useState<Record<number, Partial<DoctorDto>>>({});

  async function load() {
    setIsLoading(true);
    setError(null);
    try {
      const [deps, docs] = await Promise.all([
        apiJson<DepartmentDto[]>("/backend/admin/departments"),
        apiJson<DoctorDto[]>("/backend/admin/doctors"),
      ]);
      setDepartments(deps);
      setItems(docs);
      setCreateForm((p) => ({ ...p, departmentId: p.departmentId || (deps[0]?.id ?? 0) }));
    } catch (e) {
      setError(e instanceof Error ? e.message : "Yükleme başarısız.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, []);

  const sorted = useMemo(() => [...items].sort((a, b) => a.name.localeCompare(b.name, "tr")), [items]);

  async function create() {
    setError(null);
    try {
      await apiJson<DoctorDto>("/backend/admin/doctors", {
        method: "POST",
        body: JSON.stringify({
          name: createForm.name,
          departmentId: Number(createForm.departmentId),
          email: createForm.email.trim(),
          password: createForm.password.trim(),
          userId: createForm.userId,
        }),
      });
      setCreateForm((p) => ({ ...p, name: "", email: "", password: "", userId: null }));
      toast.success("Doktor başarıyla eklendi");
      await load();
    } catch (e) {
      const errorMsg = e instanceof Error ? e.message : "Oluşturma başarısız.";
      setError(errorMsg);
      toast.error(errorMsg);
    }
  }

  function getEditValue(id: number, current: DoctorDto): DoctorDto {
    const patch = edit[id] ?? {};
    return {
      ...current,
      ...patch,
      departmentId: patch.departmentId ?? current.departmentId,
      isActive: patch.isActive ?? current.isActive,
      userId: (patch.userId ?? current.userId) as string | null,
    };
  }

  async function update(id: number, current: DoctorDto) {
    setError(null);
    const v = getEditValue(id, current);
    try {
      await apiJson<DoctorDto>(`/backend/admin/doctors/${id}`, {
        method: "PUT",
        body: JSON.stringify({
          name: v.name,
          departmentId: Number(v.departmentId),
          isActive: Boolean(v.isActive),
          userId: v.userId ? String(v.userId) : null,
        }),
      });
      setEdit((p) => {
        const next = { ...p };
        delete next[id];
        return next;
      });
      toast.success("Doktor başarıyla güncellendi");
      await load();
    } catch (e) {
      const errorMsg = e instanceof Error ? e.message : "Güncelleme başarısız.";
      setError(errorMsg);
      toast.error(errorMsg);
    }
  }

  async function remove(id: number) {
    if (!confirm("Silinsin mi? (Doktor pasif yapılacak)") ) return;
    setError(null);
    try {
      await apiJson<void>(`/backend/admin/doctors/${id}`, { method: "DELETE" });
      toast.success("Doktor başarıyla silindi");
      await load();
    } catch (e) {
      const errorMsg = e instanceof Error ? e.message : "Silme başarısız.";
      setError(errorMsg);
      toast.error(errorMsg);
    }
  }

  return (
    <div className="grid gap-6">
      <PageHeader title="Doktorlar" subtitle="Doktor listele/ekle/güncelle." />

      <Card>
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          <Input label="Ad Soyad" value={createForm.name} onChange={(e) => setCreateForm((p) => ({ ...p, name: e.target.value }))} placeholder="Dr. Ahmet Yılmaz" />
          <div>
            <div className="mb-2 block text-sm font-medium text-slate-700 dark:text-slate-300">Bölüm</div>
            <select
              className="w-full rounded-lg border-2 border-slate-200 bg-white px-3 py-2 text-sm outline-none transition-all focus:border-blue-500 focus:ring-2 focus:ring-blue-200 dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100 dark:focus:border-blue-400 dark:focus:ring-blue-900"
              value={createForm.departmentId}
              onChange={(e) => setCreateForm((p) => ({ ...p, departmentId: Number(e.target.value) }))}
            >
              <option value={0}>Bölüm seçin...</option>
              {departments.map((d) => (
                <option key={d.id} value={d.id}>
                  {d.name}
                </option>
              ))}
            </select>
          </div>
          <Input label="E-posta" type="email" value={createForm.email} onChange={(e) => setCreateForm((p) => ({ ...p, email: e.target.value }))} placeholder="ahmet@hospital.local" />
          <Input label="Şifre" type="password" value={createForm.password} onChange={(e) => setCreateForm((p) => ({ ...p, password: e.target.value }))} placeholder="En az 6 karakter" />
          <div className="flex items-end">
            <Button onClick={create} disabled={!createForm.name.trim() || !createForm.departmentId || !createForm.email.trim() || !createForm.password.trim()} className="w-full">
              Doktor Ekle
            </Button>
          </div>
        </div>
      </Card>

      {error ? <Card><p className="text-sm text-red-600">{error}</p></Card> : null}
      {isLoading ? <Card><p className="text-sm text-zinc-600">Yükleniyor…</p></Card> : null}

      <div className="grid gap-3">
        {sorted.map((d) => {
          const v = getEditValue(d.id, d);
          return (
            <Card key={d.id}>
              <div className="grid gap-3 sm:grid-cols-[1fr_220px_220px_auto] sm:items-end">
                <Input
                  label={`Doktor #${d.id}`}
                  value={v.name}
                  onChange={(e) => setEdit((p) => ({ ...p, [d.id]: { ...p[d.id], name: e.target.value } }))}
                />

                <div>
                  <div className="mb-1 text-xs font-medium text-zinc-600">Bölüm</div>
                  <select
                    className="w-full rounded-xl border border-black/10 bg-white px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-sky-500"
                    value={v.departmentId}
                    onChange={(e) => setEdit((p) => ({ ...p, [d.id]: { ...p[d.id], departmentId: Number(e.target.value) } }))}
                  >
                    {departments.map((dep) => (
                      <option key={dep.id} value={dep.id}>
                        {dep.name}
                      </option>
                    ))}
                  </select>
                </div>

                <div className="flex items-center gap-3">
                  <label className="flex items-center gap-2 text-sm text-zinc-700">
                    <input
                      type="checkbox"
                      checked={v.isActive}
                      onChange={(e) => setEdit((p) => ({ ...p, [d.id]: { ...p[d.id], isActive: e.target.checked } }))}
                    />
                    Aktif
                  </label>
                </div>

                <div className="flex gap-2">
                  <Button variant="secondary" onClick={() => update(d.id, d)}>
                    Kaydet
                  </Button>
                  <Button variant="danger" onClick={() => remove(d.id)}>
                    Sil
                  </Button>
                </div>
              </div>
              <div className="mt-2 text-xs text-zinc-600">Bölüm: {d.departmentName}</div>
            </Card>
          );
        })}
      </div>
    </div>
  );
}
