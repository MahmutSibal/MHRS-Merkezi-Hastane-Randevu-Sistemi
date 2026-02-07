"use client";

import { useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { useToast } from "@/components/session/ToastProvider";
import { apiJson } from "@/lib/api-client";

type HospitalDto = { id: number; name: string; address?: string | null; latitude?: number | null; longitude?: number | null };
type DepartmentDto = { id: number; name: string };
type DoctorDto = { id: number; name: string; departmentId: number; departmentName: string; isActive: boolean; userId: string | null };
type DependentDto = { id: number; fullName: string; tcKimlikNo: string; birthDate: string; relation: string };

type DoctorDailySlotPublicDto = {
  startTime: string; // HH:mm
  endTime: string; // HH:mm
  isAvailable: boolean;
  unavailableReason?: string | null;
};

type DoctorPublicDetailDto = {
  id: number;
  name: string;
  title: string;
  departmentId: number;
  departmentName: string;
  profileStatus: string;
  graduationUniversity: string | null;
  experienceSummary: string | null;
};

function withTurkeyOffset(date: string, time: string) {
  if (!date || !time) return "";
  const dateTimeLocal = `${date}T${time}:00`;
  return `${dateTimeLocal}+03:00`;
}

export default function NewAppointmentPage() {
  const router = useRouter();
  const toast = useToast();
  const [hospitals, setHospitals] = useState<HospitalDto[]>([]);
  const [departments, setDepartments] = useState<DepartmentDto[]>([]);
  const [doctors, setDoctors] = useState<DoctorDto[]>([]);
  const [dependents, setDependents] = useState<DependentDto[]>([]);

  const [hospitalId, setHospitalId] = useState<number>(0);
  const [departmentId, setDepartmentId] = useState<number>(0);
  const [doctorId, setDoctorId] = useState<number>(0);
  const [date, setDate] = useState<string>("");
  const [time, setTime] = useState<string>("");
  const [dependentId, setDependentId] = useState<number>(0);

  const [doctorDetail, setDoctorDetail] = useState<DoctorPublicDetailDto | null>(null);
  const [isDoctorDetailLoading, setIsDoctorDetailLoading] = useState(false);

  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [slots, setSlots] = useState<DoctorDailySlotPublicDto[]>([]);
  const [isSlotsLoading, setIsSlotsLoading] = useState(false);

  async function loadDependents() {
    try {
      const list = await apiJson<DependentDto[]>("/backend/patient/dependents/me");
      setDependents(list);
    } catch (e) {
      // Optional feature; keep page usable even if this fails.
      setDependents([]);
    }
  }

  useEffect(() => {
    (async () => {
      setIsLoading(true);
      setError(null);
      try {
        // Try geolocation for nearest hospitals
        let nearest: HospitalDto[] | null = null;
        if (typeof navigator !== "undefined" && navigator.geolocation) {
          const pos = await new Promise<GeolocationPosition | null>((resolve) => {
            navigator.geolocation.getCurrentPosition(
              (p) => resolve(p),
              () => resolve(null),
              { enableHighAccuracy: true, timeout: 5000 }
            );
          });
          if (pos) {
            nearest = await apiJson<HospitalDto[]>(`/backend/catalog/hospitals?latitude=${pos.coords.latitude}&longitude=${pos.coords.longitude}&take=10`);
          }
        }

        const hosps = nearest ?? await apiJson<HospitalDto[]>("/backend/catalog/hospitals");
        setHospitals(hosps);
        const firstHospitalId = hosps[0]?.id ?? 0;
        setHospitalId(firstHospitalId);
      } catch (e) {
        setError(e instanceof Error ? e.message : "Hastaneler yüklenemedi.");
      } finally {
        setIsLoading(false);
      }
    })();
  }, []);

  useEffect(() => {
    void loadDependents();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    (async () => {
      if (!hospitalId) { setDepartments([]); setDepartmentId(0); return; }
      setError(null);
      try {
        const deps = await apiJson<DepartmentDto[]>(`/backend/catalog/departments?hospitalId=${hospitalId}`);
        setDepartments(deps);
        const firstId = deps[0]?.id ?? 0;
        setDepartmentId(firstId);
      } catch (e) {
        setError(e instanceof Error ? e.message : "Bölümler yüklenemedi.");
      }
    })();
  }, [hospitalId]);

  useEffect(() => {
    (async () => {
      if (!departmentId) return;
      setError(null);
      try {
        const docs = await apiJson<DoctorDto[]>(`/backend/catalog/doctors?departmentId=${departmentId}`);
        setDoctors(docs);
        setDoctorId(docs[0]?.id ?? 0);
      } catch (e) {
        setError(e instanceof Error ? e.message : "Doktorlar yüklenemedi.");
      }
    })();
  }, [departmentId]);

  useEffect(() => {
    let isStale = false;
    (async () => {
      if (!doctorId) {
        setDoctorDetail(null);
        return;
      }
      setIsDoctorDetailLoading(true);
      try {
        const dto = await apiJson<DoctorPublicDetailDto>(`/backend/catalog/doctors/${doctorId}`);
        if (!isStale) setDoctorDetail(dto);
      } catch (e) {
        if (!isStale) setDoctorDetail(null);
      } finally {
        if (!isStale) setIsDoctorDetailLoading(false);
      }
    })();
    return () => {
      isStale = true;
    };
  }, [doctorId]);

  useEffect(() => {
    let isStale = false;
    (async () => {
      if (!doctorId || !date) {
        setSlots([]);
        setTime("");
        return;
      }

      setIsSlotsLoading(true);
      setError(null);
      try {
        const data = await apiJson<DoctorDailySlotPublicDto[]>(`/backend/catalog/doctors/${doctorId}/daily-slots?date=${encodeURIComponent(date)}`);
        if (isStale) return;
        setSlots(data);

        const firstAvailable = data.find((x) => x.isAvailable)?.startTime ?? "";
        setTime((prev) => {
          if (prev && data.some((x) => x.startTime === prev && x.isAvailable)) return prev;
          return firstAvailable;
        });
      } catch (e) {
        if (isStale) return;
        setSlots([]);
        setTime("");
        const msg = e instanceof Error ? e.message : "Uygun saatler getirilemedi.";
        setError(msg);
      } finally {
        if (!isStale) setIsSlotsLoading(false);
      }
    })();
    return () => {
      isStale = true;
    };
  }, [doctorId, date]);

  const selectedDoctor = useMemo(() => doctors.find((d) => d.id === doctorId) ?? null, [doctors, doctorId]);

  async function submit() {
    setIsSubmitting(true);
    setError(null);
    try {
      if (!date || !time) {
        throw new Error("Lütfen tarih ve saat seçin.");
      }

      const appointmentDate = withTurkeyOffset(date, time);
      await apiJson("/backend/appointments", {
        method: "POST",
        body: JSON.stringify({ doctorId, appointmentDate, dependentId: dependentId || null }),
      });
      toast.success("Randevu başarıyla oluşturuldu");
      router.replace("/patient/appointments");
    } catch (e) {
      const errorMsg = e instanceof Error ? e.message : "Randevu oluşturma başarısız.";
      setError(errorMsg);
      toast.error(errorMsg);
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="grid gap-6">
      <PageHeader title="Yeni Randevu" subtitle="Önce hastane, sonra bölüm ve doktor seçin." />

      {error ? (
        <Card>
          <div className="space-y-2">
            <p className="text-sm font-semibold text-red-700 dark:text-red-400">Hata: {error}</p>
          </div>
        </Card>
      ) : null}
      
      {isLoading ? <Card><p className="text-sm text-slate-600 dark:text-slate-400">Yükleniyor…</p></Card> : null}

      <Card>
        <div className="space-y-5">
          <div className="grid gap-4 sm:grid-cols-2">
            <div>
              <label className="mb-2 block text-sm font-medium text-slate-700 dark:text-slate-300">Randevu Kimin İçin?</label>
              <select
                className="w-full rounded-lg border-2 border-slate-200 bg-white px-3 py-2 text-sm outline-none transition-all focus:border-blue-500 focus:ring-2 focus:ring-blue-200 dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100 dark:focus:border-blue-400 dark:focus:ring-blue-900"
                value={dependentId}
                onChange={(e) => setDependentId(Number(e.target.value))}
              >
                <option value={0}>Kendim</option>
                {dependents.map((d) => (
                  <option key={d.id} value={d.id}>
                    {d.fullName}
                  </option>
                ))}
              </select>
              <p className="mt-1.5 text-xs text-slate-500 dark:text-slate-400">Yakın ekleme ve yönetimi Profil sayfasından yapılır.</p>
            </div>
          </div>

          <div className="grid gap-4 sm:grid-cols-2">
            <div>
              <label className="mb-2 block text-sm font-medium text-slate-700 dark:text-slate-300">Hastane</label>
              <select
                className="w-full rounded-lg border-2 border-slate-200 bg-white px-3 py-2 text-sm outline-none transition-all focus:border-blue-500 focus:ring-2 focus:ring-blue-200 dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100 dark:focus:border-blue-400 dark:focus:ring-blue-900"
                value={hospitalId}
                onChange={(e) => setHospitalId(Number(e.target.value))}
              >
                <option value={0}>Hastane seçin...</option>
                {hospitals.map((h) => (
                  <option key={h.id} value={h.id}>
                    {h.name}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="mb-2 block text-sm font-medium text-slate-700 dark:text-slate-300">Bölüm</label>
              <select
                className="w-full rounded-lg border-2 border-slate-200 bg-white px-3 py-2 text-sm outline-none transition-all focus:border-blue-500 focus:ring-2 focus:ring-blue-200 dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100 dark:focus:border-blue-400 dark:focus:ring-blue-900"
                value={departmentId}
                onChange={(e) => setDepartmentId(Number(e.target.value))}
              >
                <option value={0}>Bölüm seçin...</option>
                {departments.map((d) => (
                  <option key={d.id} value={d.id}>
                    {d.name}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label className="mb-2 block text-sm font-medium text-slate-700 dark:text-slate-300">Doktor</label>
              <select
                className="w-full rounded-lg border-2 border-slate-200 bg-white px-3 py-2 text-sm outline-none transition-all focus:border-blue-500 focus:ring-2 focus:ring-blue-200 dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100 dark:focus:border-blue-400 dark:focus:ring-blue-900"
                value={doctorId}
                onChange={(e) => setDoctorId(Number(e.target.value))}
              >
                <option value={0}>Doktor seçin...</option>
                {doctors.map((d) => (
                  <option key={d.id} value={d.id}>
                    {d.name}
                  </option>
                ))}
              </select>
              {selectedDoctor ? <div className="mt-2 text-xs text-slate-600 dark:text-slate-400">Bölüm: {selectedDoctor.departmentName}</div> : null}
            </div>
          </div>

          <Card>
            <div className="space-y-2">
              <div className="text-sm font-medium text-slate-900 dark:text-slate-100">Doktor Detayı</div>
              {isDoctorDetailLoading ? (
                <p className="text-sm text-slate-600 dark:text-slate-400">Yükleniyor…</p>
              ) : doctorDetail ? (
                <div className="space-y-2">
                  <div className="text-sm text-slate-700 dark:text-slate-300">
                    {doctorDetail.name} – {doctorDetail.title}
                  </div>
                  <div className="text-xs text-slate-500 dark:text-slate-400">{doctorDetail.departmentName}</div>

                  {doctorDetail.graduationUniversity ? (
                    <div className="text-sm">
                      <div className="font-medium">Mezuniyet</div>
                      <div className="text-slate-700 dark:text-slate-300">{doctorDetail.graduationUniversity}</div>
                    </div>
                  ) : null}

                  {doctorDetail.experienceSummary ? (
                    <div className="text-sm">
                      <div className="font-medium">Deneyim</div>
                      <div className="whitespace-pre-wrap text-slate-700 dark:text-slate-300">{doctorDetail.experienceSummary}</div>
                    </div>
                  ) : null}

                  {!doctorDetail.graduationUniversity && !doctorDetail.experienceSummary ? (
                    <p className="text-xs text-slate-500 dark:text-slate-400">Bu doktorun onaylı uzmanlık bilgisi bulunmuyor.</p>
                  ) : null}
                </div>
              ) : (
                <p className="text-xs text-slate-500 dark:text-slate-400">Doktor bilgileri getirilemedi.</p>
              )}
            </div>
          </Card>

          <div>
            <label className="mb-2 block text-sm font-medium text-slate-700 dark:text-slate-300">Tarih</label>
            <div className="relative">
              <input
                type="date"
                value={date}
                onChange={(e) => {
                  const next = e.target.value;
                  if (next) {
                    const day = new Date(`${next}T00:00:00`).getDay();
                    if (day === 0 || day === 6) {
                      toast.error("Hafta sonu randevu alınamaz.");
                      setDate("");
                      setSlots([]);
                      setTime("");
                      return;
                    }
                  }
                  setDate(next);
                }}
                min={new Date().toISOString().split('T')[0]}
                required
                className="w-full rounded-lg border-2 border-slate-200 bg-white px-4 py-2.5 text-sm font-medium outline-none transition-all duration-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100 dark:focus:border-blue-400 dark:focus:ring-blue-900 [&::-webkit-calendar-picker-indicator]:cursor-pointer [&::-webkit-calendar-picker-indicator]:opacity-60 hover:[&::-webkit-calendar-picker-indicator]:opacity-100"
              />
            </div>
            <p className="mt-1.5 text-xs text-slate-500 dark:text-slate-400">gg.aa.yyyy</p>
          </div>

          <div>
            <label className="mb-2 block text-sm font-medium text-slate-700 dark:text-slate-300">Saat</label>
            <select
              className="w-full rounded-lg border-2 border-slate-200 bg-white px-3 py-2 text-sm outline-none transition-all focus:border-blue-500 focus:ring-2 focus:ring-blue-200 disabled:opacity-60 dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100 dark:focus:border-blue-400 dark:focus:ring-blue-900"
              value={time}
              onChange={(e) => setTime(e.target.value)}
              disabled={!doctorId || !date || isSlotsLoading || slots.length === 0}
            >
              <option value="">Saat seçin…</option>
              {slots.map((s) => (
                <option key={s.startTime} value={s.startTime} disabled={!s.isAvailable}>
                  {s.startTime}{s.isAvailable ? "" : ` (Uygun değil)`}
                </option>
              ))}
            </select>
            <p className="mt-1.5 text-xs text-slate-500 dark:text-slate-400">
              {isSlotsLoading
                ? "Uygun saatler yükleniyor…"
                : doctorId && date && slots.length === 0
                  ? "Bu gün için uygun randevu saati bulunmuyor."
                  : "Sadece uygun saatler seçilebilir."}
            </p>
          </div>

          <Button 
            onClick={submit} 
            isLoading={isSubmitting} 
            disabled={!hospitalId || !departmentId || !doctorId || !date || !time}
            className="w-full"
          >
            Randevu Al
          </Button>
        </div>
      </Card>
    </div>
  );
}
