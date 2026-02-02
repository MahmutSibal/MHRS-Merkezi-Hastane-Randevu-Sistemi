import { cookies } from "next/headers";
import { NextResponse, type NextRequest } from "next/server";
import { getBackendOrigin } from "@/lib/backend";

// Ensure Node.js runtime to safely access process.env
export const runtime = "nodejs";

const ACCESS_COOKIE = "mhrs_at";
const REFRESH_COOKIE = "mhrs_rt";

type RouteParams = { path: string[] };

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

async function proxy(req: NextRequest, params: RouteParams) {
  const origin = getBackendOrigin();
  const cookieStore = await cookies();
  const accessToken = cookieStore.get(ACCESS_COOKIE)?.value;
  const refreshToken = cookieStore.get(REFRESH_COOKIE)?.value;

  const url = new URL(req.url);
  const backendPath = params.path.join("/");
  const backendUrl = `${origin}/api/${backendPath}${url.search}`;

  const headers = new Headers(req.headers);
  headers.delete("host");
  headers.delete("cookie");
  if (accessToken) headers.set("authorization", `Bearer ${accessToken}`);

  // Multi-tenant: forward explicit tenant id if provided via env
  const envTenant = process.env.TENANT_ID ?? process.env.NEXT_PUBLIC_TENANT_ID;
  if (envTenant && !headers.has("X-Tenant-Id")) {
    headers.set("X-Tenant-Id", String(envTenant));
  }

  const init: RequestInit = {
    method: req.method,
    headers,
    cache: "no-store",
    redirect: "manual",
  };

  if (req.method !== "GET" && req.method !== "HEAD") {
    init.body = await req.arrayBuffer();
  }

  let res = await fetch(backendUrl, init);

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

      headers.set("authorization", `Bearer ${refreshed.accessToken}`);
      res = await fetch(backendUrl, init);
    }
  }

  const responseHeaders = new Headers(res.headers);
  responseHeaders.delete("transfer-encoding");

  return new NextResponse(res.body, {
    status: res.status,
    headers: responseHeaders,
  });
}

type RouteContext = { params: Promise<RouteParams> };

export async function GET(req: NextRequest, context: RouteContext) {
  const params = await context.params;
  return proxy(req, params);
}
export async function POST(req: NextRequest, context: RouteContext) {
  const params = await context.params;
  return proxy(req, params);
}
export async function PUT(req: NextRequest, context: RouteContext) {
  const params = await context.params;
  return proxy(req, params);
}
export async function PATCH(req: NextRequest, context: RouteContext) {
  const params = await context.params;
  return proxy(req, params);
}
export async function DELETE(req: NextRequest, context: RouteContext) {
  const params = await context.params;
  return proxy(req, params);
}
