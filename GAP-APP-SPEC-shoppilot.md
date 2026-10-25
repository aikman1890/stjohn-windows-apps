# GAP APP SPEC — ShopPilot

**Status:** Spec only (no build authorized yet)
**Date:** 2026-10-25
**Line:** The Night Shift SRE / St. John Windows apps (WinUI 3, local-first, Gumroad membership)
**Prepared for:** Shaun Nicholas St. John
**Industry:** Manufacturing — small machine shops & job shops (NAICS 332/333)

---

## 1. The app in one line

**ShopPilot** — a local-first Windows desktop app that runs a small job shop's quotes, jobs, and travelers: quote it, win it, schedule it on the board, print the traveler, cost it when it ships — one flat price, every seat in the shop included, on the shop's own computer.

## 2. The gap

The job-shop software market skips the bottom on purpose: enterprise ERP economics keep the smallest shops on paper, and the vendors' own pricing proves it.

**The incumbents are priced for a different business:**

- **JobBOSS²** (ECI, successor to both JobBOSS and E2 Shop System) publishes **no price — quote only**. Third-party estimates run **$85–$150/user/mo** (one aggregator puts the start near **$200/user/mo**) plus **$5,000–$40,000+ implementation** and a 3–6 month rollout (https://eziil.com/jobboss-vs-eziil-for-small-steel-fabricators/ and https://www.top10erp.org/products/jobboss²/pricing — figures are ESTIMATES from aggregators; the vendor does not publish).
- **ProShop ERP** is likewise quote-only; review-site figures conflict (SelectHub: from **$500/mo**; Zoftware: from **$7,800/yr** incl. support — https://www.selecthub.com/p/manufacturing-software/proshop-erp/ and https://zoftwarehub.com/products/proshop/zoftware-analysis), with implementation reported at $5,000–$50,000. Treat ~$500/mo as the ESTIMATE entry point for a small shop.
- The cheapest credible self-serve MRP, **MRPeasy**, is verified on the vendor's own site at **$49/user/mo** Starter / $69 Professional / $99 Enterprise (https://www.mrpeasy.com/blog/manufacturing-erp-systems/) — still per-user, still cloud.

**Per-user pricing is the mechanism of exclusion, and the trade press says so out loud:** shops "give logins to the office only and keep the floor on paper" (https://eziil.com/jobboss-vs-eziil-for-small-steel-fabricators/). The documented status quo at the small end is **paper travelers + whiteboard + Excel quoting** — the quote lives in a spreadsheet, the job lives on a clipboard, the schedule lives on a whiteboard, and actual-vs-quoted cost is never reconciled because no single system ever saw the whole job.

**The unserved layer is the quote-to-ship record for a 2–15-person shop**: quoting with win/loss history, a job card that becomes the printed traveler, a scheduling board the whole shop can see, and job costing when the parts go out the door — at a flat price that does not punish the owner for letting the floor see it. That work is office-desktop work; nothing about it needs a vendor cloud.

## 3. Target user

- **Primary:** Owner-operators and shop foremen of 2–15-person machine shops, fab shops, and job shops quoting in Excel and running paper travelers.
- **Secondary:** Slightly larger shops (15–30 staff) that bought an ERP tier for the office and still run the floor on paper — ShopPilot is the floor-visible layer.
- **Tertiary:** One-person shops quoting nights/weekends who need quote history more than they need an ERP.

## 4. Core features (MVP, all local-first)

1. **Quote log with win/loss** — quote number, customer, part description, material, quantity breaks, quoted price and quoted lead time, status (Open / Won / Lost / No-bid), loss reason. Win-rate and average-margin views by customer — the numbers a shop needs to stop quoting losers.
2. **Job card → traveler** — a won quote becomes a job: operations list (saw → mill → lathe → deburr → inspect → ship, user-defined operation library), material, quantities, due date, customer PO reference. Print a clean paper traveler with the operation checklist and sign-off boxes — paper on the floor is a feature, generated from the system instead of competing with it.
3. **Scheduling board** — jobs by machine/work-center and due date; drag-order priority per work center; overdue jobs flagged in the line's color language (green / orange ≤2 days / red overdue). A wall-monitor view (read-only board) is in scope — it runs on the same local data.
4. **Job costing** — per job: material cost, outside services (plating, heat treat — vendor + PO + cost), labor hours per operation (entered from the traveler sign-offs), machine time. Actual vs. quoted comparison on close-out, with the variance shown plainly. Money math in decimals throughout.
5. **Customer & part history** — every quote and job for a customer in one view; repeat-part lookup ("what did we charge for this bracket in March, and did we make money?") — the single most valuable screen in a job shop.
6. **Material & outside-service log** — material purchases tied to jobs (cert references noted as free-text fields — ShopPilot records that a cert exists and where it's filed; it is not a QMS), outside-service POs with promised/actual return dates and overdue flags.
7. **Shipping/close-out** — ship date, quantity shipped vs. ordered, invoice reference (InvoicePilot or the shop's accounting tool does the invoicing; ShopPilot records the reference), job closed with final margin.
8. **Dashboard** — open quotes aging, jobs due this week, overdue jobs, win rate (30/90 days), margin trend on closed jobs. The Monday-morning screen.

**Platform rules (same as the rest of the line):** local JSON storage, no network calls, no account, no telemetry, works fully offline. Flat membership — no per-seat concept exists in the product at all, and the listing copy says so.

## 5. What it deliberately does NOT do

- **No CAD/CAM, no G-code, no machine integration.** The machines already have their software; ShopPilot runs the business of the job, not the cut.
- **No full MRP/ERP** — no inventory optimization, no BOM explosion across assemblies, no accounting ledger, no payroll. Shops that genuinely need MRPeasy/JobBOSS² depth have outgrown this product and the spec says so.
- **No QMS/AS9100/ISO certification claims.** It records quality sign-offs and cert references; it does not certify anything and copy must never imply compliance certification.
- **No customer portal, no email integration, no cloud sync.** Quotes leave the shop however the shop sends them today; ShopPilot keeps the record.
- **Not a replacement for InvoicePilot** (invoicing) — ShopPilot ends at ship + invoice reference.

## 6. Nearest incumbents — and how ShopPilot differs

| Incumbent | What it is | Price | ShopPilot's difference |
|---|---|---|---|
| **JobBOSS²** (ECI) | Full job-shop ERP: quoting, scheduling, inventory, quality, accounting links | Quote-only; ~$85–200/user/mo ESTIMATE + $5,000–40,000+ implementation | Per-user cloud ERP with a 3–6 month rollout. ShopPilot is the quote→job→traveler→costing core at one flat $19.99/mo, running today, on the shop's own PC, with seats for the floor included by design. |
| **ProShop ERP** | Paperless ERP+MES+QMS for precision shops | Quote-only; ~$500/mo ESTIMATE entry | Built for shops pursuing formal QMS depth and paperless MES. ShopPilot prints the traveler instead of abolishing it and skips the QMS claim entirely — it's for the shop that has no system, not the shop upgrading one. |
| **MRPeasy** | Self-serve cloud MRP for 10–200 staff | $49/user/mo Starter (verified) | A real MRP — production planning, stock, purchasing — priced per user in a vendor cloud. ShopPilot has no MRP pretense: no stock optimization, no planning engine; just the job record, at a flat price, offline. |
| **Excel + paper traveler (DIY status quo)** | The actual incumbent at this size | Free + chaos | ShopPilot is that workflow finished: quote history with win/loss, a board instead of a whiteboard, and costing that finally answers "did we make money on that job?" |

## 7. Suggested Gumroad price

**$19.99/month membership** — top of the line's band, justified: a single saved bad quote or one recovered margin leak pays for years of membership, and the competitive set starts at $49/user/mo (verified) and runs to an estimated $500/mo. Flat, all seats — the anti-per-user price is the headline.

## 8. Candidates considered (scored 1–5; 20 max)

| Candidate | Pain frequency | Incumbent weakness | Local-WinUI feasibility | Willingness to pay | Total |
|---|---|---|---|---|---|
| **A. ShopPilot — job-shop quote/job/traveler/costing** | 5 | 5 | 5 | 4 | **19** |
| B. Full small-manufacturer MRP (inventory + planning) | 4 | 3 | 2 | 3 | 12 |
| C. Standalone shop scheduling board only | 4 | 3 | 5 | 2 | 14 |

Scoring notes:

- **A (19):** Highest score in the entire master sweep. Pain is daily (every quote, every job), incumbent weakness is structural (per-user pricing + five-figure implementations), the workflow is pure office-desktop data, and shop owners demonstrably pay for software when it fits — they are priced out of fit, not unwilling.
- **B (12):** Real MRP (stock, planning, purchasing) is where feasibility collapses for a local-first MVP and where MRPeasy already competes at $49/user/mo. Scope trap; explicitly out (§5).
- **C (14):** A board alone is the weakest third of the product — without quote history and costing it doesn't answer the money questions, and willingness to pay for a prettier whiteboard is low. It's a feature of A, not a product.

## 9. Build notes (for whoever builds it — not part of this spec task)

- Closest pattern relatives: ClientPilot (customer history views), TruckPilot (per-job profitability rollups, deadline colors), InvoicePilot (money handling — decimals, no floats).
- The printed traveler is a first-class deliverable: design the print layout early (operation rows, sign-off boxes, barcode-free — no scanner dependency).
- First-run sample shop (fictional "Bayou Fabrication" — labeled sample): 3 customers, 5 quotes (2 won, 1 lost), 2 jobs mid-route, 1 closed job with a margin variance, so the costing screen teaches itself.
- Hard rule for copy: lead with "every seat in the shop, one flat price" and "your jobs never leave your computer." Never promise ERP/MRP/QMS capability.
