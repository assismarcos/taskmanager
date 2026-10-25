---
name: new-adr
description: Records a new architecture decision in docs/adr/ using the project template
disable-model-invocation: true
argument-hint: <short title in the imperative>
---

Record the decision "$ARGUMENTS" as a new ADR.

1. Find the highest number in `docs/adr/` and use the next one, zero-padded to four digits.
2. Create `docs/adr/NNNN-<kebab-case-title>.md` from `docs/adr/0000-template.md` with
   `Status: Proposed` and today's date.
3. Fill Context (3 to 5 lines), Decision (one or two sentences), at least two Alternatives considered (each
   with why not) and Consequences (positive and accepted trade-offs), using what we discussed. Ask me for
   anything missing. If no real alternative exists, tell me the decision probably does not need an ADR.
4. If it replaces an earlier ADR, set the old one to `Superseded by NNNN`.
5. Show me the ADR and wait. When I approve, set `Status: Accepted` and reference it from the related
   design doc's header.
