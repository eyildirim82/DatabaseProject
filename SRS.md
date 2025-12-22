Software Requirements Specification (SRS)
Project Name: DistributorFinanceDB Version: 1.2 (Final Release) Date: 22.12.2025

1. Giriş (Introduction)
1.1 Amaç (Purpose)
Bu doküman, DistributorFinanceDB (Cari Risk ve Finans Yönetim Sistemi) projesinin yazılım gereksinimlerini tanımlar. Sistem, KOBİ ölçeğindeki dağıtım firmalarının nakit akışını, müşteri risklerini ve banka mutabakatlarını dijitalleştirmeyi amaçlar.

1.2 Kapsam (Scope)
Sistem, aşağıdaki temel modülleri içerir:

Cari Yönetim: Müşteri kartları ve bakiye takibi.

Finansal İşlemler: Tahsilat, tediye ve virman kayıtları.

Mutabakat (Reconciliation): Banka Excel ekstrelerinin sisteme işlenmesi.

Raporlama: Yönetici paneli (Dashboard) ve Muhasebe Entegrasyon çıktıları.

2. Genel Bakış (Overall Description)
2.1 Ürün Perspektifi (Product Perspective)
DistributorFinanceDB, kurumsal intranet üzerinde çalışan, merkezi bir MS SQL Server veritabanına bağlı bağımsız bir web uygulamasıdır. Harici bir donanım veya istemci tarafında MS Office kurulumu gerektirmez.

2.2 Kullanıcı Karakteristikleri (User Characteristics)
Admin: Sistem yapılandırması ve kullanıcı yönetimi.

Muhasebe: Günlük finansal işlem girişi ve Excel mutabakatı.

Satış Temsilcisi: Bakiye sorgulama ve müşteri notu ekleme.

2.3 Varsayımlar ve Kısıtlamalar (Assumptions & Constraints)
İstemci: Modern bir web tarayıcısı (Chrome, Edge, Firefox) yeterlidir.

Bağımlılık: Sunucu tarafında .NET 8.0 Runtime ve SQL Server 2019+ gereklidir.

Dil: Kullanıcı arayüzü dili Türkçedir.

3. Sistem Özellikleri (Functional Requirements)
3.1 FR-01: Cari Hesap ve Risk Yönetimi
Gereksinim: Sistem, işlem anında müşterinin güncel bakiyesini ve tanımlı risk limitini kontrol etmelidir.

İş Kuralı: Risk limitini aşan vadeli işlem girişleri (INSERT) veritabanı seviyesinde engellenmeli ve kullanıcıya hata mesajı gösterilmelidir.


Teknik Karşılık: sp_AddChequeWithRiskCheck ve CK_RiskLimit_Positive .


3.2 FR-02: Veri Bütünlüğü ve Silme Politikası (Strict Policy)
Gereksinim: Finansal tutarlılığı korumak adına, işlem görmüş kayıtların silinmesi engellenmelidir.

İş Kuralı (Strict): Eğer bir müşteriye ait Transactions veya Cheques tablosunda en az bir kayıt varsa, o müşteri kartı asla silinemez. Sistem bu durumda Foreign Key hatası veya özel bir iş kuralı hatası fırlatmalıdır.

Teknik Karşılık: sp_DeleteCustomer prosedürü içindeki EXISTS kontrolleri.

3.3 FR-03: Excel Banka Mutabakatı (Reconciliation)
Gereksinim: Kullanıcılar, banka formatındaki .xlsx dosyalarını sisteme yükleyebilmelidir.

İş Kuralı: Sistem, yüklenen dosyadaki verileri Regex ile ayrıştırmalı, mevcut bakiyelerle karşılaştırmalı ve farkları raporlamalıdır. İstemci bilgisayarda Excel yüklü olma zorunluluğu yoktur.

Teknik Karşılık: ClosedXML kütüphanesi ve ImportBatches tablosu.

3.4 FR-04: Ters Kayıt (Reverse Entry) Mekanizması
Gereksinim: Hatalı girilen finansal işlemler fiziksel olarak silinmemeli, muhasebe standartlarına uygun olarak ters kayıtla nötrlenmelidir.

İş Kuralı: İptal edilen işlemin tutarı negatif (-) işaretli olarak yeni bir kayıt şeklinde eklenir.

4. Dış Arayüz Gereksinimleri (External Interface Requirements)
4.1 Kullanıcı Arayüzleri
Sistem, HTML5 ve Bootstrap 5 standartlarına uygun, sade ve odaklı bir web arayüzü sunacaktır.

Mobil uyumluluk (Responsive Design) desteklenecektir ancak öncelikli kullanım masaüstü tarayıcılardır.

4.2 Yazılım Arayüzleri
Veritabanı İletişimi: Tüm veri işlemleri ADO.NET ve Stored Procedures üzerinden yürütülecektir. ORM (Entity Framework) kullanılmayarak ham SQL performansı hedeflenmiştir.

Dışa Aktarım: Sistem, muhasebe programları için .xlsx formatında veri üretecektir. PDF formatında ekstre çıktısı kapsam dışıdır.

5. Kalite Nitelikleri (Non-Functional Requirements)
5.1 Performans
Sistem, 5 eşzamanlı kullanıcının veri girişini <2 saniye tepki süresi ile karşılamalıdır.

Excel import işlemlerinde 1000 satıra kadar olan dosyalar <30 saniye içinde işlenmelidir.

5.2 Güvenlik
Şifreler veritabanında SHA-256 algoritması ile hashlenmiş olarak saklanacaktır.

SQL Injection saldırılarına karşı tüm parametreler SqlParameter nesneleri ile kapsüllenecektir.

6. Tasarım Kısıtlamaları (Design Constraints)
Proje, aşağıdaki teknoloji yığınına sadık kalarak geliştirilmiştir:

Backend: ASP.NET Core (.NET 8.0)

Veritabanı: Microsoft SQL Server 2019

Frontend: HTML5, CSS3, JavaScript (jQuery), Bootstrap 5

Veri Erişim: ADO.NET (Native SQL)

Kütüphaneler: ClosedXML (Excel İşlemleri)