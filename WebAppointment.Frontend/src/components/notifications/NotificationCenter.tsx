"use client";

import { useEffect, useMemo, useState } from "react";
import { apiJson } from "@/lib/api-client";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { BellIcon } from "@/components/ui/icons";

type NotificationDto = {
  id: number;
  appointmentId: string;
  message: string;
  createdAtUtc: string;
  isSent: boolean;
  isRead: boolean;
};

export function NotificationCenter() {
  const [isOpen, setIsOpen] = useState(false);
  const [items, setItems] = useState<NotificationDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const unreadCount = useMemo(() => items.filter((x) => !x.isRead).length, [items]);

  async function load() {
    setIsLoading(true);
    setError(null);
    try {
      const data = await apiJson<NotificationDto[]>("/backend/notifications?take=8");
      setItems(data);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Bildirimler yüklenemedi.");
    } finally {
      setIsLoading(false);
    }
  }

  async function markAllRead() {
    setIsLoading(true);
    setError(null);
    try {
      await apiJson<number>("/backend/notifications/mark-all-read", { method: "POST" });
      setItems((prev) => prev.map((item) => ({ ...item, isRead: true })));
    } catch (e) {
      setError(e instanceof Error ? e.message : "Bildirimler güncellenemedi.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    if (isOpen) {
      void load();
    }
  }, [isOpen]);

  return (
    <div className="relative">
      <Button
        variant="secondary"
        size="sm"
        onClick={() => setIsOpen((prev) => !prev)}
        className="relative"
        aria-label="Bildirimler"
      >
        <BellIcon className="h-4 w-4" />
        {unreadCount > 0 ? (
          <span className="absolute -right-1 -top-1 flex h-4 min-w-[16px] items-center justify-center rounded-full bg-rose-500 px-1 text-[10px] font-semibold text-white">
            {unreadCount > 9 ? "9+" : unreadCount}
          </span>
        ) : null}
      </Button>

      {isOpen ? (
        <div className="absolute right-0 top-12 z-50 w-[min(320px,90vw)]">
          <Card>
            <div className="flex items-center justify-between gap-2">
              <div>
                <h4 className="text-sm font-semibold text-slate-900 dark:text-slate-100">Bildirimler</h4>
                <p className="text-xs text-slate-500 dark:text-slate-400">Son randevu güncellemeleri</p>
              </div>
              <div className="flex items-center gap-2">
                <Button variant="ghost" size="sm" onClick={load} disabled={isLoading}>
                  Yenile
                </Button>
                <Button variant="ghost" size="sm" onClick={markAllRead} disabled={isLoading || items.length === 0}>
                  Okundu Yap
                </Button>
              </div>
            </div>

            <div className="mt-3 space-y-3">
              {error ? <p className="text-xs text-red-600">{error}</p> : null}
              {isLoading ? <p className="text-xs text-slate-500">Yükleniyor…</p> : null}
              {!isLoading && items.length === 0 ? (
                <p className="text-xs text-slate-500">Yeni bildirim bulunmuyor.</p>
              ) : null}
              {items.map((item) => (
                <div
                  key={item.id}
                  className={`rounded-xl border p-3 text-xs dark:border-slate-700 ${item.isRead ? "border-slate-200" : "border-blue-200 bg-blue-50/40 dark:border-blue-900/60 dark:bg-blue-900/10"}`}
                >
                  <div className="flex items-center justify-between gap-2">
                    <span className="font-medium text-slate-700 dark:text-slate-200">
                      {item.message}
                    </span>
                    {!item.isRead ? <span className="rounded-full bg-blue-100 px-2 py-0.5 text-[10px] text-blue-700">Yeni</span> : null}
                  </div>
                  <div className="mt-1 text-[11px] text-slate-500">
                    {new Date(item.createdAtUtc).toLocaleString("tr-TR")}
                  </div>
                </div>
              ))}
            </div>
          </Card>
        </div>
      ) : null}
    </div>
  );
}
