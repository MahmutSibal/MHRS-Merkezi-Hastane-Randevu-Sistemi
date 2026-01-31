"use client";

import { useEffect, useMemo, useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useToast } from "@/components/session/ToastProvider";
import { apiJson } from "@/lib/api-client";

type DepartmentDto = { id: number; name: string };

export default function AdminDepartmentsPage() {
  const toast = useToast();
  const [items, setItems] = useState<DepartmentDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [newName, setNewName] = useState("");
  const [edit, setEdit] = useState<Record<number, string>>({});

  async function load() {
    setIsLoading(true);
    setError(null);
    try {
      const data = await apiJson<DepartmentDto[]>("/backend/admin/departments");
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

  const sorted = useMemo(() => [...items].sort((a, b) => a.name.localeCompare(b.name, "tr")), [items]);

  async function create() {
    setError(null);
    try {
      await apiJson<DepartmentDto>("/backend/admin/departments", {
        method: "POST",
        body: JSON.stringify({ name: newName }),
      });
      setNewName("");
      toast.success("Bölüm başarıyla eklendi");
      await load();
    } catch (e) {
      const errorMsg = e instanceof Error ? e.message : "Oluşturma başarısız.";
      setError(errorMsg);
      toast.error(errorMsg);
    }
  }

  async function update(id: number) {
    setError(null);
    const name = edit[id] ?? "";
    try {
      await apiJson<DepartmentDto>(`/backend/admin/departments/${id}`, {
        method: "PUT",
        body: JSON.stringify({ name }),
      });
      setEdit((p) => {
        const next = { ...p };
        delete next[id];
        return next;
      });
      toast.success("Bölüm başarıyla güncellendi");
      await load();
    } catch (e) {
      const errorMsg = e instanceof Error ? e.message : "Güncelleme başarısız.";
      setError(errorMsg);
      toast.error(errorMsg);
    }
  }

  async function remove(id: number) {
    if (!confirm("Silinsin mi?") ) return;
    setError(null);
    try {
      await apiJson<void>(`/backend/admin/departments/${id}`, { method: "DELETE" });
      toast.success("Bölüm başarıyla silindi");
      await load();
    } catch (e) {
      const errorMsg = e instanceof Error ? e.message : "Silme başarısız.";
      setError(errorMsg);
      toast.error(errorMsg);
    }
  }

  return (
    <div className="grid gap-6">
      <PageHeader title="Bölümler" subtitle="Bölüm listele/ekle/güncelle." />

      <Card>
        <div className="grid gap-3 sm:grid-cols-[1fr_auto] sm:items-end">
          <Input label="Yeni bölüm adı" value={newName} onChange={(e) => setNewName(e.target.value)} />
          <Button onClick={create} disabled={!newName.trim()}>
            Ekle
          </Button>
        </div>
      </Card>

      {error ? <Card><p className="text-sm text-red-600">{error}</p></Card> : null}
      {isLoading ? <Card><p className="text-sm text-zinc-600">Yükleniyor…</p></Card> : null}

      <div className="grid gap-3">
        {sorted.map((d) => (
          <Card key={d.id}>
            <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
              <div className="w-full">
                <div className="text-sm text-zinc-600">#{d.id}</div>
                <Input
                  label="Ad"
                  value={edit[d.id] ?? d.name}
                  onChange={(e) => setEdit((p) => ({ ...p, [d.id]: e.target.value }))}
                />
              </div>
              <div className="flex gap-2">
                <Button variant="secondary" onClick={() => update(d.id)}>
                  Kaydet
                </Button>
                <Button variant="danger" onClick={() => remove(d.id)}>
                  Sil
                </Button>
              </div>
            </div>
          </Card>
        ))}
      </div>
    </div>
  );
}
