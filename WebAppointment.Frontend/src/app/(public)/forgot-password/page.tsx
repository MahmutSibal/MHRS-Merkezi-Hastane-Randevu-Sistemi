"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Panel } from "@/components/ui/panel";
import { useToast } from "@/components/session/ToastProvider";

export default function ForgotPasswordPage() {
  const router = useRouter();
  const toast = useToast();
  const [form, setForm] = useState({
    firstName: "",
    lastName: "",
    tcKimlikNo: "",
    phone: "",
  });
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);

  function update<K extends keyof typeof form>(key: K, value: (typeof form)[K]) {
    setForm((p) => ({ ...p, [key]: value }));
  }

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      const res = await fetch("/api/session/forgot-password", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(form),
      });

      if (!res.ok) {
        const text = await res.text();
        throw new Error(text || "Sifre sifirlama basarisiz.");
      }

      setIsSuccess(true);
      toast.success("Yeni sifreniz WhatsApp ile gonderildi.");
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : "Sifre sifirlama basarisiz.";
      setError(errorMsg);
      toast.error(errorMsg);
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="space-y-6">
      <div className="rounded-xl border border-slate-200 bg-white/70 p-3 text-center text-xs font-medium text-slate-700 shadow-sm glass dark:border-slate-800 dark:bg-slate-900/70 dark:text-slate-200">
        MHRS — Sifre Sifirlama
      </div>
      <div className="relative overflow-hidden rounded-2xl">
        <div className="absolute inset-0 hero-bg" />
        <div className="absolute inset-0 light-hero-overlay md:hero-overlay" />
        <div className="relative mx-auto w-full max-w-xl p-6 sm:p-8">
          <Panel
            title="Sifremi Unuttum"
            description="Ad, soyad, TC ve telefon bilgileriniz dogrulaninca yeni sifreniz WhatsApp uzerinden iletilir."
          >
            {isSuccess ? (
              <div className="space-y-4">
                <div className="rounded-lg border border-emerald-200 bg-emerald-50 p-4 text-sm font-medium text-emerald-800 dark:border-emerald-900 dark:bg-emerald-950 dark:text-emerald-200">
                  Yeni sifreniz WhatsApp ile gonderildi. Guvenliginiz icin giris yaptiktan sonra sifrenizi degistirin.
                </div>
                <Button onClick={() => router.push("/login/patient")} size="md" className="w-full">
                  Hasta Girisine Don
                </Button>
              </div>
            ) : (
              <form className="space-y-5" onSubmit={onSubmit}>
                <div className="grid gap-4 sm:grid-cols-2">
                  <Input
                    label="Adiniz"
                    value={form.firstName}
                    onChange={(e) => update("firstName", e.target.value)}
                    placeholder="Ahmet"
                    required
                  />
                  <Input
                    label="Soyadiniz"
                    value={form.lastName}
                    onChange={(e) => update("lastName", e.target.value)}
                    placeholder="Yilmaz"
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
                    label="Telefon Numarasi"
                    value={form.phone}
                    onChange={(e) => update("phone", e.target.value)}
                    placeholder="5551234567"
                    required
                  />
                </div>

                {error ? (
                  <div className="rounded-lg border border-red-200 bg-red-50 p-3 text-sm font-medium text-red-700 dark:border-red-900 dark:bg-red-950 dark:text-red-200">
                    {error}
                  </div>
                ) : null}

                <Button type="submit" isLoading={isSubmitting} size="md" className="w-full">
                  Yeni Sifre Gonder
                </Button>

                <div className="relative my-5">
                  <div className="absolute inset-0 flex items-center">
                    <div className="w-full border-t border-slate-300 dark:border-slate-700" />
                  </div>
                  <div className="relative flex justify-center text-sm">
                    <span className="bg-white px-2 text-slate-500 dark:bg-slate-800 dark:text-slate-400">
                      giris ekranina don
                    </span>
                  </div>
                </div>

                <div className="rounded-lg border-2 border-dashed border-slate-300 bg-slate-50 p-4 text-center dark:border-slate-700 dark:bg-slate-900">
                  <p className="text-sm text-slate-600 dark:text-slate-400">
                    <Link className="font-semibold text-blue-600 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300" href="/login/patient">
                      Hasta Girisi
                    </Link>
                  </p>
                </div>
              </form>
            )}
          </Panel>
        </div>
      </div>
    </div>
  );
}