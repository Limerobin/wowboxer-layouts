using System.Text.Json.Serialization;

namespace WoWBoxer.Layouts.Core;

public enum SlotRole { Main, Follower }

/// <summary>Z-order wish for a slot. <see cref="Top"/> = always on top (overlays), <see cref="Bottom"/> = not on top.</summary>
public enum ZOrder { Normal, Top, Bottom }

/// <summary>Where a slot that was shrunk to the minimum aspect sits inside its rectangle.</summary>
public enum Anchor { Top, Center, Bottom }

/// <summary>
/// <see cref="ShrinkAnchor"/>: a slot narrower than the minimum aspect loses height (so WoW does not letterbox)
/// and is placed by its <see cref="Anchor"/>. <see cref="None"/>: use the rectangle as it is.
/// </summary>
public enum FitMode { ShrinkAnchor, None }

/// <summary>A layout in the shared <c>wowboxer-layout/1</c> format. Positions are % of a monitor's work area.</summary>
public sealed record LayoutProfile
{
    public const string CurrentFormat = "wowboxer-layout/1";

    public string Format { get; init; } = CurrentFormat;
    public string Name { get; init; } = "";
    public int Clients { get; init; }
    /// <summary>The preset this layout was made from (for "Reset to preset"); null for captured or imported layouts.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PresetKind? Preset { get; init; }
    /// <summary>The game this layout is for (null in older files = Classic Era). Each game keeps its own layouts.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public WowGame? Game { get; init; }
    public IReadOnlyList<LayoutSlot> Slots { get; init; } = [];
    /// <summary>The monitors the layout is made for, in order (1 = the main's). Null in older files: the monitors the slots use.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<ScreenRef>? Screens { get; init; }
    /// <summary>6–10 clients on one monitor: how the two parties share it. Null = whichever gives the bigger game windows.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PartySplit? Split { get; init; }
    /// <summary>Overlays with up to 5 clients: the followers float on monitor 2 instead of over the main.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool FollowersOnSecond { get; init; }

    /// <summary>The game, with older files (no game) counted as Classic Era – the only game before Forever.</summary>
    [JsonIgnore]
    public WowGame GameOrDefault => Game is null or WowGame.Unknown ? WowGame.ClassicEra : Game.Value;
    public LayoutOptions Options { get; init; } = new();

    public bool Equals(LayoutProfile? other) =>
        other is not null && Format == other.Format && Name == other.Name && Clients == other.Clients && Preset == other.Preset && Game == other.Game
        && Options == other.Options && Slots.SequenceEqual(other.Slots) && Split == other.Split && FollowersOnSecond == other.FollowersOnSecond
        && (Screens ?? []).SequenceEqual(other.Screens ?? []) && (Screens is null) == (other.Screens is null);

    public override int GetHashCode() => HashCode.Combine(Format, Name, Clients, Options, Slots.Count);
}

public sealed record LayoutSlot
{
    /// <summary>1-based slot number. Windows fill slots in this order (launch order).</summary>
    public int N { get; init; }
    public SlotRole Role { get; init; } = SlotRole.Follower;
    /// <summary>Optional label (character name), used for window titles and messages.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; init; }
    /// <summary>Optional WoW class of the character in this slot ("paladin", "priest" …), used as colour accent.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Class { get; init; }
    /// <summary>"primary", or a 1-based monitor index as Windows reports it.</summary>
    public string Monitor { get; init; } = "primary";
    public double X { get; init; }
    public double Y { get; init; }
    public double W { get; init; }
    public double H { get; init; }
    public ZOrder Z { get; init; } = ZOrder.Normal;
    public Anchor Anchor { get; init; } = Anchor.Top;
    /// <summary>Overrides <see cref="LayoutOptions.Fit"/> for this slot (e.g. the main window keeps its full size).</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FitMode? Fit { get; init; }

    [JsonIgnore]
    public string Label => Name ?? (Role == SlotRole.Main ? $"Main {N}" : $"Slot {N}");
}

public sealed record LayoutOptions
{
    /// <summary>Remove title bars (popup chrome). Off by default.</summary>
    public bool Borderless { get; init; }
    /// <summary>Width/height below which WoW letterboxes. 1.25 (5:4) is our own measurement.</summary>
    public double MinAspect { get; init; } = Geometry.DefaultMinAspect;
    public FitMode Fit { get; init; } = FitMode.ShrinkAnchor;
    public bool UseTaskbarArea { get; init; }
    /// <summary>
    /// Shrink each window 2 px and back after placing. Off by default: it did not help WoW's UI (field test
    /// 2026-10-27), and the second, asynchronous resize could get lost and leave a window 2 px too small.
    /// </summary>
    public bool ResizeNudge { get; init; }
}
