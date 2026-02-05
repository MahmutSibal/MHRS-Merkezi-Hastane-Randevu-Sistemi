"use client";

import { useEffect, useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useToast } from "@/components/session/ToastProvider";
import { apiJson } from "@/lib/api-client";

type DoctorDailySlotDto = { startAtUtc: string; endAtUtc: string; isAvailable: boolean };

function todayYmd() {
  const d = new Date();
  const y = d.getFullYear();
  const m = String(d.getMonth() + 1).padStart(2, "0");
  const day = String(d.getDate()).padStart(2, "0");
  return `${y}-${m}-${day}`;
}

export default function DoctorCalendarPage() {
  const toast = useToast();
  const [date, setDate] = useState(todayYmd());
  const [items, setItems] = useState<DoctorDailySlotDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  async function load() {
    setIsLoading(true);
    setError(null);
    try {
      const data = await apiJson<DoctorDailySlotDto[]>(`/backend/doctor/calendar/daily-slots?date=${encodeURIComponent(date)}`);
      setItems(data);
      toast.success("Slotlar yüklendi");
    } catch (e) {
      const errorMsg = e instanceof Error ? e.message : "Yükleme başarısız.";
      setError(errorMsg);
      toast.error(errorMsg);
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <div className="grid gap-6">
      <PageHeader title="Takvim" subtitle="Günlük slot görünümü." />

      <Card>
        <div className="grid gap-3 sm:grid-cols-[1fr_auto] sm:items-end">
          <Input label="Tarih" type="date" value={date} onChange={(e) => setDate(e.target.value)} />
          <Button onClick={load}>Getir</Button>
        </div>
      </Card>

      {error ? (
        <Card>
          <div className="space-y-2">
            <p className="text-sm font-semibold text-red-700 dark:text-red-400">Hata: {error}</p>
            {error.includes("403") && (
              <p className="text-xs text-red-600 dark:text-red-300">
                Doktor profili oluşturulmamış. Lütfen sistem yöneticisine başvurun.
              </p>
            )}
          </div>
        </Card>
      ) : null}
      
      {isLoading ? <Card><p className="text-sm text-slate-600 dark:text-slate-400">Yükleniyor…</p></Card> : null}

      {!error && items.length > 0 && (
        <>
          <Card>
            <div className="grid gap-2 sm:grid-cols-2 lg:grid-cols-3">
              {items
                .map((s) => {
                  const startDate = new Date(s.startAtUtc);
                  const endDate = new Date(s.endAtUtc);
                  
                  return (
                    <div
                      key={s.startAtUtc}
                      className={
                        "rounded-lg border px-3 py-2 text-sm transition " +
                        (s.isAvailable 
                          ? "border-emerald-200 bg-emerald-50 dark:border-emerald-900 dark:bg-emerald-950" 
                          : "border-slate-200 bg-slate-50 dark:border-slate-700 dark:bg-slate-800"
                        )
                      }
                    >
                      <div className="font-medium text-slate-900 dark:text-slate-100">
                        {startDate.toLocaleTimeString("tr-TR", { hour: "2-digit", minute: "2-digit" })} –
                        {" "}
                        {endDate.toLocaleTimeString("tr-TR", { hour: "2-digit", minute: "2-digit" })}
                      </div>
                      <div className="text-xs text-slate-600 dark:text-slate-400">
                        {s.isAvailable ? "Uygun" : "Dolu"}
                      </div>
                    </div>
                  );
                })
              }
            </div>
          </Card>
        </>
      )}
    </div>
  );
}
