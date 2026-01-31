"use client";

import type { InputHTMLAttributes } from "react";
import { cn } from "@/lib/cn";

export function Input({
  label,
  hint,
  error,
  className,
  ...props
}: InputHTMLAttributes<HTMLInputElement> & {
  label?: string;
  hint?: string;
  error?: string;
}) {
  const describedById = props.id ? `${props.id}-help` : undefined;
  return (
    <label className="block">
      {label ? (
        <span className="mb-2 block text-sm font-medium text-slate-700 dark:text-slate-300">{label}</span>
      ) : null}
      <input
        aria-invalid={error ? "true" : "false"}
        aria-describedby={hint || error ? describedById : undefined}
        className={cn(
          "h-10 w-full rounded-lg border-2 border-slate-200 bg-white px-3 py-2 text-sm outline-none transition-all duration-200 placeholder:text-slate-400 focus:border-blue-500 focus:ring-2 focus:ring-blue-200 dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100 dark:placeholder:text-slate-500 dark:focus:border-blue-400 dark:focus:ring-blue-900",
          error ? "border-red-500 focus:border-red-500 focus:ring-red-200 dark:focus:ring-red-900" : "",
          className,
        )}
        {...props}
      />
      {hint || error ? (
        <span id={describedById} className={cn("mt-1.5 block text-xs font-medium", error ? "text-red-600 dark:text-red-400" : "text-slate-500 dark:text-slate-400")}>
          {error ?? hint}
        </span>
      ) : null}
    </label>
  );
}
