import { cookies } from "next/headers";
import { NextResponse } from "next/server";
import { getBackendOrigin } from "@/lib/backend";

const ACCESS_COOKIE = "mhrs_at";
const REFRESH_COOKIE = "mhrs_rt";

export async function POST(req: Request) {
  const origin = getBackendOrigin();
  const body = await req.text();

  const res = await fetch(`${origin}/api/auth/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body,
    cache: "no-store",
  });

  const text = await res.text();
  if (!res.ok) {
    return new NextResponse(text, { status: res.status });
  }

  const data = JSON.parse(text) as { accessToken: string; refreshToken: string };

  const cookieStore = await cookies();
  cookieStore.set(ACCESS_COOKIE, data.accessToken, {
    httpOnly: true,
    sameSite: "lax",
    secure: process.env.NODE_ENV === "production",
    path: "/",
  });
  cookieStore.set(REFRESH_COOKIE, data.refreshToken, {
    httpOnly: true,
    sameSite: "lax",
    secure: process.env.NODE_ENV === "production",
    path: "/",
  });

  return new NextResponse(text, {
    status: 200,
    headers: { "Content-Type": "application/json" },
  });
}
