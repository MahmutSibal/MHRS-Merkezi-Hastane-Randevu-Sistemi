import type { Metadata } from "next";
import { Inter, Geist_Mono } from "next/font/google";
import { ToastProvider } from "@/components/session/ToastProvider";
import { LoadingProvider } from "@/components/session/LoadingProvider";
import "./globals.css";

const inter = Inter({
  variable: "--font-inter",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "MHRS - Merkezi Hastane Randevu Sistemi",
  description: "Merkezi Hastane Randevu Sistemi (MHRS) - yönetim, doktor ve hasta portalları.",
  icons: {
    icon: "/favicon.svg",
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="tr">
      <body className={`${inter.variable} ${geistMono.variable} antialiased`}>
        <LoadingProvider>
          <ToastProvider>
            {children}
          </ToastProvider>
        </LoadingProvider>
      </body>
    </html>
  );
}
