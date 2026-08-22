"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Panel } from "@/components/ui/panel";
import { useToast } from "@/components/session/ToastProvider";

export default function RegisterPage() {
  const router = useRouter();
  const toast = useToast();
  const [form, setForm] = useState({
    password: "",
    tcKimlikNo: "",
    firstName: "",
    lastName: "",
    phone: "",
    birthDay: "",
    birthMonth: "",
    birthYear: "",
  });
  const [error, setError] = useState<string | null>(null);
  const [isRequestingCode, setIsRequestingCode] = useState(false);
  const [isVerifying, setIsVerifying] = useState(false);
  const [verificationCode, setVerificationCode] = useState("");
  const [verificationError, setVerificationError] = useState<string | null>(null);
  const [expiresAt, setExpiresAt] = useState<number | null>(null);
  const [remainingSeconds, setRemainingSeconds] = useState(0);

  function update<K extends keyof typeof form>(key: K, value: (typeof form)[K]) {
    setForm((p) => ({ ...p, [key]: value }));
  }

  useEffect(() => {
    if (!expiresAt) return;
    const id = setInterval(() => {
      const next = Math.max(0, Math.ceil((expiresAt - Date.now()) / 1000));
      setRemainingSeconds(next);
      if (next === 0) {
        clearInterval(id);
      }
    }, 500);

    return () => clearInterval(id);
  }, [expiresAt]);

  function startCountdown() {
    const next = Date.now() + 5 * 60 * 1000;
    setExpiresAt(next);
    setRemainingSeconds(90);
  }

  function formatCountdown(seconds: number) {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${String(mins).padStart(2, "0")}:${String(secs).padStart(2, "0")}`;
  }

  async function requestVerificationCode() {
    setIsRequestingCode(true);
    setError(null);
    try {
      const res = await fetch("/api/session/register/request-code", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ phone: form.phone }),
      });

      if (!res.ok) {
        const text = await res.text();
        throw new Error(text || "Dogrulama kodu gonderilemedi.");
      }

      startCountdown();
      setVerificationCode("");
      setVerificationError(null);
      toast.success("Dogrulama kodu gonderildi.");
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : "Dogrulama kodu gonderilemedi.";
      setError(errorMsg);
      toast.error(errorMsg);
    } finally {
      setIsRequestingCode(false);
    }
  }

  async function verifyAndRegister() {
    setVerificationError(null);
    setIsVerifying(true);

    try {
      const { birthDay, birthMonth, birthYear, ...rest } = form;
      const birthDate = `${birthYear}-${birthMonth.padStart(2, "0")}-${birthDay.padStart(2, "0")}`;
      const res = await fetch("/api/session/register", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ ...rest, birthDate, code: verificationCode }),
      });

      if (!res.ok) {
        const text = await res.text();
        throw new Error(text || "Kayıt başarısız.");
      }

      toast.success("Basariyla kayit oldunuz");
      router.replace("/patient");
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : "Kayıt başarısız.";
      setVerificationError(errorMsg);
      toast.error(errorMsg);
    } finally {
      setIsVerifying(false);
    }
  }

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    await verifyAndRegister();
  }

  const canSubmit = verificationCode.length === 6 && remainingSeconds > 0;

  return (
    <>
      <div className="space-y-6">
        <div className="rounded-xl border border-slate-200 bg-white/70 p-3 text-center text-xs font-medium text-slate-700 shadow-sm glass dark:border-slate-800 dark:bg-slate-900/70 dark:text-slate-200">
          MHRS — Kayıt işlemleriniz güvenle saklanır
        </div>
        <div className="relative overflow-hidden rounded-2xl">
          <div className="absolute inset-0 hero-gradient" />
          <div className="absolute inset-0 hero-mesh opacity-30" />
          <div className="relative mx-auto w-full max-w-2xl p-6 sm:p-8">
            <Panel title="Hasta Kaydı Oluştur" description="Yeni bir hasta hesabı oluşturun ve sisteme katılın.">
              <div className="space-y-6">
                <form className="space-y-6" onSubmit={onSubmit}>
                    <div className="rounded-lg bg-blue-50 p-4 dark:bg-blue-950">
                      <p className="text-sm text-blue-800 dark:text-blue-200">
                        Lütfen doğru bilgileri girin. Bu bilgiler tıbbi kayıtlarınızda kullanılacaktır.
                      </p>
                    </div>

                    <div className="space-y-4">
                      <h3 className="text-sm font-semibold text-slate-900 dark:text-white">Kişisel Bilgiler</h3>
                      <div className="grid gap-4 sm:grid-cols-2">
                        <Input
                          label="Adınız"
                          value={form.firstName}
                          onChange={(e) => update("firstName", e.target.value)}
                          placeholder="Ahmet"
                          required
                        />
                        <Input
                          label="Soyadınız"
                          value={form.lastName}
                          onChange={(e) => update("lastName", e.target.value)}
                          placeholder="Yılmaz"
                          required
                        />
                      </div>

                      <div className="grid gap-4 sm:grid-cols-2">
                        <Input
                          label="TC Kimlik No"
                          value={form.tcKimlikNo}
                          onChange={(e) => update("tcKimlikNo", e.target.value)}
                          placeholder="12345678901"
                          required
                        />
                        <Input
                          label="Telefon Numarası"
                          value={form.phone}
                          onChange={(e) => update("phone", e.target.value)}
                          placeholder="5551234567"
                          required
                        />
                      </div>

                      <div>
                        <label className="mb-1.5 block text-sm font-medium text-slate-700 dark:text-slate-300">Doğum Tarihi</label>
                        <div className="grid grid-cols-3 gap-3">
                          <select
                            className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 dark:border-slate-700 dark:bg-slate-900 dark:text-white"
                            value={form.birthDay}
                            onChange={(e) => update("birthDay", e.target.value)}
                            required
                          >
                            <option value="">Gün</option>
                            {Array.from({ length: 31 }, (_, i) => i + 1).map((d) => (
                              <option key={d} value={String(d)}>{d}</option>
                            ))}
                          </select>
                          <select
                            className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 dark:border-slate-700 dark:bg-slate-900 dark:text-white"
                            value={form.birthMonth}
                            onChange={(e) => update("birthMonth", e.target.value)}
                            required
                          >
                            <option value="">Ay</option>
                            {[
                              "Ocak", "\u015eubat", "Mart", "Nisan", "May\u0131s", "Haziran",
                              "Temmuz", "A\u011fustos", "Eyl\u00fcl", "Ekim", "Kas\u0131m", "Aral\u0131k"
                            ].map((name, i) => (
                              <option key={i + 1} value={String(i + 1)}>{name}</option>
                            ))}
                          </select>
                          <select
                            className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500 dark:border-slate-700 dark:bg-slate-900 dark:text-white"
                            value={form.birthYear}
                            onChange={(e) => update("birthYear", e.target.value)}
                            required
                          >
                            <option value="">Yıl</option>
                            {Array.from({ length: 100 }, (_, i) => new Date().getFullYear() - i).map((y) => (
                              <option key={y} value={String(y)}>{y}</option>
                            ))}
                          </select>
                        </div>
                      </div>
                    </div>

                    <div className="space-y-4">
                      <h3 className="text-sm font-semibold text-slate-900 dark:text-white">Hesap Bilgileri</h3>
                      <Input
                        label="Şifre"
                        type="password"
                        value={form.password}
                        onChange={(e) => update("password", e.target.value)}
                        placeholder="En az 6 karakter"
                        required
                      />
                    </div>

                    <div className="space-y-4">
                      <h3 className="text-sm font-semibold text-slate-900 dark:text-white">Telefon Doğrulama</h3>
                      <div className="rounded-lg border border-slate-200 bg-slate-50 p-4 dark:border-slate-800 dark:bg-slate-900">
                        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                          <div className="text-sm text-slate-600 dark:text-slate-300">
                            Kod 5 dakika geçerlidir.
                          </div>
                          <div className="text-sm font-semibold text-slate-900 dark:text-white">
                            Süre: {formatCountdown(remainingSeconds)}
                          </div>
                        </div>
                        <div className="mt-3 flex flex-col gap-3 sm:flex-row">
                          <Button
                            type="button"
                            variant="outline"
                            onClick={requestVerificationCode}
                            disabled={isRequestingCode || !form.phone}
                            isLoading={isRequestingCode}
                            className="sm:w-52"
                          >
                            Doğrulama Kodu Gönder
                          </Button>
                          <div className="flex-1">
                            <Input
                              label="6 haneli kod"
                              value={verificationCode}
                              onChange={(e) => setVerificationCode(e.target.value.replace(/\D/g, "").slice(0, 6))}
                              placeholder="123456"
                              inputMode="numeric"
                              autoComplete="one-time-code"
                            />
                          </div>
                        </div>
                        {verificationError ? (
                          <div className="mt-3 rounded-lg border border-red-200 bg-red-50 p-3 text-sm font-medium text-red-700 dark:border-red-900 dark:bg-red-950 dark:text-red-200">
                            {verificationError}
                          </div>
                        ) : null}
                      </div>
                    </div>

                    {error ? (
                      <div className="rounded-lg border border-red-200 bg-red-50 p-4 text-sm font-medium text-red-700 dark:border-red-900 dark:bg-red-950 dark:text-red-200">
                        {error}
                      </div>
                    ) : null}

                    <Button
                      type="submit"
                      isLoading={isVerifying}
                      size="md"
                      className="w-full"
                      disabled={!canSubmit}
                    >
                      Kaydı Tamamla
                    </Button>

                    <div className="relative my-5">
                      <div className="absolute inset-0 flex items-center">
                        <div className="w-full border-t border-slate-300 dark:border-slate-700" />
                      </div>
                      <div className="relative flex justify-center text-sm">
                        <span className="bg-white px-2 text-slate-500 dark:bg-slate-800 dark:text-slate-400">
                          zaten üye misiniz?
                        </span>
                      </div>
                    </div>

                    <div className="rounded-lg border-2 border-dashed border-slate-300 bg-slate-50 p-4 text-center dark:border-slate-700 dark:bg-slate-900">
                      <p className="text-sm text-slate-600 dark:text-slate-400">
                        <Link
                          className="font-semibold text-blue-600 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300"
                          href="/login"
                        >
                          Giriş Yap
                        </Link>
                      </p>
                    </div>
                  </form>
              </div>
            </Panel>
          </div>
        </div>
      </div>
    </>
  );
}
