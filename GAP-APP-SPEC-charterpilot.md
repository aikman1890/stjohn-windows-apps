# GAP APP SPEC — CharterPilot

**Status:** Spec only (no build authorized yet)
**Date:** 2026-10-25
**Line:** The Night Shift SRE / St. John Windows apps (WinUI 3, local-first, Gumroad membership)
**Prepared for:** Shaun Nicholas St. John
**Hunt:** Gap hunt #4 — the millionaire industries (businesses that serve high-net-worth clients)

---

## 1. The app in one line

**CharterPilot** — a local-first Windows desktop app that runs an independent private-jet charter broker's book: every quote sent, every follow-up owed, every operator and tail in the vendor book, every trip file, and the margin on every deal — because in charter brokerage the money is lost in the follow-up, not the flying.

## 2. The gap

Charter brokerage is a quote-speed business run on inboxes. The platforms serve operators; the broker's own book is email, memory, and spreadsheets.

**The platform pricing points at operators, not brokers:**

- **Leon Software**, the business-aviation ops platform, publishes its pricing openly: operators pay **€99/aircraft/month (Basic) up to €350–€929/aircraft/month (Pro, by weight class)** — and **charter brokers pay €99/user/month** for its broker tier (https://www.leonsoftware.com/pricing.html). FL3XX and Avianis sell the same operator-side layer (quoting engines tied to a fleet an independent broker doesn't have).
- What the €99 buys a broker is a seat inside an operator-centric platform. What it does not give him is *his own book*: the record of every quote he has ever sent a client, what it cost him, what margin he quoted, who never answered, and which operator's tail actually performed.
- Leon's own review summaries state the category pitch plainly: the platform exists to replace "disconnected spreadsheets and multiple point solutions" (https://sourceforge.net/software/compare/Aircraft-Maintenance-Systems-vs-Leon/). For operators with fleets and duty rosters, that's true. The independent broker — 1 to 3 people, no aircraft, living on other people's tails — is the segment the platforms price as an afterthought seat.

**The pain is a follow-up clock, and it is brutal:**

- A charter quote is perishable: aircraft reposition, crews time out, and the client is simultaneously holding quotes from three other brokers. A quote not followed up within hours is usually a dead quote — but nothing in an inbox tells a broker *which* of the forty quotes he sent this week are still alive, which need a nudge today, and which died and why.
- The margin record matters just as much: a broker who can't see, per client and per operator, what he quoted vs. what the trip actually cost him cannot tell his profitable clients from his time sinks. That ledger today lives in email threads and a spreadsheet updated when someone remembers.
- Client preference knowledge (who needs a specific cabin class, who always wants a particular FBO, whose assistant actually books) is the broker's entire competitive moat — and it lives in his head and his inbox, unsearchable and unexportable.

**The unserved layer is the broker's own book — quote log with follow-up clocks, client book, operator/tail vendor book, trip files, and a margin ledger — local, private, and flat-priced.** The platforms won't build it: their customer is the operator.

## 3. Target user

- **Primary:** Independent charter brokers and 1–3-person brokerages quoting retail and wholesale trips daily, currently tracking quotes in email + spreadsheet.
- **Secondary:** Aircraft management companies' charter desks (a managed fleet quoted by one or two people) who need the quote book without buying a full ops platform.
- **Tertiary:** Concierge/travel firms that arrange charter as one service line and need the trip-file discipline without aviation-platform pricing.

## 4. Core features (MVP, all local-first)

1. **Quote log with follow-up clocks** — one record per quote: client, route, dates, aircraft class, operator quoted, cost, price quoted, margin, status pipeline (Sent → Follow-up due → Negotiating → Booked / Lost / Expired). Each sent quote gets a follow-up clock (user-set presets: 4 hours, same-day, next morning) shown in the line's green/orange/red language — the dashboard is a list of quotes that will die today unless touched.
2. **Client book** — clients and their bookers/assistants: contact, preferences (aircraft class, cabin needs, catering, FBO preferences, ground transport), quote history, booked history, lifetime margin. The moat, made searchable.
3. **Operator & tail vendor book** — operators and specific tails they fly: aircraft type, home base, the broker's own notes on reliability, catering standard, cancellation behavior, last trips flown, deadhead/repositioning notes. When a trip goes wrong, the record is here — not in memory.
4. **Trip file** — per booked trip: the document checklist (signed charter agreement, operator confirmation, passenger list received, catering order, ground transport, payment received) with check-off states and a readiness view — the app shows a trip "not ready" while any checklist item is unlogged, the DenialPilot/LienPilot gate pattern.
5. **Margin ledger** — per trip and rolled up per client/operator/month: quoted price, final operator cost, other costs, margin kept. Decimal money math; multi-currency (USD/EUR/GBP) with the currency stored per record.
6. **Lost-quote register** — every lost/expired quote with a reason code (price, timing, no response, aircraft unavailable, went with another broker) — the view that shows a broker his real loss pattern instead of his impression of it.
7. **Dashboard** — follow-ups due today, trips departing in 48 hours with incomplete files, quotes awaiting operator answers, month margin to date. One screen, sorted by what expires first.

**Platform rules (same as the rest of the line):** local JSON storage, no network calls, no account, no telemetry, works fully offline. Client lists and margins are the most commercially sensitive data a broker owns; that they never leave his machine is a headline feature, not a footnote.

## 5. What it deliberately does NOT do

- **No flight operations, dispatch, or crew-duty/FTL tracking.** That layer must be live and is the operator platforms' job (Leon/FL3XX); CharterPilot never presents itself as an ops or safety tool, and screens say so.
- **No marketplace access or live availability** (no Avinode-style sourcing, no empty-leg feeds) — networked by definition, out of the local-first lane.
- **No payments, invoicing-as-a-platform, or client portals.** Invoices can be generated/exported as documents (InvoicePilot pattern), but money never moves through the app and clients never log in.
- **No quoting engine** (no live cost calculation against operator rate feeds). The broker enters the quotes he actually sent; CharterPilot owns the record and the clocks, not the pricing math.
- **Not ClientPilot** (BCCS's own prospect/pitch tracker) **and not InvoicePilot** — CharterPilot is a vertical book for charter brokerage: quote perishability clocks, operator/tail records, and trip-file checklists are its reason to exist.

## 6. Nearest incumbents — and how CharterPilot differs

| Incumbent | What it is | Price | CharterPilot's difference |
|---|---|---|---|
| **Leon Software — broker tier** (leonsoftware.com) | Seat in an operator-centric cloud ops platform (scheduling, quoting, crew, maintenance for operators) | €99/user/mo (brokers); €99–€929/aircraft/mo (operators) | Leon's customer is the operator; the broker rents a seat in someone else's system of record. CharterPilot is the broker's *own* system of record: his quotes, his margins, his client preferences — local, flat-priced, and his when he cancels. |
| **Avinode / marketplace + inbox (status quo stack)** | Sourcing marketplace where requests are found, plus the broker's email and spreadsheets for everything after | Marketplace fees + free tools + hours | Avinode finds the aircraft; it does not run the follow-up. The deals die after the quote, in the inbox. CharterPilot is the layer after the quote: clocks, trip files, margin truth. |
| **The spreadsheet quote tracker (DIY status quo)** | Broker-built sheet: quotes, prices, a status column updated when remembered | Free + the deals that died un-followed-up | CharterPilot is that sheet finished: computed follow-up clocks that turn red, a trip-file gate that won't call a trip ready with documents missing, and a lost-quote register with reason codes. |

## 7. Suggested Gumroad price

**$39.99/month membership** — premium tier, same as YachtPilot. Rationale: the reference price is €99/user/month for a platform seat that doesn't hold the broker's own book, and the unit economics are extreme — a single mid-size charter trip carries four figures of broker margin, so one quote saved from the follow-up graveyard pays for years of membership. Brokers serving UHNW clients are also buying discretion: a local book of clients and margins is worth more to them than a cheaper cloud one.

## 8. Candidates considered (scored 1–5; 20 max)

| Candidate | Pain frequency | Incumbent weakness | Local-WinUI feasibility | Willingness to pay | Total |
|---|---|---|---|---|---|
| A. YachtPilot — yacht management book | 5 | 4 | 5 | 5 | 19 |
| **B. CharterPilot — charter broker quote & trip book** | 4 | 4 | 5 | 5 | **18** |
| C. GalleryPilot — art gallery consignment & provenance book | 4 | 5 | 5 | 4 | 18 |
| D. EstatePilot — estate/house manager's book | 4 | 4 | 5 | 5 | 18 |
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

- **A (19):** Spec'd separately as this hunt's winner — Total Superyacht lists from $200/mo per vessel (https://sourceforge.net/software/marine/for-small-business/?page=2) and YachtOpera charges €120–€220/mo per vessel (https://yachtopera.com/).
- **C (18):** Spec'd separately. Artlogic Essential is $205/mo (https://www.softwareworld.co/software/artlogic-reviews/) — highest incumbent floor in the hunt; CharterPilot edges it on pain frequency (quotes die daily; consignments move monthly).
- **D (18):** Spec'd separately. Nines' 12-month pre-paid memberships plus onboarding fees (https://ninesliving.com/pricing) leave the solo estate manager unserved.
- **E (17):** Spec'd separately. Statutory clocks are real (12-month Coggins validity — https://animalofthings.com/eia-coggins-test-requirements-in-florida/) but barn incumbents start at $25–49/mo cloud tiers, so weakness scores 3.
- **Operator-side aviation (not scored as a candidate):** building *for operators* was rejected outright — flight ops is a live, regulated, networked layer (duty legality, real-time dispatch) that a local-first records app must not imitate. CharterPilot deliberately takes only the broker's records layer.

## 9. Build notes (for whoever builds it — not part of this spec task)

- Reuse the line's proven patterns: local JSON store; derived countdowns with green/orange/red states (short-cycle presets for quote follow-ups — hours, not days; the deadline engine must support hour-granularity clocks, a small extension of the DisputePilot pattern); status pipelines; proof-ledger notes; document export for the trip summary.
- No shared Licensing/voice/integrity code, consistent with the v40 apps — no new secrets needed.
- Ship 100 languages the same way as the rest of the line; all date/time math locale-safe (store ISO timestamps; follow-up clocks are timezone-sensitive — store the broker's local timezone with the record).
- Money in decimals with explicit currency codes (USD/EUR/GBP); margins computed, never typed, so the ledger can't disagree with itself.
- First-run sample book (two fictional clients, three quotes at different clock states, one booked trip with an incomplete file, clearly labeled sample) so a broker sees the follow-up board working before entering a real client.
- Hard rule for copy: never promise bookings or revenue ("win more trips"). Sell the follow-up discipline and the record — the things the evidence says brokers actually lose by lacking. Standing disclaimer: records and reminders only; flight operations and safety remain the operator's responsibility.
