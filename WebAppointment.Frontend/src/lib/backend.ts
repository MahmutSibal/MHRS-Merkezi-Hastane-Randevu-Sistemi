export function getBackendOrigin() {
  return process.env.BACKEND_ORIGIN ?? "http://localhost:5233";
}
