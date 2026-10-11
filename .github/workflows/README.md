# Store Build & Publish Pipeline

`build-and-publish.yml` builds all four apps on GitHub's free Windows runners and
submits each MSIX to the Microsoft Store via the winapp CLI. No Windows machine needed.

## One-time setup

### 1. Create the four app reservations in Partner Center
At https://partner.microsoft.com/dashboard, create a new app for each and note its
**Product ID / Store ID** (found under the app's Identity page):
- DayPilot
- BizPilot
- wuse
- ClaimPilot

While there, copy the **Publisher CN** from any app's Identity page and replace the
`Publisher="CN=St. John Pictures"` placeholder in each app's `Package.appxmanifest`
so the manifest identity matches Partner Center exactly.

### 2. Create Partner Center API credentials
In Partner Center go to **Account settings > Users**, create an app registration
(Azure AD), and grant it access to your Partner Center account. Collect:
- Tenant ID
- Seller ID (Partner Center account ID)
- Client ID (the app registration's application/client ID)
- Client secret

### 3. Add GitHub repo secrets
In the GitHub repo: **Settings > Secrets and variables > Actions > New repository secret**.
Add these 8 secrets: DAYPILOT_STORE_ID, BIZPILOT_STORE_ID, WUSE_STORE_ID,
CLAIMPILOT_STORE_ID, STORE_TENANT_ID, STORE_SELLER_ID, STORE_CLIENT_ID,
STORE_CLIENT_SECRET (values from steps 1-2).

### 4. Push
Pushing to main auto-runs the workflow. Each app builds in its own matrix job.

## What the workflow does (per app)
1. `dotnet publish -r win-x64 -c Release -o ./publish/<app>`
2. `winapp pack` (Store certificate comes from the Partner Center app reservation)
3. `winapp store reconfigure` with the API credentials
4. `winapp store publish ./<app>.msix --appId <secret>`
