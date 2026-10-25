# GAP APP SPEC — KennelPilot

**Status:** Spec only (no build authorized yet)
**Date:** 2026-10-25
**Line:** The Night Shift SRE / St. John Windows apps (WinUI 3, local-first, Gumroad membership)
**Prepared for:** Shaun Nicholas St. John
**Industry:** Pet services (small boarding kennels, dog daycare, grooming)

---

## 1. The app in one line

**KennelPilot** — a local-first Windows desktop app for a small boarding/daycare operation: who is in which run, whose vaccinations expire when, who gets what food and medication at what time, and who has paid — on the front-desk PC, without a $95–$250/mo resort platform and its card-processing lock-in.

## 2. The gap

Pet boarding platforms were built for high-volume resorts, priced accordingly, and discounted only if you surrender your card processing. The small kennel — 10 to 40 runs, often an owner plus two staff — runs on the tools the platforms' own marketing describes.

**The status quo is two spreadsheets and a paper binder:**

- A platform vendor's own pitch names it: most facilities run "a pile of spreadsheets, a paper binder for vaccine records, a separate invoicing tool, and a standalone retail POS" (https://www.packtrackercrm.com/).
- The failure mode is not theoretical: one kennel owner sought custom software because "her vaccination reminders were going out of a spreadsheet and her booking system was going out of a different spreadsheet, and the two spreadsheets disagreed about eleven dogs" (https://www.founderandai.com/blog/custom-pet-boarding-grooming-software-baltimore.html). Vaccination records are not bookkeeping — rabies/DHPP/bordetella currency is the legal and safety gate for accepting a dog at all, and in Maryland the proof standard is a specific form (NASPHV Form 51, per COMAR 10.06.02.10, same source).

**The incumbents price and shape small operators out:**

- **Gingr**, the boarding-specific leader, starts around **$95/mo** (SoftwareAdvice listing) with review roundups putting real plans at roughly **$129/mo and up, often quote-based**, and Stay pricing at **$179/mo with their integrated payments vs. $209 without** — i.e., a discount for handing them your card volume (https://www.softwareadvice.co.uk/software/223150/gingr/; https://www.happypet.tech/blog/guides/best-pet-boarding-software-in-2026-complete-comparison-guide; https://www.founderandai.com/blog/custom-pet-boarding-grooming-software-baltimore.html). The same roundups conclude Gingr is "less suited to smaller or growing facilities" — built for resorts with the budget and staff for a complex system.
- The rest of the field prices the same direction: PackTracker at **$208–$249/mo**, MoeGo at $79–$239/mo per location, DaySmart similar tiered pricing with the cheaper number conditional on their processing (sources above; https://www.happypet.tech/blog/guides/best-pet-grooming-software-in-2026-complete-comparison-guide).
- What the small kennel is paying for in those tiers is the resort stack: online booking portals, customer apps, photo report cards, multi-location reporting, retail POS, marketing hubs. What it actually needs every day is smaller and more serious: **the board (who's here, who's coming, who's leaving), the vaccine gate, and the care log** (feeding, medication, special instructions) — the records that decide whether a dog can be accepted and whether it was cared for as promised.

**The unserved layer is the kennel's operational record — board, vaccination compliance, care instructions, and stay billing — as a simple local tool** on the front-desk computer the kennel already has, at a price a 15-run kennel can say yes to without a demo call.

## 3. Target user

- **Primary:** Single-location boarding kennels and dog daycares, roughly 5–40 runs, owner-operated — currently on spreadsheets + paper vaccine binder, or churning off a resort platform.
- **Secondary:** Grooming salons that also board a few dogs, pet sitters with a small facility, and rescue/shelter-adjacent boarding operations that need vaccine and intake records more than marketing.
- **Tertiary:** Farm/dog-training facilities taking board-and-train clients (stay records + training notes per dog).

## 4. Core features (MVP, all local-first)

1. **Kennel board** — today at a glance: arrivals, departures, current guests by run/kennel assignment, capacity count. Status pipeline per stay (Booked → Checked in → In care → Checked out). The spreadsheet, replaced by the screen it was imitating.
2. **Pet & owner files** — owner contacts (incl. emergency contact and vet contact), pets with breed, weight, age, temperament/behavior notes, allergies, and **vaccination records with expiry dates per vaccine** (rabies, DHPP, bordetella + user-added types). Records attach as local file references (the scanned Form 51/certificate lives in the pet's file list).
3. **Vaccination gate & expiry engine** — the compliance core: booking or check-in a pet whose required vaccines are expired (or expire mid-stay) raises a hard visible flag in DisputePilot color language; a forward view lists every pet whose vaccinations expire in the next 30/60/90 days so the kennel can chase paperwork *before* the holiday rush, not at the door. Required-vaccine list is user-configured (rules differ by jurisdiction and facility — the app enforces the kennel's own list and says so).
4. **Stay records & care log** — per stay: check-in/out times, run assignment, **feeding schedule and medication instructions** (what, how much, when — with per-day checkboxes staff initial off), special instructions, incident notes. The daily care sheet prints for the kennel wall and files back against the stay. This is the record that answers "did you give him his medication?" with proof.
5. **Bookings calendar** — future stays by date with capacity warnings (over-capacity dates flagged as entered, never silently double-booked past the run count the user sets).
6. **Stay billing** — per stay: nights × rate (per-pet rates, multi-pet same-run handling as user-entered lines), daycare days, add-on services (grooming, extra play, medication administration — user-defined price list), deposits recorded, balance due at checkout, print/PDF invoice. Payment is recorded, not processed (see non-goals).
7. **Owner communications log** — calls/notes per owner (booking changes, incidents, "he ate fine") with date/time — the memory a two-person operation can't keep in two heads.
8. **Dashboard** — today's arrivals/departures, occupancy tonight and this week, vaccine expiries needing action, balances outstanding.

**Platform rules (same as the rest of the line):** local JSON storage, no network calls, no account, no telemetry, works fully offline. Customer and pet records never leave the kennel's PC.

## 5. What it deliberately does NOT do

- **No online booking portal, customer app, or photo report cards.** Those require a hosted service; they are the resort stack's draw and its price. Bookings are entered by staff from phone/email, as small kennels actually take them.
- **No payment processing.** Invoices record deposits and payments; no card data, no processing discount games. The kennel keeps its own processor.
- **No SMS/email sending or reminder automation.** The vaccine-expiry *list* is the product; sending is the kennel's existing channels. (Print/export of the list is in.)
- **No retail POS / inventory.** A separate concern the platforms bundle to justify tiers.
- **No veterinary records or medical advice.** Vaccination records are documents and dates the owner provides; the app tracks expiry, it does not judge medical sufficiency. Medication entries are the owner's instructions, recorded and checked off — the app never alters dosages or schedules.
- **No multi-location.** One facility, one database, honestly labeled; resorts are the incumbents' market, not this one's.
- **No staff scheduling/payroll.** Care-log initials are an accountability record, not a time clock.

## 6. Nearest incumbents — and how KennelPilot differs

| Incumbent | What it is | Price | KennelPilot's difference |
|---|---|---|---|
| **Gingr** (gingr.com) | Cloud resort platform: online reservations, run management, vaccination tracking, POS, multi-location reporting | From ~$95/mo; commonly ~$129+/mo; Stay $179/mo with their payments, $209 without | The full resort stack, priced and complex accordingly — its own reviewers call it less suited to smaller facilities. KennelPilot is the board + vaccine gate + care log + stay bill, local on the desk PC, at $19.99, and the kennel keeps its own card processor. |
| **PackTracker** (packtrackercrm.com) | Cloud all-in-one: reservations, vaccine records, payments, retail, marketing hub | $249/mo ($208 annual) | Sells consolidation — replacing spreadsheets *and* the binder *and* POS *and* marketing at resort money, with processing fees up to 4.5% + 35¢ on top. KennelPilot replaces the spreadsheets and the binder — the parts a small kennel actually bleeds on — and touches nothing else. |
| **The two spreadsheets + paper vaccine binder (DIY status quo)** | Booking in one sheet, vaccine reminders in another, certificates in a binder — the setup that "disagreed about eleven dogs" | Free + risk | KennelPilot is one record per pet: the vaccine dates that gate check-in are the same record the booking reads, expiries surface 30/60/90 days out instead of at the door, and the care log is the same system as the board — no second spreadsheet to disagree with. |

## 7. Suggested Gumroad price

**$19.99/month membership** — a fifth of Gingr's realistic entry and a twelfth of PackTracker's sticker. Rationale: the buyer's alternative is $95+/mo or paper; one turned-away holiday booking (vaccine caught too late to renew) or one medication dispute answered with a signed-off care log pays for the year — price it where a 15-run kennel doesn't need a demo to decide.

## 8. Candidates considered in this hunt (scored 1–5; 20 max)

| Candidate | Pain frequency | Incumbent weakness | Local-WinUI feasibility | Willingness to pay | Total |
|---|---|---|---|---|---|
| **KennelPilot — small pet boarding/daycare records** | 4 | 4 | 4 | 4 | **16** |
| BayPilot — small auto repair shop work orders | 4 | 5 | 4 | 5 | 18 |
| TruckPilot — owner-operator IFTA & records (trucking) | 5 | 3 | 5 | 4 | 17 |
| PlatePilot — independent restaurant recipe costing | 4 | 4 | 5 | 3 | 16 |
| Small-landlord rent/lease ledger | 4 | 2 | 5 | 2 | 13 |
| Reseller inventory & profit ledger | 4 | 2 | 5 | 2 | 13 |
| Family child care CACFP records | 4 | 2 | 3 | 2 | 11 |
| Small-church giving records | 3 | 2 | 5 | 1 | 11 |
| Cattle herd records | 3 | 2 | 3 | 3 | 11 |

Scoring notes (evidence, not vibes):

- **Pet services (16):** Pain is daily and the vaccine gate is a compliance cliff, not a convenience — but the top of the market is genuinely served (Gingr's vaccination tracking exists; the weakness is price/shape/fit for small facilities, hence incumbent weakness 4 rather than 5). Feasibility 4: board + records + billing is well inside the v40 pattern, discounted because stay/care workflows are a slightly wider surface than a pure tracker.
- **Auto repair (18) and trucking (17):** specified separately in this hunt's spec set; both outscore pet services on willingness-to-pay proof and (trucking) pain frequency.
- **Restaurants (16):** ties pet services on score with a different profile — cheaper incumbents exist at the very bottom (meez from $19/mo), offset by pure-math feasibility (5).
- **Dropped:** landlords (free cloud rent-collection incumbents), child care (KidKare is purpose-built and sponsor-integrated from ~$9.95 — the workflow is networked by regulation), resellers (spreadsheet economy + new $14.99 web entrant filling the bottom), churches (free tiers + existing desktop products + lowest willingness to pay), ranching (CattleMax entrenched at ~$12–30/mo and the work is mobile, not desktop).

## 9. Build notes (for whoever builds it — not part of this spec task)

- Follow the v40 pattern: local JSON store, no network calls, no shared Licensing/voice/integrity code — no new secrets needed. Compose from existing skeletons: entity files (ClientPilot pattern) + per-stay document with status pipeline (DisputePilot pattern) + deadline/expiry engine (DisputePilot countdown logic, inverted to "days until vaccine expiry").
- The required-vaccines list, capacities, rates, and add-on services are all user-configured data with sensible starter defaults (rabies/DHPP/bordetella) clearly labeled as examples to confirm against local rules — never present defaults as legal requirements.
- Care log check-off must be fast for wet-hands use: big targets, one click per scheduled item, staff initials from a short local staff list. Print the daily care sheet from the same data (print/PDF reuse of InvoicePilot document code).
- Dates: store ISO; vaccine validity is date-only arithmetic (no time zones in expiry math). Money in decimal.
- Ship 100 languages the same way as the rest of the line (script-qualified resw tags).
- First-run sample: one fictional owner with two dogs (one with an expiring bordetella date so the gate visibly fires) and one in-progress stay, clearly labeled sample data.
- Hard rule for copy: never promise regulatory compliance ("state-approved records"). Sell one record that can't disagree with itself, expiries caught in time, and a care log that proves what was done.
