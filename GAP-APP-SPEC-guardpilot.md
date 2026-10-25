# GAP APP SPEC — GuardPilot

**Status:** Spec only (no build authorized yet)
**Date:** 2026-10-25
**Line:** The Night Shift SRE / St. John Windows apps (WinUI 3, local-first, Gumroad membership)
**Prepared for:** Shaun Nicholas St. John
**Industry:** Administrative & Support Services — private security guard companies (NAICS 561612)

---

## 1. The app in one line

**GuardPilot** — a local-first Windows desktop app that is the security company's office system of record: client sites and post orders, guard rosters and schedules, incident logs, and the hours-to-invoice ledger — for the small guard firm quoted enterprise prices by the platforms, or running posts on paper and WhatsApp.

## 2. The gap

Security-guard software pricing is built on opacity and per-site minimums, aimed at firms with dozens of locations — while the industry's long tail is small firms running a handful of posts.

**The pricing posture, as verified in the sweep:**

- **TrackTik (Trackforce):** vendor **does not publish pricing** — "contact sales." A third-party listing shows Guarding Suite **$78/mo per location licence** and GuardTek **$427/mo for 5 licences (~$85.30/location)** (https://elearningindustry.com/tracktik-pricing) — treat as ESTIMATE-grade, not vendor-verified.
- **Silvertrac (also Trackforce-owned):** conflicting third-party figures — $50/mo per active mobile device (https://sourceforge.net/software/compare/Silvertrac-Software-vs-Telephone-Timesheets/) vs. $249/mo for up to 10 devices (https://www.therms.io/best-security-guard-reporting-software/) — unverified; the consistent facts are per-device/per-site structure, setup fees, and annual contracts.
- **OfficerApps / OfficerReports:** from **$70/site/mo** per a Connecteam comparison (https://connecteam.com/top-10-apps-security-guards/). Cheaper guard-native outliers exist (GuardsPro at $5/user/mo, 3-user minimum, same source) — so the gap is **quote opacity + per-site minimums + contract lock-in**, not the absolute absence of cheap cloud.

**The sweep's sector finding is precise about where the local gap is:** for security firms the gap is the **back office** — *"posts/rosters, post orders, incident log, route/customer billing ledger"* for firms *"priced out of per-location/per-truck cloud"* — with the honest caveat that **GPS patrol proof inherently needs mobile/network**, so the local app is the **office system of record, not the field proof**. The documented small-firm pattern is *"cloud guard-tour platforms or paper/WhatsApp"* — meaning at the bottom, the roster is a spreadsheet, the post orders are a binder (or a text message), incident reports are Word files in a folder, and invoicing hours are reconstructed at month-end from the schedule someone remembers.

**The unserved layer is that office record**: which client, which post, what orders, which guard, which hours, what happened — kept in one place, printable for a client who asks "what exactly did your officer do Tuesday night?", and reconcilable to the invoice without an archaeology project. Client lists and incident narratives are also competitively and legally sensitive; a small firm's whole book sitting in a quote-only vendor's cloud is a posture many owners never chose on purpose.

## 3. Target user

- **Primary:** Owner-operators of small guard companies (roughly 3–30 guards, 2–15 client sites) — the owner is also the scheduler, the supervisor, and the biller.
- **Secondary:** Small firms that bought a per-site platform for one anchor client and run everything else on paper — GuardPilot is the record for the rest of the book.
- **Tertiary:** Startup guard companies in their first licensing year, before they can carry per-site minimums and annual contracts.

## 4. Core features (MVP, all local-first)

1. **Client & site register** — client, site address, contract reference, bill rate per post/hour (user-entered), billing terms, site contact. Every post hangs off a site; every hour hangs off a post.
2. **Post orders book** — per post: standing orders text, site instructions, emergency contacts, escalation chain, effective date + revision history (orders change; the version in force on a given night must be retrievable). Printable post-order sheet for the site binder — generated from the system instead of competing with it.
3. **Guard roster** — guards with licence/certification fields as user-entered data (licence number, expiry — with the line's expiry-warning colors), contact info, status (active/inactive), notes. Licence expiries surface on the dashboard before they become a compliance incident.
4. **Schedule board** — posts × shifts grid by week: assign guards to shifts, flag unfilled shifts (red), double-bookings (same guard, overlapping shifts — hard warning), and overtime accumulation per guard. The Friday "who's on the warehouse post?" answer in one screen.
5. **Incident log** — per incident: date/time, site/post, guard, type (user-defined list: trespass, alarm, damage, injury, police called…), narrative, follow-up actions + owner + due date with overdue flags. Searchable by site, guard, type, date — the artifact clients and insurers actually ask for.
6. **Hours & billing ledger** — scheduled vs. actual hours per shift (actuals entered from guard reports/timesheets at the office), per site per period totals × bill rate → invoice-ready summary per client (the invoice itself can be issued in InvoicePilot; GuardPilot produces the verified hours it stands on). Guard pay-hour totals per period exported for payroll prep.
7. **Shift/DAR intake log** — daily activity report entries logged at the office from whatever the guard submitted (paper, text, call-in): date, post, guard, summary. This is the honest local version of field reporting — entered once, searchable forever, instead of lost in a chat thread.
8. **Dashboard** — unfilled shifts this week, licence expiries ≤30 days, open incident follow-ups, hours billed vs. scheduled variance by site, top clients by hours. The owner's Monday screen.

**Platform rules (same as the rest of the line):** local JSON storage, no network calls, no account, no telemetry, works fully offline.

## 5. What it deliberately does NOT do

- **No GPS patrol verification, no checkpoint scanning, no live guard tracking.** That requires phones + network and is the platforms' real product — GuardPilot does not fake it. The listing states plainly: this is the office system of record; if you need proof-of-presence in the field, that's a different tool. Honesty here is the positioning.
- **No guard-facing mobile app, no messaging.** Guards report as they do today; the office logs it once.
- **No payroll processing and no invoicing engine** — pay-hour totals and invoice-ready summaries export to the firm's existing payroll/invoice flow (InvoicePilot covers invoicing in-line).
- **No licensing/compliance authority.** Licence fields are the firm's own records with expiry warnings; GuardPilot does not verify licences with any state or claim compliance certification.
- **No background checks, no applicant tracking** — staffing ATS is Bullhorn's world ($99/user/mo) and not this product.

## 6. Nearest incumbents — and how GuardPilot differs

| Incumbent | What it is | Price | GuardPilot's difference |
|---|---|---|---|
| **TrackTik (Trackforce)** | Guard-tour + operations platform (GPS checkpoints, scheduling, reporting) | Quote-only; ~$78/mo per location ESTIMATE-grade | An enterprise field platform sold by contract. GuardPilot covers the office half — posts, orders, rosters, incidents, billing hours — at $14.99/mo flat with a published price and no contract, and says plainly that field proof is not its job. |
| **Silvertrac** | Guard reporting / tour software | Per-device flat plans, unverified ($50/device or $249/10 devices reported) | Field reporting first. GuardPilot is the record those reports land in: versioned post orders and a searchable incident ledger with follow-up ownership, which per-device reporting tools treat as an afterthought. |
| **Paper binder + WhatsApp (DIY status quo)** | The documented small-firm reality | Free + month-end archaeology | GuardPilot is that binder finished: schedules that flag their own gaps, incident narratives that can be found, hours that reconcile to invoices without reconstruction. |

## 7. Suggested Gumroad price

**$14.99/month membership** — flat, all sites and guards included (the anti-per-site price is the headline, as with ShopPilot's anti-per-user). Against ~$70–85/site/mo incumbent postures, a 6-site firm saves an estimated $4,000–5,000/yr — stated in copy as arithmetic on published/estimated figures, never as a guarantee.

## 8. Candidates considered (scored 1–5; 20 max)

| Candidate | Pain frequency | Incumbent weakness | Local-WinUI feasibility | Willingness to pay | Total |
|---|---|---|---|---|---|
| **A. GuardPilot — security-firm back office** | 5 | 4 | 5 | 4 | **18** |
| B. Small waste-hauler route/billing back office | 4 | 3 | 4 | 3 | 14 |
| C. Staffing-agency mini-ATS | 3 | 2 | 3 | 3 | 11 |

Scoring notes:

- **A (18):** Pain is daily (scheduling never stops), incumbent weakness is structural (opacity + per-site minimums + contracts — scored 4 because cheap guard-native outliers do exist), feasibility is total once the field-proof scope is honestly excluded, and guard firms demonstrably pay per-site money when they can carry it.
- **B (14):** Small haulers run paper route sheets under quote-only routing platforms (Routeware; ESTIMATE ~$29/truck/mo — https://www.itqlick.com/routeware/pricing). Real gap, but routing's value is optimization (network-shaped), leaving a thinner local slice than security's back office. Possible follower.
- **C (11):** Staffing ATS is Bullhorn's entrenched cloud ($99/user/mo Starter — https://www.bullhorn.com/small-agency-software/pricing/) and candidate pipelines are network products (job boards, messaging). No local wedge.

## 9. Build notes (for whoever builds it — not part of this spec task)

- Closest pattern relatives: TruckPilot/BayPilot (roster/board + per-job records), DenialPilot (dated log entries with follow-up ownership), KennelPilot (board view discipline).
- Post-order versioning mirrors WellPilot's deck versioning: the version in force on a past date must be retrievable verbatim. Build the revision store early.
- Schedule conflict detection (unfilled / double-booked / licence-expired-on-shift) is the demo moment — make the sample data contain one of each.
- First-run sample (fictional, labeled): "Bayou City Patrol Services" — 4 sites, 6 guards, one week scheduled with one unfilled shift, one licence expiring in 12 days, two incidents (one follow-up overdue), one client invoice summary ready.
- Hard rule for copy: lead with the system-of-record honesty ("the office record your whole book runs on"), publish the price in the first line, and explicitly disclaim field/GPS proof so no buyer feels misled. Never use the words "guard tour" — that's their category, not this one.
