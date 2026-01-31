import type { ReactNode } from "react";

export function Card({
  title,
  description,
  children,
  className,
}: {
  title?: string;
  description?: string;
  children?: ReactNode;
  className?: string;
}) {
  return (
    <div className={`rounded-xl border border-slate-200 bg-white p-6 shadow-sm transition-all hover:shadow-md dark:border-slate-700 dark:bg-slate-800 ${className || ""}`}>
      {title ? <div className="text-base font-semibold text-slate-900 dark:text-slate-100">{title}</div> : null}
      {description ? <div className="mt-2 text-sm leading-6 text-slate-600 dark:text-slate-400">{description}</div> : null}
      {children ? <div className={(title || description) ? "mt-5" : ""}>{children}</div> : null}
    </div>
  );
}

