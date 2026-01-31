import { Suspense } from "react";
import LoginClient from "./LoginClient";

export default function LoginPage() {
  return (
    <Suspense fallback={<div className="text-sm text-zinc-600">Yükleniyor…</div>}>
      <LoginClient />
    </Suspense>
  );
}
