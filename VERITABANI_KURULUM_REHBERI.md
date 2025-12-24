# Veritabanı Kurulum Rehberi

## Hata: "Cannot open database DistributorFinanceDB"

Bu hata, veritabanının henüz oluşturulmamış olmasından kaynaklanıyor. Aşağıdaki adımları takip edin:

## Adım 1: SQL Server Management Studio (SSMS) veya Azure Data Studio'yu Açın

## Adım 2: Veritabanını Oluşturun

### Seçenek A: quries.sql Dosyasını Çalıştırın (ÖNERİLEN)

1. SSMS'de `File > Open > File` menüsünden `quries.sql` dosyasını açın
2. Dosyanın tamamını seçin (Ctrl+A)
3. `Execute` (F5) tuşuna basın
4. Bu işlem:
   - Veritabanını oluşturur
   - Tüm tabloları oluşturur
   - View'ları oluşturur
   - Trigger'ları oluşturur
   - Stored Procedure'leri oluşturur
   - Örnek verileri ekler

### Seçenek B: database_update.sql Dosyasını Çalıştırın (SADECE SP/TRIGGER/VIEW GÜNCELLEMELERİ İÇİN)

**ÖNEMLİ:** `database_update.sql` dosyası sadece mevcut veritabanına SP, trigger ve view ekler. 
Eğer veritabanı henüz oluşturulmadıysa, önce `quries.sql` dosyasını çalıştırın!

## Adım 3: Connection String'i Kontrol Edin

`appsettings.json` dosyasındaki connection string'i kontrol edin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=DistributorFinanceDB;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### SQL Server Instance Adınızı Bulma

Eğer SQL Server instance adınız farklıysa:

1. **SSMS'de:** Object Explorer'da server adına bakın
   - Örnek: `(localdb)\MSSQLLocalDB` veya `localhost` veya `.\SQLEXPRESS`

2. **PowerShell'de:**
   ```powershell
   Get-Service | Where-Object {$_.Name -like "*SQL*"}
   ```

3. **Command Prompt'ta:**
   ```cmd
   sc query | findstr SQL
   ```

### Connection String Seçenekleri

#### SQL Server Express (Varsayılan):
```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=DistributorFinanceDB;Trusted_Connection=True;TrustServerCertificate=True"
```

#### LocalDB:
```json
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=DistributorFinanceDB;Trusted_Connection=True;TrustServerCertificate=True"
```

#### Named Instance:
```json
"DefaultConnection": "Server=localhost\\INSTANCE_NAME;Database=DistributorFinanceDB;Trusted_Connection=True;TrustServerCertificate=True"
```

#### SQL Server Authentication (Kullanıcı adı/şifre ile):
```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=DistributorFinanceDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True"
```

## Adım 4: Veritabanı Erişim İzinlerini Kontrol Edin

Eğer hala erişim hatası alıyorsanız:

### Windows Authentication Kullanıyorsanız:

1. SSMS'de `Security > Logins` klasörüne gidin
2. `PrometheusV3\erkan` kullanıcısını bulun (veya oluşturun)
3. Sağ tıklayın > `Properties`
4. `User Mapping` sekmesine gidin
5. `DistributorFinanceDB` veritabanını seçin
6. `db_owner` veya `db_datareader` ve `db_datawriter` rollerini verin
7. `OK` tıklayın

### SQL Server Authentication Kullanıyorsanız:

1. `Security > Logins` > `New Login`
2. Login name: `sa` (veya istediğiniz kullanıcı adı)
3. `SQL Server authentication` seçin
4. Şifre girin
5. `User Mapping` sekmesinde `DistributorFinanceDB` seçin
6. Rolleri verin
7. `OK` tıklayın

## Adım 5: Veritabanının Oluşturulduğunu Doğrulayın

SSMS'de:
1. Object Explorer'da `Databases` klasörünü genişletin
2. `DistributorFinanceDB` veritabanının listede olduğunu kontrol edin
3. Veritabanına sağ tıklayın > `New Query`
4. Şu sorguyu çalıştırın:
   ```sql
   SELECT COUNT(*) FROM Customers;
   ```
   Eğer sayı dönerse, veritabanı başarıyla oluşturulmuştur.

## Adım 6: Uygulamayı Yeniden Başlatın

1. Visual Studio'da uygulamayı durdurun (Stop)
2. Uygulamayı yeniden başlatın (F5)

## Hala Hata Alıyorsanız

### Test Scripti:

`appsettings.json` dosyasını geçici olarak şu şekilde değiştirin (test için):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=master;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Eğer bu çalışırsa, sorun veritabanı adındadır. `DistributorFinanceDB` veritabanını oluşturmanız gerekiyor.

### Alternatif: Veritabanını Manuel Oluşturma

SSMS'de:
1. `Databases` klasörüne sağ tıklayın
2. `New Database...`
3. Database name: `DistributorFinanceDB`
4. `OK` tıklayın
5. Sonra `quries.sql` dosyasını çalıştırın

## Hızlı Test

PowerShell'de test edin:

```powershell
# SQL Server'a bağlan
sqlcmd -S .\SQLEXPRESS -E -Q "SELECT name FROM sys.databases WHERE name = 'DistributorFinanceDB'"
```

Eğer sonuç dönerse, veritabanı mevcut demektir.

