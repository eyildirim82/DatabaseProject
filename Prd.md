Product Requirements Document (PRD)

----------------------------------------------------------------------
Proje Adı      : DistributorFinanceDB
Versiyon       : 1.0
Durum          : Final Release
Tarih          : 22.12.2025
----------------------------------------------------------------------

1. Yönetici Özeti (Executive Summary)
--------------------------------------

1.1 Problem Tanımı (The "Pain")
Endüstriyel dağıtım sektöründe faaliyet gösteren firmalarda, finansal takip süreçlerinde ciddi "sürtünmeler" yaşanmaktadır. Mevcut manuel sistemlerde ortaya çıkan başlıca problemler:

- **Güvensizlik:** Müşterilerin "Cuma günü ödeyeceğim" gibi sözlü vaatleri takip edilemiyor, tahsilat gecikmeleri yaşanıyor.
- **Kayıp:** Vadeli çeklerin takibi yapılamıyor, vadesi gelen çekler gözden kaçıyor.
- **Verimsizlik:** Banka hareketleri ve tahsilatlar tek tek elle giriliyor, bu da operasyonel yük oluşturuyor.

1.2 Çözüm ve Değer Önermesi (Value Proposition)
DistributorFinanceDB; müşteri riskini, çek portföyünü ve nakit akışını tek merkezden yöneten; "Toplu Tahsilat İşleme" (Batch Entry) ile operasyonel süreyi dakikalara indiren bir finansal yönetim veritabanıdır.

2. Hedef Kitle (Target Audience)
--------------------------------

Bu proje, akademik bir çalışma olup iki ana hedef kitlesi vardır:

2.1 Birincil Kitle (Akademik Değerlendirme)
- **Ders Yürütücüsü (Instructor/Grader):** Projenin veritabanı normalizasyonu (3NF), SQL yetenekleri (Stored Procedure, Trigger), arayüz işlevselliği ve kodun açıklanabilirliğine odaklanır.
- **Beklenti:** Sistemin hatasız çalışması ve kodun açıklanabilir olması.

2.2 İkincil Kitle (İş Simülasyonu)
- **Muhasebe Personeli:** Excel'den toplu veri aktarımı yapan, hata yapmaktan çekinen ofis çalışanı.
- **Saha Satış Temsilcisi:** Müşteriye mal vermeden önce geçmiş notlara/durumlara bakmak isteyen personel.

3. Kullanıcı Hikayeleri ve Öncelikler (User Stories)
----------------------------------------------------

| ID     | Rol             | İstek (I want to...)                       | Amaç (So that...)                                    | Öncelik | Teknik Karşılık               |
|--------|-----------------|--------------------------------------------|------------------------------------------------------|---------|------------------------------|
| US-01  | Muhasebe        | Excel ekstresini sisteme yüklemek          | Tek tek veri girmekle uğraşmadan toplu tahsilat      | P0      | ImportBatches, Regex Parser  |
| US-02  | Yönetici        | Müşteri risk durumunu anlık görmek         | Limit aşımı olan müşteriye mal verilmesini engellemek| P0      | sp_AddChequeWithRiskCheck    |
| US-03  | Satış Temsilcisi| Müşteri görüşme notlarını kaydetmek        | Sözlerin takibi ve uyarı sistemi                     | P1      | CollectionNotes Table         |
| US-04  | Muhasebe        | Hatalı işlemi iptal etmek (Reverse)        | Silmeden, muhasebe kurallarına uygun düzeltme        | P0      | sp_ReverseTransaction         |
| US-05  | Muhasebe        | Çek durumunu güncellemek (Tahsil/Karşılıksız)| Müşteri bakiyesinin otomatik güncellendiğini görmek | P0      | trg_UpdateBalance             |

4. Başarı Metrikleri (Success Metrics)
--------------------------------------

4.1 Akademik Başarı (Primary Goal)
- **Gereksinim Karşılama:** CSE3055 Proje dökümanındaki şartların (8 tablo, 4 view, 1 trigger, 8 SP) eksiksiz sağlanması.
- **Demo Performansı:** Demo sırasında sistemin uçtan uca (E2E) senaryoyu hatasız tamamlaması.
- **Açıklanabilirlik:** Grup üyelerinin teknik seçimi (ör. Trigger) savunabilmesi.

4.2 İş Simülasyonu Başarısı
- **Veri Bütünlüğü:** Müşteri bakiyelerinin işlemlerle birebir tutması (consistency).
- **Hız:** Excel import ile haftalık 500 satırlık banka hareketinin 1 dakikadan kısa sürede işlenmesi.

5. Yol Haritası ve Kapsam Sınırları (Roadmap & Constraints)
-----------------------------------------------------------

**✅ Kapsamda (MVP):**
- Login / Logout (Rol Tabanlı Güvenlik)
- Müşteri Kartı ve Bakiye Görüntüleme
- Tekil İşlem Girişi (Tahsilat/Ödeme)
- Excel Toplu Veri Yükleme (Fark Yaratan Özellik)
- Müşteri Notları (CRM Lite)
- Muhasebe Entegrasyon Raporu (Excel Çıktısı)

**❌ Kapsam Dışı (v2.0):**
- Detaylı Stok ve Ürün Yönetimi (karmaşıklığı önlemek için)
- Fatura Dizaynı ve Yazdırma
- Email/SMS Bildirim Servisleri
- Mobil Uygulama

---

💡 **Demo İçin "Açıklanabilirlik" İpuçları**
- **Soru:** "Neden Excel Import yaptınız? Elle girselerdi?"
  - **Cevap:** Gerçek hayatta binlerce satırlık ekstre geliyor. Toplu veri işleme yeteneğimiz ve insan hatasını engellemek için yaptık.
- **Soru:** "Neden işlemi silmek yerine 'Ters Kayıt' (Reverse) atıyorsunuz?"
  - **Cevap:** Audit Trail çok önemli. Silersek hile yapılabilir. Doğru olan finansal yöntem, ters kayıtla nötrlemektir.
- **Soru:** "CRM Notları neden var?"
  - **Cevap:** Veritabanı yalnızca sayıları değil, ilişkileri de tutmalı. "Verilen Sözler" işin sosyal boyutunu veriye dönüştürüyor.
