"use client";

import { useEffect, useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useToast } from "@/components/session/ToastProvider";
import { apiJson } from "@/lib/api-client";

type DoctorDailySlotDto = { startAtUtc: string; endAtUtc: string; isAvailable: boolean };

type DoctorAvailabilityDto = {
  workStart: string;
  workEnd: string;
  lunchStart?: string | null;
  lunchEnd?: string | null;
  slotMinutes: number;
};

type DoctorTimeOffDto = { id: number; startAtUtc: string; endAtUtc: string; reason?: string | null };

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

  const [availability, setAvailability] = useState<DoctorAvailabilityDto | null>(null);
  const [isAvailabilityLoading, setIsAvailabilityLoading] = useState(true);
  const [isAvailabilitySaving, setIsAvailabilitySaving] = useState(false);

  const [timeOffStartLocal, setTimeOffStartLocal] = useState<string>("");
  const [timeOffEndLocal, setTimeOffEndLocal] = useState<string>("");
  const [timeOffReason, setTimeOffReason] = useState<string>("");
  const [myTimeOffs, setMyTimeOffs] = useState<DoctorTimeOffDto[]>([]);
  const [isTimeOffsLoading, setIsTimeOffsLoading] = useState(false);
  const [isTimeOffCreating, setIsTimeOffCreating] = useState(false);

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

  async function loadAvailability() {
    setIsAvailabilityLoading(true);
    try {
      const data = await apiJson<DoctorAvailabilityDto>("/backend/doctor/availability/me");
      setAvailability(data);
    } catch (e) {
      const errorMsg = e instanceof Error ? e.message : "Çalışma saatleri yüklenemedi.";
      toast.error(errorMsg);
      setAvailability(null);
    } finally {
      setIsAvailabilityLoading(false);
    }
  }

  async function saveAvailability() {
    if (!availability) return;
    setIsAvailabilitySaving(true);
    try {
      await apiJson("/backend/doctor/availability/me", {
        method: "PUT",
        body: JSON.stringify({
          workStart: availability.workStart,
          workEnd: availability.workEnd,
          lunchStart: availability.lunchStart ? availability.lunchStart : null,
          lunchEnd: availability.lunchEnd ? availability.lunchEnd : null,
          slotMinutes: 30,
        }),
      });
      toast.success("Çalışma saatleri güncellendi");
      await load();
    } catch (e) {
      toast.error(e instanceof Error ? e.message : "Kaydetme başarısız.");
    } finally {
      setIsAvailabilitySaving(false);
    }
  }

  function dayRangeUtcForDate(dateYmd: string) {
    const startLocal = new Date(`${dateYmd}T00:00:00`);
    const endLocal = new Date(`${dateYmd}T23:59:59`);
    return { fromUtc: startLocal.toISOString(), toUtc: endLocal.toISOString() };
  }

  async function loadMyTimeOffs() {
    setIsTimeOffsLoading(true);
    try {
      const { fromUtc, toUtc } = dayRangeUtcForDate(date);
      const data = await apiJson<DoctorTimeOffDto[]>(`/backend/doctor/time-offs/me?fromUtc=${encodeURIComponent(fromUtc)}&toUtc=${encodeURIComponent(toUtc)}`);
      setMyTimeOffs(data);
    } catch (e) {
      const errorMsg = e instanceof Error ? e.message : "İzinler yüklenemedi.";
      toast.error(errorMsg);
      setMyTimeOffs([]);
    } finally {
      setIsTimeOffsLoading(false);
    }
  }

  async function createTimeOff() {
    if (!timeOffStartLocal || !timeOffEndLocal) {
      toast.error("Lütfen başlangıç ve bitiş seçin.");
      return;
    }
    setIsTimeOffCreating(true);
    try {
      const startAtUtc = new Date(timeOffStartLocal).toISOString();
      const endAtUtc = new Date(timeOffEndLocal).toISOString();

      await apiJson("/backend/doctor/time-offs/me", {
        method: "POST",
        body: JSON.stringify({
          startAtUtc,
          endAtUtc,
          reason: timeOffReason.trim() ? timeOffReason.trim() : null,
        }),
      });

      toast.success("İzin eklendi");
      setTimeOffStartLocal("");
      setTimeOffEndLocal("");
      setTimeOffReason("");
      await loadMyTimeOffs();
      await load();
    } catch (e) {
      toast.error(e instanceof Error ? e.message : "İzin ekleme başarısız.");
    } finally {
      setIsTimeOffCreating(false);
    }
  }

  useEffect(() => {
    void load();
    void loadAvailability();
    void loadMyTimeOffs();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    void loadMyTimeOffs();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [date]);

  return (
    <div className="grid gap-6">
      <PageHeader title="Takvim" subtitle="Günlük slot görünümü." />

      <Card>
        <div className="space-y-3">
          <div className="text-sm font-medium text-slate-700 dark:text-slate-300">Çalışma Saatleri</div>
          {isAvailabilityLoading ? (
            <div className="text-sm text-slate-600 dark:text-slate-400">Yükleniyor…</div>
          ) : availability ? (
            <>
              <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
                <Input label="Başlangıç" value={availability.workStart} onChange={(e) => setAvailability({ ...availability, workStart: e.target.value })} placeholder="09:00" />
                <Input label="Bitiş" value={availability.workEnd} onChange={(e) => setAvailability({ ...availability, workEnd: e.target.value })} placeholder="17:00" />
                <Input label="Öğle Başlangıç (ops.)" value={availability.lunchStart ?? ""} onChange={(e) => setAvailability({ ...availability, lunchStart: e.target.value })} placeholder="12:00" />
                <Input label="Öğle Bitiş (ops.)" value={availability.lunchEnd ?? ""} onChange={(e) => setAvailability({ ...availability, lunchEnd: e.target.value })} placeholder="13:00" />
              </div>
              <div className="flex items-center gap-3">
                <Button onClick={saveAvailability} isLoading={isAvailabilitySaving}>Kaydet</Button>
                <div className="text-xs text-slate-500 dark:text-slate-400">Slot süresi şu an sabit: 30 dk</div>
              </div>
            </>
          ) : (
            <div className="text-sm text-slate-600 dark:text-slate-400">Çalışma saatleri bulunamadı.</div>
          )}
        </div>
      </Card>

      <Card>
        <div className="space-y-3">
          <div className="text-sm font-medium text-slate-700 dark:text-slate-300">İzin / Uygun Değil Zaman</div>
          <div className="grid gap-3 sm:grid-cols-2">
            <Input label="Başlangıç" type="datetime-local" value={timeOffStartLocal} onChange={(e) => setTimeOffStartLocal(e.target.value)} />
            <Input label="Bitiş" type="datetime-local" value={timeOffEndLocal} onChange={(e) => setTimeOffEndLocal(e.target.value)} />
            <div className="sm:col-span-2">
              <Input label="Açıklama (opsiyonel)" value={timeOffReason} onChange={(e) => setTimeOffReason(e.target.value)} placeholder="Örn: Kongre / İzin" />
            </div>
          </div>
          <Button onClick={createTimeOff} isLoading={isTimeOffCreating} disabled={!timeOffStartLocal || !timeOffEndLocal}>
            İzin Ekle
          </Button>
        </div>
      </Card>

      <Card>
        <div className="grid gap-3 sm:grid-cols-[1fr_auto] sm:items-end">
          <Input label="Tarih" type="date" value={date} onChange={(e) => setDate(e.target.value)} />
          <Button onClick={load}>Getir</Button>
        </div>
      </Card>

      <Card>
        <div className="space-y-2">
          <div className="text-sm font-medium text-slate-700 dark:text-slate-300">Seçili Gün İzinleri</div>
          {isTimeOffsLoading ? (
            <div className="text-sm text-slate-600 dark:text-slate-400">Yükleniyor…</div>
          ) : myTimeOffs.length === 0 ? (
            <div className="text-sm text-slate-600 dark:text-slate-400">Bu gün için izin bulunmuyor.</div>
          ) : (
            <div className="grid gap-2 sm:grid-cols-2 lg:grid-cols-3">
              {myTimeOffs.map((t) => {
                const startLocal = new Date(t.startAtUtc);
                const endLocal = new Date(t.endAtUtc);
                return (
                  <div key={t.id} className="rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-800">
                    <div className="font-medium text-slate-900 dark:text-slate-100">
                      {startLocal.toLocaleTimeString("tr-TR", { hour: "2-digit", minute: "2-digit" })} – {endLocal.toLocaleTimeString("tr-TR", { hour: "2-digit", minute: "2-digit" })}
                    </div>
                    <div className="text-xs text-slate-600 dark:text-slate-400">{t.reason ? t.reason : "İzinli"}</div>
                  </div>
                );
              })}
            </div>
          )}
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
