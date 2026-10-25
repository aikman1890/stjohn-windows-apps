# GAP APP SPEC — PlotPilot

**Status:** Spec only (no build authorized yet)
**Date:** 2026-10-25
**Line:** The Night Shift SRE / St. John Windows apps (WinUI 3, local-first, Gumroad membership)
**Prepared for:** Shaun Nicholas St. John
**Industry:** Other Services / Public Administration — small cemeteries (NAICS 812220 / 92)

---

## 1. The app in one line

**PlotPilot** — a local-first Windows desktop app that keeps a small cemetery's records straight: who is buried where, who owns which plot, what the paper map says versus what the notebook says — reconciled into one searchable record on the cemetery office's own computer, for the cemeteries still running on notebooks and a plat map in a drawer.

## 2. The gap

Cemetery records are among the last paper systems in American local life — and the incumbents start at prices a church cemetery or a two-person municipal cemetery office cannot carry.

**The status quo is documented by the vendors themselves:**

- A Cemify review from a small cemetery: they started *"with just an old map and several old notebooks of ownership and Death records"* (https://www.capterra.co.uk/software/175752/cemify).
- CemeteryBase, a church-cemetery product, sells explicitly to this condition: *"migrate paper or spreadsheet records… if your records are in paper logbooks"* and *"upload your paper map"* (https://cemeterybase.com/cemetery-software-for-church-cemeteries).
- The municipal township of Arran-Elderslie (6 cemeteries) ran on *"paper maps and an Excel-based database"* until a 2023–24 digitization project converted paper maps to GIS (https://www.cimscemeterysoftware.com/case-study/case-study-arran-elderslie/).

**The price floor above that paper:**

- **Cemify:** **$599/yr** flat (Preservation tier: 3 users, 2,000 plots); Management $999/yr — the cheapest verified self-serve floor (Capterra listing, same URL as above).
- **CemeteryBase:** $99/mo / $899/yr (church-cemetery positioning, same URL as above).
- **CemeteryPro:** $500 setup + $125/mo, cited in a 2026 build-vs-buy guide (https://ideausher.com/blog/build-funeral-home-software-cost/).
- **CemSites / PlotBox:** no published pricing — quote/demo only (https://sourceforge.net/software/compare/CemSites-vs-Cemify-vs-Chronicle-Cemetery/ and https://www.getapp.za.com/software/113883/plotbox).

**Why local-first fits this buyer uniquely well:** cemetery records are *permanent* (a 150-year obligation), *local* (one office, one groundskeeper, walk-in families), *sensitive in a quiet way* (deeds, burial rights, family disputes over plots), and *never* need a network — nobody needs a cloud-synced burial record at 2 a.m.; they need to answer "is there room in the Henderson family plot?" while a family stands at the counter. The sweep rated death-care the strongest slice of NAICS 81 and flagged the single-clerk record jobs of tiny municipalities (NAICS 92) as the same shape: offices *"priced out of Tyler by 3 orders of magnitude"* doing inherently local work. A permanent record on a vendor's subscription cloud is also a genuine mismatch — when the subscription lapses, the map should not. Local files the cemetery owns outright answer that.

## 3. Target user

- **Primary:** Small cemeteries — church cemeteries, family/association cemeteries, small municipal cemeteries (roughly under 5,000 plots) — run by a sexton, a clerk, or a volunteer board member with notebooks and a plat map.
- **Secondary:** Small-town/village clerks (1–3-person offices) who keep the cemetery book alongside permits and licences.
- **Tertiary:** Genealogy-minded cemetery associations digitizing records before the last person who can read the old map retires — the urgency is real and the copy may say so.

## 4. Core features (MVP, all local-first)

1. **Plot register** — section / block / lot / space addressing (user-defined scheme — every cemetery's is different), per space: status (Available / Sold / Reserved / Occupied / Unknown-from-old-records), owner of record, deed reference, purchase date and price as recorded. Unknown is a first-class status — old records have gaps and the app must represent them honestly instead of forcing false precision.
2. **Burial records** — per interment: decedent name, dates, funeral home, interment date, space occupied, marker notes, veteran flag (user-entered), record source (which notebook/page the entry came from — provenance is a feature, see feature 6).
3. **Ownership & deed ledger** — plot sales, transfers, and inheritance notes as a dated chain per space; who may authorize a burial in this space is answerable from the record. Perpetual-care fund contributions recorded per sale (amount as entered; the fund's accounting stays with the treasurer — PlotPilot keeps the record, not the books).
4. **Visual plot map** — a grid map per section built from the register (rows × spaces), colored by status, click a space → its full record. Not GIS, not surveyed coordinates: a faithful digital version of the paper plat, which is exactly what the office uses. Print a section map for the grounds.
5. **Search that answers counter questions** — find by decedent name, owner name, or space address in seconds; "available spaces near X" view (same section/block, status Available) for families choosing at the counter.
6. **Provenance & reconciliation** — every record carries its source (notebook/page, deed book, plat annotation, staff entry + date); a reconciliation view lists conflicts (space shown occupied on the plat but no burial record; deed holder differs from notebook) for the board to work through deliberately instead of discovering during a burial.
7. **Work log** — openings/closings scheduled, marker settings, maintenance notes per section — the sexton's dated log, tied to spaces.
8. **Reports & export** — burials by year, available inventory by section, sales by year; full data export (CSV/PDF) the cemetery owns — printed archival copy support is a stated design goal (paper backup of the digital record is a feature in this market, not irony).

**Platform rules (same as the rest of the line):** local JSON storage, no network calls, no account, no telemetry, works fully offline. **Data permanence posture:** the export is plain and complete — the cemetery's records must outlive any vendor, including this one; the listing says so.

## 5. What it deliberately does NOT do

- **No GIS/survey-grade mapping, no GPS grave-finding, no public-facing "find a grave" website.** The map is the office plat, digitized. Public genealogy portals are a different (network) product.
- **No funeral-home case management** — funeral homes are hunt #3's territory and a different buyer; PlotPilot serves the cemetery side only, recording the funeral home's name per interment.
- **No accounting beyond the record** — perpetual-care and general funds are recorded per transaction; the books stay with the treasurer/accountant.
- **No legal determination of burial rights.** PlotPilot shows the recorded chain of ownership and flags conflicts; disputed rights are for the cemetery board and its counsel. Copy must never imply the app settles ownership questions.
- **No headstone/marker sales or monument ordering.**

## 6. Nearest incumbents — and how PlotPilot differs

| Incumbent | What it is | Price | PlotPilot's difference |
|---|---|---|---|
| **Cemify** | Cloud cemetery management (records, maps, websites) | $599/yr (3 users, 2,000 plots); $999/yr Management | The cheapest verified floor — still a cloud subscription holding permanent records, with tiers gated by plot count. PlotPilot is $9.99/mo flat, no plot-count tiers, records stored as local files the cemetery owns outright. |
| **CemeteryBase** | Church-cemetery cloud software | $99/mo / $899/yr | Proves the buyer (it markets directly at paper logbooks) — at ~8× PlotPilot's annual cost, in a cloud. PlotPilot takes the same migration pitch local. |
| **CemSites / PlotBox** | Cemetery software, quote-only | Unpublished | Quote-only enterprise posture for the buyer least able to sit through a sales process. PlotPilot is self-serve, published price, running the same day. |
| **Notebooks + plat map (DIY status quo)** | The documented reality | Free + fragility | One fire, one flood, one retiring sexton from partial loss. PlotPilot is the notebooks reconciled, searchable, backed up, and printable — with every entry's source preserved so the old books remain the authority the app cites. |

## 7. Suggested Gumroad price

**$9.99/month membership** — $120/yr against a $599/yr verified floor and $899–$1,188/yr church/municipal alternatives. Low in absolute terms because many buyers are volunteers and tiny public offices; the market is wide (tens of thousands of small cemeteries — stated qualitatively; no count is claimed) and the record, once migrated, never churns.

## 8. Candidates considered (scored 1–5; 20 max)

| Candidate | Pain frequency | Incumbent weakness | Local-WinUI feasibility | Willingness to pay | Total |
|---|---|---|---|---|---|
| **A. PlotPilot — cemetery records + plat** | 3 | 5 | 5 | 3 | **16** |
| B. Small-town clerk's general records (permits/licences/minutes) | 3 | 4 | 5 | 2 | 14 |
| C. Funeral-home case manager | 4 | 4 | 4 | 4 | 16* |

Scoring notes:

- **A (16):** Pain frequency is low (burials are not daily at this size) but pain *severity* and permanence are extreme — a lost or wrong record is forever. Incumbent weakness is total (quote-only or $599+ against notebooks). Willingness to pay is the honest weak point (volunteer buyers) — priced for it at $9.99.
- **B (14):** The NAICS 92 single-clerk slice is real, but public-records portals, state reporting, and audit rules push general municipal records toward cloud/ERP over time; cemetery records are the one clerk job that stays local forever. PlotPilot is B's best module, shipped first and standalone.
- **C (16\*):** Funeral-home management ties A on score but is **hunt #3's assigned industry** — not spec'd here to avoid duplication; its incumbents (Osiris from $79/mo, Passare quote-only, SRS $2,495 one-time legacy) are recorded in the master map for that hunt.

## 9. Build notes (for whoever builds it — not part of this spec task)

- Addressing scheme flexibility is the make-or-break design decision: sections/blocks/lots/spaces must be user-named and user-shaped (some cemeteries use ranges/tiers; churchyards may be freeform with a "map-only" space list). Build the scheme editor before the register UI.
- The grid map renders *from* the register (never a separate drawing that can drift) — the Arran-Elderslie lesson is that map and database disagreeing is the original sin of this niche.
- Closest pattern relatives: CasePilot (per-record provenance and dated chains), ClientPilot (search-first register design), KennelPilot (grid/board view discipline).
- First-run sample (fictional, labeled): "Brookmont Cemetery" — 2 sections, 60 spaces in mixed statuses, 14 burial records with notebook provenance, one deliberate plat-vs-notebook conflict surfaced in the reconciliation view.
- Hard rule for copy: dignity first, permanence second ("records that outlive us all, kept by you"), price third. Never use fear-of-loss language beyond the factual — this buyer buries their neighbors; the copy behaves accordingly.
