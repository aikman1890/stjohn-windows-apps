# DayPilot — Build & Publish Guide

DayPilot is a WinUI 3 desktop app (C# / .NET 8 / Windows App SDK 1.8). It is built
**unpackaged and self-contained**, then packed for the Store with the `winapp` CLI.
All AI runs on-device (Phi Silica via `Microsoft.Windows.AI`); the app makes zero
cloud calls.

## Prerequisites

- Windows 11 (24H2 or later recommended — required for Phi Silica on-device AI)
- Visual Studio 2022 17.8+ with ".NET desktop development" and
  "Windows application development" workloads
- .NET 8 SDK
- A Copilot+ PC (NPU) if you want the on-device AI features to light up; the app
  runs fine without it and disables the AI buttons with an explanatory tooltip

## Local build (Visual Studio)

1. Open `DayPilot.sln`.
2. Restore NuGet packages (automatic on build):
   - `Microsoft.WindowsAppSDK` 1.8.251106002
   - `CommunityToolkit.Mvvm` 8.2.2
3. Select **x64** (or **ARM64**) and **Debug**, then press **F5**.
4. Notes are stored in `%LOCALAPPDATA%\Packages\...\LocalState\notes.json`,
   reminders in `reminders.json`.

## Publish + pack (CI / command line)

Run from the repository root:

```powershell
dotnet publish -r win-x64 -c Release -o ./publish
winapp pack ./publish --manifest ./Package.appxmanifest
```

Notes:

- Do **not** pass `--generate-cert` to `winapp pack`. The signing certificate comes
  from Partner Center after you associate the app (see below).
- For ARM64, publish with `-r win-arm64` instead.
- The `winapp pack` step consumes `Package.appxmanifest` directly; the
  `EntryPoint="$targetentrypoint$"` token in the manifest is resolved by winapp.

## Store publish

```powershell
winapp store publish
```

Run this **after** associating the app with Partner Center so the Store-issued
signing certificate is used.

## IMPORTANT — Publisher CN placeholder

`Package.appxmanifest` currently declares:

```xml
Publisher="CN=St. John Pictures"
```

This is a **placeholder**. Before submitting to the Store, associate the app in
Partner Center and replace the `Publisher` attribute with the **exact** Publisher
CN from your Partner Center account (character for character). A mismatched CN
will fail certification and the package will not install.

## On-device AI notes

- `AiService` probes `LanguageModel.GetReadyState()` at startup and calls
  `LanguageModel.EnsureReadyAsync()` if the model is not ready yet. Every AI call
  is wrapped in try/catch; any failure marks AI unavailable and disables the AI
  buttons (with a tooltip explaining why) instead of crashing.
- **Limited Access Feature token:** full on-device text generation with Phi Silica
  may require a Limited Access Feature (LAF) token from Microsoft on some builds.
  No token is fabricated or embedded anywhere in this source — if generation is
  gated on your machine, request/apply the token per Microsoft's Windows AI
  documentation and rebuild.

## Reminders

- Reminder due-checks run on a 60-second `DispatcherTimer` while the app is
  running, plus one immediate check at startup. Toasts fire exactly once per
  reminder (in-memory notified set).
- Reminders do **not** fire while the app is closed. Keep DayPilot running
  (it can sit minimized — `Ctrl+Shift+D` brings it back) for timely toasts.
- The natural-language parser (`Services/ReminderParser.cs`) is pure local code:
  no network, no AI. Unrecognized input defaults to tomorrow at 9:00 AM.
