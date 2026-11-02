# WoW display findings (field tests)

Measured on the owner's PC, 2026-10-27: Classic Era 2026 (`WowClassic.exe`, class `waApplication Window`),
3 clients, monitor 1 2560×1440 (work area 2560×1392, 100 %), monitor 2 1920×1080. Checked with `wbx info`
(exact sizes) and `wbx snap` (PrintWindow pictures of each client). Markers as in the research doc.

## Config.wtf
- All clients started from one install share one `WTF\Config.wtf`, so per-client window sizes cannot be set there.
  The client rewrites it on exit. We only read it [verified].
- Owner's file: `gxMaximize "0"` (plain Windowed), `RenderScale "1"`, `gxWindowedResolution "870x696"` (the last
  windowed size WoW saved), no `gxWindow` line, no `uiScale` in any account config → automatic UI scale [verified].
- `wbx diagnose` lists these cvars.

## Action bars "cut off"
- **The UI is always 768 units tall, and Classic Era's main action bar (with the gryphons) is 1024 units wide.**
  The whole bar is visible only when the game area is **4:3 (1.333) or wider** [verified with snapshots]:
  - 2558×1360 (1.88): complete bar, centred.
  - 1751×1360 (1.29) and 868×664 (1.31): gryphon ends clipped, all buttons visible.
  - 830×664 (1.25, owner's followers, 2026-10-27 evening): no left gryphon, starts at button 1, cut in the middle of
    the bag slots on the right (owner's screenshot `04_Input/screenshots/actionbar-follower-2026-10-27.png` in the
    project folder = our `wbx snap`) – the "before" for the UI Scale fix.
  - 805×664 (1.21): left gryphon gone, backpack clipped on the right.
  - 886×664 (1.334, follower 3 widened for a minute): whole bar with both gryphon tips [verified with snapshot].
- Below 4:3 the bar is clipped equally on both sides. That is geometry, not a resize bug. It cannot be fixed by
  moving windows, only by giving the client a wider area (or a UI addon that shrinks the bar).
- To see the full bar in a slot, its *game area* (client area, without the title bar) must be ≥ 4:3.
  Use `--min-aspect 1.334`, or arrange the windows wider.

## Black bars (the 5:4 rule)
- 1690×1392 (1.21) showed **no** black bars in a snapshot [verified]. So the launcher's 5:4 "minimum aspect" is not
  a hard WoW limit for Classic Era 2026. It stays as an optional rule ("Window rules" in the app) and as a warning
  colour, not as a claim of letterboxing.

## Title bar vs borderless
- The owner saw the UI drawn at the wrong size after the PowerShell launcher removed title bars. With title bars
  kept (placed by their visible edges), the result looked right to him [verified by the owner].
- Later, `wbx arrange --borderless` (870×696) looked consistent in snapshots: the UI scaled with the height
  (minimap ≈ 20 % of the height in every client) [verified with snapshots]. So the launcher's problem probably came
  from removing the frame without a real size change, so WoW never re-laid-out its UI [derived, not reproduced].
- Default stays **title bars on** (the owner's confirmed setup). `--borderless` is available.
- The 2 px resize nudge does not fix anything [verified by the owner]. It stays on (harmless) until we can remove it
  after more field data.
- 2026-11-02: ClientLayout-style popup chrome (`WS_POPUP`, client-sized place, 1 px width nudge) lands in
  `WindowPlacer` so borderless no longer leaves an overlapped frame. Setup gains a **Remove title bars** toggle
  (same `Borderless` flag as Settings). Default stays **off**.

## Where UI scale lives – answered (2026-10-27, Forever client started and closed by us)
Test on our own Forever client (`_forever_\Wow.exe` 1.60.1, not running for the owner): Config.wtf backed up →
`SET useUiScale "1"` and `SET uiScale "0.64"` added → client started (login screen) → closed normally → **WoW rewrote
Config.wtf on exit (new time, lines in a new order) and kept both lines** [verified]. A CVar stored per account would
have been dropped from the shared file. → UI Scale lives in the **shared `WTF\Config.wtf`**: one value for every
client of an install, rewritten by every client when it closes. The original file was restored byte for byte (same
SHA-256). The login screen itself ignores UI Scale (buttons the same size), so the effect on the action bar still needs
a logged-in client: Classic Era 2026 is the same engine, but this is [derived] until the owner restarts their clients.

Consequences for the app:
- The fix is one value for the whole squad: the smallest any window needs (rule below). Owner: main 1726×1360 needs
  ≤ 0.95, followers 830×664 need ≤ 0.93 → **0.93**.
- Written only while **no** client of that install runs (otherwise the last client to close overwrites it), only the
  two lines, with a one-time backup `Config.wtf.wowboxer-backup`. "UI Scale off" sets `useUiScale "0"`.
- The action-bar mark uses (game aspect ÷ UI Scale); "buttons clipped" below 1.26, "gryphons clipped" up to 4:3 (main 1726×1360 = 1.269: every button visible).

### Earlier read-only check (same day)
- Shared `WTF\Config.wtf`: no `uiScale`/`useUiScale` line. Every `WTF\Account\<ACC>\config-cache.wtf`: none either.
  → UI Scale has never been changed on this PC, so its storage place is unproven [AFKLAR: change it once in game on
  one follower and look which file changes].
- Each account has `edit-mode-cache-account.txt` (Classic Era 2026 has Edit Mode); all hold only defaults. Format
  undocumented → the app never writes it. Edit Mode's action-bar scale is an in-game, per-account alternative.
- Derived rule (not yet verified in game): the 1024-unit bar fits when UI Scale ≤ 0.75 × game aspect
  (1.21 → 0.90, 1.31 → 0.95).
- Command lines of all clients are identical (`-launcherlogin -uid wow_classic_era -initialgamemode=cera`), so a window
  cannot be mapped to its account from outside. Starting `WowClassic.exe` directly opens WoW's login screen
  (`docs/launch-research.md`).

## Still to test
- Display Mode "Windowed (Fullscreen)" (`gxMaximize 1`), as WowOpenBox recommends: can it be resized from
  outside, and does it behave better? Needs one in-game setting change by the owner.
- Forever in game (4–5 Nov): action bar, Edit Mode, the same checks after login.

## Forever client (2026-10-27, login screen; name reservation only)
- Install folder `World of Warcraft\_forever_`, exe **`Wow.exe`** (the same name as Retail – so the game is told by
  the folder, not the exe), window class **`waApplication Window`**, title "World of Warcraft", version 1.60.1 (70634).
- Command line when we start it directly: just the exe path. `.flavor.info` = `wowf`, `agentUID "wowf"`.
- Default display mode **Windowed (Fullscreen)**: `GxMaximize "1"`, style `0x95000000` (popup, no frame), 2560×1440.
  `wbx arrange --game forever` moved it, but the client maximised itself again (2560×1392) → it can only be placed
  after Display Mode = Windowed (as for Classic). Not elevated; moving is allowed.
- Forever and Classic Era windows can be open at the same time; with v0.2.2 each game's layout only takes its own
  windows (`wbx info` prints the game per window).
