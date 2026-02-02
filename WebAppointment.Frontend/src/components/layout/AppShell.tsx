"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useSession } from "@/components/session/useSession";
import { Button } from "@/components/ui/button";
import { CalendarIcon, DepartmentIcon, DoctorIcon, PatientIcon, AppointmentIcon, ReportIcon } from "@/components/ui/icons";
import { Logo } from "@/components/ui/logo";

function NavLink({ href, label, icon: Icon }: { href: string; label: string; icon?: React.ComponentType<React.SVGProps<SVGSVGElement>> }) {
  const pathname = usePathname();
  const active = pathname === href || pathname.startsWith(href + "/");
  return (
    <Link
      href={href}
      className={
        "group relative flex items-center gap-3 rounded-xl px-4 py-2.5 text-sm font-medium transition-all " +
        (active
          ? "active-accent bg-blue-50 text-blue-700 shadow-sm dark:bg-blue-950 dark:text-blue-200"
          : "text-slate-700 hover:bg-slate-100 dark:text-slate-200 dark:hover:bg-slate-700")
      }
    >
      {Icon ? (
        <Icon className={"h-5 w-5 " + (active ? "text-blue-600" : "text-slate-500 group-hover:text-slate-700 dark:text-slate-400")} />
      ) : null}
      <span className="truncate">{label}</span>
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
    <div className="min-h-dvh bg-[var(--background)] dark:bg-slate-950">
      <header className="sticky top-0 z-50 glass supports-[backdrop-filter]:bg-white/70 dark:supports-[backdrop-filter]:bg-slate-900/70">
        <div className="mx-auto flex w-full max-w-6xl items-center justify-between gap-4 px-4 py-3 sm:px-6 lg:px-8">
          <Link href="/app" className="transition hover:opacity-85">
            <Logo />
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
        <aside className="rounded-2xl border border-slate-200 bg-white p-6 soft-shadow backdrop-blur dark:border-slate-800 dark:bg-slate-800/90 lg:w-64 lg:flex-shrink-0 lg:sticky lg:top-20 lg:max-h-[calc(100vh-100px)]">
          <nav className="space-y-1">
            <NavLink href="/app" label="Kontrol Paneli" icon={CalendarIcon} />

            {role === "Admin" ? (
              <>
                <div className="mt-6 pt-5 border-t border-slate-200 dark:border-slate-700">
                  <h3 className="px-4 py-2 text-xs font-bold uppercase tracking-widest text-slate-600 dark:text-slate-300">Yönetim</h3>
                  <div className="mt-2 space-y-1">
                    <NavLink href="/admin/hospitals" label="Hastaneler" icon={DepartmentIcon} />
                  </div>
                </div>
              </>
            ) : null}

            {role === "HospitalAdmin" ? (
              <>
                <div className="mt-6 pt-5 border-t border-slate-200 dark:border-slate-700">
                  <h3 className="px-4 py-2 text-xs font-bold uppercase tracking-widest text-slate-600 dark:text-slate-300">Hastane Yönetimi</h3>
                  <div className="mt-2 space-y-1">
                    <NavLink href="/hospital/departments" label="Bölümler" icon={DepartmentIcon} />
                    <NavLink href="/hospital/doctors" label="Doktorlar" icon={DoctorIcon} />
                    <NavLink href="/hospital/patients" label="Hastalar" icon={PatientIcon} />
                    <NavLink href="/hospital/appointments" label="Randevular" icon={AppointmentIcon} />
                    <NavLink href="/hospital/reports" label="Raporlar" icon={ReportIcon} />
                  </div>
                </div>
              </>
            ) : null}

            

            {role === "Doctor" ? (
              <>
                <div className="mt-6 pt-5 border-t border-slate-200 dark:border-slate-700">
                  <h3 className="px-4 py-2 text-xs font-bold uppercase tracking-widest text-slate-600 dark:text-slate-300">Doktor</h3>
                  <div className="mt-2 space-y-1">
                    <NavLink href="/doctor" label="Profil" icon={DoctorIcon} />
                    <NavLink href="/doctor/appointments" label="Randevularım" icon={AppointmentIcon} />
                    <NavLink href="/doctor/calendar" label="Takvim" icon={CalendarIcon} />
                  </div>
                </div>
              </>
            ) : null}

            {role === "Patient" ? (
              <>
                <div className="mt-6 pt-5 border-t border-slate-200 dark:border-slate-700">
                  <h3 className="px-4 py-2 text-xs font-bold uppercase tracking-widest text-slate-600 dark:text-slate-300">Hasta</h3>
                  <div className="mt-2 space-y-1">
                    <NavLink href="/patient" label="Profil" icon={PatientIcon} />
                    <NavLink href="/patient/appointments" label="Randevularım" icon={AppointmentIcon} />
                    <NavLink href="/patient/appointments/new" label="Yeni Randevu" icon={AppointmentIcon} />
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
