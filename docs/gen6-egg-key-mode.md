# XY / ORAS current egg Seed display and search mode

## Root cause

`Search6_Egg()` in both master and `codex/pkhex-export` constructs
`MersenneTwister(Seed.Value)` for a **future-frame search**. It uses the
user-provided `Key0` and `Key1` only once, for the `Current` egg
(`FrameNum=-1`). Every later row derives a *new* egg key from the
separate main MT stream, so changing the current egg's key does not
change those rows.

In **Masuda Method + Shiny Charm** mode, the PID of the current egg
*does* come from that egg's key. The top-level main Seed does not
participate in that PID generation; the user is correct about this.

## UX correction

On the Gen6 egg tab, editing `[0]` or `[1]` automatically checks
**仅计算当前蛋 Seed**. Clicking **计算** then produces just the current
unfiltered egg (frame `-1`), and a separate preview of PID, EC,
and shiny type. This avoids accidentally interpreting the unchanged
main-MT future-frame list as the result of the edited key.

To recover the master future-frame search, uncheck this box or change
the top-level main Seed. **Accept / Reject** options and the actual
`Egg6` generation algorithm remain unmodified.

This does not assume that a current egg key determines the future
main RNG stream. Predicting later eggs requires the independently
tracked main RNG state; a single current egg key cannot be treated as
a documented replacement for it.

## Regression A/B

The Windows test project now checks the production `Egg6` with
Masuda Method + Shiny Charm and screenshot-style Destiny Knot /
Everstone settings (TSV 131):

- `[1]=11111111, [0]=22222222` => PID `D8CDD888`, EC `0AD2D21A`.
- `[1]=33333333, [0]=44444444` => PID `BCE5CFB2`, EC `CAAC136A`.
- Keeping the egg key fixed while changing the main MT Seed from
  `00000000` to `DEADBEEF` must leave the egg's PID, EC and IVs
  unchanged.

The CI builds **.NET 10**, explicitly identifies the csproj to avoid
MSB1011 from the sibling sln and slnx, and runs xUnit regression tests.
UI interactive testing against the actual exe still needs a separate
Windows UI test; automated RNG tests do not prove that GUI input works.

## UI result invalidation and packaging

After a Gen6 egg-key edit, the UI now discards the previous result grid rather
than leaving unrelated main-MT hits visible. The compact checkbox beside the
seed is the explicit **current egg** versus **future main-MT search** switch.
A fresh calculation in current-egg mode shows only frame `-1`; future-frame
results will not be misrepresented as derived from the edited key.

The release workflow must build and test the actual branch commit on each push.
Do **not** download a hard-coded older CI run's artifact and publish it under
the latest branch name. Each beta release is identified by its source SHA.

## 2026-11-08 follow-up: is the next egg derived from the current egg key?

In XY/ORAS the current egg's stored 64-bit Key gives its already-fixed
properties (and the PID when MM / Shiny Charm are active). Accepting or
rejecting that egg generates the *next* egg's Key from the main MT stream at
the time of the interaction. The current Key is not a reversible or complete
representation of that separate MT state.

- Retail reference: https://www.pokemonrng.com/retail-xy-egg-mmsc/
- Retail reference: https://www.pokemonrng.com/retail-oras-egg-no-mmsc/

The screenshot's top Seed is `00000000`; its current egg Key is
`[1]=FECED4DB, [0]=F5D266F8`. The added production-path regression
asserts that changing the Key but keeping main Seed fixed does **not**
alter the later frame candidates; changing main Seed **does** change them.
This guards against producing a plausible but game-incorrect "next key"
by repeatedly seeding `Egg6` from its previous per-egg key.

The GUI no longer automatically checks **仅查看当前蛋** after a Key edit.
The chosen view is preserved and the old rows are invalidated. By default
recalculation shows current row and future main-MT candidates together,
with tooltip text showing the distinct data sources. Current-only view is
still available by explicit checkbox selection.
