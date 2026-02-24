import Link from "next/link";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";

export default function LoginPage() {
  return (
    <div className="space-y-6">
      <div className="rounded-xl border border-slate-200 bg-white/70 p-3 text-center text-xs font-medium text-slate-700 shadow-sm glass dark:border-slate-800 dark:bg-slate-900/70 dark:text-slate-200">
        MHRS — Giriş Portalı Seçimi
      </div>
      <div className="relative overflow-hidden rounded-2xl">
        <div className="absolute inset-0 hero-gradient" />
        <div className="absolute inset-0 hero-mesh opacity-30" />
        <div className="relative mx-auto w-full max-w-3xl p-6 sm:p-8">
        <section className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          <Card 
            title="Hasta Portalı" 
            description="TC Kimlik No ve şifre ile giriş yapın."
            className="lg:hover:scale-105"
          >
            <Link href="/login/patient">
              <Button size="md">Hasta Girişi</Button>
            </Link>
          </Card>
          <Card 
            title="Doktor Paneli" 
            description="E-posta ve şifre ile giriş yapın."
            className="lg:hover:scale-105"
          >
            <Link href="/login/doctor">
              <Button size="md" variant="outline">Doktor Girişi</Button>
            </Link>
          </Card>
          <Card 
            title="Yönetim Paneli" 
            description="E-posta ve şifre ile giriş yapın."
            className="lg:hover:scale-105"
          >
            <Link href="/login/admin">
              <Button size="md" variant="secondary">Yönetim Girişi</Button>
            </Link>
          </Card>
        </section>
        </div>
      </div>
    </div>
  );
}
