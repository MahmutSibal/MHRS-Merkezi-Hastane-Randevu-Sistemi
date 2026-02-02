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
    <div className={`rounded-2xl border border-slate-200 bg-white p-6 soft-shadow backdrop-blur transition-all hover:soft-shadow-lg hover:translate-y-[-1px] dark:border-slate-700 dark:bg-slate-800/90 ${className || ""}`}>
      {title ? <div className="text-lg font-semibold text-slate-900 dark:text-slate-100">{title}</div> : null}
      {description ? <div className="mt-2 text-sm leading-6 text-slate-600 dark:text-slate-400">{description}</div> : null}
      {children ? <div className={(title || description) ? "mt-5" : ""}>{children}</div> : null}
    </div>
  );
}

