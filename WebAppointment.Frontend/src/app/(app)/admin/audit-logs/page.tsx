"use client";

import { useEffect, useMemo, useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { useToast } from "@/components/session/ToastProvider";
import { apiJson } from "@/lib/api-client";

type AuditLogDto = {
  id: number;
  action: string;
  entity: string;
  entityId: string;
  role: string;
  userId?: string | null;
  ipAddress?: string | null;
  timestampUtc: string;
};

export default function AdminAuditLogsPage() {
  const toast = useToast();
  const [items, setItems] = useState<AuditLogDto[]>([]);
  const [entity, setEntity] = useState("");
  const [action, setAction] = useState("");
  const [loading, setLoading] = useState(false);

  const query = useMemo(() => {
    const params = new URLSearchParams();
    params.set("take", "200");
    if (entity.trim()) params.set("entity", entity.trim());
    if (action.trim()) params.set("action", action.trim());
    return params.toString();
  }, [entity, action]);

  async function load() {
    setLoading(true);
    try {
      const list = await apiJson<AuditLogDto[]>(`/backend/admin/audit-logs?${query}`);
      setItems(list);
    } catch (e) {
      toast.error(e instanceof Error ? e.message : "Audit log yüklenemedi.");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <div className="grid gap-6">
      <PageHeader title="Audit Log" subtitle="Kim neyi ne zaman değiştirdi" />

      <Card>
        <div className="grid gap-3 sm:grid-cols-[1fr_1fr_auto]">
          <Input value={entity} onChange={(e) => setEntity(e.target.value)} placeholder="Entity (örn: Hospital, User, Doctor, Department, Appointment)" />
          <Input value={action} onChange={(e) => setAction(e.target.value)} placeholder="Action (Created/Updated/Deleted)" />
          <Button onClick={load} isLoading={loading}>Filtrele</Button>
        </div>
      </Card>

      <Card>
        <div className="space-y-2">
          <p className="text-sm text-slate-600 dark:text-slate-400">Toplam {items.length} kayıt</p>
          <ul className="grid gap-2">
            {items.map((x) => (
              <li key={x.id} className="rounded-lg border border-slate-200 p-3 text-sm dark:border-slate-700">
                <div className="flex flex-wrap items-center gap-x-3 gap-y-1">
                  <span className="font-medium text-slate-900 dark:text-slate-100">{x.action}</span>
                  <span className="text-slate-700 dark:text-slate-300">{x.entity}</span>
                  <span className="text-slate-500 dark:text-slate-400">#{x.entityId}</span>
                  <span className="text-slate-500 dark:text-slate-400">{new Date(x.timestampUtc).toLocaleString("tr-TR")}</span>
                </div>
                <div className="mt-1 text-xs text-slate-500 dark:text-slate-400">
                  {x.role ? `Rol: ${x.role}` : null}{x.userId ? ` • UserId: ${x.userId}` : ""}{x.ipAddress ? ` • IP: ${x.ipAddress}` : ""}
                </div>
              </li>
            ))}
          </ul>
        </div>
      </Card>
    </div>
  );
}
