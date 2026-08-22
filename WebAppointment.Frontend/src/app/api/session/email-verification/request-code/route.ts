import { NextResponse } from "next/server";
import { getBackendOrigin } from "@/lib/backend";

export async function POST(req: Request) {
  const origin = getBackendOrigin();
  const body = await req.text();

  const res = await fetch(`${origin}/api/auth/email-verification/request-code`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body,
    cache: "no-store",
  });

  if (!res.ok) {
    const text = await res.text();
    return new NextResponse(text, { status: res.status });
  }

  return new NextResponse(null, { status: 204 });
}

export async function GET() {
  return new NextResponse("Doğrulama kodu için POST isteği kullanın.", {
    status: 405,
    headers: { "Content-Type": "text/plain; charset=utf-8" },
  });
}
