
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
```

---

## ⚙️ Ortam Değişkenleri

### Frontend (.env.local)

```env
NEXT_PUBLIC_API_URL=https://localhost:5001
```

---
