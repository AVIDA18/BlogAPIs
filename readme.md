# Blogs API

A RESTful API built with ASP.NET Core for managing blogs, categories, comments, likes, and users.

## Requirements

- [.NET SDK 8+](https://dotnet.microsoft.com/download)
- SQL Server or PostgreSQL(I am using SQL server. To use postgre you have to make some changes like installing package of postgre instead of of sqlserver)

## Getting Started

### 1. Clone the project

```bash
git clone <repo-url>
cd BlogAPI
```

### 2. Configure the database

Open `appsettings.json` and replace the connection string with your own:

```json
"ConnectionStrings": {
  "DefaultConnection": "your-connection-string-here"
}
```

### 3. Install dependencies

```bash
dotnet tool install --global dotnet-ef
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

> Use `Microsoft.EntityFrameworkCore.Npgsql` instead if you are using PostgreSQL.

### 4. Build & run migrations

```bash
dotnet build
dotnet ef migrations add InitialMigration
dotnet ef database update
```

### 5. Run the project

```bash
dotnet run
```

### 6. Open Swagger

Navigate to: http://localhost:5092/swagger/index.html
