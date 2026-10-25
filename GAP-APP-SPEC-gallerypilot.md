# GAP APP SPEC — GalleryPilot

**Status:** Spec only (no build authorized yet)
**Date:** 2026-10-25
**Line:** The Night Shift SRE / St. John Windows apps (WinUI 3, local-first, Gumroad membership)
**Prepared for:** Shaun Nicholas St. John
**Hunt:** Gap hunt #4 — the millionaire industries (businesses that serve high-net-worth clients)

---

## 1. The app in one line

**GalleryPilot** — a local-first Windows desktop app that runs a small art gallery's real book: artwork inventory with provenance and condition records, the consignment ledger with artist payables, exhibition and loan clocks, and the collector list — the records core of gallery software, without the $200-a-month cloud suite wrapped around it.

## 2. The gap

Gallery software is the most expensive category in this hunt relative to the size of the businesses buying it — and most of what the suites sell is a website the small gallery didn't ask for.

**The incumbent floor is enterprise-priced:**

- **Artlogic**, the category standard, is listed at **$205/month (Essential) and $409/month (Professional)** (https://www.softwareworld.co/software/artlogic-reviews/); GetApp lists its starting price at **$130/month, usage-based** (https://www.getapp.com/industries-software/a/artlogic/). What that buys is an integrated platform: database + website + online viewing rooms + marketing email + sales app.
- **ArtBinder**, the other established name, starts at **$119/month** (https://sourceforge.net/software/art-gallery/usa/?sort=featured&feature_art-gallery=Online+Gallery).
- A one-to-three-person gallery — the overwhelming majority of galleries — needs perhaps a fifth of that surface: what it cannot afford to get wrong is the *records*: which works it holds, whose they are, what condition they arrived in, what they're consigned for and until when, what sold, and what it now owes the artist.

**The consignment ledger is the pain with teeth:**

- Galleries hold other people's property. A consigned work has an owner (the artist or a collector), an agreed price, a split, and a term. When the term lapses unnoticed, the gallery is storing — and insuring — work it no longer has the right to sell. When a work sells, the artist's share becomes a payable with its own clock; late or wrong artist payments are the classic small-gallery reputation killer in a business that runs entirely on artist relationships.
- Provenance and condition are the other records that cannot be reconstructed after the fact: a condition report not written at intake becomes an argument at return; a provenance gap discovered at sale time kills the sale. These records are created once, at a desk, and kept for years — the exact shape of a local-first desktop book.
- Discretion is structural in this market: collector identities and prices are the gallery's crown jewels, and the art world has repeatedly watched cloud databases become gossip. A book that never leaves the gallery's machine is a feature collectors' galleries can say out loud.

**The unserved layer is the records core — inventory + provenance/condition + consignment clocks + artist payables — at a flat desktop price.** The suites bundle it with websites and viewing rooms and price the bundle; nobody sells the core alone, locally, to the small gallery.

## 3. Target user

- **Primary:** Small and mid-size galleries (1–5 staff) holding consigned inventory from dozens of artists, currently splitting records between a spreadsheet, a shared drive of condition photos/reports, and memory.
- **Secondary:** Art advisors and private dealers who hold consigned works and owe artists/collectors clean records without a gallery storefront.
- **Tertiary:** Artist estates and studio managers tracking works consigned *out* to galleries (the mirror image of the same ledger: what's out, where, until when, what's owed back).

## 4. Core features (MVP, all local-first)

1. **Artwork inventory** — one record per work: artist, title, year, medium, dimensions, edition, location (gallery / storage / on loan / with client), asking price, status (available, reserved, sold, returned). Image attachments stored as local file references.
2. **Provenance & condition record** — per work: ownership history entries, exhibition/publication history, and a condition log (dated entries at intake, before loan, at return — with photo references). The record is append-only in spirit: entries are dated and never silently rewritten, mirroring the proof-ledger pattern.
3. **Consignment ledger with clocks** — per consignment: consignor (artist/collector), works covered, agreed price and split %, start date, **term end date with the line's countdown language (green / orange ≤30 days / red expired)**, renewal/return status. The dashboard shows consignments expiring before anyone has decided what happens next.
4. **Artist payable ledger** — when a work sells: sale price, split computed (decimal math, explicit currency), amount owed to the artist, payment due date (user-set terms), paid state. A gallery sees, on one screen, every artist it currently owes money to and how long they've been owed it.
5. **Exhibition & loan book** — exhibitions (dates, works included, install/deinstall clocks) and outgoing loans (borrower, venue, loan period, return-due clock, condition-out/in pair required before the loan can be marked closed).
6. **Collector book** — collectors and contacts: interests, works bought, works offered, viewing history notes. Local only — this list is the business.
7. **Sales ledger & exports** — sales by period/artist/collector; consignment statements per artist (what sold, what's owed, what's still held) exported to PDF/print — the document that ends "what do you owe me?" phone calls.
8. **Dashboard** — consignment terms expiring, artist payables outstanding, loans due back, works without a condition entry at their current location. Sorted by clock pressure, like the rest of the line.

**Platform rules (same as the rest of the line):** local JSON storage, no network calls, no account, no telemetry, works fully offline.

## 5. What it deliberately does NOT do

- **No website, no online viewing rooms, no e-commerce.** That is the suites' bundle and the reason for their price; GalleryPilot's buyer needs the book, not a second website.
- **No marketing email or CRM campaigns.** The collector book holds records and notes; outreach stays in the gallery's email where it belongs.
- **No valuation or appraisal opinions, and no authentication claims.** It records provenance as entered by the gallery; it does not certify authenticity or value, and screens say so (records tool, not an appraisal service — staying clear of regulated-advice territory, per the hunt's rules).
- **No payments or accounting integration.** Sales and payables are recorded and totaled; money movement and the gallery's books stay with its accountant.
- **Not VenuePilot** (event venues' date book) **and not a general inventory app** — consignment terms, artist splits, and condition/provenance records are gallery-specific structures, and they are the product.

## 6. Nearest incumbents — and how GalleryPilot differs

| Incumbent | What it is | Price | GalleryPilot's difference |
|---|---|---|---|
| **Artlogic** (artlogic.net) | Cloud gallery platform: database, website, viewing rooms, marketing, sales app | $205/mo Essential, $409/mo Professional (SoftwareWorld); from $130/mo usage-based (GetApp) | A platform rented forever, priced on the whole bundle. GalleryPilot is the records core only — inventory, consignment clocks, artist payables — flat-priced, on the gallery's machine, with collector data that never sits in a vendor cloud. |
| **ArtBinder** (artbinder.com) | Cloud inventory/presentation tool for galleries and advisors | From $119/mo (SourceForge) | Presentation-first and cloud-only. GalleryPilot is ledger-first: consignment terms that turn red, artist payables with due clocks, condition records tied to loans — the back office, not the viewing room. |
| **The spreadsheet + shared drive (DIY status quo)** | Inventory in a sheet, condition photos in a folder, consignment terms in email, artist payables in the owner's head | Free + one expired consignment or one late artist payment away from a burned relationship | This is the real incumbent for the small gallery. GalleryPilot is that stack, finished: expiring terms computed, payables aged on a dashboard, condition entries chained to loans, and artist statements generated instead of assembled. |

## 7. Suggested Gumroad price

**$29.99/month membership** — premium for the line, a fraction of the category. Rationale: the buyer's reference price is $119–$409/month for suites whose differentiating features (websites, viewing rooms) the small gallery demonstrably doesn't use; $29.99 for the records core is an easy yes against a single avoided incident — one consignment term caught before lapse, one artist paid on time with a clean statement. Galleries serving wealthy collectors will also pay for the discretion story: their prices and collector names never leave the building.

## 8. Candidates considered (scored 1–5; 20 max)

| Candidate | Pain frequency | Incumbent weakness | Local-WinUI feasibility | Willingness to pay | Total |
|---|---|---|---|---|---|
| A. YachtPilot — yacht management book | 5 | 4 | 5 | 5 | 19 |
| B. CharterPilot — charter broker quote & trip book | 4 | 4 | 5 | 5 | 18 |
| **C. GalleryPilot — art gallery consignment & provenance book** | 4 | 5 | 5 | 4 | **18** |
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

- **A (19):** Spec'd separately as the hunt winner — per-vessel cloud pricing (Total Superyacht from $200/mo; YachtOpera €120–€220/mo per vessel, https://yachtopera.com/) against a spreadsheet status quo the vendors' own testimonials describe.
- **B (18):** Spec'd separately. Leon's broker tier is €99/user/mo (https://www.leonsoftware.com/pricing.html); GalleryPilot ties it on total and beats it on incumbent weakness (5 — the $205–$409/mo floor is the hunt's highest), losing only on pain frequency (quotes die daily; consignments move monthly).
- **D (18):** Spec'd separately. Nines requires 12-month pre-paid memberships with onboarding fees (https://ninesliving.com/pricing) — commitment, not just price, is the barrier.
- **H (14):** The watch/jewelry dealer's consignment-and-repair book is GalleryPilot's pattern in a smaller market; noted as a possible later CONFIG-style sibling, not a separate build.

## 9. Build notes (for whoever builds it — not part of this spec task)

- Reuse the line's proven patterns: local JSON store; derived countdowns with green/orange/red states (30-day orange band for consignment terms and loan returns); status pipelines; append-only ledger entries; PDF/print export for artist consignment statements.
- No shared Licensing/voice/integrity code, consistent with the v40 apps — no new secrets needed.
- Ship 100 languages the same way as the rest of the line; all date math locale-safe (store ISO dates; render localized). Money in decimals with explicit currency codes (USD/EUR/GBP/CHF — this market is international); splits computed from stored percentages, never typed totals.
- Image handling is references-only (local file paths), not an embedded media store — keeps the data file lean and matches the line's document-vault pattern.
- First-run sample gallery (fictional artists and works, one consignment inside its orange window, one unpaid artist payable, clearly labeled sample) so a buyer sees the clocks and ledgers working before entering a real record.
- Hard rule for copy: never promise sales, valuations, or authentication. Sell the records: terms that can't lapse silently, artists paid on time, condition documented before it becomes an argument. Standing disclaimer on relevant screens: records tool only — not an appraisal, authentication, or valuation service.
