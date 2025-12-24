# Hızlı Çözüm: Veritabanı Bağlantı Hatası

## Sorun
```
Cannot open database "DistributorFinanceDB" requested by the login. 
The login failed. Login failed for user 'PrometheusV3\erkan'.
```

## Hızlı Çözüm Adımları

### 1. SQL Server Management Studio'yu Açın

### 2. test_connection.sql Dosyasını Çalıştırın

1. SSMS'de `File > Open > File` menüsünden `test_connection.sql` dosyasını açın
2. `Execute` (F5) tuşuna basın
3. Bu script:
   - SQL Server instance'ınızı gösterir
   - Mevcut veritabanlarını listeler
   - DistributorFinanceDB'yi oluşturur (yoksa)
   - İzinleri kontrol eder

### 3. quries.sql Dosyasını Çalıştırın

**ÖNEMLİ:** Veritabanı oluşturulduktan sonra, tüm tabloları, view'ları, trigger'ları ve stored procedure'leri oluşturmak için:

1. SSMS'de `File > Open > File` menüsünden `quries.sql` dosyasını açın
2. `Execute` (F5) tuşuna basın
3. Bu işlem 1-2 dakika sürebilir

### 4. database_update.sql Dosyasını Çalıştırın (OPSIYONEL)

Eğer `quries.sql` dosyasını daha önce çalıştırdıysanız ve sadece yeni stored procedure'leri eklemek istiyorsanız:

1. SSMS'de `File > Open > File` menüsünden `database_update.sql` dosyasını açın
2. `Execute` (F5) tuşuna basın

### 5. Connection String'i Kontrol Edin

`appsettings.json` dosyasında connection string'in doğru olduğundan emin olun:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=DistributorFinanceDB;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

**Eğer SQL Server instance adınız farklıysa:**

- LocalDB kullanıyorsanız:
  ```json
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=DistributorFinanceDB;Trusted_Connection=True;TrustServerCertificate=True"
  ```

- Farklı bir instance kullanıyorsanız:
  ```json
  "DefaultConnection": "Server=localhost\\INSTANCE_NAME;Database=DistributorFinanceDB;Trusted_Connection=True;TrustServerCertificate=True"
  ```

### 6. Uygulamayı Yeniden Başlatın

1. Visual Studio'da uygulamayı durdurun
2. Uygulamayı yeniden başlatın (F5)

## SQL Server Instance Adınızı Bulma

### PowerShell'de:
```powershell
Get-Service | Where-Object {$_.Name -like "*SQL*"}
```

### SSMS'de:
Object Explorer'da server adına bakın (üstteki bağlantı bilgisi)

## Hala Çalışmıyorsa

1. **SQL Server servisinin çalıştığından emin olun:**
   - Windows Services'te `SQL Server (SQLEXPRESS)` veya `SQL Server (MSSQLSERVER)` servisinin çalıştığını kontrol edin

2. **Kullanıcı izinlerini kontrol edin:**
   - SSMS'de `Security > Logins` klasörüne gidin
   - `PrometheusV3\erkan` kullanıcısını bulun
   - Sağ tıklayın > `Properties` > `User Mapping`
   - `DistributorFinanceDB` seçin ve `db_owner` rolünü verin

3. **Alternatif: SQL Server Authentication kullanın:**
   ```json
   "DefaultConnection": "Server=.\\SQLEXPRESS;Database=DistributorFinanceDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True"
   ```

## Test

SSMS'de yeni bir query açın ve şunu çalıştırın:
```sql
USE DistributorFinanceDB;
SELECT COUNT(*) FROM Customers;
```

Eğer sayı dönerse, veritabanı başarıyla kurulmuştur! ✅

