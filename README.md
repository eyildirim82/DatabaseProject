# Distributor Finance Database Project

<p align="center">
  <strong>Database systems coursework combining a normalized Microsoft SQL Server schema with an ASP.NET Core MVC application.</strong>
</p>

<p align="center">
  <a href="https://github.com/eyildirim82/DatabaseProject/actions/workflows/dotnet-build.yml"><img src="https://github.com/eyildirim82/DatabaseProject/actions/workflows/dotnet-build.yml/badge.svg" alt=".NET Build" /></a>
  <img src="https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white" alt=".NET 10" />
  <img src="https://img.shields.io/badge/SQL%20Server-relational%20database-CC2927?logo=microsoftsqlserver&logoColor=white" alt="Microsoft SQL Server" />
</p>

A finance-oriented database project for a distributor workflow, covering customers, company accounts, financial transactions, cheques, card-installment records, collection notes, imports, users/roles and audit-oriented data structures.

The project is preserved as an **academic portfolio project** demonstrating relational modeling, SQL Server DDL, constraints/views and application-level database access from ASP.NET Core.

## Academic context and attribution

Developed for **CSE3055 — Database Systems** as collaborative coursework.

The original SQL submission header identifies the group members as:

- Doğukan Demir
- Erkan Yıldırım
- Fatih Kaba

This repository is presented as a record of that group project and its application implementation; it is not presented as a solo coursework submission.

## Engineering highlights

| Area | What the repository demonstrates |
| --- | --- |
| **Relational design** | SQL Server schema with primary/foreign keys and normalized business entities |
| **Financial modeling** | Customers, company accounts, transactions, cheques, installments and collection notes |
| **Data integrity** | `UNIQUE`, `CHECK`, `DEFAULT`, identity columns and computed values |
| **Reporting model** | SQL views such as customer risk status for operational/reporting queries |
| **Audit-oriented design** | `SystemLogs` plus later database update scripts for change tracking |
| **Application access** | ASP.NET Core MVC with a DAL built on `Microsoft.Data.SqlClient` / ADO.NET-style access |
| **Spreadsheet workflows** | ClosedXML integration for Excel-oriented application flows |
| **Verification** | GitHub Actions restores and compiles the .NET 10 application in Release configuration |

## Core schema

The main SQL schema includes entities such as:

- `UserRoles`
- `PaymentMethods`
- `CompanyAccounts`
- `AppUsers`
- `Customers`
- `Transactions`
- `Cheques`
- `CreditCardInstallments`
- `CollectionNotes`
- `SystemLogs`
- import batch/detail structures added by the project scripts

Representative integrity rules include:

- unique usernames, account codes and IBAN values;
- non-negative customer risk limits;
- positive transaction amounts in the original schema;
- controlled cheque status values;
- foreign-key relationships between users, customers, accounts, payment methods and financial records;
- computed available-risk values based on customer risk limit and current balance.

## Application architecture

```text
Browser
  │
  ▼
ASP.NET Core MVC (.NET 10)
Controllers + Razor Views
  │
  ▼
DAL / Microsoft.Data.SqlClient
  │
  ▼
Microsoft SQL Server
DistributorFinanceDB
```

The application uses the configured `DefaultConnection` and opens SQL Server connections through the DAL classes rather than using an ORM.

## Project structure

```text
├── Controllers/              # MVC request handling
├── DAL/                      # SQL Server data-access layer
├── Models/                   # Application/view models
├── Views/                    # Razor MVC views
├── Pages/                    # Additional Razor content
├── Filters/                  # MVC filters
├── quries.sql                # Main coursework schema / DDL submission
├── database_update.sql       # Follow-up database updates and audit logic
├── appsettings.json          # Local SQL Server Express configuration
├── DatabaseProject.csproj    # .NET 10 web project
└── CSE3055_*                 # Coursework documentation
```

## Tech stack

- ASP.NET Core MVC
- .NET 10 / C#
- Microsoft SQL Server
- `Microsoft.Data.SqlClient`
- ClosedXML
- Razor views
- GitHub Actions

## Verification

Pull requests and pushes to `main` run a clean .NET build:

```bash
dotnet restore DatabaseProject.csproj
dotnet build DatabaseProject.csproj --configuration Release --no-restore
```

This verifies that the ASP.NET Core application and its package references compile on a clean runner. The CI job intentionally does **not** claim end-to-end database verification: runtime database workflows still require a reachable Microsoft SQL Server instance with the project schema applied.

## Running locally

### Requirements

- .NET 10 SDK
- Microsoft SQL Server or SQL Server Express

The default development configuration expects a local SQL Server Express instance and a database named `DistributorFinanceDB`:

```text
Server=.\SQLEXPRESS;Database=DistributorFinanceDB;Trusted_Connection=True;TrustServerCertificate=True
```

Prepare the database with the included SQL scripts, then run:

```bash
dotnet restore
dotnet run
```

Additional coursework, schema and architecture documentation is included in the repository.

## Portfolio note

The repository is useful primarily as evidence of database design and server-side data-access work: it shows the relational model, integrity constraints, reporting views, SQL Server access layer and the web application that consumes that model. It should be evaluated as an academic database/application project rather than as a production finance product.
