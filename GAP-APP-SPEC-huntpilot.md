# GAP APP SPEC — HuntPilot

**Status:** Spec only (no build authorized yet)
**Date:** 2026-10-25
**Line:** The Night Shift SRE / St. John Windows apps (WinUI 3, local-first, Gumroad membership)
**Prepared for:** Shaun Nicholas St. John
**Hunt:** Gap hunt #5 — destination experiences (exotic-game hunting ranches)

---

## 1. The app in one line

**HuntPilot** — a local-first Windows desktop app that runs an exotic-game ranch's hunt book: every hunter, every species and season, every trophy fee and balance, and the whole post-harvest chain — field dressing, meat processor, taxidermist, crating and shipping — as tracked clocks on the ranch's own machine, until the trophy is on the client's wall.

## 2. The gap

Exotic hunting ranches sell four- and five-figure hunts. Their front door is finally getting software; everything after the harvest — the part the client actually judges them on for the next year — still runs on paper, memory, and a taxidermist's phone number.

**The stakes, from the category's flagship:**

- Ox Ranch (Uvalde, TX; 18,000 acres, 60+ species) posts trophy fees from **$5,500** (Blackbuck) to **$12,000** (Addax), **$17,500** (Nile Lechwe, Grant's Gazelle), and **$25,000** (Black Wildebeest), on top of a $1,500 all-inclusive hunting package with non-hunter guests at $500–$750/night (https://www.oxhuntingranch.com/texas/blackbuck-hunting/, https://www.oxhuntingranch.com/texas/addax-hunting/, https://www.oxhuntingranch.com/texas/black-wildebeest-hunting/).
- Every one of those hunts *includes* a post-harvest service chain, promised in writing on every species page: "Field Dressing Included… Transportation of your Animal to Meat Processor / Taxidermist" (same sources). Taxidermy and shipping then take months — and no page, portal, or tool in the category shows the client or the ranch where a given trophy is in that chain.
- Marketplace listings show the front-end pattern elsewhere in the category: per-hunter species pricing with a **50% non-refundable deposit** collected at booking (https://www.venku.com/listing/s/exotic-trophy-hunts-kerrville-tx-2) — big balances ride on every hunt, tracked by whoever keeps the notebook.

**The new software stops at the gate:**

- A young crop of cloud tools now covers booking + deposit + waiver + check-in: HuntDocs (booking, payments, paperwork — https://red-feather-guides-and-outfitter.myhuntdocs.com/), Hunt Outfitter (free starter funded by a 4%-of-payment client fee capped at $35; harvest tracking is a paid upgrade — https://www.huntoutfitter.app/blog/free-hunting-outfitter-software), Texas-built Mainbeam Booking — still "looking for 3 pilot ranches," i.e., the category's software era is just starting (https://elmforkdigital.com/mainbeam-booking.html) — plus TexasHuntLog and Ranchboard for check-ins and lease boards.
- **None of them is the ranch's back-office book of record:** the species price book with per-species season/method notes, hunter records across years (licenses, preferences, past harvests), trophy-fee balances after the deposit, guide assignments — and above all the **post-harvest follow-up clocks**: harvested → field dressed → at processor (date, ticket #) → meat ready/shipped → at taxidermist (date, work order #) → mount complete → crated → shipped → delivered. That chain is where a $17,500 client is won for life or lost forever, and it is tracked today by phone calls.
- **None of them is local-first.** These properties are rural by definition; the office is the system of record, connectivity is whatever the hill country gives you, and a high-value client list is not something a ranch wants rented back from a startup's cloud — the same posture that makes the rest of Shaun's line sell.

**The unserved layer is the hunt-and-trophy book: species, hunters, balances, and post-harvest clocks, offline, on the ranch's machine.** DenialPilot's deadline engine and DisputePilot's proof ledger are the same shapes, pointed at trophies instead of paperwork.

## 3. Target user

- **Primary:** Exotic-game and mixed-game ranches/outfitters (Texas exotics are the densest market — Ox Ranch, 777 Ranch, Y.O. Ranch, FTW Ranch class) running guided hunts with trophy fees, lodging, and a processor/taxidermist chain.
- **Secondary:** Hunting lodges and preserves in other states (driven hunts, waterfowl lodges) with the same deposit → hunt → harvest → mount chain.
- **Tertiary:** Ranches that also run experiences (the Ox Ranch pattern — hunts plus a tank attraction on one property): HuntPilot is the ranch side; RangePilot is the experience side; the guest book pattern is shared deliberately.

## 4. Core features (MVP, all local-first)

1. **Species & season book** — the ranch's own price book: species, trophy fee, season notes (year-round vs seasonal), methods allowed, herd/population notes, daily/package rates and non-hunter rates. User-maintained — the ranch's prices, not a scraped database.
2. **Hunter record** — one file per hunter across years: contact, license details (number/state/expiry as recorded by the ranch), party members, preferences (species wishlist, lodging, dietary), past hunts and harvests, lifetime value. Repeat clients are the business; this is their file.
3. **Hunt book** — bookings by date and guide: species targeted, package, deposit received, **balance ledger** (trophy fees added as harvested, payments, balance due with clock), lodging nights and non-hunter guests, waiver status register (same gate pattern as the rest of the line — records fact/version of signing).
4. **Harvest log** — per harvest: species, date, guide, measurements (horn length etc. as the ranch records them), photos referenced by local path, field-dressing completion. Feeds the trophy chain and the ranch's season records.
5. **Trophy chain tracker (the headline)** — every harvested animal as a case with a status pipeline and clocks: Field dressed → To processor (date/ticket) → Meat processed → Meat shipped/delivered → To taxidermist (date/work order) → Mount in progress → Mount complete → Crated → Shipped (carrier/tracking as recorded) → Delivered. Each stage shows days-in-stage; stages past a user-set norm go orange, then red. The ranch sees every outstanding trophy on one board — nothing sits at a taxidermist forgotten for a year.
6. **Follow-up list** — derived from the chain and the book: clients to update this week (with what stage changed), balances to collect, deposits to confirm, mounts nearing completion. The "who do I call today" screen the category currently keeps in someone's head.
7. **Guide & lodge board** — guide assignments per hunt day, room/lodge occupancy per night, airport/runway arrival notes (guest logistics as recorded facts: flight, pickup, ETA).
8. **Season report** — exportable: hunts, harvests by species, revenue by species/package, balances outstanding, trophies outstanding by stage. The owner's season review and the repeat-marketing list in one artifact.

**Platform rules (same as the rest of the line):** local JSON storage, no network, no account, no telemetry, fully offline. Client lists and trophy records never leave the ranch's machine — the privacy posture is the product.

## 5. What it deliberately does NOT do

- **No online booking, deposits, or payments.** The new cloud tools (HuntDocs, Mainbeam, Hunt Outfitter) are fighting for the storefront; HuntPilot is the book of record behind it and deliberately does not compete on their turf.
- **No hunting-regulation or license advice.** It records the license details and seasons the ranch enters; it does not interpret TPWD or any state's rules. Standard line disclaimer: records tool, not legal/compliance advice. (Exotic species in Texas are largely unregulated as game — the app takes no position either way.)
- **No herd-management / wildlife-biology suite** (population modeling, cull planning) in MVP — Ranchboard-class lease tools gesture at records; biology is a different product and a v2 conversation.
- **No client-facing portal or app.** Guests get the ranch's calls and emails; HuntPilot makes sure the ranch knows what to say. (This is the honest desktop-first scope; a guest-facing trophy tracker would be a web product — noted as a future line extension, not MVP.)
- **Not KennelPilot** (animals boarded as guests) and not a lease/hunter check-in board (TexasHuntLog/Ranchboard already do check-ins). HuntPilot owns the commercial hunt: money, harvest, and the trophy's journey home.

## 6. Nearest incumbents — and how HuntPilot differs

| Incumbent | What it is | Price | HuntPilot's difference |
|---|---|---|---|
| **HuntDocs** | Cloud platform: booking, payments, waivers, client/staff portals | Subscription + payment flow (site) | Storefront-first. HuntPilot starts where HuntDocs' flow ends: balances after harvest and the months-long processor → taxidermist → shipping chain, as local clocks — plus it works with no connectivity and no per-booking economics. |
| **Hunt Outfitter (app)** | Cloud booking for outfitters; free starter funded by client-paid fee | 4% of each payment (cap $35) on starter; paid tiers above | Percentage rent on five-figure hunts adds up, harvest tracking is paywalled, and the record lives in their cloud. HuntPilot is flat, local, and built around the post-harvest book. |
| **The notebook + taxidermist's phone number (DIY status quo)** | How the chain is actually tracked at most ranches | Free + lost trophies | This is the real incumbent. HuntPilot is that system finished: every trophy a case with a stage, a clock, and a follow-up list — the artifact that turns a $17,500 hunt into a rebooking. |

## 7. Suggested Gumroad price

**$29.99/month membership** — premium tier alongside RangePilot. Rationale: a single trophy fee ($5,500–$25,000) dwarfs a year of membership; one recovered balance or one rebooked client pays for a decade. Ranches already accept $1,500 package pricing and percentage fees at the front door, so a flat $29.99 for the back office is an easy yes — and it stays far under what the cloud platforms extract from the same bookings over a season.

## 8. Candidates considered (scored 1–5; 20 max)

| Candidate | Pain frequency | Incumbent weakness | Local-WinUI feasibility | Willingness to pay | Total |
|---|---|---|---|---|---|
| **A. HuntPilot — exotic ranch hunt & trophy book** | 5 | 4 | 5 | 5 | **19** |
| B. Another booking/waiver/check-in platform for outfitters | 4 | 2 | 1 | 3 | 10 |
| C. Hunting-lease member book (blinds, check-ins, harvest logs for lease members) | 3 | 2 | 3 | 2 | 10 |
| D. Guest-facing trophy tracker (clients watch their mount's progress) | 3 | 4 | 1 | 2 | 10 |

Scoring notes (evidence, not vibes):

- **B (10):** The storefront layer now has HuntDocs, Hunt Outfitter, Mainbeam, TexasHuntLog and Venku competing, several free or fee-funded — and it's inherently cloud/mobile, the opposite of this line's form factor. Entering it would be a me-too against funded startups. Dropped; HuntPilot deliberately starts after their flow ends.
- **C (10):** Lease/member check-in is exactly what Ranchboard and TexasHuntLog already sell, to the lowest-paying segment of the category (lease members splitting costs). Dropped.
- **D (10):** The most emotionally appealing gap — clients genuinely cannot see where their trophy is — but it's a hosted web product (clients are remote by definition), not shippable in the local-first desktop pattern. Recorded honestly as a future web extension of HuntPilot's chain data, not an MVP. Runner-up concept; do not build in this line.

## 9. Build notes (for whoever builds it — not part of this spec task)

- Reuse DenialPilot's deadline engine (derived clocks, green/orange/red, "basis shown" dates) for trophy stages and balances, and KennelPilot's board pattern for the guide/lodge day view.
- Species book ships empty-with-prompts plus a clearly labeled *sample* ranch dataset (fictional ranch, fictional hunters, three trophies at different chain stages) so the follow-up list and clocks are visible on first run.
- No shared Licensing/voice/integrity code, consistent with the v40 apps. Local JSON, ISO dates, decimal money math, locale-safe rendering.
- Hard rule for copy: never promise bookings, harvests, or regulatory compliance. Sell the book: every hunter known, every balance visible, every trophy accounted for until it's on the wall.
