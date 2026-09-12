# Independent visual review prompt

Review the supplied approved target, current raw in-engine capture, prior capture/verdict when present, and their capture-context records. You are reviewing an implementation, not awarding an AAA certification. Do not assume a tool was run, an asset is licensed or a user approved a design unless the provided evidence establishes it.

First report whether the inputs are comparable: exact scene, camera transform/projection, viewport, renderer, quality, time/weather, facility state, identities and locale. Different code commits are expected. Missing or mismatched context is **not comparable**, not a low visual score. A UI diagram or historical baseline must not be mistaken for a generated approved target. Missing target approval stops target scoring.

For comparable views score composition/layout from 0–3, lighting/palette from 0–3, material/shape fidelity from 0–3 and meaningful finishing details from 0–1. For each gap, specify the image region/object, visible defect, likely source and an actionable correction. Distinguish measured facts from hypotheses. Do not penalize approved stylization as a lack of photorealism. Do not invent a score when you cannot inspect the images.

Check the user's normal green ranch direction, building/door consistency, unblocked paths, character identity consistency, face/hand readability, restrained mana lighting and ordinary sky/weather. For UI, testable text/controls must be separate from generated art. Flag lost functions, unreadable text, contradictory values, missing focus/disabled states or artwork concealing interaction targets. These are blocking usability concerns regardless of visual score.

A still image cannot validate motion, pathfinding, physics, consent/state transitions or performance. Request short real sequences for blink closure, gaze changes, hand contact, interruption and locomotion. Record hardware/resolution/frame-time evidence separately; do not infer FPS from visual complexity or a CI pass.

Return: input comparability; scores only when justified; prioritized concrete gaps; regressions; nonvisual blockers; recommended next experiment. Never recommend a screen-covering image to fake a 3D world, weakening tests, silently changing the target, or changing other agents' economy/clock/save/navigation ownership. If the same defect persists twice, identify the model/rig/layout strategy that should be reconsidered instead of prescribing endless bloom adjustments. This prompt must go to a genuinely separate reviewer; otherwise label the output a self-review.
