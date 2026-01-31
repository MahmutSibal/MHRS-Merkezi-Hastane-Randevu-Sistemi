"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useSession } from "@/components/session/useSession";
import { Button } from "@/components/ui/button";

function NavLink({ href, label }: { href: string; label: string }) {
  const pathname = usePathname();
  const active = pathname === href || pathname.startsWith(href + "/");
  return (
    <Link
      href={href}
      className={
        "block rounded-md px-4 py-2.5 text-sm font-medium transition-colors " +
        (active 
          ? "bg-blue-600 text-white dark:bg-blue-500" 
          : "text-slate-700 hover:bg-slate-100 dark:text-slate-200 dark:hover:bg-slate-700")
      }
    >
      {label}
    </Link>
  );
}

export function AppShell({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const { session, isLoading } = useSession();

  async function logout() {
    await fetch("/api/session/logout", { method: "POST" });
    router.replace("/login");
  }

  const role = session?.role ?? "";

  return (
    <div className="min-h-dvh bg-slate-50 dark:bg-slate-950">
      <header className="border-b border-slate-200 bg-white dark:border-slate-800 dark:bg-slate-900">
        <div className="mx-auto flex w-full max-w-6xl items-center justify-between gap-4 px-4 py-4 sm:px-6 lg:px-8">
          <Link href="/app" className="flex items-center gap-2 transition hover:opacity-80">
            <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-gradient-to-br from-blue-600 to-blue-700 shadow-md">
              <div className="h-6 w-6 rounded-md bg-white/90" />
            </div>
            <div className="text-sm font-bold text-slate-900 dark:text-white">MHRS</div>
          </Link>

          <div className="flex flex-col items-end gap-1 min-w-0 sm:flex-row sm:items-center sm:gap-3">
            <div className="truncate text-xs sm:text-sm text-slate-600 dark:text-slate-400">
              {isLoading ? "Yükleniyor…" : session?.email}
              {role && <span className="ml-1 text-slate-500 dark:text-slate-500">({role})</span>}
            </div>
            <Button variant="secondary" onClick={logout} size="sm">
              Çıkış
            </Button>
          </div>
        </div>
      </header>

      <div className="mx-auto flex w-full max-w-6xl flex-col gap-6 px-4 py-6 sm:px-6 lg:gap-8 lg:flex-row lg:px-8">
        <aside className="rounded-lg border border-slate-200 bg-white p-6 shadow-sm dark:border-slate-800 dark:bg-slate-800 lg:w-64 lg:flex-shrink-0 lg:sticky lg:top-20 lg:max-h-[calc(100vh-100px)]">
          <nav className="space-y-1">
            <NavLink href="/app" label="Kontrol Paneli" />

            {role === "Admin" ? (
              <>
                <div className="mt-6 pt-5 border-t border-slate-300 dark:border-slate-700">
                  <h3 className="px-4 py-2 text-xs font-bold uppercase tracking-widest text-slate-700 dark:text-slate-300">Yönetim</h3>
                  <div className="mt-2 space-y-1">
                    <NavLink href="/admin/departments" label="Bölümler" />
                    <NavLink href="/admin/doctors" label="Doktorlar" />
                    <NavLink href="/admin/patients" label="Hastalar" />
                    <NavLink href="/admin/appointments" label="Randevular" />
                    <NavLink href="/admin/reports" label="Raporlar" />
                  </div>
                </div>
              </>
            ) : null}

            {role === "Doctor" ? (
              <>
                <div className="mt-6 pt-5 border-t border-slate-300 dark:border-slate-700">
                  <h3 className="px-4 py-2 text-xs font-bold uppercase tracking-widest text-slate-700 dark:text-slate-300">Doktor</h3>
                  <div className="mt-2 space-y-1">
                    <NavLink href="/doctor" label="Profil" />
                    <NavLink href="/doctor/appointments" label="Randevularım" />
                    <NavLink href="/doctor/calendar" label="Takvim" />
                  </div>
                </div>
              </>
            ) : null}

            {role === "Patient" ? (
              <>
                <div className="mt-6 pt-5 border-t border-slate-300 dark:border-slate-700">
                  <h3 className="px-4 py-2 text-xs font-bold uppercase tracking-widest text-slate-700 dark:text-slate-300">Hasta</h3>
                  <div className="mt-2 space-y-1">
                    <NavLink href="/patient" label="Profil" />
                    <NavLink href="/patient/appointments" label="Randevularım" />
                    <NavLink href="/patient/appointments/new" label="Yeni Randevu" />
                  </div>
                </div>
              </>
            ) : null}
          </nav>
        </aside>

        <main className="min-w-0 flex-1">{children}</main>
      </div>
    </div>
  );
}
