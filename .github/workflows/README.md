# Build & Publish Pipeline

`build-and-publish.yml` builds all four apps on GitHub's free Windows runners and
uploads each as a ready-to-sell ZIP containing a self-contained EXE.
Download, unzip, double-click — no installer, no certificates, no Microsoft Store,
no Partner Center, no secrets needed.

Runs on every push to `main` that touches an app (or this workflow), plus manual
dispatch from the Actions tab.

## One-time setup: the LICENSE_SECRETS repo secret

The apps verify license keys offline with per-app signing secrets. Those secrets
are NEVER committed to the repo — the build generates `Licensing/LicenseSecrets.cs`
from a repo secret at build time (see the "Restore license secrets" step).

Create it once: repo **Settings > Secrets and variables > Actions >
New repository secret**, name `LICENSE_SECRETS`, value = five lines:

```
daypilot=<64 hex chars>
bizpilot=<64 hex chars>
wuse=<64 hex chars>
wusebusiness=<64 hex chars>
claimpilot=<64 hex chars>
```

(`wusebusiness` signs wuse Business-tier keys — a separate secret so Business
keys can never verify as Personal keys and vice versa.)

(The values live in `Licensing/LicenseSecrets.cs` on Shaun's machine only.
If a secret ever leaks: generate a fresh 32-byte secret, update the repo secret,
re-issue keys with `tools/KeyGen`, and ship an app update — old keys stop verifying.)

## One-time setup: the STORE_PUBLISHER repo secret (Microsoft Store)

Each green build also packs a Store-ready `.msix` per app — but only when the
`STORE_PUBLISHER` repo secret exists. Create it once: repo **Settings > Secrets
and variables > Actions > New repository secret**, name `STORE_PUBLISHER`, value
= the exact **Publisher** string from Partner Center (any app's Identity page —
it looks like `CN=12345678-abcd-...`). Without it, the build skips the MSIX and
still uploads the direct-sale ZIPs.

## What the workflow does (per app)
1. `dotnet publish <app>.csproj -r win-x64 -c Release --self-contained true`
   (the .NET 8 runtime is bundled inside — the buyer needs nothing installed)
2. Zips the publish folder to `<app>-windows.zip`
3. Uploads the ZIP as a workflow artifact (`<app>-windows`)

Each app builds in its own matrix job (`fail-fast: false`), so one app's failure
doesn't block the others.

## Getting the ZIPs to sell
1. Open the repo's **Actions** tab, click the latest green **Build Windows apps** run.
2. Download the artifacts at the bottom of the run page:
   - `<app>-windows` — the direct-sale ZIP (unzip and run)
   - `<app>-setup-exe` — a real installer (`<app>-Setup.exe`, Start-menu shortcut,
     uninstaller included). **Silent install: `/SILENT`** (progress only) or
     `/VERYSILENT` (fully silent). Silent uninstall:
     `"%ProgramFiles%\St. John Pictures\<App>\unins000.exe" /SILENT`.
   - `<app>-store-msix` — the Microsoft Store package (only present when the
     `STORE_PUBLISHER` secret is set; upload it at Partner Center > Packages)
3. Sell the ZIPs from the website (https://aikman1890.github.io/stjohn-windows-apps/)
   — the license-key system inside each app handles trials, keys, and lockout.

## Pricing (monthly subscriptions)
- DayPilot — $2.99/month (4-day free trial)
- BizPilot — $9.99/month
- wuse — $4.99/month
- ClaimPilot — $14.99/month, plus Bankruptcy Plus $9.99/month and Legal Matters $25.99/month add-ons

Keys are issued with `tools/KeyGen` (HMAC-signed, monthly expiry).

## Notes
- The build happens on `windows-latest` runners because WinUI 3 only compiles
  on Windows — the code is written on Linux but never compiled there.
- On-device AI features (Phi Silica) degrade gracefully: if the model is
  unavailable on a machine, the AI buttons disable with an explanation and the
  rest of each app keeps working.
- QuestPDF (BizPilot, ClaimPilot) is set to `LicenseType.Community`. Confirm the
  current QuestPDF license terms cover your use before commercial sale.
