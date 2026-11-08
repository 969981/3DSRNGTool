# Gen 6 egg Seed: current-egg preview vs future-frame search

## Root cause

In X/Y/OR/AS egg RNG, the two fields `[0]` and `[1]` are the 64-bit
**per-egg key**. `Egg6.Generate(null, new uint[] { Key0.Value, Key1.Value })`
reconstructs the **current egg** from that key.

The **future-frame** search is different: `Search6_Egg()` creates a
`MersenneTwister(Seed.Value)`, then `Egg6.Generate()` consumes two main-MT
outputs as each hypothetical future egg's key. Editing `Key0`/`Key1` cannot
change that MT sequence. The egg key is not a replacement for the main Seed.

## UI behavior

- Edit either `[0]` or `[1]` on the Gen 6 egg tab: **只验当前蛋** is selected
  automatically. Click **计算** to get **one row, frame -1**, computed directly
  from the entered key, even when the IV/shiny filters would reject that egg.
  In the `Egg Seed` / 64-bit column the combined key must be
  `((ulong)Key1 << 32) | Key0`.
- Uncheck **只验当前蛋**: the prior main-MT future-frame search is preserved.
  Its results depend on the **top-level Seed**, not the current egg key.
- Edit the **top-level Seed** while in Gen 6 egg mode: the checkbox is cleared
  automatically to reflect future-frame search intent.
- Nothing about this change attempts to infer future egg keys from a current egg.
  That would require the game's main MT state/seed, which two 32-bit egg-key
  words alone do not provide.

## Manual A/B verification (Windows, X/Y)

Keep parent parameters, TSV/TRV and the top-level MT Seed constant.

| Case | [1] | [0] | Mode | Expected |
|---|---|---|---|---|
| A | 11111111 | 22222222 | Auto-select current-egg only | One `-1` row, egg key `1111111122222222` |
| B | 33333333 | 44444444 | Auto-select current-egg only | One `-1` row, egg key `3333333344444444`, independent egg attributes |
| C | 11111111 | 22222222 | Uncheck current-egg only | Main-MT results from top-level Seed |
| D | 33333333 | 44444444 | Uncheck current-egg only | Same future-frame rows as C |
| E | any | any | Change top-level Seed | Checkbox clears; future-frame results reflect new main MT |

Verify both shiny and non-shiny current eggs appear, rather than being
filtered out by the batch-search settings. If the game uses the main RNG
for PID (without Masuda Method/shiny charm), the current egg key is not
sufficient to determine that PID; the existing `0xFFFFFFFF` placeholder
is preserved for the current-egg row.

**Build verification:** `dotnet build --configuration Release` on Windows.
This is a UI regression checklist, not a claim of an emulator acceptance test.
