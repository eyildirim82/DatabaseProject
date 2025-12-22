Harika. Verdiğiniz cevaplar, projenin "karanlık noktalarını" aydınlattı. Özellikle Transaction yönetimini Stored Procedure içinde (DB seviyesinde) tutma kararınız, akademik bir proje için en sağlam ve savunması en kolay yöntemdir. "Neden?" diye sorulursa; "Veri tutarlılığını uygulama katmanına güvenmeden, verinin kalbinde (DB) garanti altına almak istedik" dersiniz.

İşte cevaplarınıza göre son halini alan, tuzak sorulara karşı zırhlandırılmış Final SAD Dokümanı.

Bunu raporunuzun "4. System Architecture" bölümüne veya ayrı bir SAD.md dosyasına yapıştırabilirsiniz.

Software Architecture Document (SAD)
Project Name: DistributorFinanceDB Version: 1.1 (Final Release) Date: 22.12.2025

1. Giriş (Introduction)
1.1 Amaç (Purpose)
Bu doküman, DistributorFinanceDB projesinin mimari yapısını, teknoloji tercihlerini ve tasarım kararlarını detaylandırır. Dokümanın hedef kitlesi, projenin teknik bakımını üstlenecek geliştiriciler ve akademik değerlendirme kuruludur.

1.2 Mimari Vizyon (Architectural Vision)
Sistem, "Yalınlık ve Performans" prensipleri üzerine kurulmuştur. Karmaşık ORM (Object-Relational Mapping) katmanları yerine, Native ADO.NET kullanılarak veritabanı hakimiyeti maksimize edilmiş, iş mantığı (Business Logic) verinin kaynağına en yakın nokta olan SQL Server Stored Procedure'lerine taşınmıştır.

2. Mimari Temsil (Architectural Representation)
Sistem, Gevşek Bağlı (Loosely Coupled) N-Katmanlı bir yapıdadır. Katmanlar arası bağımlılıklar Dependency Injection (DI) ile yönetilir.

2.1 Teknoloji Yığını (Tech Stack)
Presentation Layer: ASP.NET Core MVC (.NET 8.0), Razor Views, Bootstrap 5.

Application Layer: C# Controller Logic, Action Filters (RoleCheck).

Data Access Layer (DAL): ADO.NET (SqlConnection, SqlCommand), Manual Mapping.

Database Layer: MS SQL Server 2019 (Stored Procedures, Triggers, Views).

3. Mantıksal Görünüm (Logical View)
3.1 Servis Yaşam Döngüsü (Service Lifecycle)
DAL sınıfları (TransactionDAL, CustomerDAL), bellek yönetimi ve bağlantı havuzu (connection pooling) verimliliği için Transient (Her istekte yeni nesne) yaşam döngüsü ile yapılandırılmıştır.

C#

// Program.cs Kaydı
builder.Services.AddTransient<TransactionDAL>();
builder.Services.AddTransient<CustomerDAL>();
3.2 Hata Yönetimi Stratejisi (Error Handling Strategy)
Controller Seviyesinde Yakalama: Global bir middleware yerine, her işlem kendi try-catch blokları ile izole edilmiştir.

Kullanıcı Geri Bildirimi: Yakalanan hatalar ViewBag.Error veya TempData["ErrorMessage"] üzerinden arayüze taşınır.

SQL Hata Yayılımı (Propagation): SQL Server tarafında iş kuralları ihlal edildiğinde (Örn: Risk Limiti Aşımı) RAISERROR ile fırlatılan hatalar, C# tarafında SqlException olarak yakalanır ve ex.Message özelliği doğrudan kullanıcıya gösterilir. Bu sayede iş kuralları tek bir merkezden (DB) yönetilir.

4. Süreç Görünümü (Process View)
4.1 Transaction Politikası (ACID Policy)
Veri bütünlüğü, uygulama katmanındaki kesintilerden (elektrik, network vb.) etkilenmemesi için Veritabanı Seviyesinde yönetilmektedir.

Yöntem: Kritik işlemler (Örn: sp_ProcessReconciliation), Stored Procedure içinde BEGIN TRANSACTION ... COMMIT/ROLLBACK blokları ile atomik olarak işlenir. C# tarafı sadece sonucu bekler.

4.2 Veri Eşleme (Data Mapping)
Performans darboğazlarını önlemek ve Reflection maliyetinden kaçınmak amacıyla, SqlDataReader'dan gelen veriler Manuel Eşleme (Manual Mapping) yöntemiyle C# modellerine dönüştürülür. AutoMapper gibi 3. parti kütüphaneler bilinçli olarak kullanılmamıştır.

5. Güvenlik Tasarımı (Security Design)
5.1 Uygulama Güvenliği
CSRF Koruması: Tüm POST formlarında ASP.NET Core'un @Html.AntiForgeryToken() mekanizması aktiftir.

XSS Koruması: Kullanıcıdan alınan serbest metinler (Örn: Açıklama alanları), Razor View Engine tarafından otomatik olarak encode edilerek ekrana basılır.

SQL Injection: DAL katmanında dinamik SQL birleştirme yasaklanmış, tüm veriler %100 SqlParameter kullanılarak sorgulanmıştır.

5.2 Yetkilendirme (Authorization)
Rol tabanlı erişim, Filters/RoleCheckAttribute.cs dosyası ile Aspect-Oriented (AOP) bir yaklaşımla sağlanmıştır. Controller metotlarının başına [RoleCheck(1)] yazılarak yetki kontrolü merkezi hale getirilmiştir.

6. Fiziksel Görünüm (Deployment View)
Web Sunucusu: IIS Express / Kestrel (Self-Hosted).

Veritabanı: LocalDB veya SQL Server Enterprise.

Konfigürasyon: Veritabanı bağlantı cümlesi (Connection String), appsettings.json dosyasında şifresiz olarak (Development ortamı için) saklanmaktadır.