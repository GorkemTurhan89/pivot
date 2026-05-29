# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Proje Durumu

Proje henüz iskelet aşamasında — kod yok, sadece plan var. ASP.NET Core MVC ile yurt dışı eğitim danışmanlığı için bir teklif/sepet üretme uygulaması olarak başlayacak. Başlangıçta tek kişilik, local Visual Studio kullanımı hedefleniyor; release ileride gündeme gelecek.

## Mimari Genel Bakış

### Stack
- **ASP.NET Core MVC** (.NET 8 LTS, `net8.0` hedefi — Visual Studio sürümü .NET 10 desteklemediği için LTS seçildi)
- **Entity Framework Core 8.0.10** + **Pomelo.EntityFrameworkCore.MySql 8.0.2** (de-facto standart, Oracle'ın resmi provider'ından daha mature)
- **MySQL Server 8.0.46** (local)
- **PDF üretimi** için QuestPDF veya iTextSharp (sepet → teklif PDF'i — sonradan eklenecek)
- Veri girişi başlangıçta **mock data** (Data/SeedData.cs, Development modda startup'ta seed), sonradan Excel import (EPPlus/ClosedXML)

### Domain Modeli

Filtre akışı hiyerarşik: **Country → City → School → CourseProgram → StartDate → LengthWeeks → PaymentPlan seçimi**.

```
Country       (Id, Name)
City          (Id, Name, CountryId)
School        (Id, Name, CityId)
CourseProgram (Id, Name, SchoolId)   -- entity adı CourseProgram, çünkü "Program"
                                       Program.cs entry point class'ıyla çakışıyor.
                                       Property/UI hep "Program" olarak geçer.

PaymentPlan (Id, SchoolId, ProgramId?, LengthWeeks, ValidFrom, ValidTo,
             PackageType, Name, IsActive)
PaymentPlanItem (Id, PaymentPlanId, ItemName, ItemPrice, Currency, Quantity)
```

- `PaymentPlan` = filtreleme boyutları + ana metadata. `PackageType` enum: `Main` / `AddOn`.
- Main paketler bir `CourseProgram`'a bağlı (`ProgramId` zorunlu); AddOn paketler okul-bazlı (`ProgramId` null). Aynı okulda birden çok program (örn. "General English 20", "Intensive English 30") olur, her programın kendi fiyat planları olur.
- `PaymentPlanItem` = 1:N kalem-kalem fiyat dökümü (ders ücreti, kayıt, materyal vs.) — PDF'de tek tek gösterilir.
- Aynı program + aynı uzunluk farklı tarih aralıklarında ayrı `PaymentPlan` satırı olur (sezon ayrımı için ek tablo yok, `ValidFrom`/`ValidTo` yeterli).

### Kritik İş Kuralları

1. **Sezon sonu kontrolü (runtime filtre):** Kullanıcı başlangıç tarihi seçtiğinde sadece sezona sığan uzunluklar gösterilir:
   ```
   WHERE ValidFrom <= @startDate <= ValidTo
     AND DATEADD(week, LengthWeeks, @startDate) <= ValidTo
   ```
   Yani 24 haftalık plan, sezonun son 2 ayında başlatılmaya çalışılırsa listede çıkmaz.

2. **Sepet kalıcı değil:** Session/TempData'da tutulur, PDF üretildiğinde iş biter. Geçmiş sepet referans tutulmuyor — Excel re-import sırasında PaymentPlan ID'leri değişebilir, eski sepetler bozulsa sorun değil. İsteğe bağlı olarak ileride `GeneratedQuotes` arşiv tablosu eklenebilir (PDF binary + snapshot JSON).

3. **Excel import stratejisi (ileride):** Master tablolarda truncate+insert; PaymentPlan'da da truncate+insert (sepet kalıcı olmadığı için FK kopması yok). Manuel PaymentPlan girişi de mümkün olacak, Excel export ile geri alınabilecek.

### Kompleks Ek Paketler (Yurt vb.)

Basit add-on'lar (`PackageType=AddOn`) aynı `PaymentPlan` tablosunda kalır.

Yurt/konaklama gibi **kendi seçim kriterleri ve fiyatlandırması olan** ek paketler için ayrı entity gerekecek (örn. `Accommodation` + kendi item'ları). Bu modül init sonrasına ertelendi — basit akış oturduktan sonra eklenecek.

## Geliştirme

```
dotnet build
dotnet ef migrations add <Name>
dotnet ef database update
```

Uygulama Visual Studio'dan çalıştırılır (F5). `dotnet run` da çalışır ama kullanıcı Visual Studio'yu tercih ediyor.

MySQL bağlantı string'i `appsettings.Development.json` içinde tutulacak. Local MySQL server gerekir.

## Çalışma Tarzı Notları

- Kullanıcı Türkçe iletişim kuruyor; cevaplar ve kod yorumları Türkçe olabilir (kod kimliği, sınıf/metod isimleri İngilizce).
- Küçük adımlarla ilerle: önce iskelet + master tablolar + filtre akışı, sonra PaymentPlan/Item, sonra sepet, en son PDF ve Excel import.
- Karar verirken: "şimdilik basit, ileride genişletilebilir" tarafını tercih et. Yurt gibi kompleks add-on'lar ana akış oturana kadar bekleyebilir.
