# Project Specification Document (PSD)

| **Project Name** | Distributor Finance Management System (DFMS) |
| :--- | :--- |
| **Course** | CSE3055 Database Systems |
| **Term** | Fall 2025 |
| **Domain** | Industrial Automation Distribution (B2B) |
| **Version** | 1.0.0 |
| **Status** | In Development |

---

## 1. Executive Summary
This project aims to develop a comprehensive financial management database application for a distributor company in the industrial electronics sector (Reference: EKSEN Endüstriyel). The system digitizes current account tracking, dynamic risk management, cheque maturity lifecycles, and automates bank reconciliation processes via Excel integration.

The core objective is to replace manual Excel-based tracking with a secure, transactional database system that enforces business rules (e.g., stopping sales when risk limits are exceeded) and ensures data integrity through ACID-compliant transactions.

## 2. Team Members
* **GrRep:** Doğukan Demir (150122539)
* **Member 2:** Erkan Yıldırım (150119509)
* **Member 3:** Fatih Kaba (150120057)

---

## 3. Functional Requirements & Modules

### 3.1. Authentication & Authorization (Auth)
* **User Management:** System access via `AppUsers` table with hashed passwords.
* **Role-Based Access Control (RBAC):**
    * `Admin`: Full system access, audit log viewing.
    * `Accountant`: Payment entry, reconciliation, cheque status updates.
    * `Sales Rep`: Read-only access to customer balances, CRM note entry.
* **Audit Logging:** Critical data changes (Deletes/Updates) are recorded in `SystemLogs` with `OldValue` and `NewValue`.

### 3.2. Customer & Risk Management
* **Customer Profile:** Management of Company Name, Tax ID, and Address.
* **Dynamic Risk Logic:**
    * Each customer has a defined `RiskLimit`.
    * `AvailableRisk` is automatically calculated (`RiskLimit - CurrentBalance`).
    * **Constraint:** The system prevents any transaction (Invoice/Cheque) that causes the balance to exceed the risk limit.

### 3.3. Financial Transactions
* **Transaction Types:** Cash, Bank Transfer, Credit Card.
* **Real-time Balance Updates:** A database trigger (`trg_UpdateBalance_AfterPayment`) automatically updates the Customer's master balance upon any transaction entry.
* **Validation:** Negative amounts are strictly prohibited via database constraints.

### 3.4. Cheque Lifecycle Management
* **Entry:** Recording post-dated cheques with `DueDate` and `BankName`.
* **Status Workflow:** Cheques transition through statuses:
    `Portfolio` -> `Collected` (Updates Balance) OR `Bounced` (Alerts Risk).
* **Reporting:** `vw_PortfolioCheques` provides a list of cheques nearing maturity.

### 3.5. Automated Reconciliation (Core Feature)
* **Excel Import:** Bulk upload of external bank/accounting statements (`.xlsx`).
* **Batch Processing:** Uploads are tracked in `ImportBatches` and `ImportDetails`.
* **Conflict Resolution:**
    * The system compares Excel balances against Database balances.
    * Differences are highlighted for user approval.
    * Approved differences update the system balance via a transactional Stored Procedure (`sp_ProcessReconciliation`).

---

## 4. Technical Architecture

### 4.1. Technology Stack
* **Backend Framework:** .NET 8.0 / ASP.NET Core MVC
* **Database Engine:** Microsoft SQL Server 2019+
* **Data Access:** ADO.NET with Stored Procedures (DAL Pattern)
    * *Note: Entity Framework is strictly avoided for core financial logic to ensure performance and explicit SQL control.*
* **Frontend:** Razor Views, Bootstrap 5, jQuery.
* **External Libraries:** `ClosedXML` (for Excel processing).

### 4.2. Database Design Statistics
* **Tables:** 12 Tables (Normalized to 3NF).
* **Stored Procedures:** 10+ (Logic encapsulation).
* **Triggers:** 2 (Balance automation & Security auditing).
* **Views:** 4 (Reporting).
* **Security:** SQL Injection protection via `SqlParameter` usage.

---

## 5. Gap Analysis & Roadmap

The following features are identified as necessary to complete the project scope:

| Feature | Status | Priority | Description |
| :--- | :--- | :--- | :--- |
| **Database Schema** | ✅ Completed | - | Tables, SPs, Triggers are created in SQL. |
| **Excel Import Backend** | 🔄 In Progress | High | `ImportBatch` logic in DAL and Controller. |
| **Excel Import UI** | ❌ Pending | High | The interface to upload files and view results. |
| **Dashboard UI** | ❌ Pending | Medium | Charts showing Risk Status and Daily Cash Flow. |
| **Transaction UI** | ❌ Pending | Medium | Forms to add Payments/Cheques with error handling. |
| **Audit Log UI** | ❌ Pending | Low | Admin screen to view `SystemLogs` table. |

---

## 6. Business Rules & Constraints

1.  **Risk Limit Integrity:** A customer's `CurrentBalance` can never exceed `RiskLimit` through manual entry.
2.  **Reconciliation Safety:** A newer reconciliation file cannot be processed if it is older than the last processed file timestamp.
3.  **Data Immutability:** `Transactions` cannot be physically deleted by standard users; they are soft-deleted or reverse-entered (Audit Trigger active).
4.  **Cheque States:** A cheque cannot jump from `Portfolio` to `Bounced` without a manual intervention record.