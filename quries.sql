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
    SELECT * FROM Transactions 
    WHERE CustomerID = @CustomerID 
      AND TransactionDate BETWEEN @StartDate AND @EndDate
    ORDER BY TransactionDate DESC;
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
        (SELECT COUNT(*) FROM Cheques WHERE Status = 'Portfolio') as PendingCheques;
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
CREATE PROCEDURE sp_ProcessReconciliation
    @BatchID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        -- 1. Detay tablosuna o anki sistem bakiyesini bilgi amaçlı yaz (Loglama)
        UPDATE D
        SET D.SystemBalanceAtTime = C.CurrentBalance
        FROM ImportDetails D
        INNER JOIN Customers C ON D.AccountCode = C.AccountCode
        WHERE D.BatchID = @BatchID;

        -- 2. Müşteri Bakiyelerini DİREKT olarak Excel verisine eşitle (Overwrite)
        -- Fark hesabı veya Transaction kaydı YOK. Sadece son durum geçerli.
        UPDATE C
        SET C.CurrentBalance = D.ExcelBalance
        FROM Customers C
        JOIN ImportDetails D ON C.AccountCode = D.AccountCode
        WHERE D.BatchID = @BatchID;

        -- 3. Batch işlemini tamamlandı olarak işaretle
        UPDATE ImportBatches SET Status = 'Processed' WHERE BatchID = @BatchID;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW; -- Hatayı C# tarafına fırlat
    END CATCH
END;
GO