"use client";

import { usePathname, useRouter } from "next/navigation";

const TABS = [
  { href: "/", label: "MHRS", isActive: (path: string) => !path.startsWith("/sma") },
  { href: "/sma", label: "SMA Bağış", isActive: (path: string) => path.startsWith("/sma") },
];

const HIDDEN_ON_PREFIXES = ["/login", "/register", "/forgot-password"];

export function SiteSwitchTabs() {
  const pathname = usePathname();
  const router = useRouter();

  if (HIDDEN_ON_PREFIXES.some((prefix) => pathname.startsWith(prefix))) {
    return null;
  }

  function navigate(href: string) {
    if (pathname === href) return;

    // "SMA Bağış" is the right-hand tab: going there slides forward (new content from the
    // right); coming back to "MHRS" reverses it (new content from the left). The direction
    // is read by a plain CSS animation on the incoming page (see PageSlideWrapper) — the
    // native View Transitions API turned out to be unreliable with Next.js App Router
    // navigation (intermittently timed out waiting for the DOM update).
    const direction = href === "/sma" ? "forward" : "back";
    // eslint-disable-next-line react-hooks/immutability -- imperative browser API, not React state
    document.documentElement.dataset.transitionDirection = direction;
    router.push(href);
  }

  return (
    <div className="flex rounded-full border border-slate-200 bg-slate-100/80 p-1 dark:border-slate-700 dark:bg-slate-800/80">
      {TABS.map((tab) => {
        const active = tab.isActive(pathname);
        return (
          <button
            key={tab.href}
            type="button"
            onClick={() => navigate(tab.href)}
            aria-current={active ? "page" : undefined}
            className={
              "rounded-full px-4 py-1.5 text-sm font-semibold transition " +
              (active
                ? "bg-white text-blue-700 shadow-sm dark:bg-slate-900 dark:text-blue-300"
                : "text-slate-600 hover:text-slate-900 dark:text-slate-300 dark:hover:text-white")
            }
          >
            {tab.label}
          </button>
        );
      })}
    </div>
  );
}
