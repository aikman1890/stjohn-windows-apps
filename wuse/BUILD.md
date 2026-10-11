# wuse — Build & Packaging Guide

wuse is a WinUI 3 desktop app (C# / .NET 8 / Windows App SDK 1.8). It is
Windows-only: it cannot be compiled on Linux or macOS.

## Prerequisites

- **Windows 10 version 1809 (build 17763) or later** to build and run.
  On-device AI (Windows AI Foundry / Phi Silica) additionally needs
  **Windows 11 24H2 (build 26100+) on a Copilot+ PC**. Without that hardware,
  wuse runs in local mode — everything except the language model still works.
- **Visual Studio 2022 17.10+** with the ".NET desktop development" workload
  (includes the Windows App SDK), **or** the .NET 8 SDK plus the Windows SDK.
- .NET 8 SDK (`dotnet --version` should report 8.x).

## 1. Restore and build

From the repo root:

```powershell
dotnet restore Wuse.sln
dotnet build src\Wuse\Wuse.csproj -c Release -r win-x64
```

For ARM64: `-r win-arm64`.

## 2. Publish (unpackaged exe)

The project sets `<WindowsPackageType>None</WindowsPackageType>`, so publish
produces a plain unpackaged exe plus its dependencies:

```powershell
cd src\Wuse
dotnet publish -r win-x64 -o .\publish
# ARM64:
dotnet publish -r win-arm64 -o .\publish-arm64
```

## 3. Pack as MSIX

```powershell
dotnet tool install -g winapp
winapp pack .\publish --manifest .\Package.appxmanifest
```

Run from `src\Wuse` so the relative paths match. Use `.\publish-arm64` with an
ARM64 manifest (see the ProcessorArchitecture note below).

## 4. Sign and install

- **Store submission:** upload the MSIX to Partner Center; the Store signs it.
- **Sideloading:** sign with your own trusted certificate, e.g.
  `signtool sign /fd SHA256 /a /f MyCert.pfx .\wuse.msix`, then install.

## ⚠️ Publisher CN — ACTION REQUIRED BEFORE PACKING

`Package.appxmanifest` ships with a placeholder:

```xml
Publisher="CN=St. John Pictures"
```

Replace it with the **exact Subject CN from Partner Center** (or from your
signing certificate) before packing. A mismatch between the manifest Publisher
and the signing certificate makes install fail. Also update
`PublisherDisplayName` if your Store publisher name differs.

`ProcessorArchitecture="x64"` is set in the manifest Identity. For an ARM64
package, change it to `"ARM64"` (or produce one manifest per architecture).

## On-device AI notes

- The app declares the `<systemai:systemAIModels />` manifest capability and
  probes the model at runtime (`LanguageModel.GetReadyState()` /
  `EnsureReadyAsync()` / `CreateAsync()`). Windows handles user consent.
- **Limited Access Features: never fabricate a token.** wuse does not embed,
  hardcode, or invent any Limited Access Feature token anywhere. If a future
  Windows AI Foundry API requires LAF enrollment, that is a manual,
  per-publisher step done outside the app — do not check a token into source.
- Every AI call is wrapped in try/catch. When the model is unavailable, the
  chat shows an explanatory banner ("On-device AI isn't available on this PC…")
  and the app answers with local rule-based replies.

## Assets

The manifest references `Assets\Square150x150Logo.png` and
`Assets\Square44x44Logo.png`, which are included under `src\Wuse\Assets`
(copied from the repo-root `Assets` folder). Replace them with final art before
Store submission; all other logo sizes in the root `Assets` folder are available
for the Store listing.

## CI (Windows runner sketch)

```yaml
- uses: actions/setup-dotnet@v4
  with: { dotnet-version: '8.0.x' }
- run: dotnet publish src/Wuse/Wuse.csproj -c Release -r win-x64 -o ./publish
- run: dotnet tool install -g winapp
- run: winapp pack ./publish --manifest ./src/Wuse/Package.appxmanifest
# then sign with Azure Trusted Signing or a PFX secret
```

## Troubleshooting

- **NuGet restore fails on a non-Windows agent:** the csproj sets
  `<EnableWindowsTargeting>true</EnableWindowsTargeting>` so restore works;
  compile still needs Windows.
- **AI probe always "not ready":** expected on non-Copilot+ PCs and on
  Windows builds below 26100 — local mode is the designed behavior there.
- **`runFullTrust` / `Process.Start` blocked:** the MSIX must include the
  `rescap:runFullTrust` capability (already in the manifest); sideloaded
  installs may need Developer Mode or a trusted signature.
