import type { SVGProps } from "react";

export function LogoMark({ className, ...props }: SVGProps<SVGSVGElement>) {
  return (
    <svg
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth={1.8}
      strokeLinecap="round"
      strokeLinejoin="round"
      className={className}
      aria-hidden="true"
      {...props}
    >
      <circle cx="12" cy="12" r="9" />
      <path d="M4 12h3l2-3 2 6 2-4 2 2h3" />
    </svg>
  );
}

export function Logo({ className }: { className?: string }) {
  return (
    <div className={"flex items-center gap-3 " + (className || "")}> 
      <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-blue-600 text-white shadow-md">
        <LogoMark className="h-5 w-5" />
      </div>
      <div>
        <div className="text-sm font-bold text-slate-900 dark:text-white">MHRS</div>
        <div className="hidden text-[11px] text-slate-500 dark:text-slate-400 sm:block">Merkezi Randevu Sistemi</div>
      </div>
    </div>
  );
}
