@echo off
Rem This is essentially an alias to generate a migration for all the db contexts
dotnet ef migrations add -p CentCom.Common -s CentCom.Server -c NpgsqlDbContext -o Migrations/Postgres %1
dotnet ef migrations add -p CentCom.Common -s CentCom.Server -c MySqlDbContext -o Migrations/MySql %1
dotnet ef migrations add -p CentCom.Common -s CentCom.Server -c MariaDbContext -o Migrations/MariaDb %1