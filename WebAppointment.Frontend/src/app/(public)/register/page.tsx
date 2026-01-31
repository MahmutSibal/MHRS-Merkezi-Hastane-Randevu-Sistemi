"use client";

import { useState } from "react";
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
    email: "",
    password: "",
    tcKimlikNo: "",
    firstName: "",
    lastName: "",
    phone: "",
  });
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  function update<K extends keyof typeof form>(key: K, value: (typeof form)[K]) {
    setForm((p) => ({ ...p, [key]: value }));
  }

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setIsLoading(true);

    try {
      const res = await fetch("/api/session/register", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(form),
      });

      if (!res.ok) {
        const text = await res.text();
        throw new Error(text || "Kayıt başarısız.");
      }

      toast.success("Başarıyla kayıt oldunuz");
      router.replace("/patient");
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : "Kayıt başarısız.";
      setError(errorMsg);
      toast.error(errorMsg);
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <div className="mx-auto w-full max-w-2xl">
      <Panel title="Hasta Kaydı Oluştur" description="Yeni bir hasta hesabı oluşturun ve sisteme katılın.">
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
          </div>

          <div className="space-y-4">
            <h3 className="text-sm font-semibold text-slate-900 dark:text-white">Hesap Bilgileri</h3>
            <Input 
              label="E-posta Adresi" 
              type="email" 
              value={form.email} 
              onChange={(e) => update("email", e.target.value)} 
              placeholder="ornek@example.com"
              required 
            />
            <Input 
              label="Şifre" 
              type="password" 
              value={form.password} 
              onChange={(e) => update("password", e.target.value)} 
              placeholder="En az 6 karakter"
              required 
            />
          </div>

          {error ? (
            <div className="rounded-lg border border-red-200 bg-red-50 p-4 text-sm font-medium text-red-700 dark:border-red-900 dark:bg-red-950 dark:text-red-200">
              {error}
            </div>
          ) : null}

          <Button type="submit" isLoading={isLoading} size="md" className="w-full">
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
              <Link className="font-semibold text-blue-600 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300" href="/login">
                Giriş Yap
              </Link>
            </p>
          </div>
        </form>
      </Panel>
    </div>
  );
}
