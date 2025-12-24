/* =============================================
   CSE3055 Database Systems - Project Step #3 Update
   Tüm Inline SQL Sorgularını Stored Procedure'lere Dönüştürme
   ============================================= */

USE DistributorFinanceDB;
GO

/* =============================================
   SECTION 1: ALTER TABLE - Ek Özellikler Kontrolü
   ============================================= */

-- COMPUTED COLUMN kontrolü (zaten var: AvailableRisk)
-- CHECK CONSTRAINT kontrolü (zaten var: CK_RiskLimit_Positive, CK_Trans_Amount, CK_Cheque_Status)
-- DEFAULT kontrolü (zaten var: GETDATE(), Status = 'Portfolio')

-- Ek CHECK CONSTRAINT: Transaction Amount negatif olabilir (iptal için)
-- Mevcut CHECK CONSTRAINT'i güncelle (Amount != 0 olmalı, 0'dan büyük değil)
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Trans_Amount')
BEGIN
    ALTER TABLE Transactions DROP CONSTRAINT CK_Trans_Amount;
END
GO

ALTER TABLE Transactions
ADD CONSTRAINT CK_Trans_Amount_NonZero CHECK (Amount != 0);
GO

-- Ek DEFAULT: Cheques tablosuna ChequeNumber için default değer
IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Cheques_ChequeNumber')
BEGIN
    ALTER TABLE Cheques
    ADD CONSTRAINT DF_Cheques_ChequeNumber DEFAULT 'N/A' FOR ChequeNumber;
END
GO

-- Ek COMPUTED COLUMN: Transactions tablosuna NetAmount (Amount mutlak değeri)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Transactions') AND name = 'NetAmount')
BEGIN
    ALTER TABLE Transactions
    ADD NetAmount AS (ABS(Amount)) PERSISTED;
END
GO

/* =============================================
   SECTION 2: STORED PROCEDURES - Customer Operations
   ============================================= */

-- SP: GetAllCustomers
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_GetAllCustomers')
    DROP PROCEDURE sp_GetAllCustomers;
GO

CREATE PROCEDURE sp_GetAllCustomers
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CustomerID, AccountCode, CompanyName, TaxID, TaxOffice, 
           Address, PhoneNumber, RiskLimit, CurrentBalance, AvailableRisk 
    FROM Customers 
    ORDER BY CompanyName;
END;
GO

-- SP: GetCustomerById
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_GetCustomerById')
    DROP PROCEDURE sp_GetCustomerById;
GO

CREATE PROCEDURE sp_GetCustomerById
    @CustomerID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CustomerID, AccountCode, CompanyName, TaxID, TaxOffice, 
           Address, PhoneNumber, RiskLimit, CurrentBalance, AvailableRisk 
    FROM Customers 
    WHERE CustomerID = @CustomerID;
END;
GO

-- SP: GetImportHistory
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_GetImportHistory')
    DROP PROCEDURE sp_GetImportHistory;
GO

CREATE PROCEDURE sp_GetImportHistory
    @TopCount INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@TopCount) BatchID, FileName, FileTimestamp, UploadDate, TotalRecords, Status 
    FROM ImportBatches 
    ORDER BY UploadDate DESC;
END;
GO

-- SP: GetImportHistoryWithFilters
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_GetImportHistoryWithFilters')
    DROP PROCEDURE sp_GetImportHistoryWithFilters;
GO

CREATE PROCEDURE sp_GetImportHistoryWithFilters
    @Status NVARCHAR(20) = NULL,
    @FileName NVARCHAR(255) = NULL,
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SortBy NVARCHAR(50) = 'UploadDate',
    @SortDirection NVARCHAR(10) = 'DESC',
    @TotalRecords INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Toplam kayıt sayısını hesapla
    SELECT @TotalRecords = COUNT(*) 
    FROM ImportBatches 
    WHERE (@Status IS NULL OR Status = @Status)
      AND (@FileName IS NULL OR FileName LIKE '%' + @FileName + '%')
      AND (@StartDate IS NULL OR UploadDate >= @StartDate)
      AND (@EndDate IS NULL OR UploadDate <= @EndDate);
    
    -- Sayfalanmış ve sıralanmış verileri getir
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    DECLARE @SQL NVARCHAR(MAX);
    
    SET @SQL = N'
    SELECT BatchID, FileName, FileTimestamp, UploadDate, TotalRecords, Status 
    FROM ImportBatches 
    WHERE (@Status IS NULL OR Status = @Status)
      AND (@FileName IS NULL OR FileName LIKE ''%'' + @FileName + ''%'')
      AND (@StartDate IS NULL OR UploadDate >= @StartDate)
      AND (@EndDate IS NULL OR UploadDate <= @EndDate)
    ORDER BY ' + QUOTENAME(@SortBy) + ' ' + @SortDirection + '
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY';
    
    EXEC sp_executesql @SQL,
        N'@Status NVARCHAR(20), @FileName NVARCHAR(255), @StartDate DATETIME, @EndDate DATETIME, @Offset INT, @PageSize INT',
        @Status, @FileName, @StartDate, @EndDate, @Offset, @PageSize;
END;
GO

-- SP: GetBatchDetails
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_GetBatchDetails')
    DROP PROCEDURE sp_GetBatchDetails;
GO

CREATE PROCEDURE sp_GetBatchDetails
    @BatchID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DetailID, BatchID, AccountCode, DetectedName, ExcelBalance, SystemBalanceAtTime, ErrorMessage
    FROM ImportDetails
    WHERE BatchID = @BatchID
    ORDER BY AccountCode;
END;
GO

-- SP: GetBatchStatistics
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_GetBatchStatistics')
    DROP PROCEDURE sp_GetBatchStatistics;
GO

CREATE PROCEDURE sp_GetBatchStatistics
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        COUNT(*) as TotalBatches,
        SUM(CASE WHEN Status = 'Processed' THEN 1 ELSE 0 END) as ProcessedCount,
        SUM(CASE WHEN Status = 'Pending' THEN 1 ELSE 0 END) as PendingCount,
        SUM(CASE WHEN Status = 'Rejected' THEN 1 ELSE 0 END) as RejectedCount,
        SUM(TotalRecords) as TotalRecords,
        MAX(UploadDate) as LastUploadDate
    FROM ImportBatches;
END;
GO

-- SP: UpdateBatchStatus
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_UpdateBatchStatus')
    DROP PROCEDURE sp_UpdateBatchStatus;
GO

CREATE PROCEDURE sp_UpdateBatchStatus
    @BatchID INT,
    @Status NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE ImportBatches 
    SET Status = @Status 
    WHERE BatchID = @BatchID;
    
    SELECT @@ROWCOUNT as RowsAffected;
END;
GO

-- SP: DeleteBatch
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_DeleteBatch')
    DROP PROCEDURE sp_DeleteBatch;
GO

CREATE PROCEDURE sp_DeleteBatch
    @BatchID INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Önce ImportDetails kayıtlarını sil
        DELETE FROM ImportDetails WHERE BatchID = @BatchID;
        
        -- Sonra ImportBatch kaydını sil
        DELETE FROM ImportBatches WHERE BatchID = @BatchID;
        
        COMMIT TRANSACTION;
        SELECT @@ROWCOUNT as RowsAffected;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

-- SP: UpdateBatchTotalRecords
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_UpdateBatchTotalRecords')
    DROP PROCEDURE sp_UpdateBatchTotalRecords;
GO

CREATE PROCEDURE sp_UpdateBatchTotalRecords
    @BatchID INT,
    @TotalRecords INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE ImportBatches 
    SET TotalRecords = @TotalRecords 
    WHERE BatchID = @BatchID;
END;
GO

/* =============================================
   SECTION 3: STORED PROCEDURES - Transaction Operations
   ============================================= */

-- SP: GetPaymentMethods
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_GetPaymentMethods')
    DROP PROCEDURE sp_GetPaymentMethods;
GO

CREATE PROCEDURE sp_GetPaymentMethods
AS
BEGIN
    SET NOCOUNT ON;
    SELECT MethodID, MethodName 
    FROM PaymentMethods 
    ORDER BY MethodName;
END;
GO

-- SP: GetCompanyAccounts
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_GetCompanyAccounts')
    DROP PROCEDURE sp_GetCompanyAccounts;
GO

CREATE PROCEDURE sp_GetCompanyAccounts
AS
BEGIN
    SET NOCOUNT ON;
    SELECT AccountID, BankName, IBAN 
    FROM CompanyAccounts 
    ORDER BY BankName;
END;
GO

-- SP: GetAllTransactions
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_GetAllTransactions')
    DROP PROCEDURE sp_GetAllTransactions;
GO

CREATE PROCEDURE sp_GetAllTransactions
    @CustomerID INT = NULL,
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT t.TransactionID, t.CustomerID, t.MethodID, t.AccountID, t.Amount, 
           t.Description, t.TransactionDate, t.CreatedBy,
           c.CompanyName, pm.MethodName, ca.BankName, u.FullName
    FROM Transactions t
    INNER JOIN Customers c ON t.CustomerID = c.CustomerID
    INNER JOIN PaymentMethods pm ON t.MethodID = pm.MethodID
    LEFT JOIN CompanyAccounts ca ON t.AccountID = ca.AccountID
    INNER JOIN AppUsers u ON t.CreatedBy = u.UserID
    WHERE (@CustomerID IS NULL OR t.CustomerID = @CustomerID)
      AND (@StartDate IS NULL OR t.TransactionDate >= @StartDate)
      AND (@EndDate IS NULL OR t.TransactionDate <= @EndDate)
    ORDER BY t.TransactionDate DESC;
END;
GO

/* =============================================
   SECTION 4: STORED PROCEDURES - Cheque Operations
   ============================================= */

-- SP: GetPortfolioCheques
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_GetPortfolioCheques')
    DROP PROCEDURE sp_GetPortfolioCheques;
GO

CREATE PROCEDURE sp_GetPortfolioCheques
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ChequeID, CompanyName, BankName, Amount, DueDate, DaysToMaturity
    FROM vw_PortfolioCheques
    ORDER BY DueDate ASC;
END;
GO

-- SP: GetAllCheques
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_GetAllCheques')
    DROP PROCEDURE sp_GetAllCheques;
GO

CREATE PROCEDURE sp_GetAllCheques
    @Status NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ch.ChequeID, ch.CustomerID, c.CompanyName, ch.BankName, ch.ChequeNumber, 
           ch.Amount, ch.DueDate, ch.Status, ch.ReceivedDate
    FROM Cheques ch
    INNER JOIN Customers c ON ch.CustomerID = c.CustomerID
    WHERE (@Status IS NULL OR ch.Status = @Status)
    ORDER BY ch.DueDate ASC;
END;
GO

-- SP: GetChequeById
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_GetChequeById')
    DROP PROCEDURE sp_GetChequeById;
GO

CREATE PROCEDURE sp_GetChequeById
    @ChequeID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ch.ChequeID, ch.CustomerID, c.CompanyName, ch.BankName, ch.ChequeNumber, 
           ch.Amount, ch.DueDate, ch.Status, ch.ReceivedDate
    FROM Cheques ch
    INNER JOIN Customers c ON ch.CustomerID = c.CustomerID
    WHERE ch.ChequeID = @ChequeID;
END;
GO

-- SP: DeleteCheque
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_DeleteCheque')
    DROP PROCEDURE sp_DeleteCheque;
GO

CREATE PROCEDURE sp_DeleteCheque
    @ChequeID INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Cheques WHERE ChequeID = @ChequeID;
    SELECT @@ROWCOUNT as RowsAffected;
END;
GO

/* =============================================
   SECTION 5: STORED PROCEDURES - Collection Note Operations
   ============================================= */

-- SP: GetCollectionNotes
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_GetCollectionNotes')
    DROP PROCEDURE sp_GetCollectionNotes;
GO

CREATE PROCEDURE sp_GetCollectionNotes
    @CustomerID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT cn.NoteID, cn.CustomerID, cn.UserID, cn.NoteText, cn.PromiseDate, cn.CreatedAt,
           c.CompanyName, u.FullName
    FROM CollectionNotes cn
    INNER JOIN Customers c ON cn.CustomerID = c.CustomerID
    INNER JOIN AppUsers u ON cn.UserID = u.UserID
    WHERE cn.CustomerID = @CustomerID
    ORDER BY cn.CreatedAt DESC;
END;
GO

-- SP: GetCollectionNotesByUser
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_GetCollectionNotesByUser')
    DROP PROCEDURE sp_GetCollectionNotesByUser;
GO

CREATE PROCEDURE sp_GetCollectionNotesByUser
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT cn.NoteID, cn.CustomerID, cn.UserID, cn.NoteText, cn.PromiseDate, cn.CreatedAt,
           c.CompanyName, u.FullName
    FROM CollectionNotes cn
    INNER JOIN Customers c ON cn.CustomerID = c.CustomerID
    INNER JOIN AppUsers u ON cn.UserID = u.UserID
    WHERE cn.UserID = @UserID
    ORDER BY cn.CreatedAt DESC;
END;
GO

-- SP: GetAllCollectionNotes
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_GetAllCollectionNotes')
    DROP PROCEDURE sp_GetAllCollectionNotes;
GO

CREATE PROCEDURE sp_GetAllCollectionNotes
    @CustomerID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT cn.NoteID, cn.CustomerID, cn.UserID, cn.NoteText, cn.PromiseDate, cn.CreatedAt,
           c.CompanyName, u.FullName
    FROM CollectionNotes cn
    INNER JOIN Customers c ON cn.CustomerID = c.CustomerID
    INNER JOIN AppUsers u ON cn.UserID = u.UserID
    WHERE (@CustomerID IS NULL OR cn.CustomerID = @CustomerID)
    ORDER BY cn.CreatedAt DESC;
END;
GO

-- SP: GetCollectionNoteById
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_GetCollectionNoteById')
    DROP PROCEDURE sp_GetCollectionNoteById;
GO

CREATE PROCEDURE sp_GetCollectionNoteById
    @NoteID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT cn.NoteID, cn.CustomerID, cn.UserID, cn.NoteText, cn.PromiseDate, cn.CreatedAt,
           c.CompanyName, u.FullName
    FROM CollectionNotes cn
    INNER JOIN Customers c ON cn.CustomerID = c.CustomerID
    INNER JOIN AppUsers u ON cn.UserID = u.UserID
    WHERE cn.NoteID = @NoteID;
END;
GO

-- SP: UpdateCollectionNote
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_UpdateCollectionNote')
    DROP PROCEDURE sp_UpdateCollectionNote;
GO

CREATE PROCEDURE sp_UpdateCollectionNote
    @NoteID INT,
    @NoteText NVARCHAR(MAX),
    @PromiseDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE CollectionNotes 
    SET NoteText = @NoteText, 
        PromiseDate = @PromiseDate
    WHERE NoteID = @NoteID;
    
    SELECT @@ROWCOUNT as RowsAffected;
END;
GO

-- SP: DeleteCollectionNote
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_DeleteCollectionNote')
    DROP PROCEDURE sp_DeleteCollectionNote;
GO

CREATE PROCEDURE sp_DeleteCollectionNote
    @NoteID INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM CollectionNotes WHERE NoteID = @NoteID;
    SELECT @@ROWCOUNT as RowsAffected;
END;
GO

/* =============================================
   SECTION 6: STORED PROCEDURES - Dashboard Operations
   ============================================= */

-- SP: GetRiskStatusDistribution
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_GetRiskStatusDistribution')
    DROP PROCEDURE sp_GetRiskStatusDistribution;
GO

CREATE PROCEDURE sp_GetRiskStatusDistribution
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RiskStatus, COUNT(*) as Count 
    FROM vw_CustomerRiskStatus 
    GROUP BY RiskStatus;
END;
GO

-- SP: GetDailyCashFlow
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_GetDailyCashFlow')
    DROP PROCEDURE sp_GetDailyCashFlow;
GO

CREATE PROCEDURE sp_GetDailyCashFlow
    @Days INT = 30
AS
BEGIN
    SET NOCOUNT ON;
    SELECT PaymentDate, MethodName, TransactionCount, TotalAmount 
    FROM vw_DailyCashFlow 
    WHERE PaymentDate >= DATEADD(day, -@Days, GETDATE())
    ORDER BY PaymentDate ASC;
END;
GO

-- SP: GetCustomerRiskStatuses
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_GetCustomerRiskStatuses')
    DROP PROCEDURE sp_GetCustomerRiskStatuses;
GO

CREATE PROCEDURE sp_GetCustomerRiskStatuses
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CustomerID, CompanyName, RiskLimit, CurrentBalance, 
           AvailableLimit, RiskStatus 
    FROM vw_CustomerRiskStatus 
    ORDER BY RiskStatus, CurrentBalance DESC;
END;
GO

-- SP: GetOverdueChequesCount
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_GetOverdueChequesCount')
    DROP PROCEDURE sp_GetOverdueChequesCount;
GO

CREATE PROCEDURE sp_GetOverdueChequesCount
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(*) as OverdueCount
    FROM Cheques 
    WHERE Status = 'Portfolio' 
      AND DueDate < GETDATE();
END;
GO

/* =============================================
   SECTION 7: TRIGGER - Audit Trail (Mevcut trigger'ı kontrol et)
   ============================================= */

-- Mevcut trigger'lar zaten var:
-- 1. trg_UpdateBalance_AfterPayment (Transactions INSERT)
-- 2. trg_Audit_TransactionDelete (Transactions DELETE)

-- Ek Trigger: Customer güncellemelerini logla
IF EXISTS (SELECT 1 FROM sys.triggers WHERE name = 'trg_Audit_CustomerUpdate')
    DROP TRIGGER trg_Audit_CustomerUpdate;
GO

CREATE TRIGGER trg_Audit_CustomerUpdate
ON Customers
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO SystemLogs (TableName, RecordID, OperationType, OldValue, NewValue, ChangedBy)
    SELECT 
        'Customers',
        i.CustomerID,
        'UPDATE',
        CONCAT('RiskLimit: ', d.RiskLimit, ', CurrentBalance: ', d.CurrentBalance),
        CONCAT('RiskLimit: ', i.RiskLimit, ', CurrentBalance: ', i.CurrentBalance),
        NULL -- ChangedBy bilgisi Customers tablosunda yok, NULL bırakıyoruz
    FROM inserted i
    INNER JOIN deleted d ON i.CustomerID = d.CustomerID
    WHERE i.RiskLimit != d.RiskLimit OR i.CurrentBalance != d.CurrentBalance;
END;
GO

/* =============================================
   SECTION 8: VIEW Kontrolü (Mevcut view'lar zaten var)
   ============================================= */

-- Mevcut view'lar:
-- 1. vw_CustomerRiskStatus
-- 2. vw_DailyCashFlow
-- 3. vw_PortfolioCheques
-- 4. vw_RepPerformance

-- Ek View: Transaction Summary (Aylık özet)
IF EXISTS (SELECT 1 FROM sys.views WHERE name = 'vw_MonthlyTransactionSummary')
    DROP VIEW vw_MonthlyTransactionSummary;
GO

CREATE VIEW vw_MonthlyTransactionSummary AS
SELECT 
    YEAR(TransactionDate) AS Year,
    MONTH(TransactionDate) AS Month,
    DATENAME(MONTH, TransactionDate) AS MonthName,
    COUNT(TransactionID) AS TransactionCount,
    SUM(Amount) AS TotalAmount,
    AVG(Amount) AS AverageAmount,
    MIN(Amount) AS MinAmount,
    MAX(Amount) AS MaxAmount
FROM Transactions
GROUP BY YEAR(TransactionDate), MONTH(TransactionDate), DATENAME(MONTH, TransactionDate);
GO

PRINT 'Tüm Stored Procedure''ler, Trigger''lar ve View''lar başarıyla oluşturuldu/güncellendi.';
GO

