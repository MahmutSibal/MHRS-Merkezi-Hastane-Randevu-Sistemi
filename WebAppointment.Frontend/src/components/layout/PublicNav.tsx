import Link from "next/link";
import { Logo } from "@/components/ui/logo";

export function PublicNav() {
  return (
    <header className="sticky top-0 z-50 glass supports-[backdrop-filter]:bg-white/70 dark:supports-[backdrop-filter]:bg-slate-900/70">
      <div className="mx-auto flex w-full max-w-6xl items-center justify-between px-4 py-3 sm:px-6 lg:px-8">
        <Link href="/" className="transition hover:opacity-85">
          <Logo />
        </Link>

        <nav className="flex items-center gap-2 sm:gap-3">
          <Link 
            className="rounded-lg px-3 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-100 dark:text-slate-300 dark:hover:bg-slate-800" 
            href="/login"
          >
            Giriş
          </Link>
          <Link 
            className="rounded-lg bg-gradient-to-r from-blue-600 to-blue-700 px-4 py-2 text-sm font-semibold text-white shadow-md transition hover:shadow-lg hover:from-blue-700 hover:to-blue-800 active:scale-95" 
            href="/register"
          >
            Kayıt Ol
          </Link>
        </nav>
      </div>
    </header>
  );
}
