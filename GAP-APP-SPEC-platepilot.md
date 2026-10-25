# GAP APP SPEC — PlatePilot

**Status:** Spec only (no build authorized yet)
**Date:** 2026-10-25
**Line:** The Night Shift SRE / St. John Windows apps (WinUI 3, local-first, Gumroad membership)
**Prepared for:** Shaun Nicholas St. John
**Industry:** Restaurants & food service (independent restaurants, food trucks, small caterers)

---

## 1. The app in one line

**PlatePilot** — a local-first Windows desktop app that answers the one question a small restaurant's survival hangs on — *what does this dish actually cost me today, and what should it cost on the menu?* — with an ingredient price book, recipes with real yields, and plate costs that recalculate the moment an ingredient price changes. No POS integration, no invoice-scanning service, no $350/mo back-office platform.

## 2. The gap

Recipe costing is the highest-stakes arithmetic in a small restaurant, and the industry still does it by hand while the software that does it properly is priced for chains.

**The manual status quo is the majority, and it's stale the day it's written:**

- Per National Restaurant Association data cited in MarketMan's recipe-costing guide, **more than 40% of restaurants still use manual processes — pen-and-paper or spreadsheets — to track back-of-house data** (https://www.marketman.com/blog/recipe-costing-software-complete-guide-to-profitable-menus).
- The same guide names why spreadsheets rot: ingredient prices "swing 10–30% seasonally," portion creep silently adds cost, and with ~70% annual staff turnover the one person who understood the spreadsheet keeps leaving. Manual recipe updates are estimated at **10+ hours per week per location** in the vendor's accounting — even discounted for salesmanship, a quarterly repricing in a busy kitchen simply doesn't happen, so menus are priced off costs from two price increases ago (same source).
- The lived version, from an inventory-app review roundup: the chef at 11 PM "staring at a spreadsheet that hasn't been updated in three weeks… the formulas are broken" (https://www.savortheapp.com/blog/cuisine-location-guides/easiest-restaurant-inventory-apps/).

**The real tools start where a single location can't follow:**

- **MarginEdge: $350 per location per month**, flat, from its own pricing FAQ (https://www.marginedge.com/) — a full back-office platform (invoice processing, bill pay, POS sync, daily P&L) whose recipe costing is one module in a chain-priced whole. Review coverage notes the monthly cost is steep for very small, budget-conscious restaurants (https://www.selecthub.com/restaurant-management-software/marginedge-vs-restaurant365/).
- **MarketMan: from $179–$249/mo** depending on the roundup, **plus a $500 setup fee**, with integrations (POS, accounting, supplier ordering) that are the product's center of gravity (https://www.softwaresuggest.com/blog/best-restaurant-inventory-software/; https://www.toolsforhumans.ai/toolkit/best-accounting-software-small-restaurant — which concedes the pricing "is high for a single-location restaurant with thin margins").
- **The cheap end exists but is a different animal:** meez starts at **$19/mo** and reciProfity at $65/mo (https://www.guideflow.com/blog/food-cost-management-software) — meez is a cloud culinary platform built around recipe collaboration and kitchen-team workflows for professional chefs, not a cost book the owner of a food truck opens to reprice a menu. Nobody sells the narrow thing: **your ingredient prices, your recipes, your plate costs, on your PC, offline, in dollars you can defend.**

**The unserved layer is the costing book itself** — ingredient price history, yield-adjusted recipes, plate cost %, and menu-price targets — deliberately separated from the inventory/POS/invoice machinery that inflates every incumbent's price. A food truck or 40-seat independent doesn't need their invoices OCR'd; they need to know the taco plate crossed 32% food cost when tortillas and beef moved, before the month is gone.

## 3. Target user

- **Primary:** Independent single-location restaurants and food trucks whose owner/chef prices the menu — currently costing recipes in a spreadsheet (or not at all), repricing by feel.
- **Secondary:** Small caterers and bakeries/cafés quoting per-event and per-item prices from real ingredient costs (a quote built on PlatePilot numbers instead of memory).
- **Tertiary:** Ghost-kitchen and pop-up operators testing whether a dish survives at a viable price before it goes on a menu.

## 4. Core features (MVP, all local-first)

1. **Ingredient price book** — ingredient, purchase unit (case/weight/volume/count), purchase price, **price history with dates** (every price change recorded, so cost drift is a visible line, not a surprise), supplier note (free text — which vendor, no integration), and edible-yield % per ingredient (trim/cooking loss) with editable starter defaults labeled as starting points.
2. **Unit conversion engine** — the unglamorous core that breaks spreadsheets: purchase units ↔ recipe units (lb ↔ oz ↔ g, case ↔ each, volume measures) with density/count conversions the user confirms per ingredient. All math shown, all conversions stored with the ingredient — the recipe never silently assumes.
3. **Recipe builder with sub-recipes** — dishes built from ingredients and other recipes (sauces, doughs, marinades), batch size + portion size → per-plate ingredient quantities, yield-adjusted. Per-plate cost computed live from current price-book prices.
4. **Plate cost & menu engineering view** — per dish: plate cost, menu price, food-cost %, contribution margin; target food-cost % set by the user (e.g., 30%) → **suggested menu price** per dish. The whole menu on one screen sorted by cost-% drift; dishes whose food cost crossed the user's threshold since last review are flagged (the "slow bleed," made visible).
5. **Price-change impact** — when an ingredient price is updated, one view: every dish using it, old plate cost → new plate cost, new food-cost % at current menu prices. This is the feature that turns a supplier increase into a same-day repricing decision.
6. **Quote mode (caterers)** — build an event quote from recipes × servings + user-added lines (labor, rentals, fees as plain amounts), producing a printable quote with cost and margin visible to the operator (cost side clearly marked operator-only on the internal view; the printed quote shows prices, not costs).
7. **Review cadence** — a "cost review" checklist state per dish (costed date, reviewed date) and a dashboard: dishes never costed, dishes stale (>user-set days since price review), top cost-drift movers. The app sells the habit the spreadsheet couldn't sustain.
8. **Print/export** — recipe cards (quantities scaled to a chosen batch size) and a menu-cost sheet to PDF/print; CSV export of the price book and recipe costs for the user's accountant or their own backup.

**Platform rules (same as the rest of the line):** local JSON storage, no network calls, no account, no telemetry, works fully offline — including in a food truck with no signal, which is rather the point.

## 5. What it deliberately does NOT do

- **No POS integration, no sales data, no theoretical-vs-actual variance.** That requires the POS pipe and doubles the product; it's MarginEdge's game. PlatePilot costs what a dish *should* cost; it doesn't audit what the kitchen actually used.
- **No invoice scanning/OCR, no supplier ordering, no inventory counts/stock levels.** The price book is updated by the user from their invoices (a 2-minute habit the impact view rewards); stock counting is a different job with different incumbents.
- **No nutrition labeling or allergen certification.** Allergen *tags* carried from ingredients to dishes as user-entered flags are in scope as information organization; the app never certifies a dish allergen-free or computes regulatory nutrition panels.
- **No accounting, payroll, or scheduling.** Exports only.
- **No recipe discovery/content.** No built-in recipe library beyond clearly-labeled sample data; the user's recipes are the product.
- **No multi-location menu management.** One operation, one price book; chains are the incumbents' market.

## 6. Nearest incumbents — and how PlatePilot differs

| Incumbent | What it is | Price | PlatePilot's difference |
|---|---|---|---|
| **MarginEdge** (marginedge.com) | Cloud back-office platform: invoice processing, inventory, recipe costing, bill pay, POS/accounting sync, daily P&L | $350/location/mo flat | A platform priced for operators who need the whole back office automated. PlatePilot is only the costing book — no invoice pipeline, no POS pipe — at $9.99, offline, for the operator whose actual problem is a stale spreadsheet and a menu priced off old beef costs. |
| **MarketMan** (marketman.com) | Cloud inventory & supplier platform with recipe costing, ordering, POS/accounting integrations | From $179–$249/mo + $500 setup fee | Inventory-and-supplier-first; costing rides on its integration stack, with setup to match. PlatePilot has no setup project and no integrations to maintain: enter ingredients and recipes, get plate costs — the 40%-still-manual segment's actual ask. |
| **meez** (getmeez.com) | Cloud recipe platform for culinary professionals: recipe collaboration, scaling, kitchen workflows; costing included | From $19/mo | The closest price neighbor, but it's a chef-collaboration tool in a vendor cloud, organized around recipes as team documents. PlatePilot is organized around **cost**: price history, drift flags, plate-cost % and menu-price targets, owned locally by the operator rather than shared with a kitchen team. |

## 7. Suggested Gumroad price

**$9.99/month membership** — the line's single-purpose-tool tier, deliberately under meez's $19 floor and ~3% of MarginEdge's per-location price. Rationale: the audience runs on 5–10% net margins and will not trial a $179 tool, but one dish caught crossing from 28% to 36% food cost — or one catering quote priced from real costs instead of memory — returns the year. The price says "spreadsheet replacement," not "platform."

## 8. Candidates considered in this hunt (scored 1–5; 20 max)

| Candidate | Pain frequency | Incumbent weakness | Local-WinUI feasibility | Willingness to pay | Total |
|---|---|---|---|---|---|
| **PlatePilot — independent restaurant recipe costing** | 4 | 4 | 5 | 3 | **16** |
| BayPilot — small auto repair shop work orders | 4 | 5 | 4 | 5 | 18 |
| TruckPilot — owner-operator IFTA & records (trucking) | 5 | 3 | 5 | 4 | 17 |
| KennelPilot — small pet boarding/daycare records | 4 | 4 | 4 | 4 | 16 |
| Small-landlord rent/lease ledger | 4 | 2 | 5 | 2 | 13 |
| Reseller inventory & profit ledger | 4 | 2 | 5 | 2 | 13 |
| Family child care CACFP records | 4 | 2 | 3 | 2 | 11 |
| Small-church giving records | 3 | 2 | 5 | 1 | 11 |
| Cattle herd records | 3 | 2 | 3 | 3 | 11 |

Scoring notes (evidence, not vibes):

- **Restaurants (16):** The highest feasibility in the set (5 — the product is arithmetic: units, yields, prices) against the softest willingness to pay (3 — thin margins, high churn, an industry where businesses die young). Incumbent weakness is a split picture: the full platforms are chain-priced ($179–$350/location), while the $19 meez floor keeps weakness at 4, not 5. The wedge is that *neither* end sells an offline, operator-owned costing book.
- **Auto repair (18) / trucking (17) / pet services (16):** specified separately in this hunt's spec set.
- **Dropped:** landlords — free cloud incumbents own the rent-collection hook (https://www.nestledgerapp.com/ is one of several new entrants even before the established free tiers); resellers — demand is real but the bottom is filling (Etsy spreadsheet products, https://aiflipledger.app/ at $14.99/mo) and the audience is side-hustle price-sensitive; child care — KidKare/Minute Menu owns CACFP with sponsor integration a local app cannot replicate (https://www.kidkare.com/how-family-child-care-providers-can-maximize-food-cost-reimbursements-and-tax-deductions-cacfp-participation-vs-non-participation-explained/); churches — free/cheap incumbents and the lowest willingness to pay surveyed (https://reachrightstudios.com/blog/donor-management-software/); ranching — CattleMax, 25 years in, from ~$12–30/mo, and the work is in the field on a phone (https://farmonaut.com/precision-farming/cattle-ranching-software-7-tools-to-boost-efficiency).

## 9. Build notes (for whoever builds it — not part of this spec task)

- Follow the v40 pattern: local JSON store, no network calls, no shared Licensing/voice/integrity code — no new secrets needed. This app is a computation product, not a tracker: build from the SRE Toolbox skeleton (pure local computation + reference data) with entity files (ingredients/recipes) in the ClientPilot pattern.
- **Decimal math everywhere; no floats for money or quantities.** Every computed plate cost must be expandable to its lines (ingredient, quantity after yield, unit price, line cost) — a chef who can't see the math won't trust the number, and the incumbents' spreadsheets failed exactly there.
- Unit system: store quantities internally in grams/ml/count with per-ingredient conversion definitions; imperial and metric entry both first-class (US independents buy in lb/case and portion in oz). Conversion edits must re-flow recipe costs visibly, never silently.
- Starter yield defaults (common trim/cooking yields) ship as *editable sample data with an "industry-typical starting point — verify against your kitchen" label*; no yield is ever presented as authoritative.
- Price history is append-only in the data model (price, date, source note) — the drift features depend on never overwriting a price.
- Ship 100 languages the same way as the rest of the line (script-qualified resw tags); numbers/dates localized; currency is a user setting, never hardcoded to USD in computation.
- First-run sample: one fictional burger-joint mini menu (burger + fries + sauce sub-recipe) with two historical price points on beef so the impact view and drift flags demonstrate themselves, clearly labeled sample data.
- Hard rule for copy: never promise profit outcomes ("cut food cost 5%"). Sell knowing today's plate cost, catching drift the week it happens, and pricing a menu from numbers instead of memory.
