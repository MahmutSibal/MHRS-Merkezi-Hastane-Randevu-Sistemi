import Link from "next/link";
import { PublicNav } from "@/components/layout/PublicNav";

export default function PublicLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="min-h-dvh bg-gradient-to-b from-blue-50 via-white to-slate-50 dark:from-slate-950 dark:via-slate-900 dark:to-slate-950">
      <PublicNav />

      <main className="mx-auto w-full max-w-6xl px-4 py-8 sm:px-6 lg:px-8 lg:py-12">
        {children}
      </main>

      <footer className="border-t border-slate-200 bg-white/50 backdrop-blur dark:border-slate-800 dark:bg-slate-900/50">
        <div className="mx-auto flex w-full max-w-6xl flex-col gap-4 px-4 py-8 text-sm text-slate-600 sm:flex-row sm:items-center sm:justify-between sm:px-6 dark:text-slate-400 lg:px-8">
          <span className="font-medium">© {new Date().getFullYear()} MHRS - Merkezi Hastane Randevu Sistemi</span>
          <div className="flex gap-6">
            <Link className="transition hover:text-slate-900 dark:hover:text-slate-100" href="/login">
              Giriş
            </Link>
            <Link className="transition hover:text-slate-900 dark:hover:text-slate-100" href="/register">
              Kayıt Ol
            </Link>
          </div>
        </div>
      </footer>
    </div>
  );
}
