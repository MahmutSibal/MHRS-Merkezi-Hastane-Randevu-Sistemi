export function PageHeader({ title, subtitle }: { title: string; subtitle?: string }) {
  return (
    <div className="space-y-2 border-b border-slate-200 pb-6 dark:border-slate-800">
      <h1 className="text-3xl font-bold tracking-tight text-slate-900 dark:text-white sm:text-4xl">{title}</h1>
      {subtitle ? <p className="text-base text-slate-600 dark:text-slate-400">{subtitle}</p> : null}
    </div>
  );
}
