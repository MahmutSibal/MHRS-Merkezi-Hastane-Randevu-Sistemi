"use client";

import { cn } from "@/lib/cn";

interface LoadingSpinnerProps {
  size?: "sm" | "md" | "lg";
  fullscreen?: boolean;
  message?: string;
}

export function LoadingSpinner({ size = "md", fullscreen = false, message }: LoadingSpinnerProps) {
  const sizeClasses = {
    sm: "h-6 w-6",
    md: "h-10 w-10",
    lg: "h-16 w-16",
  };

  const spinner = (
    <div className={cn("relative", sizeClasses[size])}>
      {/* Track */}
      <span className="absolute inset-0 rounded-full border-4 border-slate-200 dark:border-slate-700" aria-hidden="true" />
      {/* Animated ring */}
      <span
        className="absolute inset-0 rounded-full animate-spin"
        style={{
          background: "conic-gradient(var(--brand-primary), transparent 40%)",
          WebkitMask: "radial-gradient(farthest-side, transparent calc(100% - 6px), black 0)",
          mask: "radial-gradient(farthest-side, transparent calc(100% - 6px), black 0)",
        }}
        aria-hidden="true"
      />
      {/* Center dot */}
      <span className="absolute left-1/2 top-1/2 size-1 -translate-x-1/2 -translate-y-1/2 rounded-full bg-[var(--brand-primary)]" aria-hidden="true" />
      <span className="sr-only">Yükleniyor</span>
    </div>
  );

  if (fullscreen) {
    return (
      <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur">
        <div className="flex flex-col items-center gap-4 rounded-xl bg-white/90 p-8 shadow-xl dark:bg-slate-800/90">
          {spinner}
          {message && (
            <p className="text-sm font-medium text-slate-600 dark:text-slate-300">{message}</p>
          )}
        </div>
      </div>
    );
  }

  return (
    <div className="flex flex-col items-center gap-2">
      {spinner}
      {message && (
        <p className="text-sm text-slate-600 dark:text-slate-400">{message}</p>
      )}
    </div>
  );
}
