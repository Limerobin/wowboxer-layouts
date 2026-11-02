# Borderless Chrome + Setup Toggle Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix WoWBoxer borderless chrome using ClientLayout’s popup + client-rect approach, and expose “Remove title bars” as a Setup toggle (default off) that applies immediately and stays wired to the existing layout `Borderless` flag for an upstream PR.

**Architecture:** Extract pure chrome style/geometry helpers in Win32 (`WindowChrome`), teach `WindowPlacer` to use popup styles + client-as-slot sizing + a 1 px width nudge, bind a Setup switch to `MainViewModel.Borderless`, and on toggle apply `Place` to mapped windows even when paused. Settings / CLI keep the same flag and helpers.

**Tech Stack:** C# / .NET 10, WPF app, P/Invoke `user32`/`dwmapi`, xunit, existing `WoWBoxer.Layouts.{Core,Win32,App,Cli}` projects.

**Spec:** `docs/superpowers/specs/2026-11-02-borderless-chrome-design.md`

## Global Constraints

- Window styling only — never send input; always `SWP_NOACTIVATE` (and `SWP_ASYNCWINDOWPOS` where the placer already uses it).
- Default remains title bars on (`Borderless = false`).
- Single source of truth: layout `LayoutOptions.Borderless`.
- Restore framed style via existing `BorderStyles` bits; do not invent a second restore vocabulary.
- Warnings as errors; English UI/docs; changelog line under next version; no personal data in commits.
- Branch: `fix/borderless-chrome` on fork `Limerobin/wowboxer-layouts`; PR to `WoW-Multiboxing/wowboxer-layouts`.

## File structure

| File | Responsibility |
|---|---|
| `src/WoWBoxer.Layouts.Win32/WindowChrome.cs` | Pure style transforms + `ExpandOuterForClient` + DWM corner helpers |
| `src/WoWBoxer.Layouts.Win32/NativeMethods.cs` | Add missing WS_/DWM constants and `AdjustWindowRectEx*` / `DwmSetWindowAttribute` / `GetDpiForWindow` / `ClientToScreen` |
| `src/WoWBoxer.Layouts.Win32/WindowPlacer.cs` | Call `WindowChrome` from MakeBorderless/AddFrame/RestoreFrame/Place; 1 px chrome nudge |
| `tests/WoWBoxer.Layouts.Win32.Tests/*` | Unit tests for `WindowChrome` (net10.0-windows) |
| `WoWBoxer.Layouts.slnx` | Register Win32 test project |
| `src/WoWBoxer.Layouts.App/Views/SetupView.xaml` | Setup toggle above Hover |
| `src/WoWBoxer.Layouts.App/Views/SettingsView.xaml` | Soften explain text |
| `src/WoWBoxer.Layouts.App/ViewModels/MainViewModel.cs` | Immediate chrome apply on `Borderless` set |
| `src/WoWBoxer.Layouts.Core/LayoutProfile.cs` | Soften `Borderless` XML doc if it still claims UI always breaks |
| `docs/wow-display-findings.md` | Note popup-chrome fix; default still off |
| `CHANGELOG.md` | Next-version bullet |

---

### Task 1: WindowChrome helpers + Win32 tests

**Files:**
- Create: `src/WoWBoxer.Layouts.Win32/WindowChrome.cs`
- Modify: `src/WoWBoxer.Layouts.Win32/NativeMethods.cs`
- Create: `tests/WoWBoxer.Layouts.Win32.Tests/WoWBoxer.Layouts.Win32.Tests.csproj`
- Create: `tests/WoWBoxer.Layouts.Win32.Tests/WindowChromeTests.cs`
- Modify: `WoWBoxer.Layouts.slnx`

**Interfaces:**
- Consumes: existing `NativeMethods` style constants; new ones listed below
- Produces:
  - `WindowChrome.ToBorderless(int style, int exStyle) -> (int Style, int ExStyle)`
  - `WindowChrome.ToFramed(int style, int exStyle) -> (int Style, int ExStyle)`
  - `WindowChrome.ExpandOuterForClient(PixelRect client, int style, int exStyle, uint dpi) -> PixelRect`
  - `WindowChrome.ApplyCornerPreference(nint hwnd, bool borderless)`
  - Constants used by tests/placer: `WS_POPUP`, `WS_VISIBLE`, `WS_CLIPSIBLINGS`, `WS_OVERLAPPEDWINDOW`, ex-style edge flags, DWM attrs

- [ ] **Step 1: Add missing native constants and imports**

In `NativeMethods.cs`, add alongside existing `WS_*` constants:

```csharp
public const int WS_POPUP = unchecked((int)0x80000000);
public const int WS_VISIBLE = 0x10000000;
public const int WS_CLIPSIBLINGS = 0x04000000;
public const int WS_OVERLAPPEDWINDOW = WS_CAPTION | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX | WS_SYSMENU;
public const int WS_EX_DLGMODALFRAME = 0x00000001;
public const int WS_EX_CLIENTEDGE = 0x00000200;
public const int WS_EX_STATICEDGE = 0x00020000;
public const int WS_EX_WINDOWEDGE = 0x00000100;

public const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
public const int DWMWA_BORDER_COLOR = 34;
public const int DWMWCP_DEFAULT = 0;
public const int DWMWCP_DONOTROUND = 1;
public const int DWMWA_COLOR_NONE = unchecked((int)0xFFFFFFFE);
public const int DWMWA_COLOR_DEFAULT = unchecked((int)0xFFFFFFFF);
```

Add P/Invokes (LibraryImport, matching file style):

```csharp
[LibraryImport("user32.dll", SetLastError = true)]
[return: MarshalAs(UnmanagedType.Bool)]
public static partial bool AdjustWindowRectEx(ref RECT lpRect, int dwStyle, [MarshalAs(UnmanagedType.Bool)] bool bMenu, int dwExStyle);

[LibraryImport("user32.dll", SetLastError = true)]
[return: MarshalAs(UnmanagedType.Bool)]
public static partial bool AdjustWindowRectExForDpi(ref RECT lpRect, int dwStyle, [MarshalAs(UnmanagedType.Bool)] bool bMenu, int dwExStyle, uint dpi);

[LibraryImport("user32.dll")]
public static partial uint GetDpiForWindow(nint hwnd);

[LibraryImport("user32.dll")]
[return: MarshalAs(UnmanagedType.Bool)]
public static partial bool ClientToScreen(nint hWnd, ref POINT point);

[StructLayout(LayoutKind.Sequential)]
public struct POINT { public int X, Y; }

[LibraryImport("dwmapi.dll")]
public static partial int DwmSetWindowAttribute(nint hwnd, int attribute, ref int value, int size);
```

- [ ] **Step 2: Write failing Win32 tests project + tests**

Create `tests/WoWBoxer.Layouts.Win32.Tests/WoWBoxer.Layouts.Win32.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0-windows</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.4" />
  </ItemGroup>
  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\WoWBoxer.Layouts.Win32\WoWBoxer.Layouts.Win32.csproj" />
  </ItemGroup>
</Project>
```

Add the project under `/tests/` in `WoWBoxer.Layouts.slnx`.

Create `WindowChromeTests.cs`:

```csharp
using WoWBoxer.Layouts.Core;
using WoWBoxer.Layouts.Win32;
using static WoWBoxer.Layouts.Win32.NativeMethods;

namespace WoWBoxer.Layouts.Win32.Tests;

public class WindowChromeTests
{
    private const int Framed = WS_CAPTION | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX | WS_SYSMENU | WS_VISIBLE;

    [Fact]
    public void ToBorderless_sets_popup_and_clears_caption()
    {
        var (style, ex) = WindowChrome.ToBorderless(Framed, WS_EX_WINDOWEDGE);
        Assert.Equal(0, style & WS_CAPTION);
        Assert.Equal(0, style & WS_THICKFRAME);
        Assert.NotEqual(0, style & WS_POPUP);
        Assert.NotEqual(0, style & WS_SYSMENU);
        Assert.Equal(0, ex & WS_EX_WINDOWEDGE);
    }

    [Fact]
    public void ToFramed_clears_popup_and_restores_border_styles()
    {
        var (borderless, _) = WindowChrome.ToBorderless(Framed, 0);
        var (style, ex) = WindowChrome.ToFramed(borderless, 0);
        Assert.Equal(0, style & WS_POPUP);
        Assert.Equal(WS_CAPTION | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX | WS_SYSMENU,
            style & (WS_CAPTION | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX | WS_SYSMENU));
        Assert.NotEqual(0, ex & WS_EX_WINDOWEDGE);
    }

    [Fact]
    public void ExpandOuterForClient_grows_for_framed_styles()
    {
        var client = new PixelRect(100, 100, 800, 600);
        var style = Framed;
        var ex = WS_EX_WINDOWEDGE;
        var outer = WindowChrome.ExpandOuterForClient(client, style, ex, 96);
        Assert.True(outer.Width > client.Width);
        Assert.True(outer.Height > client.Height);
        Assert.True(outer.X <= client.X);
        Assert.True(outer.Y <= client.Y);
    }
}
```

- [ ] **Step 3: Run tests — expect fail (type missing)**

Run: `dotnet test tests/WoWBoxer.Layouts.Win32.Tests/WoWBoxer.Layouts.Win32.Tests.csproj -c Release`

Expected: FAIL — `WindowChrome` does not exist / compile error.

- [ ] **Step 4: Implement `WindowChrome`**

Create `src/WoWBoxer.Layouts.Win32/WindowChrome.cs`:

```csharp
using WoWBoxer.Layouts.Core;
using static WoWBoxer.Layouts.Win32.NativeMethods;

namespace WoWBoxer.Layouts.Win32;

/// <summary>
/// Style and outer-rect maths for removing/restoring the Windows frame around a WoW window.
/// Slot rectangles are the visible client area; borderless uses WS_POPUP so the window rect equals the client.
/// </summary>
public static class WindowChrome
{
    private const int BorderStyles = WS_CAPTION | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX | WS_SYSMENU;
    private const int EdgeExStyles = WS_EX_DLGMODALFRAME | WS_EX_CLIENTEDGE | WS_EX_STATICEDGE | WS_EX_WINDOWEDGE;

    public static (int Style, int ExStyle) ToBorderless(int style, int exStyle)
    {
        style &= ~(BorderStyles | WS_OVERLAPPEDWINDOW);
        style |= WS_POPUP | WS_VISIBLE | WS_CLIPSIBLINGS | WS_SYSMENU;
        exStyle &= ~EdgeExStyles;
        return (style, exStyle);
    }

    public static (int Style, int ExStyle) ToFramed(int style, int exStyle)
    {
        style &= ~WS_POPUP;
        style |= BorderStyles | WS_VISIBLE;
        exStyle |= WS_EX_WINDOWEDGE;
        return (style, exStyle);
    }

    public static PixelRect ExpandOuterForClient(PixelRect client, int style, int exStyle, uint dpi)
    {
        var rect = new RECT { Left = client.X, Top = client.Y, Right = client.Right, Bottom = client.Bottom };
        var ok = dpi != 0 && AdjustWindowRectExForDpi(ref rect, style, false, exStyle, dpi);
        if (!ok)
        {
            rect = new RECT { Left = client.X, Top = client.Y, Right = client.Right, Bottom = client.Bottom };
            if (!AdjustWindowRectEx(ref rect, style, false, exStyle)) return client;
        }
        return new PixelRect(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
    }

    public static void ApplyCornerPreference(nint hwnd, bool borderless)
    {
        int corner = borderless ? DWMWCP_DONOTROUND : DWMWCP_DEFAULT;
        int color = borderless ? DWMWA_COLOR_NONE : DWMWA_COLOR_DEFAULT;
        _ = DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref corner, sizeof(int));
        _ = DwmSetWindowAttribute(hwnd, DWMWA_BORDER_COLOR, ref color, sizeof(int));
    }
}
```

- [ ] **Step 5: Run tests — expect pass**

Run: `dotnet test tests/WoWBoxer.Layouts.Win32.Tests/WoWBoxer.Layouts.Win32.Tests.csproj -c Release`

Expected: PASS (all 3 facts).

- [ ] **Step 6: Commit**

```powershell
git add src/WoWBoxer.Layouts.Win32/NativeMethods.cs src/WoWBoxer.Layouts.Win32/WindowChrome.cs tests/WoWBoxer.Layouts.Win32.Tests WoWBoxer.Layouts.slnx
git commit -m "Add WindowChrome helpers and Win32 tests for borderless styles."
```

---

### Task 2: Wire WindowPlacer to popup chrome

**Files:**
- Modify: `src/WoWBoxer.Layouts.Win32/WindowPlacer.cs`
- Test: `tests/WoWBoxer.Layouts.Win32.Tests/WindowChromeTests.cs` (already green); full solution build

**Interfaces:**
- Consumes: `WindowChrome.ToBorderless`, `ToFramed`, `ApplyCornerPreference`; existing `WithInvisibleFrame`
- Produces: updated `MakeBorderless` / `AddFrame` / `RestoreFrame` / `Place` behavior (same public signatures)

- [ ] **Step 1: Update MakeBorderless / AddFrame / RestoreFrame**

Replace the bodies so they use `WindowChrome` and set both style and ex-style:

```csharp
public static bool MakeBorderless(nint h)
{
    if (Sandbox.Active) return false;
    var style = GetWindowLong(h, GWL_STYLE);
    var ex = GetWindowLong(h, GWL_EXSTYLE);
    var (nextStyle, nextEx) = WindowChrome.ToBorderless(style, ex);
    if (nextStyle == style && nextEx == ex) return false;
    SetWindowLong(h, GWL_STYLE, nextStyle);
    SetWindowLong(h, GWL_EXSTYLE, nextEx);
    WindowChrome.ApplyCornerPreference(h, borderless: true);
    return true;
}

public static bool AddFrame(nint h)
{
    if (Sandbox.Active) return false;
    var style = GetWindowLong(h, GWL_STYLE);
    var ex = GetWindowLong(h, GWL_EXSTYLE);
    var (nextStyle, nextEx) = WindowChrome.ToFramed(style, ex);
    if (nextStyle == style && nextEx == ex) return false;
    SetWindowLong(h, GWL_STYLE, nextStyle);
    SetWindowLong(h, GWL_EXSTYLE, nextEx);
    WindowChrome.ApplyCornerPreference(h, borderless: false);
    return true;
}

public static bool RestoreFrame(nint h)
{
    if (Sandbox.Active) return Sandbox.SetTopmost(h, false);
    if (!IsWindow(h)) return false;
    var style = GetWindowLong(h, GWL_STYLE);
    var ex = GetWindowLong(h, GWL_EXSTYLE);
    var (nextStyle, nextEx) = WindowChrome.ToFramed(style, ex);
    SetWindowLong(h, GWL_STYLE, nextStyle);
    SetWindowLong(h, GWL_EXSTYLE, nextEx);
    WindowChrome.ApplyCornerPreference(h, borderless: false);
    return SetWindowPos(h, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_FRAMECHANGED | SWP_NOOWNERZORDER | NoActivateAsync);
}
```

- [ ] **Step 2: After borderless Place, apply 1 px width chrome nudge**

In `Place`, after the successful `SetWindowPos` that moves to `r`, when `borderless` is true and not sandbox:

```csharp
var r = WithInvisibleFrame(h, rect);
SetWindowPos(h, 0, r.X, r.Y, r.Width, r.Height, NoActivateAsync | SWP_FRAMECHANGED | SWP_SHOWWINDOW | SWP_NOZORDER);
if (borderless) ChromeNudge(h, r);
return PlaceResult.Requested;
```

Add private helper (synchronous, no delay — distinct from optional async `NudgeAsync` / `ResizeNudge`):

```csharp
/// <summary>One pixel wider, then exact size, so WoW rereads mouse bounds after popup chrome.</summary>
private static void ChromeNudge(nint h, PixelRect r)
{
    if (r.Width < 1 || r.Height < 1) return;
    const uint flags = NoActivateAsync | SWP_NOMOVE | SWP_NOZORDER;
    SetWindowPos(h, 0, 0, 0, r.Width + 1, r.Height, flags);
    SetWindowPos(h, 0, 0, 0, r.Width, r.Height, flags | SWP_FRAMECHANGED);
}
```

Do **not** change `NudgeAsync`’s 2 px path; leave `ResizeNudge` default off as today.

- [ ] **Step 3: Build and test**

Run:

```
dotnet build WoWBoxer.Layouts.slnx -c Release
dotnet test WoWBoxer.Layouts.slnx -c Release
```

Expected: build 0 warnings/errors; all tests pass.

- [ ] **Step 4: Commit**

```powershell
git add src/WoWBoxer.Layouts.Win32/WindowPlacer.cs
git commit -m "Apply popup borderless chrome and client-size nudge in WindowPlacer."
```

---

### Task 3: Setup toggle + immediate apply

**Files:**
- Modify: `src/WoWBoxer.Layouts.App/Views/SetupView.xaml` (above Hover `Border`, ~line 416)
- Modify: `src/WoWBoxer.Layouts.App/Views/SettingsView.xaml` (explain text ~line 35)
- Modify: `src/WoWBoxer.Layouts.App/ViewModels/MainViewModel.cs` (`Borderless` setter + helper near `RestoreFrames`)
- Modify: `src/WoWBoxer.Layouts.Core/LayoutProfile.cs` (`Borderless` doc comment)

**Interfaces:**
- Consumes: `MainViewModel.Borderless`, `WindowPlacer.Place` via `SquadPlan.Place`, `Placements()`, `Plan()`, `RestoreFrames` pattern
- Produces: Setup switch AutomationId `BorderlessToggle`; setter applies chrome now

- [ ] **Step 1: Add Setup toggle UI**

In `SetupView.xaml`, insert a new `DockPanel.Dock="Bottom"` block **above** the Hover `Border` (so visual order is: toggle, then Hover). Keep Hover’s DataContext isolation; the new block uses the main view model:

```xml
<!-- Remove title bars: same layout Borderless flag as Settings › Window rules. Applies at once. -->
<Border DockPanel.Dock="Bottom" Margin="0,10,0,0" Padding="0,10,0,0"
        BorderBrush="{StaticResource LineSoftBrush}" BorderThickness="0,1,0,0">
    <StackPanel>
        <CheckBox Style="{StaticResource Switch}" IsChecked="{Binding Borderless}"
                  AutomationProperties.AutomationId="BorderlessToggle"
                  AutomationProperties.Name="Remove title bars"
                  ToolTip="Removes the Windows title bar and frame so slots sit edge to edge. WoW must stay Windowed. Off by default.">
            <TextBlock Text="Remove title bars" FontSize="13" />
        </CheckBox>
        <TextBlock Margin="0,6,0,0" FontSize="11.5" TextWrapping="Wrap" Foreground="{StaticResource MutedBrush}"
                   Text="Off is safest. On removes the Windows frame; keep Display Mode on Windowed." />
    </StackPanel>
</Border>
```

Because both are `DockPanel.Dock="Bottom"`, declare **Hover first in XAML, then the title-bars border** so title bars docks closer to the squad list (above Hover visually). Order in file: Hover block unchanged position relative to ListBox; insert title-bars border **after** Hover in the markup if Dock Bottom stacks last-declared closest to content — verify visually: the circled empty area is **above** Hover, so the title-bars `Border` must be declared **after** the Hover `Border` (last Bottom child sits above earlier Bottom siblings in a DockPanel). Confirm against WPF DockPanel rules: last child fills; Bottom children stack from bottom upward in declaration order. So declare title-bars **before** Hover so title-bars is higher… Actually: first Bottom docks to bottom; second Bottom docks above it. So declare **Hover first**, then **title-bars** → title-bars sits above Hover. Insert the new Border **after** the Hover Border closes.

- [ ] **Step 2: Immediate apply in MainViewModel**

Change `Borderless` setter and add helper next to `RestoreFrames`:

```csharp
public bool Borderless
{
    get => Current?.Options.Borderless ?? false;
    set
    {
        if (Current is null || Current.Options.Borderless == value) return;
        EditOptions(o => o with { Borderless = value });
        ApplyBorderlessChrome();
    }
}

/// <summary>
/// Applies or restores window frames on mapped WoW windows right away (even while paused).
/// Later Start / Re-arrange keep using Profile.Options.Borderless.
/// </summary>
private void ApplyBorderlessChrome()
{
    if (Current is null || Plan() is not { } plan) return;
    RefreshWindows();
    var placements = Placements();
    if (placements.Count == 0) return;
    foreach (var (window, slot) in placements)
        plan.Place(window.Handle, slot);
    Status.Info(Current.Options.Borderless
        ? $"Removed title bars on {placements.Count} WoW window(s)."
        : $"Restored title bars on {placements.Count} WoW window(s).");
}
```

Note: `EditOptions` → `Update` may already `ScheduleLive` if fit rects change; that is fine. `ApplyBorderlessChrome` still runs when paused (`ScheduleLive` would not).

- [ ] **Step 3: Soften Settings + Core docs**

Settings explain (`SettingsView.xaml`):

```xml
<TextBlock Style="{StaticResource Explain}"
           Text="Off by default. On removes the Windows frame with popup chrome so the slot size is the game area; keep WoW on Windowed." />
```

`LayoutOptions.Borderless` doc in `LayoutProfile.cs`:

```csharp
/// <summary>Remove title bars (popup chrome). Off by default.</summary>
```

- [ ] **Step 4: Build**

Run: `dotnet build WoWBoxer.Layouts.slnx -c Release`

Expected: success, 0 warnings.

- [ ] **Step 5: Commit**

```powershell
git add src/WoWBoxer.Layouts.App/Views/SetupView.xaml src/WoWBoxer.Layouts.App/Views/SettingsView.xaml src/WoWBoxer.Layouts.App/ViewModels/MainViewModel.cs src/WoWBoxer.Layouts.Core/LayoutProfile.cs
git commit -m "Add Setup Remove title bars toggle with immediate chrome apply."
```

---

### Task 4: Docs, changelog, verify, push PR

**Files:**
- Modify: `docs/wow-display-findings.md` (section “Title bar vs borderless”)
- Modify: `CHANGELOG.md` (new unreleased / next version section)
- Manual: run app against real or `tools/fakewow` windows if available

- [ ] **Step 1: Update findings**

Append under `## Title bar vs borderless`:

```markdown
- 2026-11-02: ClientLayout-style popup chrome (`WS_POPUP`, client-sized place, 1 px width nudge) lands in
  `WindowPlacer` so borderless no longer leaves an overlapped frame. Setup gains a **Remove title bars** toggle
  (same `Borderless` flag as Settings). Default stays **off**.
```

- [ ] **Step 2: Changelog**

At top of `CHANGELOG.md`, under a `## Unreleased` (or next version heading if the repo prefers that):

```markdown
## Unreleased
- **Remove title bars on Setup** (Your squad, above Hover): same layout option as Settings › Window rules; applies
  at once. Borderless chrome now uses a real popup frame and client-sized placement so mouse/UI stay aligned.
  Default remains off.
```

- [ ] **Step 3: Full verify**

```
dotnet build WoWBoxer.Layouts.slnx -c Release
dotnet test WoWBoxer.Layouts.slnx -c Release
```

Manual (when WoW or FakeWow available): toggle on → no caption, clicks align; toggle off → bars back; Settings checkbox mirrors Setup; Re-arrange with borderless on keeps chrome.

- [ ] **Step 4: Commit docs**

```powershell
git add docs/wow-display-findings.md CHANGELOG.md
git commit -m "Document borderless Setup toggle and popup chrome fix."
```

- [ ] **Step 5: Push fork branch and open PR**

```powershell
git push -u origin HEAD
gh pr create --repo WoW-Multiboxing/wowboxer-layouts --title "Fix borderless chrome and add Setup title-bar toggle" --body "$(@'
## Summary
- Fix Remove title bars with ClientLayout-style popup chrome (WS_POPUP, client-sized slots, 1 px nudge) so mouse/UI stay aligned.
- Add a Setup toggle above Hover bound to the existing layout Borderless flag (default off); applies immediately.
- Settings / CLI / restore keep the same path; Win32 unit tests cover style transforms.

## Test plan
- [ ] `dotnet test WoWBoxer.Layouts.slnx -c Release`
- [ ] Toggle Remove title bars on Setup with windowed WoW clients: no caption, clicks land correctly
- [ ] Toggle off: title bars return
- [ ] Settings checkbox stays in sync
- [ ] Start / Re-arrange with borderless on keeps chrome
'@)"
```

---

## Self-review (plan vs spec)

| Spec requirement | Task |
|---|---|
| Popup borderless styles + clear edge ex-styles | Task 1–2 |
| Framed restore via existing BorderStyles | Task 1 `ToFramed` + Task 2 |
| Slot = client; AdjustWindowRect when expanding | Task 1 `ExpandOuterForClient` (available for restore-preserving-client paths); Place uses slot as client when borderless via no thickframe + WithInvisibleFrame |
| SWP_FRAMECHANGED + NOACTIVATE | Existing Place flags retained in Task 2 |
| 1 px chrome nudge | Task 2 `ChromeNudge` |
| DWM corners | Task 1–2 `ApplyCornerPreference` |
| Setup toggle above Hover, default off | Task 3 |
| Immediate apply on flip | Task 3 `ApplyBorderlessChrome` |
| Settings sync / CLI unchanged path | Task 3 binding + Task 2 helpers |
| Docs + changelog + PR | Task 4 |
| Out of scope (default flip, per-slot, full placer rewrite) | Not scheduled |

**Note:** `ExpandOuterForClient` is implemented and tested for framed expand; `Place` already expands framed windows via `WithInvisibleFrame` after `AddFrame`. If restore-while-preserving-client without a slot rect is needed later, call `ExpandOuterForClient` from `RestoreFrame`; not required for the Setup toggle path which always `Place`s to slot rects.
