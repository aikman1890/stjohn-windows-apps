# GAP APP SPEC — YachtPilot

**Status:** Spec only (no build authorized yet)
**Date:** 2026-10-25
**Line:** The Night Shift SRE / St. John Windows apps (WinUI 3, local-first, Gumroad membership)
**Prepared for:** Shaun Nicholas St. John
**Hunt:** Gap hunt #4 — the millionaire industries (businesses that serve high-net-worth clients)

---

## 1. The app in one line

**YachtPilot** — a local-first Windows desktop app that runs a small yacht management company's whole book: every vessel, every crew certificate, every maintenance clock, every charter, and every dollar spent for an owner — on the manager's own machine, at one flat price, instead of per-vessel cloud rent.

## 2. The gap

Yacht software is priced per vessel, for fleets, in the cloud — and the people who actually run 2–10 boats for owners still run on spreadsheets and chat threads.

**The incumbents' pricing assumes a fleet department:**

- **Total Superyacht**, the ISM/compliance incumbent, is listed on SourceForge starting at **$200/month** — per vessel (https://sourceforge.net/software/marine/for-small-business/?page=2).
- **YachtOpera**, the newer entrant, charges **€120–€220/month per vessel** by size band, plus a **€50/month per-vessel add-on** for its ISM module; its fleet plan for management companies is "price on request" then €99/vessel (https://yachtopera.com/).
- A small management company running five yachts is therefore looking at roughly **€600–€1,350/month** before it has typed a single record — for software whose core job, at that scale, is lists and clocks.

**The vendors' own marketing documents the status quo they replace:**

- A YachtOpera captain testimonial: the platform "replaced four separate spreadsheets and a shared WhatsApp group" (https://yachtopera.com/). That *is* the small-operator status quo: the vessel book lives in spreadsheets, and the operational record lives in a chat thread nobody can audit.
- A fleet manager testimonial on the same page: "The crew document alerts alone saved us from an embarrassing certificate lapse." Crew certificates (STCW basics, ENG1 medicals, licenses, passports/visas) expire on fixed dates; a lapse discovered at a port-state inspection or mid-charter is a reputation event with an owner who has other managers calling him. The pain is a *clock* problem, and clocks are this product line's home turf (DisputePilot's countdowns, LienPilot's statutory timelines).
- The third structural fact: **boats lose signal.** A management tool that requires a cloud account and a live connection is least available exactly where the work is — offshore, in a yard, in a marina dead zone. The incumbents are cloud-only by design.

**The unserved layer is the management company's own book — vessel files, certificate clocks, maintenance clocks, charter calendar, and the owner expense ledger — in one local app at one flat price, owned by the manager, readable offline.** Nobody sells that: the category sells per-vessel cloud platforms to fleets and leaves the 2–10-boat manager to spreadsheets and WhatsApp.

## 3. Target user

- **Primary:** Small yacht management companies (2–10 vessels under management) — the manager who answers to owners monthly and currently assembles that answer from spreadsheets, email, and chat history.
- **Secondary:** Captains of owner-operated yachts (40–80 ft) who are their own manager: certificate clocks, maintenance clocks, and an expense record the owner can be shown.
- **Tertiary:** Charter brokers' back offices and yacht service coordinators who track the same vessels' readiness across a season.

## 4. Core features (MVP, all local-first)

1. **Vessel file** — one record per yacht: name, flag, berth, owner, dimensions/class, insurance and registration expiry dates (user-entered, clocked like everything else), document vault (local file paths for certificates, surveys, manuals).
2. **Crew certificate clock engine** — per crew member: certificate type (STCW, ENG1 medical, license, passport, visa — free-form types allowed), issue/expiry dates, and the line's standard countdown language (green / orange ≤30 days / red expired — wider orange band than the legal apps because renewals involve courses and doctors booked weeks out). The app refuses to show a vessel "charter-ready" while any assigned crew certificate is expired, until the user overrides with a note — the DenialPilot/LienPilot "not ready with a step unlogged" pattern.
3. **Maintenance clocks** — recurring and metered items per vessel: engine/generator service (by date or engine hours, user-logged), haul-out & antifouling, zincs, liferaft & flare service dates, rig inspection (sailing yachts). Next-due computed and shown per vessel; overdue items roll up to the dashboard.
4. **Charter & owner-use calendar** — per vessel: charters (broker, charterer name held locally, dates, rate, APA-style advance allowance tracked as a simple ledger), owner-use blocks, yard/maintenance blocks. Conflicts flagged on entry (double-booked dates, charter overlapping a maintenance block or an expired-certificate window).
5. **Owner expense ledger** — every cost per vessel per month: berth, crew, maintenance, provisions, fuel, management fee. Monthly **owner statement export** (PDF/print): the document the manager sends the owner, generated from the ledger instead of assembled from spreadsheets.
6. **Vendor & yard book** — yards, engineers, riggers, detailers, provisioners: contact, vessels they've worked on, last job, next booked job. (The vendor's insurance/COI expiry can be clocked too.)
7. **Fleet dashboard** — all vessels sorted by clock pressure: certificates expiring, maintenance overdue, charters upcoming, unlogged expenses. One board for the whole book.
8. **Proof & activity ledger** — per vessel, a chronological record: certificates renewed, services completed, charters run, statements sent (method + date). Exportable per vessel — the artifact that answers an owner's "what have you actually done this quarter?"

**Platform rules (same as the rest of the line):** local JSON storage, no network calls, no account, no telemetry, works fully offline — which is not a limitation here but the headline feature: the book works at sea, in the yard, and in the marina dead zone.

## 5. What it deliberately does NOT do

- **No live vessel tracking (AIS), no weather, no navigation.** That layer is networked by nature and owned by others; YachtPilot is the back-office book, not the bridge.
- **No ISM/flag-state compliance certification.** It tracks the dates and documents the manager enters and flags expiries; it does not certify compliance with ISM, MLC, or any flag state's code, and screens say so (same disclaimer pattern as ClaimPilot/DenialPilot: organization and records, not regulated advice).
- **No charter booking marketplace, no payments, no crew payroll.** Money is recorded in the ledger; it never moves through the app.
- **No owner or crew accounts/portals.** The incumbents' collaboration layer is exactly what makes them per-vessel cloud rent; YachtPilot's buyer is the manager, and the owner-facing artifact is the exported statement.
- **Not a marina/berth-billing system** (PacsoftNG and marina platforms own that, with payment processing YachtPilot deliberately lacks) **and not KennelPilot/LienPilot-style storage** — vessels under active management are a different book from units or kennels.

## 6. Nearest incumbents — and how YachtPilot differs

| Incumbent | What it is | Price | YachtPilot's difference |
|---|---|---|---|
| **Total Superyacht** (totalsuperyacht.com) | Cloud ISM/safety-management platform for crewed yachts and fleets | From $200/mo per vessel (SourceForge listing) | A compliance platform rented per hull. YachtPilot is the small manager's whole book — certificates, maintenance, charters, owner money — at one flat membership, on the manager's own machine, working with no signal. |
| **YachtOpera** (yachtopera.com) | Cloud yacht-operations platform; per-vessel tiers by size, ISM as paid add-on | €120–€220/mo per vessel + €50/mo ISM add-on | Its own testimonials describe replacing "four separate spreadsheets and a shared WhatsApp group" — proof of the status quo. YachtPilot replaces the same stack for the manager who has 3 boats, not 30, and refuses per-vessel rent and per-module add-ons to do it. |
| **The spreadsheet + WhatsApp stack (DIY status quo)** | Vessel info in spreadsheets, operations in a chat thread, owner reports assembled by hand | Free + hours + one lapsed certificate away from a fired manager | This is the real incumbent for the 2–10-boat segment. YachtPilot is that stack, finished: computed certificate/maintenance clocks with a charter-ready gate, an owner statement generated from a ledger, and an activity record an owner can be shown. |

## 7. Suggested Gumroad price

**$39.99/month membership** — the line's premium tier. Rationale: the comparison is not other desktop apps; it is €120–€220 *per vessel per month* of cloud rent. A five-vessel manager saves several hundred dollars a month from the first vessel, and the clientele (owners of seven-figure assets, and the managers who answer to them) treats $39.99 as a rounding error against a single avoided certificate lapse or one clean owner statement. Price it flat — no per-vessel tiers — because "we don't rent by the hull" is the marketing line.

## 8. Candidates considered (scored 1–5; 20 max)

| Candidate | Pain frequency | Incumbent weakness | Local-WinUI feasibility | Willingness to pay | Total |
|---|---|---|---|---|---|
| **A. YachtPilot — yacht management book (vessels, crew certs, owner ledger)** | 5 | 4 | 5 | 5 | **19** |
| B. CharterPilot — charter broker quote & trip book | 4 | 4 | 5 | 5 | **18** |
| C. GalleryPilot — art gallery consignment & provenance book | 4 | 5 | 5 | 4 | **18** |
| D. EstatePilot — estate/house manager's book | 4 | 4 | 5 | 5 | **18** |
| E. StablePilot — equestrian boarding/training barn book | 5 | 3 | 5 | 4 | **17** |
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

- **B (18):** Spec'd separately in this hunt. Leon charges brokers €99/user/mo (https://www.leonsoftware.com/pricing.html) for a platform tier; the broker's own quote book is still email and spreadsheets.
- **C (18):** Spec'd separately. Artlogic starts at $205/mo Essential / $409/mo Professional (https://www.softwareworld.co/software/artlogic-reviews/) — the highest incumbent floor in the hunt; loses a point only on buyer count (fewer galleries than barns).
- **D (18):** Spec'd separately. Nines sells estate management on 12-month pre-paid memberships with onboarding fees and firm licensing (https://ninesliving.com/pricing) — a commitment structure a solo manager can't justify; privacy (local-only) is the wedge.
- **E (17):** Spec'd separately. Real clocks (12-month Coggins validity is law in most states — https://animalofthings.com/eia-coggins-test-requirements-in-florida/), but incumbents start lower (BarnManager per-feature from $10/feature/mo — https://www.getapp.com/industries-software/a/barnmanager/), so incumbent weakness scores 3.
- **F (16):** Planners are served by cloud CRMs (Aisle Planner/HoneyBook class) and VenuePilot already covers the venue side of the same events; the planner-only book is real but adjacent to a spec already in the line.
- **G (15):** Exotic-car storage records (condition intake, tender/exercise clocks) are real, but the pattern sits between BayPilot (repair) and LienPilot (storage clocks) — cannibalization risk caps it.
- **M/N (10):** Clubs and concierge practices buy institutionally (Jonas/Northstar-class club systems; EHR-integrated DPC platforms like Hint) — the value is networked (POS, member billing, e-prescribing), which local-first cannot and should not replicate.
- **O (8):** Guard scheduling/tour verification is a live mobile cloud category (TrackTik class). Wrong form factor, same failure mode as hunt #3's field-service drops.

## 9. Build notes (for whoever builds it — not part of this spec task)

- Reuse the line's proven patterns: local JSON store; derived countdowns with green/orange/red states (wider orange band, 30 days, for certificates); status pipelines; proof-ledger notes; PDF/print export for the owner statement (mirrors InvoicePilot/BizPilot export patterns).
- No shared Licensing/voice/integrity code, consistent with the v40 apps — no new secrets needed.
- Ship 100 languages the same way as the rest of the line; all date math locale-safe (store ISO dates; render localized). Currency fields must be multi-currency (EUR/USD/GBP) — this market is Mediterranean and Caribbean as much as American; store amounts as decimals with an explicit currency code, never floats.
- First-run sample fleet (two fictional vessels with crew, clocks, and one charter, clearly labeled sample) so a manager sees the charter-ready gate and the owner statement working before entering anything real.
- Hard rule for copy: never promise compliance or outcomes ("pass your inspection"). Sell organization, clocks, and proof — the things the evidence says managers actually lose by lacking. Screens carry a standing disclaimer: records and reminders only; maritime certification and flag-state compliance remain the operator's responsibility.
