# GAP APP SPEC — FarewellPilot

**Status:** Spec only (no build authorized yet)
**Date:** 2026-10-25
**Line:** The Night Shift SRE / St. John Windows apps (WinUI 3, local-first, Gumroad membership)
**Prepared for:** Shaun Nicholas St. John
**Industry:** Funeral services (independent single-location funeral homes)

---

## 1. The app in one line

**FarewellPilot** — a local-first Windows desktop app that runs an independent funeral home's case book: every family's arrangements, paperwork clocks, contract ledger, and preneed file in one place, on the home's own PC — in a category where almost nobody will even publish a price.

## 2. The gap

Funeral home software is the most price-opaque category surveyed in any of the gap hunts, and the independent home is exactly who the opacity is aimed past.

**Nobody publishes a price:**

- Of eleven funeral management/arrangement platforms checked in July 2026, **eight had no pricing page at all** — Passare, SRS Computing, FrontRunner Professional, CRaKN, funeralOne, Parting Pro, eFuneral, and Tribute Technology; exactly one printed a dollar figure, Osiris at "$79/month and up," metered by at-need call volume. The honest planning range for a single-location independent is **roughly $150–$300/month** once forms, e-signature, website, and accounting integrations are included (https://www.founderandai.com/blog/custom-funeral-home-software-baltimore.html).
- Corroborating figures: Halcyon starts around **$295/month**, 1Director around **$350/month per location**, SRS Computing from **$2,495 one-time per user** plus add-ons, Osiris effectively ~$135/month in typical configurations (https://pickmysoft.com/blog/best-funeral-home-management-software; https://ideausher.com/blog/build-funeral-home-software-cost/).
- The published platforms sell the connected layer: family portals, e-signature, obituary websites, tribute video, marketing. A single-location home doing 60–150 calls a year needs the *case book* — arrangements, deadlines, contracts, preneed — and is quoted for the empire version.

**What paper and memory lose a small home:**

- **Paperwork clocks are real and per-case:** vital-records data for the death certificate must be gathered and filed inside the state's window; preneed money must reach trust/insurance inside the deposit window the home's state sets (the "ten-day clock" is a standard compliance refrain in the trade — https://www.founderandai.com/blog/custom-funeral-home-software-baltimore.html). These are per-case computed deadlines — a deadline engine's home turf.
- **The contract is the record:** itemized selections, cash-advance items, insurance assignments, and payments live in folders; an aging-receivables question ("which families still owe, and what did we advance?") takes an afternoon to answer.
- **Preneed is an audit file, not a drawer:** preneed contracts, funding status, and deposit proof must reconcile on demand for state examiners.
- **Aftercare is the referral engine** — follow-up calls at a week, a month, the anniversary — and in small homes it runs on the director's memory, so it silently stops when the director is busy. Which is always.

**The unserved layer is the case book itself — case file, arrangement checklist, paperwork clocks, contract ledger, preneed book, aftercare list — local and unbundled.** Family data in a small town is the most sensitive data a business holds; "it never leaves this PC" is a selling point here, not a limitation.

## 3. Target user

- **Primary:** Independent single-location funeral homes (owner/director plus a small staff), roughly 40–200 calls/year, currently on paper case files, spreadsheets, or an aging desktop program.
- **Secondary:** Small funeral homes with an attached crematory or a second chapel run from the same office PC.
- **Tertiary:** Homes quoted onto a cloud platform that churned on price or on the family-portal features they never used.

## 4. Core features (MVP, all local-first)

1. **Case file** — one case per family: decedent information, next of kin and decision-makers, service type (burial / cremation / memorial / graveside), status pipeline (First call → Arrangements → Paperwork pending → Service scheduled → Service complete → Aftercare → Closed). Case numbering automatic.
2. **Arrangement checklist** — per-case-type checklists: vital-records data gathered, obituary drafted and placed, clergy/venue confirmed, merchandise selected, cemetery/crematory scheduled, flowers, music, register book/stationery. Each item carries who confirmed it and when — the checklist is the handoff when a second director picks up the case.
3. **Paperwork clock panel** — computed clocks per case with **user-confirmed presets**: death-certificate filing window (days set from the home's own state rule), preneed deposit clock (days set by the user from their trust requirement), permit tracking. Green / orange ≤5 days / red overdue; every date shows its basis. Nothing ships as legal fact — same user-confirmed pattern as DenialPilot.
4. **Contract & payment ledger** — itemized contract lines (professional services, facilities, merchandise, cash advances), payments received, insurance-assignment tracking (filed / pending / paid), balance due with aging view. Print/PDF family statement and file copy.
5. **Preneed book** — preneed contracts: purchaser, beneficiary, funded status (trust/insurance), deposit log with dates and amounts, transfer/cancellation notes. The examiner-ready reconciliation view: every preneed dollar has a deposit entry.
6. **Document vault (local)** — attach copies/paths of death certificates, veteran discharge papers, obituary text, signed contracts to the case. Everything stays in the local store.
7. **Aftercare follow-up list** — per closed case, scheduled follow-ups (1 week / 1 month / anniversary — user-set cadence) with done flags and a "due this week" view. The referral engine, written down.
8. **Reports** — cases by type and month, receivables aging, preneed totals, cash-advance summary for the accountant (CSV/PDF export).

**Platform rules (same as the rest of the line):** local JSON storage, no network calls, no account, no telemetry, works fully offline. Family data never leaves the home's PC — for this industry, the privacy posture *is* the product.

## 5. What it deliberately does NOT do

- **No family portal, no e-signature, no online arrangements.** That connected layer is the platforms' product and their price; FarewellPilot records the signed paperwork, it doesn't collect it.
- **No obituary website, tribute pages, or webcasting.**
- **No embalming prep-room or crematory operations logs** — different workflow, different buyer moment; MVP is the business case book.
- **No accounting system.** Ledger and exports only; no trust-account banking functions — deposits are *recorded*, never moved.
- **No legal or regulatory advice.** Clock presets are user-confirmed from the home's own rules; screens carry a general-information disclaimer like ClaimPilot's.
- **No payment processing.** No card data ever touches the app.

## 6. Nearest incumbents — and how FarewellPilot differs

| Incumbent | What it is | Price | FarewellPilot's difference |
|---|---|---|---|
| **Passare** | Cloud case management with family collaboration portal; 2,400+ homes | Custom quote only (no published price) | Passare's value is the connected family experience. FarewellPilot is the offline case book for homes whose families walk in the door — at a published flat membership instead of a demo-gated quote. |
| **Osiris (Funeral Directors Resource)** | The category's budget option; case management, obituary tools, receipts | From $65–79/mo, metered by call volume | Osiris meters by the number of families served — the busier your worst month, the bigger the bill. FarewellPilot is flat, local, and centered on clocks/ledgers (paperwork deadlines, preneed deposit proof, receivables aging) rather than obituary production. |
| **The paper case file + spreadsheet (DIY status quo)** | Folders per family, preneed in a drawer, aftercare in the director's head | Free + risk | This is the real incumbent. FarewellPilot is that system finished: computed paperwork clocks with overdue escalation, a preneed book that reconciles deposits to contracts, and an aftercare list that survives a busy week. |

## 7. Suggested Gumroad price

**$19.99/month membership** — top of the line's band. Rationale: the comparison set effectively starts at $150–300/month all-in and hides even that behind demos; a home that runs one additional well-documented case a year covers the membership many times over. Priced as a business tool for a licensed, revenue-producing firm — same tier as LienPilot and BayPilot.

## 8. Candidates considered (scored 1–5; 20 max)

| Candidate | Pain frequency | Incumbent weakness | Local-WinUI feasibility | Willingness to pay | Total |
|---|---|---|---|---|---|
| **A. FarewellPilot — funeral home case & preneed book** | 4 | 5 | 4 | 4 | **17** |
| B. LienPilot — self-storage delinquency & lien-sale manager | 4 | 5 | 5 | 4 | 18 |
| C. RemodelPilot — remodeler job costing & change orders | 5 | 4 | 5 | 4 | 18 |
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

- **A (17):** Incumbent weakness is at ceiling — a category where 8 of 11 vendors refuse to publish any price is a category telling small buyers to go away (https://www.founderandai.com/blog/custom-funeral-home-software-baltimore.html). Feasibility is 4, not 5: the workflow is fully buildable, but forms vary by state, so the MVP keeps forms as document attachments and checklists rather than generated filings.
- **F (13):** The AMS moat is IVANS carrier downloads; without them a tracker is a companion, not a replacement, and solo-agent willingness to pay caps low (cheapest published AMS entry is already $99/mo including one user — https://www.founderandai.com/blog/custom-insurance-agency-software-baltimore.html).
- **K (10):** Cleaning is served by a genuinely cheap purpose-built tool — ZenMaid from $19/mo with recurring scheduling built for maid services (https://rankite.com/blog/best-software-for-cleaning-business). No pricing gap to sell into.
- **M (9):** My Music Staff is flat $14.95/mo, purpose-built, and well-liked (https://www.softwareworld.co/software/my-music-staff-reviews/). The bottom of this market is already owned.

## 9. Build notes (for whoever builds it — not part of this spec task)

- Reuse proven patterns: CasePilot-style case files and status pipelines, DisputePilot's deadline countdown (green/orange/red), DenialPilot's user-confirmed deadline presets for the paperwork clocks, ClaimPilot's disclaimer language pattern.
- Decimal money math throughout; store ISO dates, render localized; clock math must be locale-safe.
- No shared Licensing/voice/integrity code, consistent with the v40 apps. Ship 100 languages the same way as the rest of the line.
- First-run sample case (a fictional family and a fictional preneed contract, clearly labeled sample) so buyers see the checklist, clocks, and preneed reconciliation working before entering anything real.
- Tone rule for all copy: this industry runs on dignity. No growth-hack language, no "close more families." Sell order, deadlines kept, and records that reconcile.
