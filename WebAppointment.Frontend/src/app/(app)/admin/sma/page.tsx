"use client";

import { useEffect, useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useToast } from "@/components/session/ToastProvider";
import { apiJson } from "@/lib/api-client";
import { TURKEY_PROVINCES } from "@/lib/turkey-provinces";

type SmaCaseAdminDto = {
  id: number;
  slug: string;
  displayName: string;
  provinceSlug: string;
  provinceName: string;
  story: string | null;
  iban: string;
  bankAccountHolderName: string;
  photoUrl: string | null;
  isVerified: boolean;
  isPublished: boolean;
  createdAtUtc: string;
};

const sortedProvinces = [...TURKEY_PROVINCES].sort((a, b) => a.name.localeCompare(b.name, "tr"));

export default function AdminSmaPage() {
  const toast = useToast();
  const [items, setItems] = useState<SmaCaseAdminDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  const [isSmaEnabled, setIsSmaEnabled] = useState<boolean | null>(null);
  const [isTogglingFeature, setIsTogglingFeature] = useState(false);

  const [displayName, setDisplayName] = useState("");
  const [provinceSlug, setProvinceSlug] = useState(sortedProvinces[0]?.id ?? "");
  const [story, setStory] = useState("");
  const [iban, setIban] = useState("");
  const [accountHolder, setAccountHolder] = useState("");
  const [photoUrl, setPhotoUrl] = useState("");
  const [isSaving, setIsSaving] = useState(false);

  async function load() {
    setIsLoading(true);
    try {
      const list = await apiJson<SmaCaseAdminDto[]>("/backend/admin/sma");
      setItems(list);
    } catch (e) {
      toast.error(e instanceof Error ? e.message : "Yükleme başarısız.");
    } finally {
      setIsLoading(false);
    }
  }

  async function loadSettings() {
    try {
      const settings = await apiJson<{ isSmaEnabled: boolean }>("/backend/admin/sma/settings");
      setIsSmaEnabled(settings.isSmaEnabled);
    } catch (e) {
      toast.error(e instanceof Error ? e.message : "Ayarlar yüklenemedi.");
    }
  }

  useEffect(() => {
    void load();
    void loadSettings();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  async function toggleFeature() {
    if (isSmaEnabled === null) return;
    setIsTogglingFeature(true);
    try {
      const next = !isSmaEnabled;
      await apiJson("/backend/admin/sma/settings", {
        method: "PATCH",
        body: JSON.stringify({ isSmaEnabled: next }),
      });
      setIsSmaEnabled(next);
      toast.success(next ? "SMA Bağış sistemi aktif edildi." : "SMA Bağış sistemi kapatıldı — /sma artık erişilemez.");
    } catch (e) {
      toast.error(e instanceof Error ? e.message : "Güncellenemedi.");
    } finally {
      setIsTogglingFeature(false);
    }
  }

  async function onCreate(e: React.FormEvent) {
    e.preventDefault();
    const province = sortedProvinces.find((p) => p.id === provinceSlug);
    if (!province) return;

    setIsSaving(true);
    try {
      await apiJson("/backend/admin/sma", {
        method: "POST",
        body: JSON.stringify({
          displayName,
          provinceSlug: province.id,
          provinceName: province.name,
          story: story || null,
          iban,
          bankAccountHolderName: accountHolder,
          photoUrl: photoUrl || null,
        }),
      });
      toast.success("Vaka eklendi. Doğrulayıp yayınlamayı unutmayın.");
      setDisplayName("");
      setStory("");
      setIban("");
      setAccountHolder("");
      setPhotoUrl("");
      await load();
    } catch (e) {
      toast.error(e instanceof Error ? e.message : "Kaydedilemedi.");
    } finally {
      setIsSaving(false);
    }
  }

  async function toggleStatus(item: SmaCaseAdminDto, field: "isVerified" | "isPublished") {
    try {
      await apiJson(`/backend/admin/sma/${item.id}/status`, {
        method: "PATCH",
        body: JSON.stringify({
          isVerified: field === "isVerified" ? !item.isVerified : item.isVerified,
          isPublished: field === "isPublished" ? !item.isPublished : item.isPublished,
        }),
      });
      await load();
    } catch (e) {
      toast.error(e instanceof Error ? e.message : "Güncellenemedi.");
    }
  }

  async function remove(id: number) {
    try {
      await apiJson(`/backend/admin/sma/${id}`, { method: "DELETE" });
      toast.success("Vaka silindi.");
      await load();
    } catch (e) {
      toast.error(e instanceof Error ? e.message : "Silinemedi.");
    }
  }

  return (
    <div className="grid gap-6">
      <PageHeader
        title="SMA Bağış Yönetimi"
        subtitle="Vaka ekleyin, doğrulayın ve yayınlayın. Yalnızca doğrulanmış + yayınlanmış vakalar /sma sayfasında görünür."
      />

      <Card title="Sistem Durumu" description="Kapatıldığında ana sayfadaki MHRS/SMA menüsü kaybolur ve /sma adresine doğrudan girilse bile erişilemez.">
        <div className="flex items-center justify-between gap-4">
          <div>
            <p className="font-medium text-slate-900 dark:text-white">
              SMA Bağış Sistemi {isSmaEnabled === null ? "" : isSmaEnabled ? "Aktif" : "Kapalı"}
            </p>
            <p className="text-sm text-slate-500 dark:text-slate-400">
              {isSmaEnabled ? "Herkese açık, /sma üzerinden erişilebilir." : "Gizli — sadece siz burada yeniden açabilirsiniz."}
            </p>
          </div>
          <Button
            variant={isSmaEnabled ? "secondary" : "primary"}
            isLoading={isTogglingFeature}
            disabled={isSmaEnabled === null}
            onClick={toggleFeature}
          >
            {isSmaEnabled ? "Kapat" : "Aç"}
          </Button>
        </div>
      </Card>

      <Card title="Yeni Vaka Ekle">
        <form className="grid gap-4 sm:grid-cols-2" onSubmit={onCreate}>
          <Input label="Görünen Ad" value={displayName} onChange={(e) => setDisplayName(e.target.value)} required />

          <div>
            <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">İl</label>
            <select
              className="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 dark:border-slate-700 dark:bg-slate-900 dark:text-white"
              value={provinceSlug}
              onChange={(e) => setProvinceSlug(e.target.value)}
            >
              {sortedProvinces.map((p) => (
                <option key={p.id} value={p.id}>{p.name}</option>
              ))}
            </select>
          </div>

          <Input label="IBAN" value={iban} onChange={(e) => setIban(e.target.value)} placeholder="TR.." required />
          <Input label="Hesap Sahibi Adı" value={accountHolder} onChange={(e) => setAccountHolder(e.target.value)} required />
          <Input label="Fotoğraf URL (opsiyonel)" value={photoUrl} onChange={(e) => setPhotoUrl(e.target.value)} />

          <div className="sm:col-span-2">
            <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">Hikaye (opsiyonel)</label>
            <textarea
              className="w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 dark:border-slate-700 dark:bg-slate-900 dark:text-white"
              rows={3}
              value={story}
              onChange={(e) => setStory(e.target.value)}
            />
          </div>

          <div className="sm:col-span-2">
            <Button type="submit" isLoading={isSaving}>Ekle</Button>
          </div>
        </form>
      </Card>

      <Card title="Vakalar">
        {isLoading ? (
          <p className="text-sm text-slate-500 dark:text-slate-400">Yükleniyor…</p>
        ) : items.length === 0 ? (
          <p className="text-sm text-slate-500 dark:text-slate-400">Henüz vaka eklenmemiş.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead className="text-xs text-slate-600 dark:text-slate-400">
                <tr>
                  <th className="py-2">Ad</th>
                  <th className="py-2">İl</th>
                  <th className="py-2">IBAN</th>
                  <th className="py-2">Doğrulandı</th>
                  <th className="py-2">Yayında</th>
                  <th className="py-2">İşlem</th>
                </tr>
              </thead>
              <tbody>
                {items.map((item) => (
                  <tr key={item.id} className="border-t border-black/5">
                    <td className="py-2">{item.displayName}</td>
                    <td className="py-2">{item.provinceName}</td>
                    <td className="py-2 font-mono text-xs">{item.iban}</td>
                    <td className="py-2">
                      <Button size="sm" variant={item.isVerified ? "primary" : "outline"} onClick={() => toggleStatus(item, "isVerified")}>
                        {item.isVerified ? "Doğrulandı" : "Doğrula"}
                      </Button>
                    </td>
                    <td className="py-2">
                      <Button size="sm" variant={item.isPublished ? "primary" : "outline"} onClick={() => toggleStatus(item, "isPublished")}>
                        {item.isPublished ? "Yayında" : "Yayınla"}
                      </Button>
                    </td>
                    <td className="py-2">
                      <Button size="sm" variant="secondary" onClick={() => remove(item.id)}>Sil</Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>
    </div>
  );
}
