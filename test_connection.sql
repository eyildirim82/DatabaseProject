-- Veritabanı Bağlantı Testi ve Kurulum Scripti
-- Bu scripti SQL Server Management Studio'da çalıştırın

-- 1. SQL Server instance'ınızı kontrol edin
SELECT @@SERVERNAME AS 'Server Name', @@VERSION AS 'SQL Server Version';
GO

-- 2. Mevcut veritabanlarını listeleyin
SELECT name FROM sys.databases ORDER BY name;
GO

-- 3. Eğer DistributorFinanceDB yoksa, oluşturun
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'DistributorFinanceDB')
BEGIN
    PRINT 'DistributorFinanceDB veritabanı bulunamadı. Oluşturuluyor...';
    CREATE DATABASE DistributorFinanceDB;
    PRINT 'DistributorFinanceDB veritabanı başarıyla oluşturuldu.';
END
ELSE
BEGIN
    PRINT 'DistributorFinanceDB veritabanı zaten mevcut.';
END
GO

-- 4. Veritabanına geçin
USE DistributorFinanceDB;
GO

-- 5. Kullanıcı izinlerini kontrol edin
SELECT 
    dp.name AS 'User',
    dp.type_desc AS 'Type',
    r.name AS 'Role'
FROM sys.database_role_members rm
INNER JOIN sys.database_principals r ON rm.role_principal_id = r.principal_id
INNER JOIN sys.database_principals dp ON rm.member_principal_id = dp.principal_id
WHERE dp.name = SYSTEM_USER;
GO

-- 6. Eğer tablolar yoksa, quries.sql dosyasını çalıştırmanız gerekiyor
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Customers')
BEGIN
    PRINT 'UYARI: Customers tablosu bulunamadı!';
    PRINT 'Lütfen quries.sql dosyasını çalıştırarak tüm tabloları, view''ları, trigger''ları ve stored procedure''leri oluşturun.';
END
ELSE
BEGIN
    PRINT 'Customers tablosu mevcut. Veritabanı kurulumu tamamlanmış görünüyor.';
    SELECT COUNT(*) AS 'Customer Count' FROM Customers;
END
GO

