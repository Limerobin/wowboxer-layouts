# Changelog

## Unreleased
- **Remove title bars on Setup** (Your squad, above Hover): same layout option as Settings › Window rules; applies
  at once. Borderless chrome now uses a real popup frame and client-sized placement so mouse/UI stay aligned.
  Default remains off.

## v0.6.0 – Now simply called WoWBoxer (2026-10-31)
- **The app is now called "WoWBoxer"** everywhere you see it: title bar, About, welcome screen, What's new, the tray,
  Diagnose and Report a problem, the installer, the Start menu and desktop shortcuts and Windows' Apps list. Where app
  and addon need telling apart: "WoWBoxer" (the app) and "WoWBoxer AddOn".
- **The exe is `WoWBoxer.exe`** (was `WoWBoxer.Layouts.exe`). Updating from 0.5.x moves everything along: Velopack
  points every shortcut into the install folder at the new exe – the Start menu, desktop and taskbar ones and your own
  layout shortcuts, with their `--start` arguments – and "Update now" opens the renamed app with the same arguments.
  Tested with a throwaway package before release (title and exe renamed in one update).
- Unchanged, so nothing breaks: the install folder and Velopack id (`WoWBoxer.Layouts`), settings, layouts and logs
  folders, the `wbx` command line, the repository and the code's names. The old launcher
  `WoWBoxer Window Layouts.exe` stays in the install folder; it still opens the app.
- **Website screenshots, always current.** `--demo` opens the app with made-up monitors (2560×1440 + 1920×1080) and a
  made-up squad; nothing real on the PC is read or changed. `--screenshots FOLDER` renders every screen (Setup with 3
  clients, 2 monitors + Free for all, 10 clients in two parties, window details, Settings, Help, About, the tray menu,
  the welcome screen) and `tools/make-screenshots.ps1` turns them into WebP (1440 and 720 wide) plus `manifest.json`.
  The set lives in `docs/screenshots/site/`; CI renders it on every push, and every release gets `site-screenshots.zip`
  built from its own version.

## v0.5.1 – A Help tab (2026-10-30)
- **New tab: HELP** (between Settings and About; Ctrl+4 or F1). Left: *Get help* – Report a problem, and the Discord,
  YouTube and website links – and *See what the app sees* – Identify all windows, Diagnose, Open logs, Give title
  bars back. Right: *Common problems*, each with its fix (stopped app, admin rights, windows jumping back, cut-off
  action bar, Windowed (Fullscreen), wrong character, ignored windows, hover too quick, unplugged monitor, SmartScreen,
  idle disconnects).
- Settings › Tools keeps the setup tools (Identify, Close clients I started, title bars, layouts folder, welcome
  screen); Diagnose, Open logs and Report a problem moved to Help. Guide › If something looks wrong has an "Open Help"
  button; the tray's Report a problem opens Help first. About is Ctrl+5 now.

## v0.5.0 – Community links and Report a problem (2026-10-30)
- **Community links, small and always there:** Discord, YouTube and website icons in the status bar, the same with
  labels in About, and one line – "Join the WoWBoxer Discord – tips, layouts and help" – on the welcome screen and at
  the bottom of What's new. Never a pop-up, reminder or badge.
- **Links in one place:** the app reads `https://wowboxer.com/links.json` at most once a day (cached on the PC;
  format `wowboxer-links/1`, see `Core/CommunityLinks.cs`) and uses the built-in links until then or when it cannot be
  read. Only https links to Discord, YouTube and wowboxer.com are accepted. It follows the "Check for updates" switch,
  so that switch off means no network at all. docs/privacy.md explains it.
- **Report a problem** (Guide › If something looks wrong, Settings › Tools, tray menu): builds a report – version,
  Windows, layout, clients, monitors, each slot's status, action bar / UI Scale, hover setting, last 50 log lines –
  with the Windows user folder and name, WoW account and character names hidden. You see and can edit it, add "What
  happened?", and **Post in Discord** copies it as one Discord message (a code block, shortened to fit 2,000
  characters by dropping the oldest log lines), saves the full report (plus a read-only Diagnose that moves no window)
  as a .txt in the logs folder, and opens the invite to `#setup-help`: "Paste with Ctrl+V in #setup-help and press
  Enter." Nothing is ever posted by the app.
- About no longer links to the source code on GitHub (no GitHub links in the app); the README still does.
- Unit tests for the links file (overrides, broken files, unsafe links), redaction (a fake user name, accounts,
  characters, whole words only) and the Discord message size.

## v0.4.2 – The layout cards are back (2026-10-30)
- **Pick a layout with the cards again** (owner's preference): Main + side, Overlays, Grid and Free for all side by
  side, each with its live preview for the current clients and monitors – one click switches. The dropdown from v0.4.0
  is gone; the Monitors section stays right below the cards, and the chosen layout's short description under them.

## v0.4.1 – What's new fits the screen (2026-10-30)
- **What's new is smaller and scrolls.** It lists only the six newest changes (the rest is here in the CHANGELOG),
  never grows taller than the screen (at most 640 px), and the list scrolls while the title and **Continue** always stay
  in view. Before, the long list pushed Continue off the bottom of the screen (owner's report on v0.4.0). Enter or Esc
  also closes it.

## v0.4.0 – Up to 10 clients; layout and monitors as two clear choices (2026-10-30)
- **1–10 clients.** The number buttons go to 10 (two rows of five): clients 1–5 are party 1, 6–10 party 2, with
  client 6 as party 2's main. Your squad groups its rows as "Party 1" and "Party 2" above 5 clients; dragging a row into
  the other party works as before (rows, map and real windows follow).
- **Layout is a dropdown.** Section II is now one compact dropdown – Main + side, Overlays, Grid, Free for all – each
  entry with a live mini preview; the chosen layout's short description sits under it.
- **New section: Monitors** (right below the layout, only with 2+ monitors): "1 monitor", "2 monitors" … up to the
  number Windows reports, each showing which ones ("Primary + right · 2560×1440 + 1920×1080"). With fewer than all,
  "Which ones" picks them (default: the primary first, then left to right). Default: 1 monitor, the primary.
- **Your screens shows exactly the chosen monitors**, numbered 1, 2, 3 … in their real arrangement and scale, and glides
  when the layout, monitors or client count change.
- **Every layout works across the chosen monitors:**
  - *Main + side*: up to 5 clients on 1 monitor as before; on 2+ monitors the main fills monitor 1 and the followers
    spread over the others (this replaces the separate "2 monitors" layout – older layouts made with it open as Main +
    side on two monitors). 6–10 clients: one Main + side block per party – one per monitor, or the two halves of one
    monitor (side by side or stacked, whichever gives the bigger game windows; **Flip** turns it).
  - *Overlays*: main on monitor 1 with the followers floating along the bottom, or on monitor 2 ("Followers on monitor
    2"). 6–10 clients: two rows on one monitor, or one row per monitor with party 2's main under its row.
  - *Grid*: tiles spread evenly over the chosen monitors.
  - *Free for all*: new windows start spread over the chosen monitors; drag a window anywhere, also across a monitor
    border – it lands on the monitor holding its centre and snaps to each monitor's edges.
- **The action-bar verdict for the whole layout** sits under the layout ("The whole action bar fits in 7 of 10
  windows …"). With two parties on one monitor it suggests using 2 monitors.
- **Saved layouts remember their monitors** (by Windows' device ID, with place and size as a fallback when IDs change).
  When monitors are plugged in or out the list refreshes; if a monitor of the layout is gone, it goes back to 1
  monitor with a short notice (Ctrl+Z, or choose the monitors again when it is back). Share codes leave the device IDs
  out.
- **Updates:** when the app opens and you already have the newest version, the activity log now says so ("You have
  the newest version (0.4.0)."), so a quiet start is not a mystery. When a newer one is out, it asks "Update now /
  Later" as since v0.3.5 (that worked: 0.3.5 → 0.3.6 on the owner's PC; there was simply nothing newer to offer after).
- Guide › chapter 2, README and the plan explain it. CLI: `--clients` 1–10, `--preset free`.
- Unit tests: every layout × 1–3 monitors (incl. three mixed-DPI monitors numbered out of order) × 1–10 clients, plus
  parties, split, monitor matching and fallback (2,338 cases). `tools/ui-sweep.ps1` covers the new controls.
- `docs/test-plan.md`: repaired characters that an earlier edit had double-encoded ("âœ…" → "✅").

## v0.3.6 – update test (2026-10-29)
A small release to check that v0.3.5 asks "Update now / Later" when it opens. No other changes.

## v0.3.5 – Hover starts at 30 ms, off on quit; the app asks to update when it opens (2026-10-29)
- **Hover to activate starts at 30 ms.** Turning the switch on (Setup › Your squad, Settings › Switching windows, the
  tray or the Guide) now writes a 30 ms delay together with "on", whatever delay Windows had before; the slider jumps
  to 30 ms with it. The slider stays free (0–500 ms) and still applies when you let go.
- **Reset to default** next to the delay slider (Setup and Settings) sets the delay back to 30 ms at once, with a
  "Default: 30 ms" label under the slider (the mark on the slider is the same 30 ms). It is separate from "Restore my
  original Windows settings", which still puts back your own values from before you first used the switch.
- **Quitting turns hover to activate off.** Closing the app (✕, tray › Quit, signing out of Windows) switches it off
  again; the delay stays for next time. Settings › Switching windows › "Turn hover to activate off when WoWBoxer quits"
  (on by default). "Restore them when WoWBoxer quits" still puts back your own values instead.
- **The app asks to update when it opens.** If a new version is out, a box asks "Update now" or "Later". Update now
  downloads it, closes the app and opens the new version (What's new shows what changed); Later installs it when you
  quit. Before, every update was only installed on quit without a word, so the app opened one version behind. A tray
  start (no window) still installs on quit. This version still arrives the old way; from the next one on you are asked.
- Guide › Switch between windows explains the default, Reset and quitting.

## v0.3.4 – Hover to activate is the WoWBoxer way to switch (2026-10-29)
- **Hover to activate is the WoWBoxer way to switch windows** (owner's decision). The old notices next to the switch
  (Setup › Your squad, Settings › Switching windows) are gone, and so is the advice about other ways to switch
  windows (Guide, What's new, README, known issues). In their place, the facts: hover to activate is a Windows setting; it
  sends nothing to the game; WoWBoxer never sends keys or clicks.
- Behaviour is unchanged: off by default, only changed on your click, your own Windows values kept and restored
  (Settings › Restore, on quit if chosen, always on uninstall).
- **Website links now open https://wowboxer.com/** (the site's new domain) instead of the Vercel address: the Guide,
  the first-run screen, About and the rules link next to the switch.
  The README links to the website, and the GitHub repos list it as their website.

## v0.3.3 – Fix: the app did not start (2026-10-28)
- **v0.3.2 closed right away on start.** The Settings screen used the tooltip box style that only the Setup screen
  defined, so the main window could not be built. The style now lives in the shared theme. Installed v0.3.2 cannot
  update itself (it never gets far enough to check), so install this version with WoWBoxer-Setup.exe.
- CI and the release now start the built app and fail if it does not open its main window (`tools/smoke-start.ps1`),
  so a start-up crash like this cannot be released again.

## v0.3.2 – Start/Stop and hover to activate (2026-10-28)
Released at the owner's request before the live test on real WoW windows (needs the PC idle). Hover to activate
is off by default; its Windows values are unit-tested only so far.
- **Start / Stop placing windows.** The app opens **stopped** and leaves every window alone until you press **Start
  placing windows** (the main button while stopped). While started, a **■ Stop** button sits next to it; the tray menu
  has the same Start/Stop. Found by the owner: opening the app during a Retail session moved the Retail windows into
  the layout. Desktop layout shortcuts (`--start`) and `--resume` start at once; Settings › Windows › "Start placing
  windows when the app opens" (off by default) brings back the old behaviour. Opening clients from the app also starts.
- **Hover to activate, built in:** a switch for Windows' own "Activate a window by hovering over it with the mouse"
  (Ease of Access) in Your squad on the Setup tab, in Settings › Switching windows, in the tray menu and in the
  Guide's "Switch between windows" chapter (it replaces the button that opened the Windows setting). With it: a
  **Delay** slider (0–500 ms, 30 ms marked as recommended, written when you let go) and **Bring window to front when
  activated** (off keeps floating overlays visible; best with "Always on top"). Per user, no admin
  (`SystemParametersInfo` with `SPIF_UPDATEINIFILE | SPIF_SENDCHANGE`, so it is saved and applies at once). Changes
  made outside the app show up within a second. Off by default; the app never changes it on its own.
- **Your own values are kept:** the first change saves your Windows values (on/off, delay, bring to front).
  Settings › "Restore my original Windows settings" puts them back; "Restore them when WoWBoxer quits" (off by
  default) does it on quit; uninstalling always does it (Velopack uninstall hook).
- Notice next to the switch (WoW tooltip style) explaining that hover to activate is a Windows feature, not part of
  WoW, with links to the Guide chapter and the website's rules page. (Replaced in v0.3.4 by a short factual line.)
- Website links point to the live site (`wowboxer-site.vercel.app`) instead of a placeholder.
- New app-wide slider style (gold track, marked values).

## v0.3.1 – update test (2026-10-28)
A small release to check that installed copies of v0.3.0 update themselves.
- Docs: the release file list in `docs/releasing.md` matches what a release really contains.

## v0.3.0 – first public release (2026-10-28)
The first public release. Same app as v0.2.4 (plug and play), now installable from GitHub Releases with auto-update.
- **Install:** `WoWBoxer-Setup.exe` (stable link: `/releases/latest/download/WoWBoxer-Setup.exe`). Installs per user
  without admin rights and fetches the .NET 10 Desktop Runtime from Microsoft if it is missing. Also
  `WoWBoxer-Portable.zip` and `SHA256SUMS.txt`.
- **Updates:** installed copies update themselves from GitHub Releases (applied when you quit). A beta channel for
  prereleases exists; stable installs never see betas.
- **Open source:** README, CONTRIBUTING, SECURITY, CODE_OF_CONDUCT, issue/PR templates, updated privacy notes,
  code signing policy (SignPath Foundation application prepared; this release is unsigned).
- **Release pipeline:** notes from this file, stable file names, checksums, draft-then-publish, website notification.
- Highlights since the first builds (details below): layouts for 1–8 clients on one or more monitors, a to-scale
  monitor map, automatic placement of every new WoW window, Auto-fit action bar for Classic Era, always-on-top per
  window, layout sharing (`WBX1:` codes), tray, desktop shortcuts, Diagnose, and the `wbx` command line.

## Earlier builds (private)

### v0.2.4 – plug and play (2026-10-27)
Owner: "it works well, but too much is going on". Before/after: `docs/screenshots/setup-before-v0.2.4.png`,
`setup-after-v0.2.4.png` (+ `-details`, `settings-after-v0.2.4.png`).
- **Always watching:** the app always waits for WoW windows and puts each new one in the next free slot (placed again
  for 30 s while WoW loads); a closed window frees its slot, the others keep theirs; a window that already sits in a
  slot keeps it. The "Move WoW windows live" switch, "Apply to open windows", "Start squad", "Stop watching" and the
  start checklist are gone. **Pause watching** lives in the tray menu (and `--pause` / `--resume`, also sent to a running
  copy); while paused nothing is moved or kept on top.
- **Status instead of steps:** each slot on the map has a ring round its number (pulsing = Awaiting…, green =
  Connected, amber = Placing… / Moved) and a label; each squad row has the same chip. Title bar: "Awaiting clients ·
  1 of 3 connected" → "All 3 connected" (dot: pulsing gold / green / amber / grey when paused).
- **One primary button:** "OPEN N WOW CLIENTS" (login screens for the empty slots only, from the open clients' install,
  else the last used one) → "AWAITING CLIENTS 1/3" → "RE-ARRANGE" (snap everything back, also F5). "Open Battle.net
  instead" link next to it. Undo/Reset are icon buttons; "Saved automatically as …" stays.
- **No game selector:** every WoW install is treated the same (Classic Era, Forever, Retail, PTR …), found by its install
  folder. With windows of several installs open the squad follows the install with the most windows (tie: it stays
  with the one it follows; at the start the one opened first); the others are left alone with a notice "1 Forever
  window ignored · Include". Layouts no longer belong to a game. UI Scale, Auto-fit and account hints follow the
  squad's install.
- **Less on screen:** "Full action bar" switch gone (Auto-fit action bar does it, on by default); the map shows the
  monitors in use; Window rules, Action bar (UI Scale) and Tools (Identify all, Close clients I started, title bars
  back, Diagnose) moved to Settings; Saved layout is a compact dropdown with Save as… and ⋯ (Rename, Delete, share,
  import, capture); the help line is a "?" tooltip; the activity log opens from the status bar.
- **Squad rows:** one line per window – drag handle, number in the class colour, name (optional), pin, status chip,
  ▾. The ▾ details hold class, WoW account (+ last-played hint), Always on top, Identify, X/Y/W/H, Keep size and the
  action-bar line.
- **1 client = normal play:** the client numbers now go 1–8. Picking 1 makes one window fill the main monitor (no multiboxing) without closing the app; other open clients are left where they are. Back to 3 (or Undo) and the squad returns.
- Typing a name in a squad row no longer shows the faded placeholder ("Main", "Follower 1") under the text until it is saved.
- Changing a name, class or "on top" no longer re-arranges every window (only a change of the windows' places does).
- Fixes found while testing: an open row closed by itself after a moment (a focus style trigger undid the selection –
  now a focus handler keeps it); a window closed while being placed was reported as "WoW runs as administrator";
  extra windows were named after the squad's game.
- New tools: `tools/plug-and-play-check.ps1` (19 checks with fake WoW windows on monitor 2, `tools/fakewow/FakeWow.cs`);
  `ui-sweep.ps1` now needs the app started with `--pause`.

### v0.2.3 – owner's test of v0.2.2 fixed (2026-10-27)
- **One main screen:** Squad is merged into Setup – clients, layout, saved layout and window rules on the left, the map
  in the middle, "Your squad" on the right (rows per window: number, name, class, account, status, pin, Identify, drag
  ⋮⋮ to reorder). Selecting a row selects the window on the map and opens its details inline (X/Y/W/H, Always on top,
  Keep size, action bar). Start with Battle.net / Login screens, Apply and Start squad in the footer; tools, start
  checklist and activity at the bottom of the squad panel. Tabs: Setup · Guide · Settings · About.
- **Auto-fit action bar** (default on, Settings and the action-bar box): whenever no client of the install runs – app
  start, before Start squad, when the last client closes, or after a layout change – the UI Scale the layout needs is
  written (backup kept), with a notice on the map and a line in the activity log. "Fix action bar" stays as the
  manual fallback; the UI explains why it only works while the clients are closed. Confirmed in game: UI Scale 0.93
  shows the whole bar in the owner's 830×664 followers. Classic Era only for now (Forever's UI is not verified;
  the button works there).
- **Always on top, reliable:** applies instantly with live on or off; the open-window list now refreshes every 2 s
  app-wide (it only did on the Squad tab); Start squad's slot mapping drives the rows; a passive guard (WinEvent hooks:
  foreground, location, reorder – listening only) re-asserts on-top when anything changes it. A pin on the map and on
  each row shows the real state (red = being put right).
- Fixes found while testing: dragging a squad row did nothing in the new list (the list took the mouse); Undo after a
  row drag restored names but not which window/account was in which row; the guard logged our own changes as
  "set by something else"; "Unknown class" label shortened to "Unknown".
- `tools/topmost-check.ps1`: toggles with live off + 24 disturbance rounds (another program clears/sets on-top, window
  moved, foreground switched between our own windows).

### v0.2.2 – owner's test of v0.2.1 fixed (2026-10-27)
- **Always on top works both ways:** applied on every placement – also when the window is already in place (the main
  window never became topmost) – and cleared with HWND_NOTOPMOST when switched off (followers stayed on top). Also at
  app start and on every edit with live on. Checked on the real windows with `tools/topmost-check.ps1`.
- **Only the selected game is arranged:** each window's game comes from its install folder (`_classic_era_`,
  `_forever_` …; exe names are shared between games). Layouts remember their game (older files = Classic Era), each
  game remembers its last layout, and the WoW client/account hints follow the game. `wbx --game classic|forever`.
- **Forever client recorded:** `_forever_\Wow.exe`, class `waApplication Window`, starts in Windowed (Fullscreen)
  (`docs/wow-display-findings.md`).
- **Action bar, really solvable:** UI Scale is stored in the shared Config.wtf (verified on our own Forever client).
  "Fix action bar" writes the one UI Scale the squad needs (owner: 0.93) – only while all clients of that install are
  closed, with a backup – or shows the exact in-game steps. "UI Scale off" undoes it; "Shorten window instead" keeps
  the old 4:3 route. The action-bar mark is now measured on the open window (solid dot) instead of guessed (ring);
  "buttons clipped" starts below 1.26 (owner's screenshot; was 1.2). `wbx info`/`wbx snap` print the same verdict.
- **Visible version:** title bar and About show the version (was stuck at 0.1.0), "What's new" once after an update.
- Inspector: a window picker next to the header (keyboard and UI Automation friendly).

### v0.2.1 – "make it work" (unattended bug-hunt run)
- **Classes never guessed:** slots show "Unknown class" (neutral colour) until set. Name, class and WoW account are set
  per row on the Squad tab; the account list and a "last played here" hint (with Use) come read-only from the WTF
  folders. Plan for automatic classes via the WoWBoxer addon: `docs/classes-and-accounts.md`.
- **Drag and drop that stays in sync:** drag a Squad row by ⋮⋮ to move a character (name, class, account and its
  window) to another slot; drop a window onto another on the canvas to swap places. Rows, canvas and – with live on –
  the real windows always follow; Undo works for both.
- **Launch:** Start squad can open N WoW login screens (only the missing ones) instead of Battle.net; "Close clients I
  started" closes only those. Research in `docs/launch-research.md` (Battle.net auto-login is never used: it could
  disconnect a running client).
- **Action bar:** "Full action bar" layout switch, per-window "Fit action bar", in-game UI-scale hint (marked untested).
  UI-scale storage checked read-only (`docs/wow-display-findings.md`).
- **Fixes:** resize nudge left a window 2 px small (now off by default, re-places after); open windows already in a slot
  are matched to it ("Not in place" status); Start squad keeps the rows' order; second start with `--start NAME`
  reaches the running app; drag onto a text box did nothing; unnamed rows did not move real windows.
- **Testing:** `docs/test-plan.md` (full matrix with results), `tools/ui-sweep.ps1` (64 UI Automation checks, all
  passing), `tools/guarded-drag.ps1`; `docs/known-issues.md`, `docs/backlog.md`. 800 unit tests.

### v0.2 – "feels like a game client"
- **New look** after the website and the app mockup v0.2: custom "War Table" theme (navy panels with gold corner
  brackets, Cinzel headings, red/gold "Enter World" buttons, WoW-tooltip boxes, gold switches), custom title bar with
  the shield crest, tabs **Setup · Squad · Guide · Settings · About**, live status in the title bar, page transitions,
  glide animation when windows change place, new shield icon, installer splash and welcome screen.
- **Setup:** I. how many clients (2–8), II. layout cards with live previews of your monitors (Main + side, Overlays,
  Grid, 2 monitors, Free for all – one click applies), III. the canvas: the taskbar is drawn and labelled (the
  "unused strip" was the taskbar), "Cover the taskbar area" with an auto-hide check, only the monitors in use (or all),
  gold number medallions with an action-bar verdict per window, live move of open WoW windows, sticky footer
  (Undo · Reset to preset · saved automatically · Apply to open windows · Start squad), class colour per window.
- **Squad:** one row per character and its window (status Waiting → Placed, Identify, ▲▼ order), a live
  "Start your squad" checklist (Battle.net running, Play with account 1 … N), layout preview, activity log.
- **Guide:** quest-log chapters on a parchment page (prepare WoW, layouts, start, switching windows, troubleshooting,
  what it never does), a helper that opens the Windows hover setting, and a link to the website guide.
- **Exact sizes:** with title bars on, the aspect rule and the presets use the *game area* (window minus the 32 px
  caption/border, scaled with DPI), so "4:3" really gives the game a 4:3 picture.
- Core: Free-for-all preset, optional `class` per slot, `preset` per layout (for Reset); 797 tests.
- Fixes: opening the app never moves windows; the window being edited is drawn above its neighbours.
- Tested on the owner's PC with screenshots, UI Automation and a guarded real mouse drag on the app's own window.

### App (M2–M5)
- **WPF app** with the Windows 11 Fluent dark theme (Mica): monitors drawn to scale, colour-coded slots with size
  and aspect (green = 4:3+, full Classic action bar).
- **Editor:** drag to move, handles to resize, snapping to monitors and other slots (Alt = free), arrow-key nudge,
  Ctrl+arrows resize, Tab selection, numeric X/Y/W/H, always-on-top and keep-size per slot, undo (Ctrl+Z).
- **Layouts:** auto-saved in `%APPDATA%\WoWBoxer\Layouts\profiles`; new from preset, duplicate, rename, delete;
  `WBX1:` codes and `.json` files; capture the current windows.
- **Squad:** Start squad (Battle.net + watcher), Arrange now, reorderable window list, Identify (click-through
  numbers on each WoW window, never activated), give title bars back, Diagnose report, logs.
- **Tray icon**, single instance, `--tray` / `--start NAME`, desktop shortcut per layout, first-run screen.
- **Installer and auto-update** (Velopack, GitHub Releases): `release.yml` builds a 10 MB setup that installs the
  .NET 10 Desktop Runtime when missing. Unsigned until SignPath approves; see `docs/code-signing-policy.md`.
- `docs/privacy.md`: no telemetry; the update check is the only network call.
- Fix: the app's own process (`WoWBoxer.Layouts.exe` starts with "wow") was detected as a WoW client.
  Detection rules moved to Core with tests.
- Shared engine: `SquadPlan`, `Arranger`, `SquadWatcher`, `Diagnostics` in Win32, used by the CLI and the app.

### M1 follow-up
- `wbx snap` (PrintWindow pictures of each client, read-only) and Config.wtf display cvars in diagnose.
- Findings in `docs/wow-display-findings.md`: Classic's action bar is complete only at 4:3 or wider; 1.21 shows no
  black bars (the 5:4 rule is a soft guideline); Config.wtf is shared by all clients.

### M0–M1
- Repo created (private, MIT). Added CLAUDE.md, plan, research and the PowerShell prototype as reference.
- M0: solution skeleton (`WoWBoxer.Layouts.slnx`) with Core, Win32, Cli (`wbx`), App (WPF stub) and Core.Tests (xUnit);
  shared `Directory.Build.props` (nullable, warnings as errors), `global.json` (.NET 10 SDK),
  Per-Monitor-V2 / asInvoker manifests, GitHub Actions build + test on `windows-latest`.
- M1 Core: `wowboxer-layout/1` profile model + JSON, `WBX1:` share codes, validation; layout engine
  (% of work area → pixels, edge rounding, 5:4 shrink-anchor fit, per-slot fit override); presets
  `side`, `overlay`, `grid`, `dual` for 2–8 clients. `side` with 3 clients reproduces the launcher exactly on the
  reference setup (1690×1392 + 2× 870×696). 592 xUnit cases (presets × N × 1080p/1440p/21:9/4K/dual/mixed DPI).
- M1 Win32: port of the launcher's `FmWin` with `LibraryImport` (instead of CsWin32): game windows by class,
  `Wow*.exe` name and `\World of Warcraft\` path; monitors with work area and DPI; borderless; async no-activate
  placement; topmost for overlay slots; resize nudge; elevation check; oldest-first via `GetProcessTimes`.
- M1 CLI: `wbx start | arrange | diagnose | info | layout`. Start = 30 s re-apply per new window, one nudge,
  closed windows free their slot. Admin mismatch (UIPI) is explained, never auto-elevated (unlike the launcher).
  Logs and diagnose reports in `%LOCALAPPDATA%\WoWBoxer\Layouts\logs`, user profile path redacted.
- Field test (3 Classic Era clients, 2560×1392 work area): `wbx diagnose` and `arrange` find and place all
  windows exactly. Removing title bars makes WoW draw action bars/minimap at the wrong size; the 2 px resize
  nudge does not help. **Title bars are now kept by default**; `--borderless` opts in.
- Framed windows are placed by their visible edges (DWM extended frame bounds), so no gaps between them.
- New: `wbx capture --save FILE.json` saves the current arrangement (e.g. made with Windows Snap) as a layout;
  `wbx restore` gives windows their title bars back; `wbx info` shows the size the game renders at.
- Not ported: the launcher's `Shortcut` mode (the app's installer will make shortcuts in M5).
