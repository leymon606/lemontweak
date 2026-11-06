@echo off
title lemonistan Standalone Derleyici
echo [lemonistan] Standalone tek dosya .exe derleniyor (Runtime gomulu)...
echo.
dotnet publish lemonistan.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o .\publish
echo.
if %errorlevel% equ 0 (
    echo [BASARILI] Derleme tamamlandi! 
    echo Dosya konumu: publish\MoliTweak.exe
    echo Bu dosyayi herhangi bir bilgisayara atip direkt calistirabilirsiniz (.NET Runtime istemez).
) else (
    echo [HATA] Derleme sirasinda hata olustu.
)
pause
