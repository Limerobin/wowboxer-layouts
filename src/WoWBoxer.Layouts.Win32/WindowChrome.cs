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
