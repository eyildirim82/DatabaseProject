# Distributor Finance Database Project

A database systems coursework project built around a distributor finance workflow, combining a normalized Microsoft SQL Server schema with an ASP.NET Core web application.

## Academic Context

Developed for **CSE3055 — Database Systems**.

The project focuses on relational database design, physical implementation and application-level access to financial and customer data.

## Database Highlights

- Microsoft SQL Server
- 3NF relational design
- Customer and company account management
- Income / expense and customer transactions
- Cheque tracking
- Credit-card installment records
- Collection notes
- Import batch tracking
- Application users and roles
- System audit logs
- Indexed query paths
- Unique, check and default constraints
- Computed columns
- Database views for reporting and operational analysis

## Main Data Model

The schema includes entities such as:

- `Customers`
- `CompanyAccounts`
- `Transactions`
- `Cheques`
- `CreditCardInstallments`
- `CollectionNotes`
- `AppUsers`
- `UserRoles`
- `PaymentMethods`
- `SystemLogs`
- `ImportBatches`
- `ImportDetails`

## Application Stack

- ASP.NET Core
- .NET 10
- C#
- Microsoft SQL Server
- Microsoft.Data.SqlClient
- ClosedXML

## Database Design

The implementation demonstrates database concepts including:

- primary and foreign keys
- normalization to Third Normal Form
- indexes
- unique constraints
- check constraints
- default values
- identity columns
- computed columns
- SQL views

## Running Locally

### Requirements

- .NET SDK compatible with the project target
- Microsoft SQL Server / SQL Server Express

The default development configuration expects a local SQL Server Express instance and a database named `DistributorFinanceDB`.

```bash
dotnet restore
dotnet run
```

Additional setup and coursework documentation are included in the repository.

## Repository Note

This repository is preserved as an academic portfolio project demonstrating relational database design and integration with a server-side web application.
