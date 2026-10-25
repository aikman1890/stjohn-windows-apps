# GAP APP SPEC — StablePilot

**Status:** Spec only (no build authorized yet)
**Date:** 2026-10-25
**Line:** The Night Shift SRE / St. John Windows apps (WinUI 3, local-first, Gumroad membership)
**Prepared for:** Shaun Nicholas St. John
**Hunt:** Gap hunt #4 — the millionaire industries (businesses that serve high-net-worth clients)

---

## 1. The app in one line

**StablePilot** — a local-first Windows desktop app that runs a boarding/training barn's horse book: every horse, every owner, the care clocks the law and the calendar impose (Coggins, farrier, vaccinations, deworming), the feed and turnout boards, and the monthly board billing — built for the barn office, where the cloud platforms' portals matter less than the clocks.

## 2. The gap

Equestrian software is a shelf of cloud platforms selling portals, online booking, and per-feature add-ons to lesson barns — while the moneyed end of the market, the private and boarding/training barn, runs on a whiteboard, a feed chart, and a paper health binder, and answers to owners who expect a private banker's standard of record-keeping.

**The clocks here are not preferences — several are law:**

- A negative **Coggins (EIA) test within 12 months** is required for interstate movement in all states and is a condition of entry at most boarding facilities, shows, and events; Florida's rules extend the requirement to boarding facilities, shows, exhibitions, and changes of ownership, with facilities required to keep EIA records (https://animalofthings.com/eia-coggins-test-requirements-in-florida/). A horse whose Coggins lapses can't legally travel to the show the owner is paying to attend — and the barn that let it lapse owns that conversation.
- Around that statutory clock sits a ring of care cycles: farrier visits on a roughly 6–8 week rotation per horse, vaccination schedules, deworming rotations, dental floats. On a 25-horse barn that is hundreds of dated obligations a year, tracked today on whiteboards and in the barn manager's head.
- Health certificates (CVI) for travel are typically valid only **30 days** (https://usrider.org/prepare-paperwork-ahead-of-time-what-you-need-before-hitting-the-road-with-your-horse/) — a show-season barn is constantly inside a paperwork window.

**The incumbents are cloud platforms priced and shaped for a different barn:**

- **BarnManager**, the category name, prices **per feature** (starting at $10/feature/month per GetApp — https://www.getapp.com/industries-software/a/barnmanager/; third-party comparisons put it around **~$59/month** in typical configurations, with EquineOps at $35/month and Stable Secretary at ~$25/month — https://equineops.com/blog/the-best-barn-management-software-for-horse-trainers-%E2%80%94-equineops-vs.-barnmanager-vs.-stable-secretary). **Stable Manager Pro** runs **$49–$249/month** (https://stablemanagerpro.com/).
- The comparison literature itself shows the shape mismatch: the platforms compete on client portals, online lesson booking, provider logins, and payment processing — the lesson-barn business. A private boarding/training barn's owners don't book lessons online; they expect their horse's records to be perfect and their bill to be right.
- And the form factor fights the workplace: barn offices and barn aisles are exactly where connectivity is worst and a shared cloud login is most awkward. The health binder is paper because paper works in a barn. A local desktop book in the barn office is the first form factor that matches the place.

**The unserved layer is the horse book itself — per-horse records with statutory and care clocks computed, feed/turnout boards, incident/health log, and board billing with owner statements — local, flat, and offline.** (This is equestrian, a moneyed market of private owners — expressly not the cattle/ranching lane dropped in an earlier hunt; no livestock production software is proposed or wanted.)

## 3. Target user

- **Primary:** Boarding and training barns (10–40 horses) serving private owners — dressage, hunter/jumper, and sport-horse barns where board runs four figures a month per horse and owners expect records to match.
- **Secondary:** Private barns (a family's own stable and staff) where the barn manager is the record-keeper of record.
- **Tertiary:** Trainers with a string of client horses in training — per-horse training/session logs feeding the monthly owner bill.

## 4. Core features (MVP, all local-first)

1. **Horse file** — one record per horse: owner, stall, arrival date, feed program, turnout group, blanket notes, vet/farrier assignments, document vault (local paths: Coggins certificate, health papers, registration, insurance). Owner view rolls up every horse they board.
2. **Care clock engine** — per horse, the cycles that run a barn, in the line's countdown language (green / orange ≤14 days / red overdue): **Coggins expiry (12-month statutory cycle)**, farrier cycle (user-set weeks), vaccinations, deworming rotation, dental, and CVI windows when travel is scheduled. The dashboard shows the whole barn's clocks sorted by pressure — and a horse with an expired Coggins is flagged before it is loaded onto a trailer, not after.
3. **Feed & turnout board** — the daily board, printable for the barn aisle: per-horse feed (AM/PM), supplements/meds with sign-off, turnout assignments, blanket rules by temperature (user-entered). Print is a first-class output — the aisle copy is paper by design.
4. **Health & incident log** — dated, append-only entries per horse: vet visits, injuries, treatments, lameness notes, with the treating provider. This is the record an owner (or an insurer) asks for, kept as it happens.
5. **Training/session log** (for training barns) — sessions per horse per trainer, feeding the monthly bill as itemized training charges.
6. **Board billing ledger** — monthly board per horse plus itemized extras (training, holding for vet/farrier, supplies, show fees advanced), computed in decimal math; **owner statement export** (PDF/print) per owner per month — the document that replaces the hand-built invoice spreadsheet.
7. **Provider book** — vets, farriers, dentists, chiropractors, transporters: contact, horses they serve, last visit, next scheduled round (a farrier day booked against the cycle clocks).
8. **Show/travel readiness view** — pick horses and a date: the app shows which are travel-legal (Coggins current, CVI window, vaccinations as entered) and which are not, before the trailer is booked.

**Platform rules (same as the rest of the line):** local JSON storage, no network calls, no account, no telemetry, works fully offline — in the barn office, on the barn's computer, whether or not the property has usable internet at the stable.

## 5. What it deliberately does NOT do

- **No lesson booking, client portals, or online payments.** That is the incumbents' lesson-barn business; StablePilot's buyer bills monthly from a ledger and owners pay as they always have.
- **No veterinary advice, diagnosis, or treatment recommendations.** It tracks the schedules and records the vet sets; medical judgment stays with the vet, and screens say so.
- **No breeding management, no sales/auction tools, no cattle or livestock production features.** This is a sport/boarding horse book — the ranching lane stays dropped, per the hunt's rules.
- **No owner app or cloud sync.** The owner-facing artifact is the exported statement and the readiness view shown in the office.
- **Not KennelPilot** — the boarding pattern rhymes, but the records (Coggins law, farrier cycles, training logs, four-figure monthly board) and the buyers are a different market entirely.

## 6. Nearest incumbents — and how StablePilot differs

| Incumbent | What it is | Price | StablePilot's difference |
|---|---|---|---|
| **BarnManager** (barnmanager.com) | Cloud barn platform: records, calendar, invoicing on upper tiers, mobile apps | Per-feature pricing from $10/feature/mo (GetApp); ~$59/mo typical (EquineOps comparison) | Priced by the feature and built around cloud access everywhere. StablePilot is one flat book on the barn's own computer: the statutory clock engine and billing ledger are the core, not add-on features, and it works in a barn with no signal. |
| **EquineOps / Stable Secretary / Stable Manager Pro** | Cloud platforms aimed at trainers and lesson/riding-school operations (portals, booking, provider logins) | EquineOps $35/mo; Stable Secretary ~$25/mo; Stable Manager Pro $49–$249/mo | They sell the lesson-barn front office. StablePilot sells the boarding barn's back office: Coggins/farrier/vaccination clocks per horse, health log, and owner statements for owners who pay four figures a month and read them. |
| **The whiteboard + feed chart + paper binder (DIY status quo)** | Care schedules on the aisle whiteboard, health records in a binder, bills built in a spreadsheet monthly | Free + one lapsed Coggins or one wrong bill away from losing a client worth thousands a year | This is the real incumbent at the moneyed end. StablePilot is that system finished: clocks computed per horse, travel legality checked before the trailer, bills generated from the ledger instead of assembled from memory. |

## 7. Suggested Gumroad price

**$29.99/month membership** — premium tier. Rationale: the buyer's arithmetic is per-horse — a single boarded horse at a quality barn bills more per month than a year of StablePilot, and a barn of twenty horses loses more than the membership costs in one billing error or one show trip missed over an expired Coggins. The price sits deliberately under the cloud platforms' realistic configurations while refusing their per-feature/per-seat metering: one barn, one book, one price.

## 8. Candidates considered (scored 1–5; 20 max)

| Candidate | Pain frequency | Incumbent weakness | Local-WinUI feasibility | Willingness to pay | Total |
|---|---|---|---|---|---|
| A. YachtPilot — yacht management book | 5 | 4 | 5 | 5 | 19 |
| B. CharterPilot — charter broker quote & trip book | 4 | 4 | 5 | 5 | 18 |
| C. GalleryPilot — art gallery consignment & provenance book | 4 | 5 | 5 | 4 | 18 |
| D. EstatePilot — estate/house manager's book | 4 | 4 | 5 | 5 | 18 |
| **E. StablePilot — equestrian boarding/training barn book** | 5 | 3 | 5 | 4 | **17** |
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

- **A–D (19/18):** Spec'd separately in this hunt. StablePilot's pain frequency ties the winner's (daily care, monthly billing, statutory deadlines), but its incumbents are cheaper and closer to the need (barn platforms from $25–59/mo), so incumbent weakness scores 3 — the wedge is form factor and the clock engine, not a $200/mo floor.
- **F (16):** The planner's book loses on adjacency: VenuePilot (already spec'd) covers the venue half of the same events and cloud planner CRMs cover the rest adequately.
- **G (15):** Exotic-car storage is a real book (condition intake, exercise/tender clocks) but it splits the difference between BayPilot and LienPilot; possible later sibling of LienPilot rather than its own build.

## 9. Build notes (for whoever builds it — not part of this spec task)

- Reuse the line's proven patterns: local JSON store; derived countdowns with green/orange/red states (14-day orange band for care cycles; 30-day for Coggins); recurring-cycle engine in the same shape as KennelPilot's schedules and EstatePilot's maintenance clocks; append-only health log; printable board output (design the feed/turnout board for paper first); PDF/print owner statements.
- No shared Licensing/voice/integrity code, consistent with the v40 apps — no new secrets needed.
- Ship 100 languages the same way as the rest of the line; all date math locale-safe (store ISO dates; render localized). Money in decimals.
- First-run sample barn (four fictional horses — one with a Coggins inside its orange window, one overdue for the farrier — an owner with two horses, and a generated monthly statement, clearly labeled sample) so a barn manager sees the clocks, the board, and the bill working before entering a real horse.
- Hard rule for copy: never promise animal health outcomes or legal compliance ("keep you legal"). Sell the clocks and the records: nothing expires unnoticed, every bill itemized, every incident written down when it happened. Standing disclaimer: records and scheduling only — veterinary care and regulatory compliance remain the responsibility of the barn and its veterinarians.
