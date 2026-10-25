# GAP APP SPEC — DenialPilot

**Status:** Spec only (no build authorized yet)
**Date:** 2026-10-25
**Line:** The Night Shift SRE / St. John Windows apps (WinUI 3, local-first, Gumroad membership)
**Prepared for:** Shaun Nicholas St. John

---

## 1. The app in one line

**DenialPilot** — a local-first Windows desktop app that runs a health-insurance denial like a campaign: every denial, deadline, phone call, document, and escalation level in one place, on the patient's own machine, until the insurer answers or the case escalates.

## 2. The gap

The appeal *letter* is a solved, commoditized, free problem. The appeal *campaign* is unserved.

**The pain is massive and recurring:**

- Insurers of HealthCare.gov plans denied **19% of in-network claims in 2024** (37% of out-of-network claims), yet **fewer than 1% of denied claims were ever appealed** — and roughly **one-third of the appeals that were filed were overturned** at internal appeal (KFF, *Claims Denials and Appeals in ACA Marketplace Plans in 2024* — https://www.kff.org/patient-consumer-protections/claims-denials-and-appeals-in-aca-marketplace-plans-in-2024/).
- The dominant denial reasons are beatable process reasons, not medical judgment: "other" 36%, administrative 25%, missing prior authorization 9%, medical necessity only 5% (same KFF brief). People lose by disorganization, not on the merits.
- For chronically ill patients this is not a one-time event — it is a repeating administrative workload layered on top of being sick.

**The incumbents all stop at the letter:**

- Every notable patient-facing tool is a **one-shot web letter generator**: upload your denial letter to someone's server, get a draft, done. Examples: Fight Health Insurance (free, web, AI letters — https://moge.ai/product/fight-health-insurance), AskBenji (free web upload tool; its privacy pitch is that your documents are "auto-deleted in 24 hours" — i.e., your medical documents still leave your PC — https://www.askbenji.co/), ReclAIm (web, early access — https://www.reclaimhealth.ai/).
- Several are **geographically or scope-limited**: Overturn is built on California independent-review decisions (https://overturn.co/); GetMyYes sells a California-specific packet for $5 (https://getmyyes.com/appeals/health-insurance-appeal-california); AuthFight focuses on prior-authorization denials (https://www.authfight.com/).
- **None of them manages the fight after the letter**: the 180-day appeal window, the insurer's 30-day response clock, Level 2 appeals, the 4-month external-review window, state Department of Insurance complaints, follow-up call cadence, and proof of what was sent when. The how-to guides in this space openly admit the gap — they instruct patients to hand-build the missing layer: "Keep a denial tracker—log date of denial, denial code, and appeal deadline" (https://penuw.com/appeal-denied-health-insurance-claim-step-by-step-guide/) and to log every call's date, representative name, and "call reference number" by hand (https://forum.caregiversupportcommunity.org/t/insurance-appeals/72).

**The unserved layer is case management for a months-long, multi-denial fight — offline, private, and on the patient's own computer.** That is exactly the layer Shaun's existing line already knows how to build (DisputePilot's deadline countdowns, ClaimPilot's letters/deadlines/evidence model) pointed at a fight neither of those apps covers.

## 3. Target user

- **Primary:** Chronically ill patients and their caregivers juggling repeat denials across multiple claims, providers, and plan years — the people for whom "just generate a letter" fails by the third denial.
- **Secondary:** Patients with one large denial (surgery, specialty drug, DME) where a single case is worth hundreds to thousands of dollars and the deadline clock matters.
- **Tertiary:** Patient advocates and family members managing someone else's fight (caregiver mode is a first-class use case, not an afterthought).

## 4. Core features (MVP, all local-first)

1. **Denial case file** — one case per denial: insurer, plan, claim number, date of service, provider, amount denied, denial date, denial reason code + plain-language decode of common reason categories (administrative, prior auth, medical necessity, coding, eligibility — categories per the KFF reason breakdown), status pipeline (Denied → Info gathered → Level 1 appeal → Level 2 appeal → External review → DOI complaint → Resolved won/lost/paid).
2. **Deadline engine** — computed clocks per case, same color language as DisputePilot (green / orange ≤5 days / red overdue): appeal filing window (user-set from the denial letter, with 180-day and 4-month external-review presets), insurer response clock (30-day standard / 72-hour urgent presets), follow-up nudges. Every computed date shows its basis ("180 days from denial date per your letter") — no silent math.
3. **Call log built for insurer calls** — date/time, representative name, call reference number, what was promised, next follow-up date with overdue flag. (The exact fields the caregiver guides tell patients to capture by hand.)
4. **Evidence checklist per denial type** — denial letter, EOB, policy excerpt, medical records, physician letter of medical necessity, proof of submission (fax confirmation / certified mail / portal screenshot note). Check items off; the case shows a readiness state before you file.
5. **Document vault (local)** — attach copies/paths of letters, EOBs, and records to the case; everything stays in the local data store. Export a complete case packet (summary + log + checklist) to PDF/print for a doctor, advocate, or regulator.
6. **Letter template library** — fill-in templates (general information, not legal advice): Level 1 internal appeal, Level 2 appeal, external review request, state DOI complaint cover letter, physician letter-of-medical-necessity *request* letter, and a medical-bill dispute letter. Templates merge case fields; user edits and signs; the app records what was sent, when, and by what method.
7. **Multi-case dashboard** — all open fights sorted by deadline pressure; total dollars in dispute; wins/losses tally. A patient fighting five denials sees one board, not five folders.
8. **Method & proof ledger** — per case, a chronological ledger: every letter sent (method, tracking/confirmation), every call, every insurer response, every deadline met or missed — exportable. This is the artifact that wins escalations and DOI complaints.

**Platform rules (same as the rest of the line):** local JSON storage, no network calls, no account, no telemetry, works fully offline. Data never leaves the machine — the privacy posture *is* the product.

## 5. What it deliberately does NOT do

- **No AI letter generation and no document upload analysis.** That layer is free and crowded (Fight Health Insurance, AskBenji, Overturn); competing there adds PHI-handling risk and no differentiation. Templates + the user's own words only.
- **No medical or legal advice.** It organizes, reminds, and documents. Screens carry the same class of disclaimer ClaimPilot uses: general information and document tools only.
- **No insurer portal integration, no e-filing, no faxing.** Submission methods are the insurer's; DenialPilot tracks proof that you submitted.
- **No claims submission or billing/coding tools** (provider-side denial management is a different, EHR-dominated market).
- **No cloud sync, no mobile app** in MVP. Desktop-first is the gap; the incumbents' weakness is web-only.
- **Not a replacement for HealthPilot** (medications, appointments, symptom/CPAP log) **or ClaimPilot** (legal claims and legal matters — traffic, criminal, bankruptcy, federal matters). DenialPilot owns exactly one fight: patient vs. health insurer. Cross-links ("open this case in ClaimPilot" if it becomes a legal matter) are a post-MVP nicety, not MVP scope.

## 6. Nearest incumbents — and how DenialPilot differs

| Incumbent | What it is | Price | DenialPilot's difference |
|---|---|---|---|
| **Fight Health Insurance** (moge.ai) | Free web AI: upload denial letter, get generated appeal letters; optional paid faxing | Free + per-fax fees | Web one-shot: your documents go to their server and the tool's job ends at the letter. DenialPilot never receives your documents (they never leave your PC) and manages the full multi-level fight for months, across every denial you have. |
| **GetMyYes** (getmyyes.com) | California-only appeal packet (DMHC/CDI/ERISA routing + drafts) | $5/packet | Single-state, single-packet, web. DenialPilot is state-agnostic case management — deadline presets are user-confirmed from their own denial letter rather than one state's rulebook — and one membership covers unlimited cases and plan years. |
| **The spreadsheet / notebook (DIY status quo)** | What the guides actually tell patients to build: denial tracker, call log, calendar reminders | Free + hours | This is the real incumbent. DenialPilot is that system, finished: computed deadline clocks with overdue escalation, insurer-call fields (rep name + reference #), evidence readiness, and a proof ledger that exports as one packet — without the patient having to design it while sick. |

## 7. Suggested Gumroad price

**$9.99/month membership** — same tier as DisputePilot. Rationale: the dollars at stake in a single denial routinely exceed a year of membership, but the audience is price-sensitive patients, so it must sit far below the $49/mo web dispute-software norm and at parity with the line's other single-fight tool rather than up with ClaimPilot's $14.99 broad legal suite.

## 8. Candidates considered (scored 1–5; 20 max)

| Candidate | Pain frequency | Incumbent weakness | Local-WinUI feasibility | Willingness to pay | Total |
|---|---|---|---|---|---|
| **A. DenialPilot — health-insurance denial appeal case manager** | 5 | 4 | 5 | 4 | **18** |
| B. DocketPilot — rules-based court deadline calculator (FRCP/day-count) for pro se + solos | 4 | 3 | 3 | 3 | 13 |
| C. Credit-dispute campaign manager (letters + rounds) | 4 | 2 | 4 | 3 | 13 |
| D. Micro-team on-call scheduling/handoff (post-Opsgenie) | 4 | 3 | 1 | 3 | 11 |
| E. Night-shift sleep planner (desktop) | 3 | 2 | 2 | 2 | 9 |

Scoring notes (evidence, not vibes):

- **B (13):** Real pain — ABA Profile data: calendar/deadline errors are the leading malpractice-claim category (16.63% for failure to know/respond to the court calendar alone; ~25–30% across calendaring categories — https://blog.bluestonelawfirm.com/2008/10/legal-malpractice-news/the-most-common-cause-of-legal-malpractice/ and https://www.runsensible.com/blog/legal-calendaring-software-law-firms/). But incumbents are entrenched and firm-priced (LawToolBox $40–60/user/mo — https://agiled.app/blog/best-scheduling-software-for-legal-professionals; DocketCalendar $20/court + $20/user/mo — https://softwarefinder.com/legal/docketcalendar), the truly unserved segment (pro se) is the least able to pay, a rules engine carries the highest correctness/liability burden of any candidate, and it sits closest to CasePilot (in build). Runner-up #1.
- **C (13):** Crowded, not a gap: DisputeBee $49/mo, Dispute Beast $49.99/mo, CreditRefresh $49.99/mo, Credit Repair Cloud $179/mo (https://rankite.com/blog/best-credit-dispute-software). A local-first privacy angle exists, but it cannibalizes DisputePilot, already in the line. Runner-up #2.
- **D (11):** The Opsgenie shutdown (sales ended June 4, 2025; service ends April 5, 2027 — https://allquiet.app/blog/top-opsgenie-alternatives-and-migration-targets) created real churn, but the core value of the category is cloud paging/SMS escalation, which a local-first desktop app cannot deliver; SaaS replacements already start at $4.99/user/mo (All Quiet, same source). Form-factor mismatch kills it.
- **E (9):** The category is actively served by mobile-native products (Timeshifter — http://www.timeshifter.com/shift-work-app; 3AM — https://3am.rest/; CircaFlow — https://circaflow.app/). Sleep tooling belongs on the phone by the bed, not the desktop.

## 9. Build notes (for whoever builds it — not part of this spec task)

- Reuse DisputePilot's patterns wholesale: local JSON store, derived deadline countdown with green/orange/red states, status pipeline, proof-ledger notes. Reuse ClaimPilot's disclaimer language pattern.
- No shared Licensing/voice/integrity code, consistent with the v40 apps — no new secrets needed.
- Ship 100 languages the same way as the rest of the line; deadline-date math must be locale-safe (store ISO dates; render localized).
- First-run sample case (a fictional denied claim, clearly labeled sample) so buyers see the pipeline, clocks, and ledger working before entering anything real — mirrors BizPilot's sample-client pattern.
- Hard rule for copy: never promise outcomes ("win your appeal"). Sell organization, deadlines, and proof — the things the evidence says patients actually lose by lacking.
