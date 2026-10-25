# GAP APP SPEC — TruckPilot

**Status:** Spec only (no build authorized yet)
**Date:** 2026-10-25
**Line:** The Night Shift SRE / St. John Windows apps (WinUI 3, local-first, Gumroad membership)
**Prepared for:** Shaun Nicholas St. John
**Industry:** Trucking (owner-operators & 1–5 truck carriers)

---

## 1. The app in one line

**TruckPilot** — a local-first Windows desktop app that keeps an owner-operator's trip records, fuel purchases, and expenses in one place all quarter, then produces the quarterly IFTA worksheet and a 4-year audit file — no dispatch suite, no ELD requirement, no vendor cloud holding the records an auditor will demand.

## 2. The gap

IFTA compliance is a records discipline problem with statutory teeth, and the tooling is shaped wrong for the smallest carriers.

**The pain is quarterly, mandatory, and punished:**

- Every IFTA licensee must file quarterly (due Apr 30 / Jul 31 / Oct 31 / Jan 31), including **zero-activity returns** — a skipped quarter costs the greater of 10% of net tax or **$50**, plus 1%/month interest, and flags audit eligibility (https://roadvault.polsia.app/blog/ifta-filing-guide-owner-operators and https://www.americantruckersllc.com/blog/ifta-filing-guide-owner-operators.html).
- Supporting records — trip sheets, fuel receipts, ELD data, filed returns — must be retained **4 years** from the return due date; audits can reach back the full 4 years, and each jurisdiction must audit 3% of its licensees annually (https://www.truckingoffice.com/blog/prevent-missing-ifta-records-for-your-audit/).
- Inadequate records are not a slap on the wrist: auditors substitute a worst-case allocation (estimated per-state miles get reassigned toward the highest-tax state, typically raising assessed tax **30–60%** on audited quarters), disallow gallons for lost fuel receipts, and flag returns whose consumed-vs-purchased reconciliation falls outside ~5% (http://myquickstart.io/news/common-ifta-mistakes-new-owner-operators/).
- The manual status quo is documented as miserable: non-ELD operators run IFTA out of spreadsheets, capturing odometer readings, fuel stops, and tolls by hand — "overwhelming for most busy owner-operators," with error risk riding on every entry (https://freedotnumber.com/the-pitfalls-of-recordkeeping-for-ifta-and-irp-requirements/). Even the vendor content admits the split: "The drivers who dread IFTA are the ones doing it manually — cross-referencing ELD printouts with gas station receipts, totaling miles by state on a calculator" (https://www.americantruckersllc.com/blog/owner-operator-quarterly-game-plan-2026.html). Expense tracking is the same story — one receipt-cohort analysis found **19.6% of 12,601 owner-operator receipts carried no category at all**, which is where audit defensibility dies (https://www.shoeboxed.com/blog/owner-operator-expense-spreadsheet).

**The incumbents are all cloud TMS suites, and the shape is the problem:**

- Every established option is a **full trucking management system** — dispatch, load boards, invoicing, settlements, ELD upsells — rented from a vendor cloud: TruckingOffice from $25/mo (1–2 trucks, PC*MILER routing extra), TruckLogics $39.95/mo, Rigbooks bookkeeping tiers $19–$49/mo for one truck (https://www.truxello.com/blog/best-tms-small-carriers.html and https://beancount.io/blog/2025/08/17/he-top-trucking-accounting-software-2025-guide).
- Their IFTA automation pitch depends on **ELD and fuel-card data feeds**. That leaves out exactly the operators with the worst paperwork: ELD-exempt trucks (pre-2000 engines), short-haul and agricultural exemptions, and leased-on owner-operators whose carrier dispatches them (they don't need a TMS at all — they need their *own* records for their *own* taxes and audit exposure).
- None of them sells the simple thing: **your trip records on your own computer, organized the way an auditor asks for them**, with the business's most sensitive data (rates, lanes, fuel spend) not sitting in a dispatch vendor's database.

**The unserved layer is single-purpose, offline, audit-shaped record-keeping for the one-truck operator** — the segment every TMS treats as the bottom tier of a dispatch product.

## 3. Target user

- **Primary:** Single-truck owner-operators under their own authority who self-file IFTA — currently on paper trip sheets or a spreadsheet, dreading the last week of every quarter.
- **Secondary:** Leased-on owner-operators (carrier dispatches, but the tax/audit record burden is theirs) and ELD-exempt operators (pre-2000 equipment, short-haul, ag) whom ELD-fed tools don't serve.
- **Tertiary:** 2–5 truck micro-fleets where the owner is also a driver and there is no back office.

## 4. Core features (MVP, all local-first)

1. **Trip record** — one entry per trip: date range, truck, driver, origin/destination, load reference, and **miles by jurisdiction** (state/province lines, including deadhead/bobtail/personal miles — all miles count). Odometer start/end with a built-in sanity check (jurisdiction miles must reconcile to odometer delta; flag gaps before the auditor does).
2. **Fuel purchase log** — date, jurisdiction, station/location, gallons, total cost, fuel type, receipt reference (photo/file path stored locally). Non-qualified-vehicle fuel is kept visibly separate (mixing it in is a known audit reversal).
3. **Quarterly IFTA worksheet engine** — per quarter: total miles and gallons by jurisdiction, fleet MPG (with the 5–10 MPG plausibility flag auditors use), taxable gallons, tax due/credit per jurisdiction using a **user-maintained jurisdiction rate table** (rates ship as editable defaults clearly labeled with their as-of date; the user confirms rates against their base-state return before filing — the app shows its math, never silent numbers). Consumed-vs-purchased reconciliation with the ~5% tolerance warning. Zero-activity quarters are first-class: the worksheet produces the zero return record instead of letting the quarter be forgotten.
4. **Deadline engine** — DisputePilot color language (green / orange ≤5 days / red overdue) on the four quarterly due dates, plus annual items the user configures (HVUT Form 2290, IRP renewal, IFTA license renewal) with user-set dates.
5. **Expense log with fixed categories** — the standard owner-operator set (fuel, maintenance/repairs, truck/trailer payments, insurance, tolls & scales, permits & licenses, ELD/compliance, lodging, communications, professional fees, tools, washes, other) chosen from dropdowns, never free text — the fix for the 19.6%-uncategorized problem. Per-mile cost rollups (total cost ÷ total miles) and per-trip profitability (revenue entered per load vs. allocated cost).
6. **Audit file export** — one export per quarter (and per 4-year window): trip records, fuel log, worksheet, and return summary in a printable/PDF packet, organized the way base-jurisdiction audits request them. Records view automatically keeps 4 years + current quarter visible and warns before any purge (purging is manual and confirmed; default is keep everything).
7. **IRP annual summary** — the same jurisdiction mileage rolls up to an annual IRP-style mileage report (user files it; TruckPilot does the totaling).
8. **Dashboard** — current quarter completeness at a glance: trips logged, miles by top jurisdictions, fuel logged, reconciliation status, days to filing deadline. The "will this quarter hurt?" screen.

**Platform rules (same as the rest of the line):** local JSON storage, no network calls, no account, no telemetry, works fully offline.

## 5. What it deliberately does NOT do

- **No dispatch, load boards, or broker integrations.** That is the TMS incumbents' game and the reason their products are the wrong shape here.
- **No ELD hardware or ELD data feeds.** MVP is manual/CSV entry by design — it serves the operators the feeds leave out. (CSV import of a generic trip/mileage export is in scope; vendor ELD integrations are not.)
- **No e-filing.** Base-state portals do the filing; TruckPilot produces the worksheet and keeps the proof. (Same posture as DenialPilot: submission methods are the authority's; the app tracks and documents.)
- **No tax advice and no guaranteed rates.** The rate table is user-confirmed data with an as-of label; screens state figures must be verified against the current base-jurisdiction return. General information and record tools only.
- **No payroll, settlements, or driver management** for fleets — past 5 trucks the user has outgrown the product and the spec says so.
- **No general accounting/ledger.** Year-end export (CSV/PDF totals by category) for the user's tax preparer is in scope; double-entry books are not.

## 6. Nearest incumbents — and how TruckPilot differs

| Incumbent | What it is | Price | TruckPilot's difference |
|---|---|---|---|
| **TruckingOffice** (truckingoffice.com) | Cloud TMS: dispatch, invoicing, settlements, IFTA reports; own ELD sold separately; routing is a PC*MILER add-on | From $25/mo (1–2 trucks), PRO $35/mo | A dispatch suite rented in a vendor cloud, priced by fleet, with IFTA as one module among many. TruckPilot is one job — records, worksheet, audit file — on the operator's own PC, no dispatch surface to learn or pay for, and it works for operators with no ELD to feed it. |
| **Rigbooks** (rigbooks.com) | Cloud bookkeeping for owner-operators: expense/revenue records, IFTA tracking and calculation | $19–$49/mo for one truck by tier | Closest in spirit, still cloud-only and tier-gated (invoicing only unlocks at the $49 Independent tier). TruckPilot is local-first with the audit packet and 4-year retention discipline built in, one membership, no tiers. |
| **The spreadsheet / paper trip sheet (DIY status quo)** | What the guides tell exempt and manual operators to run: hand-kept trip sheets, receipt envelopes, a quarterly calculator session | Free + hours + audit risk | TruckPilot is that system finished: odometer reconciliation checks, MPG plausibility flags, consumed-vs-purchased tolerance warnings, and a one-click audit packet — the checks that decide whether an audit is a nuisance or a 30–60% assessment. |

## 7. Suggested Gumroad price

**$14.99/month membership** — under TruckingOffice's $25 floor and Rigbooks' mid tiers, above the line's $9.99 single-fight tools. Rationale: one avoided late penalty ($50 + interest) or one disallowed-receipt reversal pays for months of membership; the quarterly deadline makes the value recur four times a year on a date the user already fears.

## 8. Candidates considered in this hunt (scored 1–5; 20 max)

| Candidate | Pain frequency | Incumbent weakness | Local-WinUI feasibility | Willingness to pay | Total |
|---|---|---|---|---|---|
| **TruckPilot — owner-operator IFTA & records (trucking)** | 5 | 3 | 5 | 4 | **17** |
| BayPilot — small auto repair shop work orders | 4 | 5 | 4 | 5 | 18 |
| KennelPilot — small pet boarding/daycare records | 4 | 4 | 4 | 4 | 16 |
| PlatePilot — independent restaurant recipe costing | 4 | 4 | 5 | 3 | 16 |
| Small-landlord rent/lease ledger | 4 | 2 | 5 | 2 | 13 |
| Reseller inventory & profit ledger | 4 | 2 | 5 | 2 | 13 |
| Family child care CACFP records | 4 | 2 | 3 | 2 | 11 |
| Small-church giving records | 3 | 2 | 5 | 1 | 11 |
| Cattle herd records | 3 | 2 | 3 | 3 | 11 |

Scoring notes (evidence, not vibes):

- **Trucking (17):** Incumbent weakness is only a 3 — TruckingOffice and Rigbooks are neither expensive nor hated. The gap is *shape*, not price: everything is a cloud TMS, the automation story presumes ELD/fuel-card feeds, and the ELD-exempt and leased-on segments are structurally unserved. Pain (5) and feasibility (5) carry it: quarterly statutory deadlines with penalties, a 3%-per-year audit lottery, and pure local arithmetic for the fix.
- **Landlords (13):** Spreadsheet pain is real (BiggerPockets threads, https://www.biggerpockets.com/forums/899/topics/1299957-anyone-actually-solved-the-multi-unit-tracking-chaos), but the segment is flooded with free cloud incumbents (TurboTenant/Avail/RentRedi class, plus new entrants like NestLedger) whose rent-collection hooks a local app cannot match. No gap to sell into.
- **Resellers (13):** The pain is genuine ("death piles," unknown true profit) but the market is filling at the bottom — a functioning Etsy spreadsheet economy (https://www.etsy.com/listing/4528711695/reseller-spreadsheet-app-ebay-etsy) and web entrants like AI Flip Ledger at $14.99/mo (https://aiflipledger.app/). Low willingness to pay in a side-hustle audience.
- **Child care (11):** CACFP record burden is heavy (Minnesota's own recordkeeping file list runs to dozens of required artifacts — https://education.mn.gov/MDE/dse/FNS/prog/CACFPCen/Rec/055136), but KidKare/Minute Menu is purpose-built, sponsor-integrated, and starts ~$9.95 — and the claim workflow *requires* sponsor connectivity a local app cannot replicate.
- **Churches (11):** Real spreadsheet pain, but DonorDock has a free tier, DonorPerfect/Neon start ~$99/mo at the top, and desktop incumbents (PowerChurch class) already exist; a volunteer treasurer's willingness to pay is the lowest of any industry surveyed (https://reachrightstudios.com/blog/donor-management-software/).
- **Ranching (11):** CattleMax is 25 years entrenched, rancher-built, and starts ~$12–30/mo (https://www.drovers.com/news/education/simplicity-record-keeping-all-one-hub); the work happens in the field on phones, the wrong form factor for a desktop-first line.

## 9. Build notes (for whoever builds it — not part of this spec task)

- Follow the v40 pattern exactly: clone the DisputePilot project skeleton (local JSON store, status pipeline, derived date/countdown logic) — the deadline engine and dashboard are near-direct reuses; the worksheet engine is new code and needs the most care.
- Jurisdiction data (US states + Canadian provinces) ships as an editable local table: name, tax rate, rate as-of date. **No rates are fetched or updated over the network** (the app makes no network calls); the UI must show the as-of date wherever a rate is used.
- All money/mile math in decimal, never float; every computed figure on the worksheet shows its inputs (miles, gallons, rate) so the user can defend it line by line.
- No shared Licensing/voice/integrity code, consistent with the v40 apps — no new secrets needed.
- Ship 100 languages the same way as the rest of the line (script-qualified resw tags); store ISO dates; render localized. Spanish is the highest-value second language in this market — verify its strings first.
- First-run sample: one fictional truck, one sample trip and fuel stop, clearly labeled sample data (mirrors BizPilot's sample-client pattern).
- Hard rule for copy: never promise audit outcomes or "IRS/IFTA-approved" status. Sell records an auditor can follow, deadlines that can't slip, and math that shows its work.
