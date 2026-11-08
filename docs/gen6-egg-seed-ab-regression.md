# Gen 6 egg Seed A/B investigation

The X/Y/OR/AS implementation in `master` and in this feature branch has
the same breeding PRNG and the same accept/reject frame search logic.

- `[0]` = low 32 bits and `[1]` = high 32 bits of a **single egg's key**.
- `Search6_Egg` uses this key for its **Current** row, `FrameNum=-1`.
  This row is added regardless of shiny/IV filters.
- The rest of the frame list uses a `MersenneTwister(Seed.Value)` object
  to generate each hypothetical **future** egg key. With the same top-level
  MT seed, changing only `[0]`/`[1]` does not change these future rows.
- The `接受`/`拒绝` controls alter the timing assumptions (`Timedelay`)
  and the main-RNG PID path, not the source of future egg keys.
- With neither Masuda Method nor Shiny Charm enabled, the *current egg's PID*
  cannot be determined from the 64-bit egg key alone. The UI uses a
  `0xFFFFFFFF` placeholder in the current row. Changing the key still
  changes the egg's EC and potentially IVs, nature, ability and inheritance.
  A placeholder must not be interpreted as a shiny prediction.

## Executable reproduction

On Windows: `dotnet run --project tests/Gen6EggSeedRegression/Gen6EggSeedRegression.csproj --configuration Release`

Uses production `Egg6` and `RNGPool` code to compare:
1. A: `[1]=11111111`, `[0]=22222222`.
2. B: `[1]=33333333`, `[0]=44444444`.
3. A repeated for deterministic equality.
4. The first eight future frames with a fixed top-level main MT seed of zero.
5. The same A/B seeds with both Masuda and Shiny Charm enabled.

Expected: A/B differ in current-egg EC and other attributes; A repeated is
identical; first eight main-MT frames are identical between A/B; Masuda+Charm
PID changes between A/B. The executable prints the actual values.

This test is source-level regression evidence, *not* game/emulator validation.
It intentionally does not invent a future-egg sequence from a single egg key.
