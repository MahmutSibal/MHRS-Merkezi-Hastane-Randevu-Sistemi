import Link from "next/link";

export function PublicNav() {
  return (
    <header className="sticky top-0 z-50 border-b border-slate-200 bg-white/80 backdrop-blur-md supports-[backdrop-filter]:bg-white/60 dark:border-slate-800 dark:bg-slate-900/80">
      <div className="mx-auto flex w-full max-w-6xl items-center justify-between px-4 py-3 sm:px-6 lg:px-8">
        <Link href="/" className="flex items-center gap-3 transition hover:opacity-80">
          <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-gradient-to-br from-blue-600 to-blue-700 shadow-md">
            <div className="h-5 w-5 rounded-md bg-white/90" />
          </div>
          <div className="hidden sm:block">
            <div className="text-sm font-bold text-slate-900 dark:text-white">MHRS</div>
            <div className="text-xs text-slate-500 dark:text-slate-400">Hastane Randevu</div>
          </div>
        </Link>

        <nav className="flex items-center gap-2 sm:gap-3">
          <Link 
            className="rounded-lg px-3 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-100 dark:text-slate-300 dark:hover:bg-slate-800" 
            href="/login"
          >
            Giriş
          </Link>
          <Link 
            className="rounded-lg bg-gradient-to-r from-blue-600 to-blue-700 px-4 py-2 text-sm font-semibold text-white shadow-md transition hover:shadow-lg hover:from-blue-700 hover:to-blue-800" 
            href="/register"
          >
            Kayıt Ol
          </Link>
        </nav>
      </div>
    </header>
  );
}
