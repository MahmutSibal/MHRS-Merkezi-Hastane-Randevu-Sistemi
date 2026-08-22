import { redirect } from "next/navigation";
import Link from "next/link";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import Hero from "@/components/layout/Hero";
import { getServerSession } from "@/lib/server-session";

export default async function HomePage() {
  const session = await getServerSession();
  if (session) redirect("/app");

  return (
    <div className="space-y-12">
      {/* Hero section with provided image */}
      <Hero />

      <section className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
        <Card 
          title="Hasta Portalı" 
          description="Kolay ve hızlı bir şekilde doktor randevusu oluşturun. Mevcut slotları görüntüleyin, randevularınızı yönetin."
          className="lg:hover:scale-105"
        >
          <Link href="/login/patient">
            <Button size="md">Hasta Girişine Git</Button>
          </Link>
        </Card>
        <Card 
          title="Doktor Paneli" 
          description="Günlük çalışma slotlarınızı yönetin. Gelen randevuları onayla, işaretleme ve raporlama yapın."
          className="lg:hover:scale-105"
        >
          <Link href="/login/doctor">
            <Button size="md" variant="outline">Doktor Girişine Git</Button>
          </Link>
        </Card>
        <Card 
          title="Yönetici Paneli" 
          description="Hastane bölümleri, doktor ve hasta kayıtlarını yönetin. Kapsamlı raporlar ve istatistikler görüntüleyin."
          className="lg:hover:scale-105"
        >
          <Link href="/login/admin">
            <Button size="md" variant="secondary">Yönetim Girişine Git</Button>
          </Link>
        </Card>
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
