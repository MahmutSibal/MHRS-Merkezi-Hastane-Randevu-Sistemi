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

export default function AdminHospitalsPage() {
  const toast = useToast();
  const [hospitals, setHospitals] = useState<HospitalDto[]>([]);
  const [name, setName] = useState("");
  const [address, setAddress] = useState("");
  const [latitude, setLatitude] = useState<string>("");
  const [longitude, setLongitude] = useState<string>("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
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

  useEffect(() => { load(); }, []);

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
    } catch (e) {
      toast.error(e instanceof Error ? e.message : "Atama başarısız.");
    }
  }

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
            <Input value={latitude} onChange={(e) => setLatitude(e.target.value)} placeholder="Enlem (lat)" />
            <Input value={longitude} onChange={(e) => setLongitude(e.target.value)} placeholder="Boylam (lng)" />
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
            <Input value={email} onChange={(e) => setEmail(e.target.value)} placeholder="Alt admin e-posta" />
            <Input type="password" value={password} onChange={(e) => setPassword(e.target.value)} placeholder="Şifre" />
          </div>
          <Button onClick={assignSubAdmin} disabled={!selectedId || !email || !password}>Alt Admin Ata</Button>
        </div>
      </Card>

      <Card>
        <div className="space-y-2">
          <p className="text-sm text-slate-600 dark:text-slate-400">Toplam {hospitals.length} hastane</p>
          <ul className="grid gap-2">
            {hospitals.map((h) => (
              <li key={h.id} className="rounded-lg border border-slate-200 p-3 dark:border-slate-700">
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
