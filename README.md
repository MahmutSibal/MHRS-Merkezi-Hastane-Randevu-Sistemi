<<<<<<< HEAD
# WebAppointment – Hastane Randevu Sistemi

Modern, ölçeklenebilir ve güvenli bir hastane randevu yönetim platformu. Çok rollü erişim (Hasta, Doktor, Hastane Yöneticisi, Admin), JWT tabanlı kimlik doğrulama, güçlü kurallar ve sezgisel bir Next.js arayüzü ile gelir.

---

## Özellikler

- **Roller:** Patient, Doctor, HospitalAdmin, Admin
- **Randevu Yönetimi:** Alma, listeleme, iptal (kurallı), doktor onayı/tamamlama
- **Katalog:** Hastaneler (konum bazlı), bölümler, doktorlar
- **Raporlar:** En popüler doktorlar (Chart.js görselleştirme)
- **AI Asistan:** Gemini destekli konuşarak randevu alma
- **Haritalar:** Google Maps ile yakın hastaneler/işaretçiler
- **Güvenlik:** JWT + Refresh, rol/policy, rate limiting, global hata yakalama
- **Dev Deneyimi:** Otomatik EF migrasyonları, Swagger, Serilog loglama

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

---

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
- **Randevu Akışı (Hasta)**
  - Oluştur: `POST /api/appointments`
  - Liste: `GET /api/appointments/my`
  - İptal: `PUT /api/appointments/{id}/cancel`
  - Kurallar: geçmiş/başlamaya 15 dk kala iptal edilemez; 30 dk sabit süre; çakışma önleme
- **Doktor Akışı**
  - Liste: `GET /api/doctor/appointments/my`
  - Onay: `PUT /api/doctor/appointments/{id}/approve`
  - Tamamla: `PUT /api/doctor/appointments/{id}/complete`
  - Takvim slotları: `GET /api/doctor/calendar/daily-slots?date=YYYY-MM-DD`
- **Katalog**
  - Hastaneler: `GET /api/catalog/hospitals` (opsiyonel lat/lng/take)
  - Bölümler: `GET /api/catalog/departments?hospitalId={id}`
  - Doktorlar: `GET /api/catalog/doctors?departmentId={id}`
- **Hastane Yöneticisi**
  - Bölümler: CRUD `api/hospitaladmin/departments`
  - Doktorlar: CRUD `api/hospitaladmin/doctors`
- **Admin**
  - Tam kapsam CRUD controller’ları (Doktor yönetimi artık HospitalAdmin panelindedir)
  - Raporlar: `GET /api/admin/reports/top-doctors?days=30&take=10`
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
- **Ekranlar**
  - Hasta: [WebAppointment.Frontend/src/app/(app)/patient](WebAppointment.Frontend/src/app/(app)/patient)
    - Yeni randevu: [WebAppointment.Frontend/src/app/(app)/patient/appointments/new/page.tsx](WebAppointment.Frontend/src/app/(app)/patient/appointments/new/page.tsx)
  - Doktor: [WebAppointment.Frontend/src/app/(app)/doctor](WebAppointment.Frontend/src/app/(app)/doctor)
    - Randevular: [WebAppointment.Frontend/src/app/(app)/doctor/appointments/page.tsx](WebAppointment.Frontend/src/app/(app)/doctor/appointments/page.tsx)
  - Hastane Yöneticisi: [WebAppointment.Frontend/src/app/(app)/hospital](WebAppointment.Frontend/src/app/(app)/hospital)
    - Bölümler: [WebAppointment.Frontend/src/app/(app)/hospital/departments/page.tsx](WebAppointment.Frontend/src/app/(app)/hospital/departments/page.tsx)
  - Admin: [WebAppointment.Frontend/src/app/(app)/admin](WebAppointment.Frontend/src/app/(app)/admin)
    - Raporlar: [WebAppointment.Frontend/src/app/(app)/admin/reports/page.tsx](WebAppointment.Frontend/src/app/(app)/admin/reports/page.tsx)
    - Not: Admin doktor yönetimi kaldırılmıştır; doktor ekleme/güncelleme sadece HospitalAdmin tarafındadır.
- **Harita Bileşeni**: [WebAppointment.Frontend/src/components/map/HospitalMap.tsx](WebAppointment.Frontend/src/components/map/HospitalMap.tsx)
- **AI Asistan**: [WebAppointment.Frontend/src/components/assistant/AssistantWidget.tsx](WebAppointment.Frontend/src/components/assistant/AssistantWidget.tsx) ve API: [WebAppointment.Frontend/src/app/api/assistant/chat/route.ts](WebAppointment.Frontend/src/app/api/assistant/chat/route.ts)

---

## Kurulum

### Önkoşullar
- .NET 8 SDK
- Node.js 20+ ve pnpm/npm/yarn (örn. npm)
- SQL Server (LocalDB/Developer/Container), `localhost` erişilebilir
- Opsiyonel: Google Maps ve Gemini API anahtarları

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
```

> Frontend, backend’e `/api/backend/...` proxy’si ile gider; `BACKEND_ORIGIN` bu yüzden kritiktir.

Backend `appsettings.json` çok-kiracılık örneği:

```
"MultiTenancy": {
  "DefaultTenantId": 1
}
=======

---

# MHRS – Merkezi Hastane Randevu Sistemi

Bu proje, **Türkiye’de kullanılan MHRS sistemini referans alarak** geliştirilmiş, uçtan uca çalışan bir **hastane randevu ve yönetim platformudur**.
Backend ve Frontend katmanları **tamamen ayrık (decoupled)** olup modern web mimarisi prensiplerine göre tasarlanmıştır.

**Amaç:**
Hastalar, doktorlar ve yöneticiler için randevu süreçlerini dijitalleştirmek, sağlık hizmetlerine erişimi kolaylaştırmak ve yönetilebilir bir sistem sunmak.

**Geliştirici:** Mahmut Sibal
**E-posta:** [mahmutsibal9@gmail.com](mailto:mahmutsibal9@gmail.com)

---

## 🧩 Sistem Mimarisi

Proje **Client–Server** mimarisine sahiptir.

```
[ Next.js Frontend ]  --->  [ .NET 8 Web API ]  --->  [ SQL Server ]
         |
         └── JWT ile kimlik doğrulama
```

* Frontend yalnızca API ile haberleşir
* Backend tüm iş kurallarını ve güvenliği yönetir
* Veritabanı erişimi yalnızca Backend üzerinden yapılır

---

## 📁 Proje Yapısı

```
MHRS/
│
├── WebAppointmentApi/
│   ├── WebAppointmentApi.WebApi
│   ├── WebAppointmentApi.Application
│   ├── WebAppointmentApi.Domain
│   └── WebAppointmentApi.Infrastructure
│
└── WebAppointment.Frontend/
    ├── app/
    ├── components/
    ├── services/
    └── styles/
```

---

## 🔧 Backend (WebAppointmentApi)

**Teknolojiler**

* .NET 8
* ASP.NET Core Web API
* Entity Framework Core
* SQL Server
* JWT Authentication
* Clean Architecture yaklaşımı

### Katmanlar

#### 1. Domain

* Entity’ler (User, Doctor, Patient, Appointment vb.)
* Enum ve temel kurallar
* Hiçbir framework bağımlılığı yoktur

#### 2. Application

* Business logic
* DTO’lar
* Service ve Interface tanımları
* Validation kuralları

#### 3. Infrastructure

* EF Core DbContext
* Repository implementasyonları
* Database migration’lar

#### 4. WebApi

* Controller’lar
* Auth & Authorization
* Middleware’ler
* Swagger yapılandırması

---

### 🔐 Kimlik Doğrulama & Yetkilendirme

* JWT (JSON Web Token) kullanılır
* Rol bazlı erişim:

  * **Admin**
  * **Doctor**
  * **Patient**

Örnek:

```http
Authorization: Bearer <token>
```

---

### 📌 Backend’i Çalıştırma

```bash
cd WebAppointmentApi
dotnet restore
dotnet ef database update
dotnet run --project WebAppointmentApi.WebApi
```

Varsayılan adres:

```
https://localhost:5001
```

Swagger:

```
https://localhost:5001/swagger
```

---

## 🎨 Frontend (WebAppointment.Frontend)

**Teknolojiler**

* Next.js 16 (App Router)
* React
* Tailwind CSS
* Axios
* JWT tabanlı Auth

---

### Frontend Yapısı

#### app/

* Route bazlı sayfalar
* Server & Client Components

#### components/

* Tekrar kullanılabilir UI bileşenleri
* Formlar, modal’lar, tablolar

#### services/

* API çağrıları
* Axios instance
* Token yönetimi

---

### 🧑‍⚕️ Kullanıcı Rollerine Göre Özellikler

#### Hasta

* Kayıt / giriş
* Doktor ve branş arama
* Randevu alma / iptal
* Randevu geçmişi

#### Doktor

* Günlük randevuları görüntüleme
* Uygunluk saatleri
* Hasta listesi

#### Admin

* Doktor / branş yönetimi
* Kullanıcı yönetimi
* Sistem kontrolü

---

### 📌 Frontend’i Çalıştırma

```bash
cd WebAppointment.Frontend
npm install
npm run dev
```

Varsayılan adres:

```
http://localhost:3000
>>>>>>> c196c44a62e0dc2280edecce0b3c570cf0a6dc15
```

---

<<<<<<< HEAD
## Hızlı Deneme Senaryoları

- **Hasta Kaydı & Giriş**
  1) `/register` üzerinden kayıt ol veya `/login` ile giriş yap
  2) `/patient/appointments/new` alanından hastane → bölüm → doktor → tarih/saat seçip randevu oluştur
- **Doktor Onayı**
  1) Doktor olarak giriş yap
  2) `/doctor/appointments` ekranından bekleyen randevuyu **Onayla** ardından **Tamamla**
- **Raporlar (Admin)**
  1) Admin olarak giriş yap
  2) `/admin/reports` ekranından filtreleri kullanarak en popüler doktorları gör

---

## Güvenlik Notları

- HttpOnly cookie’ler: `mhrs_at` (access), `mhrs_rt` (refresh)
- Role-based guard: Frontend middleware ile hassas route’lar korunur
- Rate limit: kullanıcı/IP bazlı kısıtlama
- ProblemDetails ile tutarlı hata formatı; frontend `apiJson` bunları kullanıcı dostu mesaja çevirir

---

## Sık Karşılaşılan Sorunlar

- "Bağlantı sağlanamıyor": SQL Server servisinin çalıştığını ve `DefaultConnection` değerinin doğru olduğunu doğrulayın.
- 401/403 hataları: Rol ve token geçerliliğini kontrol edin. Proxy route otomatik refresh dener.
- Saat dilimi: Frontend, randevu gönderiminde Türkiye saati için `+03:00` ekler; backend tüm tarihleri UTC olarak saklar.

---

## Lisans

Bu proje iç kullanım amacıyla sağlanmıştır. (Lisans bilgisi eklemek isterseniz bu bölümü güncelleyiniz.)
=======
## ⚙️ Ortam Değişkenleri

### Frontend (.env.local)

```env
NEXT_PUBLIC_API_URL=https://localhost:5001
```

---
>>>>>>> c196c44a62e0dc2280edecce0b3c570cf0a6dc15
