import { cookies } from "next/headers";
import { NextResponse } from "next/server";
import { getBackendOrigin } from "@/lib/backend";

const ACCESS_COOKIE = "mhrs_at";
const REFRESH_COOKIE = "mhrs_rt";

export async function POST() {
  const origin = getBackendOrigin();
  const cookieStore = await cookies();
  const refreshToken = cookieStore.get(REFRESH_COOKIE)?.value;

  if (refreshToken) {
    await fetch(`${origin}/api/auth/logout`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ refreshToken }),
      cache: "no-store",
    });
  }

  cookieStore.delete(ACCESS_COOKIE);
  cookieStore.delete(REFRESH_COOKIE);

  return NextResponse.json({ ok: true });
}
