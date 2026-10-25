# GAP APP SPEC — MovePilot

**Status:** Spec only (no build authorized yet)
**Date:** 2026-10-25
**Line:** The Night Shift SRE / St. John Windows apps (WinUI 3, local-first, Gumroad membership)
**Prepared for:** Shaun Nicholas St. John
**Industry:** Moving & storage (small local movers, 1–3 trucks)

---

## 1. The app in one line

**MovePilot** — a local-first Windows desktop app that runs a small moving company's back office: surveyed estimates with a real inventory, booked jobs with an approval record, and a claims ledger with clocks — on the mover's own PC, instead of a $299–$399/month platform built for van-line scale.

## 2. The gap

Moving software is priced for companies with a sales department. The one-to-three-truck local mover — the bulk of the industry by count — runs on paper surveys, carbon estimates, and a notebook, because the platforms start where his software budget ends.

**The pricing wall:**

- SmartMoving, the category's growth platform, starts at **$299/month (Essential) to $399/month (Growth)** (https://softwarefinder.com/fleet-management-software/smartmoving; https://www.getapp.co.uk/software/2048331/smartmoving-software).
- MoveitPro, the "smaller" alternative, starts at **$150/month** (https://www.selecthub.com/moving-company-software/smartmoving-vs-moveitpro/), and both sell the connected layer: lead capture, dispatch, crew mobile apps, customer portals, marketing automation.
- A local mover's software-critical work is not that layer. It is three back-office artifacts — **the survey/estimate, the booked job record, and the claim file** — all of which are created at a desk, and none of which needs a network.

**What paper loses a small mover, specifically:**

- **Estimates without an inventory are guesses that become disputes.** The room-by-room survey (what's going, cubic feet, packing needs, access problems) is taken on a clipboard and thrown away after move day — so the "you never told me the piano was extra" argument has no paper on the mover's side.
- **The approval is a handshake.** Estimate sent, customer says "sounds good" on a call, crew rolls — and on delivery day the price conversation starts over with no record of what was approved, at what price, under what terms.
- **Claims live past their paperwork.** Damage claims arrive by phone and email; the file (claim date, item, amount, response dates, offers) is scattered — and claims are a deadline business: the mover's own claim paperwork and tariff set response clocks that a notebook doesn't count down. Missed, muddled claim files are how small movers end up in regulator complaints and chargebacks.
- **Storage lots are a shoebox list.** Goods held in the mover's warehouse accrue monthly storage with no clean lot list, in/out record, or balance view.

**The unserved layer is the mover's desk core — survey → estimate → approval → job sheet → claim file — as a simple local tool.** Dispatch and crew apps are the platforms' game; the estimate and the claim are where a small mover's money and reputation actually move, and nobody sells that layer unbundled.

## 3. Target user

- **Primary:** Owner-operated local moving companies, 1–3 trucks, roughly 5–40 moves a month, currently on paper surveys, spreadsheet estimates, and notebook claims.
- **Secondary:** Moving-and-storage operators with a small warehouse whose storage lots are tracked on paper.
- **Tertiary:** Movers who quoted onto SmartMoving/MoveitPro and churned — they know the workflow; they need the desk core without the sales-platform bundle.

## 4. Core features (MVP, all local-first)

1. **Move file** — one file per move: customer, origin/destination, move date, status pipeline (Lead → Surveyed → Booked → Packed → Moved → Delivered → Closed / Claim open). Everything about the move hangs off this record.
2. **Room-by-room inventory & cube sheet** — rooms → items with user-editable cubic-feet/weight presets, packing flags (mover-packed / owner-packed), special items (piano, safe, pool table) and access notes (stairs, elevator, long carry, gate code as free text). Totals drive the estimate; the survey survives move day.
3. **Estimate builder** — the mover's own rate table (crew size, truck count, hourly rate, travel/service fees, materials prices) applied to the inventory; estimate types the mover actually uses (hourly, flat, not-to-exceed — user's own terms text); **estimate versions preserved**, and an **approval log** (approved/declined, date, method, who approved) exactly like BayPilot's — the record that ends delivery-day price arguments.
4. **Job sheet** — per booked move, a printed crew sheet: addresses, inventory summary, access notes, crew and truck assignment, deposit/balance terms. The crew lead's paper, generated from the file instead of re-copied.
5. **Valuation & claims ledger** — valuation choice recorded per move (from the estimate file); per claim: item, amount claimed, dates (filed, acknowledged, inspected, offered, settled/declined) with **user-confirmed response clocks** taken from the mover's own claim paperwork — green/orange/red countdown like the rest of the line — and a proof trail of every letter and offer. Claims stop living in an inbox.
6. **Storage lot book** — lots in the warehouse: customer, lot ID, in date, monthly storage rate, out date, balance; a monthly storage-billing list and aging view.
7. **Payments ledger** — deposit, collected-on-delivery, balance states per move; print/PDF final statement from the file.
8. **Dashboard** — moves this week, estimates awaiting approval (booked revenue sitting unsigned), claims open and past their clocks, storage balances outstanding.

**Platform rules (same as the rest of the line):** local JSON storage, no network calls, no account, no telemetry, works fully offline. Customer data never leaves the mover's PC.

## 5. What it deliberately does NOT do

- **No dispatch, GPS, or crew mobile app.** Move-day coordination is the platforms' product; MovePilot is the desk core before and after move day.
- **No lead capture, marketing automation, or customer portal.** The upsell machinery that makes the incumbents cost what they cost.
- **No payment processing.** Payments are recorded; cards never touch the app.
- **No tariff/van-line engines.** Rates are the mover's own table; the app does the arithmetic, it does not supply prices or regulated-tariff logic.
- **No claims legal advice.** Claim clocks are user-confirmed presets from the mover's own paperwork; the app organizes and counts down, it does not opine on liability or valuation law.
- **No accounting system.** Ledgers plus CSV/PDF export for the accountant.

## 6. Nearest incumbents — and how MovePilot differs

| Incumbent | What it is | Price | MovePilot's difference |
|---|---|---|---|
| **SmartMoving** | Full cloud platform: CRM, sales automation, dispatch, crew app, storage billing | $299–$399/mo | MovePilot is the desk core — survey, estimate, approval, claims — local and flat, at roughly two weeks of SmartMoving per year. No sales automation, no dispatch, no per-anything meter. |
| **MoveitPro** | Moving operations platform with scheduling, dispatch, customer portal | From $150/mo | Same story at a lower floor: still a connected operations platform. MovePilot's center of gravity is the estimate record and the claims ledger with clocks — the two artifacts a small mover actually gets sued and disputed over. |
| **The clipboard + notebook (DIY status quo)** | Paper survey thrown away, estimate in a drawer, claims in an inbox | Free + disputes | This is the real incumbent. MovePilot is that system finished: the survey becomes the estimate, the approval is logged, and every claim carries its dates and its countdown in one file. |

## 7. Suggested Gumroad price

**$14.99/month membership** — mid-band. Rationale: the comparison set starts at $150/mo, but the buyer is a very small business; $14.99 sits at ClaimPilot's tier, far under MoveitPro's floor, and one defended estimate dispute or one properly-worked claim covers a year of membership.

## 8. Candidates considered (scored 1–5; 20 max)

| Candidate | Pain frequency | Incumbent weakness | Local-WinUI feasibility | Willingness to pay | Total |
|---|---|---|---|---|---|
| **A. MovePilot — small mover estimates, inventory & claims** | 4 | 4 | 4 | 4 | **16** |
| B. LienPilot — self-storage delinquency & lien-sale manager | 4 | 5 | 5 | 4 | 18 |
| C. RemodelPilot — remodeler job costing & change orders | 5 | 4 | 5 | 4 | 18 |
| D. FarewellPilot — funeral home case & preneed book | 4 | 5 | 4 | 4 | 17 |
| E. AgreementPilot — trades maintenance-agreement & equipment book | 4 | 4 | 5 | 4 | 17 |
| F. RenewalPilot — solo insurance agent renewal book | 4 | 3 | 4 | 2 | 13 |
| G. ChemLog — pest-control application records vault | 4 | 3 | 2 | 3 | 12 |
| H. Septic book — pumping due-date & county records | 3 | 2 | 4 | 3 | 12 |
| I. LotPilot — towing impound & lien ledger | 3 | 2 | 3 | 3 | 11 |
| J. RouteBook — pool service route & chemistry log | 3 | 3 | 2 | 3 | 11 |
| K. MaidBook — cleaning service recurring book | 3 | 1 | 4 | 2 | 10 |
| L. PackLedger — trainer session-pack ledger | 3 | 3 | 2 | 2 | 10 |
| M. StudioBook — music teacher studio book | 2 | 1 | 4 | 2 | 9 |

Scoring notes (evidence, not vibes):

- **A (16):** The lowest-scoring winner — the pain is episodic per move rather than daily, and feasibility is 4 because move-day work is inherently mobile (explicitly out of scope). It wins the fifth slot because the pricing wall is the steepest in the hunt relative to business size ($299/mo entry against a two-truck operation) and the two artifacts it manages — estimates and claims — are the dispute surface of the whole business.
- **I (11):** Towing's back-office slice was the closest structural cousin (impound ledgers, state notices), but Towbook already reminds on state-required impound notices from $49/mo (https://www.getapp.com/industries-software/a/towbook-management-software/) and real-world Towbook cost lands at $150–300/mo all-in (https://www.raftlabs.com/blog/how-to-build-towing-roadside-assistance-dispatch-software) — the unserved remainder is too thin.
- **L (10):** Session-pack reconciliation is a genuinely documented pain ("spreadsheets fail at reconciliation because the payment and the remaining count live in two different places" — https://packsleft.com/resources/leaving-trainerize-session-packs), but the category is mobile-first, per-client pricing starts near $1/client, and solo-trainer willingness to pay is the thinnest surveyed.

## 9. Build notes (for whoever builds it — not part of this spec task)

- Reuse proven patterns: BayPilot's estimate builder + approval log, DisputePilot/DenialPilot's user-confirmed deadline presets for claim clocks, InvoicePilot's print/PDF statement pattern.
- Decimal money math throughout; cubic-feet/weight totals in decimals as well. Store ISO dates; render localized; ship 100 languages like the rest of the line.
- No shared Licensing/voice/integrity code, consistent with the v40 apps.
- First-run sample move (a fictional two-bedroom move with one approved estimate and one open claim, clearly labeled sample) so buyers see the survey→estimate→claim chain working before entering anything real.
- Hard rule for copy: never promise dispute or claim outcomes. Sell the survey that survives, the approval that's logged, and the claim file with its clocks — the things the evidence says small movers actually lose by lacking.
