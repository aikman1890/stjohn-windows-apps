# GAP APP SPEC — TaxPilot

**Status:** Spec only (no build authorized yet)
**Date:** 2026-10-25
**Line:** The Night Shift SRE / St. John Windows apps (WinUI 3, local-first, Gumroad membership)
**Prepared for:** Shaun Nicholas St. John
**Industry:** Professional Services — tax preparation offices (NAICS 541213)

---

## 1. The app in one line

**TaxPilot** — a local-first Windows desktop app that runs a small tax office's season: every client, every return, every missing document, every e-file status and deadline in one place, on the preparer's own machine — the tracking layer the $800-a-year cloud suites sell, without the cloud or the per-user rent.

## 2. The gap

Practice-management pricing for tax offices is per-user, cloud-only, and set for firms — while the actual tracking job (who owes me what document, which returns are stuck, what is due Friday) is local data that solo and two-person offices still run on email and spreadsheets.

**The incumbents, verified from fetched vendor/review pages:**

- **TaxDome:** Essentials **$800/user/yr** (~$67/user/mo, solo only), Pro **$1,000/user/yr**, Business **$1,200/user/yr** (https://jetpackworkflow.com/blog/jetpack-workflow-vs-taxdome/ and https://jetpackworkflow.com/blog/taxdome-vs-financial-cents/).
- **Canopy:** Standard **$74**, Plus **$109**, Premium **$149 per user/mo** (annual), plus metered add-ons (https://www.getcanopy.com/blog/best-tax-practice-management-software/ and https://getuku.com/articles/canopy-vs-karbon/).
- **Karbon:** Team **$59/user/mo** annual, Business **$89/user/mo** (https://getuku.com/articles/canopy-vs-karbon/ and https://getuku.com/articles/karbon-pricing/).

**The shape is the problem, twice over.** First, price: a two-preparer office pays $1,600–$3,600/yr before add-ons for what is, at its core, a client list with statuses and dates. Second, posture: these suites are built around client portals, e-signature, and document exchange — the client PII and tax documents live in the vendor's cloud by design. The sweep's NAICS 54 finding is explicit: solos still run deadlines and client status on **email + spreadsheets**, and the viable wedge is a **local tracker at a flat price that keeps client lists off the cloud — sold as a tracker, not a suite** (it cannot replace portal/e-sign/e-file, and must not pretend to).

**The unserved layer is the office's own control board**: engagements, return pipeline, document chase lists, deadline engine across entity types, and e-file status tracking — the part of practice management that never needed to leave the building. For an office whose clients walk in with shoeboxes, that layer *is* the product.

## 3. Target user

- **Primary:** Solo enrolled agents, CPAs, and storefront/seasonal preparers (1–3 people) running the season from email, a whiteboard, and a spreadsheet.
- **Secondary:** Small bookkeeping-plus-tax offices that do returns seasonally and refuse a second per-user stack on top of their bookkeeping tools.
- **Tertiary:** New preparers in their first three seasons, building the tracking discipline before they can justify TaxDome money.

## 4. Core features (MVP, all local-first)

1. **Client & engagement register** — client record (individual / business, entity type: 1040, 1120, 1120-S, 1065, 1120-H, 990, 1041 — user-extensible), contact fields, engagement status per tax year (Not started / Docs pending / In prep / In review / Ready to file / Filed / Accepted / Rejected-fix / Extended / Done), fee quoted/paid. One row per client-year; seasons don't overwrite each other.
2. **Document chase list** — per client-year checklist (W-2, 1099s, K-1s, prior-year return, ID, bank letter, business books — templates per entity type, user-editable), received/missing state, and a **chase view**: every client sorted by what's missing, with last-contacted date and follow-up overdue flags. The March panic screen, minus the panic.
3. **Deadline engine** — the federal calendar per entity type (user-confirmed, with as-of label and jurisdiction note): standard due dates, extension dates, and per-client extended dates once an extension is filed. Line's color language (green / orange ≤5 days / red overdue). State dates are user-added jurisdictions (no shipped 50-state authority — same as-of posture as TruckPilot's rate table).
4. **E-file status log** — per return: submitted date/time, method, acceptance/rejection as reported back to the preparer, rejection code + note, resubmission. TaxPilot records the status the preparer's e-file software reports; it does not transmit anything (§5).
5. **Pipeline dashboard** — counts by status, returns due in 7/14/30 days, docs-outstanding total, extensions to file, fees outstanding. Seasonal load at a glance.
6. **Recurring-client rollover** — copy last year's client list into the new season with statuses reset and carry-forward notes (K-1 expected, rental sold, new baby — free-text flags) intact. The January head start.
7. **Appointment & contact log** — calls/meetings per client with date, who, what was promised — the DenialPilot call-log pattern (names, references, follow-ups) applied to preparer-client contact.
8. **Season export packet** — per season: full client list with statuses, filed/accepted summary, outstanding items — printable/PDF for the office file and for the off-season "where did we leave Mrs. Garcia" question.

**Platform rules (same as the rest of the line):** local JSON storage, no network calls, no account, no telemetry, works fully offline. **Privacy posture is the product:** client PII never leaves the preparer's machine, and the listing copy leads with it.

## 5. What it deliberately does NOT do

- **No tax preparation.** TaxPilot never computes a return — Drake/ProSeries/UltraTax do that. It tracks the business around the returns.
- **No e-file transmission, no IRS integration, no e-signature, no client portal.** Those are network/compliance functions belonging to the prep software and the cloud suites; TaxPilot logs the outcomes the preparer enters.
- **No document storage of client tax documents in MVP** — it tracks *that* a document was received and where the office files it (path/reference note), keeping the PII footprint minimal by design. (A local attachment vault is a post-MVP decision, not a promise.)
- **No tax advice, no shipped deadline authority for states.** Federal dates ship as clearly-labeled defaults the preparer confirms; state dates are the preparer's own data.
- **Not a bookkeeping close tool** — that's ClosePilot's mapped territory (first alternate); TaxPilot owns the return pipeline, not the monthly close.

## 6. Nearest incumbents — and how TaxPilot differs

| Incumbent | What it is | Price | TaxPilot's difference |
|---|---|---|---|
| **TaxDome** | All-in-one tax practice platform: portal, e-sign, workflow, billing | $800/user/yr Essentials; $1,200 Business | A cloud firm platform whose price and posture assume staff and client portals. TaxPilot is the tracking layer only — no portal, no PII in anyone's cloud — at $14.99/mo flat for the whole office. |
| **Canopy** | Practice management suite with metered add-ons | $74–149/user/mo + add-ons | Suite breadth (tax resolution, transcripts) a storefront office doesn't use, rented per user. TaxPilot deliberately covers the season board and nothing else. |
| **Karbon / Jetpack Workflow** | Work-management for accounting firms | Karbon $59–89/user/mo; Jetpack $40–50/seat/mo | Built for multi-staff firms with email-integrated collaboration. TaxPilot has no collaboration layer because a 1–3-person office shares a room, not a platform — they need the board, not the network. |
| **Email + spreadsheet (DIY status quo)** | The sweep-verified solo reality | Free + season stress | TaxPilot is that spreadsheet finished: entity-aware deadlines, a chase list that sorts itself, rollover between seasons, and an e-file log that answers "was the Henderson return accepted?" in seconds. |

## 7. Suggested Gumroad price

**$14.99/month membership** — one month costs less than 25 minutes of the cheapest incumbent's annual per-user fee amortized (TaxDome Essentials ≈ $67/user/mo). Priced below ShopPilot/WellPilot because the buyer is a solo preparer, not an industry-margin business — but the seasonal pain is acute and annual (they will keep it year-round for the deadline engine, which is the retention design).

## 8. Candidates considered (scored 1–5; 20 max)

| Candidate | Pain frequency | Incumbent weakness | Local-WinUI feasibility | Willingness to pay | Total |
|---|---|---|---|---|---|
| **A. TaxPilot — tax-office season tracker** | 5 | 4 | 5 | 4 | **18** |
| B. ClosePilot — bookkeeping monthly-close pipeline | 4 | 4 | 5 | 3 | 16 |
| C. Full practice suite (portal/e-sign/billing) | 5 | 2 | 1 | 4 | 12 |

Scoring notes:

- **A (18):** Pain peaks brutally every season and recurs monthly off-season (extensions, notices). Incumbent weakness scored 4, not 5, because TaxDome genuinely serves firms well — the gap is price/posture for solos, not product quality. Feasibility is maximal: lists, statuses, dates.
- **B (16):** ClosePilot — bookkeeping firms' recurring monthly-close checklist pipeline (incumbents: Financial Cents Solo $19/mo verified floor, Team $49/user/mo; Jetpack Workflow $40–50/seat/mo — https://financial-cents.com/accounting-workflow-software-om/ and https://jetpackworkflow.com/pricing/). A real, verified gap and the **first alternate for the whole app line** — held out of the top 8 only because TaxPilot takes the NAICS 54 slot and ClosePilot is its natural fast follower once the pattern proves out.
- **C (12):** The suite is the incumbents' game and is network-bound by definition (portals, e-sign, e-file). Building it local-first is a contradiction; scored to document the rejection.

## 9. Build notes (for whoever builds it — not part of this spec task)

- Closest pattern relatives: DenialPilot (deadline engine + call log + status pipeline), CasePilot (per-matter registers with dated status history), TruckPilot (user-confirmed date/rate tables with as-of labels).
- Entity-type deadline tables must be data, not code — preparers will edit them; ship federal defaults labeled with tax-year and "confirm before relying."
- Rollover (feature 6) is the retention feature — build it early, test it against a two-season sample.
- First-run sample (fictional, labeled): a 12-client mini-season — mixed entity types, two clients docs-outstanding, one rejected e-file fixed and resubmitted, one extension — showing every status color on first launch.
- Hard rule for copy: "Your client list never leaves your office." Never claim IRS integration, e-file, or deadline authority.
