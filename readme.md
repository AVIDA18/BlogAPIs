--------------------------------------------Code to run Project----------------------------------
CLONE THE PROJECT
Open project and open appsettings.json and replace the connection string with your own.
Requirements: install dot net sdk 8 or 9 or any.
open terminal and goto main project file from terminal.
Use these commands:
  -dotnet tool install --global dotnet-ef
  -dotnet add package Microsoft.EntityFrameworkCore
  -dotnet add package Microsoft.EntityFrameworkCore.Design
  -dotnet add package Microsoft.EntityFrameworkCore.SqlServer **SqlServer OR PostgreSQL.
  -dotnet build
  -dotnet ef migrations add JustClonedBlogsApi
  -dotnet ef database update
  -dotnet run

Now goto http://localhost:5092/swagger/index.html.
