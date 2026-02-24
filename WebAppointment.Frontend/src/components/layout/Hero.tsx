"use client";

import Image from "next/image";
import Link from "next/link";

export default function Hero() {
  return (
    <section className="relative overflow-hidden rounded-2xl border border-slate-200 shadow-lg dark:border-slate-800">
      {/* Gradient background */}
      <div className="absolute inset-0 hero-gradient" />
      {/* Animated mesh pattern */}
      <div className="absolute inset-0 hero-mesh opacity-30" />

      {/* Content */}
      <div className="relative z-10 grid gap-6 p-6 sm:p-10 lg:p-12 md:grid-cols-2">
        <div className="max-w-xl">
          <div className="inline-flex items-center gap-2 rounded-full bg-white/90 px-4 py-2 text-xs font-medium text-slate-800 shadow-sm glass dark:bg-slate-900/70 dark:text-slate-100">
            <span className="flex h-2 w-2 rounded-full bg-[var(--brand-accent)]" />
            <span>MHRS — Merkezi Hastane Randevu Sistemi</span>
          </div>

          <h1 className="mt-5 text-4xl font-bold tracking-tight text-white sm:text-5xl lg:text-6xl">
            Sağlık Hizmetlerini Modern ve Hızlı Yönetin
          </h1>
          <p className="mt-3 max-w-lg text-lg leading-8 text-slate-200">
            Hasta, doktor ve yönetici panelleriyle; randevu oluşturma, onaylama ve raporlama işlemlerinin tümü tek bir sistemde.
          </p>

          <div className="mt-6 flex flex-col gap-3 sm:flex-row">
            <Link
              className="inline-flex h-12 items-center justify-center rounded-lg bg-gradient-to-r from-blue-600 to-blue-700 px-6 text-base font-semibold text-white shadow-lg transition-all hover:shadow-xl hover:from-blue-700 hover:to-blue-800 active:scale-95 sm:h-11 sm:text-sm"
              href="/login"
            >
              Giriş Yap
            </Link>
            <Link
              className="inline-flex h-12 items-center justify-center rounded-lg border-2 border-white/80 bg-white/10 px-6 text-base font-semibold text-white backdrop-blur transition-all hover:bg-white/20 active:scale-95 sm:h-11 sm:text-sm"
              href="/register"
            >
              Kayıt Ol
            </Link>
          </div>

          <div className="mt-6 flex items-center gap-3 text-xs text-slate-200">
            <span className="rounded-md bg-white/10 px-2 py-1">.NET 8</span>
            <span className="rounded-md bg-white/10 px-2 py-1">C#</span>
            <span className="rounded-md bg-white/10 px-2 py-1">React</span>
            <span className="rounded-md bg-white/10 px-2 py-1">Next.js</span>
          </div>
        </div>

        <div className="relative hidden md:block">
          <div className="absolute -right-6 -bottom-6 h-48 w-48 rounded-full bg-[var(--brand-accent)]/20 blur-2xl" />
          <div className="absolute -right-8 -top-8 h-64 w-64 rounded-full bg-[var(--brand-primary)]/20 blur-2xl" />
          <div className="relative mx-auto max-w-md rounded-xl bg-white/10 p-3 shadow-2xl backdrop-blur-sm border border-white/20">
            <Image src="/mhrs-hero.svg" alt="MHRS Dashboard" width={640} height={400} className="h-auto w-full rounded-lg" priority />
          </div>
        </div>
      </div>
    </section>
  );
}
