import { PageHeader } from "@/components/ui/page-header";
import Link from "next/link";
import { DepartmentIcon } from "@/components/ui/icons";

const adminCards = [
  { title: "Hastaneler", desc: "Hastaneleri görüntüle ve yönet", href: "/admin/hospitals", icon: DepartmentIcon },
];

export default function AdminHomePage() {
  return (
    <div className="space-y-8">
      <PageHeader 
        title="Admin Yönetim Paneli" 
        subtitle="Sadece hastane yönetimi yetkileri aktif." 
      />

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {adminCards.map((card) => {
          const Icon = card.icon;
          return (
            <Link 
              key={card.href}
              className="group rounded-xl border border-slate-200 bg-white/90 p-6 shadow-sm backdrop-blur transition-all hover:shadow-lg hover:border-blue-300 dark:border-slate-700 dark:bg-slate-800/90 dark:hover:border-blue-600" 
              href={card.href}
            >
              <div className="flex items-center gap-3">
                <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-blue-600/10 text-blue-600 dark:bg-blue-400/10 dark:text-blue-400">
                  <Icon className="size-6" />
                </div>
                <div className="text-lg font-semibold text-slate-900 group-hover:text-blue-600 dark:text-white dark:group-hover:text-blue-400">{card.title}</div>
              </div>
              <div className="mt-2 text-sm text-slate-600 dark:text-slate-400">{card.desc}</div>
              <div className="mt-4 inline-flex items-center text-sm font-medium text-blue-600 group-hover:translate-x-1 transition-transform dark:text-blue-400">
                Aç →
              </div>
            </Link>
          );
        })}
      </div>
    </div>
  );
}
