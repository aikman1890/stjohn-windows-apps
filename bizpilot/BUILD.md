# BizPilot — Build & Packaging Guide

BizPilot is a WinUI 3 desktop app (C# / .NET 8 / Windows App SDK 1.8). All data
stays on the device: SQLite database, receipt images, and settings live under
the app's local app-data folder. There are no network calls.

## Prerequisites

- Windows 10 version 1809 (build 17763) or later; build on Windows 10/11 x64 or ARM64.
- Visual Studio 2022 17.10+ with the **.NET desktop development** and
  **Windows application development** workloads.
- .NET 8 SDK.
- For CI packaging: a Windows runner with the .NET 8 SDK and the `winapp` CLI.

## Local build (Visual Studio)

1. Open `BizPilot.sln`.
2. Select the `x64` (or `ARM64`) platform and `Debug`.
3. Build → Build Solution. NuGet restores the pinned packages automatically:
   - `Microsoft.WindowsAppSDK` 1.8.251106002
   - `CommunityToolkit.Mvvm` 8.2.2
   - `Microsoft.Data.Sqlite` 8.0.0
   - `QuestPDF` 2026.9.1
4. Press F5 to run. The first launch creates
   `%LOCALAPPDATA%\Packages\StJohnPictures.BizPilot_<hash>\LocalState\bizpilot.db`
   and seeds the four tables (Clients, Invoices, InvoiceLines, Expenses).

## Command line (on Windows)

```powershell
dotnet restore
dotnet build -c Release
```

## Publish + MSIX package (CI / release)

```powershell
# x64
dotnet publish -c Release -r win-x64 -o ./publish
winapp pack ./publish --manifest ./Package.appxmanifest

# ARM64
dotnet publish -c Release -r win-arm64 -o ./publish-arm64
winapp pack ./publish-arm64 --manifest ./Package.appxmanifest
```

`winapp pack` consumes the publish output together with `Package.appxmanifest`
to produce the installable `.msix`.

## Store submission notes

- **Publisher CN (placeholder):** `Package.appxmanifest` currently declares
  `Publisher="CN=St. John Pictures"`. Before signing or submitting to the
  Microsoft Store, replace this with the EXACT Publisher CN shown in Partner
  Center (product → Product identity). The manifest CN must match the signing
  certificate exactly, or installation will fail.
- **Capabilities:** the manifest declares `runFullTrust` (restricted) and
  `systemai:systemAIModels` for the on-device AI features. Both must be
  declared in the Store submission.
- **On-device AI:** AI features use the Windows on-device language model
  (Phi Silica class). On PCs where the model is not present/ready, the app
  detects this at startup, disables the AI buttons, and shows an explanatory
  tooltip and status message. No AI call is ever made without a readiness
  check, and every AI call is wrapped in try/catch.

## Limited Access Features — do NOT fabricate tokens

BizPilot does not use any Limited Access Feature (LAF) gated API and ships no
LAF token. If a future feature requires a LAF-gated API, request access
through Microsoft's official Limited Access Features process. Never invent,
hardcode, or paste a token from an unofficial source.

## QuestPDF license

`App()` sets `QuestPDF.Settings.License = LicenseType.Community`. The QuestPDF
Community license is free for small businesses under QuestPDF's license terms —
verify the current terms at https://www.questpdf.com/license before shipping.

## Data storage layout

| Data | Location |
|---|---|
| SQLite database (`bizpilot.db`) | `LocalState\` |
| Receipt images | `LocalState\Receipts\` |
| Business profile, settings | `LocalSettings` |

Uninstalling the app removes all of the above.
