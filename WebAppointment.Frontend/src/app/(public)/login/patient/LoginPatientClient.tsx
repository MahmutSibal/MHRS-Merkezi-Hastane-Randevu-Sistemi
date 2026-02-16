"use client";

import { useMemo, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Panel } from "@/components/ui/panel";
import { useToast } from "@/components/session/ToastProvider";

export default function LoginPatientClient() {
  const router = useRouter();
  const toast = useToast();
  const searchParams = useSearchParams();

  const [tcKimlikNo, setTcKimlikNo] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const next = useMemo(() => searchParams.get("next"), [searchParams]);

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setIsLoading(true);

    try {
      const res = await fetch("/api/session/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ tcKimlikNo, password }),
      });

      if (!res.ok) {
        const text = await res.text();
        throw new Error(text || "Giriş başarısız.");
      }

      toast.success("Başarıyla giriş yaptınız");
      if (next) {
        router.replace(next);
        return;
      }
      router.replace("/patient");
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : "Giriş başarısız.";
      setError(errorMsg);
      toast.error(errorMsg);
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <div className="mx-auto w-full max-w-md">
      <Panel title="Hasta Girişi" description="TC Kimlik Numaran ve şifren ile giriş yap.">
        <form className="space-y-5" onSubmit={onSubmit}>
          <Input 
            label="TC Kimlik No" 
            value={tcKimlikNo} 
            onChange={(e) => setTcKimlikNo(e.target.value)} 
            placeholder="12345678901"
            required 
          />
          <Input 
            label="Şifre" 
            type="password" 
            value={password} 
            onChange={(e) => setPassword(e.target.value)} 
            placeholder="••••••••"
            required 
          />

          {error ? (
            <div className="rounded-lg border border-red-200 bg-red-50 p-3 text-sm font-medium text-red-700 dark:border-red-900 dark:bg-red-950 dark:text-red-200">
              {error}
            </div>
          ) : null}

          <Button type="submit" isLoading={isLoading} size="md" className="w-full">
            Giriş Yap
          </Button>

          <div className="text-center text-sm">
            <Link className="font-semibold text-blue-600 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300" href="/forgot-password">
              Sifremi Unuttum
            </Link>
          </div>

          <div className="relative my-5">
            <div className="absolute inset-0 flex items-center">
              <div className="w-full border-t border-slate-300 dark:border-slate-700" />
            </div>
            <div className="relative flex justify-center text-sm">
              <span className="bg-white px-2 text-slate-500 dark:bg-slate-800 dark:text-slate-400">
                ya da
              </span>
            </div>
          </div>

          <div className="rounded-lg border-2 border-dashed border-slate-300 bg-slate-50 p-4 text-center dark:border-slate-700 dark:bg-slate-900">
            <p className="text-sm text-slate-600 dark:text-slate-400">
              Henüz hesabın yok mu?{" "}
              <Link className="font-semibold text-blue-600 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300" href="/register">
                Kayıt Ol
              </Link>
            </p>
          </div>
        </form>
      </Panel>
    </div>
  );
}
