"use client";

import Link from "next/link";

export default function AdminDoctorsPage() {
  return (
    <div className="grid gap-6">
      <div className="rounded-xl border border-slate-200 bg-white p-6 dark:border-slate-800 dark:bg-slate-800/90">
        <h1 className="text-lg font-semibold">Bu sayfa kaldırıldı</h1>
        <p className="mt-2 text-sm text-slate-600 dark:text-slate-300">
          Doktor yönetimi artık yalnızca <strong>HospitalAdmin</strong> rolünde yapılmaktadır.
        </p>
        <div className="mt-4">
          <Link href="/app" className="text-sm font-medium text-blue-600 hover:underline">
            Kontrol Paneli'ne dön
          </Link>
        </div>
      </div>
    </div>
  );
}
