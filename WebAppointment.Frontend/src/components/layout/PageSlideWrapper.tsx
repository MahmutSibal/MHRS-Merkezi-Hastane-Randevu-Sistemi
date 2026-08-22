"use client";

import { useEffect, useRef } from "react";
import { usePathname } from "next/navigation";

export function PageSlideWrapper({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const el = ref.current;
    if (!el) return;

    const clearDirection = () => delete document.documentElement.dataset.transitionDirection;
    el.addEventListener("animationend", clearDirection);
    return () => el.removeEventListener("animationend", clearDirection);
  }, [pathname]);

  // key={pathname} forces a remount on every route change, which re-triggers the
  // CSS "slide in" animation defined in globals.css (page-slide-in).
  return (
    <div key={pathname} ref={ref} className="page-slide-in">
      {children}
    </div>
  );
}
