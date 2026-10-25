# GAP APP SPEC — RallyPilot

**Status:** Spec only (no build authorized yet)
**Date:** 2026-10-25
**Line:** The Night Shift SRE / St. John Windows apps (WinUI 3, local-first, Gumroad membership)
**Prepared for:** Shaun Nicholas St. John
**Hunt:** Gap hunt #5 — destination experiences (group & corporate events at adventure destinations)

---

## 1. The app in one line

**RallyPilot** — a local-first Windows desktop app that builds and runs group and corporate events for adventure outfitters: quote the package, track the headcount and the waivers, schedule the day across activities and staff, and close the balance — the event book for operators whose group business today lives in email threads and phone calls.

## 2. The gap

Groups are the highest-value bookings at every destination in this category — and the only segment with no tooling at all. Individual guests book online; a 40-person corporate day is still assembled by hand.

**The evidence:**

- Drive A Tank (Kasota, MN) offers "Group, corporate and special events… scheduled all year long" — booked the same way as everything else there: "E-mail or call Incredible Adventures today for a full color brochure, schedules, details and rates" (https://www.incredible-adventures.com/tank-adventures.html). The group pipeline starts with a brochure request.
- DriveTanks sells group-shaped product (packages priced per activity for up to 6 participants, full-day and multi-vehicle outings "can exceed $3,500," extended adventures built by consultation — https://casago.com/frio-river/things-to-do/drive-tanks/ and https://texashappens.com/drive-a-tank-texas/) and donates group packages to charity auctions — group demand is proven, group *process* is a phone call.
- At the ranches, the group product is structural: Ox Ranch prices non-hunter guests at $500–$750/night alongside every hunt (https://www.oxhuntingranch.com/texas/blackbuck-hunting/) — every booking is quietly a small group event with lodging, meals, and add-on activities, coordinated per-party by staff.
- The booking platforms don't fill this hole: FareHarbor/Peek/Xola sell *slots to individuals* (at ~6% per booking — https://www.bokun.io/tour-and-activity-booking-system-pricing). A corporate event isn't a slot: it's a built package — activities sequenced across a day, per-head pricing with tiers and add-ons (extra laps $150, passenger passes $50, video $75 at Tank America — https://www.theorlandoreal.com/drive-a-military-tank-at-tank-america-orlando/?submit=&p=9), deposits and balances on an organizer's timeline, a roster of participants whose waivers trickle in for weeks, catering and transport, and a run-of-day that staff execute.

**What breaks today, at every destination checked:** the quote is rebuilt from scratch per inquiry (no package builder); the participant roster and waiver chase lives in the organizer's inbox; deposits/balances are tracked per email thread; and the day itself is run from a printed sheet that can't know a vehicle went down this morning. Corporate planners — the buyers — judge vendors on exactly this professionalism, and they rebook the ones who make them look organized.

**The unserved layer is the event book: package builder → quote → roster/waiver chase → run-of-day → balance close**, for the outfitter's highest-margin segment. It reuses the line's proven shapes (InvoicePilot's quote math, KennelPilot's boards, DenialPilot's clocks) on a segment none of the booking platforms models.

## 3. Target user

- **Primary:** Adventure outfitters and experience attractions that sell corporate/group days — tank and range attractions, exotic ranches, multi-activity adventure parks, shooting/hunting lodges hosting retreats.
- **Secondary:** Ranch and lodge operators hosting weddings-adjacent and reunion groups *as activity clients* (not venue rental — that's VenuePilot's book; RallyPilot runs the activity program inside the stay).
- **Tertiary:** Independent event producers who white-label outfitter activities for corporate clients and need one book per event across multiple venues.

## 4. Core features (MVP, all local-first)

1. **Package builder** — reusable event packages from the operator's own components: activities (with per-head or per-group pricing), add-ons (extra laps, passenger passes, media packages, catering, transport), capacity and staffing requirements, minimums. A quote is assembled, not re-typed.
2. **Quote book** — per-inquiry quotes with tiers (e.g., 10/25/50 heads), deposit terms, quote status pipeline (Draft → Sent → Won/Lost → Booked), follow-up clock on unanswered quotes (orange ≤3 days, red overdue) — the corporate pipeline no one in the category tracks.
3. **Event file** — the won event as one record: organizer contact, contract/quote reference, headcount (expected vs confirmed), deposit and balance ledger with due-date clocks, event-day timeline, special requirements (dietary, transport, AV, awards).
4. **Roster & waiver chase** — participant roster per event with per-person waiver status; the chase list shows exactly who hasn't signed, exportable as a reminder list for the organizer. The two-weeks-out scramble becomes a screen.
5. **Run-of-day planner** — the event day as a schedule board: groups rotating across activities/stations, staff assigned per block (cross-checked against RangePilot-style asset status when the operator maintains it — manual status field in MVP), buffer and meal blocks; printable day sheet for staff.
6. **Add-on & upsell ledger** — day-of add-ons sold per event (media, extra activities, merch) recorded against the event, so the final invoice matches what actually happened on the ground.
7. **Balance close & event report** — one-click event summary: quoted vs final, deposits, add-ons, balance collected, per-head realized revenue; plus a post-event follow-up entry (thank-you sent, rebook prompt date) feeding next year's corporate pipeline.
8. **Organizer book** — corporate clients and organizers across events: who books annually, typical size, lifetime value. The rebooking list, owned by the outfitter — not rented from a platform.

**Platform rules (same as the rest of the line):** local JSON storage, no network calls, no account, no telemetry, fully offline. Client lists and pricing live on the operator's machine.

## 5. What it deliberately does NOT do

- **No consumer online booking or payments** (the platforms' turf, at their percentage). RallyPilot quotes and tracks; the operator invoices and collects as they do today — with the ledger to prove what's owed.
- **No CRM/marketing automation suite.** The organizer book is a record and a rebook list, not an email platform. Keeps scope shippable and avoids competing with HubSpot-class tools.
- **No venue-rental management** — date holds, venue contracts, and day-of venue sheets are **VenuePilot's** book. RallyPilot runs the *activity program* for groups, whether at the operator's own site or inside someone else's venue.
- **No attendee-facing app or registration portal.** Rosters arrive from the organizer (import/paste); the chase list is the tool. A portal would be a web product — noted as a future extension, not MVP.
- **No legal/contract generation.** It records quote and contract references and terms the operator sets; it doesn't draft contracts.

## 6. Nearest incumbents — and how RallyPilot differs

| Incumbent | What it is | Price | RallyPilot's difference |
|---|---|---|---|
| **FareHarbor / Peek Pro** | Slot booking for individuals | ~6% per booking | A corporate event is not a slot. They can't build a tiered package quote, chase 40 waivers against one roster, or run a rotation schedule. RallyPilot is the group layer they don't model — and takes no percentage of a five-figure event. |
| **Email + spreadsheet + printed day sheet (DIY status quo)** | How group events are actually run in this category | Free + chaos | This is the real incumbent (Drive A Tank's "email or call for a brochure" pipeline). RallyPilot is that workflow finished: quote pipeline with follow-up clocks, roster/waiver chase, run-of-day board, balance close. |
| **VenuePilot (same line)** | Event-*venue* book: holds, contracts, deposits, day-of sheet | $19.99–29.99/mo tier (its spec) | VenuePilot serves the venue renting space. RallyPilot serves the outfitter selling a built activity program to a group — different buyer, different book (packages, rosters, rotations, per-head math). Complementary, not overlapping. |

## 7. Suggested Gumroad price

**$19.99/month membership** — mid-premium. Rationale: one corporate event is a $3,500–$25,000 day; a single saved quote follow-up or one rebooked annual client pays for years of membership. Priced under RangePilot/HuntPilot ($29.99) because group business is one segment of an operator's revenue rather than the whole operation — and an operator running both pays a combined $49.98/mo for a complete back office, still less than one booking's platform percentage.

## 8. Candidates considered (scored 1–5; 20 max)

| Candidate | Pain frequency | Incumbent weakness | Local-WinUI feasibility | Willingness to pay | Total |
|---|---|---|---|---|---|
| **A. RallyPilot — group & corporate event book for outfitters** | 4 | 5 | 5 | 5 | **19** |
| B. Corporate-buyer-side planner (for the HR/office manager booking the outing) | 3 | 3 | 3 | 1 | 10 |
| C. Charity-auction package manager (donated experience packages, redemption tracking) | 2 | 4 | 5 | 2 | 13 |
| D. Season-pass / membership book for attractions | 2 | 3 | 4 | 3 | 12 |

Scoring notes (evidence, not vibes):

- **B (10):** The buyer-side user books one outing a year and will never buy software for it. The pain is real but the wallet is on the operator side. Dropped.
- **C (13):** A genuinely visible niche — DriveTanks packages appear in charity auctions with 1-year redemption windows (https://e.givesmart.com/events/im8/i/_All/bgfX/) and donated-package redemption is untracked everywhere — but it's a feature, not a product: it folds into RangePilot's source tracking and gift-card/redemption ledger. Runner-up as a feature, dropped as a standalone.
- **D (12):** Memberships exist at big ranges but are rare across this category (most are once-in-a-lifetime purchases); the repeat engine that matters here is guests and organizers, already in RangePilot/RallyPilot. Dropped.

## 9. Build notes (for whoever builds it — not part of this spec task)

- Reuse InvoicePilot's quote/line-item math (decimal money, tiers) and DenialPilot's follow-up clocks for quotes and balances. Run-of-day board reuses the line's board patterns (KennelPilot).
- Sample data on first run: one fictional corporate event mid-flight — quote sent, roster at 31/40 waivers, balance clock orange — so every chase list and clock is visible before real data.
- No shared Licensing/voice/integrity code, consistent with the v40 apps. Local JSON, ISO dates, locale-safe rendering.
- Hard rule for copy: never promise corporate clients or revenue. Sell the organized event: quotes answered, waivers chased, the day run off one sheet, the balance closed.
