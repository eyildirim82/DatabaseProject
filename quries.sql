/* CSE3055 Database Systems - Project Step #3 Submission
   Group Members:
   - Doğukan Demir (150122539)
   - Erkan Yıldırım (150119509)
   - Fatih Kaba (150120057)
*/

USE master;
GO

IF EXISTS (SELECT * FROM sys.databases WHERE name = 'DistributorFinanceDB')
    DROP DATABASE DistributorFinanceDB;
GO

CREATE DATABASE DistributorFinanceDB;
GO

USE DistributorFinanceDB;
GO

/* =============================================
   SECTION 1: TABLE CREATION (DDL)
   ============================================= */

-- 1. Table: UserRoles
CREATE TABLE UserRoles (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(200)
);
GO

-- 2. Table: PaymentMethods
CREATE TABLE PaymentMethods (
    MethodID INT IDENTITY(1,1) PRIMARY KEY,
    MethodName NVARCHAR(50) NOT NULL UNIQUE
);
GO

-- 3. Table: CompanyAccounts
CREATE TABLE CompanyAccounts (
    AccountID INT IDENTITY(1,1) PRIMARY KEY,
    BankName NVARCHAR(100) NOT NULL,
    BranchName NVARCHAR(100),
    IBAN VARCHAR(34) NOT NULL UNIQUE,
    CurrencyCode CHAR(3) DEFAULT 'TRY',
    CurrentBalance DECIMAL(18, 2) DEFAULT 0.00
);
GO

-- 4. Table: AppUsers
CREATE TABLE AppUsers (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    RoleID INT NOT NULL,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleID) REFERENCES UserRoles(RoleID)
);
GO

-- 5. Table: Customers
CREATE TABLE Customers (
    CustomerID INT IDENTITY(1,1) PRIMARY KEY,
    AccountCode NVARCHAR(20) NOT NULL UNIQUE,
    CompanyName NVARCHAR(150) NOT NULL,
    TaxID NVARCHAR(20),
    TaxOffice NVARCHAR(50),
    Address NVARCHAR(250),
    PhoneNumber NVARCHAR(20),
    RiskLimit DECIMAL(18, 2) DEFAULT 0.00,
    CurrentBalance DECIMAL(18, 2) DEFAULT 0.00,
    
    -- Computed Column: Available Risk
    AvailableRisk AS (RiskLimit - CurrentBalance), 
    
    -- Check Constraint: Risk Limit cannot be negative
    CONSTRAINT CK_RiskLimit_Positive CHECK (RiskLimit >= 0)
);
GO

-- 6. Table: Transactions
CREATE TABLE Transactions (
    TransactionID INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID INT NOT NULL,
    AccountID INT NULL,
    MethodID INT NOT NULL,
    CreatedBy INT NOT NULL,
    TransactionDate DATETIME DEFAULT GETDATE(),
    Amount DECIMAL(18, 2) NOT NULL,
    Description NVARCHAR(250),
    
    CONSTRAINT FK_Trans_Customer FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID),
    CONSTRAINT FK_Trans_Account FOREIGN KEY (AccountID) REFERENCES CompanyAccounts(AccountID),
    CONSTRAINT FK_Trans_Method FOREIGN KEY (MethodID) REFERENCES PaymentMethods(MethodID),
    CONSTRAINT FK_Trans_User FOREIGN KEY (CreatedBy) REFERENCES AppUsers(UserID),
    
    CONSTRAINT CK_Trans_Amount CHECK (Amount > 0)
);
GO

-- 7. Table: Cheques
CREATE TABLE Cheques (
    ChequeID INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID INT NOT NULL,
    BankName NVARCHAR(100) NOT NULL,
    ChequeNumber NVARCHAR(50),
    Amount DECIMAL(18, 2) NOT NULL,
    DueDate DATE NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Portfolio',
    ReceivedDate DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_Cheque_Customer FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID),
    CONSTRAINT CK_Cheque_Status CHECK (Status IN ('Portfolio', 'Collected', 'Bounced', 'Returned'))
);
GO

-- 8. Table: CreditCardInstallments
CREATE TABLE CreditCardInstallments (
    InstallmentID INT IDENTITY(1,1) PRIMARY KEY,
    TransactionID INT NOT NULL,
    BankID INT NOT NULL,
    ReleaseDate DATE NOT NULL,
    Amount DECIMAL(18, 2) NOT NULL,
    IsReleased BIT DEFAULT 0,
    
    CONSTRAINT FK_Installment_Trans FOREIGN KEY (TransactionID) REFERENCES Transactions(TransactionID),
    CONSTRAINT FK_Installment_Bank FOREIGN KEY (BankID) REFERENCES CompanyAccounts(AccountID)
);
GO

-- 9. Table: CollectionNotes (CRM)
CREATE TABLE CollectionNotes (
    NoteID INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID INT NOT NULL,
    UserID INT NOT NULL,
    NoteText NVARCHAR(MAX) NOT NULL,
    PromiseDate DATE NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_Note_Customer FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID),
    CONSTRAINT FK_Note_User FOREIGN KEY (UserID) REFERENCES AppUsers(UserID)
);
GO

-- 10. Table: SystemLogs (Audit Trail)
CREATE TABLE SystemLogs (
    LogID INT IDENTITY(1,1) PRIMARY KEY,
    TableName NVARCHAR(50) NOT NULL,
    RecordID INT NOT NULL,
    OperationType NVARCHAR(10) NOT NULL,
    OldValue NVARCHAR(MAX) NULL,
    NewValue NVARCHAR(MAX) NULL,
    ChangedBy INT NULL,
    LogDate DATETIME DEFAULT GETDATE()
);
GO

/* =============================================
   SECTION 2: VIEWS
   ============================================= */
GO

-- View 1: Customer Risk Status
CREATE VIEW vw_CustomerRiskStatus AS
SELECT 
    c.CustomerID,
    c.CompanyName,
    c.RiskLimit,
    c.CurrentBalance,
    (c.RiskLimit - c.CurrentBalance) AS AvailableLimit,
    CASE 
        WHEN c.CurrentBalance > c.RiskLimit THEN 'Risk Limit Exceeded'
        WHEN c.CurrentBalance > (c.RiskLimit * 0.9) THEN 'Critical'
        ELSE 'Safe'
    END AS RiskStatus
FROM Customers c;
GO

-- View 2: Daily Cash Flow
CREATE VIEW vw_DailyCashFlow AS
SELECT 
    CONVERT(DATE, t.TransactionDate) AS PaymentDate,
    pm.MethodName,
    COUNT(t.TransactionID) AS TransactionCount,
    SUM(t.Amount) AS TotalAmount
FROM Transactions t
JOIN PaymentMethods pm ON t.MethodID = pm.MethodID
GROUP BY CONVERT(DATE, t.TransactionDate), pm.MethodName;
GO

-- View 3: Portfolio Cheques
CREATE VIEW vw_PortfolioCheques AS
SELECT 
    ch.ChequeID,
    c.CompanyName,
    ch.BankName,
    ch.Amount,
    ch.DueDate,
    DATEDIFF(day, GETDATE(), ch.DueDate) AS DaysToMaturity
FROM Cheques ch
JOIN Customers c ON ch.CustomerID = c.CustomerID
WHERE ch.Status = 'Portfolio';
GO

-- View 4: Representative Performance
CREATE VIEW vw_RepPerformance AS
SELECT 
    u.FullName,
    COUNT(t.TransactionID) AS TotalTransactions,
    SUM(t.Amount) AS TotalCollectedAmount
FROM Transactions t
JOIN AppUsers u ON t.CreatedBy = u.UserID
GROUP BY u.FullName;
GO

/* =============================================
   SECTION 3: TRIGGERS
   ============================================= */
GO

-- Trigger 1: Update Balance After Payment
CREATE TRIGGER trg_UpdateBalance_AfterPayment
ON Transactions
AFTER INSERT
AS
BEGIN
    UPDATE C
    SET C.CurrentBalance = C.CurrentBalance - I.Amount
    FROM Customers C
    INNER JOIN inserted I ON C.CustomerID = I.CustomerID;
END;
GO

-- Trigger 2: Audit Transaction Deletion
CREATE TRIGGER trg_Audit_TransactionDelete
ON Transactions
AFTER DELETE
AS
BEGIN
    INSERT INTO SystemLogs (TableName, RecordID, OperationType, OldValue, ChangedBy)
    SELECT 
        'Transactions', 
        d.TransactionID, 
        'DELETE', 
        CONCAT('Amount: ', d.Amount, ', Date: ', d.TransactionDate), 
        d.CreatedBy
    FROM deleted d;
END;
GO

/* =============================================
   SECTION 4: STORED PROCEDURES
   ============================================= */
GO

-- SP 1: Add Transaction
CREATE PROCEDURE sp_AddTransaction
    @CustomerID INT,
    @MethodID INT,
    @AccountID INT,
    @Amount DECIMAL(18,2),
    @UserID INT,
    @Description NVARCHAR(250)
AS
BEGIN
    INSERT INTO Transactions (CustomerID, MethodID, AccountID, Amount, CreatedBy, Description)
    VALUES (@CustomerID, @MethodID, @AccountID, @Amount, @UserID, @Description);
END;
GO

-- SP 2: Add Cheque With Risk Check
CREATE PROCEDURE sp_AddChequeWithRiskCheck
    @CustomerID INT,
    @BankName NVARCHAR(100),
    @Amount DECIMAL(18,2),
    @DueDate DATE
AS
BEGIN
    DECLARE @CurrentRisk DECIMAL(18,2);
    DECLARE @Limit DECIMAL(18,2);

    SELECT @CurrentRisk = CurrentBalance, @Limit = RiskLimit FROM Customers WHERE CustomerID = @CustomerID;

    IF (@CurrentRisk + @Amount) > @Limit
    BEGIN
        RAISERROR('Transaction Rejected: Customer Risk Limit Exceeded!', 16, 1);
        RETURN;
    END

    INSERT INTO Cheques (CustomerID, BankName, Amount, DueDate, Status)
    VALUES (@CustomerID, @BankName, @Amount, @DueDate, 'Portfolio');
END;
GO

-- SP 3: Update Cheque Status
CREATE PROCEDURE sp_UpdateChequeStatus
    @ChequeID INT,
    @NewStatus NVARCHAR(20)
AS
BEGIN
    UPDATE Cheques SET Status = @NewStatus WHERE ChequeID = @ChequeID;
END;
GO

-- SP 4: Get Customer Statement
CREATE PROCEDURE sp_GetCustomerStatement
    @CustomerID INT,
    @StartDate DATE,
    @EndDate DATE
AS
BEGIN
    SELECT 
        t.TransactionID,
        t.CustomerID,
        t.MethodID,
        pm.MethodName,
        t.AccountID,
        t.Amount,
        t.Description,
        t.TransactionDate,
        t.CreatedBy
    FROM Transactions t
    INNER JOIN PaymentMethods pm ON t.MethodID = pm.MethodID
    WHERE t.CustomerID = @CustomerID 
      AND t.TransactionDate BETWEEN @StartDate AND @EndDate
    ORDER BY t.TransactionDate DESC;
END;
GO

-- SP 5: Get Overdue Receivables
CREATE PROCEDURE sp_GetOverdueReceivables
AS
BEGIN
    SELECT c.CompanyName, c.CurrentBalance, MAX(t.TransactionDate) as LastPaymentDate
    FROM Customers c
    LEFT JOIN Transactions t ON c.CustomerID = t.CustomerID
    GROUP BY c.CompanyName, c.CurrentBalance
    HAVING DATEDIFF(day, MAX(t.TransactionDate), GETDATE()) > 60;
END;
GO

-- SP 6: User Login
CREATE PROCEDURE sp_UserLogin
    @Username NVARCHAR(50),
    @PasswordHash NVARCHAR(256)
AS
BEGIN
    SELECT UserID, FullName, RoleID 
    FROM AppUsers 
    WHERE Username = @Username AND PasswordHash = @PasswordHash AND IsActive = 1;
END;
GO

-- SP 7: Dashboard Stats
CREATE PROCEDURE sp_GetDashboardStats
AS
BEGIN
    SELECT 
        (SELECT COUNT(*) FROM Customers) as TotalCustomers,
        (SELECT ISNULL(SUM(Amount), 0) FROM Transactions WHERE TransactionDate >= CAST(GETDATE() AS DATE)) as TodayCollection,
        (SELECT COUNT(*) FROM Cheques WHERE Status = 'Portfolio') as PendingCheques,
        (SELECT ISNULL(SUM(CurrentBalance), 0) FROM Customers WHERE CurrentBalance > 0) as TotalReceivables,
        (SELECT COUNT(*) FROM vw_CustomerRiskStatus WHERE RiskStatus IN ('Critical', 'Risk Limit Exceeded')) as RiskyCustomerCount;
END;
GO

-- SP 8: Add Collection Note
CREATE PROCEDURE sp_AddCollectionNote
    @CustomerID INT,
    @UserID INT,
    @NoteText NVARCHAR(MAX),
    @PromiseDate DATE
AS
BEGIN
    INSERT INTO CollectionNotes (CustomerID, UserID, NoteText, PromiseDate)
    VALUES (@CustomerID, @UserID, @NoteText, @PromiseDate);
END;
GO

-- SP 9: Add Customer
CREATE PROCEDURE sp_AddCustomer
    @AccountCode NVARCHAR(20),
    @CompanyName NVARCHAR(200),
    @TaxID NVARCHAR(20),
    @TaxOffice NVARCHAR(100),
    @Address NVARCHAR(500),
    @PhoneNumber NVARCHAR(20),
    @RiskLimit DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- AccountCode unique kontrolü
        IF EXISTS (SELECT 1 FROM Customers WHERE AccountCode = @AccountCode)
        BEGIN
            RAISERROR('Bu hesap kodu zaten kullanılıyor.', 16, 1);
            RETURN;
        END
        
        INSERT INTO Customers (AccountCode, CompanyName, TaxID, TaxOffice, Address, PhoneNumber, RiskLimit, CurrentBalance)
        VALUES (@AccountCode, @CompanyName, @TaxID, @TaxOffice, @Address, @PhoneNumber, @RiskLimit, 0);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

-- SP 10: Update Customer
CREATE PROCEDURE sp_UpdateCustomer
    @CustomerID INT,
    @CompanyName NVARCHAR(200),
    @TaxID NVARCHAR(20),
    @TaxOffice NVARCHAR(100),
    @Address NVARCHAR(500),
    @PhoneNumber NVARCHAR(20),
    @RiskLimit DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM Customers WHERE CustomerID = @CustomerID)
        BEGIN
            RAISERROR('Müşteri bulunamadı.', 16, 1);
            RETURN;
        END
        
        UPDATE Customers
        SET CompanyName = @CompanyName,
            TaxID = @TaxID,
            TaxOffice = @TaxOffice,
            Address = @Address,
            PhoneNumber = @PhoneNumber,
            RiskLimit = @RiskLimit
        WHERE CustomerID = @CustomerID;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

-- SP 11: Delete Customer (Soft delete - sadece risk limitini 0 yapar veya fiziksel silme)
-- Not: Transaction'lar varsa silme işlemi yapılmamalı
CREATE PROCEDURE sp_DeleteCustomer
    @CustomerID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Transaction kontrolü
        IF EXISTS (SELECT 1 FROM Transactions WHERE CustomerID = @CustomerID)
        BEGIN
            RAISERROR('Bu müşteriye ait işlem kayıtları bulunduğu için silinemez.', 16, 1);
            RETURN;
        END
        
        -- Çek kontrolü
        IF EXISTS (SELECT 1 FROM Cheques WHERE CustomerID = @CustomerID)
        BEGIN
            RAISERROR('Bu müşteriye ait çek kayıtları bulunduğu için silinemez.', 16, 1);
            RETURN;
        END
        
        DELETE FROM Customers WHERE CustomerID = @CustomerID;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

/* =============================================
   SECTION 5: INITIAL DATA POPULATION
   ============================================= */

-- Roles
INSERT INTO UserRoles (RoleName, Description) VALUES 
('Admin', 'Administrator'),
('Accountant', 'Accounting Staff'),
('SalesRep', 'Field Sales Rep');

-- Payment Methods
INSERT INTO PaymentMethods (MethodName) VALUES 
('Cash'), ('Bank Transfer'), ('Credit Card'), ('Mobile Payment');

-- Company Accounts (Ensure ID 1 and 2 exist)
INSERT INTO CompanyAccounts (BankName, IBAN, CurrencyCode) VALUES
('Garanti BBVA', 'TR120006200000012345678901', 'TRY'),
('Is Bankasi', 'TR560006400000098765432109', 'USD');

-- Users
INSERT INTO AppUsers (RoleID, Username, PasswordHash, FullName, Email) VALUES
(1, 'admin', '5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8', 'System Admin', 'admin@system.com'),
(2, 'accountant1', 'hash1234', 'Ahmet Yilmaz', 'ahmet@system.com'),
(3, 'sales1', 'hash5678', 'Mehmet Demir', 'mehmet@system.com');

/* =============================================
   SECTION 6: MASS DATA POPULATION (25+ RECORDS)
   Fixed to avoid Foreign Key errors.
   ============================================= */

DECLARE @i INT = 1;
DECLARE @randAmount DECIMAL(18,2);
DECLARE @randCustomer INT;
DECLARE @randMethod INT;
DECLARE @randUser INT;
DECLARE @randAccount INT;

-- 1. Create 30 Customers
WHILE @i <= 30
BEGIN
    INSERT INTO Customers (AccountCode, CompanyName, RiskLimit, CurrentBalance)
    VALUES (
        'C-' + CAST((1000 + @i) AS NVARCHAR), 
        'Demo Company ' + CAST(@i AS NVARCHAR), 
        100000.00 + (@i * 1000), 
        0.00 
    );
    SET @i = @i + 1;
END;

-- 2. Create 50 Transactions
SET @i = 1;
WHILE @i <= 50
BEGIN
    -- Select VALID random IDs from existing tables
    SELECT TOP 1 @randCustomer = CustomerID FROM Customers ORDER BY NEWID();
    SELECT TOP 1 @randMethod = MethodID FROM PaymentMethods ORDER BY NEWID();
    SELECT TOP 1 @randUser = UserID FROM AppUsers ORDER BY NEWID();
    SELECT TOP 1 @randAccount = AccountID FROM CompanyAccounts ORDER BY NEWID();
    SELECT @randAmount = CAST(RAND() * 9900 + 100 AS DECIMAL(18,2));

    INSERT INTO Transactions (CustomerID, AccountID, MethodID, CreatedBy, Amount, Description)
    VALUES (
        @randCustomer, 
        @randAccount, 
        @randMethod, 
        @randUser, 
        @randAmount, 
        'Auto Generated Payment #' + CAST(@i AS NVARCHAR)
    );
    SET @i = @i + 1;
END;

-- 3. Create 25 Cheques
SET @i = 1;
WHILE @i <= 25
BEGIN
    SELECT TOP 1 @randCustomer = CustomerID FROM Customers ORDER BY NEWID();
    SELECT @randAmount = CAST(RAND() * 50000 + 1000 AS DECIMAL(18,2));

    INSERT INTO Cheques (CustomerID, BankName, Amount, DueDate, Status)
    VALUES (
        @randCustomer,
        'Demo Bank A.S.',
        @randAmount,
        DATEADD(day, @i * 2, GETDATE()), 
        'Portfolio'
    );
    SET @i = @i + 1;
END;

PRINT 'Data Population Completed Successfully.';

/* =============================================
   SECTION 7: PERFORMANCE INDEXES
   ============================================= */

CREATE NONCLUSTERED INDEX IX_Transactions_Date 
ON Transactions (TransactionDate);
GO

CREATE NONCLUSTERED INDEX IX_Transactions_CustomerID 
ON Transactions (CustomerID) 
INCLUDE (Amount, MethodID);
GO

CREATE NONCLUSTERED INDEX IX_Cheques_Status_DueDate 
ON Cheques (Status, DueDate);
GO

CREATE NONCLUSTERED INDEX IX_Customers_CompanyName 
ON Customers (CompanyName);
GO

CREATE NONCLUSTERED INDEX IX_SystemLogs_LogDate 
ON SystemLogs (LogDate DESC);
GO
/* =============================================
   SECTION: EXCEL IMPORT & RECONCILIATION TABLES
   ============================================= */

-- 1. Yükleme Paketleri (Excel Dosyasının Kimliği)
CREATE TABLE ImportBatches (
    BatchID INT IDENTITY(1,1) PRIMARY KEY,
    FileName NVARCHAR(255) NOT NULL,       -- Örn: TopluCariEkstreRaporu_20251215142935.xlsx
    FileTimestamp DATETIME NOT NULL,       -- Dosya isminden alınan tarih
    UploadDate DATETIME DEFAULT GETDATE(), -- Sisteme yüklendiği an
    UploadedBy INT,                        -- AppUsers tablosuna FK
    TotalRecords INT,
    Status NVARCHAR(20) DEFAULT 'Pending', -- Pending, Processed, Rejected (Eski tarihliyse red)
    
    CONSTRAINT UQ_FileName UNIQUE (FileName)
);
GO

-- 2. Yükleme Detayları (Excel Satırları)
CREATE TABLE ImportDetails (
    DetailID INT IDENTITY(1,1) PRIMARY KEY,
    BatchID INT NOT NULL,
    AccountCode NVARCHAR(50), 
    DetectedName NVARCHAR(200),
    ExcelBalance DECIMAL(18,2),            -- Excel'deki o anki bakiye
    SystemBalanceAtTime DECIMAL(18,2),     -- Karşılaştırma anındaki sistem bakiyesi
    
    CONSTRAINT FK_Import_Batch FOREIGN KEY (BatchID) REFERENCES ImportBatches(BatchID)
);
GO
CREATE PROCEDURE sp_ValidateAndCreateBatch
    @FileName NVARCHAR(255),
    @FileTimestamp DATETIME, -- C#'ta dosya isminden parse edilip buraya gelecek
    @UploadedBy INT,
    @TotalRecords INT,
    @BatchID INT OUTPUT      -- Geriye ID döndürecek
AS
BEGIN
    SET NOCOUNT ON;

    -- KURAL: İçeride daha yeni tarihli ve İŞLENMİŞ bir veri var mı?
    -- Varsa, eski dosyayı kabul etme.
    IF EXISTS (
        SELECT 1 FROM ImportBatches 
        WHERE FileTimestamp > @FileTimestamp 
        AND Status = 'Processed'
    )
    BEGIN
        -- Her ihtimale karşı, çağıran tarafın "bozuk" bir BatchID ile devam etmemesi için
        SET @BatchID = 0;
        RAISERROR('HATA: Sistemde bu tarihten daha güncel bir veri zaten yüklü. İşlem reddedildi.', 16, 1) WITH SETERROR;
        RETURN;
    END

    -- Sorun yoksa kaydı aç
    INSERT INTO ImportBatches (FileName, FileTimestamp, UploadedBy, TotalRecords, Status)
    VALUES (@FileName, @FileTimestamp, @UploadedBy, @TotalRecords, 'Pending');

    SET @BatchID = SCOPE_IDENTITY();
END;
GO
CREATE OR ALTER PROCEDURE sp_ProcessReconciliation
    @BatchID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        -- 1. ADIM: Eşleşenlerin Eski Bakiyesini Logla
        -- (Burada duplicate olması sorun yaratmaz, update ezer geçer)
        UPDATE D
        SET D.SystemBalanceAtTime = C.CurrentBalance
        FROM ImportDetails D
        INNER JOIN Customers C ON D.AccountCode = C.AccountCode
        WHERE D.BatchID = @BatchID;

        -- 2. ADIM (DÜZELTİLEN KISIM): Yeni Müşterileri 'Customers' Tablosuna EKLE
        -- GROUP BY kullanarak aynı koddan birden fazla varsa TEKE düşürüyoruz.
        INSERT INTO Customers (AccountCode, CompanyName, CurrentBalance, RiskLimit, TaxID, Address)
        SELECT 
            D.AccountCode, 
            MAX(D.DetectedName), -- Aynı koddan 2 tane varsa ismin birini seç
            MAX(D.ExcelBalance), -- Bakiyenin birini seç (Genelde sonuncudur)
            0, 
            NULL, 
            NULL
        FROM ImportDetails D
        LEFT JOIN Customers C ON D.AccountCode = C.AccountCode
        WHERE D.BatchID = @BatchID 
          AND C.CustomerID IS NULL -- Sadece sistemde olmayanlar
        GROUP BY D.AccountCode; -- <--- İŞTE BU SATIR HATAYI ÇÖZER

        -- 3. ADIM: Mevcut Müşterilerin Bakiyesini Güncelle
        -- Burada da duplicate ihtimaline karşı subquery ile tekil veri alıyoruz
        UPDATE C
        SET C.CurrentBalance = Source.MaxBalance
        FROM Customers C
        INNER JOIN (
            SELECT AccountCode, MAX(ExcelBalance) as MaxBalance
            FROM ImportDetails
            WHERE BatchID = @BatchID
            GROUP BY AccountCode
        ) Source ON C.AccountCode = Source.AccountCode;

        -- 4. ADIM: Batch Durumunu Kapat
        UPDATE ImportBatches SET Status = 'Processed' WHERE BatchID = @BatchID;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- SP: Collect Cheque (Çek Tahsil Etme)
-- Çek tahsil edildiğinde transaction oluşturur ve müşteri bakiyesini günceller
CREATE PROCEDURE sp_CollectCheque
    @ChequeID INT,
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        DECLARE @CustomerID INT;
        DECLARE @Amount DECIMAL(18,2);
        DECLARE @MethodID INT;
        DECLARE @AccountID INT;
        
        -- Çek bilgilerini al
        SELECT @CustomerID = CustomerID, @Amount = Amount
        FROM Cheques
        WHERE ChequeID = @ChequeID AND Status = 'Portfolio';
        
        IF @CustomerID IS NULL
        BEGIN
            RAISERROR('Çek bulunamadı veya tahsil edilemez durumda.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Çek ödeme yöntemini bul (MethodID = 2 genellikle "Çek" olabilir, yoksa ilkini al)
        SELECT TOP 1 @MethodID = MethodID FROM PaymentMethods WHERE MethodName LIKE '%Çek%' OR MethodName LIKE '%Cheque%';
        IF @MethodID IS NULL
        BEGIN
            SELECT TOP 1 @MethodID = MethodID FROM PaymentMethods ORDER BY MethodID;
        END
        
        -- Varsayılan hesap (AccountID NULL olabilir)
        SET @AccountID = NULL;
        
        -- Transaction oluştur (pozitif amount - müşteriden alınan para, alacak azalır)
        INSERT INTO Transactions (CustomerID, MethodID, AccountID, Amount, CreatedBy, Description)
        VALUES (@CustomerID, @MethodID, @AccountID, @Amount, @UserID, 
                'Çek Tahsil Edildi - Çek ID: ' + CAST(@ChequeID AS NVARCHAR(10)));
        
        -- Müşteri bakiyesini güncelle (alacak azalır, bu yüzden CurrentBalance azalır)
        UPDATE Customers
        SET CurrentBalance = CurrentBalance - @Amount
        WHERE CustomerID = @CustomerID;
        
        -- Çek durumunu güncelle
        UPDATE Cheques
        SET Status = 'Collected'
        WHERE ChequeID = @ChequeID;
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- SP: Mark Cheque Bounced (Çek Karşılıksız)
-- Çek karşılıksız olduğunda durumu günceller
CREATE PROCEDURE sp_MarkChequeBounced
    @ChequeID INT,
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Çek durumunu kontrol et
        IF NOT EXISTS (SELECT 1 FROM Cheques WHERE ChequeID = @ChequeID AND Status = 'Portfolio')
        BEGIN
            RAISERROR('Çek bulunamadı veya karşılıksız olarak işaretlenemez durumda.', 16, 1);
            RETURN;
        END
        
        -- Çek durumunu "Bounced" olarak güncelle
        UPDATE Cheques
        SET Status = 'Bounced'
        WHERE ChequeID = @ChequeID;
        
        -- Not: Risk limiti zaten çek eklendiğinde kontrol edildi,
        -- karşılıksız çek risk limitini etkilemez (sadece durum güncellenir)
        
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

-- SP: Reverse Transaction (Transaction İptal)
-- Mevcut transaction'ı tersine çeviren yeni transaction oluşturur
CREATE PROCEDURE sp_ReverseTransaction
    @TransactionID INT,
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        DECLARE @CustomerID INT;
        DECLARE @MethodID INT;
        DECLARE @AccountID INT;
        DECLARE @Amount DECIMAL(18,2);
        DECLARE @Description NVARCHAR(250);
        
        -- Orijinal transaction bilgilerini al
        SELECT @CustomerID = CustomerID, 
               @MethodID = MethodID, 
               @AccountID = AccountID, 
               @Amount = Amount,
               @Description = Description
        FROM Transactions
        WHERE TransactionID = @TransactionID;
        
        IF @CustomerID IS NULL
        BEGIN
            RAISERROR('Transaction bulunamadı.', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Tersine çevrilmiş transaction oluştur (Amount * -1)
        INSERT INTO Transactions (CustomerID, MethodID, AccountID, Amount, CreatedBy, Description)
        VALUES (@CustomerID, @MethodID, @AccountID, @Amount * -1, @UserID, 
                'İptal Edildi - Transaction ID: ' + CAST(@TransactionID AS NVARCHAR(10)) + 
                CASE WHEN @Description IS NOT NULL THEN ' - ' + @Description ELSE '' END);
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- SP: Change Password (Şifre Değiştirme)
-- Kullanıcının mevcut şifresini kontrol eder ve yeni şifreyi günceller
CREATE PROCEDURE sp_ChangePassword
    @UserID INT,
    @CurrentPasswordHash NVARCHAR(256),
    @NewPasswordHash NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Mevcut şifreyi kontrol et
        IF NOT EXISTS (
            SELECT 1 FROM AppUsers 
            WHERE UserID = @UserID 
            AND PasswordHash = @CurrentPasswordHash 
            AND IsActive = 1
        )
        BEGIN
            RAISERROR('Mevcut şifre hatalı.', 16, 1);
            RETURN;
        END
        
        -- Yeni şifreyi güncelle
        UPDATE AppUsers
        SET PasswordHash = @NewPasswordHash
        WHERE UserID = @UserID;
        
        IF @@ROWCOUNT = 0
        BEGIN
            RAISERROR('Şifre güncellenemedi.', 16, 1);
            RETURN;
        END
        
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END;
GO

-- Admin Kullanıcısı Ekleme Scripti
-- Username: admin
-- Password: admin123
-- SHA256 Hash: 240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9
-- Not: Eğer admin kullanıcısı zaten varsa, bu script hata verecektir (güvenlik için)
IF NOT EXISTS (SELECT 1 FROM AppUsers WHERE Username = 'admin')
BEGIN
    INSERT INTO AppUsers (Username, PasswordHash, FullName, RoleID, IsActive)
    VALUES ('admin', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 'Sistem Yöneticisi', 1, 1);
    PRINT 'Admin kullanıcısı başarıyla oluşturuldu.';
END
ELSE
BEGIN
    PRINT 'Admin kullanıcısı zaten mevcut.';
END
GO

-- Admin Kullanıcısı Şifre Güncelleme Scripti
-- Mevcut admin kullanıcısının şifresini admin123 olarak günceller
-- Username: admin
-- Yeni Password: admin123
-- SHA256 Hash: 240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9
UPDATE AppUsers 
SET PasswordHash = '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9'
WHERE Username = 'admin' AND RoleID = 1;
PRINT 'Admin kullanıcısı şifresi güncellendi. Yeni şifre: admin123';
GO

-- ImportDetails tablosuna ErrorMessage kolonu ekleme (eğer yoksa)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ImportDetails') AND name = 'ErrorMessage')
BEGIN
    ALTER TABLE ImportDetails ADD ErrorMessage NVARCHAR(500) NULL;
    PRINT 'ErrorMessage kolonu ImportDetails tablosuna eklendi.';
END
ELSE
BEGIN
    PRINT 'ErrorMessage kolonu zaten mevcut.';
END
GO