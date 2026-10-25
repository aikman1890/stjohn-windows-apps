# GAP APP SPEC — EstatePilot

**Status:** Spec only (no build authorized yet)
**Date:** 2026-10-25
**Line:** The Night Shift SRE / St. John Windows apps (WinUI 3, local-first, Gumroad membership)
**Prepared for:** Shaun Nicholas St. John
**Hunt:** Gap hunt #4 — the millionaire industries (businesses that serve high-net-worth clients)

---

## 1. The app in one line

**EstatePilot** — a local-first Windows desktop app that is the estate manager's private book: every property, every staff member, every vendor, every recurring maintenance clock, and the house manual itself — held on the manager's own machine, because the families who pay for estate management also pay for discretion, and a cloud platform full of their household details is the opposite of it.

## 2. The gap

Estate management software is sold as a collaboration platform to households and firms — with a commitment structure to match — while the working estate manager's actual book is still a binder, a spreadsheet, and a phone full of vendor contacts.

**The incumbent's terms are the barrier:**

- **Nines**, the category-defining platform, describes itself as the platform for "discerning households, family offices, property managers, boutique hospitality, yacht crews, and their private service professionals" — and sells memberships on a **12-month pre-paid commitment with a one-time onboarding fee**, with custom licensing for firms and even a self-hosted configuration offered as a special arrangement (https://ninesliving.com/pricing). Dollar pricing is not published on its pricing page; the commitment is.
- That structure fits a family office standardizing across properties. It does not fit the buyer who actually holds this book day to day: the independent estate manager or house manager serving one to four families, who would be committing a year of fees and an onboarding process — and putting each family's household manual, staff details, vendor list, and preferences into a vendor's cloud, with accounts for staff to collaborate in.
- The category's other visible options are facility-maintenance tools pointed at commercial buildings (Landport and the CMMS class) — work-order systems that know nothing about households: no preferences, no staff roster in the household sense, no principal's standing instructions.

**The privacy math is the real gap:**

- An estate's records are among the most sensitive operational data a family has: addresses and floor details, staff names and schedules, when the house is occupied and when it is empty, vendor access, alarm-adjacent service history, personal preferences. The UHNW service world's entire value proposition is discretion. A local-first book — no accounts, no cloud, nothing to breach at a vendor — is not a compromise in this market; it is the premium position, and none of the incumbents occupy it (Nines' self-hosted option exists precisely because some clients refuse the cloud — offered as a custom arrangement, not a product).
- The clocks are real too: vendor insurance certificates expire, generator/HVAC/pool/elevator services come due, seasonal openings and closings (the beach house in April, the ski house in November) run on checklists that live in the manager's binder. A missed COI from a vendor working in the house, discovered after an incident, is an estate manager's career event.

**The unserved layer is the manager's own private book — properties, people, vendors, clocks, and the manual — owned outright, offline, at a flat price.** The incumbents sell platforms to households; nobody sells the manager his own book.

## 3. Target user

- **Primary:** Independent estate managers and house managers serving 1–4 UHNW families, currently running on binders, spreadsheets, and phone contacts.
- **Secondary:** Estate management firms' individual managers who need a personal working book alongside (or instead of) the firm's platform — and principals' executive assistants who carry household operations as part of their role.
- **Tertiary:** Property caretakers for seasonal/second homes (the manager of three vacation properties for absentee owners) — same book, smaller scale.

## 4. Core features (MVP, all local-first)

1. **Property file** — per residence: address, type (primary/seasonal), key systems (HVAC, generator, pool, elevator, security — free-form), access notes held locally, document vault (local paths: manuals, warranties, service contracts, floor plans).
2. **Maintenance clock engine** — recurring services per property with the line's countdown language (green / orange ≤30 days / red overdue): HVAC service, generator exercise/service, pool opening/closing, chimney, septic, elevator inspection, appliance services. Next-due computed from last-done entries; the seasonal open/close checklists attach to the clocks they belong to.
3. **Staff book** — household staff per property: role, schedule pattern, contact, start date, document clocks where the manager tracks them (e.g., certifications, background-check renewal dates — user-entered, user-confirmed). No payroll — records only.
4. **Vendor book with insurance clocks** — vendors per property: trade, contact, last job, rate notes, and **certificate-of-insurance expiry tracked as a first-class clock** — the vendor list shows at a glance who is currently insured to work in the house.
5. **House manual** — structured, searchable local manual per property: room-by-room notes, standing instructions, principal preferences (how the house is set on arrival, pantry standards, guest preferences), seasonal procedures. This is the artifact Nines sells as its centerpiece — here it lives in the manager's book, exportable to PDF/print for handover.
6. **Project & job ledger** — work done per property: vendor, date, cost, invoice reference, warranty expiry clocked where relevant. The record that answers an owner's "when was the roof last done and what did it cost?"
7. **Arrival/departure checklists** — reusable checklists per property (open the house, close the house, guest arrival) with check-off states and a dated completion record — the proof ledger for the invisible work.
8. **Dashboard across properties** — every clock in every house sorted by pressure: services overdue, COIs expiring, seasonal openings upcoming, checklists uncompleted for a scheduled arrival.

**Platform rules (same as the rest of the line):** local JSON storage, no network calls, no account, no telemetry, works fully offline. In this market the rule is the marketing: *your families' details never leave your machine.*

## 5. What it deliberately does NOT do

- **No staff accounts, messaging, or task assignment.** Collaboration is Nines' platform and its price/commitment; EstatePilot is a book, not a workplace chat. (Checklists are recorded and exported, not assigned.)
- **No payroll, timekeeping, or HR functions** for household staff — records and clocks only; employment compliance stays with the family's payroll provider.
- **No financial management or investment/family-office accounting.** Explicitly out of scope per the hunt's rules: no regulated-advice territory, no portfolio anything. Household operating *expenses* appear only as job-ledger costs (what a repair cost), never as financial advice or wealth records.
- **No smart-home/security system integration and no storage of alarm codes or access credentials.** The app records *that* a system exists and when it was serviced; it never holds the keys to the house — a deliberate safety line for copy and design alike.
- **Not VenuePilot** (a venue's commercial date book) **and not a property-management system for rentals** — no tenants, no rent roll (the landlord lane was dropped in an earlier hunt for cause). These are owner-occupied residences under private management.

## 6. Nearest incumbents — and how EstatePilot differs

| Incumbent | What it is | Price | EstatePilot's difference |
|---|---|---|---|
| **Nines** (ninesliving.com) | Cloud household/estate management platform: house manual, tasks, staff & vendor collaboration for households and family offices | 12-month pre-paid memberships + one-time onboarding fee; firm licensing custom; dollar pricing not published (pricing page) | Nines rents the *household* a collaboration platform, forever, with everyone's accounts inside the family's data. EstatePilot sells the *manager* his own book: flat membership, no commitment, no onboarding, and the data never leaves his machine — the discretion position Nines only offers as a custom self-hosted arrangement. |
| **Landport / CMMS class** (landport.net) | Facility-maintenance and work-order systems aimed at buildings and property portfolios | Landport listed from $45/mo (SourceForge comparison data) | Work orders for facilities, not households: no preferences, no house manual, no staff/vendor book in the household sense, no seasonal open/close discipline. EstatePilot is built around how a residence is actually run. |
| **The binder + spreadsheet + phone (DIY status quo)** | House manual in a binder, services in a spreadsheet or the manager's head, vendors in phone contacts | Free + one forgotten generator service or one lapsed vendor COI away from a very bad week | This is the real incumbent for the independent manager. EstatePilot is that binder, finished: every clock computed, every COI expiring in view, the manual searchable, and the work ledger exportable when an owner asks what got done. |

## 7. Suggested Gumroad price

**$39.99/month membership** — premium tier. Rationale: the comparison is a 12-month pre-paid platform commitment with onboarding fees, bought to serve families for whom the manager's competence *is* the product. For an estate manager, $39.99 is priced against one avoided failure — a generator that didn't get serviced before storm season, a vendor working uninsured — and against the discretion pitch he can make to principals: "your household's details live in my book, on my machine, nowhere else." That sentence alone is worth the tier in this market.

## 8. Candidates considered (scored 1–5; 20 max)

| Candidate | Pain frequency | Incumbent weakness | Local-WinUI feasibility | Willingness to pay | Total |
|---|---|---|---|---|---|
| A. YachtPilot — yacht management book | 5 | 4 | 5 | 5 | 19 |
| B. CharterPilot — charter broker quote & trip book | 4 | 4 | 5 | 5 | 18 |
| C. GalleryPilot — art gallery consignment & provenance book | 4 | 5 | 5 | 4 | 18 |
| **D. EstatePilot — estate/house manager's book** | 4 | 4 | 5 | 5 | **18** |
| E. StablePilot — equestrian boarding/training barn book | 5 | 3 | 5 | 4 | 17 |
| F. Atelier book — luxury wedding/event planner | 4 | 3 | 5 | 4 | 16 |
| G. Vault book — exotic/luxury car storage | 3 | 3 | 5 | 4 | 15 |
| H. Dealer book — watch & jewelry consignment/repair | 3 | 3 | 5 | 3 | 14 |
| I. Chef's book — private chef client & menu records | 3 | 2 | 5 | 3 | 13 |
| J. Settlement book — regional auction houses | 3 | 2 | 4 | 3 | 12 |
| K. Cellar book — wine storage & private cellars | 3 | 2 | 4 | 3 | 12 |
| L. Lodge book — luxury fishing/hunting lodges | 3 | 2 | 4 | 3 | 12 |
| M. Club book — private golf/country clubs | 3 | 1 | 2 | 4 | 10 |
| N. Practice book — concierge medicine | 3 | 1 | 2 | 4 | 10 |
| O. Guard book — private security firms | 3 | 1 | 1 | 3 | 8 |

Scoring notes (evidence, not vibes):

- **A (19):** Spec'd separately as the hunt winner (Total Superyacht from $200/mo per vessel — https://sourceforge.net/software/marine/for-small-business/?page=2; YachtOpera €120–€220/mo per vessel — https://yachtopera.com/).
- **B/C (18):** Spec'd separately. EstatePilot ties them on total with the strongest willingness-to-pay in the set and the only candidate whose local-first posture is itself the premium feature; it loses a point on incumbent weakness because Nines is a genuinely good product — the gap is its terms and form factor, not its quality.
- **E (17):** Spec'd separately — the barn book's clocks are statutory (12-month Coggins — https://animalofthings.com/eia-coggins-test-requirements-in-florida/) but incumbents are cheaper and closer.
- **F (16):** The luxury planner's book is real but sits between VenuePilot (venues, already spec'd) and cloud planner CRMs; EstatePilot's buyer is less served by both neighbors.

## 9. Build notes (for whoever builds it — not part of this spec task)

- Reuse the line's proven patterns: local JSON store; derived countdowns with green/orange/red states (30-day orange band for services and COIs); recurring-clock engine shared in shape with KennelPilot's care schedules and LienPilot's timelines; checklist module with dated completions; PDF/print export for the house manual and per-property work ledger.
- No shared Licensing/voice/integrity code, consistent with the v40 apps — no new secrets needed.
- Ship 100 languages the same way as the rest of the line; all date math locale-safe (store ISO dates; render localized).
- **Security design note (binding):** the app must never offer fields for alarm codes, safe combinations, gate codes, or passwords. Property access is recorded as "held by [role]" pointers only. This is a product-safety rule, not a preference.
- First-run sample estate (one fictional residence with systems, clocks, two vendors — one COI inside its orange window — and a starter house manual, clearly labeled sample) so a manager sees the dashboard working before entering a real family's details.
- Hard rule for copy: never promise security or outcomes. Sell the book: clocks that can't lapse silently, a manual that survives staff turnover, and discretion — records that never leave the manager's machine. Standing disclaimer: records and reminders only; employment, insurance, and safety compliance remain the responsibility of the household and its advisors.
