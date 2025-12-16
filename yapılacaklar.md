📋 Görev 1: Veri Modeli Oluşturma (C# Model)
Veritabanındaki ImportBatches tablosunu C# tarafında karşılayacak bir sınıf lazım.

[ ] Models klasörüne ImportBatch.cs adında yeni bir class dosyası oluştur.

[ ] İçine BatchID, FileName, FileTimestamp, UploadDate, TotalRecords, Status özelliklerini (property) ekle.

📋 Görev 2: Veri Erişim Katmanı (DAL) Güncellemesi
Veritabanından geçmiş yüklemeleri çeken metodu yazacağız.

[ ] DAL/CustomerDAL.cs dosyasını aç.

[ ] GetImportHistory() adında, geriye List<ImportBatch> döndüren bir metod ekle.

[ ] İçinde SELECT TOP 10 * FROM ImportBatches ORDER BY UploadDate DESC sorgusunu çalıştır ve listeyi doldur.

📋 Görev 3: Controller Güncellemesi
Sayfa açılırken bu listeyi View'a göndermeliyiz.

[ ] Controllers/CustomerController.cs dosyasını aç.

[ ] ImportExcel (GET) metodunu bul (POST olanı değil).

[ ] Metodun içinde _customerDAL.GetImportHistory() çağırıp sonucu View'a model olarak gönder: return View(historyList);.

📋 Görev 4: Görünüm (View) Tasarımı
Kullanıcının göreceği tabloyu ekleyeceğiz.

[ ] Views/Customer/ImportExcel.cshtml dosyasını aç.

[ ] En tepeye model tanımını ekle: @model List<DatabaseProject.Models.ImportBatch>.

[ ] Formun (</form>) bittiği yerin altına yeni bir HTML Tablosu (<table>) ekle.

[ ] foreach döngüsü ile modelden gelen verileri satır satır yazdır.

[ ] "Status" alanına göre renkli etiketler (Badge) ekle (Örn: İşlendi ise Yeşil, Hata ise Kırmızı).

📋 Görev 5: Test ve Demo Hazırlığı
[ ] Projeyi çalıştır ve ImportExcel sayfasına git.

[ ] Tablonun geldiğini ve (varsa) eski kayıtları gösterdiğini doğrula.

[ ] Yeni bir Excel yükle ve tablonun güncellendiğini teyit et.