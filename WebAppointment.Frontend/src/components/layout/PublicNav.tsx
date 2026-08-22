"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { Logo } from "@/components/ui/logo";
import { SiteSwitchTabs } from "@/components/layout/SiteSwitchTabs";

export function PublicNav({ isSmaEnabled }: { isSmaEnabled: boolean }) {
  const pathname = usePathname();
  const isSma = pathname.startsWith("/sma");
  const hideRightNav = isSmaEnabled && isSma;

  return (
    <header className="sticky top-0 z-50 glass supports-[backdrop-filter]:bg-white/70 dark:supports-[backdrop-filter]:bg-slate-900/70">
      <div className="mx-auto grid w-full max-w-6xl grid-cols-[auto_1fr_auto] items-center gap-3 px-4 py-3 sm:px-6 lg:px-8">
        <Link href="/" className="transition hover:opacity-85">
          <Logo />
        </Link>

        <div className="flex justify-center">
          {isSmaEnabled ? <SiteSwitchTabs /> : null}
        </div>

        <nav
          className={
            "flex items-center gap-2 sm:gap-3 justify-self-end" +
            (hideRightNav ? " invisible pointer-events-none" : "")
          }
          aria-hidden={hideRightNav || undefined}
        >
          <Link
            className="rounded-lg px-3 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-100 dark:text-slate-300 dark:hover:bg-slate-800"
            href="/login"
            tabIndex={hideRightNav ? -1 : undefined}
          >
            Giriş
          </Link>
          <Link
            className="rounded-lg bg-gradient-to-r from-blue-600 to-blue-700 px-4 py-2 text-sm font-semibold text-white shadow-md transition hover:shadow-lg hover:from-blue-700 hover:to-blue-800 active:scale-95"
            href="/register"
            tabIndex={hideRightNav ? -1 : undefined}
          >
            Kayıt Ol
          </Link>
        </nav>
      </div>
    </header>
  );
}
