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
