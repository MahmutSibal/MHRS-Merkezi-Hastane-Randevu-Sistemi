import { Suspense } from "react";
import LoginPatientClient from "./LoginPatientClient";

export default function PatientLoginPage() {
  return (
    <div className="space-y-6">
      <div className="rounded-xl border border-slate-200 bg-white/70 p-3 text-center text-xs font-medium text-slate-700 shadow-sm glass dark:border-slate-800 dark:bg-slate-900/70 dark:text-slate-200">
        MHRS — Hasta Girişi
      </div>
      <div className="relative overflow-hidden rounded-2xl">
        <div className="absolute inset-0 hero-gradient" />
        <div className="absolute inset-0 hero-mesh opacity-30" />
        <div className="relative mx-auto w-full max-w-md p-6 sm:p-8">
          <Suspense fallback={<div className="text-sm text-slate-200">Yükleniyor…</div>}>
            <LoginPatientClient />
          </Suspense>
        </div>
      </div>
    </div>
  );
}
