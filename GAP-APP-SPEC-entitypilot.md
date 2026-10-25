# GAP APP SPEC — EntityPilot

**Status:** Spec only (no build authorized yet)
**Date:** 2026-10-25
**Line:** The Night Shift SRE / St. John Windows apps (WinUI 3, local-first, Gumroad membership)
**Prepared for:** Shaun Nicholas St. John
**Industry:** Management of Companies & Enterprises — small holding structures (NAICS 55)

---

## 1. The app in one line

**EntityPilot** — a local-first Windows desktop app that keeps a small multi-company owner's whole structure straight: every LLC and corp, its state filings and deadlines, its registered agent, its owners, and the loans between them — the register that today lives in a spreadsheet and a pile of state-portal passwords, because accounting software charges per company and entity tools charge per entity.

## 2. The gap

NAICS 55 has no small-operator tool at all. Owners of 2–10 LLCs — real-estate holders (before the landlord drop: this is the *entity* layer, not rent collection), serial founders, family holding structures, side-business collectors — run one accounting subscription **per company** plus a spreadsheet for everything that makes a company legally exist.

**The pricing shape, verified in the sweep:**

- **QuickBooks Online:** ~**$38/mo** (Simple Start) to ~**$100–275/mo per company** — the per-entity multiplication is the product's defining cost (https://blog.send.win/accounting-software-to-manage-multiple-companies-multi-account-management-guide-2026/ and https://agiled.app/blog/best-invoicing-software-for-bookkeepers).
- True multi-entity consolidation starts at **Gravity, from $375/mo** (https://www.selecthub.com/accounting-software/quickbooks-enterprise-vs-gravity-software/) — firm-grade money for a portfolio an individual owns.
- The sweep's sector finding: *"No dedicated small-operator tool — owners of 2–10 LLCs/subsidiaries run one QuickBooks Online subscription per entity plus spreadsheets for entity deadlines, registered-agent renewals, intercompany loans and ownership,"* and the gap verdict is **YES**: *"entity register, annual-report/franchise-tax deadline calendar, officer/member and intercompany-loan ledger is pure local data with zero network need, and per-entity cloud pricing punishes exactly this buyer."*

**The pain is deadline-shaped and unforgiving.** Every entity carries its own calendar: state annual reports / franchise-tax filings (Texas franchise tax, Wyoming annual report, Delaware franchise tax — each state its own month, its own portal, its own penalty), registered-agent renewals, assumed-name renewals, EIN records. Miss one and the state administratively dissolves the company — the owner finds out when a bank, a title company, or a court filing bounces. This is not a hypothetical buyer: **Shaun himself owns this problem** — BCCS involuntarily ended over franchise-tax filings, SNSJ LLC administratively dissolved in Wyoming over an unfiled annual report (both in his record, October 2026). The target user is Shaun, multiplied by every small operator with a drawer of entities.

**The unserved layer is the register itself**: what companies do I own, in what states, in what standing (as last confirmed), what is due when, who is the agent, who owns what percent, and which company owes which company money. Pure local data — arguably the *most* local data a business has — that no vendor sells to small holders without per-entity rent.

## 3. Target user

- **Primary:** Individuals and couples holding 2–10 entities (LLCs, corps, LPs) across one or more states — founders with an operating company plus holding/real-estate entities, serial side-business owners.
- **Secondary:** Small family offices / family administrators keeping the structure for relatives' entities.
- **Tertiary:** Bookkeepers and registered-agent-adjacent solo practitioners who maintain deadline registers for several small clients (per-client local files).

## 4. Core features (MVP, all local-first)

1. **Entity register** — per entity: legal name, type (LLC / S-corp / C-corp / LP / PLLC — extensible), state of formation, formation date, state entity/file number, EIN (**stored locally, masked by default, shown on deliberate reveal** — the value never leaves the machine and is never required), fiscal year end, status field (Active / Inactive / Dissolved / Unknown) **with an "as confirmed on" date** — the app records what the owner last verified, never claims live state data.
2. **Deadline engine (the core)** — per entity, recurring obligations as user-built rules: annual report, franchise tax, registered-agent renewal, assumed-name/DBA renewal, plus one-off dates. Ship **starter rule templates for common states (TX, WY, DE, FL, CA, NV…) clearly labeled as starting points the owner must confirm against their state portal** — the TruckPilot/TaxPilot as-of posture. Line's color language (green / orange ≤30 days configurable / red overdue), and a **consolidated calendar across all entities** — the single screen that answers "what is due in the next 90 days, everywhere?"
3. **Registered-agent book** — per entity: agent name, agent address, agent fee/renewal date, portal login *reference* (site + username note — **passwords are never stored**; the field is a pointer, e.g., "password in vault"), correspondence log (date, what arrived, action taken).
4. **Ownership register** — members/shareholders per entity with percentages (validated to sum to 100% per class where applicable), capital contributions, dated changes (admissions, transfers) as a versioned chain — the WellPilot deck discipline applied to cap tables. Officers/managers/directors per entity with appointment dates.
5. **Intercompany ledger** — loans and transfers between the owner's entities: date, from, to, amount, terms note, balance per pair, repayment entries. The spreadsheet page that always goes stale first, made structural. (Records only — no interest computation authority; user-entered terms, simple balance math in decimals.)
6. **Document index** — per entity: formation docs, operating agreement/bylaws, EIN letter, annual filings — recorded as local file paths/references with received/filed dates. EntityPilot indexes the owner's own files; it does not fetch anything from any state.
7. **Standing checklist** — per entity, a "what good standing needs" checklist view: filings current?, agent current?, taxes current? — each answered from the owner's own records with as-of dates, feeding a portfolio health view (entities by status, entities with anything overdue at top).
8. **Portfolio dashboard** — entity count by state/type/status, next 5 deadlines across everything, entities with overdue items, total intercompany balances. The "is anything about to dissolve?" screen — the one Shaun needed twice.

**Platform rules (same as the rest of the line):** local JSON storage, no network calls, no account, no telemetry, works fully offline. Sensitive fields (EIN) masked by default; the app's privacy posture — your whole structure on your machine only — is a headline feature.

## 5. What it deliberately does NOT do

- **No state filing, no portal automation, no live status lookups.** EntityPilot tracks deadlines and records confirmations the owner enters; it never touches a state system and never represents that an entity *is* in good standing — only when the owner last confirmed it.
- **No registered-agent service.** It manages the owner's record of their agent; it is not an agent.
- **No accounting.** Entity books stay in the owner's accounting tools; EntityPilot exports nothing to them in MVP beyond printable summaries. (It replaces the spreadsheet register, not QuickBooks.)
- **No legal/tax advice and no deadline authority.** State rule templates are labeled starting points; screens instruct owners to confirm against their state's official portal. Formation/dissolution decisions are the owner's and their counsel's.
- **No document generation for filings in MVP** — it indexes documents and tracks dates; state forms remain the state's.

## 6. Nearest incumbents — and how EntityPilot differs

| Incumbent | What it is | Price | EntityPilot's difference |
|---|---|---|---|
| **QuickBooks Online (×N entities)** | Per-company accounting subscriptions | ~$38–275/mo **per company** | QBO does books, not existence: no annual-report calendar, no agent book, no ownership register — and charges again for every entity. EntityPilot is the missing register at $9.99/mo flat for *all* entities, deliberately not an accounting tool. |
| **Gravity / multi-entity ERP** | Consolidated accounting for multi-entity companies | From $375/mo | Firm-grade consolidation for staff accountants. The small holder doesn't need consolidation — they need to not get dissolved. Different job, 1/37th the price. |
| **Registered-agent / compliance services** (per-entity annual fees + upsells) | Agent service bundled with compliance reminders | Per entity, per year (varies; agent fees plus compliance add-ons) | They remind you about *their* entities on *their* portal, one login per service, and sell the fix when you lapse. EntityPilot covers every entity regardless of agent, on the owner's machine, with the intercompany and ownership layers no agent service touches. |
| **Spreadsheet + portal passwords (DIY status quo)** | The sweep-verified reality | Free + dissolution risk | EntityPilot is that spreadsheet finished: deadlines that flag themselves across the whole portfolio, ownership that validates its own sums, and an as-of discipline that turns "I think Wyoming is fine" into a dated fact. |

## 7. Suggested Gumroad price

**$9.99/month membership** — flat, unlimited entities. The comparison writes itself: less than one month of *one* extra QBO subscription, against a portfolio-wide deadline register. Priced at the band's lower end because the buyer is an individual, not a firm — and because the retention is structural (nobody deletes the register their companies live in).

## 8. Candidates considered (scored 1–5; 20 max)

| Candidate | Pain frequency | Incumbent weakness | Local-WinUI feasibility | Willingness to pay | Total |
|---|---|---|---|---|---|
| **A. EntityPilot — multi-entity register + deadlines** | 4 | 5 | 5 | 3 | **17** |
| B. Multi-entity consolidated accounting | 3 | 3 | 1 | 3 | 10 |
| C. Registered-agent correspondence tracker only | 2 | 3 | 5 | 2 | 12 |

Scoring notes:

- **A (17):** Incumbent weakness is total — the category has *no* small-operator product, only per-entity multiplication of adjacent ones. Pain is episodic but catastrophic (administrative dissolution), feasibility is maximal (registers + dates), willingness to pay is the modest score (individual buyers) — priced for at $9.99. Shaun is the living proof-of-pain (two entities, two states, two lapses).
- **B (10):** Consolidation is real accounting (eliminations, FX, periods) — infeasible local-first at this scope and Gravity's territory besides.
- **C (12):** Agent correspondence alone is a feature of A, not a product — nobody pays monthly for a mail log.

## 9. Build notes (for whoever builds it — not part of this spec task)

- Closest pattern relatives: CasePilot (per-matter register with dated status + as-of discipline), TaxPilot (user-confirmed deadline rule tables), WellPilot (versioned ownership chains, sum validation).
- The state starter templates need an editorial pass listing each template's source and "last reviewed" date in-app; build the template format so new states are data files, not code.
- EIN handling: masked display, reveal-on-click, never in exports by default (export checkbox, default off), never in sample data (samples use 00-0000000-style obvious fakes).
- First-run sample (fictional, labeled): "The St. John Portfolio (sample)" — 4 entities across TX/WY/DE: one with an annual report due in 9 days (orange), one overdue agent renewal (red), one intercompany loan with a balance, ownership split across two members — every dashboard element exercised. Use fictional entity names in shipped samples, not Shaun's real companies.
- Hard rule for copy: sell the save, factually — "One missed annual report can dissolve a company. This is the calendar that doesn't miss." Never claim to file, verify status, or provide agent services.
