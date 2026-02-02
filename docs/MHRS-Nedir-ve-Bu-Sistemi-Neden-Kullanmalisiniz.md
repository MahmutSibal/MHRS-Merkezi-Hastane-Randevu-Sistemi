# MHRS Nedir? Bu Sistemi Neden Kullanmalısınız?

Bu doküman, Türkiye'de yaygın olarak kullanılan MHRS (Merkezi Hastane Randevu Sistemi) konseptini açıklarken, bu depodaki "WebAppointment – Hastane Randevu Sistemi" uygulamasının neden tercih edilebileceğini iş gerekçeleri ve teknik açıdan detaylandırır.

---

## MHRS Nedir?

MHRS, vatandaşların devlet hastaneleri ve anlaşmalı sağlık kuruluşlarında **hekim veya branş bazlı randevu almasını** sağlayan, **merkezi** ve **standart** süreçlere sahip bir **çevrimiçi randevu platformudur**. Amaç; sağlık hizmetlerine erişimi kolaylaştırmak, yoğunluğu azaltmak ve randevu yönetimini kurallı bir çerçevede işletmektir.

Temel özellikler:
- Branş ve hastane bazlı arama, uygun saatler üzerinden randevu seçimi
- Randevu iptali ve yönetimi için belirlenmiş kurallar (zaman kısıtları vb.)
- Kullanıcı kimlik doğrulama, rol ve yetki kontrolü
- Merkezi istatistik ve raporlama

> Not: Bu repo **resmi MHRS değildir**. Eğitim, demo ve özel kurum içi kullanım senaryolarına yönelik, MHRS mimarisinden ilham alan bir çözüm sunar.

---

## Bu Sistemi Neden Kullanmalısınız?

Bu proje, kurum içi ihtiyaçlarınız ve modern yazılım geliştirme gereksinimleriniz için **esnek, genişletilebilir** ve **modüler** bir randevu platformu sağlar.

Öne çıkan nedenler:
- **Kurumunuza Özel Uyarlama:** Çok-kiracılı mimari (multi-tenant) ile her hastane/kurum kendi verileriyle izole çalışır. Kuralları, akışları ve ekranları kurumunuza göre özelleştirebilirsiniz.
- **Modern Teknoloji Yığını:** Backend `.NET 8 Web API`, Frontend `Next.js 16` üstüne kuruludur. Temiz mimari katmanlarıyla bakım ve gelişim kolaydır.
- **Güvenlik ve Uyumluluk:** JWT + Refresh token, rol/policy tabanlı yetkilendirme, denetim (audit) kayıtları, hız limitleri ve global hata yakalama ile sağlam bir güvenlik zemini.
- **Geliştirici Üretkenliği:** Otomatik migrasyon, Swagger, Serilog loglama ve proxy mimarisi sayesinde hızlı geliştirme döngüsü.
- **Görselleştirme ve Raporlama:** Popüler doktorlar gibi metrikleri grafikler halinde görselleştirebilirsiniz.
- **Akıllı Asistan:** Gemini destekli konuşarak randevu alma deneyimi ile çağdaş bir UX.

Kimler için uygun?
- Hastaneler ve klinikler (kurum içi randevu yönetimi)
- Eğitim kurumları ve Ar-Ge birimleri (örnek uygulama / prototipleme)
- Yazılım ekipleri (Clean Architecture ve tam-stack örnek)

---

## Öne Çıkan Özellikler

- **Roller:** Patient, Doctor, HospitalAdmin, Admin
- **Randevu Yönetimi:** Alma, listeleme, iptal (kurallı), doktor onayı/tamamlama
- **Katalog:** Hastaneler (konum bazlı), bölümler, doktorlar
- **Raporlar:** En popüler doktorlar (Chart.js görselleştirme)
- **AI Asistan:** Gemini destekli konuşarak randevu alma
- **Haritalar:** Google Maps ile yakın hastaneler/işaretçiler
- **Güvenlik:** JWT + Refresh, rol/policy, rate limiting, global hata yakalama
- **Dev Deneyimi:** Otomatik EF migrasyonları, Swagger, Serilog loglama

İlgili konumlar:
- Başlangıç noktası (Backend): [WebAppointment.Api/WebAppointmentApi.WebApi/Program.cs](WebAppointment.Api/WebAppointmentApi.WebApi/Program.cs)
- Controller’lar: [WebAppointment.Api/WebAppointmentApi.WebApi/Controllers](WebAppointment.Api/WebAppointmentApi.WebApi/Controllers)
- Frontend App Router: [WebAppointment.Frontend/src/app](WebAppointment.Frontend/src/app)
- Frontend middleware: [WebAppointment.Frontend/middleware.ts](WebAppointment.Frontend/middleware.ts)

---

## Mimari ve Teknolojiler

- **Backend:** .NET 8 Web API (Clean Architecture)
  - Katmanlar: WebApi, Application, Infrastructure, Domain
  - Çok-kiracılı yapı: `TenantId` alanları ve istek başına global filtreler
  - Denetim (audit): Kim/Ne zaman/Hangi entity/Önce-Sonra/IP
  - Validasyon: FluentValidation + ProblemDetails çıktısı
  - Gözlemlenebilirlik: Serilog request logging
  - Performans ve Güvenlik: Rate limiting, global hata yakalama
- **Frontend:** Next.js 16 + React 19 + TypeScript
  - Route koruması ve rol bazlı erişim
  - Proxy ile backend iletişimi ve JWT cookie’leri
  - Modern UI bileşenleri ve grafik görselleştirme

---

## MHRS’ye Göre Konumlandırma

- **Amaç:** Resmi MHRS’nin kurallı randevu deneyimini kurum içi, özelleştirilebilir ve çok-kiracılı yapıda örneklemek.
- **Kapsam:** Eğitim, demo ve kurum içi kullanım; resmi entegrasyon veya prod ortam yerine, örnek/başvuru uygulaması.
- **Uyarlama:** Branş kuralları, iptal/çakışma politikaları, raporlar ve veri modeli kolayca genişletilebilir.

> Bu çözüm, prod ortamda kullanılmadan önce ek güvenlik, ölçeklenebilirlik ve operasyonel gereksinimlerin (izleme, yedekleme, uyarı sistemleri vb.) kurumunuzun standartlarına göre tamamlanmasını önerir.

---

## KVKK ve Güvenlik Notları

- HttpOnly cookie’ler: `mhrs_at` (access), `mhrs_rt` (refresh)
- Role-based guard: Frontend middleware ile hassas route’lar korunur
- Rate limit: kullanıcı/IP bazlı kısıtlama
- ProblemDetails ile tutarlı hata formatı; frontend istemcisi kullanıcı dostu mesajlar üretir
- Denetim kayıtları (audit log) ile kim/hangi değişikliği yaptı izlenir.
- Çok-kiracılık ile veri izolasyonu sağlanır; her tenant kendi verisini görür.

---

## Kurulum ve İlk Çalıştırma

Kurulum adımları kök dokümanda yer alır: [README.md](README.md)
- Veritabanı bağlantıları: [WebAppointment.Api/WebAppointmentApi.WebApi/appsettings.json](WebAppointment.Api/WebAppointmentApi.WebApi/appsettings.json)
- Migration komutları ve çalışma talimatları README’de örneklerle mevcuttur.

---

## SSS (Sık Sorulan Sorular)

- **Bu uygulama resmi MHRS mi?** Hayır. MHRS mimarisini örnek alan, eğitim ve kurum içi kullanım odaklı bir çözümdür.
- **Gerçek hasta verisi ile kullanabilir miyiz?** Üretim ortamı için gerekli güvenlik ve KVKK süreçlerini tamamladıktan sonra, kurum politikalarına uygun şekilde kullanılabilir.
- **Özelleştirme yapabilir miyiz?** Evet. Clean Architecture ve modüler yapı sayesinde kuralları, akışı ve UI’ı uyarlayabilirsiniz.
- **Harici sistemlere entegrasyon mümkün mü?** Evet. Web API katmanı üzerinden üçüncü parti sistemlerle entegrasyon geliştirilebilir.

---

## Yazar ve İletişim

- İsim: Mahmut Sibal
- GitHub: [MahmutSibal](https://github.com/MahmutSibal)
- E-posta: [mahmutsibal9@gmail.com](mailto:mahmutsibal9@gmail.com)
