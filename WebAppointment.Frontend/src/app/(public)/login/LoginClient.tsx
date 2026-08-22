"use client";

import { useEffect, useMemo, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Panel } from "@/components/ui/panel";
import { useToast } from "@/components/session/ToastProvider";
import { RecaptchaWidget } from "@/components/auth/RecaptchaWidget";
import { GoogleSignInButton } from "@/components/auth/GoogleSignInButton";

type LoginResponse = {
  accessToken: string;
  refreshToken: string;
  userId: string;
  email: string;
  role: "Admin" | "Doctor" | "Patient" | string;
  accessTokenExpiresAtUtc: string;
};

function formatCountdown(seconds: number) {
  const mins = Math.floor(seconds / 60);
  const secs = seconds % 60;
  return `${String(mins).padStart(2, "0")}:${String(secs).padStart(2, "0")}`;
}

export default function LoginClient() {
  const router = useRouter();
  const toast = useToast();
  const searchParams = useSearchParams();

  const [step, setStep] = useState<"login" | "verifyEmail">("login");

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [recaptchaToken, setRecaptchaToken] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const [isRequestingCode, setIsRequestingCode] = useState(false);
  const [isVerifying, setIsVerifying] = useState(false);
  const [verificationCode, setVerificationCode] = useState("");
  const [verificationError, setVerificationError] = useState<string | null>(null);
  const [expiresAt, setExpiresAt] = useState<number | null>(null);
  const [remainingSeconds, setRemainingSeconds] = useState(0);

  const next = useMemo(() => searchParams.get("next"), [searchParams]);

  useEffect(() => {
    if (!expiresAt) return;
    const id = setInterval(() => {
      const remaining = Math.max(0, Math.ceil((expiresAt - Date.now()) / 1000));
      setRemainingSeconds(remaining);
      if (remaining === 0) {
        clearInterval(id);
      }
    }, 500);

    return () => clearInterval(id);
  }, [expiresAt]);

  function startCountdown() {
    const windowMs = 5 * 60 * 1000;
    setExpiresAt(Date.now() + windowMs);
    setRemainingSeconds(Math.ceil(windowMs / 1000));
  }

  function redirectAfterLogin(data: LoginResponse) {
    if (next) {
      router.replace(next);
      return;
    }

    if (data.role === "Admin") router.replace("/admin");
    else if (data.role === "HospitalAdmin") router.replace("/hospital/departments");
    else if (data.role === "Doctor") router.replace("/doctor");
    else router.replace("/patient");
  }

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setIsLoading(true);

    try {
      const res = await fetch("/api/session/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, password, recaptchaToken }),
      });

      if (!res.ok) {
        const text = await res.text();
        throw new Error(text || "Giriş başarısız.");
      }

      const data = (await res.json()) as LoginResponse;
      toast.success("Başarıyla giriş yaptınız");
      redirectAfterLogin(data);
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : "Giriş başarısız.";
      if (errorMsg === "EMAIL_NOT_VERIFIED") {
        toast.info("Hesabınız için bir doğrulama kodu e-postanıza gönderildi.");
        startCountdown();
        setStep("verifyEmail");
        return;
      }
      setError(errorMsg);
      toast.error(errorMsg);
    } finally {
      setIsLoading(false);
    }
  }

  async function onResendCode() {
    setIsRequestingCode(true);
    setVerificationError(null);
    try {
      const res = await fetch("/api/session/email-verification/request-code", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, password }),
      });

      if (!res.ok) {
        const text = await res.text();
        throw new Error(text || "Doğrulama kodu gönderilemedi.");
      }

      startCountdown();
      setVerificationCode("");
      toast.success("Doğrulama kodu tekrar gönderildi.");
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : "Doğrulama kodu gönderilemedi.";
      setVerificationError(errorMsg);
      toast.error(errorMsg);
    } finally {
      setIsRequestingCode(false);
    }
  }

  async function onConfirmCode(e: React.FormEvent) {
    e.preventDefault();
    setVerificationError(null);
    setIsVerifying(true);

    try {
      const res = await fetch("/api/session/email-verification/confirm", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, password, code: verificationCode }),
      });

      if (!res.ok) {
        const text = await res.text();
        throw new Error(text || "Doğrulama başarısız.");
      }

      const data = (await res.json()) as LoginResponse;
      toast.success("E-posta doğrulandı, giriş yapıldı.");
      redirectAfterLogin(data);
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : "Doğrulama başarısız.";
      setVerificationError(errorMsg);
      toast.error(errorMsg);
    } finally {
      setIsVerifying(false);
    }
  }

  async function onGoogleCredential(idToken: string) {
    setError(null);
    setIsLoading(true);
    try {
      const res = await fetch("/api/session/google-login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ idToken }),
      });

      if (!res.ok) {
        const text = await res.text();
        throw new Error(text || "Google ile giriş başarısız.");
      }

      const data = (await res.json()) as LoginResponse;
      toast.success("Başarıyla giriş yaptınız");
      redirectAfterLogin(data);
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : "Google ile giriş başarısız.";
      setError(errorMsg);
      toast.error(errorMsg);
    } finally {
      setIsLoading(false);
    }
  }

  const canConfirmCode = verificationCode.length === 6 && remainingSeconds > 0;

  if (step === "verifyEmail") {
    return (
      <div className="mx-auto w-full max-w-md">
        <Panel title="E-posta Doğrulama" description="Hesabınıza gönderdiğimiz 6 haneli kodu girin.">
          <form className="space-y-5" onSubmit={onConfirmCode}>
            <div className="rounded-lg border border-slate-200 bg-slate-50 p-4 dark:border-slate-800 dark:bg-slate-900">
              <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                <div className="text-sm text-slate-600 dark:text-slate-300">Kod 5 dakika geçerlidir.</div>
                <div className="text-sm font-semibold text-slate-900 dark:text-white">
                  Süre: {formatCountdown(remainingSeconds)}
                </div>
              </div>
              <div className="mt-3 flex flex-col gap-3 sm:flex-row">
                <Button
                  type="button"
                  variant="outline"
                  onClick={onResendCode}
                  disabled={isRequestingCode}
                  isLoading={isRequestingCode}
                  className="sm:w-52"
                >
                  Kodu Tekrar Gönder
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

            <Button type="submit" isLoading={isVerifying} disabled={!canConfirmCode} size="md" className="w-full">
              Doğrula ve Giriş Yap
            </Button>

            <Button type="button" variant="secondary" size="md" className="w-full" onClick={() => setStep("login")}>
              Geri Dön
            </Button>
          </form>
        </Panel>
      </div>
    );
  }

  return (
    <div className="mx-auto w-full max-w-md">
      <Panel title="Giriş Yap" description="Hesabınla oturum aç ve yönetim paneline erişin.">
        <form className="space-y-5" onSubmit={onSubmit}>
          <Input
            label="E-posta Adresi"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="ornek@hastane.com"
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

          <RecaptchaWidget onChange={setRecaptchaToken} />

          {error ? (
            <div className="rounded-lg border border-red-200 bg-red-50 p-3 text-sm font-medium text-red-700 dark:border-red-900 dark:bg-red-950 dark:text-red-200">
              {error}
            </div>
          ) : null}

          <Button type="submit" isLoading={isLoading} disabled={!recaptchaToken} size="md" className="w-full">
            Giriş Yap
          </Button>

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

          <GoogleSignInButton onCredential={onGoogleCredential} />

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
