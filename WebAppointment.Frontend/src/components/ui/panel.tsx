export function Panel({
  title,
  description,
  children,
}: {
  title: string;
  description?: string;
  children: React.ReactNode;
}) {
  return (
    <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-md transition-all duration-300 hover:shadow-lg dark:border-slate-700 dark:bg-slate-800 sm:p-8">
      <div className="mb-6">
        <h1 className="text-2xl font-bold tracking-tight text-slate-900 dark:text-slate-100 sm:text-3xl">{title}</h1>
        {description ? <p className="mt-2 text-sm text-slate-600 dark:text-slate-400">{description}</p> : null}
      </div>
      {children}
    </div>
  );
}

