# GAP APP SPEC — BayPilot

**Status:** Spec only (no build authorized yet)
**Date:** 2026-10-25
**Line:** The Night Shift SRE / St. John Windows apps (WinUI 3, local-first, Gumroad membership)
**Prepared for:** Shaun Nicholas St. John
**Industry:** Auto repair (independent 1–3 bay shops)

---

## 1. The app in one line

**BayPilot** — a local-first Windows desktop app that runs a small repair shop's day: repair orders, customer and vehicle history, estimates the customer approved, work they declined, and what's due back — on the shop's own PC, for a tenth of what the cloud shop platforms charge before the per-seat fees start.

## 2. The gap

Shop management software had a price-and-complexity reset aimed at growing multi-bay operations, and the 1–3 bay independent got priced out of its own category.

**The pricing wall is documented, in the vendors' own numbers:**

- Mainstream shop platforms start at **$179–$239/mo per shop** (AutoLeap from $179, Tekmetric Start $199, Shopmonkey Basic $239) and climb to $439–$999/mo at upper tiers (https://nextcarhub.com/auto-repair-shop-software-comparison.html, prices read off vendor pricing pages Oct 2026).
- The meters stack: Shopmonkey charges **$20/mo per additional user** past the included seats and a **$500 data migration fee**; Shop Boss charges $399–$899 for data transfer; Mitchell 1 bundles to roughly $275–$500/mo once shop management and repair information are combined (same source; https://www.founderandai.com/blog/custom-auto-repair-software-baltimore.html).
- Industry summaries are blunt about who that excludes: Shopmonkey is "not ideal for very small shops due to higher price point," and platform costs land in "the upper hundreds of dollars per month, taking some smaller garages out of the running" (https://blog.csiaccounting.com/top-shop-management-software-auto-repair-reviews-breakdown).
- The one credible cheaper entrant proves the gap by what it omits: NextCarHub at **$49/mo** does scheduling, customers, vehicles, and history — and explicitly does **not** do estimating, invoicing, parts, or inspections; "if you need those, $49 doesn't do the job that $239 was doing" (https://nextcarhub.com/auto-repair-shop-software-cost.html).

**So the smallest shops run on the remaining option: paper repair orders, a whiteboard, and the owner's memory.** What that loses them is specific and expensive:

- **Declined work evaporates.** A customer who declines brakes today is next quarter's sold job — if anyone wrote it down. The big platforms are themselves dinged for weak declined-service follow-up; paper shops have none at all.
- **Vehicle history lives in a filing cabinet** (or doesn't), so every returning car starts from zero: what was done, what was recommended, what the customer was warned about — the liability and the upsell are in the same missing record.
- **Approval proof is a phone call nobody logged.** "You said fix it" vs. "I never approved that" is a chargeback and a bad review; small shops have no estimate/approval trail.

**The unserved layer is the repair-order core — RO, customer, vehicle, estimate, approval, declined work, history — as a simple local tool**, without the cloud platform's marketing suites, texting tiers, and per-seat meters. A front-counter PC is already how these shops work; the software just never showed up at their price.

## 3. Target user

- **Primary:** Owner-operated independent shops, 1–3 bays, 1–5 people total, currently on paper/whiteboard/spreadsheet or an aging desktop program — general repair, not franchise or dealer.
- **Secondary:** Mobile mechanics and specialty one-person operations (diesel, European, classics) who need customer/vehicle history and a defensible approval record more than they need scheduling software.
- **Tertiary:** Shops that tried a cloud platform and churned on price — they know the workflow; they need it without the $200+/mo meter.

## 4. Core features (MVP, all local-first)

1. **Repair order (RO) board** — the day at a glance: vehicles in, status pipeline (Checked in → Diagnosing → Waiting approval → Approved/In progress → Waiting parts → Done → Picked up), promised time, tech assigned. Drag-free simple status moves; the whiteboard, digitized, nothing more exotic.
2. **Customer & vehicle files** — customer contact + vehicles (year/make/model, VIN, plate, mileage-in history). Every RO attaches to a vehicle; the vehicle's full history (every RO, every line, every recommendation) is one click — the record paper shops can't produce.
3. **Estimates with line items** — labor lines (hours × shop rate, editable presets), parts lines (part name/number, cost, markup % → price), shop supplies/fees, tax; totals computed and shown. Print/PDF the estimate for the customer's signature line.
4. **Approval log** — per estimate: approved/declined, date-time, method (in person / phone), who approved, notes. This is the dispute record; it is a first-class object, not a comment field.
5. **Declined-work register** — every declined line lands here with the vehicle, date, and reason if given; a follow-up view sorts declined work by age and value ("call these people back") with next-contact dates and overdue flags. The feature paper can't do and the platforms do badly.
6. **Service history & recommendations** — per vehicle: completed work, declined work, and tech notes/recommendations carried forward and surfaced automatically when that vehicle gets a new RO ("last visit: recommended rear brakes — declined").
7. **Simple invoicing from the RO** — the approved estimate becomes the invoice on completion (actual hours/parts adjustable), paid/unpaid/partial states, print/PDF. Deliberately simple: no card processing, no payment links (see non-goals).
8. **End-of-day summary** — ROs opened/closed, invoiced total, collected total, vehicles due back tomorrow, approvals still pending. One screen for the owner at closing time.
9. **Reminders view** — promised-time risk (ROs past promised time still open), follow-ups due (declined work + service-due estimates by months/mileage the *user* sets — flagged as shop estimates, not manufacturer data).

**Platform rules (same as the rest of the line):** local JSON storage, no network calls, no account, no telemetry, works fully offline. Customer data never leaves the shop's PC — for a small shop, that's a selling point against cloud platforms, not a limitation.

## 5. What it deliberately does NOT do

- **No labor guides or repair information.** Mitchell/ProDemand-style data is a licensed-data business; BayPilot never presents times as authoritative. Labor hours are the shop's own entries/presets.
- **No parts ordering or supplier catalogs.** Parts are free-text lines with cost/markup; no vendor integrations, no live pricing.
- **No customer texting/email campaigns, no marketing suite, no review solicitation.** That is the upsell tier of every incumbent and the reason their prices start where they do. (Printing a follow-up list is in; sending is the shop's business.)
- **No payment processing.** Invoices record payment; they don't take it. No card data ever touches the app.
- **No accounting system.** CSV/PDF export of invoices and totals for the shop's accountant/QuickBooks import file is in scope; ledgers are not.
- **No multi-location, no per-seat anything.** One shop, one database, any number of the shop's own PCs on their own network share is a post-MVP question — MVP is one counter PC, honestly labeled.
- **No appointment marketing website/online booking portal.** A local app can't be a web portal; the board covers walk-ins and phone bookings the shop enters itself.

## 6. Nearest incumbents — and how BayPilot differs

| Incumbent | What it is | Price | BayPilot's difference |
|---|---|---|---|
| **Tekmetric** (tekmetric.com) | Cloud shop platform: ROs, digital inspections, MOTOR labor times, live parts ordering, reporting | Start $199/mo ($179 annual), Grow $349, Scale $439; marketing add-on $345/mo | Built and priced for growing multi-tech shops; its strengths (inspections, parts network, texting tiers) are exactly what a 2-bay shop pays for and doesn't use. BayPilot is the RO/estimate/approval/history core, local, at $19.99 — no tiers, no add-on meters, no demo-gated sales process. |
| **Shopmonkey** (shopmonkey.com) | Cloud platform: estimating, scheduling, texting, inventory, profitability | Basic $239/mo + $20/mo per extra user; $500 migration fee | Per-seat pricing punishes exactly the small team BayPilot serves, and migration fees punish leaving your old system. BayPilot: one membership, the shop's data in a local file they can copy with a USB stick. |
| **NextCarHub** (nextcarhub.com) | Cloud scheduling + customer/vehicle history for small shops | $49/mo per shop | The honest budget option — and it explicitly does not do estimating or invoicing (https://nextcarhub.com/auto-repair-shop-software-cost.html). BayPilot's center of gravity is the repair order itself: estimate, approval log, declined-work register, invoice — the money documents, not just the calendar. |

## 7. Suggested Gumroad price

**$19.99/month membership** — one-tenth of Tekmetric's entry tier and under NextCarHub's $49, because the buyer is a 1–3 bay shop for whom $199/mo was already the "no." Rationale: a single recovered declined job per month (one brake job called back and sold) pays for a year of membership; price it so the decision needs no demo, no sales call, and no migration fee.

## 8. Candidates considered in this hunt (scored 1–5; 20 max)

| Candidate | Pain frequency | Incumbent weakness | Local-WinUI feasibility | Willingness to pay | Total |
|---|---|---|---|---|---|
| **BayPilot — small auto repair shop work orders** | 4 | 5 | 4 | 5 | **18** |
| TruckPilot — owner-operator IFTA & records (trucking) | 5 | 3 | 5 | 4 | 17 |
| KennelPilot — small pet boarding/daycare records | 4 | 4 | 4 | 4 | 16 |
| PlatePilot — independent restaurant recipe costing | 4 | 4 | 5 | 3 | 16 |
| Small-landlord rent/lease ledger | 4 | 2 | 5 | 2 | 13 |
| Reseller inventory & profit ledger | 4 | 2 | 5 | 2 | 13 |
| Family child care CACFP records | 4 | 2 | 3 | 2 | 11 |
| Small-church giving records | 3 | 2 | 5 | 1 | 11 |
| Cattle herd records | 3 | 2 | 3 | 3 | 11 |

Scoring notes (evidence, not vibes):

- **Auto repair (18) — the strongest incumbent-weakness score in the hunt.** The category's price floor moved to $179–$239/mo with per-seat and migration meters (https://nextcarhub.com/auto-repair-shop-software-comparison.html), the budget entrant omits the core money documents by its own admission, and willingness to pay is proven — shops demonstrably pay these prices when they can; the unserved are those who can't, not those who won't. Feasibility is 4, not 5: the RO core is straightforward local data work, but it is a wider surface than a single-purpose tracker.
- **Trucking (17), pet services (16), restaurants (16):** specified separately in this hunt's spec set.
- **Landlords (13):** free cloud incumbents with rent collection (the one feature that matters most) make a local paid tool a hard sell; spreadsheet pain alone isn't a gap when the fix is free.
- **Resellers (13) / child care (11) / churches (11) / ranching (11):** dropped — resellers are being filled at the bottom by spreadsheet products and a $14.99 web entrant; child care's incumbent (KidKare, from ~$9.95) is sponsor-integrated in a way local software structurally can't be; churches combine the lowest willingness to pay with existing desktop incumbents; ranching's incumbent (CattleMax) is entrenched, cheap, and the work is mobile-field work, not desk work.

## 9. Build notes (for whoever builds it — not part of this spec task)

- Follow the v40 pattern: local JSON store, no network calls, no shared Licensing/voice/integrity code — no new secrets needed. Start from the ClientPilot/BizPilot skeleton (entity files + line-item documents) rather than a tracker skeleton; the RO is a document with a status pipeline (DisputePilot pattern) over entity records (customer/vehicle).
- Money math in decimal throughout; tax rate and shop-supply fee are shop-level settings, user-entered, shown on every estimate — never hardcoded.
- Print/PDF output for estimates and invoices must look like a real shop document (shop name/address/phone header from settings, RO number, vehicle, lines, totals, approval signature line on estimates). Reuse InvoicePilot's document-generation code paths where they exist.
- Mileage and dates: store ISO dates; mileage is a plain integer field with in/out pair per RO; never infer or fetch vehicle data (no VIN decode — VIN is a stored string).
- Ship 100 languages the same way as the rest of the line (script-qualified resw tags); shop documents print in the shop's locale formats.
- First-run sample: one fictional customer + vehicle + a half-finished RO with one declined line, clearly labeled sample data (BizPilot sample pattern), so the board, history, and declined register all show themselves working.
- Hard rule for copy: never claim labor times, OEM data, or "industry-standard" rates. Sell the approval trail, the history, and the declined-work call-backs — the things paper actually loses.
