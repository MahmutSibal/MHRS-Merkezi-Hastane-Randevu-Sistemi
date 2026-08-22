import { PublicNav } from "@/components/layout/PublicNav";
import { PageSlideWrapper } from "@/components/layout/PageSlideWrapper";
import { getIsSmaEnabled } from "@/lib/sma-settings";

export default async function PublicLayout({ children }: { children: React.ReactNode }) {
  const isSmaEnabled = await getIsSmaEnabled();

  return (
    <div className="min-h-dvh bg-gradient-to-b from-blue-50 via-white to-slate-50 dark:from-slate-950 dark:via-slate-900 dark:to-slate-950">
      <PublicNav isSmaEnabled={isSmaEnabled} />

      <main className="mx-auto w-full max-w-6xl overflow-x-hidden px-4 py-8 sm:px-6 lg:px-8 lg:py-12">
        <PageSlideWrapper>{children}</PageSlideWrapper>
      </main>

      <footer className="border-t border-slate-200 bg-white/50 backdrop-blur dark:border-slate-800 dark:bg-slate-900/50">
        <div className="mx-auto w-full max-w-6xl px-4 py-8 text-sm text-slate-600 sm:px-6 dark:text-slate-400 lg:px-8">
          <span className="font-medium">© {new Date().getFullYear()} MHRS - Merkezi Hastane Randevu Sistemi</span>
        </div>
      </footer>

      {/* Asistan, kök layout'ta küresel olarak eklenmiştir */}
    </div>
  );
}
