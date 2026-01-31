import Link from "next/link";
import { Card } from "@/components/ui/card";

export default function HomePage() {
  return (
    <div className="space-y-12">
      <section className="space-y-8">
        <div className="inline-flex items-center gap-2 rounded-full border border-slate-300 bg-white px-4 py-2 text-xs font-medium text-slate-700 shadow-sm dark:border-slate-700 dark:bg-slate-800 dark:text-slate-300">
          <span className="flex h-2 w-2 rounded-full bg-blue-600"></span>
          <span>MHRS - Merkezi Hastane Randevu Sistemi</span>
        </div>

        <div className="space-y-4">
          <h1 className="text-4xl font-bold tracking-tight text-slate-900 dark:text-white sm:text-5xl lg:text-6xl">
            Sağlık Hizmetlerini
            <span className="block bg-gradient-to-r from-blue-600 to-blue-700 bg-clip-text py-2 text-transparent">
              Kolayca Yönetiniz
            </span>
          </h1>

          <p className="max-w-2xl text-lg leading-8 text-slate-600 dark:text-slate-300">
            Hasta, doktor ve yönetici panelleriyle; randevu oluşturma, onaylama ve raporlama işlemlerinin tümü tek bir sistemde. Modern, hızlı ve güvenli.
          </p>
        </div>

        <div className="flex flex-col gap-3 sm:flex-row">
          <Link
            className="inline-flex h-12 items-center justify-center rounded-lg bg-gradient-to-r from-blue-600 to-blue-700 px-6 text-base font-semibold text-white shadow-lg transition-all hover:shadow-xl hover:from-blue-700 hover:to-blue-800 active:scale-95 sm:h-11 sm:text-sm"
            href="/login"
          >
            Giriş Yap
          </Link>
          <Link
            className="inline-flex h-12 items-center justify-center rounded-lg border-2 border-slate-300 bg-white px-6 text-base font-semibold text-slate-900 shadow-md transition-all hover:bg-slate-50 dark:border-slate-700 dark:bg-slate-800 dark:text-slate-100 dark:hover:bg-slate-700 active:scale-95 sm:h-11 sm:text-sm"
            href="/register"
          >
            Hasta Kaydı Oluştur
          </Link>
        </div>
      </section>

      <section className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
        <Card 
          title="Hasta Portalı" 
          description="Kolay ve hızlı bir şekilde doktor randevusu oluşturun. Mevcut slotları görüntüleyin, randevularınızı yönetin."
          className="lg:hover:scale-105"
        />
        <Card 
          title="Doktor Paneli" 
          description="Günlük çalışma slotlarınızı yönetin. Gelen randevuları onayla, işaretleme ve raporlama yapın."
          className="lg:hover:scale-105"
        />
        <Card 
          title="Yönetici Paneli" 
          description="Hastane bölümleri, doktor ve hasta kayıtlarını yönetin. Kapsamlı raporlar ve istatistikler görüntüleyin."
          className="lg:hover:scale-105"
        />
      </section>

      <section className="grid gap-6 rounded-xl border border-slate-200 bg-white p-8 shadow-md dark:border-slate-700 dark:bg-slate-800">
        <div>
          <h2 className="text-2xl font-bold text-slate-900 dark:text-white">Neden MHRS?</h2>
          <p className="mt-2 text-slate-600 dark:text-slate-300">Modern teknoloji ile sağlık hizmetlerini yönetimi basitleştirildi</p>
        </div>
        
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {[
            { title: "Hızlı", desc: "Sistem anında yanıt verir ve işlemlerinizi tamamlar" },
            { title: "Güvenli", desc: "Bütün verileriniz şifreli ve korunuyor" },
            { title: "Responsive", desc: "Mobil, tablet ve masaüstünde mükemmel çalışır" },
            { title: "Erişilebilir", desc: "Herkes için kullanışlı arayüz tasarımı" },
            { title: "Dark Mode", desc: "Karanlık modu ile rahat görüş sağlanıyor" },
            { title: "Raporlar", desc: "Detaylı analiz ve istatistik raporları" },
          ].map((item, i) => (
            <div key={i} className="rounded-lg border border-slate-200 p-4 dark:border-slate-700">
              <p className="font-semibold text-slate-900 dark:text-white">{item.title}</p>
              <p className="mt-1 text-sm text-slate-600 dark:text-slate-400">{item.desc}</p>
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}
