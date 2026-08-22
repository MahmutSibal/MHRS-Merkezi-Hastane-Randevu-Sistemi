# MHRS – Merkezi Hastane Randevu Sistemi


**Teknoloji Yığını:** .NET 8 · Next.js 16 · TypeScript 5.x · SQL Server · Node.js (WPPConnect WhatsApp Bridge) · Brevo (E-posta) · Google Identity Services · Google reCAPTCHA
**Lisans:** MIT

Kurumsal ölçekte kurgulanmış, yüksek güvenlik ve operasyonel süreklilik odaklı bir hastane randevu yönetim platformu.
Çok rollü erişim (Hasta, Doktor, Hastane Yöneticisi, Admin), güçlü iş kuralları, izlenebilirlik (audit), çok kiracılı yapı (tenant), WhatsApp bildirim köprüsü ve modern Next.js arayüzü ile üretim ortamına hazır bir temel sunar.

---

## İçindekiler

- [Özellikler](#özellikler)
- [Son Eklenen Özellikler](#son-eklenen-özellikler)
- [Kurumsal Değer Önerisi](#kurumsal-değer-önerisi)
- [Mimari](#mimari)
- [Hızlı Başlangıç](#hızlı-başlangıç)
- [Backend – Ayrıntılar](#backend--ayrıntılar)
- [Frontend – Ayrıntılar](#frontend--ayrıntılar)
- [WhatsApp Bot](#whatsapp-bot)
- [Ortam Değişkenleri](#ortam-değişkenleri)
- [Hızlı Deneme Senaryoları](#hızlı-deneme-senaryoları)
- [Güvenlik Notları](#güvenlik-notları)
- [Sık Karşılaşılan Sorunlar](#sık-karşılaşılan-sorunlar)
- [QA Düzeltme Kontrol Listesi](#qa-düzeltme-kontrol-listesi)
- [Lisans](#lisans)

## Kurumsal Değer Önerisi

- **Güvenilir Operasyon:** Kritik randevu akışlarında kural tabanlı doğrulama, çakışma önleme ve rol bazlı yetki kontrolü.
- **İzlenebilirlik ve Uyum:** Değişikliklerin audit log ile kayıt altına alınması; kurumsal denetim ve uyumluluk süreçlerine hazır altyapı.
- **Ölçeklenebilir Mimari:** Clean Architecture, katmanlı ayrım ve tenant tabanlı veri izolasyonu ile sürdürülebilir büyüme.
- **Omnichannel İletişim:** WhatsApp üzerinden doğrulama, şifre işlemleri ve randevu bildirimleriyle yüksek kullanıcı erişilebilirliği.
- **Bakım Kolaylığı:** Ayrık backend/frontend yapısı, net API sınırları ve geliştirici dostu proje organizasyonu.

---

### Dokümantasyon
- Ayrıntılı açıklama: [docs/MHRS-Nedir-ve-Bu-Sistemi-Neden-Kullanmalisiniz.md](docs/MHRS-Nedir-ve-Bu-Sistemi-Neden-Kullanmalisiniz.md)

---

## Özellikler

- **Roller:** Patient, Doctor, HospitalAdmin, Admin
- **Randevu Yönetimi:** Alma, listeleme, iptal (kurallı), doktor onayı/tamamlama
- **Randevu Yönetimi:** Alma, listeleme, iptal (kurallı), erteleme, doktor onayı/tamamlama
- **Veli / Çocuk (Dependent):** Hasta hesabına bağlı çocuk kaydı ekleme ve çocuk adına randevu oluşturma
- **Kimlik Doğrulama (NVI):** Hasta kaydı ve hasta yakını ekleme akışlarında TC Kimlik doğrulama entegrasyonu
- **Katalog:** Hastaneler (konum bazlı), bölümler, doktorlar
- **Doktor Profil Onayı:** Doktor mezuniyet (üniversite) + deneyim bilgisi girer; Hastane Yöneticisi onaylar; hasta sadece onaylı bilgiyi görür
- **Raporlar:** En popüler doktorlar (Chart.js görselleştirme)
- **AI Asistan:** Gemini destekli konuşarak randevu alma
- **Haritalar:** Google Maps ile yakın hastaneler/işaretçiler
- **Güvenlik:** JWT + Refresh, rol/policy, rate limiting, global hata yakalama
- **Telefon Doğrulama:** 6 haneli kod ile kayıt onayı (90 sn geçerli)
- **Şifremi Unuttum (Hasta):** Ad + Soyad + TC + Telefon ile doğrulama, yeni şifre WhatsApp üzerinden gönderilir
- **Randevu WhatsApp Bildirimi:** Randevu alınca hastaneye, bölüme, doktora ve tarih-saat bilgisini içeren mesaj
- **Otomatik WhatsApp Hatırlatma:** Randevuya 24 saat kala ve 3 saat kala (ikinci/son dakika) arka plan servisi ile otomatik, kişiselleştirilmiş hatırlatma
- **İki Yönlü WhatsApp Onay/İptal:** Hasta hatırlatmaya "1" (katılacağım) / "2" (gelemeyeceğim) yazarak yanıt verir; iptal otomatik olarak bekleme listesindeki bir sonraki hastaya açılır
- **No-Show Risk Skoru:** Hastanın geçmiş katılım/iptal davranışına göre risk skoru hesaplanır; yanıt vermeyen yüksek riskli randevular otomatik iptal edilip slot boşaltılır; yönetim/hastane raporlarında riskli randevular panosu gösterilir
- **Sağlık Profili:** Hastanın alerji, kronik hastalık, ilaç ve acil iletişim bilgilerini profilinden yönetebilmesi
- **WhatsApp Köprüsü:** Kod, şifre ve randevu bildirimlerini ileten Node.js servis; API tarafından otomatik başlatılıp yönetilir, bağlantı QR kodu terminal yerine Yönetim panelinde gösterilir
- **E-posta Doğrulama (Doktor/Yönetim):** İlk girişte e-postaya gönderilen 6 haneli kod girilmeden panele erişilemez
- **Google ile Giriş:** Doktor/Yönetim girişinde, mevcut hesapla eşleşen Google hesabıyla giriş yapabilme (yeni hesap açmaz)
- **reCAPTCHA:** Hasta, Doktor ve Yönetim girişlerinin tamamında bot koruması
- **SMA Bağış Dizini:** İnteraktif Türkiye il haritası üzerinden SMA'lı hastalar için IBAN bilgisi gösteren, ödeme işlemi yapmayan bir bağış rehberi; süperadmin panelinden tek tuşla açılıp kapatılabilir
- **Dev Deneyimi:** Otomatik EF migrasyonları, Swagger, Serilog loglama

---

## Son Eklenen Özellikler

Bu bölüm, en son geliştirme turunda eklenen dört ana özelliği özetler.

### 1) İki Yönlü WhatsApp Hatırlatma & No-Show Risk Skoru
- Randevudan 24 saat önce kişiselleştirilmiş ("Sayın Ad,") hatırlatma; randevuya 3 saat kala ikinci/son bir hatırlatma.
- Hasta WhatsApp'tan "1" veya "2" yazarak randevusunu onaylayabilir/iptal edebilir.
- Onaylanmayan yüksek risk skorlu (`Patient.NoShowScore`) randevular otomatik iptal edilip bekleme listesindeki bir sonraki hastaya açılır; düşük riskli hastalara dokunulmaz, personel kararına bırakılır.
- Doktor bir randevuyu "Gelmedi" olarak işaretlediğinde veya hasta WhatsApp'tan iptal/onay verdiğinde risk skoru otomatik güncellenir.
- İlgili kodlar: [AppointmentReminderService.cs](WebAppointment.Api/WebAppointmentApi.Infrastructure/BackgroundJobs/AppointmentReminderService.cs), [WhatsAppReplyService.cs](WebAppointment.Api/WebAppointmentApi.Application/Appointments/Services/WhatsAppReplyService.cs), [NoShowScoring.cs](WebAppointment.Api/WebAppointmentApi.Application/Patients/Services/NoShowScoring.cs)

### 2) Doktor/Yönetim Girişi: E-posta Doğrulama + Google ile Giriş + reCAPTCHA
- İlk girişte şifre doğruysa ama e-posta henüz doğrulanmamışsa, hesaba bağlı e-postaya (Brevo ile) 6 haneli kod gönderilir; kod girilmeden token verilmez.
- "Google ile Giriş" butonu, Google hesabının e-postası sistemde **kayıtlı** bir hesapla eşleşirse giriş yaptırır (yeni hesap açmaz) ve Google zaten e-postayı doğruladığı için doğrulama adımını da otomatik tamamlar.
- Hasta, Doktor ve Yönetim girişlerinin üçünde de reCAPTCHA v2 zorunludur.
- İlgili kodlar: [AuthService.cs](WebAppointment.Api/WebAppointmentApi.Application/Auth/Services/AuthService.cs), [LoginClient.tsx](WebAppointment.Frontend/src/app/(public)/login/LoginClient.tsx)

### 3) WhatsApp Bridge Otomatik Yönetimi + Web'den QR Bağlantısı
- `mhrs-whatsapp-bot` artık elle `node index.js` ile ayrı bir terminalde başlatılmak zorunda değil — .NET API açılırken kendisi başlatır ve kapanışta kapatır (`WhatsAppBridgeProcessService`); zaten çalışan bir bridge varsa tekrar başlatmaz.
- Bağlantı durumu ve (gerekiyorsa) QR kodu artık terminal yerine **Yönetim → WhatsApp Bağlantısı** sayfasında gösterilir.
- İlgili kodlar: [WhatsAppBridgeProcessService.cs](WebAppointment.Api/WebAppointmentApi.Infrastructure/BackgroundJobs/WhatsAppBridgeProcessService.cs), [admin/whatsapp/page.tsx](WebAppointment.Frontend/src/app/(app)/admin/whatsapp/page.tsx)

### 4) SMA Bağış Dizini
- `/sma` altında, gerçek il sınırlarıyla interaktif bir Türkiye haritası; bir ile tıklanınca o ildeki doğrulanmış SMA vakaları listelenir.
- Vaka detay sayfası (`/sma/[slug]`) sadece hikaye + IBAN + hesap sahibi adını gösterir — **platform bağış toplamaz veya iletmez**, bağışçı kendi banka uygulamasından doğrudan gönderir.
- Süperadmin, **Yönetim → SMA Bağış Yönetimi** ekranından vaka ekler, doğrular, yayınlar; **Sistem Durumu** anahtarıyla tüm özelliği tek tuşla kapatabilir — kapalıyken ana sayfadaki menüden kaybolur ve `/sma` adresine doğrudan girmek de 404 döner.
- İlgili kodlar: [SmaCaseService.cs](WebAppointment.Api/WebAppointmentApi.Application/Sma/Services/SmaCaseService.cs), [sma/page.tsx](WebAppointment.Frontend/src/app/(public)/sma/page.tsx), [admin/sma/page.tsx](WebAppointment.Frontend/src/app/(app)/admin/sma/page.tsx)

---

## Mimari

- **Backend:** .NET 8 Web API (Clean Architecture)
  - Katmanlar: WebApi, Application, Infrastructure, Domain
  - Önemli giriş noktası: [WebAppointment.Api/WebAppointmentApi.WebApi/Program.cs](WebAppointment.Api/WebAppointmentApi.WebApi/Program.cs)
  - Global hata yakalama: [WebAppointment.Api/WebAppointmentApi.WebApi/Middleware/ExceptionHandlingMiddleware.cs](WebAppointment.Api/WebAppointmentApi.WebApi/Middleware/ExceptionHandlingMiddleware.cs)
  - Controller örnekleri: [WebAppointment.Api/WebAppointmentApi.WebApi/Controllers](WebAppointment.Api/WebAppointmentApi.WebApi/Controllers)
- **Frontend:** Next.js 16 + React 19 + TypeScript
  - App Router ile sayfalar: [WebAppointment.Frontend/src/app](WebAppointment.Frontend/src/app)
  - Proxy + session API’leri: [WebAppointment.Frontend/src/app/api](WebAppointment.Frontend/src/app/api)
  - Rol korumalı middleware: [WebAppointment.Frontend/middleware.ts](WebAppointment.Frontend/middleware.ts)
- **WhatsApp Bot:** Node.js + WPPConnect
  - Basit köprü API: [mhrs-whatsapp-bot/index.js](mhrs-whatsapp-bot/index.js)

---

## Hızlı Başlangıç

Yerel geliştirme için asgari adımlar:

```powershell
# 1) Backend
cd WebAppointment.Api/WebAppointmentApi.WebApi
dotnet restore
dotnet ef database update --project ..\WebAppointmentApi.Infrastructure --startup-project .
dotnet run

# 2) Frontend (yeni bir terminalde)
cd ..\..\WebAppointment.Frontend
npm install
npm run dev

# 3) WhatsApp bot (yeni bir terminalde)
cd ..\mhrs-whatsapp-bot
npm install
node index.js
```

- Varsayılan adresler: Backend http://localhost:5233, Frontend http://localhost:3000
- WhatsApp bot varsayılan portu: http://localhost:8080
- Backend bağlantı dizinini ve JWT anahtarını ihtiyaçlarınıza göre özelleştirin (aşağıya bakınız).

## Backend – Ayrıntılar

- **Multi-tenant (SaaS Temeli)**
  - Tüm ana tablolar `TenantId` içerir ve istek başına tenant global filtre ile sınırlandırılır.
  - JWT içinde `tenant_id` claim’i yer alır; yoksa `MultiTenancy:DefaultTenantId` (varsayılan 1) kullanılır.
  - Admin/HospitalAdmin kullanıcılar sadece kendi tenant verisini görür.
  - İlgili kodlar: [WebAppointmentApi.Domain/Common/IMultiTenant.cs](WebAppointment.Api/WebAppointmentApi.Domain/Common/IMultiTenant.cs), [AppDbContext](WebAppointment.Api/WebAppointmentApi.Infrastructure/Data/AppDbContext.cs), [JwtTokenService](WebAppointment.Api/WebAppointmentApi.Infrastructure/Security/JwtTokenService.cs), [TenantContext](WebAppointment.Api/WebAppointmentApi.WebApi/Security/TenantContext.cs).

- **Audit Log (KVKK Uyumlu)**
  - Değişiklikler otomatik izlenir: Kim (UserId/Role), Ne zaman, Hangi Entity, Önce/Sonra, IP.
  - Kayıtlar `AuditLogs` tablosuna yazılır (SaveChanges içinde interceptor).
  - İlgili kodlar: [AuditLog](WebAppointment.Api/WebAppointmentApi.Domain/Entities/AuditLog.cs), [AppDbContext.SaveChangesAsync](WebAppointment.Api/WebAppointmentApi.Infrastructure/Data/AppDbContext.cs).

- **Kimlik Doğrulama & Yetki**
  - JWT Access (15 dk) + Refresh (30 gün) – ayarlar: [WebAppointment.Api/WebAppointmentApi.WebApi/appsettings.json](WebAppointment.Api/WebAppointmentApi.WebApi/appsettings.json)
  - Roller: Patient, Doctor, Admin, HospitalAdmin (bkz. [WebAppointment.Api/WebAppointmentApi.Domain/Enums/UserRole.cs](WebAppointment.Api/WebAppointmentApi.Domain/Enums/UserRole.cs))
  - Özel politika: "DoctorProfile" – doktorun ilişkili profili olmalı
  - Gelişmiş politika: `CanManageDepartment` (departman yönetimi tenant eşleşmesiyle kısıtlanır)
- **Telefon Doğrulama (Hasta Kaydı)**
  - Kod gönderme: `POST /api/auth/patient/register/request-code`
  - Kodu onaylayıp kayıt: `POST /api/auth/patient/register/confirm`
  - Kod süresi: 5 dakika, yanlış denemeler kayıt altına alınır
- **Şifremi Unuttum (Hasta)**
  - İstek: `POST /api/auth/patient/forgot-password`
  - Girdi: Ad, soyad, TC Kimlik No, telefon
  - Doğrulama başarılıysa yeni şifre üretilir ve WhatsApp ile gönderilir
- **Randevu Akışı (Hasta)**
  - Oluştur: `POST /api/appointments`
  - Liste: `GET /api/appointments/my`
  - İptal: `PUT /api/appointments/{id}/cancel`
  - Kurallar: geçmiş/başlamaya 2 saat kala iptal edilemez; 30 dk sabit süre; çakışma önleme
  - Çocuk için randevu: `dependentId` alanı ile (opsiyonel)
  - Bildirim: Randevu oluşturulunca WhatsApp üzerinden detaylı mesaj gönderilir
- **Doktor Akışı**
  - Liste: `GET /api/doctor/appointments/my`
  - Onay: `PUT /api/doctor/appointments/{id}/approve`
  - Tamamla: `PUT /api/doctor/appointments/{id}/complete`
  - Takvim slotları: `GET /api/doctor/calendar/daily-slots?date=YYYY-MM-DD`
  - Müsaitlik: `GET /api/doctor/availability/me`, `PUT /api/doctor/availability/me`
  - İzin (Time off): `POST /api/doctor/time-offs/me`, `GET /api/doctor/time-offs/me?fromUtc=...&toUtc=...`
  - Uzmanlık bilgisi: `GET /api/doctor/profile/me`, `PUT /api/doctor/profile/me` (gönderince onay bekler)
- **Katalog**
  - Hastaneler: `GET /api/catalog/hospitals` (opsiyonel lat/lng/take)
  - Bölümler: `GET /api/catalog/departments?hospitalId={id}`
  - Doktorlar: `GET /api/catalog/doctors?departmentId={id}`
  - Doktor detayı: `GET /api/catalog/doctors/{id}` (mezuniyet/deneyim sadece onaylıysa gelir)
- **Hastane Yöneticisi**
  - Bölümler: CRUD `api/hospitaladmin/departments`
  - Doktorlar: CRUD `api/hospitaladmin/doctors`
  - Doktor profil onayı: `GET /api/hospitaladmin/doctor-profiles/pending`, `POST /api/hospitaladmin/doctor-profiles/{doctorId}/approve`
- **Admin**
  - Tam kapsam CRUD controller’ları (Doktor yönetimi artık HospitalAdmin panelindedir)
  - Raporlar: `GET /api/admin/reports/top-doctors?days=30&take=10`
  - Riskli randevular (no-show): `GET /api/admin/reports/no-show-risk`
- **Hastane Raporları**
  - Riskli randevular (no-show): `GET /api/hospital/reports/no-show-risk`
- **E-posta Doğrulama (Doktor/Yönetim)**
  - Kod gönder: `POST /api/auth/email/request-code`
  - Kodu onayla: `POST /api/auth/email/confirm`
  - Google ile giriş: `POST /api/auth/google-login` (yalnızca eşleşen mevcut hesapla giriş yapar, yeni hesap açmaz)
  - Girişlerde reCAPTCHA doğrulaması: `Recaptcha:Disabled=false` iken hasta/doktor/admin login isteklerinin tümünde token zorunludur
- **WhatsApp İki Yönlü Mesajlaşma & No-Show Risk**
  - Gelen mesaj webhook’u: köprü servisinden `.NET` API’ye iletilir, "1"/"2" yanıtları randevuyu onaylar/iptal eder
  - Risk skoru hesaplama ve otomatik iptal: [NoShowScoring.cs](WebAppointment.Api/WebAppointmentApi.Application/Patients/Services/NoShowScoring.cs)
  - Hatırlatma zamanlaması (24 saat + 3 saat kala): [AppointmentReminderService.cs](WebAppointment.Api/WebAppointmentApi.Infrastructure/BackgroundJobs/AppointmentReminderService.cs)
- **WhatsApp Köprü Yönetimi (Admin)**
  - Durum: `GET /api/admin/whatsapp/status`
  - QR kod: `GET /api/admin/whatsapp/qr`
  - Köprü süreci .NET API tarafından otomatik başlatılır/kapatılır: [WhatsAppBridgeProcessService.cs](WebAppointment.Api/WebAppointmentApi.Infrastructure/BackgroundJobs/WhatsAppBridgeProcessService.cs)
- **SMA Bağış Dizini**
  - Herkese açık: `GET /api/sma/cases?province={slug}`, `GET /api/sma/cases/{slug}`
  - Yönetim (Admin): `GET/POST/PUT/DELETE /api/admin/sma`, durum: `PATCH /api/admin/sma/{id}/status`
  - Sistem aç/kapa: `GET/PATCH /api/admin/sma/settings` (kapatıldığında herkese açık uçlar da devre dışı kalır)
- **Altyapı**
  - Rate limiting (dakikada 60 istek)
  - Serilog request logging
  - FluentValidation + ProblemDetails hata çıktısı
  - EF Core, otomatik migrasyon (startup’ta)

> Entity’ler ve durumlar için örnekler: [WebAppointment.Api/WebAppointmentApi.Domain/Entities](WebAppointment.Api/WebAppointmentApi.Domain/Entities) ve [WebAppointment.Api/WebAppointmentApi.Domain/Enums](WebAppointment.Api/WebAppointmentApi.Domain/Enums)

---

## Frontend – Ayrıntılar

- **Route Koruması**: [WebAppointment.Frontend/middleware.ts](WebAppointment.Frontend/middleware.ts) JWT cookie’lerine göre `/admin`, `/doctor`, `/patient`, `/app` alanlarını korur.
- **Session & Proxy**
  - Backend origin seçimi: [WebAppointment.Frontend/src/lib/backend.ts](WebAppointment.Frontend/src/lib/backend.ts)
  - JSON istemci: [WebAppointment.Frontend/src/lib/api-client.ts](WebAppointment.Frontend/src/lib/api-client.ts)
  - Backend proxy: [WebAppointment.Frontend/src/app/api/backend/[...path]/route.ts](WebAppointment.Frontend/src/app/api/backend/%5B...path%5D/route.ts)
  - Login/Register/Logout: [WebAppointment.Frontend/src/app/api/session](WebAppointment.Frontend/src/app/api/session)
  - Şifre sıfırlama: [WebAppointment.Frontend/src/app/api/session/forgot-password/route.ts](WebAppointment.Frontend/src/app/api/session/forgot-password/route.ts)
- **Ekranlar**
  - Hasta: [WebAppointment.Frontend/src/app/(app)/patient](WebAppointment.Frontend/src/app/(app)/patient)
    - Yeni randevu: [WebAppointment.Frontend/src/app/(app)/patient/appointments/new/page.tsx](WebAppointment.Frontend/src/app/(app)/patient/appointments/new/page.tsx)
  - Şifremi Unuttum: [WebAppointment.Frontend/src/app/(public)/forgot-password/page.tsx](WebAppointment.Frontend/src/app/(public)/forgot-password/page.tsx)
  - Doktor: [WebAppointment.Frontend/src/app/(app)/doctor](WebAppointment.Frontend/src/app/(app)/doctor)
    - Randevular: [WebAppointment.Frontend/src/app/(app)/doctor/appointments/page.tsx](WebAppointment.Frontend/src/app/(app)/doctor/appointments/page.tsx)
    - Uzmanlık bilgileri: [WebAppointment.Frontend/src/app/(app)/doctor/profile/page.tsx](WebAppointment.Frontend/src/app/(app)/doctor/profile/page.tsx)
  - Hastane Yöneticisi: [WebAppointment.Frontend/src/app/(app)/hospital](WebAppointment.Frontend/src/app/(app)/hospital)
    - Bölümler: [WebAppointment.Frontend/src/app/(app)/hospital/departments/page.tsx](WebAppointment.Frontend/src/app/(app)/hospital/departments/page.tsx)
    - Doktor onayları: [WebAppointment.Frontend/src/app/(app)/hospital/doctor-profiles/page.tsx](WebAppointment.Frontend/src/app/(app)/hospital/doctor-profiles/page.tsx)
  - Admin: [WebAppointment.Frontend/src/app/(app)/admin](WebAppointment.Frontend/src/app/(app)/admin)
    - Raporlar (riskli randevular dahil): [WebAppointment.Frontend/src/app/(app)/admin/reports/page.tsx](WebAppointment.Frontend/src/app/(app)/admin/reports/page.tsx)
    - SMA Bağış Yönetimi (vaka CRUD + sistem aç/kapa): [WebAppointment.Frontend/src/app/(app)/admin/sma/page.tsx](WebAppointment.Frontend/src/app/(app)/admin/sma/page.tsx)
    - WhatsApp Bağlantısı (QR/durum): [WebAppointment.Frontend/src/app/(app)/admin/whatsapp/page.tsx](WebAppointment.Frontend/src/app/(app)/admin/whatsapp/page.tsx)
    - Not: Admin doktor yönetimi kaldırılmıştır; doktor ekleme/güncelleme sadece HospitalAdmin tarafındadır.
  - Hastane Yöneticisi Raporları (riskli randevular): [WebAppointment.Frontend/src/app/(app)/hospital/reports/page.tsx](WebAppointment.Frontend/src/app/(app)/hospital/reports/page.tsx)
- **Giriş Akışı (Doktor/Admin)**: E-posta doğrulama adımı + Google ile Giriş + reCAPTCHA — [WebAppointment.Frontend/src/app/(public)/login/LoginClient.tsx](WebAppointment.Frontend/src/app/(public)/login/LoginClient.tsx)
- **SMA Bağış Dizini (Public)**
  - İl haritası: [WebAppointment.Frontend/src/app/(public)/sma/page.tsx](WebAppointment.Frontend/src/app/(public)/sma/page.tsx), harita bileşeni: [WebAppointment.Frontend/src/components/sma/TurkeyMap.tsx](WebAppointment.Frontend/src/components/sma/TurkeyMap.tsx)
  - Vaka detayı: [WebAppointment.Frontend/src/app/(public)/sma/[slug]/page.tsx](<WebAppointment.Frontend/src/app/(public)/sma/[slug]/page.tsx>)
  - Erişim koruması (sistem kapalıyken 404): [WebAppointment.Frontend/src/app/(public)/sma/layout.tsx](WebAppointment.Frontend/src/app/(public)/sma/layout.tsx)
  - MHRS/SMA menü geçişi + kayma efekti: [WebAppointment.Frontend/src/components/layout/SiteSwitchTabs.tsx](WebAppointment.Frontend/src/components/layout/SiteSwitchTabs.tsx), [WebAppointment.Frontend/src/components/layout/PageSlideWrapper.tsx](WebAppointment.Frontend/src/components/layout/PageSlideWrapper.tsx)
- **Harita Bileşeni (Hastane)**: [WebAppointment.Frontend/src/components/map/HospitalMap.tsx](WebAppointment.Frontend/src/components/map/HospitalMap.tsx)
- **AI Asistan**: [WebAppointment.Frontend/src/components/assistant/AssistantWidget.tsx](WebAppointment.Frontend/src/components/assistant/AssistantWidget.tsx) ve API: [WebAppointment.Frontend/src/app/api/assistant/chat/route.ts](WebAppointment.Frontend/src/app/api/assistant/chat/route.ts)

---

## Kurulum

### Önkoşullar
- .NET 8 SDK
- Node.js 20+ ve pnpm/npm/yarn (örn. npm)
- SQL Server (LocalDB/Developer/Container), `localhost` erişilebilir
- Opsiyonel: Google Maps ve Gemini API anahtarları
- Opsiyonel: Brevo API anahtarı (e-posta doğrulama), Google OAuth Client ID (Google ile Giriş), reCAPTCHA anahtar çifti — bunlar olmadan da sistem çalışır; `Recaptcha:Disabled=true` yapılabilir ve e-posta doğrulama/Google girişi yalnızca Doktor/Admin akışını etkiler

### Test ve Kalite Kontrolleri

Backend testleri:

```powershell
cd WebAppointment.Api
dotnet test
```

Frontend kalite kontrolleri:

```powershell
cd WebAppointment.Frontend
npm run lint
```

### Backend’i Çalıştırma

```powershell
# Proje kökünden Web API klasörüne geçin
cd WebAppointment.Api/WebAppointmentApi.WebApi

# Bağımlılıkları geri yükleyin ve çalıştırın
dotnet restore
dotnet run
# Varsayılan URL: http://localhost:5233  (bkz. Properties/launchSettings.json)
```

- Veritabanı bağlantısı: [WebAppointment.Api/WebAppointmentApi.WebApi/appsettings.json](WebAppointment.Api/WebAppointmentApi.WebApi/appsettings.json)
  - `ConnectionStrings:DefaultConnection` değerini kendi SQL Server ortamınıza göre güncelleyin.
  - `Jwt:SigningKey` değerini en az 32 karakterlik güçlü bir gizli anahtar ile değiştirin.
- İlk açılışta otomatik migrasyon uygulanır. Swagger: `http://localhost:5233/swagger`

> Not: Şemaya (TenantId/AuditLog vb.) yeni alanlar eklendi. Gerekirse EF migration üretip uygulayın:

```powershell
# Migration oluşturma (Infrastructure projesi context’i içerir)
cd ..\WebAppointmentApi.Infrastructure
dotnet ef migrations add MultiTenant_Audit_Initial --startup-project ..\WebAppointmentApi.WebApi --project .
dotnet ef database update --startup-project ..\WebAppointmentApi.WebApi --project .
```

### Frontend’i Çalıştırma

```powershell
# Proje köküne dönün ve frontend klasörüne geçin
cd ..\..\WebAppointment.Frontend

# Bağımlılıkları kurun
npm install

# Geliştirme sunucusunu başlatın
npm run dev
# Varsayılan URL: http://localhost:3000
```

### Ortam Değişkenleri

Frontend `.env.local` örneği:

```
# Backend Web API adresi
BACKEND_ORIGIN=http://localhost:5233

# Opsiyonel – Google Gemini
GEMINI_API_KEY=your_gemini_key

# Opsiyonel – Google Maps
NEXT_PUBLIC_GOOGLE_MAPS_API_KEY=your_maps_key

# Google ile Giriş (Doktor/Admin) – Google Identity Services client ID
NEXT_PUBLIC_GOOGLE_CLIENT_ID=your_google_oauth_client_id

# Google reCAPTCHA v2 site key (üç login formunda da kullanılır)
NEXT_PUBLIC_RECAPTCHA_SITE_KEY=your_recaptcha_site_key
```

> Frontend, backend’e `/api/backend/...` proxy’si ile gider; `BACKEND_ORIGIN` bu yüzden kritiktir.

Backend `appsettings.json` çok-kiracılık örneği:

```
"MultiTenancy": {
  "DefaultTenantId": 1
}
```

WhatsApp köprü servisi (Web API) örneği:

```
"WhatsAppBridge": {
  "BaseUrl": "http://localhost:8080",
  "AutoStart": true,
  "WorkingDirectory": "../../mhrs-whatsapp-bot"
}
```

> `AutoStart: true` iken API açılırken köprü süreci otomatik başlar (zaten çalışıyorsa tekrar başlatmaz) ve API kapanırken kapatılır. Bağlantı QR kodu Yönetim → WhatsApp Bağlantısı sayfasından okutulur.

E-posta doğrulama (Brevo), Google ile Giriş ve reCAPTCHA — gerçek değerler **asla** `appsettings.json`'a yazılmaz, sadece `dotnet user-secrets` ile ayarlanır:

```
"Brevo": {
  "ApiKey": "__SET_VIA_USER_SECRETS__",
  "SenderEmail": "__SET_VIA_USER_SECRETS__",
  "SenderName": "MHRS"
},
"GoogleAuth": {
  "ClientId": "__SET_VIA_USER_SECRETS__"
},
"Recaptcha": {
  "SecretKey": "__SET_VIA_USER_SECRETS__",
  "Disabled": false
}
```

```powershell
cd WebAppointment.Api/WebAppointmentApi.WebApi
dotnet user-secrets set "Brevo:ApiKey" "..."
dotnet user-secrets set "Brevo:SenderEmail" "..."
dotnet user-secrets set "GoogleAuth:ClientId" "..."
dotnet user-secrets set "Recaptcha:SecretKey" "..."
```

---

## Hızlı Deneme Senaryoları

- **Hasta Kaydı & Giriş**
  1) `/register` üzerinden kayıt ol veya `/login` ile giriş yap
  2) `/patient/appointments/new` alanında "Randevu Kimin İçin?" bölümünden (opsiyonel) çocuk ekle / seç
  3) Hastane → bölüm → doktor → tarih/saat seçip randevu oluştur (doktor detay kartında onaylı uzmanlık bilgisi varsa görünür)
  4) Randevu oluşturulunca WhatsApp üzerinden detaylı bilgilendirme gelir
- **Şifremi Unuttum (Hasta)**
  1) `/forgot-password` sayfasına git
  2) Ad, soyad, TC ve telefon gir
  3) Yeni şifre WhatsApp üzerinden gönderilir
- **Doktor Onayı**
  1) Doktor olarak giriş yap
  2) `/doctor/appointments` ekranından bekleyen randevuyu **Onayla** ardından **Tamamla**
- **Doktor Uzmanlık Bilgisi Onayı**
  1) Doktor olarak giriş yap → `/doctor/profile` ekranından mezuniyet ve deneyim girip **Onaya Gönder**
  2) Hastane yöneticisi (HospitalAdmin) olarak giriş yap → `/hospital/doctor-profiles` ekranından **Onayla**
  3) Hasta olarak `/patient/appointments/new` ekranında doktor detayında onaylı bilgiler görünür
- **Raporlar (Admin)**
  1) Admin olarak giriş yap
  2) `/admin/reports` ekranından filtreleri kullanarak en popüler doktorları ve riskli (no-show) randevuları gör
- **Doktor/Admin Girişi – E-posta Doğrulama**
  1) Doktor veya Admin hesabıyla ilk kez `/login` üzerinden giriş yap (şifre doğru olmalı)
  2) reCAPTCHA'yı tamamla; sistem hesaba bağlı e-postaya 6 haneli kod gönderir
  3) Kodu ekrana girip onayla; bundan sonraki girişlerde tekrar istenmez
- **Google ile Giriş (Doktor/Admin)**
  1) `/login` ekranında "Google ile Giriş" butonuna tıkla
  2) Google hesabının e-postası sistemde kayıtlı bir Doktor/Admin hesabıyla eşleşiyorsa doğrudan giriş yapılır (yeni hesap açılmaz, eşleşme yoksa reddedilir)
- **WhatsApp Bağlantısı**
  1) Admin olarak `/admin/whatsapp` ekranına git
  2) Bağlı değilse ekrana gelen QR kodu WhatsApp mobil uygulamasından okut
  3) Bağlantı kurulunca hatırlatma/onay-iptal mesajları bu numaradan gönderilip alınır
- **İki Yönlü WhatsApp Onay/İptal**
  1) Bir hastanın yaklaşan randevusu için hatırlatma mesajı gönderilmesini bekle (24 saat veya 3 saat kala)
  2) Hasta WhatsApp'tan "1" (katılacağım) veya "2" (gelemeyeceğim) yazar
  3) "2" yanıtı randevuyu iptal eder ve bekleme listesindeki bir sonraki hastaya slotu açar; yüksek riskli hastalarda yanıt gelmezse randevu otomatik iptal edilir
- **SMA Bağış Akışı**
  1) Ana sayfanın üst-orta menüsünden **SMA Bağış**'a geç (kayma efektiyle sayfa değişir)
  2) Türkiye haritasında bir ile tıkla → o ildeki doğrulanmış/yayınlanmış vakaları gör
  3) Bir vakaya tıkla → hikaye + IBAN + hesap sahibi adını gör (ödeme formu yok, doğrudan banka üzerinden gönderilir)
- **SMA Sistemini Aç/Kapa (Süperadmin)**
  1) `/admin/sma` → **Sistem Durumu** kartından **Kapat**'a bas
  2) Ana sayfadaki MHRS/SMA menüsü kaybolur; `/sma` adresine doğrudan girmeye çalışmak 404 döner
  3) Aynı ekrandan **Aç**'a basarak tekrar herkese açık hale getirebilirsin

---

## Güvenlik Notları

- HttpOnly cookie’ler: `mhrs_at` (access), `mhrs_rt` (refresh)
- Role-based guard: Frontend middleware ile hassas route’lar korunur
- Rate limit: kullanıcı/IP bazlı kısıtlama
- ProblemDetails ile tutarlı hata formatı; frontend `apiJson` bunları kullanıcı dostu mesaja çevirir
- Rate limiting, JWT secret ve bağlantı dizileri gibi kritik değerleri production ortamında bir secret manager üzerinden yönetin.

## Operasyonel Olgunluk

- **Arka Plan İşleri:** Periyodik çalışan servisler ile zaman bazlı bildirim süreçleri merkezi olarak yönetilir.
- **Hata Dayanıklılığı:** Global exception middleware, tutarlı ProblemDetails çıktısı ve Serilog tabanlı izleme ile hızlı kök neden analizi.
- **Veri Bütünlüğü:** Transaction ve kilitleme stratejileriyle randevu çakışması ve yarış durumu riskleri azaltılır.
- **Sürüm Güvenliği:** EF Core migration yaklaşımı ile şema değişiklikleri kontrollü ve izlenebilir şekilde taşınır.

---

---

## Katkı Rehberi (CONTRIBUTING)

- Yeni özellikler için küçük, odaklı branch’ler açın.
- PR’larda değişiklik amacı, test sonuçları ve olası riskleri netçe belirtin.
- Kodunuzu göndermeden önce ilgili test/kalite kontrollerini çalıştırın.

## Sık Karşılaşılan Sorunlar

- "Bağlantı sağlanamıyor": SQL Server servisinin çalıştığını ve `DefaultConnection` değerinin doğru olduğunu doğrulayın.
- 401/403 hataları: Rol ve token geçerliliğini kontrol edin. Proxy route otomatik refresh dener.
- Saat dilimi: Frontend, randevu gönderiminde Türkiye saati için `+03:00` ekler; backend tüm tarihleri UTC olarak saklar.
- `Invalid object name 'PhoneVerificationCodes'`: Migration uygulanmamıştır. EF migration çalıştırın veya veritabanını sıfırlayın.
- `Cannot insert duplicate key ... IX_Users_TenantId_Email`: Aynı tenant içinde boş e-posta ile kayıt denenmiştir. Yeni kayıtlarda benzersiz placeholder e-posta üretilir; eski yarım kayıtlar temizlenmelidir.
- `Permission denied` Git uyarıları (tokens): [mhrs-whatsapp-bot/tokens](mhrs-whatsapp-bot/tokens) klasörünü git dışına alın (bkz. .gitignore).
- WhatsApp köprüsünde `"No LID for user"` veya `"Cannot read properties of undefined"`: WhatsApp Web ile wppconnect sürüm uyumsuzluğudur, uygulama hatası değildir. `mhrs-whatsapp-bot/package.json` içindeki `@wppconnect-team/wppconnect` sürümünü güncelleyin (`npm install`).
- E-posta doğrulama kodu gelmiyor / Brevo 401-403: `dotnet user-secrets list` ile `Brevo:ApiKey`/`Brevo:SenderEmail` değerlerinin ayarlı olduğunu doğrulayın; gönderen e-posta Brevo hesabında doğrulanmış olmalıdır.
- Google ile Giriş "hesap bulunamadı" hatası: Bu akış yeni hesap açmaz — Google hesabının e-postası sistemde önceden kayıtlı bir Doktor/Admin hesabıyla birebir eşleşmelidir.
- reCAPTCHA doğrulanamadı: `NEXT_PUBLIC_RECAPTCHA_SITE_KEY` (frontend) ile `Recaptcha:SecretKey` (backend, user-secrets) aynı reCAPTCHA anahtar çiftine ait olmalıdır; test ortamında `Recaptcha:Disabled=true` ile geçici olarak kapatılabilir.
- `/sma` adresine girince 404: SMA sistemi süperadmin tarafından `/admin/sma` üzerinden kapatılmıştır — bu kasıtlı bir erişim engelidir, hata değildir.

---

## WhatsApp Bot

- Amaç: Kayıt doğrulama kodu, hasta şifre sıfırlama, randevu bildirimleri, hatırlatmalar ve iki yönlü onay/iptal mesajlarını WhatsApp üzerinden göndermek/almak.
- Çalışma portu: `8080`
- Endpoint'ler: `POST /send-message` (body: `{ phone, message }`), `GET /status`, `GET /qr`
- Kod: [mhrs-whatsapp-bot/index.js](mhrs-whatsapp-bot/index.js)
- **Otomatik yönetim**: Elle başlatmaya gerek yoktur — `WhatsAppBridge:AutoStart=true` iken .NET API kendisi başlatır/kapatır ([WhatsAppBridgeProcessService.cs](WebAppointment.Api/WebAppointmentApi.Infrastructure/BackgroundJobs/WhatsAppBridgeProcessService.cs)); bağlantı QR kodu Yönetim → WhatsApp Bağlantısı sayfasında gösterilir.

> Not: `mhrs-whatsapp-bot/tokens` klasörü WhatsApp oturum ve cache dosyalarıdır. Repoya eklenmemelidir.

---

## Lisans

Bu proje MIT lisansı ile sunulmaktadır. Ayrıntılar için `LICENSE` dosyasına bakınız.

---

## İletişim

- İsim: Mahmut Sibal
- GitHub: [MahmutSibal](https://github.com/MahmutSibal)
- E-posta: [mahmutsibal9@gmail.com](mailto:mahmutsibal9@gmail.com)

---

## Katkıda Bulunanlar

- Test desteği: Elif Küçük ([GitHub: ekucuk-eng](https://github.com/ekucuk-eng), [LinkedIn](https://www.linkedin.com/in/elif-k%C3%BC%C3%A7%C3%BCk-187258326/))
- Tüm liste için bkz. [CONTRIBUTORS.md](CONTRIBUTORS.md)

---

## QA Düzeltme Kontrol Listesi

- ✅ Doktor Entity: `Title (Unvan)` alanı eklendi, API cevaplarında döndürülüyor.
- ✅ Doktor oluşturma/güncelleme: `Title` zorunlu alan olarak doğrulanıyor.
- ✅ E-posta doğrulama: RFC benzeri kontrol + Türkçe karakter (ı, İ) engeli.
- ✅ Şifre doğrulama: Min. 8 karakter, sadece rakam olamaz; zayıf şifre reddedilir.
- ✅ Hospital Admin: Doktor e-posta/şifre güncelleme için güvenli `PUT /api/hospitaladmin/doctors/{id}/credentials` endpoint’i eklendi.
- ✅ Hospital Admin: Doktor pasifleştirme (`DELETE`) korunmuş ve güvenli (departman/hastane sahipliği kontrolü).
- ✅ Hospital Admin: Yanlış girilmiş departman adlarını ve doktor bilgilerini düzenleme endpoint’leri mevcut ve güvenli.
- ✅ Global Hata Yakalama: Son kullanıcıya stack trace asla gösterilmez; backend log’da detaylar kalır.
- ✅ Frontend Doktor UI: Doktor adı yanında bölüm ve unvan gösterimi (örn. “Tuğba Çoban – İç Hastalıkları – Uz. Dr.”).
- ✅ Frontend Form UX: E-posta/şifre için anlık doğrulama mesajları; geçersizken gönderim engellenir.
- ✅ AI Asistan (UI): Karanlık modda mesajları görünür kılan kontrast düzeltmesi yapıldı.
- ✅ AI Asistan (Oturum): Oturum değişince/çıkışta sohbet geçmişi sıfırlanır; farklı admin hesapları arasında kalıcı olmaz.
- ✅ Kullanıcı Dostu Hatalar: Teknik detaylar gizlenir; anlaşılır mesajlar gösterilir.
- ✅ Durum Yönetimi: Auth ve chat state izolasyonu; sızıntılar önlendi.
- ✅ Veli/Çocuk: Hasta hesabına bağlı çocuk (dependent) ekleme ve çocuk adına randevu oluşturma akışı eklendi.
- ✅ Doktor Profil Onayı: Doktor mezuniyet/deneyim bilgisi gönderir; HospitalAdmin onaylar; hasta sadece onaylı bilgiyi görür.
- ✅ AI Asistan Kayıt Akışı: Çift `/api` öneki nedeniyle 404 veren hasta kayıt çağrısı düzeltildi; eksik WhatsApp doğrulama kodu adımı sohbet akışına eklendi.
- ✅ Ana Sayfa Layout Hatası: Kök `src/app/page.tsx` dosyası `(public)/layout.tsx`'i atlayarak header'ın hiç render edilmemesine sebep oluyordu; dosya kaldırılıp yönlendirme mantığı `(public)/page.tsx`'e taşındı.
- ✅ Sayfa Geçiş Efekti: Native View Transitions API, Next.js App Router ile kararsız çalışıp (`Transition was aborted...`) tarayıcıda görünür hata veriyordu; saf CSS remount tabanlı kayma animasyonuyla değiştirildi.
- ✅ Menü Konum Kayması: MHRS/SMA sayfaları arasında üst-orta menünün birkaç piksel kayması, gizlenen sağ menünün her zaman aynı genişlikte render edilmesi ve `scrollbar-gutter: stable` ile giderildi.
- ✅ MHRS/SMA Menüsü Login'de Görünmesin: `/login`, `/register`, `/forgot-password` sayfalarında geçiş menüsü gizlendi.

---

## Proje Yapısı (Özet)

```
WebAppointment.Api/
  WebAppointmentApi.Domain/         # Entity ve temel kavramlar
  WebAppointmentApi.Application/    # İş kuralları, DTO, Validasyon
  WebAppointmentApi.Infrastructure/ # EF Core, Repos, Güvenlik
  WebAppointmentApi.WebApi/         # API (Controllers, Middleware)
WebAppointment.Frontend/            # Next.js 16 uygulaması
docs/                               # Dokümantasyon
```
