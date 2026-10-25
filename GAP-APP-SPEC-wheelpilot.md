# GAP APP SPEC — WheelPilot

**Status:** Spec only (no build authorized yet)
**Date:** 2026-10-25
**Line:** The Night Shift SRE / St. John Windows apps (WinUI 3, local-first, Gumroad membership)
**Prepared for:** Shaun Nicholas St. John
**Industry:** Educational Services — driving schools (NAICS 611692)

---

## 1. The app in one line

**WheelPilot** — a local-first Windows desktop app that runs a small driving school's day: students, lesson packages and balances, instructor × car × student scheduling, and road-test readiness — for the 1–3-instructor school facing a $250/mo floor for software, or keeping the whole business in a paper diary.

## 2. The gap

Driving-school software exists — and is priced as if every school were a franchise. The smallest schools, which teach a disproportionate share of teen drivers in small towns, are left with a paper diary and a spreadsheet.

**The pricing, verified from a fetched comparison:**

- **Drive Scout:** **$50/user/mo with a 5-user minimum — a $250/mo effective floor** — plus **$250 one-time setup**, website templates +$100/mo (https://zutobi.com/us/driver-guides/driving-school-software-alternatives).
- Same comparison: **Zutobi Instructor** from $99/mo (first 12 students included); **Drivers Ed Solutions** $6.25/student with no monthly fee but **$600–$1,200 setup**; DrivingSchoolSoftware.com pricing not published.
- The UK solo-instructor comparator proves the workable price point exists elsewhere: **Driving Diary Solo £19/mo, Small School (≤5 instructors) £49/mo** (https://www.drivingdiary.co.uk/) — roughly 5–10× below the US floor for the same job.

**The paper status quo is documented by the tools themselves:** the low-end market sells spreadsheet/SOP kits and imports ("import from a spreadsheet"), and the sweep's sector finding is explicit — driving schools run on *"Drive Scout/DrivingSchoolSoftware.com or paper diary/Google Calendar + spreadsheets,"* and the local-first gap verdict is **YES specifically for driving schools**: *"1–3-instructor schools face a $250/mo effective floor for scheduling + package balances they now keep on paper; a local dual-resource (instructor × car × student) scheduler/ledger at $9.99–$19.99 fits."* (Contrast tutoring/music, checked and rejected in §8: cloud floors there are already $16–17/mo.)

**The unserved layer is the dual-resource day book**: a lesson needs a student, an instructor, *and* a car at the same time — a three-way constraint no generic calendar handles — plus package balances (she bought 6 hours, she's used 4.5), classroom-session completion where the state course applies, and the road-test date the whole plan aims at. All of it is office-desk data. None of it needs a network. And teen student records are exactly the kind of data a parent would prefer lives on the school's own PC.

## 3. Target user

- **Primary:** Independent driving schools with 1–3 instructors and 1–4 cars, owner-taught, currently on paper diary / Google Calendar + a package-balance spreadsheet (or notebook).
- **Secondary:** Solo instructors going independent from a franchise school, setting up their own book for the first time.
- **Tertiary:** Small schools offering both teen classroom courses and behind-the-wheel packages, juggling two pipelines in one diary.

## 4. Core features (MVP, all local-first)

1. **Student register** — student record: contact + parent/guardian contact for minors, permit/license fields as user-entered data (permit number, issue/expiry — with the line's expiry-warning colors), course enrolled, status (Inquiry / Enrolled / In training / Test-ready / Passed / Completed / Inactive). Notes per student.
2. **Package & balance ledger** — lesson packages (user-defined: e.g., 6-hr BTW, 10-hr + test car, classroom-only), purchased hours, used hours logged per lesson, **remaining balance always visible on the student and on the schedule** — the spreadsheet's whole job, impossible to get stale. Payments recorded (amount/date/method note); balance owing flagged.
3. **Triple-resource scheduler** — day/week board across instructors and cars: booking a lesson assigns student + instructor + car with **hard conflict warnings** (instructor double-booked, car double-booked, student double-booked, car in maintenance). Pickup location per lesson (home/school/meet point as entered). This three-way board is the product.
4. **Cars file** — per vehicle: identifiers as entered, inspection/registration dates with expiry warnings, maintenance log (date, work, cost), out-of-service flags that block scheduling. Dual-control/insurance dates are user-entered fields with warnings — records, not verification.
5. **Course & completion tracking** — classroom sessions (user-built session list per course) with per-student attendance; behind-the-wheel milestone checklist (user-defined skills list: parking, highway, night drive…) with instructor sign-off per milestone → **test-readiness view**: who's ready, whose test is booked (date entered), who passed (date + note). Certificates/completion records are recorded as issued (date + reference) — the school issues its own documents as it does today.
6. **Lesson log** — per completed lesson: date, duration, instructor, car, route/skills covered, instructor notes, next-lesson recommendation. Feeds the balance ledger automatically (no double entry).
7. **Inquiry & waitlist** — inquiries with source and follow-up flags (untouched in 5 days = flagged); waitlist by preferred times (after-school slots are the scarce resource — the waitlist view is organized by slot demand).
8. **Dashboard & reports** — today's lessons by instructor/car, balances outstanding total, tests this week, package hours sold vs. delivered, revenue by month (from recorded payments). The Sunday-evening planning screen.

**Platform rules (same as the rest of the line):** local JSON storage, no network calls, no account, no telemetry, works fully offline. **Student data — much of it minors' — never leaves the school's computer; the listing copy leads with exactly that.**

## 5. What it deliberately does NOT do

- **No online booking / parent portal / text reminders.** Those are network features and the cloud tools' pitch; WheelPilot is the school's book of record. (Copy states this plainly — honesty is the positioning, as with GuardPilot.)
- **No state reporting or DMV integration, no certificate printing authority.** Requirements vary by state; WheelPilot records completions as the school enters them and never claims to satisfy any state's reporting rule. Screens and copy carry the confirm-before-relying posture used across the line.
- **No payment processing** — payments are recorded, not taken.
- **No curriculum content** (no lesson videos, no test-prep question banks) — that's Zutobi's consumer business, a different product for a different buyer.
- **No multi-location/franchise rollups.** One school, its instructors, its cars.

## 6. Nearest incumbents — and how WheelPilot differs

| Incumbent | What it is | Price | WheelPilot's difference |
|---|---|---|---|
| **Drive Scout** | Driving-school management platform (scheduling, packages, website) | $50/user/mo, 5-user minimum = **$250/mo floor** + $250 setup | The floor is the story: a 2-instructor school pays for 5 seats forever. WheelPilot is $9.99/mo flat — every instructor and car included — offline, with student records on the school's own PC. |
| **Zutobi Instructor / Drivers Ed Solutions** | Instructor tools / per-student platforms | From $99/mo / $6.25 per student + $600–1,200 setup | Per-student pricing punishes growth and setup fees punish starting. WheelPilot charges neither: no per-student fee, no setup fee, no seat count — the price is the price. |
| **Paper diary + spreadsheet (DIY status quo)** | The documented small-school reality | Free + double-bookings | WheelPilot is the diary finished: three-way conflict checks a paper diary can't do, balances that update themselves, and a test-readiness list that doesn't live in the owner's head. |

## 7. Suggested Gumroad price

**$9.99/month membership** — flat per school. Deliberately at the low end of the band: this buyer is a solo teacher, not a margin-rich business — but the $250/mo incumbent floor makes $9.99 read as relief, and the UK comparator (£19/mo solo) proves schools pay gladly at this level for exactly this job.

## 8. Candidates considered (scored 1–5; 20 max)

| Candidate | Pain frequency | Incumbent weakness | Local-WinUI feasibility | Willingness to pay | Total |
|---|---|---|---|---|---|
| **A. WheelPilot — driving-school day book** | 5 | 5 | 5 | 3 | **18** |
| B. Tutoring-center scheduler/billing | 4 | 1 | 5 | 3 | 13 |
| C. Music-school scheduler/billing | 4 | 1 | 5 | 3 | 13 |

Scoring notes:

- **A (18):** The only education niche where the incumbent floor ($250/mo effective) is an order of magnitude above the house band. Pain is daily and three-dimensional; feasibility is high; willingness to pay is the modest score (solo-teacher budgets), priced for at $9.99.
- **B/C (13 each):** Checked and **rejected on verified floors**: Teachworks from **$16.49/mo** + per-lesson fees (https://www.teachngo.com/blog/teachworks-vs-tutorbird) and My Music Staff from **$16.95/mo** (https://koalendar.com/blog/best-music-school-scheduling-software) — cloud incumbents already sit at house prices, so there is no price gap to enter, and tutoring is additionally hunt #3's assigned industry. Documented so neither is re-pitched.

## 9. Build notes (for whoever builds it — not part of this spec task)

- The triple-conflict scheduler is the engineering core (student × instructor × car, plus car-out-of-service blocking): build the conflict engine before the board UI, with unit-testable rules.
- Closest pattern relatives: KennelPilot (board + per-unit billing), GuardPilot (roster + licence-expiry warnings), VenuePilot (date book with hard conflict prevention — share the conflict philosophy, not code).
- Balance ledger must be transactional in feel: every lesson log entry moves the balance exactly once; corrections are dated adjustment entries, never silent edits (the DisputePilot ledger discipline).
- First-run sample (fictional, labeled): "Conroe Driving Academy" — 2 instructors, 2 cars, 8 students across statuses, one car in maintenance tomorrow, one instructor double-booking attempt visible in the sample week, one student test-ready with a test date set.
- Hard rule for copy: lead with the two numbers — "$250/mo floor" (theirs) vs. "$9.99 flat, every instructor and car included" (ours) — and with "your students' records stay on your computer." Never claim state compliance or DMV anything.
