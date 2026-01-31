import { PageHeader } from "@/components/ui/page-header";
import Link from "next/link";

const adminCards = [
  { title: "Bölümler", desc: "Hastane bölümlerini listele, ekle, güncelle", href: "/admin/departments" },
  { title: "Doktorlar", desc: "Doktor kayıtlarını ve bölüm atamalarını yönet", href: "/admin/doctors" },
  { title: "Hastalar", desc: "Hasta kayıtları ve detaylarını yönet", href: "/admin/patients" },
  { title: "Randevular", desc: "Tüm sistem randevularını görüntüle ve yönet", href: "/admin/appointments" },
  { title: "Raporlar", desc: "İstatistik ve analiz raporlarını görüntüle", href: "/admin/reports" },
];

export default function AdminHomePage() {
  return (
    <div className="space-y-8">
      <PageHeader 
        title="Admin Yönetim Paneli" 
        subtitle="Hastane operasyonlarını yönetin ve analiz edebilirsiniz." 
      />

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {adminCards.map((card) => (
          <Link 
            key={card.href}
            className="group rounded-lg border border-slate-200 bg-white p-6 shadow-sm transition-all hover:shadow-md hover:border-blue-300 dark:border-slate-700 dark:bg-slate-800 dark:hover:border-blue-600" 
            href={card.href}
          >
            <div className="text-lg font-semibold text-slate-900 group-hover:text-blue-600 dark:text-white dark:group-hover:text-blue-400">{card.title}</div>
            <div className="mt-2 text-sm text-slate-600 dark:text-slate-400">{card.desc}</div>
            <div className="mt-4 inline-flex items-center text-sm font-medium text-blue-600 group-hover:translate-x-1 transition-transform dark:text-blue-400">
              Aç →
            </div>
          </Link>
        ))}
      </div>
    </div>
  );
}
