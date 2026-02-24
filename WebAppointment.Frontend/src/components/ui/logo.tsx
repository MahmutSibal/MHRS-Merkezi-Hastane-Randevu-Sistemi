import type { SVGProps } from "react";

export function LogoMark({ className, ...props }: SVGProps<SVGSVGElement>) {
  return (
    <svg
      viewBox="0 0 24 24"
      fill="none"
      strokeLinecap="round"
      strokeLinejoin="round"
      className={className}
      aria-hidden="true"
      {...props}
    >
      {/* Shield outline */}
      <path
        d="M12 2L4 6v5c0 5.55 3.84 10.74 8 12 4.16-1.26 8-6.45 8-12V6L12 2z"
        stroke="currentColor"
        strokeWidth={1.6}
        fill="currentColor"
        fillOpacity={0.08}
      />
      {/* Heartbeat pulse inside shield */}
      <path
        d="M7 12h2l1.5-3 2 7 2-5.5 1.5 1.5H18"
        stroke="currentColor"
        strokeWidth={1.8}
        fill="none"
      />
      {/* Small medical cross */}
      <line x1="12" y1="5.5" x2="12" y2="8.5" stroke="currentColor" strokeWidth={1.4} opacity={0.5} />
      <line x1="10.5" y1="7" x2="13.5" y2="7" stroke="currentColor" strokeWidth={1.4} opacity={0.5} />
    </svg>
  );
}

export function Logo({ className }: { className?: string }) {
  return (
    <div className={"flex items-center gap-3 " + (className || "")}>
      <div className="relative flex h-10 w-10 items-center justify-center rounded-xl bg-gradient-to-br from-blue-500 to-blue-700 text-white shadow-md shadow-blue-500/25">
        <LogoMark className="h-5 w-5" />
      </div>
      <div>
        <div className="text-sm font-bold tracking-wide text-slate-900 dark:text-white">MHRS</div>
        <div className="hidden text-[11px] text-slate-500 dark:text-slate-400 sm:block">Merkezi Randevu Sistemi</div>
      </div>
    </div>
  );
}
