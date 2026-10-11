#!/usr/bin/env python3
"""Round 3 localization: generate Resources.resw for 59 new languages x 4 apps.

Inputs:
  /tmp/uniq_en.json      - list of 319 unique English strings (index = key id)
  /tmp/per_app_en.json   - {app: [[resw_name, english_value], ...]}
  /tmp/t3/<lang>.json    - {"<idx>": "<translation>"} per language
Output:
  ~/workspace/windows-apps/<app>/Strings/<lang>/Resources.resw
"""
import json, os, sys
from xml.sax.saxutils import escape

WORKSPACE = os.path.expanduser("~/workspace/windows-apps")
T3 = "/tmp/t3"

with open("/tmp/uniq_en.json", encoding="utf-8") as f:
    uniq = json.load(f)
value_to_idx = {v: i for i, v in enumerate(uniq)}

with open("/tmp/per_app_en.json", encoding="utf-8") as f:
    per_app = json.load(f)

langs = sorted(fn[:-5] for fn in os.listdir(T3) if fn.endswith(".json"))
print(f"languages: {len(langs)}")
assert len(langs) == 59, f"expected 59, got {len(langs)}"

translations = {}
for lang in langs:
    with open(os.path.join(T3, lang + ".json"), encoding="utf-8") as f:
        translations[lang] = json.load(f)

missing = 0
for app, items in per_app.items():
    for name, value in items:
        if value not in value_to_idx:
            print(f"MISSING EN VALUE [{app}] {name} = {value!r}")
            missing += 1
if missing:
    sys.exit(f"ABORT: {missing} per-app values not in uniq_en.json")

written = 0
for app, items in per_app.items():
    for lang in langs:
        t = translations[lang]
        out_dir = os.path.join(WORKSPACE, app, "Strings", lang)
        os.makedirs(out_dir, exist_ok=True)
        lines = ['<?xml version="1.0" encoding="utf-8"?>', "<root>"]
        for name, value in items:
            idx = value_to_idx[value]
            tr = t[str(idx)]
            if not tr.strip():
                sys.exit(f"ABORT: empty translation {app}/{lang} idx {idx}")
            lines.append(f'  <data name="{escape(name)}" xml:space="preserve">')
            lines.append(f"    <value>{escape(tr)}</value>")
            lines.append("  </data>")
        lines.append("</root>")
        with open(os.path.join(out_dir, "Resources.resw"), "w", encoding="utf-8") as f:
            f.write("\n".join(lines) + "\n")
        written += 1

print(f"wrote {written} resw files")
