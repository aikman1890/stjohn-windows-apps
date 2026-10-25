# GAP APP SPEC — RangePilot

**Status:** Spec only (no build authorized yet)
**Date:** 2026-10-25
**Line:** The Night Shift SRE / St. John Windows apps (WinUI 3, local-first, Gumroad membership)
**Prepared for:** Shaun Nicholas St. John
**Hunt:** Gap hunt #5 — destination experiences (tank-driving / machine-gun experience attractions)

---

## 1. The app in one line

**RangePilot** — a local-first Windows desktop app that runs an experience attraction's operating day: every booking, every signed waiver, every vehicle and range lane, every safety officer, and every photo/video package owed to a guest — in one book, on the operator's own machine, for a flat membership instead of a percentage of every booking.

## 2. The gap

Experience attractions (drive-a-tank, machine-gun ranges, flamethrower/artillery experiences) sell some of the highest tickets in tourism — and run their back office on generic tools that know nothing about their operation.

**The money is large and the tooling is generic:**

- DriveTanks.com (Uvalde, TX, on Ox Ranch) sells tank packages "from $750 – $9,950," drive-only courses "$650 – $2,250," priced per activity for groups of up to 6 (https://casago.com/frio-river/things-to-do/drive-tanks/). Its booking runs on a generic **Acuity Scheduling** page — a hair-salon-class scheduler holding appointments like the $4,950 "Fan Favorite" package (https://app.acuityscheduling.com/schedule/558c9b45/?appointmentTypeIds%5B0%5D=12692595).
- The category's standard booking engines charge percentage rent on those tickets: FareHarbor ~6% per booking passed to the customer, Peek Pro ~6%, Xola ~6% (or 2.39% + $0.30), Rezdy $49–$249/mo + 3% (https://www.bokun.io/tour-and-activity-booking-system-pricing and https://www.captainbook.io/blog/fareharbor-pricing-fees-explained). Six percent of one $4,950 package is ~$297 — per booking, forever.
- Tank America (Orlando) lists packages $299–$1,591 with add-ons — passenger pass $50, **video package $75**, extra laps $150 (https://www.theorlandoreal.com/drive-a-military-tank-at-tank-america-orlando/?submit=&p=9). Drive A Tank (Kasota, MN) runs $470–$6,075 and books groups by phone and email: "E-mail or call Incredible Adventures today" (https://www.incredible-adventures.com/tank-adventures.html).

**What none of the generic tools tracks — the actual operation:**

- **Waivers are a separate silo.** Battlefield Vegas requires a completed waiver before anyone shoots (https://en.wikipedia.org/wiki/Battlefield_Vegas), and the waiver market is standalone tools (Smartwaiver from ~$19–22/mo with capped waiver counts). The category comparison is blunt: "Waivers sit in a separate system from your tickets" (https://www.wakesys.com/best-trampoline-park-waiver-software). Nobody ties *this party's* waivers to *this booking's* vehicles and lanes, so check-in is a clipboard reconciliation on the busiest morning of the week.
- **The asset is the constraint.** A specific tank or gun is the product — and it goes down for maintenance without notice; guests are advised to "call ahead" because "a specific tank or gun can be down for maintenance on any given day" (https://txheadlines.com/this-wild-texas-ranch-lets-you-drive-a-32-ton-wwii-tank-and-fire-its-cannon). No booking engine knows the Sherman is down, the Scorpion is up, and lane 2 needs a safety officer at 10:00.
- **Media is sold and then untracked.** The $75 video package is a standard add-on (Tank America, above), and posing for the camera is built into the Battlefield Vegas flow — yet no tool in the category tracks who bought media, whether it was shot, or whether it was delivered. Undelivered media on a $2,500 day is a refund-and-bad-review machine.
- **Discount leakage instead of loyalty.** DriveTanks sells through Groupon and LivingSocial at up to 34% off (https://www.livingsocial.com/deals/ox-ranch-1) and donates packages to charity auctions — the category acquires guests at a discount and has no repeat-visit, referral, or loyalty system anywhere to win them back at full price.

**The unserved layer is the operator's day book: bookings + waivers + assets + staff + media owed, offline, in one place.** That is the layer Shaun's line already knows how to build (KennelPilot's board-and-gate pattern, DenialPilot's clocks) pointed at an industry whose tickets are 10–50× a kennel stay.

## 3. Target user

- **Primary:** Independent experience attractions — tank/armor experiences, machine-gun ranges, artillery/flamethrower experiences, multi-activity adventure ranges — typically one location, a handful of vehicles/weapons, seasonal staff, currently on Acuity/FareHarbor + a waiver silo + paper.
- **Secondary:** Museum-adjacent and ranch-adjacent attractions running experiences as a second line (the Ox Ranch pattern: hunts + tank experiences on one property — RangePilot covers the experience side; HuntPilot covers the ranch side).
- **Tertiary:** High-ticket guided-experience operators generally (supercar experiences, helicopter tours) whose constraint is a specific asset + a certified operator per slot.

## 4. Core features (MVP, all local-first)

1. **Day board** — one screen for the operating day: bookings by time slot, party size, package, balance due, waiver status per participant (green only when every participant is signed), assets and staff assigned. The morning huddle reads one board, not four systems.
2. **Booking book** — packages (user-built: name, price, duration, capacity, assets required, rounds/extras included), bookings with deposit/balance ledger, source tracking (direct, phone, Groupon/LivingSocial, auction, gift card) so discount leakage is visible in dollars, gift-card issuance and redemption ledger.
3. **Waiver register** — per-participant waiver records tied to the booking: signed/unsigned state, minor flag with guardian signer, waiver version used. Records the *fact and version* of signing (paper or third-party e-waiver reference) — the gate flags any party not fully signed before their slot.
4. **Asset book** — every vehicle/weapon/range lane: status (up / down / maintenance), maintenance log and next-service notes, rounds/fuel usage per booking. A down asset blocks new bookings against it and flags existing bookings that need a swap or a call-ahead — the exact failure guests are warned about today.
5. **Staff & certification roster** — safety officers, drivers/instructors, guides: who's certified for what, who's scheduled per slot; a slot can't be marked ready without its required certified roles filled.
6. **Media package ledger** — who bought photo/video, what was captured, delivery status and delivery date, follow-up clock for undelivered media (orange ≤2 days, red overdue). Turns the $75 add-on from a liability into a tracked deliverable.
7. **Guest book & repeat engine** — guest history across visits, occasions (birthdays, bachelor parties, corporate), simple loyalty/referral notes and a "win-back" list: past guests sorted by days since visit, so the operator markets to its own list instead of buying the same guest back from Groupon at 34% off.
8. **Day-close report** — exportable summary: revenue by package and source, discounts given away, media outstanding, asset downtime, incidents/notes. The artifact an owner reviews weekly.

**Platform rules (same as the rest of the line):** local JSON storage, no network calls, no account, no telemetry, fully offline — ranges are rural and the office PC is the system of record. Guest data never leaves the machine.

## 5. What it deliberately does NOT do

- **No online booking or payments.** FareHarbor/Peek/Acuity already fight over the storefront and take their percentage; RangePilot is the back-office book that reconciles what they sold against what the operation can actually run. (Copy states this plainly: keep your booking channel; this runs your day.)
- **No e-signature capture.** It registers and gates waiver status; operators keep their existing paper or waiver-tool process. This keeps the app out of e-sign legal territory.
- **No firearms/vehicle compliance advice.** It tracks the operator's own maintenance and certification records; it does not interpret ATF, state, or insurance rules. Screens carry the line's standard "records tool, not compliance advice" disclaimer.
- **No POS / merchandise inventory** in MVP (gift shop is a different, served market).
- **Not VenuePilot** (event venues: date holds, contracts, day-of sheets) **and not KennelPilot** (pet boarding). RangePilot's core is asset-and-waiver-gated experiences sold by the slot.

## 6. Nearest incumbents — and how RangePilot differs

| Incumbent | What it is | Price | RangePilot's difference |
|---|---|---|---|
| **FareHarbor / Peek Pro** | Cloud booking engines for tours & activities | ~6% of every booking, passed to guest | They sell the ticket and stop. They don't know the Sherman is down, the party's waivers are incomplete, or the $75 video was never delivered. RangePilot is the flat-fee operations book behind whichever engine the operator keeps. |
| **Acuity Scheduling** (what DriveTanks actually uses) | Generic appointment scheduler | Flat SaaS subscription | A calendar, not an operation: no assets, no waiver gate, no media ledger, no discount-source accounting. RangePilot models the business Acuity can't. |
| **Smartwaiver** | Standalone digital waivers | From ~$19–22/mo, capped waivers | Waivers only, in a silo — by the category's own comparison, separate from the tickets. RangePilot ties waiver state to the booking, the party, and the slot so the gate is checked before the guest reaches the range. |

## 7. Suggested Gumroad price

**$29.99/month membership** — premium tier, same as this hunt's HuntPilot. Rationale: the customer's average ticket is $650–$4,950; one weekend of bookings on a 6% engine costs more than a year of RangePilot, and one prevented "drove out for a tank that was down" refund covers months. Priced well below the percentage platforms' effective take and below enterprise venue systems, at the top of Shaun's line where the ticket size justifies it.

## 8. Candidates considered (scored 1–5; 20 max)

| Candidate | Pain frequency | Incumbent weakness | Local-WinUI feasibility | Willingness to pay | Total |
|---|---|---|---|---|---|
| **A. RangePilot — experience-attraction ops book** | 5 | 5 | 5 | 5 | **20** |
| B. Guest-facing trip companion app (pre-visit prep, day timeline, media delivery) for attraction guests | 3 | 3 | 1 | 2 | 9 |
| C. Standalone waiver kiosk app for attractions | 3 | 2 | 4 | 3 | 12 |
| D. Merch/gift-shop inventory for attractions | 2 | 2 | 4 | 2 | 10 |

Scoring notes (evidence, not vibes):

- **B (9):** Guests would use it once; operators won't pay for a guest app, guests won't pay at all, and it's a mobile/web product — outside the desktop line's form factor. The valuable parts (prep checklists, media delivery status) fold into RangePilot's media ledger and booking book as operator-side records. Dropped as a standalone.
- **C (12):** Real category, already served (Smartwaiver, WaiverForever, WaiverFile from ~$19/mo — https://activitymessenger.com/blog/8-best-digital-waiver-software-tools-in-canada-e-signature/) and carries e-signature legal weight. The gap is not waivers themselves but waivers *connected to the operation* — which is RangePilot's register-and-gate design without the legal exposure. Runner-up.
- **D (10):** Gift-shop retail is served by every POS on the market; no category-specific gap. Dropped.

## 9. Build notes (for whoever builds it — not part of this spec task)

- Reuse KennelPilot's board + gate patterns (status board, expiry/flag gates) and DenialPilot's clock language (green / orange / red) for media-delivery and balance clocks.
- No shared Licensing/voice/integrity code, consistent with the v40 apps. Local JSON, ISO dates, locale-safe rendering, decimal money math.
- First-run sample data: a fictional one-day attraction ("sample data" labeled) — two vehicles (one down), three bookings (one party missing a waiver, one media package owed), so the day board shows every gate working before the operator enters anything real.
- Hard rule for copy: never promise bookings, revenue, or legal protection. Sell the organized day: waivers checked, assets known, media delivered, discounts visible.
