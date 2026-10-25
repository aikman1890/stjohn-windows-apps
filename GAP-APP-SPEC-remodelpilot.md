# GAP APP SPEC — RemodelPilot

**Status:** Spec only (no build authorized yet)
**Date:** 2026-10-25
**Line:** The Night Shift SRE / St. John Windows apps (WinUI 3, local-first, Gumroad membership)
**Prepared for:** Shaun Nicholas St. John
**Industry:** Construction / remodeling (small residential contractors & remodelers)

---

## 1. The app in one line

**RemodelPilot** — a local-first Windows desktop app that runs a small remodeler's money: estimate becomes budget, every change order is priced and approved in writing, actual costs land against the budget live, and draws go out on schedule — on the contractor's own PC, for a flat membership instead of a $499/month platform.

## 2. The gap

Construction software splits into platforms a two-crew remodeler cannot justify and CRMs that deliberately omit the money math — leaving the segment's core job, *knowing whether this job is making money before it ends*, to spreadsheets.

**The pricing and omission wall, in the vendors' own numbers:**

- Buildertrend, the residential standard, starts at **$499/month** (https://planyard.com/blog/best-construction-management-software); JobTread runs **$159–$199/month**, Projul from $199/month (https://planyard.com/blog/best-general-contractor-software; https://projul.com/competitors/buildertrend-vs-jobnimbus/).
- JobNimbus — the cheaper, popular CRM — has **no job costing at any price point, no change orders, and no progress billing**, and charges **$20–$75 per person/month** on third-party figures (https://projul.com/competitors/buildertrend-vs-jobnimbus/). A contractor can buy the whole CRM and still not know a job's margin until the accountant closes the month.
- Buildertrend itself sells only by custom quote now, with reviewer complaints about data-export difficulty and a steep learning curve (same sources) — the platform is built for firms with an office manager, not for the owner who estimates at the kitchen table at night.

**What the spreadsheet status quo loses, specifically:**

- **Change orders are where remodels bleed.** Verbal approvals, a text thread, work done before price agreed — then a final invoice the client disputes line by line. The signed, priced, dated CO record is the difference between profit and a discount negotiation the contractor loses.
- **Margin is discovered after the job.** Costs live in receipts, the truck, and three inboxes; the estimate lives in a spreadsheet nobody updates. By the time actuals are totaled, the next two jobs are already bid at the same wrong prices.
- **Draws drift.** Deposit and milestone draws not invoiced on time are an interest-free loan to the client; small contractors invoice late because the draw schedule is in the contract folder, not in front of them.
- **Warranty callbacks scatter.** Punch items and warranty calls land in texts; a warranty book (what was promised, when it expires, what was already fixed) doesn't exist.

**The unserved layer is the job-money core — estimate, budget, change orders with an approval log, actuals, draw schedule, warranty book — as a simple local tool.** It is BayPilot's estimate/approval/declined-work pattern in a hard hat, and the CRM vendors have announced in writing that they are not coming for it.

## 3. Target user

- **Primary:** Owner-operated remodelers and small residential GCs (1–10 people) running 3–20 jobs a year, currently on spreadsheets, paper contracts, and text-message change orders.
- **Secondary:** Specialty contractors (kitchens/baths, decks, additions) who bid fixed-price work and feel CO pain hardest.
- **Tertiary:** Contractors who tried a platform and churned on price/complexity — they know the workflow; they need the money core without the portal, the Gantt suite, and the per-seat meter.

## 4. Core features (MVP, all local-first)

1. **Job file** — client, address, contract type and price, status pipeline (Bid → Won → In progress → Punch list → Complete → Warranty → Closed). Every job's money lives on its file.
2. **Estimate builder** — line items by user-editable cost categories (demo, framing, electrical, plumbing, finishes…), each with cost and markup → price; estimate **versions preserved** (v1/v2/v3) so what was bid, and when, survives renegotiation. Winning version becomes the contract and the budget.
3. **Budget vs. actual job costing** — actual costs logged per category (labor, materials, subs, permits, disposal) against the budget; live margin per job with a "going red" flag when a category overruns. This is the feature JobNimbus doesn't sell at any price.
4. **Change-order register with approval log** — every CO: description, price, category impact, status (Draft → Sent → Approved / Declined), approval date, method, and who approved. Approved COs raise the contract price and the category budget automatically; declined COs stay on file (the scope conversation, documented). Print/PDF CO with a signature line.
5. **Draw schedule** — deposit + milestone/percentage draws with amounts and trigger notes; invoiced / paid states; an "overdue to invoice" view so draws stop drifting. Final statement per job generated from the record (deliberately basic billing — see non-goals).
6. **Sub & supplier ledger** — per job: who was engaged for what, agreed price, paid to date, balance; lien-waiver/certificates received log (user-entered flags and dates) so the paperwork trail exists when the client or title company asks.
7. **Punch list & warranty book** — punch items per job with done flags; completed jobs roll into a warranty list with user-set warranty end dates and a callback log (date, issue, resolution, cost).
8. **Dashboard** — active jobs with live margin, COs awaiting approval (money sitting unsigned — the single most actionable number in the business), draws outstanding, warranties expiring.

**Platform rules (same as the rest of the line):** local JSON storage, no network calls, no account, no telemetry, works fully offline. Client and job data never leaves the contractor's PC.

## 5. What it deliberately does NOT do

- **No scheduling/Gantt suite, no crew dispatch, no time tracking.** Crew coordination happens on the phone and the job site; the money core is the gap.
- **No client portal, no selections portal, no daily-log photo stream.** That is the platforms' product and their price.
- **No accounting system and no QuickBooks sync.** CSV/PDF export of job totals for the accountant; the live-margin math is operational, not bookkeeping.
- **No estimating databases or cost books.** Lines are the contractor's own prices — RemodelPilot never presents costs as authoritative data.
- **No generic invoicing ambitions.** Draw statements and the final statement are job documents; standalone invoicing stays InvoicePilot's lane.
- **No contract legal language.** CO and statement templates carry the contractor's own terms; the app supplies structure and the approval record, not legal advice.

## 6. Nearest incumbents — and how RemodelPilot differs

| Incumbent | What it is | Price | RemodelPilot's difference |
|---|---|---|---|
| **Buildertrend** | Full residential construction platform: CRM, portal, scheduling, financials | From $499/mo, quote-only tiers | RemodelPilot is the money core only — estimate, budget, COs, draws — local and flat, at roughly two weeks of Buildertrend per year. No portal, no scheduling suite, no office manager required to run it. |
| **JobNimbus** | CRM for contractors (roofing/restoration-leaning) | ~$20–$75 per user/mo | JobNimbus has **no job costing and no change orders at any price** — the two features RemodelPilot is built around. A pipeline CRM tells you where the job is; RemodelPilot tells you whether it made money. |
| **The spreadsheet + text thread (DIY status quo)** | Estimate in Excel, COs by text, costs in a shoebox | Free + margin bleed | This is the real incumbent. RemodelPilot is that system finished: approved COs flow into the contract price and budget automatically, actuals land against categories live, and unsigned COs sit on a dashboard instead of in a text thread. |

## 7. Suggested Gumroad price

**$19.99/month membership** — top of the line's band. Rationale: one recovered change order pays for years of membership; the platform comparison starts at $159–499/mo; and the budget rival (Contractor Foreman, ~$49/mo — https://planyard.com/blog/best-general-contractor-software) is a generalist suite, not a job-money core. Business buyer with job revenue, priced like BayPilot and LienPilot.

## 8. Candidates considered (scored 1–5; 20 max)

| Candidate | Pain frequency | Incumbent weakness | Local-WinUI feasibility | Willingness to pay | Total |
|---|---|---|---|---|---|
| **A. RemodelPilot — remodeler job costing & change orders** | 5 | 4 | 5 | 4 | **18** |
| B. LienPilot — self-storage delinquency & lien-sale manager | 4 | 5 | 5 | 4 | 18 |
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

- **A (18):** Pain frequency is the highest in the hunt — every job generates estimates, change orders, and draws, not a monthly or per-incident subset. Incumbent weakness is documented in the competitors' own comparison pages: the affordable CRM omits job costing entirely (https://projul.com/competitors/buildertrend-vs-jobnimbus/), and the platform that has it starts at $499/mo (https://planyard.com/blog/best-construction-management-software).
- **G (12):** Legally-mandated pesticide records are real pain, but PestPac is quote-only enterprise and FieldRoutes ($60–130/mo est., https://www.itqlick.com/pestpac) already covers the small end with the mobile field capture a desktop can't match.
- **H (12):** Septic already has purpose-built entrants (SepticHub at $99, SepticBossPro from $59/mo — https://septichub.org/; https://www.septicbosspro.com/) whose headline feature is the due-date engine itself. The gap is filled; we'd be a me-too.
- **J (11):** Pool service is route-and-field work; Skimmer's per-pool meter genuinely hurts at scale ($2/pool → $800/mo at 400 pools — https://pipelineon.com/blog/pool-service-software/), but the record is created poolside on a phone, not at a desk.

## 9. Build notes (for whoever builds it — not part of this spec task)

- Reuse BayPilot's patterns wholesale: estimate line items with markup, approval log as a first-class object, declined-work register logic (declined COs), end-of-day summary → job dashboard. InvoicePilot's print/PDF statement pattern for draws.
- Decimal money math throughout — margins, markups, and draw percentages must never float-round. Store ISO dates; render localized; ship 100 languages like the rest of the line.
- No shared Licensing/voice/integrity code, consistent with the v40 apps.
- First-run sample job (a fictional kitchen remodel with two change orders — one approved, one declined — clearly labeled sample) so buyers see budget-vs-actual and the unsigned-CO dashboard working immediately.
- Hard rule for copy: never promise profit or legal protection. Sell knowing the margin while there's still time to act, and a change-order record that ends arguments — the things the evidence says small contractors actually lose by lacking.
