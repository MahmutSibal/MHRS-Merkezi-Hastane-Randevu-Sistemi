"use client";

import { useEffect, useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useToast } from "@/components/session/ToastProvider";
import { apiJson } from "@/lib/api-client";

type DepartmentDto = { id: number; name: string };

export default function HospitalDepartmentsPage() {
  const toast = useToast();
  const [departments, setDepartments] = useState<DepartmentDto[]>([]);
  const [name, setName] = useState("");

  async function load() {
    try {
      const list = await apiJson<DepartmentDto[]>("/backend/hospitaladmin/departments");
      setDepartments(list);
    } catch (e) {
      toast.error(e instanceof Error ? e.message : "Bölümler yüklenemedi.");
    }
  }

  useEffect(() => { load(); }, []);

  async function create() {
    try {
      const dto = await apiJson<DepartmentDto>("/backend/hospitaladmin/departments", {
        method: "POST",
        body: JSON.stringify({ name }),
      });
      toast.success("Bölüm oluşturuldu");
      setName("");
      setDepartments((prev) => [dto, ...prev]);
    } catch (e) {
      toast.error(e instanceof Error ? e.message : "Oluşturma başarısız.");
    }
  }

  return (
    <div className="grid gap-6">
      <PageHeader title="Bölümler" subtitle="Hastanenizdeki bölümleri yönetin." />

      <Card>
        <div className="grid gap-3 sm:grid-cols-[1fr_auto]">
          <Input value={name} onChange={(e) => setName(e.target.value)} placeholder="Bölüm adı" />
          <Button onClick={create} disabled={!name.trim()}>Ekle</Button>
        </div>
      </Card>

      <Card>
        <div className="space-y-2">
          <p className="text-sm text-slate-600 dark:text-slate-400">Toplam {departments.length} bölüm</p>
          <ul className="grid gap-2">
            {departments.map((d) => (
              <li key={d.id} className="rounded-lg border border-slate-200 p-3 dark:border-slate-700">
                <div className="font-medium">{d.name}</div>
              </li>
            ))}
          </ul>
        </div>
      </Card>
    </div>
  );
}
