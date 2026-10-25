# GAP APP SPEC — VenuePilot

**Status:** Spec only (no build authorized yet)
**Date:** 2026-10-25
**Line:** The Night Shift SRE / St. John Windows apps (WinUI 3, local-first, Gumroad membership)
**Prepared for:** Shaun Nicholas St. John
**Industry:** Arts, Entertainment & Recreation — small event venues (NAICS 711/713)

---

## 1. The app in one line

**VenuePilot** — a local-first Windows desktop app that runs a small event venue's book: date holds, contracts, deposits, vendor lists, and the day-of sheet — for the wedding barn, farm, and rental hall whose real choices today are a $375/mo hospitality platform or a paper calendar.

## 2. The gap

Small venues fall between two stools: generalist client-flow tools that don't know what a venue is, and venue-native platforms priced for hotels and multi-site operators.

**The price evidence, verified from fetched pages:**

- **Tripleseat** (venue/event-native): **$375/venue/mo**, verified on its own packages page (https://tripleseat.com/packages/). Perfect Venue: $199/mo/location (annual); Event Temple: from $249/mo (2026 pricing roundup — https://www.perfectvenue.com/post/event-management-software-pricing).
- **HoneyBook / Dubsado** (generalist): HoneyBook Starter **$29/mo** annual, Essentials $49, Premium $109 (https://www.tasksession.com/blog/honeybook-vs-dubsado/); Dubsado Starter $335/yr, Premier $525/yr. Affordable — but they are planner/photographer client-flow tools: **no native venue calendar with holds, no date-inventory concept at all.**

**The status quo at the small end is documented paper.** Planning Pod's own case study describes a Florida rustic wedding barn, Hidden Barn Venue, starting on *"Google Docs, Excel spreadsheets, e-calendars and email plus… whiteboards, paper printouts and folders"* and a *"hodgepodge of emails and spreadsheets"* (https://planningpod.com/blog/best-wedding-venue-management-software-hidden-barn-case-study). A 12-tool comparison notes most venues run **12–50 events/yr** and stay on the cheapest generalist tier ($300–$1,300/yr vs. $3,000–$10,000+ hospitality tiers — https://www.fullybookedvenue.com/best-wedding-venue-management-software/). The sweep's sector finding: the rural long tail is *"still on paper calendars, whiteboards, Excel/Google Sheets and phone bookings,"* and a local booking-and-ledger book fits because a venue serving repeat/local clientele **does not need a consumer marketplace** — its dates are not a network product.

**The unserved layer is the venue's own date book**: which Saturdays are held, whose deposit is in, what the balance schedule looks like, who the caterer is, what time load-in starts — single-site, office-PC work, at a price a 20-event barn can carry. Double-booked Saturdays and lost deposits are the failure modes; both are record-keeping failures, and both are preventable offline.

## 3. Target user

- **Primary:** Single-site venues doing 12–50 events/yr — wedding barns and farms, rental halls, historic homes, VFW/lodge halls, small wineries' event spaces — booked by phone and word of mouth.
- **Secondary:** Multi-use rural properties (venue + cabins + hunting weekends) juggling several bookable spaces on one calendar.
- **Tertiary:** Venue managers/coordinators hired part-time by the owners above, who inherit the paper system and need it to survive them.

## 4. Core features (MVP, all local-first)

1. **Date book with holds** — calendar over bookable spaces (Barn / Hall / Grounds — user-defined): inquiry → soft hold (with hold-expiry date and overdue flag) → contracted → completed. **Hard double-booking prevention**: a contracted date blocks the space; overlapping soft holds warn in the line's color language. This is the core screen and the reason the product exists.
2. **Event file** — per event: client names/contact, event type, guest count, package selected (user-defined packages with prices), contract status and date, notes. Everything about one Saturday in one place.
3. **Money ledger** — deposit amount + received date, payment schedule (user-defined milestones), balance due with due-date flags, final settlement, refund/cancellation record with reason. Decimal math; a "who owes us before Saturday" view sorted by event date.
4. **Vendor & day-of sheet** — per event: caterer, florist, DJ/band, photographer, rentals (names + phone + arrival times as entered), load-in/load-out times, ceremony/reception timeline notes → printable **day-of sheet** for the staff working the event.
5. **Inquiry pipeline** — inquiries logged with source (referral, drive-by, social, prior client), tour dates, follow-up flags (inquiry untouched in 7 days = flagged), conversion to hold. Small venues live on follow-up speed; the pipeline makes the follow-up visible.
6. **Season & pricing book** — user-maintained rate card by package/season (peak/off-peak dates flagged on the calendar view), so quotes are consistent when three family members all answer the phone.
7. **Reports** — revenue by month/season, events by type, inquiry conversion, outstanding balances total. The "was this year better than last year" answer.
8. **Multi-space support** — up to a handful of spaces per property with one combined calendar; no per-space fees, ever (see §7).

**Platform rules (same as the rest of the line):** local JSON storage, no network calls, no account, no telemetry, works fully offline — including on the venue's office PC with rural internet, which is a selling point, not a limitation.

## 5. What it deliberately does NOT do

- **No online booking, no marketplace, no listings.** VenuePilot will never send the venue a bride; it runs the book for the clients the venue already gets. (Discovery is Tripleseat/The Knot's game and the reason for their prices.)
- **No payment processing.** It records deposits and balances; money moves by check/card terminal as it does today.
- **No e-signature / contract generation in MVP** — it tracks contract status and dates; the venue's own contract document stays the venue's. (Fill-in contract templates are a post-MVP decision.)
- **No catering/kitchen, staffing-scheduling, or rental-inventory modules** — vendor names and times are recorded; their businesses are theirs.
- **No multi-property chains.** One property, a few spaces. Chains have Tripleseat money and Tripleseat needs.

## 6. Nearest incumbents — and how VenuePilot differs

| Incumbent | What it is | Price | VenuePilot's difference |
|---|---|---|---|
| **Tripleseat** | Venue/event management platform for hospitality groups | $375/venue/mo (verified) | Built and priced for hotels, restaurants groups, and multi-site operators with lead-generation plumbing. VenuePilot is the date book + ledger core for one site at $14.99/mo — 1/25th the price — because a barn's clients arrive by referral, not platform. |
| **HoneyBook** | Client-flow (inquiry → contract → payment) for creative solos | From $29/mo | Excellent at planner workflows; has no concept of a venue's date inventory — no holds, no space calendar, no double-booking protection. VenuePilot is built around the date, not the client flow. |
| **Paper calendar + spreadsheets (DIY status quo)** | The documented small-venue reality | Free + risk | VenuePilot is that system finished: holds that expire loudly, deposits that can't get lost, one calendar that three family members can't fork into three versions. |

## 7. Suggested Gumroad price

**$14.99/month membership** — flat per property, all spaces included. Anchored against $199–375/mo venue-native floors and the $29/mo generalist that lacks the one feature (the date book) the venue actually needs. One saved double-booking pays for a decade.

## 8. Candidates considered (scored 1–5; 20 max)

| Candidate | Pain frequency | Incumbent weakness | Local-WinUI feasibility | Willingness to pay | Total |
|---|---|---|---|---|---|
| **A. VenuePilot — small-venue date book + ledger** | 4 | 5 | 5 | 4 | **18** |
| B. Marina slip/lease ledger | 3 | 3 | 4 | 3 | 13 |
| C. Campground/RV site book | 3 | 2 | 4 | 3 | 12 |

Scoring notes:

- **A (18):** Incumbent weakness is a verified $375/mo native floor against a paper status quo; feasibility is maximal (calendar + ledger); pain is episodic but catastrophic when it lands (a double-booked wedding is a reputation event, not an inconvenience).
- **B (13):** Marinas checked and **rejected on the sweep's evidence**: Dockwa's value is its boater marketplace/reservation network (paid tiers from $200/mo; Molo quote-only; SpeedyDock from $200/mo dry-stack) — a no-network app cannot replicate the demand side, and the back-office-only slice is too thin. Documented here so it is not re-pitched.
- **C (12):** Campgrounds are already served at the payment layer by Campspot's $2/online-booking no-monthly-fee model — the network takes its cut per booking instead of monthly, which a local app can't undercut on price for a seasonal business.

## 9. Build notes (for whoever builds it — not part of this spec task)

- The calendar is the product: month view with space lanes, hold states as distinct visual treatments, and conflict warnings that cannot be clicked past silently. Build and test the double-booking logic before any other screen.
- Closest pattern relatives: KennelPilot (board/calendar + per-stay billing), CasePilot (per-matter file with milestone dates), InvoicePilot (ledger discipline).
- First-run sample (fictional, labeled): "St. John Family Barn" — one season: 8 events (2 inquiry, 1 soft hold expiring, 4 contracted, 1 completed), one balance overdue, one inquiry stale — every flag visible on first launch.
- Hard rule for copy: sell the Saturday. "Never double-book a Saturday again" is the headline class; never promise leads, bookings, or exposure.
