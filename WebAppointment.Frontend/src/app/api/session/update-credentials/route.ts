import { cookies } from "next/headers";
import { NextResponse } from "next/server";
import { getBackendOrigin } from "@/lib/backend";

const ACCESS_COOKIE = "mhrs_at";
const REFRESH_COOKIE = "mhrs_rt";

async function refreshSession(origin: string, refreshToken: string) {
  const res = await fetch(`${origin}/api/auth/refresh`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ refreshToken }),
    cache: "no-store",
  });

  if (!res.ok) return null;
  return (await res.json()) as { accessToken: string; refreshToken: string };
}

export async function PATCH(req: Request) {
  const origin = getBackendOrigin();
  const cookieStore = await cookies();

  const accessToken = cookieStore.get(ACCESS_COOKIE)?.value;
  const refreshToken = cookieStore.get(REFRESH_COOKIE)?.value;

  const body = await req.text();

  const envTenant = process.env.TENANT_ID ?? process.env.NEXT_PUBLIC_TENANT_ID;

  async function callBackend(token?: string) {
    return await fetch(`${origin}/api/auth/me/credentials`, {
      method: "PATCH",
      headers: {
        "Content-Type": "application/json",
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
        ...(envTenant ? { "X-Tenant-Id": String(envTenant) } : {}),
      },
      body,
      cache: "no-store",
    });
  }

  let res = await callBackend(accessToken);

  if (res.status === 401 && refreshToken) {
    const refreshed = await refreshSession(origin, refreshToken);
    if (refreshed?.accessToken && refreshed?.refreshToken) {
      cookieStore.set(ACCESS_COOKIE, refreshed.accessToken, {
        httpOnly: true,
        sameSite: "lax",
        secure: process.env.NODE_ENV === "production",
        path: "/",
      });
      cookieStore.set(REFRESH_COOKIE, refreshed.refreshToken, {
        httpOnly: true,
        sameSite: "lax",
        secure: process.env.NODE_ENV === "production",
        path: "/",
      });

      res = await callBackend(refreshed.accessToken);
    }
  }

  const text = await res.text();
  if (!res.ok) {
    return new NextResponse(text, { status: res.status });
  }

  const data = JSON.parse(text) as {
    accessToken: string;
    refreshToken: string;
    userId: string;
    email: string;
    role: string;
    accessTokenExpiresAtUtc: string;
  };

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

  return NextResponse.json(data);
}
