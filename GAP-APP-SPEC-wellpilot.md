# GAP APP SPEC — WellPilot

**Status:** Spec only (no build authorized yet)
**Date:** 2026-10-25
**Line:** The Night Shift SRE / St. John Windows apps (WinUI 3, local-first, Gumroad membership)
**Prepared for:** Shaun Nicholas St. John
**Industry:** Mining, Quarrying, Oil & Gas Extraction — micro-operators (NAICS 211)

---

## 1. The app in one line

**WellPilot** — a local-first Windows desktop app that does the monthly oil & gas back office for operators too small for the industry suites: joint-interest billing decks, revenue distribution, division orders, and well files — the records a 1–30-well operator currently pays an estimated $200–$1,200 a month to rent, or builds in spreadsheets that break every month-end.

## 2. The gap

Small-operator oil & gas accounting is a specialty discipline with specialty pricing, and the smallest operators are squeezed between a general ledger that can't do the job and industry suites priced for companies ten times their size.

**The wall everyone hits first:** QuickBooks cannot do the industry's core work — **joint interest billing (JIB), revenue distribution, and division orders** — so micro-operators run QuickBooks *plus* auxiliary spreadsheets until the spreadsheet layer collapses, then face the industry suites (https://rocketdigit.com/oil-and-gas-accounting-software-for-small-business/).

**The suites are priced for a different operator (all figures ESTIMATE — none of these vendors publish pricing):**

- **Roughneck Accounting** (aimed at 1–30 wells): "typically under $200 to $300 per month" per third-party roundup — ESTIMATE, vendor does not publish (same source).
- **OGsys**: **$500–$800/mo ESTIMATE**.
- **PakEnergy** (ex-WolfePak): **$400–$1,200+/mo ESTIMATE**.

Even the cheapest rung is **10–15× the house price band**, for software whose core monthly jobs — allocate this month's expenses across working-interest owners, distribute this month's revenue across royalty and working interests, keep the division-order deck straight — are deterministic arithmetic on local records. The data is also among the most sensitive a small business holds: who owns what share of which well, what each well makes, what each partner owes. It sits, today, in a vendor's cloud or in a fragile spreadsheet.

**The unserved layer is the micro-operator's monthly close**: well files, interest decks, JIB statements, revenue distribution, and the audit trail behind them — offline, on the operator's own machine, at a flat price. Texas is full of exactly this buyer: stripper-well operators, family-held working interests, one-person operating companies running a handful of leases out of a home office. (Shaun's own career is Texas oil & gas IT — this is home turf, and the copy can say so honestly.)

## 3. Target user

- **Primary:** Operators of 1–30 wells (stripper wells, small conventional fields, family operating companies) doing JIB and revenue distribution in spreadsheets or paying suite prices that dwarf their margins.
- **Secondary:** Non-operating working-interest owners who receive JIB statements and revenue checks from multiple operators and need their own verification ledger ("did the operator bill my share correctly?").
- **Tertiary:** Small bookkeepers/accountants who run the monthly close for several micro-operators and need a per-operator local file.

## 4. Core features (MVP, all local-first)

1. **Well & lease file** — well/lease record: API number (as free text), lease name, county/state, operator, status (producing / shut-in / plugged), first production date, notes. Wells group into leases and properties the way the operator thinks about them.
2. **Interest decks (division orders)** — per well/property: owner list with interest type (royalty, overriding royalty, working interest) and decimal interest (8-decimal precision, decimal math — never floats). Deck validation: interests must sum to 1.00000000 per type class, with a plain-language warning when they don't. Deck changes are dated and versioned — last month's distribution must always be reproducible from last month's deck.
3. **Production & revenue entry** — monthly per well: volumes (bbl / MCF, user-entered from purchaser statements), gross revenue, purchaser reference, severance tax and deductions as entered line items. WellPilot records what the statements say; it does not fetch prices or statements.
4. **Expense & JIB engine** — monthly operating expenses per well/property (LOE categories: labor, chemicals, repairs, hauling, utilities, contract services, other) allocated across working-interest owners by deck decimals → **JIB statements per owner**: their share, what they owe, cumulative balance. Print/PDF per owner.
5. **Revenue distribution engine** — net revenue distributed across royalty/ORRI/WI owners by deck → **distribution statements/check register per owner**, with suspense tracking for amounts held (unleased/minimum-royalty/suspense reasons as user-entered notes) and a suspense balance per owner that never silently disappears.
6. **Deadline & compliance calendar** — user-configured recurring dates with the line's color language (green / orange ≤5 days / red overdue): severance tax filings, purchaser statement reconciliation, state production reports, lease obligations the user enters. All dates and rates are user-maintained data with as-of labels (same posture as TruckPilot's rate table).
7. **Owner ledger** — every owner across every property: total JIB billed, total revenue distributed, current balance, suspense held. The "who do we owe / who owes us" screen.
8. **Export packet** — per month, per property: production, revenue, expenses, decks used, statements produced — a printable/PDF close packet that reproduces the month exactly. This is the audit artifact.

**Platform rules (same as the rest of the line):** local JSON storage, no network calls, no account, no telemetry, works fully offline. Rate/tax tables are user-confirmed local data (manual import allowed — see §5 note); the app shows its math on every statement.

## 5. What it deliberately does NOT do

- **No general ledger / double-entry accounting.** QuickBooks (or the operator's books) remains the ledger; WellPilot does the industry layer QuickBooks can't, and exports totals. This boundary is the product's reason to exist — never blur it in copy.
- **No e-filing, no state/purchaser portal integration, no price feeds.** Statements come from purchasers; filings go to the state; WellPilot computes and documents between those two facts.
- **No tax advice and no shipped tax-rate authority.** Severance rates are user-entered with as-of dates; screens state figures must be verified against current state requirements. General information and record tools only.
- **No AFE/project economics, no decline-curve engineering, no land/lease acquisition management** in MVP — those are different (and pricier) products; the monthly close is the wedge.
- **No multi-user/network mode.** One operator's books on one machine; per-operator data files for the bookkeeper use case.
- **Rate-table updates:** because "no network ever" cannot ship current severance tables, tables are editable local data with a visible as-of label — the TruckPilot pattern, stated plainly in the listing so no buyer is surprised.

## 6. Nearest incumbents — and how WellPilot differs

| Incumbent | What it is | Price | WellPilot's difference |
|---|---|---|---|
| **Roughneck Accounting** | Small-operator O&G accounting (1–30 wells class) | ~$200–300/mo ESTIMATE (unpublished) | Closest in spirit — and still an estimated 10–15× WellPilot's price, as a rented suite. WellPilot covers the monthly JIB/revenue/deck core only, at $19.99/mo flat, fully offline, with the operator's ownership data never leaving their office. |
| **OGsys / PakEnergy** | Full industry accounting suites (land, production, financials) | $400–1,200+/mo ESTIMATE | Suites for operators with back-office staff. WellPilot deliberately does less: no land module, no GL — the monthly close for the operator who *is* the back office. |
| **QuickBooks + spreadsheets (DIY status quo)** | GL that can't do JIB + hand-built allocation spreadsheets | ~$38–100/mo + monthly pain | The spreadsheet layer is where errors compound silently (wrong deck decimal, stale owner, broken formula). WellPilot is that layer, finished: validated decks, versioned changes, reproducible months, statements that show their math. |

## 7. Suggested Gumroad price

**$19.99/month membership** — top of band. The cheapest industry alternative is an estimated $200/mo; a single month of correct JIB on one decent well exceeds a year of membership. The audience is small but pays industry prices without blinking when the tool fits — fit, at a tenth of the floor, is the pitch.

## 8. Candidates considered (scored 1–5; 20 max)

| Candidate | Pain frequency | Incumbent weakness | Local-WinUI feasibility | Willingness to pay | Total |
|---|---|---|---|---|---|
| **A. WellPilot — micro-operator JIB + revenue distribution** | 4 | 5 | 4 | 5 | **18** |
| B. Small quarry/mine scale-ticket + billing tracker | 3 | 3 | 4 | 3 | 13 |
| C. Land/lease obligation tracker only | 3 | 3 | 4 | 3 | 13 |

Scoring notes:

- **A (18):** Pain is monthly and mandatory (you cannot skip JIB or distribution), incumbent weakness is a 10–60× price multiple, feasibility is high (deterministic allocation math + statements — the feasibility point off is for deck-versioning care), and willingness to pay is the highest of any candidate in the sweep because the industry is habituated to specialty pricing.
- **B (13):** Quarries run QuickBooks + scale-ticket spreadsheets (sweep evidence) — real but thinner pain, and ticketing leans toward scale hardware integration. Possible fast follower.
- **C (13):** Lease obligations alone are a calendar problem already half-served by EntityPilot-style deadline tooling; as a standalone it lacks the monthly arithmetic engine that makes A indispensable.

## 9. Build notes (for whoever builds it — not part of this spec task)

- Money and interest math: **decimals everywhere, 8 places for interests, round-half-even on distributions with a documented remainder rule** (largest-interest owner absorbs rounding remainder — state it in the app and in copy; accountants will look for exactly this).
- Deck versioning is the correctness core: distributions reference an immutable deck snapshot. Build this before any UI polish.
- Closest pattern relatives: TruckPilot (monthly/quarterly engines that show their math, as-of-labeled rate tables), DenialPilot (suspense/ledger discipline), InvoicePilot (statement generation).
- First-run sample (fictional, labeled): "St. John Operating Co." — 3 wells, 6 owners across RI/WI, one month of production, one expense set, one distribution run, so a buyer sees a full close in five minutes. (Use a clearly fictional operator name in shipped sample data — not Shaun's real entities.)
- Hard rule for copy: sell accuracy and ownership of data ("your decks, your machine"). Never promise regulatory compliance; promise reproducible, documented math.
