# GAP APP SPEC — AgreementPilot

**Status:** Spec only (no build authorized yet)
**Date:** 2026-10-25
**Line:** The Night Shift SRE / St. John Windows apps (WinUI 3, local-first, Gumroad membership)
**Prepared for:** Shaun Nicholas St. John
**Industry:** HVAC / plumbing / electrical trades (small shops, 1–10 techs)

---

## 1. The app in one line

**AgreementPilot** — a local-first Windows desktop app that keeps a small trade shop's two most valuable books: the equipment record for every address it serves, and the maintenance-agreement book with its renewals and tune-ups owed — on the shop's own PC, instead of a $245–$500-per-tech platform or a generic tool that tracks neither.

## 2. The gap

Trade-shop software has a hole exactly where a small shop's recurring revenue lives. The enterprise platform tracks equipment and agreements but prices like enterprise software; the affordable generalists track jobs and invoices but treat equipment history and service agreements as things you duct-tape on yourself.

**The two walls, in the sources' own words:**

- **ServiceTitan**, the enterprise standard, runs a reported **$245–$500 per technician/month** with **$5,000–$50,000+ implementation fees** and mandatory annual contracts (https://serviceagent.ai/blogs/servicetitan-pricing-explained/; https://fieldcamp.ai/reviews/servicetitan/). A five-tech shop's year-one total is reported at $19,000–$34,000+. It genuinely does equipment history and agreement automation — at a price that steers very small shops away from its own entry tier.
- **Jobber**, the affordable default, "isn't HVAC-native. There's no equipment tracking tied to service history, no flat-rate price book, and no maintenance agreement automation worth mentioning" — a G2 reviewer running six techs: *"the moment you want anything HVAC-specific like tracking a unit's maintenance history or automating a service agreement, you're duct-taping things together"* (https://pilotsuite.co/articles/best-hvac-software-small-contractors). Housecall Pro ($49–$279/mo) is the same shape: jobs and invoices, not an equipment book.

**What falls through the hole is measurable:**

- **Replacement revenue walks.** "The average residential HVAC shop leaves **$40,000–$120,000 per year** in replacement revenue on the table because the CRM does not flag 15–20 year old systems before they fail… Your shop installed it. Your tech serviced it five times. Your CRM never told you it was about to die" (https://pipelineon.com/blog/hvac-crm-software/). Equipment history *per address* — not per customer — is named as the single biggest differentiator between a generic CRM and a trade one, and "most shops are still running on a generic one" (same source).
- **Agreements are sold and then forgotten.** Maintenance agreements are the trade's prized recurring revenue, but in a small shop the book lives in a filing cabinet and a spreadsheet: renewals lapse silently, tune-up visits owed under agreements never get scheduled in the slow season (the exact weeks the shop is paying techs to be idle), and nobody can answer "is this agreement making money?" without an afternoon of arithmetic.
- **Declined recommendations evaporate** — the same hole as BayPilot's declined-work register, in a different uniform: "we told them the compressor was weak in May" exists nowhere when the unit dies in August.

**The unserved layer is the book itself — equipment per address, agreements with renewal clocks and visits-owed, replacement-age flags, declined-recommendation follow-up — as a simple local tool** that sits beside whatever the shop invoices with. Nobody sells the book unbundled; the platform sells it wrapped in dispatch, phones, and marketing at $245+/tech.

## 3. Target user

- **Primary:** Owner-operated HVAC shops (and combined HVAC/plumbing/electrical shops), 1–10 techs, selling residential maintenance agreements, currently tracking equipment in invoice notes and agreements in a spreadsheet or filing cabinet.
- **Secondary:** Plumbing and electrical shops with agreement/membership programs (water-heater flush plans, panel-check plans) — same book, different equipment.
- **Tertiary:** Shops that left ServiceTitan on price, or were quoted out at the demo, and kept the workflow knowledge but lost the tool.

## 4. Core features (MVP, all local-first)

1. **Customer & equipment book** — per service address: every unit/system (type, brand, model, serial, install date, **age auto-computed**, location-in-home notes), with the full service history per unit. The record that generic CRMs don't keep and ServiceTitan charges enterprise rates for.
2. **Maintenance agreement register** — agreements: plan name, price, start and renewal dates, visits included vs. delivered, covered units, status (Active → Renewal due → Renewed / Lapsed). Renewal pipeline sorted by days-to-expiry with the line's green / orange ≤5 days… (orange at ≤30 days for renewals — user-set thresholds) / red overdue language.
3. **Tune-ups owed board** — visits owed under agreements, grouped by season windows the user sets (spring cooling / fall heating presets), so the slow-season call list builds itself instead of living in someone's memory.
4. **Replacement watch** — units crossing user-set age thresholds (e.g., 15 / 20 years) flagged on a watch list with their service-history summary: the shop's own replacement candidates, surfaced *before* the February failure sends the customer shopping for three quotes.
5. **Service visit log** — per visit: date, unit, work performed, readings/notes (free text), recommendations made with customer response (**approved / declined** — the declined-recommendation register, BayPilot's pattern) and next-visit date.
6. **Agreement profitability view** — per agreement and per plan: price collected vs. visits delivered and discounts honored (user-entered) — an honest operational view ("this plan costs more than it brings in"), explicitly not a deferred-revenue accounting ledger.
7. **Renewal packet** — per customer, a printable one-pager: equipment list with ages, agreement status, visit history, open recommendations — the document for the renewal conversation. CSV export of the agreement book for the accountant.
8. **Dashboard** — agreements expiring in 30/60/90 days, tune-ups owed by season, replacement-watch count, declined recommendations awaiting follow-up, lapsed-agreement list (win-back calls).

**Platform rules (same as the rest of the line):** local JSON storage, no network calls, no account, no telemetry, works fully offline. Customer data never leaves the shop's PC.

## 5. What it deliberately does NOT do

- **No dispatch board, no scheduling, no tech mobile app.** The platforms' core — and their cost. AgreementPilot is the back-office book; visits are logged after the fact.
- **No invoicing or payment processing.** Agreements record price and payment status; the shop bills in whatever it already uses.
- **No flat-rate pricebook and no parts catalog.** Recommendations and work are free-text with user-entered amounts; the app supplies no pricing data.
- **No customer texting/email marketing automation.** Printing the renewal call list is in; sending campaigns is the platforms' upsell and out of scope.
- **No GAAP deferred-revenue accounting.** The profitability view is operational arithmetic on user-entered figures, labeled as such — the accountant's system stays the books.
- **Not a replacement for InvoicePilot or BayPilot.** InvoicePilot invoices; BayPilot runs an auto shop's repair orders. AgreementPilot owns exactly one thing: the equipment-and-agreement book for trade shops.

## 6. Nearest incumbents — and how AgreementPilot differs

| Incumbent | What it is | Price | AgreementPilot's difference |
|---|---|---|---|
| **ServiceTitan** | Enterprise field-service platform: dispatch, equipment history, agreement automation, marketing | $245–$500/tech/mo + $5k–50k implementation | ServiceTitan is the full platform at platform prices, with annual contracts and a 6-month implementation reputation. AgreementPilot is the equipment-and-agreement book only, local, flat — the layer a five-tech shop actually misses, unbundled from dispatch and phones. |
| **Jobber** | Affordable jobs/invoicing platform for home services | $29–$599/mo tiers | Jobber has no equipment tracking tied to service history and no maintenance-agreement automation — its own reviewers call the workaround "duct-taping." AgreementPilot is the missing book, designed to sit beside Jobber rather than replace it. |
| **The spreadsheet + filing cabinet (DIY status quo)** | Agreements in a drawer, equipment history in invoice PDFs, renewals from memory | Free + lapsed revenue | This is the real incumbent. AgreementPilot is that system finished: renewal clocks that count down, tune-ups owed grouped by season, and a replacement watch that names the aging units before they fail. |

## 7. Suggested Gumroad price

**$14.99/month membership** — mid-band, at ClaimPilot/MovePilot tier. Rationale: one saved agreement renewal or one flagged replacement sold covers a year of membership; the honest comparison is $245+/tech/mo for the platform that does this, and the free "duct-tape" workaround inside a generic tool the shop may already pay for. Priced for a 1–10 tech shop's reality, not ServiceTitan's.

## 8. Candidates considered (scored 1–5; 20 max)

| Candidate | Pain frequency | Incumbent weakness | Local-WinUI feasibility | Willingness to pay | Total |
|---|---|---|---|---|---|
| **A. AgreementPilot — trades maintenance-agreement & equipment book** | 4 | 4 | 5 | 4 | **17** |
| B. LienPilot — self-storage delinquency & lien-sale manager | 4 | 5 | 5 | 4 | 18 |
| C. RemodelPilot — remodeler job costing & change orders | 5 | 4 | 5 | 4 | 18 |
| D. FarewellPilot — funeral home case & preneed book | 4 | 5 | 4 | 4 | 17 |
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

- **A (17):** The gap is attested from both directions: the enterprise tool's price ($245–$500/tech/mo plus five-figure implementation — https://serviceagent.ai/blogs/servicetitan-pricing-explained/) and the affordable tool's own reviewers admitting the equipment/agreement layer doesn't exist (https://pilotsuite.co/articles/best-hvac-software-small-contractors). Feasibility is 5 because the entire product is back-office records and date arithmetic — no field capture required.
- **H (12):** Septic's due-date pain is real, but purpose-built entrants already lead with it (SepticHub $99 — https://septichub.org/; SepticBossPro from $59/mo — https://www.septicbosspro.com/). Entering third with the same headline feature is not a gap play.
- **J (11):** Pool records are created poolside on a phone between stops; a desktop book asks for end-of-day re-entry, which is exactly the admin work the category's tools exist to kill (https://pipelineon.com/blog/pool-service-software/).
- **F (13):** Closest non-winner. An AMS's moat is carrier downloads, not its renewal list; a local renewal book would sit *beside* a $99+/user/mo system the agent still has to buy, which caps what the book alone can charge.

## 9. Build notes (for whoever builds it — not part of this spec task)

- Reuse proven patterns: BayPilot's customer/vehicle-file shape becomes customer/equipment-per-address; BayPilot's declined-work register becomes declined recommendations; DisputePilot's countdown states for renewal clocks (with user-set thresholds — renewals need 30/60/90-day horizons, not the 5-day dispute window).
- Decimal money math throughout; store ISO dates, render localized; age computation must be date-only (no time-zone drift on install-date anniversaries). Ship 100 languages like the rest of the line.
- No shared Licensing/voice/integrity code, consistent with the v40 apps.
- First-run sample shop (a fictional five-customer book: two agreements, one expiring, one 19-year-old furnace on the watch list, clearly labeled sample) so buyers see the renewal pipeline and replacement watch working before entering anything real.
- Hard rule for copy: never promise recovered revenue figures as results. Sell the book that doesn't forget — renewals counted down, tune-ups owed listed, aging units named — the things the evidence says small shops actually lose by lacking.
