import { cn } from "@/lib/cn";

export function StatusBadge({ status, className }: { status: string; className?: string }) {
  const s = (status || "").toLowerCase();
  const style =
    s.includes("complete")
      ? "bg-emerald-50 text-emerald-700 border-emerald-200 dark:bg-emerald-950 dark:text-emerald-200 dark:border-emerald-900"
      : s.includes("approve")
      ? "bg-blue-50 text-blue-700 border-blue-200 dark:bg-blue-950 dark:text-blue-200 dark:border-blue-900"
      : s.includes("cancel")
      ? "bg-red-50 text-red-700 border-red-200 dark:bg-red-950 dark:text-red-200 dark:border-red-900"
      : "bg-slate-50 text-slate-700 border-slate-200 dark:bg-slate-900 dark:text-slate-200 dark:border-slate-700";

  const label = status;

  return (
    <span
      className={cn(
        "inline-flex items-center gap-1 rounded-full border px-2.5 py-1 text-xs font-semibold",
        style,
        className,
      )}
    >
      <span className="size-1 rounded-full bg-current opacity-60" />
      {label}
    </span>
  );
}
