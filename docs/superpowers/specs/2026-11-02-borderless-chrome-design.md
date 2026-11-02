# Borderless chrome fix + Setup toggle

Date: 2026-11-02  
Status: approved for planning  
Upstream: [WoW-Multiboxing/wowboxer-layouts](https://github.com/WoW-Multiboxing/wowboxer-layouts)  
Fork: [Limerobin/wowboxer-layouts](https://github.com/Limerobin/wowboxer-layouts)  
Branch: `fix/borderless-chrome`

## Goal

Make “Remove title bars” safe by porting the ClientLayout borderless chrome approach into WoWBoxer, and expose that setting as a Setup-tab toggle above “Hover to activate windows”. Open a pull request upstream. Default stays **off** (title bars remain).

## Background

WoWBoxer already has a layout-scoped `Borderless` flag (Settings › Window rules › “Remove title bars”) and Win32 helpers `MakeBorderless` / `AddFrame` / `RestoreFrame`. Title bars stay on by default because earlier borderless attempts left an overlapped frame: WoW still measured that frame, so UI scale and mouse clicks could drift.

[ClientLayout](https://github.com/Limerobin/client-layout) fixed this by switching to a real popup chrome (`WS_POPUP`), treating the slot as the **client** area, applying `SWP_FRAMECHANGED`, and nudging width by 1 px so the game rereads mouse bounds.

## Scope

**In**

- Harden `WindowPlacer` borderless / restore / place path with the ClientLayout chrome contract.
- Add a Setup toggle bound to the existing layout `Borderless` flag.
- Apply chrome immediately when the toggle flips; keep applying on Start / Re-arrange.
- Keep Settings checkbox and `wbx` / restore tooling on the same path.
- Tests + short doc/changelog notes.
- Fork branch and PR upstream.

**Out**

- Changing the default to borderless.
- Per-slot title-bar controls.
- Rewriting the whole placer pipeline.
- Broader ClientLayout feature parity (launch UX, etc.).

## Architecture

```
Setup toggle  ──┐
Settings checkbox ──┼──► layout Borderless flag ──► Place / MakeBorderless / RestoreFrame
CLI --borderless ───┘                              (Win32 chrome + client geometry)
Toggle flip ──────────────────────────────► apply chrome now to mapped WoW windows
```

Single source of truth: the existing layout `Borderless` property. Setup and Settings are two views of it.

## UI & behavior

### Setup toggle

- Location: Your squad column, directly above “Hover to activate windows” (same column the user marked).
- Control: same switch style as Hover (`Style="{StaticResource Switch}"`).
- Label: **Remove title bars** (match Settings).
- Default: **off**.
- Short tip: off is safest; on removes the Windows frame so slots sit edge-to-edge; WoW must stay **Windowed**.
- Binding: existing layout `Borderless` → Settings stays in sync; value saves with the layout/profile as today.

### When flipped

| Action | Effect |
|---|---|
| On | Strip chrome on every currently mapped WoW window immediately (new safe path). Later Start / Re-arrange also place borderless. |
| Off | Restore frames immediately (same idea as “Give title bars back”). Later arranges leave frames on. |

Flipping does not require Re-arrange for chrome alone; Re-arrange still moves/resizes as usual.

### Settings / CLI

- Keep “Remove title bars” and “Give title bars back”.
- Settings explain text may note that chrome is applied as a true popup client (no leftover frame), without overclaiming.
- CLI `--borderless` and `wbx restore` continue to use the same Win32 helpers.

## Win32 chrome

Implement inside `WoWBoxer.Layouts.Win32` (`WindowPlacer` and related), adapting ClientLayout’s `ApplyChrome` / place path to WoWBoxer’s existing types and `SWP_NOACTIVATE` rules.

1. **Borderless style:** clear `WS_CAPTION | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX | WS_SYSMENU | WS_OVERLAPPEDWINDOW`; set `WS_POPUP | WS_VISIBLE | WS_CLIPSIBLINGS | WS_SYSMENU` (sysmenu without caption keeps Alt+Space without reintroducing a frame). Clear `WS_EX_DLGMODALFRAME | WS_EX_CLIENTEDGE | WS_EX_STATICEDGE | WS_EX_WINDOWEDGE`.
2. **Framed restore:** clear `WS_POPUP`; restore the existing `BorderStyles` set (`WS_CAPTION | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX | WS_SYSMENU`) via the current `AddFrame` / `RestoreFrame` path so framed behavior stays compatible with today’s non-borderless layouts. Do not introduce a second restore style vocabulary.
3. **Geometry:** slot rectangles describe the **visible client area**. Borderless place uses that size as the window rect. When restoring a frame while preserving client size, expand the outer rect with `AdjustWindowRectExForDpi` (fallback `AdjustWindowRectEx`).
4. **Apply:** `SetWindowPos` with `SWP_FRAMECHANGED` and `SWP_NOACTIVATE` (never steal focus).
5. **Nudge:** after borderless place, set width to `w+1` then back to `w` so WoW rereads mouse bounds. That is the only intentional size nudge for chrome; retire any older parallel nudge if it duplicates this.
6. **DWM (Windows 11):** set corner preference / border color so rounded corners do not look like a leftover frame (`DWMWA_WINDOW_CORNER_PREFERENCE`, `DWMWA_BORDER_COLOR`), matching ClientLayout.

### Failure

If style or position fails (e.g. elevated WoW / UIPI), use the same Diagnose / permission messaging as today. Prefer not leaving a half-applied borderless state when failure is detectable; “Give title bars back” / toggle off remains available.

## Testing

- Style contract: after borderless, style has `WS_POPUP` and no `WS_CAPTION` / `WS_THICKFRAME`; after restore, no popup and caption present (unit or sandbox tests in the style the Win32 project already uses).
- Geometry helper: framed expand via `AdjustWindowRectEx*` for a known client rect (extract a testable helper if needed).
- Update any existing arrange / borderless CLI tests for the new style contract.
- Manual smoke (Classic and Forever where available): toggle on → no title bar, clicks align; toggle off → bars back; Setup ↔ Settings sync; Start / Re-arrange with borderless on.

## Docs

- Changelog entry for the PR.
- Short update to `docs/wow-display-findings.md`: borderless is safe when the slot is the client area and popup chrome is applied; Setup toggle; default still off.
- Settings explain text soft-update as above.

## PR plan

1. Work on fork branch `fix/borderless-chrome`.
2. Implement Win32 + Setup UI + tests + docs.
3. Open PR to `WoW-Multiboxing/wowboxer-layouts`: Setup toggle + ClientLayout-derived chrome fix; default remains off; window styling only (no input).

## Success criteria

- With the toggle on, mapped WoW windows have no Windows title bar/frame, slot sizes match client area, and mouse clicks land correctly.
- Toggle off restores title bars immediately.
- Setup and Settings stay in sync; default for new layouts remains title bars on.
- Upstream PR is open from the fork branch.
