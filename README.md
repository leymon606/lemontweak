<img width="1915" height="1079" alt="image" src="https://github.com/user-attachments/assets/475afdd3-c3db-4435-ae9d-1c43587209f0" />
# lemonistan
**FiveM & Windows optimizer suite** — a Windows desktop app (.NET 8, WPF) for system tuning, game-related tweaks, and monitoring. The UI is organized into focused areas (dashboard, boost, FiveM, latency, network, Windows tweaks, profiles, restore, settings).
> **Note:** Many features change Windows services, power plans, network QoS, GPU/timer behavior, and other OS-level settings. Run with appropriate care, prefer backups, and use restore options when something does not behave as expected.
## Requirements
- **OS:** Windows 10/11 (64-bit). The project targets `win-x64`.
- **To build from source:** [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
A published build is **self-contained** (no separate .NET runtime install on the target PC).
## Build
From the repository root:
```bash
dotnet build lemonistan.sln -c Release
```
Run the app from the build output (under `bin/Release/net8.0-windows/win-x64/`), or publish a single portable executable:
```bash
dotnet publish lemonistan.csproj -c Release -r win-x64 --self-contained true ^
  -p:PublishSingleFile=true ^
  -p:IncludeNativeLibrariesForSelfExtract=true ^
  -p:EnableCompressionInSingleFile=true ^
  -o ./publish
```
On Windows you can also double-click `build_standalone.bat` to publish into the `publish` folder.
The published executable is named **`lemonistan.exe`** (see `AssemblyName` in `lemonistan.csproj`).
## Solution layout
| Area | Role |
|------|------|
| `Core/` | Tweaks, monitoring, license helpers, utilities |
| `Views/` | Tab UI (dashboard, network, FiveM, etc.) |
| `Windows/` | Main window, login/splash |
| `Styles/` | WPF themes |
## Repository
Source: [github.com/leymon606/lemontweak](https://github.com/leymon606/lemontweak)
## Disclaimer
This software is provided **as-is**. You are responsible for how you use it on your system. Relevant tweaks may require **administrator** rights and can affect stability or compatibility; review changes and use undo/restore features when available.
