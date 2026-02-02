"use client";

import { useEffect, useMemo, useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useToast } from "@/components/session/ToastProvider";
import { apiJson } from "@/lib/api-client";

type DoctorDto = { id: number; name: string; departmentId: number; departmentName: string; isActive: boolean; userId?: string | null };
type DepartmentDto = { id: number; name: string };

export default function HospitalDoctorsPage() {
  const toast = useToast();
  const [doctors, setDoctors] = useState<DoctorDto[]>([]);
  const [departments, setDepartments] = useState<DepartmentDto[]>([]);
  const [name, setName] = useState("");
  const [departmentId, setDepartmentId] = useState<number>(0);
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

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
      const payload: any = { name, departmentId };
      if (email && password) {
        payload.email = email;
        payload.password = password;
      }
      const dto = await apiJson<DoctorDto>("/backend/hospitaladmin/doctors", {
        method: "POST",
        body: JSON.stringify(payload),
      });
      toast.success("Doktor oluşturuldu");
      setName(""); setDepartmentId(0); setEmail(""); setPassword("");
      setDoctors((prev) => [dto, ...prev]);
    } catch (e) {
      toast.error(e instanceof Error ? e.message : "Oluşturma başarısız.");
    }
  }

  const canCreate = useMemo(() => name.trim() && departmentId > 0, [name, departmentId]);

  return (
    <div className="grid gap-6">
      <PageHeader title="Doktorlar" subtitle="Hastanenizdeki doktorları yönetin." />

      <Card>
        <div className="grid gap-3 sm:grid-cols-2">
          <Input value={name} onChange={(e) => setName(e.target.value)} placeholder="Doktor adı" />
          <select className="rounded-lg border-2 border-slate-200 bg-white px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100" value={departmentId} onChange={(e) => setDepartmentId(Number(e.target.value))}>
            <option value={0}>Bölüm seçin...</option>
            {departments.map((d) => (
              <option key={d.id} value={d.id}>{d.name}</option>
            ))}
          </select>
          <Input value={email} onChange={(e) => setEmail(e.target.value)} placeholder="Opsiyonel: E-posta" />
          <Input type="password" value={password} onChange={(e) => setPassword(e.target.value)} placeholder="Opsiyonel: Şifre" />
          <Button onClick={create} disabled={!canCreate}>Doktor Oluştur</Button>
        </div>
      </Card>

      <Card>
        <div className="space-y-2">
          <p className="text-sm text-slate-600 dark:text-slate-400">Toplam {doctors.length} doktor</p>
          <ul className="grid gap-2">
            {doctors.map((d) => (
              <li key={d.id} className="rounded-lg border border-slate-200 p-3 dark:border-slate-700">
                <div className="font-medium">{d.name}</div>
                <div className="text-xs text-slate-500">{d.departmentName} · {d.isActive ? "Aktif" : "Pasif"}</div>
              </li>
            ))}
          </ul>
        </div>
      </Card>
    </div>
  );
}
