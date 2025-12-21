# ImportBatch Modülü - Kritik Özellikler Test Planı

## Test Kapsamı

Bu dokümantasyon, ImportBatch modülünün kritik özelliklerinin manuel test edilmesi için hazırlanmıştır. Test edilecek kritik özellikler:

1. **CRUD İşlemleri**
   - Batch oluşturma (Excel import)
   - Batch okuma (Liste ve detay görüntüleme)
   - Batch durumu güncelleme
   - Batch silme

2. **Export İşlemleri**
   - Tüm batch'lerin export edilmesi (CSV/Excel)
   - Belirli batch detaylarının export edilmesi (CSV/Excel)

---

## 1. Test Ortamı Hazırlığı

### 1.1 Veritabanı Bağlantı Kontrolü

**Adımlar:**
1. `appsettings.json` dosyasında connection string'in doğru olduğunu kontrol edin
2. SQL Server Management Studio ile veritabanına bağlanın
3. `ImportBatches` ve `ImportDetails` tablolarının mevcut olduğunu doğrulayın
4. Stored procedure'ların mevcut olduğunu kontrol edin:
   - `sp_ValidateAndCreateBatch`
   - `sp_ProcessReconciliation`

**Beklenen Sonuç:**
- Veritabanı bağlantısı başarılı
- Tüm tablolar ve stored procedure'lar mevcut

### 1.2 Test Verisi Hazırlığı

**Gerekli Test Dosyaları:**
1. **Geçerli Excel Dosyası:**
   - Dosya adı formatı: `TopluCariEkstreRaporu_20251215142935.xlsx`
   - İçerik: Cari kodları, müşteri isimleri ve bakiyeler içeren geçerli Excel dosyası
   - En az 5-10 satır veri içermeli

2. **Geçersiz Dosya Formatları:**
   - `.pdf` dosyası (desteklenmeyen format)
   - `.docx` dosyası (desteklenmeyen format)
   - Geçersiz tarih formatı içeren dosya adı

3. **Boş Dosya:**
   - İçeriği boş olan `.xlsx` dosyası

**Veritabanı Test Verileri:**
- En az 3 farklı status'e sahip batch kayıtları (Pending, Processed, Rejected)
- Her batch için en az 3-5 ImportDetail kaydı

### 1.3 Proje Derleme ve Çalıştırma

**Adımlar:**
1. Terminal'de proje klasörüne gidin
2. Projeyi derleyin: `dotnet build`
3. Derleme hatalarını kontrol edin
4. Projeyi çalıştırın: `dotnet run`
5. Tarayıcıda `https://localhost:5001` veya `http://localhost:5000` adresine gidin
6. Login sayfasından giriş yapın

**Beklenen Sonuç:**
- Proje hatasız derlenir
- Uygulama başarıyla çalışır
- Login sayfası açılır

---

## 2. Test Senaryoları Detayları

### TS-01: Excel Import (Batch Oluşturma)

**Test ID:** TS-01  
**Test Adı:** Excel Import - Batch Oluşturma  
**Öncelik:** Yüksek  
**Önkoşullar:**
- Uygulama çalışıyor
- Kullanıcı giriş yapmış
- ImportExcel sayfasına erişilebiliyor

#### TS-01.1: Geçerli Excel Dosyası Yükleme

**Test Adımları:**
1. ImportExcel sayfasına gidin (`/Customer/ImportExcel`)
2. "Excel Dosyası Seçin" input'una tıklayın
3. Geçerli formatlı Excel dosyasını seçin (`TopluCariEkstreRaporu_20251215142935.xlsx`)
4. "Yükle" butonuna tıklayın
5. Sayfanın yenilendiğini ve başarı mesajının göründüğünü kontrol edin
6. Batch listesinde yeni batch'in göründüğünü doğrulayın

**Beklenen Sonuç:**
- Dosya başarıyla yüklenir
- Yeşil başarı mesajı görünür: "Dosya başarıyla yüklendi! Batch ID: X, Toplam Kayıt: Y"
- Yeni batch ImportExcel sayfasındaki listede görünür
- Batch durumu "Pending" olarak ayarlanır
- Veritabanında `ImportBatches` tablosunda yeni kayıt oluşur

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

#### TS-01.2: Geçersiz Dosya Formatı Kontrolü

**Test Adımları:**
1. ImportExcel sayfasına gidin
2. Desteklenmeyen bir dosya formatı seçin (örn: `.pdf`, `.docx`)
3. "Yükle" butonuna tıklayın

**Beklenen Sonuç:**
- Kırmızı hata mesajı görünür: "Sadece .txt, .xlsx, .xls ve .csv dosyaları yüklenebilir."
- Dosya yüklenmez
- Batch oluşturulmaz

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

#### TS-01.3: Boş Dosya Kontrolü

**Test Adımları:**
1. ImportExcel sayfasına gidin
2. Boş bir Excel dosyası seçin
3. "Yükle" butonuna tıklayın

**Beklenen Sonuç:**
- Dosya yüklenir ancak TotalRecords = 0 olarak kaydedilir
- Veya uygun bir hata mesajı gösterilir

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

#### TS-01.4: Geçersiz Tarih Formatı Kontrolü

**Test Adımları:**
1. ImportExcel sayfasına gidin
2. Dosya adında geçerli tarih formatı olmayan bir Excel dosyası seçin (örn: `dosya.xlsx`)
3. "Yükle" butonuna tıklayın

**Beklenen Sonuç:**
- Kırmızı hata mesajı görünür: "Dosya isminde geçerli bir tarih formatı bulunamadı (Örn: TopluCariEkstreRaporu_20251215142935.xlsx)."
- Dosya yüklenmez
- Batch oluşturulmaz

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

#### TS-01.5: Dosya Seçilmeden Yükleme Denemesi

**Test Adımları:**
1. ImportExcel sayfasına gidin
2. Hiçbir dosya seçmeden "Yükle" butonuna tıklayın

**Beklenen Sonuç:**
- Kırmızı hata mesajı görünür: "Lütfen bir dosya seçin."
- Veya HTML5 validation mesajı gösterilir

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

---

### TS-02: ImportExcel Sayfası Görüntüleme

**Test ID:** TS-02  
**Test Adı:** ImportExcel Sayfası Görüntüleme  
**Öncelik:** Yüksek  
**Önkoşullar:**
- Uygulama çalışıyor
- Kullanıcı giriş yapmış
- Veritabanında en az 1 batch kaydı var

#### TS-02.1: Sayfa Açılma Kontrolü

**Test Adımları:**
1. Tarayıcıda `/Customer/ImportExcel` adresine gidin

**Beklenen Sonuç:**
- Sayfa hatasız açılır
- Sayfa başlığı "Excel Mutabakat Modülü" görünür
- Dosya yükleme formu görünür
- Batch listesi görünür

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

#### TS-02.2: İstatistik Kartlarının Görüntülenmesi

**Test Adımları:**
1. ImportExcel sayfasına gidin
2. Sayfanın üst kısmındaki istatistik kartlarını kontrol edin

**Beklenen Sonuç:**
- Toplam Batch sayısı görünür
- Processed batch sayısı görünür
- Pending batch sayısı görünür
- Rejected batch sayısı görünür
- Toplam kayıt sayısı görünür
- Son yükleme tarihi görünür

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

#### TS-02.3: Batch Listesinin Görüntülenmesi

**Test Adımları:**
1. ImportExcel sayfasına gidin
2. Batch listesi tablosunu kontrol edin

**Beklenen Sonuç:**
- Tablo başlıkları görünür: Batch ID, Dosya Adı, Dosya Tarihi, Yükleme Tarihi, Kayıt Sayısı, Durum, İşlemler
- Batch kayıtları tabloda listelenir
- Her batch için Batch ID tıklanabilir link olarak görünür
- Her batch için İptal ve Sil butonları görünür

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

#### TS-02.4: Hata Durumunda Boş Liste Gösterimi

**Test Adımları:**
1. Veritabanı bağlantısını geçici olarak kesin (veya hata simüle edin)
2. ImportExcel sayfasına gidin

**Beklenen Sonuç:**
- Hata mesajı görünür
- Boş liste gösterilir (hatasız)
- İstatistikler varsayılan değerlerle gösterilir

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

---

### TS-03: Batch Detayları Görüntüleme

**Test ID:** TS-03  
**Test Adı:** Batch Detayları Görüntüleme  
**Öncelik:** Yüksek  
**Önkoşullar:**
- Uygulama çalışıyor
- Kullanıcı giriş yapmış
- Veritabanında en az 1 batch kaydı var
- Batch'in en az 1 ImportDetail kaydı var

#### TS-03.1: Batch ID'ye Tıklama

**Test Adımları:**
1. ImportExcel sayfasına gidin
2. Bir batch'in Batch ID'sine tıklayın

**Beklenen Sonuç:**
- BatchDetails sayfasına yönlendirilir
- URL: `/Customer/BatchDetails?id=X` formatında

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

#### TS-03.2: Detay Sayfası Açılma

**Test Adımları:**
1. BatchDetails sayfasına gidin

**Beklenen Sonuç:**
- Sayfa başlığı "Batch Detayları" görünür
- Batch bilgileri kartı görünür (Batch ID, Dosya Adı, Dosya Tarihi, Yükleme Tarihi, Kayıt Sayısı, Durum)
- Detay tablosu görünür
- Geri Dön butonu görünür
- Export butonları (Excel ve CSV) görünür

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

#### TS-03.3: ImportDetail Listesinin Görüntülenmesi

**Test Adımları:**
1. BatchDetails sayfasına gidin
2. Detay tablosunu kontrol edin

**Beklenen Sonuç:**
- Tablo başlıkları görünür: Hesap Kodu, Tespit Edilen İsim, Excel Bakiyesi, Sistem Bakiyesi, Fark
- Tüm ImportDetail kayıtları listelenir
- Her satırda AccountCode, DetectedName, ExcelBalance, SystemBalanceAtTime görünür
- Tablo altında toplam satırı görünür

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

#### TS-03.4: BalanceDifference Hesaplaması

**Test Adımları:**
1. BatchDetails sayfasına gidin
2. Fark sütunundaki değerleri kontrol edin
3. Excel Bakiyesi ve Sistem Bakiyesi değerlerini manuel olarak karşılaştırın

**Beklenen Sonuç:**
- Fark = ExcelBalance - SystemBalanceAtTime olarak hesaplanır
- Fark pozitifse kırmızı, negatifse mavi, sıfırsa yeşil renkte gösterilir
- Toplam satırında toplam fark doğru hesaplanır

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

#### TS-03.5: Geçersiz Batch ID Kontrolü

**Test Adımları:**
1. Tarayıcıda `/Customer/BatchDetails?id=99999` gibi geçersiz bir ID ile sayfaya gidin

**Beklenen Sonuç:**
- Kırmızı hata mesajı görünür: "Batch bulunamadı."
- ImportExcel sayfasına yönlendirilir

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

---

### TS-04: Batch Durumu Güncelleme

**Test ID:** TS-04  
**Test Adı:** Batch Durumu Güncelleme  
**Öncelik:** Yüksek  
**Önkoşullar:**
- Uygulama çalışıyor
- Kullanıcı giriş yapmış
- Veritabanında en az 1 "Pending" durumunda batch var

#### TS-04.1: Status Dropdown'dan Seçim

**Test Adımları:**
1. ImportExcel sayfasına gidin
2. Bir batch'in "İşlemler" sütunundaki dropdown'u bulun
3. Dropdown'dan farklı bir status seçin (örn: Pending → Processed)

**Beklenen Sonuç:**
- Dropdown görünür ve tıklanabilir
- Status seçenekleri görünür: Pending, Processed, Rejected
- Seçim yapılabilir

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

#### TS-04.2: UpdateBatchStatus Action Çağrısı

**Test Adımları:**
1. ImportExcel sayfasına gidin
2. Bir batch'in status'unu değiştirin
3. Form submit edildiğinde sayfanın yenilendiğini kontrol edin

**Beklenen Sonuç:**
- Status değişikliği POST request ile gönderilir
- Sayfa yenilenir
- Başarı mesajı görünür: "Batch durumu başarıyla güncellendi."
- Batch'in yeni status'u listede görünür

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

#### TS-04.3: Başarı Mesajı Kontrolü

**Test Adımları:**
1. Başarılı bir status güncellemesi yapın
2. Sayfanın üst kısmındaki mesajı kontrol edin

**Beklenen Sonuç:**
- Yeşil alert kutusu görünür
- Mesaj: "Batch durumu başarıyla güncellendi."
- Mesaj kapatılabilir (X butonu)

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

#### TS-04.4: Hata Durumu Kontrolü

**Test Adımları:**
1. Geçersiz bir batch ID ile status güncelleme denemesi yapın (manuel olarak URL'yi değiştirerek)

**Beklenen Sonuç:**
- Kırmızı hata mesajı görünür: "Batch durumu güncellenemedi."
- Veya: "Hata: [hata mesajı]"

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

#### TS-04.5: Veritabanında Güncelleme Kontrolü

**Test Adımları:**
1. Bir batch'in status'unu güncelleyin
2. SQL Server Management Studio'da `ImportBatches` tablosunu kontrol edin
3. İlgili batch'in `Status` sütununun güncellendiğini doğrulayın

**Beklenen Sonuç:**
- Veritabanında batch'in Status değeri güncellenir
- Güncelleme zamanı doğru kaydedilir

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

---

### TS-05: Batch Silme

**Test ID:** TS-05  
**Test Adı:** Batch Silme  
**Öncelik:** Yüksek  
**Önkoşullar:**
- Uygulama çalışıyor
- Kullanıcı giriş yapmış
- Veritabanında silinebilecek en az 1 batch var
- Batch'in ImportDetails kayıtları var

#### TS-05.1: Sil Butonuna Tıklama

**Test Adımları:**
1. ImportExcel sayfasına gidin
2. Bir batch'in "Sil" butonuna tıklayın

**Beklenen Sonuç:**
- JavaScript onay mesajı görünür: "Bu batch'i silmek istediğinizden emin misiniz?"
- Onay verilirse silme işlemi başlar
- İptal edilirse işlem iptal olur

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

#### TS-05.2: Onay Mekanizması Kontrolü

**Test Adımları:**
1. Sil butonuna tıklayın
2. Onay mesajında "İptal" seçeneğini seçin

**Beklenen Sonuç:**
- Batch silinmez
- Sayfa yenilenmez
- Batch listede görünmeye devam eder

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

#### TS-05.3: DeleteBatch Action Çağrısı

**Test Adımları:**
1. Sil butonuna tıklayın
2. Onay mesajında "Tamam" seçeneğini seçin
3. Sayfanın yenilendiğini kontrol edin

**Beklenen Sonuç:**
- POST request gönderilir
- Sayfa yenilenir
- Başarı mesajı görünür: "Batch başarıyla silindi."
- Batch listeden kaldırılır

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

#### TS-05.4: Başarı Mesajı Kontrolü

**Test Adımları:**
1. Başarılı bir silme işlemi yapın
2. Sayfanın üst kısmındaki mesajı kontrol edin

**Beklenen Sonuç:**
- Yeşil alert kutusu görünür
- Mesaj: "Batch başarıyla silindi."
- Mesaj kapatılabilir

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

#### TS-05.5: ImportDetails Kayıtlarının da Silinmesi Kontrolü

**Test Adımları:**
1. Bir batch'i silin
2. SQL Server Management Studio'da `ImportDetails` tablosunu kontrol edin
3. Silinen batch'e ait ImportDetails kayıtlarının da silindiğini doğrulayın

**Beklenen Sonuç:**
- Batch silindiğinde, o batch'e ait tüm ImportDetails kayıtları da silinir
- Veritabanında orphan kayıt kalmaz

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

#### TS-05.6: Transaction Bütünlüğü Kontrolü

**Test Adımları:**
1. Bir batch'i silmeye çalışın
2. Silme işlemi sırasında bir hata oluşmasını simüle edin (örn: veritabanı bağlantısını kesin)

**Beklenen Sonuç:**
- Transaction rollback yapılır
- Ne batch ne de ImportDetails kayıtları silinir
- Hata mesajı gösterilir

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

---

### TS-06: Export History (Tüm Batch'ler)

**Test ID:** TS-06  
**Test Adı:** Export History - Tüm Batch'ler  
**Öncelik:** Orta  
**Önkoşullar:**
- Uygulama çalışıyor
- Kullanıcı giriş yapmış
- Veritabanında en az 1 batch kaydı var

#### TS-06.1: CSV Export Butonu

**Test Adımları:**
1. ImportExcel sayfasına gidin
2. "CSV İndir" butonuna tıklayın

**Beklenen Sonuç:**
- CSV dosyası indirilir
- Dosya adı: `ImportHistory_yyyyMMddHHmmss.csv` formatında
- Dosya içeriği UTF-8 encoding ile

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

#### TS-06.2: Excel Export Butonu

**Test Adımları:**
1. ImportExcel sayfasına gidin
2. "Excel İndir" butonuna tıklayın

**Beklenen Sonuç:**
- Excel dosyası indirilir
- Dosya adı: `ImportHistory_yyyyMMddHHmmss.xlsx` formatında
- Dosya açılabilir ve okunabilir

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

#### TS-06.3: Filtrelenmiş Verilerin Export Edilmesi

**Test Adımları:**
1. ImportExcel sayfasına gidin
2. Status filtresinden "Processed" seçin
3. "Filtrele" butonuna tıklayın
4. "Excel İndir" veya "CSV İndir" butonuna tıklayın

**Beklenen Sonuç:**
- Sadece filtrelenmiş batch'ler export edilir
- Export edilen dosyada sadece "Processed" status'ündeki batch'ler görünür

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

#### TS-06.4: Dosya İndirme Kontrolü

**Test Adımları:**
1. Export işlemi yapın
2. İndirilen dosyayı kontrol edin

**Beklenen Sonuç:**
- Dosya başarıyla indirilir
- Dosya boyutu 0'dan büyüktür
- Dosya açılabilir

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

#### TS-06.5: Dosya İçeriği Doğrulama

**Test Adımları:**
1. Export edilen Excel dosyasını açın
2. İçeriği kontrol edin

**Beklenen Sonuç:**
- Başlık satırı görünür: Batch ID, Dosya Adı, Dosya Tarihi, Yükleme Tarihi, Kayıt Sayısı, Durum
- Tüm batch kayıtları görünür
- Veriler doğru formatta görünür
- Tarihler doğru formatlanmıştır

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

---

### TS-07: Export Batch Details

**Test ID:** TS-07  
**Test Adı:** Export Batch Details  
**Öncelik:** Orta  
**Önkoşullar:**
- Uygulama çalışıyor
- Kullanıcı giriş yapmış
- Veritabanında en az 1 batch var
- Batch'in en az 1 ImportDetail kaydı var

#### TS-07.1: Belirli Batch için CSV Export

**Test Adımları:**
1. BatchDetails sayfasına gidin
2. "CSV İndir" butonuna tıklayın

**Beklenen Sonuç:**
- CSV dosyası indirilir
- Dosya adı: `Batch_X_Detaylar_yyyyMMddHHmmss.csv` formatında
- Dosya içeriği UTF-8 encoding ile

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

#### TS-07.2: Belirli Batch için Excel Export

**Test Adımları:**
1. BatchDetails sayfasına gidin
2. "Excel İndir" butonuna tıklayın

**Beklenen Sonuç:**
- Excel dosyası indirilir
- Dosya adı: `Batch_X_Detaylar_yyyyMMddHHmmss.xlsx` formatında
- Dosya açılabilir ve okunabilir

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

#### TS-07.3: Dosya İndirme Kontrolü

**Test Adımları:**
1. Export işlemi yapın
2. İndirilen dosyayı kontrol edin

**Beklenen Sonuç:**
- Dosya başarıyla indirilir
- Dosya boyutu 0'dan büyüktür
- Dosya açılabilir

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

#### TS-07.4: Detay Verilerinin Doğruluğu

**Test Adımları:**
1. Export edilen Excel dosyasını açın
2. İçeriği kontrol edin

**Beklenen Sonuç:**
- Batch bilgileri görünür (Batch ID, Dosya Adı, Dosya Tarihi, Yükleme Tarihi, Durum)
- Detay başlıkları görünür: Hesap Kodu, Tespit Edilen İsim, Excel Bakiyesi, Sistem Bakiyesi, Fark
- Tüm ImportDetail kayıtları görünür
- Toplam satırı görünür ve doğru hesaplanmıştır
- Veriler web sayfasındakiyle aynıdır

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

---

## 3. Test Checklist

### Genel Kontroller

- [ ] Tüm test senaryoları tamamlandı
- [ ] Beklenen sonuçlar doğrulandı
- [ ] Hata durumları test edildi
- [ ] Veritabanı değişiklikleri kontrol edildi
- [ ] UI/UX kontrolleri yapıldı
- [ ] Performans gözlemlendi

### CRUD İşlemleri

- [ ] Batch oluşturma (Excel import) test edildi
- [ ] Batch okuma (Liste görüntüleme) test edildi
- [ ] Batch okuma (Detay görüntüleme) test edildi
- [ ] Batch durumu güncelleme test edildi
- [ ] Batch silme test edildi

### Export İşlemleri

- [ ] Tüm batch'lerin CSV export'u test edildi
- [ ] Tüm batch'lerin Excel export'u test edildi
- [ ] Belirli batch'in CSV export'u test edildi
- [ ] Belirli batch'in Excel export'u test edildi
- [ ] Filtrelenmiş verilerin export'u test edildi

### Hata Senaryoları

- [ ] Geçersiz dosya formatı test edildi
- [ ] Geçersiz tarih formatı test edildi
- [ ] Boş dosya test edildi
- [ ] Geçersiz Batch ID test edildi
- [ ] Veritabanı bağlantı hatası simüle edildi

---

## 4. Hata Senaryoları Testleri

### H-01: Geçersiz Batch ID ile İşlem Yapma

**Test Senaryosu:**
- Geçersiz bir Batch ID (örn: 99999) ile BatchDetails sayfasına erişim
- Geçersiz Batch ID ile status güncelleme
- Geçersiz Batch ID ile batch silme

**Beklenen Sonuç:**
- Uygun hata mesajları gösterilir
- Sistem çökmez
- Kullanıcı ImportExcel sayfasına yönlendirilir

### H-02: Boş/Null Parametrelerle İşlem Yapma

**Test Senaryosu:**
- Null dosya ile import denemesi
- Boş status ile güncelleme denemesi
- Null batchId ile silme denemesi

**Beklenen Sonuç:**
- Validation hataları gösterilir
- İşlem gerçekleştirilmez
- Uygun hata mesajları gösterilir

### H-03: Veritabanı Bağlantı Hatası Simülasyonu

**Test Senaryosu:**
- Veritabanı bağlantısını kesin
- ImportExcel sayfasına erişim denemesi
- Batch işlemleri yapma denemesi

**Beklenen Sonuç:**
- Hata mesajları gösterilir
- Sistem çökmez
- Kullanıcıya anlamlı hata mesajları gösterilir

### H-04: Büyük Dosya Yükleme Testi

**Test Senaryosu:**
- 10MB+ büyüklüğünde Excel dosyası yükleme
- 1000+ satır içeren Excel dosyası yükleme

**Beklenen Sonuç:**
- Dosya başarıyla yüklenir veya uygun hata mesajı gösterilir
- Performans kabul edilebilir seviyededir
- Timeout hatası oluşmaz

### H-05: Eşzamanlı İşlem Testi

**Test Senaryosu:**
- Aynı batch'i iki farklı tarayıcı sekmesinde aynı anda güncelleme
- Aynı batch'i iki farklı kullanıcı aynı anda silme

**Beklenen Sonuç:**
- Race condition oluşmaz
- Veri tutarlılığı korunur
- Uygun hata mesajları gösterilir

---

## 5. Performans Testleri

### P-01: Büyük Veri Seti ile Sayfalama Testi

**Test Senaryosu:**
- Veritabanında 100+ batch kaydı oluşturun
- ImportExcel sayfasında sayfalama kontrollerini test edin
- Farklı pageSize değerleri deneyin (10, 25, 50, 100)

**Beklenen Sonuç:**
- Sayfa yükleme süresi < 3 saniye
- Sayfalama kontrolleri çalışır
- Performans kabul edilebilir seviyededir

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

### P-02: Filtreleme Performansı

**Test Senaryosu:**
- Veritabanında 100+ batch kaydı oluşturun
- Farklı filtrelerle arama yapın (status, dosya adı, tarih aralığı)
- Filtreleme süresini ölçün

**Beklenen Sonuç:**
- Filtreleme süresi < 2 saniye
- Sonuçlar doğru döner
- Performans kabul edilebilir seviyededir

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

### P-03: Export İşlemi Performansı (1000+ Kayıt)

**Test Senaryosu:**
- Veritabanında 1000+ batch kaydı oluşturun
- Export History işlemini başlatın
- Export süresini ölçün

**Beklenen Sonuç:**
- Export işlemi < 10 saniye içinde tamamlanır
- Dosya başarıyla oluşturulur
- Dosya içeriği doğrudur

**Gerçek Sonuç:**  
**Durum:** [ ] Pass [ ] Fail [ ] Blocked  
**Notlar:**

---

## 6. Test Raporu Şablonu

### Test Bilgileri

**Test Tarihi:** _______________  
**Test Edilen Kişi:** _______________  
**Test Ortamı:** _______________  
**Proje Versiyonu:** _______________  
**Veritabanı Versiyonu:** _______________

### Test Ortamı Bilgileri

**İşletim Sistemi:** _______________  
**Tarayıcı:** _______________  
**Tarayıcı Versiyonu:** _______________  
**.NET Versiyonu:** _______________  
**SQL Server Versiyonu:** _______________

### Sonuç Özeti

**Toplam Test Senaryosu:** _______________  
**Başarılı:** _______________  
**Başarısız:** _______________  
**Bloke:** _______________  
**Başarı Oranı:** _______________%

### Bulunan Hatalar Listesi

| Hata ID | Test Senaryosu | Hata Açıklaması | Öncelik | Durum |
|---------|----------------|-----------------|---------|-------|
| BUG-01  | TS-XX          |                 | Yüksek  | Açık   |
| BUG-02  | TS-XX          |                 | Orta    | Açık   |

### Öneriler

1. _________________________________________________
2. _________________________________________________
3. _________________________________________________

### Genel Değerlendirme

**Güçlü Yönler:**
- _________________________________________________
- _________________________________________________

**İyileştirme Önerileri:**
- _________________________________________________
- _________________________________________________

**Sonuç:**
- [ ] Testler başarıyla tamamlandı, üretime hazır
- [ ] Testler tamamlandı, bazı hatalar bulundu, düzeltmeler gerekli
- [ ] Testler tamamlanamadı, kritik hatalar mevcut

---

## 7. İlgili Dosyalar

- `Controllers/CustomerController.cs` - Test edilecek action'lar
- `DAL/CustomerDAL.cs` - Test edilecek DAL metodları
- `Views/Customer/ImportExcel.cshtml` - Test edilecek view
- `Views/Customer/BatchDetails.cshtml` - Test edilecek view
- `Models/ImportBatchViewModel.cs` - Test edilecek model
- `Models/ImportBatch.cs` - Batch modeli
- `Models/ImportDetail.cs` - Detail modeli
- `Models/BatchStatistics.cs` - Statistics modeli

---

## 8. Notlar

- Tüm testler manuel olarak yapılacaktır
- Test sırasında gözlemlenen tüm hatalar dokümante edilmelidir
- Performans testleri için gerçekçi veri setleri kullanılmalıdır
- Test sonuçları Test Raporu Şablonu kullanılarak kaydedilmelidir

---

**Dokümantasyon Versiyonu:** 1.0  
**Son Güncelleme:** 2024  
**Hazırlayan:** Test Ekibi
