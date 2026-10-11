# ClaimPilot — Build & Release Guide

ClaimPilot is an unpackaged WinUI 3 desktop app (C# / .NET 8 / Windows App SDK 1.8).
It is 100% on-device: no cloud calls, no accounts. All data lives in
`ApplicationData.Current.LocalFolder` (SQLite `claimpilot.db`, `Vault/` folder,
`LocalSettings`).

## 1. Prerequisites

- Windows 10 version 2004 (build 19041) or later; Windows 11 recommended.
  (On-device AI features additionally need a compatible NPU/CPU and Windows AI
  components — the app degrades gracefully when they are absent.)
- Visual Studio 2022 17.8+ with ".NET desktop development" and
  "Windows application development" workloads, **or** the .NET 8 SDK
  (`dotnet --version` → 8.x) on a Windows machine.
- The `winapp` CLI (Hatch tooling) for the pack step.

## 2. Restore, build, run (Visual Studio)

1. Open `ClaimPilot.sln`.
2. Restore NuGet packages (automatic on build):
   - `Microsoft.WindowsAppSDK` **1.8.251106002**
   - `CommunityToolkit.Mvvm` **8.2.2**
   - `QuestPDF` **2026.9.1**
   - `Microsoft.Data.Sqlite` **8.0.0**
3. Select **x64** (or **ARM64**) + **Debug**, press F5.

## 3. Publish + pack (CI / command line, Windows runner)

```powershell
# from src/ClaimPilot/
dotnet publish -c Release -r win-x64 --self-contained -o ./publish
winapp pack ./publish --manifest ./Package.appxmanifest
```

For ARM64, replace `-r win-x64` with `-r win-arm64` and set
`ProcessorArchitecture="arm64"` in `Package.appxmanifest`.

## 4. Publisher CN — PLACEHOLDER, must be replaced before Store submission

`Package.appxmanifest` currently declares:

```xml
<Identity Name="StJohnPictures.ClaimPilot" Version="1.0.0.0"
          Publisher="CN=St. John Pictures" />
```

**`CN=St. John Pictures` is a placeholder.** Before submitting to the Microsoft
Store, replace it with the **exact** Publisher CN shown in Partner Center
(Account settings → Organization profile). The package identity name
`StJohnPictures.ClaimPilot` must also match the reserved Store app identity.

## 5. Limited Access Feature (LAF) tokens — NEVER fabricate

The manifest declares the `systemai:systemAIModels` capability so the packaged
app can use on-device AI models. **Never fabricate, guess, or hardcode a
Limited Access Feature token.** On-device AI through the Windows App SDK
(`Microsoft.Windows.AI`) requires no developer-supplied LAF token. If a future
API genuinely requires one, obtain it through official Microsoft channels and
store it via the Secure Vault — never in source.

## 6. QuestPDF Community license

`App.OnLaunched` sets `QuestPDF.Settings.License = LicenseType.Community`.
The Community license covers the use described in QuestPDF's licensing terms;
if the publisher's revenue or usage exceeds the Community tier thresholds,
purchase the appropriate QuestPDF license. Re-check QuestPDF's current terms
before each Store release.

## 7. API / behavior notes and uncertainties

- **On-device AI** (`Services/AiService.cs`): uses `LanguageModel.GetReadyState()`,
  `EnsureReadyAsync()`, `CreateAsync()`, `GenerateResponseAsync(string)` → `.Text`,
  and `new TextSummarizer(model)` → `SummarizeAsync(string)` → `.Text`, per the
  Windows App SDK 1.8 AI API surface. Everything is wrapped in try/catch; when
  `GetReadyState() != AIFeatureReadyState.Ready`, AI buttons render disabled with
  an explanatory tooltip. If a future SDK revision renames these members, only
  `AiService.cs` needs to change — the rest of the app is isolated from it.
- **Toast reminders** (`Services/NotificationService.cs`): uses
  `Microsoft.Windows.AppNotifications` + `Builder`. Registration and `Show()` are
  both best-effort try/catch; if toasts are unavailable on a given machine, the
  in-app Deadlines list remains the source of truth.
- **File pickers** use `WinRT.Interop.WindowNative.GetWindowHandle` +
  `InitializeWithWindow.Initialize` against `App.MainWindow`.
- **No network calls**: the app never calls any web API. Court-website
  `HyperlinkButton`s open the user's browser only when clicked.
- **94 districts**: `Data/Districts.cs` exposes exactly 94 `DistrictInfo` entries
  (`Districts.Count == 94`, asserted in the Court Finder view model). American
  Samoa is a separate special entry stating it has no federal district court.
  Note: the original task checklist misclassified New York (actually 4 districts:
  E.D./N.D./S.D./W.D.N.Y.) and Ohio (actually 2 districts: N.D./S.D. Ohio);
  the code implements the real 94.
- **Legal safety**: the exact first-run disclaimer text lives in
  `Services/LegalText.cs` and is shown in a non-dismissable `ContentDialog`
  (acceptance in `LocalSettings`), on the Dashboard, and on the About page.
  The app never claims to be a lawyer, never predicts case outcomes, and never
  files anything. Filing fees are never hardcoded anywhere.
- **Bankruptcy Plus IAP** (`Services/Pricing.cs`, `Services/PurchaseService.cs`,
  `Views/BankruptcyPage.xaml`): prices live only in `Pricing` — base $14.99,
  Bankruptcy Plus $9.99 one-time, durable add-on product ID
  `ClaimPilot.BankruptcyPlus`. `PurchaseService` uses the real
  `Windows.Services.Store.StoreContext` API (`GetAppLicenseAsync` /
  `RequestPurchaseAsync`), all calls try/catch-guarded for sideloaded or
  offline machines. Partner Center one-time setup: create a durable add-on
  under the ClaimPilot app with product ID `ClaimPilot.BankruptcyPlus`,
  price $9.99. The bankruptcy modules render behind an unlock gate until the
  license is detected.
- **Required-courses honesty** (`Data/BankruptcyGuides.cs`,
  `ViewModels/BankruptcyViewModel.cs`): the app NEVER issues course
  certificates and says so prominently — only U.S. Trustee-approved providers
  can, via the Trustee's Certificate Generation System. The page links the two
  official UST approved-provider lists (credit counseling and debtor
  education). Course records persist in a `Courses` SQLite table; certificate
  files are copied into `LocalFolder\certificates`.

## 8. Assets

`Package.appxmanifest` references `Assets\*.png` logos. Drop the final tile,
logo, splash, and `.ico` artwork into `src/ClaimPilot/Assets/` before packing.
