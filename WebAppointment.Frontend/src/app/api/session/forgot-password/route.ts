import { NextResponse } from "next/server";
import { getBackendOrigin } from "@/lib/backend";

export async function POST(req: Request) {
  const origin = getBackendOrigin();
  const body = await req.text();

  const res = await fetch(`${origin}/api/auth/patient/forgot-password`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body,
    cache: "no-store",
  });

  const text = await res.text();
  if (!res.ok) {
    return new NextResponse(text, { status: res.status });
  }

  return new NextResponse(text || "OK", {
    status: 200,
    headers: { "Content-Type": "text/plain; charset=utf-8" },
  });
}

export async function GET() {
  return new NextResponse("Sifre sifirlama icin POST istegi kullanin.", {
    status: 405,
    headers: { "Content-Type": "text/plain; charset=utf-8" },
  });
}