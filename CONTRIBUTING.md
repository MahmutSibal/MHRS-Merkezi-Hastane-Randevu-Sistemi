# Katkı Rehberi (Contributing)

Bu proje, topluluktan gelen katkılara açıktır. Lütfen aşağıdaki adımları izleyin.

## Başlamadan Önce

- Kılavuzlar: [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md), [SECURITY.md](SECURITY.md)
- Mimari ve çalışma talimatı: [README.md](README.md)

## Nasıl Katkı Verilir?

1) Issue açın veya mevcut bir issue’yu üstlenin (etiket: `good first issue`, `help wanted`).
2) Yeni bir branch oluşturun: `feat/…`, `fix/…`, `docs/…` gibi açıklayıcı isimler kullanın.
3) Geliştirme yaparken lokal doğrulamaları çalıştırın.
4) Küçük, odaklı PR’lar açın ve PR açıklamasında değişiklik amacını, kapsamı ve test notlarını belirtin.

## Geliştirme Ortamı

- .NET 8 SDK, Node.js 20+, SQL Server
- Hızlı başlangıç komutları için README’deki talimatları izleyin.

Doğrulamalar:

```powershell
# Backend
cd WebAppointment.Api
dotnet build
# (opsiyonel) test
# dotnet test

# Frontend
cd ..\WebAppointment.Frontend
npm run lint
```

## Kod Tarzı ve Commit Mesajları

- C# ve TS/TSX dosyalarında mevcut stil ve düzeni koruyun.
- Commit mesajlarında mümkünse Conventional Commits formatını tercih edin:
	- `feat: ...`, `fix: ...`, `docs: ...`, `refactor: ...`, `chore: ...`

## PR Kontrol Listesi

- [ ] Değişiklikler sadece ilgili kapsamı etkiliyor
- [ ] Geriye uyumlu (breaking change yoksa)
- [ ] Gerekliyse EF migration veya dokümantasyon güncellendi
- [ ] Lint/build başarılı

Teşekkürler! Katkılarınız projeyi daha iyi hale getiriyor.
