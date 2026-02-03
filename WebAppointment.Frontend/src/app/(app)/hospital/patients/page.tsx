"use client";

import { useEffect, useMemo, useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
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

export default function HospitalPatientsPage() {
  const [items, setItems] = useState<PatientDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [query, setQuery] = useState("");
  const [isSaving, setIsSaving] = useState(false);
  const [editing, setEditing] = useState<PatientDto | null>(null);
  const [form, setForm] = useState({
    email: "",
    password: "",
    tcKimlikNo: "",
    firstName: "",
    lastName: "",
    phone: "",
  });

  const resetForm = () => {
    setEditing(null);
    setForm({
      email: "",
      password: "",
      tcKimlikNo: "",
      firstName: "",
      lastName: "",
      phone: "",
    });
  };

  const loadPatients = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await apiJson<PatientDto[]>("/backend/hospital/patients");
      setItems(data);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Yükleme başarısız.");
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    void loadPatients();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setIsSaving(true);
    setError(null);
    try {
      if (editing) {
        await apiJson<PatientDto>(`/backend/hospital/patients/${editing.id}`, {
          method: "PUT",
          body: JSON.stringify({
            email: form.email,
            firstName: form.firstName,
            lastName: form.lastName,
            phone: form.phone,
          }),
        });
      } else {
        await apiJson<PatientDto>("/backend/hospital/patients", {
          method: "POST",
          body: JSON.stringify({
            email: form.email,
            password: form.password,
            tcKimlikNo: form.tcKimlikNo,
            firstName: form.firstName,
            lastName: form.lastName,
            phone: form.phone,
          }),
        });
      }
      await loadPatients();
      resetForm();
    } catch (e) {
      setError(e instanceof Error ? e.message : "Kaydetme başarısız.");
    } finally {
      setIsSaving(false);
    }
  };

  const startEdit = (patient: PatientDto) => {
    setEditing(patient);
    setForm({
      email: patient.email,
      password: "",
      tcKimlikNo: patient.tcKimlikNo,
      firstName: patient.firstName,
      lastName: patient.lastName,
      phone: patient.phone,
    });
  };

  const removePatient = async (patient: PatientDto) => {
    if (!confirm(`${patient.firstName} ${patient.lastName} adlı hastayı silmek istediğinize emin misiniz?`)) {
      return;
    }
    setIsSaving(true);
    setError(null);
    try {
      await apiJson(`/backend/hospital/patients/${patient.id}`, { method: "DELETE" });
      await loadPatients();
    } catch (e) {
      setError(e instanceof Error ? e.message : "Silme başarısız.");
    } finally {
      setIsSaving(false);
    }
  };

  const filtered = useMemo(() => {
    const term = query.trim().toLowerCase();
    if (!term) {
      return items;
    }
    return items.filter((p) => {
      const haystack = [p.email, p.firstName, p.lastName, p.phone, p.tcKimlikNo].join(" ").toLowerCase();
      return haystack.includes(term);
    });
  }, [items, query]);

  return (
    <div className="grid gap-6">
      <PageHeader title="Hastalar" subtitle="Hastane kapsamındaki hasta listesi." />
      {error ? <Card><p className="text-sm text-red-600">{error}</p></Card> : null}
      {isLoading ? <Card><p className="text-sm text-zinc-600">Yükleniyor…</p></Card> : null}

      <Card>
        <Input label="Ara" placeholder="E-posta, ad, telefon veya TC" value={query} onChange={(e) => setQuery(e.target.value)} />
      </Card>

      <Card title={editing ? "Hasta Güncelle" : "Yeni Hasta Ekle"}>
        <form className="grid gap-3 sm:grid-cols-2" onSubmit={handleSubmit}>
          <Input
            label="E-posta"
            value={form.email}
            onChange={(e) => setForm((prev) => ({ ...prev, email: e.target.value }))}
          />
          {!editing ? (
            <Input
              label="Şifre"
              type="password"
              value={form.password}
              onChange={(e) => setForm((prev) => ({ ...prev, password: e.target.value }))}
            />
          ) : (
            <div />
          )}
          {!editing ? (
            <Input
              label="TC Kimlik"
              value={form.tcKimlikNo}
              onChange={(e) => setForm((prev) => ({ ...prev, tcKimlikNo: e.target.value }))}
            />
          ) : (
            <Input label="TC Kimlik" value={form.tcKimlikNo} disabled />
          )}
          <Input
            label="Ad"
            value={form.firstName}
            onChange={(e) => setForm((prev) => ({ ...prev, firstName: e.target.value }))}
          />
          <Input
            label="Soyad"
            value={form.lastName}
            onChange={(e) => setForm((prev) => ({ ...prev, lastName: e.target.value }))}
          />
          <Input
            label="Telefon"
            value={form.phone}
            onChange={(e) => setForm((prev) => ({ ...prev, phone: e.target.value }))}
          />
          <div className="flex flex-wrap items-center gap-2 sm:col-span-2">
            <Button type="submit" disabled={isSaving}>
              {editing ? "Güncelle" : "Kaydet"}
            </Button>
            {editing ? (
              <Button type="button" variant="ghost" onClick={resetForm} disabled={isSaving}>
                İptal
              </Button>
            ) : null}
          </div>
        </form>
      </Card>

      <Card title={`Toplam ${filtered.length} kayıt`}>
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="text-xs text-slate-600">
              <tr>
                <th className="py-2">Hasta</th>
                <th className="py-2">E-posta</th>
                <th className="py-2">Telefon</th>
                <th className="py-2">TC Kimlik</th>
                <th className="py-2 text-right">İşlem</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((p) => (
                <tr key={p.id} className="border-t border-slate-100 hover:bg-slate-50/60 dark:border-slate-800 dark:hover:bg-slate-800/60">
                  <td className="py-2">{p.firstName} {p.lastName}</td>
                  <td className="py-2">{p.email}</td>
                  <td className="py-2">{p.phone}</td>
                  <td className="py-2">{p.tcKimlikNo}</td>
                  <td className="py-2 text-right">
                    <div className="flex justify-end gap-2">
                      <Button size="sm" variant="ghost" onClick={() => startEdit(p)} disabled={isSaving}>
                        Düzenle
                      </Button>
                      <Button size="sm" variant="ghost" onClick={() => removePatient(p)} disabled={isSaving}>
                        Sil
                      </Button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>
    </div>
  );
}
