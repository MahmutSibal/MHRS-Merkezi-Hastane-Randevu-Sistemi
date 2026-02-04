"use client";

import type { ButtonHTMLAttributes } from "react";
import { cn } from "@/lib/cn";
import { Spinner } from "@/components/ui/spinner";

type Variant = "primary" | "secondary" | "danger" | "outline" | "ghost";
type Size = "sm" | "md" | "lg";

export function Button({
  children,
  className,
  variant = "primary",
  size = "md",
  isLoading,
  disabled,
  ...props
}: ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: Variant;
  size?: Size;
  isLoading?: boolean;
}) {
  const base = "inline-flex items-center justify-center gap-2 font-medium transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:ring-[var(--ring)]";

  const sizes = 
    size === "sm" ? "h-9 rounded-lg px-3 text-sm" 
    : size === "lg" ? "h-12 rounded-lg px-6 text-base"
    : "h-10 rounded-lg px-4 text-sm";

  const styles =
    variant === "primary"
      ? "bg-gradient-to-r from-blue-600 to-blue-700 text-white shadow-md hover:shadow-lg hover:from-blue-700 hover:to-blue-800 active:scale-95 focus-visible:shadow-lg"
      : variant === "danger"
        ? "bg-gradient-to-r from-red-500 to-red-600 text-white shadow-md hover:shadow-lg hover:from-red-600 hover:to-red-700 active:scale-95 focus-visible:shadow-lg"
        : variant === "outline"
        ? "border-2 border-blue-600 text-blue-600 hover:bg-blue-50 dark:hover:bg-blue-950 active:scale-95 focus-visible:shadow-lg"
        : variant === "ghost"
        ? "bg-transparent text-slate-700 hover:bg-slate-100 dark:text-slate-200 dark:hover:bg-slate-800 active:scale-95 focus-visible:shadow-lg"
        : "border border-slate-300 bg-white/90 text-slate-900 shadow-sm backdrop-blur hover:bg-white dark:border-slate-600 dark:bg-slate-800/90 dark:text-slate-100 dark:hover:bg-slate-700 active:scale-95 focus-visible:shadow-lg";

  return (
    <button
      className={cn(base, sizes, styles, className)}
      disabled={disabled || isLoading}
      {...props}
    >
      {isLoading ? (
        <>
          <Spinner className="opacity-90" />
          <span>İşleniyor…</span>
        </>
      ) : (
        children
      )}
    </button>
  );
}
