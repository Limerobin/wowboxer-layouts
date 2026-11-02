namespace WoWBoxer.Layouts.Core.Tests;

public class PresetTests
{
    /// <summary>Every layout × every setup × 1–3 chosen monitors (the default choice) × 1–10 clients.</summary>
    public static TheoryData<PresetKind, int, string, int> AllCombinations()
    {
        var data = new TheoryData<PresetKind, int, string, int>();
        foreach (var kind in Presets.Offered)
            foreach (var (setup, monitors) in TestMonitors.Setups)
                for (var screens = 1; screens <= monitors.Count; screens++)
                    for (var n = Presets.MinClients; n <= Presets.MaxClients; n++)
                        data.Add(kind, n, setup, screens);
        return data;
    }

    private static LayoutProfile Make(PresetKind kind, int n, string setup, int screens = 1, LayoutOptions? options = null) =>
        Presets.ForScreens(kind, n, ScreenSet.Choices(TestMonitors.Setups[setup], screens)[0], options);

    private static IReadOnlyList<PlacedSlot> Place(PresetKind kind, int n, string setup, int screens = 1, LayoutOptions? options = null) =>
        LayoutEngine.Resolve(Make(kind, n, setup, screens, options), TestMonitors.Setups[setup]);

    [Fact]
    public void Side_3_matches_the_PowerShell_launcher_on_the_reference_setup()
    {
        // The launcher removed title bars, so compare borderless.
        var borderless = new LayoutOptions { Borderless = true };
        var placed = LayoutEngine.Resolve(Presets.Create(PresetKind.Side, 3, TestMonitors.Reference, options: borderless), TestMonitors.Reference);

        Assert.Equal(new PixelRect(0, 0, 1690, 1392), placed[0].Rect);
        Assert.Equal(new PixelRect(1690, 0, 870, 696), placed[1].Rect);
        Assert.Equal(new PixelRect(1690, 696, 870, 696), placed[2].Rect);
    }

    [Fact]
    public void Side_framed_geometry_shrinks_followers_if_resolved_borderless_without_rebuild()
    {
        // Column width was computed for title bars; flipping Borderless alone makes Fit + TowardsMiddle
        // leave desktop gaps at the top and bottom of the follower column.
        var framed = Presets.Create(PresetKind.Side, 3, TestMonitors.Reference, options: new LayoutOptions { MinAspect = 4.0 / 3 });
        var mismatched = framed with { Options = framed.Options with { Borderless = true } };
        var followers = LayoutEngine.Resolve(mismatched, TestMonitors.Reference)
            .Where(p => p.Slot.Role == SlotRole.Follower)
            .ToList();

        Assert.Equal(2, followers.Count);
        Assert.Contains(followers, f => f.Rect.Height < 696);
        Assert.True(followers[0].Rect.Y > 0 || followers[1].Rect.Bottom < 1392);
    }

    [Fact]
    public void Side_rebuild_with_borderless_fills_follower_column_height()
    {
        var options = new LayoutOptions { Borderless = true, MinAspect = 4.0 / 3 };
        var placed = LayoutEngine.Resolve(Presets.Create(PresetKind.Side, 3, TestMonitors.Reference, options: options), TestMonitors.Reference);
        var followers = placed.Where(p => p.Slot.Role == SlotRole.Follower).OrderBy(p => p.Rect.Y).ToList();

        Assert.Equal(0, followers[0].Rect.Y);
        Assert.Equal(followers[0].Rect.Bottom, followers[1].Rect.Y);
        Assert.Equal(1392, followers[1].Rect.Bottom);
    }

    [Theory]
    [MemberData(nameof(AllCombinations))]
    public void Every_slot_is_inside_its_monitor_work_area(PresetKind kind, int n, string setup, int screens)
    {
        var placed = Place(kind, n, setup, screens);

        Assert.Equal(n, placed.Count);
        Assert.All(placed, p =>
        {
            Assert.False(p.MonitorFallback);
            Assert.True(p.Rect.Width > 0 && p.Rect.Height > 0, $"slot {p.Slot.N} has no size");
            Assert.True(p.Monitor.WorkArea.Contains(p.Rect), $"slot {p.Slot.N} {p.Rect} is outside {p.Monitor.WorkArea}");
        });
    }

    [Theory]
    [MemberData(nameof(AllCombinations))]
    public void Followers_are_at_least_5_to_4(PresetKind kind, int n, string setup, int screens)
    {
        var placed = Place(kind, n, setup, screens);

        if (kind == PresetKind.Free) return;   // free windows keep whatever size the user drags
        Assert.All(placed.Where(p => p.Slot.Role == SlotRole.Follower), p =>
        {
            var (w, h) = LayoutEngine.GameArea(p.Rect, LayoutEngine.FrameSize(new LayoutOptions(), p.Monitor.Dpi));
            Assert.True((double)w / h >= 1.25 - 0.005, $"slot {p.Slot.N} {p.Rect} game area {w}x{h}");
        });
    }

    [Theory]
    [MemberData(nameof(AllCombinations))]
    public void Windows_do_not_overlap_except_overlays_on_the_main(PresetKind kind, int n, string setup, int screens)
    {
        var placed = Place(kind, n, setup, screens);
        if (kind == PresetKind.Free) return;   // a cascade overlaps on purpose
        // overlays float over a main: only windows on the same level must not overlap
        foreach (var level in placed.GroupBy(p => p.Slot.Z == ZOrder.Top))
        {
            var slots = level.ToList();
            for (var i = 0; i < slots.Count; i++)
                for (var j = i + 1; j < slots.Count; j++)
                    Assert.False(slots[i].Rect.Overlaps(slots[j].Rect), $"slot {slots[i].Slot.N} {slots[i].Rect} overlaps slot {slots[j].Slot.N} {slots[j].Rect}");
        }
    }

    [Theory]
    [MemberData(nameof(AllCombinations))]
    public void Presets_are_valid_profiles(PresetKind kind, int n, string setup, int screens)
    {
        var profile = Make(kind, n, setup, screens);

        Assert.Empty(ProfileValidator.Validate(profile));
        Assert.Equal(screens, profile.Screens!.Count);
        // one main per party: slot 1, and slot 6 when there are two parties
        Assert.Equal(n > 5 ? [1, 6] : [1], profile.Slots.Where(s => s.Role == SlotRole.Main).Select(s => s.N));
        Assert.Equal(Enumerable.Range(1, n), profile.Slots.Select(s => s.N));
    }

    [Theory]
    [MemberData(nameof(AllCombinations))]
    public void Every_chosen_monitor_is_used_and_no_other(PresetKind kind, int n, string setup, int screens)
    {
        var monitors = TestMonitors.Setups[setup];
        var chosen = ScreenSet.Choices(monitors, screens)[0];
        var used = Place(kind, n, setup, screens).Select(p => p.Monitor.Index).ToHashSet();

        Assert.Subset(chosen.Select(m => m.Index).ToHashSet(), used);
        // every chosen monitor gets a window as long as there are enough windows (Overlays keep their rows with the main)
        if (kind != PresetKind.Overlay && n >= screens) Assert.Equal(chosen.Select(m => m.Index).ToHashSet(), used);
        Assert.Equal(chosen[0].Index, Place(kind, n, setup, screens)[0].Monitor.Index);   // the main is on monitor 1
    }

    [Theory]
    [InlineData("1080p")]
    [InlineData("21:9")]
    public void Side_main_shares_its_right_edge_with_the_followers(string setup)
    {
        for (var n = 2; n <= 5; n++)
        {
            var placed = Place(PresetKind.Side, n, setup);
            var firstColumn = placed.Skip(1).Min(p => p.Rect.X);
            Assert.Equal(placed[0].Rect.Right, firstColumn);
            Assert.Equal(placed[0].Monitor.WorkArea.Height, placed[0].Rect.Height);
        }
    }

    [Fact]
    public void Overlay_followers_float_inside_the_main_window_and_are_topmost()
    {
        var placed = Place(PresetKind.Overlay, 5, "1080p");

        Assert.Equal(ZOrder.Bottom, placed[0].Slot.Z);
        Assert.All(placed.Skip(1), p =>
        {
            Assert.Equal(ZOrder.Top, p.Slot.Z);
            Assert.True(placed[0].Rect.Contains(p.Rect));
            Assert.True(p.Rect.Y > placed[0].Rect.Height / 2, "followers sit in the lower half");
        });
    }

    [Fact]
    public void Dual_puts_main_on_one_monitor_and_followers_on_the_other()
    {
        var monitors = TestMonitors.Setups["dual-mixed"];
        var placed = LayoutEngine.Resolve(Presets.Create(PresetKind.Dual, 4, monitors), monitors);

        Assert.Equal(1, placed[0].Monitor.Index);
        Assert.Equal(placed[0].Monitor.WorkArea, placed[0].Rect);
        Assert.All(placed.Skip(1), p => Assert.Equal(2, p.Monitor.Index));
    }

    [Fact]
    public void Side_3_with_title_bars_gives_followers_a_4_to_3_game_area()
    {
        var options = new LayoutOptions { MinAspect = 4.0 / 3 };
        var placed = LayoutEngine.Resolve(Presets.Create(PresetKind.Side, 3, TestMonitors.Reference, options: options), TestMonitors.Reference);

        // 696 high window -> 664 game height -> 886 game width (4:3, rounded up) -> 888 wide window
        Assert.Equal(new PixelRect(1672, 0, 888, 696), placed[1].Rect);
        Assert.Equal((886, 664), LayoutEngine.GameArea(placed[1].Rect, LayoutEngine.FrameSize(options)));
    }

    [Fact]
    public void Presets_remember_their_kind_for_reset()
    {
        Assert.Equal(PresetKind.Overlay, Presets.Create(PresetKind.Overlay, 4, TestMonitors.Reference).Preset);
    }

    [Fact]
    public void Dual_with_one_monitor_is_refused()
    {
        var e = Assert.Throws<ArgumentException>(() => Presets.Create(PresetKind.Dual, 3, TestMonitors.Reference));
        Assert.Contains("two monitors", e.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void Client_counts_outside_1_to_10_are_refused(int n) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Presets.Create(PresetKind.Grid, n, TestMonitors.Reference));

    [Theory]
    [InlineData(PresetKind.Side)]
    [InlineData(PresetKind.Overlay)]
    [InlineData(PresetKind.Grid)]
    [InlineData(PresetKind.Free)]
    public void One_client_is_a_single_window_filling_the_work_area(PresetKind kind)
    {
        var profile = Presets.Create(kind, 1, TestMonitors.Reference, options: new LayoutOptions { Borderless = true });
        var placed = LayoutEngine.Resolve(profile, TestMonitors.Reference);

        Assert.Single(placed);
        Assert.Equal(new PixelRect(0, 0, 2560, 1392), placed[0].Rect);
        Assert.Equal(ZOrder.Normal, profile.Slots[0].Z);   // a normal window, not kept at the bottom
    }

    [Fact]
    public void Grid_4_on_16_9_is_two_by_two()
    {
        var cells = Presets.GridCells(4, new PixelRect(0, 0, 1920, 1080), 1.25);

        Assert.Equal([new(0, 0, 960, 540), new(960, 0, 960, 540), new(0, 540, 960, 540), new(960, 540, 960, 540)], cells);
    }

    [Fact]
    public void Grid_centres_a_short_last_row()
    {
        var cells = Presets.GridCells(3, new PixelRect(0, 0, 1920, 1080), 1.25);

        Assert.Equal(new PixelRect(480, 540, 960, 540), cells[2]);
    }

    [Fact]
    public void Main_can_go_on_the_second_monitor()
    {
        var monitors = TestMonitors.Setups["dual-1080p"];
        var placed = LayoutEngine.Resolve(Presets.Create(PresetKind.Side, 3, monitors, monitor: "2"), monitors);

        Assert.All(placed, p => Assert.Equal(2, p.Monitor.Index));
        Assert.Equal(1920, placed[0].Rect.X);
    }

    // ---------- monitors and parties ----------

    [Fact]
    public void Side_on_two_monitors_puts_the_main_on_one_and_spreads_the_followers_on_the_other()
    {
        var placed = Place(PresetKind.Side, 5, "dual-1080p", screens: 2);

        Assert.Equal(new PixelRect(0, 0, 1920, 1032), placed[0].Rect);
        Assert.All(placed.Skip(1), p => Assert.Equal(2, p.Monitor.Index));
    }

    [Fact]
    public void Side_with_two_parties_on_two_monitors_gives_each_party_its_own_monitor()
    {
        var placed = Place(PresetKind.Side, 10, "dual-1080p", screens: 2);

        Assert.All(placed.Take(5), p => Assert.Equal(1, p.Monitor.Index));
        Assert.All(placed.Skip(5), p => Assert.Equal(2, p.Monitor.Index));
        // the same block on both: each main on the left at full height, its followers beside it
        Assert.Equal(new PixelRect(1920, 0, placed[5].Rect.Width, 1080), placed[5].Rect);
        Assert.InRange(placed[5].Rect.Width, placed[0].Rect.Width - 40, placed[0].Rect.Width + 40);
        Assert.All(placed.Skip(6), p => Assert.Equal(placed[5].Rect.Right, p.Rect.X));
        Assert.Equal(SlotRole.Main, placed[5].Slot.Role);
    }

    [Theory]
    [InlineData(PartySplit.SideBySide)]
    [InlineData(PartySplit.Stacked)]
    public void Side_with_two_parties_on_one_monitor_splits_it_in_halves(PartySplit split)
    {
        var monitors = TestMonitors.Reference;
        var placed = LayoutEngine.Resolve(Presets.ForScreens(PresetKind.Side, 8, monitors, split: split), monitors);
        var area = monitors[0].WorkArea;
        var half = split == PartySplit.SideBySide ? area with { Width = area.Width / 2 } : area with { Height = area.Height / 2 };

        Assert.All(placed.Take(5), p => Assert.True(half.Contains(p.Rect), $"party 1 slot {p.Slot.N} {p.Rect} is outside {half}"));
        Assert.All(placed.Skip(5), p => Assert.False(half.Overlaps(p.Rect), $"party 2 slot {p.Slot.N} {p.Rect} is in party 1's half"));
        Assert.Equal(split, Presets.ForScreens(PresetKind.Side, 8, monitors, split: split).Split);
    }

    [Theory]
    [InlineData("1080p", PartySplit.SideBySide)]
    [InlineData("21:9", PartySplit.SideBySide)]
    public void The_best_split_gives_the_bigger_windows(string setup, PartySplit expected)
    {
        var monitor = TestMonitors.Setups[setup][0];

        Assert.Equal(expected, Presets.BestSplit(10, monitor, new LayoutOptions { MinAspect = 4.0 / 3 }));
        // without a choice the layout uses the best split
        var auto = Presets.ForScreens(PresetKind.Side, 10, [monitor]);
        var forced = Presets.ForScreens(PresetKind.Side, 10, [monitor], split: Presets.BestSplit(10, monitor, new LayoutOptions()));
        Assert.Equal(forced.Slots, auto.Slots);
        Assert.Null(auto.Split);
    }

    [Fact]
    public void Overlays_with_two_parties_on_one_monitor_make_two_rows()
    {
        var placed = Place(PresetKind.Overlay, 10, "1080p");

        Assert.Equal(ZOrder.Bottom, placed[0].Slot.Z);
        var rows = placed.Skip(1).GroupBy(p => p.Rect.Y).OrderByDescending(g => g.Key).ToList();
        Assert.Equal(2, rows.Count);
        Assert.Equal([2, 3, 4, 5], rows[0].Select(p => p.Slot.N));   // party 1 in the bottom row
        Assert.Equal([6, 7, 8, 9, 10], rows[1].Select(p => p.Slot.N));
        Assert.All(placed.Skip(1), p => Assert.True(placed[0].Rect.Contains(p.Rect)));
    }

    [Fact]
    public void Overlays_with_two_parties_on_two_monitors_make_one_row_per_monitor()
    {
        var placed = Place(PresetKind.Overlay, 10, "dual-mixed", screens: 2);

        Assert.All(placed.Take(5), p => Assert.Equal(1, p.Monitor.Index));
        Assert.All(placed.Skip(5), p => Assert.Equal(2, p.Monitor.Index));
        Assert.Equal(ZOrder.Bottom, placed[5].Slot.Z);
        Assert.Equal(placed[5].Monitor.WorkArea, placed[5].Rect);   // party 2's main fills monitor 2
        Assert.All(placed.Skip(6), p => Assert.Equal(ZOrder.Top, p.Slot.Z));
    }

    [Fact]
    public void Overlay_followers_can_float_on_monitor_two()
    {
        var monitors = TestMonitors.Setups["dual-1080p"];
        var profile = Presets.ForScreens(PresetKind.Overlay, 4, monitors, followersOnSecond: true);
        var placed = LayoutEngine.Resolve(profile, monitors);

        Assert.Equal(1, placed[0].Monitor.Index);
        Assert.All(placed.Skip(1), p => Assert.Equal(2, p.Monitor.Index));
        Assert.True(profile.FollowersOnSecond);
        Assert.False(Presets.ForScreens(PresetKind.Grid, 4, monitors, followersOnSecond: true).FollowersOnSecond);
    }

    [Theory]
    [InlineData(PresetKind.Grid)]
    [InlineData(PresetKind.Free)]
    public void Grid_and_free_spread_the_windows_evenly_over_the_monitors(PresetKind kind)
    {
        var placed = Place(kind, 10, "triple-mixed", screens: 3);

        // primary first, then left to right: 1440p (index 2), 1080p on the left (index 3), 4K on the right (index 1)
        Assert.Equal([4, 3, 3], new[] { 2, 3, 1 }.Select(i => placed.Count(p => p.Monitor.Index == i)));
        Assert.Equal(2, placed[0].Monitor.Index);
    }

    [Fact]
    public void Counts_are_as_even_as_possible_first_monitors_first()
    {
        Assert.Equal([4, 3, 3], Presets.Counts(10, 3));
        Assert.Equal([1, 1, 0], Presets.Counts(2, 3));
        Assert.Equal([5, 5], Presets.Counts(10, 2));
    }

    [Fact]
    public void Free_windows_can_be_dragged_onto_another_chosen_monitor()
    {
        var monitors = TestMonitors.Setups["dual-mixed"];
        var profile = Presets.ForScreens(PresetKind.Free, 3, ScreenSet.Ordered(monitors));
        var slot = profile.Slots[1];

        // dragged so it straddles the border, mostly on the left monitor: it lands there, inside it
        var moved = SlotEditing.WithRect(slot, new PixelRect(-700, 500, 900, 600), monitors, profile.Options);

        Assert.Equal("2", moved.Monitor);
        Assert.Equal(new PixelRect(-900, 500, 900, 600), SlotEditing.Raw(moved, monitors[1], profile.Options));
    }

    [Fact]
    public void The_old_two_monitor_preset_is_main_and_side_on_two_monitors()
    {
        var profile = Presets.Create(PresetKind.Dual, 4, TestMonitors.Setups["dual-1080p"]);

        Assert.Equal(PresetKind.Side, profile.Preset);
        Assert.Equal(2, profile.Screens!.Count);
        Assert.Equal("Main + side", Presets.DisplayName(PresetKind.Dual));
    }
}
