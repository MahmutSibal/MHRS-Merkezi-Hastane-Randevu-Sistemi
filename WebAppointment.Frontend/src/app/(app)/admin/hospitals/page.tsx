"use client";

import { useEffect, useMemo, useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useToast } from "@/components/session/ToastProvider";
import { apiJson } from "@/lib/api-client";
// Harita için Google yerine OpenStreetMap embed kullanıyoruz; API anahtarı gerektirmez.

type HospitalDto = { id: number; name: string; address?: string | null; latitude?: number | null; longitude?: number | null; type: "Public" | "Private" };
type SubAdminDto = { id: string; email: string };

export default function AdminHospitalsPage() {
  const toast = useToast();
  const [hospitals, setHospitals] = useState<HospitalDto[]>([]);
  const [subAdmins, setSubAdmins] = useState<SubAdminDto[]>([]);
  const [subAdminsLoading, setSubAdminsLoading] = useState(false);
  const [editEmailById, setEditEmailById] = useState<Record<string, string>>({});
  const [editPasswordById, setEditPasswordById] = useState<Record<string, string>>({});
  const [name, setName] = useState("");
  const [address, setAddress] = useState("");
  const [latitude, setLatitude] = useState<string>("");
  const [longitude, setLongitude] = useState<string>("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [emailError, setEmailError] = useState<string | null>(null);
  const [passwordError, setPasswordError] = useState<string | null>(null);
  const [selectedId, setSelectedId] = useState<number | null>(null);
  const [type, setType] = useState<"Public" | "Private">("Public");

  const center = useMemo(() => ({ lat: 39.925533, lng: 32.866287 }), []); // Ankara merkez
  const [mapCenter, setMapCenter] = useState(center);

  async function load() {
    try {
      const list = await apiJson<HospitalDto[]>("/backend/admin/hospitals");
      setHospitals(list);
    } catch (e) {
      toast.error(e instanceof Error ? e.message : "Hastaneler yüklenemedi.");
    }
  }

  async function loadSubAdmins(hospitalId: number) {
    setSubAdminsLoading(true);
    try {
      const list = await apiJson<SubAdminDto[]>(`/backend/admin/hospitals/${hospitalId}/subadmins`);
      setSubAdmins(list);

      const emailMap: Record<string, string> = {};
      const passwordMap: Record<string, string> = {};
      for (const s of list) {
        emailMap[s.id] = s.email;
        passwordMap[s.id] = "";
      }
      setEditEmailById(emailMap);
      setEditPasswordById(passwordMap);
    } catch (e) {
      setSubAdmins([]);
      toast.error(e instanceof Error ? e.message : "Alt adminler yüklenemedi.");
    } finally {
      setSubAdminsLoading(false);
    }
  }

  useEffect(() => { load(); }, []);

  useEffect(() => {
    if (!selectedId) {
      setSubAdmins([]);
      setEditEmailById({});
      setEditPasswordById({});
      return;
    }
    loadSubAdmins(selectedId);
  }, [selectedId]);

  // Real-time validation (Doctor sayfasıyla uyumlu)
  useEffect(() => {
    const e = email.trim();
    const p = password;

    if (!e && !p)
    {
      setEmailError(null);
      setPasswordError(null);
      return;
    }

    const hasTurkish = e.includes("ı") || e.includes("İ");
    const basicEmailOk = /.+@.+\..+/.test(e) && !hasTurkish;
    setEmailError(basicEmailOk ? null : "Geçerli bir e-posta adresi girin (Türkçe karakter yok).");

    if (p)
    {
      if (p.length < 8) setPasswordError("Şifre en az 8 karakter olmalıdır.");
      else if (/^\d+$/.test(p)) setPasswordError("Şifre sadece rakamlardan oluşamaz.");
      else setPasswordError(null);
    }
    else
    {
      setPasswordError(null);
    }
  }, [email, password]);

  // Otomatik geocode (OpenStreetMap Nominatim): isim + şehir/adres yazıldığında enlem/boylam doldur ve haritayı yakınlaştır
  useEffect(() => {
    const q = [name.trim(), address.trim()].filter(Boolean).join(" ");
    if (!q) return;
    const handle = setTimeout(async () => {
      try {
        const url = `https://nominatim.openstreetmap.org/search?format=json&limit=1&q=${encodeURIComponent(q)}`;
        const res = await fetch(url, { headers: { Accept: "application/json" } });
        const data: Array<{ lat: string; lon: string }> = await res.json();
        const first = data[0];
        if (first) {
          const lat = parseFloat(first.lat);
          const lng = parseFloat(first.lon);
          setLatitude(String(lat));
          setLongitude(String(lng));
          setMapCenter({ lat, lng });
        }
      } catch {}
    }, 600);
    return () => clearTimeout(handle);
  }, [name, address]);

  async function create() {
    try {
      const dto = await apiJson<HospitalDto>("/backend/admin/hospitals", {
        method: "POST",
        body: JSON.stringify({ name, address, latitude: latitude ? Number(latitude) : null, longitude: longitude ? Number(longitude) : null, type }),
      });
      toast.success("Hastane oluşturuldu");
      setName(""); setAddress(""); setLatitude(""); setLongitude("");
      setHospitals((prev) => [dto, ...prev]);
    } catch (e) {
      toast.error(e instanceof Error ? e.message : "Oluşturma başarısız.");
    }
  }

  async function assignSubAdmin() {
    if (!selectedId) return;
    try {
      await apiJson<string>(`/backend/admin/hospitals/${selectedId}/assign-subadmin`, {
        method: "POST",
        body: JSON.stringify({ email, password }),
      });
      toast.success("Alt admin atandı");
      setEmail(""); setPassword("");
      await loadSubAdmins(selectedId);
    } catch (e) {
      toast.error(e instanceof Error ? e.message : "Atama başarısız.");
    }
  }

  async function updateSubAdminCredentials(subAdminUserId: string) {
    if (!selectedId) return;
    try {
      const newEmail = (editEmailById[subAdminUserId] ?? "").trim();
      const newPassword = editPasswordById[subAdminUserId] ?? "";

      await apiJson<void>(`/backend/admin/hospitals/${selectedId}/subadmins/${subAdminUserId}/credentials`, {
        method: "PATCH",
        body: JSON.stringify({
          email: newEmail ? newEmail : null,
          password: newPassword ? newPassword : null,
        }),
      });

      toast.success("Alt admin güncellendi");
      await loadSubAdmins(selectedId);
    } catch (e) {
      toast.error(e instanceof Error ? e.message : "Güncelleme başarısız.");
    }
  }

  async function deleteSubAdmin(subAdminUserId: string) {
    if (!selectedId) return;
    try {
      await apiJson<void>(`/backend/admin/hospitals/${selectedId}/subadmins/${subAdminUserId}`, {
        method: "DELETE",
      });
      toast.success("Alt admin silindi");
      setSubAdmins((prev) => prev.filter((x) => x.id !== subAdminUserId));
    } catch (e) {
      toast.error(e instanceof Error ? e.message : "Silme başarısız.");
    }
  }

  const canAssignSubAdmin = useMemo(() => {
    return !!selectedId && email.trim().length > 0 && password.length > 0 && !emailError && !passwordError;
  }, [selectedId, email, password, emailError, passwordError]);

  return (
    <div className="grid gap-6">
      <PageHeader title="Hastane Yönetimi" subtitle="Hastane ekle ve alt admin ata." />

      <Card>
        <div className="grid gap-3 lg:grid-cols-[1fr_380px]">
          <div className="grid gap-3 sm:grid-cols-2">
            <Input value={name} onChange={(e) => setName(e.target.value)} placeholder="Hastane adı" />
            <Input value={address} onChange={(e) => setAddress(e.target.value)} placeholder="Adres" />
            <div>
              <div className="mb-2 block text-sm font-medium text-slate-700 dark:text-slate-300">Tür</div>
              <select className="w-full rounded-lg border-2 border-slate-200 bg-white px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100" value={type} onChange={(e) => setType(e.target.value as any)}>
                <option value="Public">Devlet Hastanesi</option>
                <option value="Private">Özel Hastane</option>
              </select>
            </div>
            <Input value={latitude} readOnly placeholder="Enlem (lat)" />
            <Input value={longitude} readOnly placeholder="Boylam (lng)" />
            <Button onClick={create}>Hastane Oluştur</Button>
          </div>

          <div className="rounded-xl overflow-hidden border border-slate-200 dark:border-slate-700 min-h-[300px]">

            {(() => {
              // OSM embed: center etrafında küçük bir bbox + marker
              const delta = 0.02;
              const bbox = `${mapCenter.lng - delta},${mapCenter.lat - delta},${mapCenter.lng + delta},${mapCenter.lat + delta}`;
              const marker = `${mapCenter.lat}%2C${mapCenter.lng}`;
              const src = `https://www.openstreetmap.org/export/embed.html?bbox=${bbox}&layer=mapnik&marker=${marker}`;
              return (
                <iframe title="Harita" src={src} style={{ width: "100%", height: "300px", border: 0 }} />
              );
            })()}

          </div>
        </div>
      </Card>

      <Card>
        <div className="space-y-4">
          <div className="grid gap-3 sm:grid-cols-3">
            <select className="rounded-lg border-2 border-slate-200 bg-white px-3 py-2 text-sm dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100" value={selectedId ?? 0} onChange={(e) => setSelectedId(Number(e.target.value) || null)}>
              <option value={0}>Hastane seçin...</option>
              {hospitals.map((h) => (
                <option key={h.id} value={h.id}>{h.name}</option>
              ))}
            </select>
            <div>
              <Input value={email} onChange={(e) => setEmail(e.target.value)} placeholder="Alt admin e-posta" aria-invalid={!!emailError} />
              {emailError && <p className="mt-1 text-xs text-red-600">{emailError}</p>}
            </div>
            <div>
              <Input type="password" value={password} onChange={(e) => setPassword(e.target.value)} placeholder="Şifre" aria-invalid={!!passwordError} />
              {passwordError && <p className="mt-1 text-xs text-red-600">{passwordError}</p>}
            </div>
          </div>
          <Button onClick={assignSubAdmin} disabled={!canAssignSubAdmin}>Alt Admin Ata</Button>

          {selectedId ? (
            <div className="space-y-2">
              <div className="text-sm font-medium text-slate-700 dark:text-slate-300">Alt Adminler</div>
              {subAdminsLoading ? (
                <p className="text-sm text-slate-600 dark:text-slate-400">Yükleniyor...</p>
              ) : subAdmins.length === 0 ? (
                <p className="text-sm text-slate-600 dark:text-slate-400">Alt admin yok.</p>
              ) : (
                <ul className="grid gap-2">
                  {subAdmins.map((s) => (
                    <li key={s.id} className="rounded-lg border border-slate-200 p-3 dark:border-slate-700">
                      <div className="grid gap-3 sm:grid-cols-[1fr_1fr_auto_auto]">
                        <Input
                          value={editEmailById[s.id] ?? ""}
                          onChange={(e) => setEditEmailById((prev) => ({ ...prev, [s.id]: e.target.value }))}
                          placeholder="E-posta"
                        />
                        <Input
                          type="password"
                          value={editPasswordById[s.id] ?? ""}
                          onChange={(e) => setEditPasswordById((prev) => ({ ...prev, [s.id]: e.target.value }))}
                          placeholder="Yeni şifre (opsiyonel)"
                        />
                        <Button onClick={() => updateSubAdminCredentials(s.id)}>Kaydet</Button>
                        <Button variant="danger" onClick={() => deleteSubAdmin(s.id)}>Sil</Button>
                      </div>
                    </li>
                  ))}
                </ul>
              )}
            </div>
          ) : null}
        </div>
      </Card>

      <Card>
        <div className="space-y-2">
          <p className="text-sm text-slate-600 dark:text-slate-400">Toplam {hospitals.length} hastane</p>
          <ul className="grid gap-2">
            {hospitals.map((h) => (
              <li
                key={h.id}
                className={`rounded-lg border border-slate-200 p-3 dark:border-slate-700 ${selectedId === h.id ? "bg-slate-50 dark:bg-slate-900/20" : ""}`}
                role="button"
                tabIndex={0}
                onClick={() => setSelectedId(h.id)}
                onKeyDown={(e) => {
                  if (e.key === "Enter" || e.key === " ") setSelectedId(h.id);
                }}
              >
                <div className="font-medium">{h.name}</div>
                {h.address ? <div className="text-xs text-slate-500">{h.address}</div> : null}
                {(h.latitude ?? null) && (h.longitude ?? null) ? <div className="text-xs text-slate-500">({h.latitude}, {h.longitude}) • {(h.type === "Public" ? "Devlet" : "Özel")}</div> : <div className="text-xs text-slate-500">{(h.type === "Public" ? "Devlet" : "Özel")}</div>}
              </li>
            ))}
          </ul>
        </div>
      </Card>
    </div>
  );
}
