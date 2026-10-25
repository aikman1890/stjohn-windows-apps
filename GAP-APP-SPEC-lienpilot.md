# GAP APP SPEC — LienPilot

**Status:** Spec only (no build authorized yet)
**Date:** 2026-10-25
**Line:** The Night Shift SRE / St. John Windows apps (WinUI 3, local-first, Gumroad membership)
**Prepared for:** Shaun Nicholas St. John
**Industry:** Self-storage (independent single-facility operators)

---

## 1. The app in one line

**LienPilot** — a local-first Windows desktop app that runs a small self-storage facility's rent roll and delinquency pipeline: who owes, what notice went out when, what step is next, and whether the file would survive a wrongful-sale claim — on the operator's own PC, without a per-user cloud meter.

## 2. The gap

Self-storage management software is priced for portfolios, and the lien process it manages is the most deadline-consequential paperwork a small operator touches.

**The pricing wall:**

- SiteLink, the category standard, starts at **$150 per user/month** (https://www.itqlick.com/sitelink-software/pricing); Storable Edge starts at **$50/month** and meters by feature (https://rankite.com/blog/best-self-storage-software; https://softwarefinder.com/property-management-software/storable-edge). First-year totals for even a modest multi-user setup run into five figures once onboarding and add-ons land (https://www.itqlick.com/storedge/pricing).
- Those platforms earn their price with tenant portals, online rentals, gate integration, and payment processing — the growth machinery a 100–300 unit independent with a part-time manager does not use. What that operator actually needs from software is the rent roll and the lien file, and nobody sells just that.

**The lien process is a statutory obstacle course, and paper fails it:**

- Every state has a self-storage lien act with the same skeleton — default notice, a formal lien notice by a specified delivery method, a waiting period (commonly **14–45 days** after notice), a published advertisement (often **once a week for two consecutive weeks**), then sale — and a misstep can void the sale and expose the operator to a wrongful-sale lawsuit (https://legalclarity.org/how-long-does-public-storage-give-you-to-pay-before-auction/; https://www.insideselfstorage.com/associations/new-minnesota-self-storage-lien-law-goes-into-effect-aug-1).
- The steps are clause-level: Texas requires an itemized account of the claim in the notice, delivery by verified mail or (with the right rental-agreement language) email, publication in two consecutive weeks, and no sale until the **15th day after first publication** (Tex. Prop. Code ch. 59, summarized at https://www.recoupunit.com/blog/texas-self-storage-lien-sale-timeline and https://ai-lean.com/resources/state-guides/texas-lien-laws).
- Small operators run this from a folder, a calendar, and memory. The failure mode is not laziness — it is that a 6-step, multi-week, per-tenant timeline has no home in a spreadsheet that also has to be the rent roll.

**The unserved layer is the delinquency-and-lien core — rent roll, per-tenant statutory timeline, notice proof ledger, sale-readiness check — as a simple local tool.** That is DisputePilot's deadline-engine pattern pointed at the operator side of a lien, and no incumbent sells it unbundled.

## 3. Target user

- **Primary:** Independent single-facility operators (roughly 50–400 units), owner-managed or with one part-time manager, currently on paper ledgers, spreadsheets, or an aging desktop program.
- **Secondary:** Small multi-facility owners (2–3 sites) who run each site from its own office PC and do not need portfolio rollups.
- **Tertiary:** Operators who tried SiteLink/Edge and churned on price — they know the workflow; they need the lien file without the platform.

## 4. Core features (MVP, all local-first)

1. **Rent roll / unit board** — units, tenant, rate, paid-through date, balance; status pipeline (Current → Late → Lien pending → Notice sent → Advertised → Sale scheduled → Released/Sold/Vacated). The day and the month on one screen.
2. **Delinquency clock engine** — per-tenant timeline computed from the default date plus a **user-confirmed state profile**: notice day, required delivery methods, waiting period, publication requirement, earliest lawful sale date. Green / orange ≤5 days / red overdue, same language as DisputePilot. Every computed date shows its basis ("sale eligible 15th day after first publication per your TX profile") — no silent math, and profiles ship blank with one clearly-labeled *sample* the user must verify against their own statute and counsel before use.
3. **Notice proof ledger** — every statutory step logged per tenant: notice type, date, delivery method (verified mail / certified mail / email / hand delivery), tracking number, and proof note. The app blocks a "sale ready" state while a profile-required step is unlogged ("publication step 2 not recorded") — a missing-step warning, not a legal opinion.
4. **Itemized claim statement** — late fees and lien fees per the operator's own fee schedule, itemized exactly as the notices must state the claim; print/PDF for the file and the tenant.
5. **Notice & advertisement templates** — first notice, lien notice, and sale advertisement text (general description of property, tenant name, facility address, time/place/terms of sale — the fields state guides enumerate), merged from the case file; user edits and sends by their chosen method; the send is logged in the ledger.
6. **Sale-day worksheet** — units cleared to sell vs. held (paid, bankruptcy flag, military-status flag, dispute flag — all user-set), plus a proceeds log: sale price, amount applied to the claim, and excess-proceeds held/claimed tracking.
7. **Tenant & unit history** — every tenancy, notice, payment arrangement, and outcome in one file per unit — the record that defends the sale and answers the tenant's lawyer.
8. **Dashboard** — dollars in delinquency by stage, steps due this week, sales scheduled, units by status. One screen for the owner's Monday morning.

**Platform rules (same as the rest of the line):** local JSON storage, no network calls, no account, no telemetry, works fully offline. Tenant data never leaves the facility PC.

## 5. What it deliberately does NOT do

- **No gate/access control, no tenant portal, no online rentals.** That is the platforms' growth machinery and the reason their prices start where they do.
- **No payment processing.** Payments are recorded; cards never touch the app.
- **No auction-platform integration and no sending of notices.** LienPilot prepares, tracks, and proves; the operator mails, publishes, and sells.
- **No legal advice and no shipped law.** State profiles are user-entered and user-confirmed (same pattern as DenialPilot's letter-based deadline presets); sample data is labeled sample. The app never represents a timeline as legally sufficient.
- **No portfolio/cloud rollup in MVP.** One facility per install is the gap; multi-site sync is the incumbents' game.
- **Not a replacement for InvoicePilot** (general invoicing) — the claim statement is a lien document, not a billing system.

## 6. Nearest incumbents — and how LienPilot differs

| Incumbent | What it is | Price | LienPilot's difference |
|---|---|---|---|
| **SiteLink (Storable)** | Full cloud facility platform: rentals, portal, payments, gate integrations | From $150/user/mo | LienPilot is the rent roll + lien file only, local, at a flat membership — roughly one month of one SiteLink seat buys years of LienPilot. No portal, no payments, no per-user meter. |
| **Storable Edge** | Cloud platform aimed at smaller operators, feature-metered plans | From $50/mo + feature fees | Edge still sells the platform (automated rentals, tenant comms, BI packs upsold). LienPilot's center of gravity is the statutory timeline and proof ledger, offline, with the operator's own verified state profile. |
| **The folder + spreadsheet (DIY status quo)** | Rent ledger in a book, lien steps tracked on a wall calendar | Free + risk | This is the real incumbent. LienPilot is that system finished: a computed per-tenant clock that refuses to call a unit sale-ready with a step unlogged, an itemized claim that matches the notice, and a proceeds/excess log — the artifacts a wrongful-sale defense is made of. |

## 7. Suggested Gumroad price

**$19.99/month membership** — top of the line's band, justified by consequence: one voided lien sale or one wrongful-sale claim costs more than a decade of membership, and the comparison set starts at $50–150/mo. Sits above DisputePilot's $9.99 because the buyer is a business with rent revenue, not a consumer in a dispute.

## 8. Candidates considered (scored 1–5; 20 max)

| Candidate | Pain frequency | Incumbent weakness | Local-WinUI feasibility | Willingness to pay | Total |
|---|---|---|---|---|---|
| **A. LienPilot — self-storage delinquency & lien-sale manager** | 4 | 5 | 5 | 4 | **18** |
| B. RemodelPilot — remodeler job costing & change orders | 5 | 4 | 5 | 4 | 18 |
| C. FarewellPilot — funeral home case & preneed book | 4 | 5 | 4 | 4 | 17 |
| D. AgreementPilot — trades maintenance-agreement & equipment book | 4 | 4 | 5 | 4 | 17 |
| E. MovePilot — small mover estimates, inventory & claims | 4 | 4 | 4 | 4 | 16 |
| F. RenewalPilot — solo insurance agent renewal book | 4 | 3 | 4 | 2 | 13 |
| G. ChemLog — pest-control application records vault | 4 | 3 | 2 | 3 | 12 |
| H. Septic book — pumping due-date & county records | 3 | 2 | 4 | 3 | 12 |
| I. LotPilot — towing impound & lien ledger | 3 | 2 | 3 | 3 | 11 |
| J. RouteBook — pool service route & chemistry log | 3 | 3 | 2 | 3 | 11 |
| K. MaidBook — cleaning service recurring book | 3 | 1 | 4 | 2 | 10 |
| L. PackLedger — trainer session-pack ledger | 3 | 3 | 2 | 2 | 10 |
| M. StudioBook — music teacher studio book | 2 | 1 | 4 | 2 | 9 |

Scoring notes (evidence, not vibes):

- **A (18):** The only candidate where the incumbent's own category is priced per-user at $150/mo *and* the DIY failure mode is litigation. The deadline-engine + proof-ledger pattern is already proven in this line (DisputePilot/DenialPilot), so feasibility is at ceiling.
- **F (13):** Solo agents do run renewal books in spreadsheets, but an AMS's real moat is IVANS carrier downloads ($35/mo add-on even on the $99 entry tier — https://www.founderandai.com/blog/custom-insurance-agency-software-baltimore.html), which no local app can replicate; a renewal tracker complements an AMS rather than replacing one, capping willingness to pay.
- **G (12):** Pesticide application records are legally mandated with multi-year retention, but the record is created in the field on a phone; an office desktop that requires re-entry is double work, and FieldRoutes ($60–130/mo est.) already bundles it (https://www.itqlick.com/pestpac).
- **I (11):** Towbook already calculates storage fees and reminds on state-required impound notices from its $49/mo entry tier (https://www.getapp.com/industries-software/a/towbook-management-software/), and the rest of towing value is real-time dispatch a desktop cannot do.

## 9. Build notes (for whoever builds it — not part of this spec task)

- Reuse DisputePilot's patterns wholesale: local JSON store, derived deadline countdown with green/orange/red states, status pipeline, proof-ledger notes. Reuse DenialPilot's user-confirmed-preset pattern for the state profiles — profiles are data the user owns, never shipped legal conclusions.
- Decimal money math throughout (rent, fees, proceeds); store ISO dates, render localized; deadline math must be locale-safe.
- No shared Licensing/voice/integrity code, consistent with the v40 apps. Ship 100 languages the same way as the rest of the line.
- First-run sample facility (a handful of fictional units and one fictional tenant mid-lien, clearly labeled sample) so buyers see the clock engine and missing-step block working before entering anything real.
- Hard rule for copy: never promise compliance or outcomes ("sell legally"). Sell the clock, the ledger, and the file that holds up — the things the evidence says small operators actually lose by lacking.
