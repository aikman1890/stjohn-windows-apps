# Security Audit — St. John Pictures Apps & Games

**Date:** 2026-10-10
**Scope:** all 4 Windows apps (`daypilot/`, `bizpilot/`, `wuse/`, `claimpilot/` — every `.cs`, `.xaml`, `.csproj`) and the game factory (`engine.js`, `netplay.js`, `netplay-server/`).
**Method:** full-text grep for `HttpClient`, `HttpWebRequest`, `WebClient`, `TcpClient`, `UdpClient`, `Socket`, `WebSocket`, `Process.Start`, registry access, `eval(`, `new Function(`, `fetch(`, `XMLHttpRequest`, remote script loading, and telemetry/analytics SDK identifiers; manual review of every hit; manifest capability review.

## Verdict

**No backdoors. No remote-access mechanisms. No telemetry. No analytics SDKs. No obfuscated code.**
Every network touchpoint found is user-initiated, documented, and narrow. All four apps work fully offline except Microsoft Store purchases (ClaimPilot's Bankruptcy Plus) and real-time multiplayer lobbies (games). None of the four app manifests even declares the `internetClient` capability — declared capabilities are only `runFullTrust` (desktop-bridge process model) and `systemAIModels` (on-device Phi Silica).

## Per-touchpoint findings

### ClaimPilot
| Touchpoint | Location | Classification | Justification |
|---|---|---|---|
| `StoreContext.GetAppLicenseAsync` / `RequestPurchaseAsync` | `Services/PurchaseService.cs` | **Legitimate** | Microsoft Store purchase API for the Bankruptcy Plus add-on. Try/catch-guarded; returns graceful failure offline. |
| `CreatorUnlockService` (LocalSettings salted SHA-256 PIN) | `Services/CreatorUnlockService.cs` | **Legitimate** | Documented local-only creator feature. Zero network calls, zero remote activation, no master key, no hard-coded PIN. Cannot grant anything to any other user. |
| `Launcher.LaunchUriAsync` → `justice.gov/ust/...` (2 URLs) | `Views/BankruptcyPage.xaml.cs` | **Legitimate** | User-clicked links to the official U.S. Trustee approved-provider lists. Opens in the user's own browser. |
| `Launcher.LaunchFileAsync` (certificate PDFs/images) | `ViewModels/BankruptcyViewModel.cs` | **Legitimate** | Opens the user's own vaulted certificate file in their default viewer. Local only. |
| SQLite + `LocalFolder\certificates` | `Services/DatabaseService.cs` | **Legitimate** | All data stays on-device. No cloud sync. |

### DayPilot / BizPilot
| Touchpoint | Location | Classification | Justification |
|---|---|---|---|
| `Microsoft.Windows.AI` / `Microsoft.Windows.AI.Text` (Phi Silica) | `Services/AiService.cs` (each) | **Legitimate** | On-device language model. No network. Guarded: AI UI disables with explanation when the model is unavailable. |
| SQLite local storage | `Services/DatabaseService.cs` | **Legitimate** | On-device only. |
| QuestPDF (BizPilot invoice PDFs) | `Services/PdfService.cs` | **Legitimate** | Local PDF generation library. No network. `LicenseType.Community` set. |

### wuse
| Touchpoint | Location | Classification | Justification |
|---|---|---|---|
| `Microsoft.Windows.AI` (Phi Silica) | `Services/AiService.cs` | **Legitimate** | On-device. Same guard pattern as above. |
| `Process.Start` — `notepad.exe`, `mspaint.exe`, `explorer.exe`, `wt.exe`→`cmd.exe` fallback | `Services/QuickActionService.cs` | **Legitimate** | Fixed allowlist only — user says "open notepad" and the app launches Notepad. The executable names are hard-coded in a dictionary; there is no path for user input to reach `Process.Start` as an arbitrary command. Every launch is user-initiated. |
| `Launcher.LaunchUriAsync` — `calculator://`, `ms-settings:`, `ms-windows-store://`, `https://www.bing.com`, `https://www.bing.com/search?q=` | `Services/QuickActionService.cs` | **Legitimate** | Fixed URI allowlist for the assistant's "open X / search Y" actions. The only user-controlled portion is the Bing search query, which is `Uri.EscapeDataString`-encoded. Opens in the user's browser. |
| `Launcher.LaunchFolderPathAsync` | `Services/QuickActionService.cs` | **Legitimate** | Opens a user-specified folder in File Explorer after verifying it exists. |
| `EasClientDeviceInformation` + `Environment` (machine info) | `Services/QuickActionService.cs` | **Legitimate** | Read-only device facts shown to the user / copied to clipboard on request. Never transmitted. |

### Game factory — engine (`engine.js`)
- No `fetch`, no `XMLHttpRequest`, no `WebSocket`, no `eval`, no `new Function`, no dynamic script injection.
- Audio logo MP3s (`ssj-sports-sting.mp3`, `stjohn-pictures-sting.mp3`) load from the local `assets/audio/` folder via `HTMLAudioElement`. No remote media.
- Best scores persist in `localStorage`. No exfiltration.

### Game factory — netplay client (`engine/netplay.js`)
- **Single WebSocket** to the game-lobby server (default `ws://localhost:8765`, configurable per game via `CONFIG.netplay.url`). Wrapped in a `typeof WebSocket` check — when unavailable (verify sandboxes), it degrades to a no-op shell and games fall back to bots.
- **Sends only game-protocol messages:** `hello` (player name), `create`/`join`/`rejoin` (room code), `ready` (green-up boolean), `start`, `input` (per-frame controller state object), `state` (host entity snapshot), `leave`, `pong` (heartbeat reply).
- **Sends nothing else.** No device fingerprinting, no file access, no chat payload beyond the player name (max 24 chars, server-truncated).
- **Fair Play gate (added 2026-10-10):** `connect()` rejects with `fairplay_required` until the player accepts the Fair Play terms (`GF.fairplay`), which are stored as a local flag only.

### Game factory — netplay server (`netplay-server/server.js`)
- Dependencies: the `ws` npm package only. No outbound connections, no telemetry, no logging services.
- HTTP surface: `GET /health` (returns `{ok, proto, rooms}`) and a plain-text banner. Nothing else.
- **Anti-cheat hooks (added 2026-10-10):** per-player input-rate monitoring, host entity speed validation, score-submission sanity checks, duplicate-session detection. Violations append to `violations.log` (JSON lines: timestamp, playerId, room, gameId, kind, detail) and auto-flag the player for ban review. No auto-ban — flags only, Shaun reviews.

## Negative findings (explicitly checked, none found)
- No `HttpClient` / `WebClient` / raw sockets in any app.
- No registry reads/writes.
- No `eval(` / `new Function(` in apps or engine.
- No telemetry, crash-reporting, or analytics SDKs or identifiers in any `.csproj`.
- No hard-coded credentials, tokens, API keys, or endpoints (beyond the documented public `.gov` link-outs and the local default `ws://localhost:8765`).
- No remote code loading in any game file.

## Offline guarantee
Everything works with no network except:
1. **Store purchases** — ClaimPilot's Bankruptcy Plus `$9.99` add-on (one `RequestPurchaseAsync` call, user-initiated).
2. **Multiplayer lobbies** — games connect to the operator's own netplay server; unreachable server ⇒ instant bot backfill, game stays fully playable.

## Creator Unlock vs. backdoor (for the record)
The Creator Unlock (`ClaimPilot/Services/CreatorUnlockService.cs`) is sometimes asked about because it bypasses a Store purchase. It is not a backdoor by any standard definition: it is documented in `BUILD.md` §7, reachable only through a physical 7-tap gesture on the About page, gated by a user-chosen PIN stored salted+hashed, makes zero network calls, performs zero remote activation, contains no master key, and is incapable of affecting any other user's machine or license.
