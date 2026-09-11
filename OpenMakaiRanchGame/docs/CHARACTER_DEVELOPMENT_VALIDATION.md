# Character development validation

Status when first staged: C# implementation and registered smoke tests are awaiting remote
build/runtime execution. Do not reuse prior PR #19 gameplay counts as proof for this change.

Local review-source run: 76 Python tests discovered, 74 passed and two original-source receipt
tests explicitly skipped because the review ZIP excludes the original ERA files. The existing
344-key UI locale validator also passed; 18 new non-UI English/German character keys are
separately checked. No local Godot or .NET run is claimed.

New C# tests cover source EP/MP separation and integer power arithmetic, baseline/query purity,
actual practice and workday effects, bounded rank/history storage, replay/high-water protection,
root SaveSlot/LoadSlot and explicit New Game+. The old resident daily-receipt assertion now
counts its own six receipt IDs rather than all unrelated character flags; its previous numeric
limit is unchanged, and the new development flags have their own bound assertion.

Remote exact-head results, limitations and artifact receipts will be recorded after execution.
