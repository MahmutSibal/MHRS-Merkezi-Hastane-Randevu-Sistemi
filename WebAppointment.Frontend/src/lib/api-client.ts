export async function apiJson<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`/api${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...(init?.headers ?? {}),
    },
  });

  if (!res.ok) {
    const text = await res.text();
    
    // JSON hata mesajını parse et
    try {
      const errorData = JSON.parse(text);
      
      // Backend'den gelen hata formatlarını kontrol et
      // Model validation (ProblemDetails) errors sözlüğü
      if (errorData.errors && typeof errorData.errors === "object") {
        const firstMessages: string[] = [];
        for (const key of Object.keys(errorData.errors)) {
          const arr = errorData.errors[key];
          if (Array.isArray(arr) && arr.length > 0) {
            firstMessages.push(String(arr[0]));
          }
        }
        if (firstMessages.length > 0) {
          throw new Error(firstMessages[0]);
        }
      }
      if (errorData.detail) {
        // ProblemDetails formatı
        throw new Error(errorData.detail);
      } else if (errorData.message) {
        // Standart error formatı
        throw new Error(errorData.message);
      } else if (errorData.title) {
        // Sadece title varsa
        throw new Error(errorData.title);
      }
    } catch (parseError) {
      // JSON parse edilemezse veya hiçbir alan yoksa
      if (parseError instanceof SyntaxError) {
        // JSON değilse, düz text kullan
        throw new Error(text || `HTTP ${res.status}`);
      }
      throw parseError;
    }
    
    // Hiçbir hata mesajı bulunamadıysa
    throw new Error(`İstek başarısız oldu (${res.status})`);
  }

  if (res.status === 204) return undefined as T;
  return (await res.json()) as T;
}
