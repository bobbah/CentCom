rmdir /Q /S publish
rmdir /Q /S CentCom.API\bin
rmdir /Q /S CentCom.API\obj
rmdir /Q /S CentCom.Common\bin
rmdir /Q /S CentCom.Common\obj
rmdir /Q /S CentCom.Server\bin
rmdir /Q /S CentCom.Server\obj
rmdir /Q /S CentCom.Bot\bin
rmdir /Q /S CentCom.Bot\obj
dotnet publish CentCom.API -o publish/linux-x64/CentCom.API/ -r "linux-x64" --self-contained false
7z a publish/CentCom.API-linux-x64.zip -r ./publish/linux-x64/CentCom.API/*
dotnet publish CentCom.Server -o publish/linux-x64/CentCom.Server -r "linux-x64" --self-contained false
7z a publish/CentCom.Server-linux-x64.zip -r ./publish/linux-x64/CentCom.Server/*
dotnet publish CentCom.Bot -o publish/linux-x64/CentCom.Bot -r "linux-x64" --self-contained false
7z a publish/CentCom.Bot-linux-x64.zip -r ./publish/linux-x64/CentCom.Bot/*
dotnet publish CentCom.API -o publish/win-x64/CentCom.API/ -r "win-x64" --self-contained false
7z a publish/CentCom.API-win-x64.zip -r ./publish/win-x64/CentCom.API/*
dotnet publish CentCom.Server -o publish/win-x64/CentCom.Server -r "win-x64" --self-contained false
7z a publish/CentCom.Server-win-x64.zip -r ./publish/win-x64/CentCom.Server/*
dotnet publish CentCom.Bot -o publish/win-x64/CentCom.Bot -r "win-x64" --self-contained false
7z a publish/CentCom.Bot-win-x64.zip -r ./publish/win-x64/CentCom.Bot/*
