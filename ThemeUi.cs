using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

// ---------- DPI + drawing helpers ----------
public static class Ui
{
    public static float S = 1f;

    [DllImport("user32.dll")]
    static extern uint GetDpiForSystem();

    public static void InitDpi()
    {
        try
        {
            uint dpi = GetDpiForSystem();
            if (dpi >= 96) { S = dpi / 96f; return; }
        }
        catch { }
        try
        {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero)) { S = g.DpiX / 96f; }
        }
        catch { S = 1f; }
    }

    public static int Px(double v) { return (int)Math.Round(v * S); }

    public static GraphicsPath Round(Rectangle r, int radius)
    {
        GraphicsPath p = new GraphicsPath();
        int d = Math.Max(2, radius * 2);
        if (radius <= 0) { p.AddRectangle(r); return p; }
        p.AddArc(r.X, r.Y, d, d, 180, 90);
        p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        p.CloseFigure();
        return p;
    }

    public static int Luminance(Color c) { return (int)(0.299 * c.R + 0.587 * c.G + 0.114 * c.B); }
    public static Color Mix(Color a, Color b, double k)
    {
        return Color.FromArgb(
            (int)Math.Round(a.R + (b.R - a.R) * k),
            (int)Math.Round(a.G + (b.G - a.G) * k),
            (int)Math.Round(a.B + (b.B - a.B) * k));
    }
    public static Color Lighten(Color c, double k) { return k >= 0 ? Mix(c, Color.White, k) : Mix(c, Color.Black, -k); }
}

// ---------- Segoe MDL2 icon helper ----------
public static class Icons
{
    public static Font Font(double px)
    {
        return new Font("Segoe MDL2 Assets", (float)px, FontStyle.Regular, GraphicsUnit.Pixel);
    }

    public static void Draw(Graphics g, string glyph, Rectangle r, Color color, double px)
    {
        if (string.IsNullOrEmpty(glyph)) return;
        using (Font fnt = Font(px))
        using (StringFormat sf = new StringFormat())
        using (SolidBrush b = new SolidBrush(color))
        {
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
            sf.FormatFlags = StringFormatFlags.NoWrap;
            g.DrawString(glyph, fnt, b, r, sf);
        }
    }

    public static void DrawImage(Graphics g, Image img, Rectangle r)
    {
        if (img == null) return;
        double ar = (double)img.Width / Math.Max(1, img.Height);
        int h = r.Height, w = (int)Math.Round(h * ar);
        if (w > r.Width) { w = r.Width; h = (int)Math.Round(w / ar); }
        g.DrawImage(img, new Rectangle(r.X + (r.Width - w) / 2, r.Y + (r.Height - h) / 2, w, h));
    }
}

// ---------- Theme ----------
public class ThemeColors
{
    public bool Dark;
    public Color Back, Card, Fore, Sub, Border, InputBack, Accent, AccentFore, ButtonBack, ButtonHover, ButtonFore, SelBack;
}

public static class ThemeManager
{
    [DllImport("dwmapi.dll")]
    static extern int DwmGetColorizationColor(out uint color, out bool opaque);

    public static Color AccentColor()
    {
        try
        {
            uint argb; bool opaque;
            if (DwmGetColorizationColor(out argb, out opaque) == 0)
            {
                uint rgb = argb & 0x00FFFFFF;
                return Color.FromArgb((int)((rgb >> 16) & 0xFF), (int)((rgb >> 8) & 0xFF), (int)(rgb & 0xFF));
            }
        }
        catch { }
        try
        {
            object v = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\DWM", "AccentColor", null);
            if (v is int)
            {
                int abgr = (int)v;
                return Color.FromArgb(abgr & 0xFF, (abgr >> 8) & 0xFF, (abgr >> 16) & 0xFF);
            }
        }
        catch { }
        return Color.FromArgb(0, 120, 215);
    }

    public static bool SystemIsDark()
    {
        try
        {
            object v = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", null);
            if (v is int) return (int)v == 0;
        }
        catch { }
        return false;
    }

    public static ThemeColors Resolve(string mode, string accentKey)
    {
        bool dark;
        if (mode == "dark") dark = true;
        else if (mode == "light") dark = false;
        else dark = SystemIsDark();
        Color accent = AccentColorFor(accentKey);
        return Build(dark, accent);
    }

    public static Color AccentColorFor(string key)
    {
        if (string.IsNullOrEmpty(key) || key == "system") return AccentColor();
        switch (key)
        {
            case "blue": return Color.FromArgb(0, 95, 184);
            case "purple": return Color.FromArgb(135, 100, 184);
            case "pink": return Color.FromArgb(194, 57, 179);
            case "red": return Color.FromArgb(196, 43, 28);
            case "orange": return Color.FromArgb(202, 80, 16);
            case "yellow": return Color.FromArgb(255, 185, 0);
            case "green": return Color.FromArgb(16, 124, 16);
            case "teal": return Color.FromArgb(3, 131, 135);
            case "gray": return Color.FromArgb(105, 121, 126);
            default: return AccentColor();
        }
    }

    public static readonly string[] AccentKeys = new string[] { "system", "blue", "purple", "pink", "red", "orange", "yellow", "green", "teal", "gray" };

    public static ThemeColors Build(bool dark)
    {
        return Build(dark, AccentColor());
    }

    public static ThemeColors Build(bool dark, Color accent)
    {
        ThemeColors t = new ThemeColors();
        t.Dark = dark;
        if (dark)
        {
            t.Back = Color.FromArgb(31, 31, 31);
            t.Card = Color.FromArgb(44, 44, 44);
            t.Fore = Color.FromArgb(236, 236, 236);
            t.Sub = Color.FromArgb(158, 158, 158);
            t.Border = Color.FromArgb(66, 66, 66);
            t.InputBack = Color.FromArgb(37, 37, 37);
            t.Accent = Ui.Lighten(accent, 0.30);
        }
        else
        {
            t.Back = Color.FromArgb(243, 243, 243);
            t.Card = Color.White;
            t.Fore = Color.FromArgb(26, 26, 26);
            t.Sub = Color.FromArgb(104, 104, 104);
            t.Border = Color.FromArgb(213, 213, 213);
            t.InputBack = Color.FromArgb(250, 250, 250);
            t.Accent = accent;
        }
        Color baseAccent = accent;
        if (dark)
        {
            t.ButtonBack = baseAccent;
            t.ButtonHover = Ui.Lighten(baseAccent, 0.12);
        }
        else
        {
            t.ButtonBack = baseAccent;
            t.ButtonHover = Ui.Lighten(baseAccent, -0.12);
        }
        t.ButtonFore = Ui.Luminance(t.ButtonBack) > 150 ? Color.Black : Color.White;
        t.SelBack = Ui.Mix(t.Card, t.Accent, 0.22);
        return t;
    }

    public static void ApplyTheme(Control root, ThemeColors t)
    {
        root.BackColor = t.Back;
        root.ForeColor = t.Fore;
        ApplyChildren(root, t, t.Back);
    }

    static void ApplyChildren(Control parent, ThemeColors t, Color surface)
    {
        foreach (Control c in parent.Controls)
        {
            Color childSurface = (c is ThemedCard) ? t.Card : surface;
            IThemed th = c as IThemed;
            if (th != null)
            {
                th.ApplyTheme(t, childSurface);
            }
            else
            {
                c.BackColor = childSurface;
                c.ForeColor = t.Fore;
            }
            ApplyChildren(c, t, childSurface);
        }
    }
}

public static class Swatches
{
    public static Bitmap Accent(string key)
    {
        Color c = ThemeManager.AccentColorFor(key);
        int side = Ui.Px(14);
        Bitmap bmp = new Bitmap(side, side);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);
            Rectangle r = new Rectangle(0, 0, side - 1, side - 1);
            using (GraphicsPath p = Ui.Round(r, Ui.Px(3)))
            {
                using (SolidBrush b = new SolidBrush(c)) g.FillPath(b, p);
                using (Pen pen = new Pen(Color.FromArgb(110, 0, 0, 0))) g.DrawPath(pen, p);
            }
        }
        return bmp;
    }
}

public interface IThemed
{
    void ApplyTheme(ThemeColors t, Color surface);
}

// ---------- Card ----------
public class ThemedCard : Panel, IThemed
{
    public string TitleKey = "";
    public string Title = "";
    public string TitleIcon = "";
    ThemeColors _t;
    Color _surface;

    public ThemedCard()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
    }

    public void ApplyTheme(ThemeColors t, Color surface)
    {
        _t = t; _surface = surface;
        BackColor = t == null ? surface : (t.Dark ? t.Card : t.Card);
        ForeColor = t == null ? Color.Black : t.Fore;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        ThemeColors t = _t != null ? _t : ThemeManager.Build(false);
        Rectangle rr = new Rectangle(0, 0, Width - 1, Height - 1);
        using (GraphicsPath p = Ui.Round(rr, Ui.Px(8)))
        {
            using (SolidBrush b = new SolidBrush(t.Card)) g.FillPath(b, p);
            using (Pen pen = new Pen(t.Border)) g.DrawPath(pen, p);
        }
        if (!string.IsNullOrEmpty(Title))
        {
            int band = Ui.Px(32);
            Rectangle iconRect = new Rectangle(Ui.Px(9), 0, Ui.Px(24), band);
            Icons.Draw(g, TitleIcon, iconRect, t.Accent, Ui.Px(22));
            using (Font f = new Font("Segoe UI", 10f, FontStyle.Bold))
            {
                TextRenderer.DrawText(g, Title, f,
                    new Rectangle(Ui.Px(39), 0, Width - Ui.Px(49), band),
                    t.Accent, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }
        }
    }
}

// ---------- Label ----------
public class ThemedLabel : Control, IThemed
{
    public string IconGlyph = "";
    public Image IconImage;
    public bool Muted = false;
    public bool Bold = false;

    ThemeColors _t;
    Color _surface;

    public ThemedLabel()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
    }

    public void ApplyTheme(ThemeColors t, Color surface)
    {
        _t = t; _surface = surface;
        BackColor = surface;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        ThemeColors t = _t != null ? _t : ThemeManager.Build(false);
        using (SolidBrush b = new SolidBrush(_surface)) g.FillRectangle(b, ClientRectangle);
        Color fg = Muted ? t.Sub : t.Fore;
        int iconSize = Ui.Px(16);
        int left = 0;
        if (!string.IsNullOrEmpty(IconGlyph) || IconImage != null)
        {
            Rectangle ir = new Rectangle(0, (Height - iconSize) / 2, iconSize, iconSize);
            if (IconImage != null) Icons.DrawImage(g, IconImage, ir);
            else Icons.Draw(g, IconGlyph, ir, Muted ? t.Sub : t.Accent, Ui.Px(14));
            left = iconSize + Ui.Px(8);
        }
        Font font = Font;
        Font boldFont = null;
        if (Bold) { boldFont = new Font(Font, FontStyle.Bold); font = boldFont; }
        Rectangle rt = new Rectangle(left, 0, Math.Max(0, Width - left), Height);
        TextRenderer.DrawText(g, Text, font, rt, fg,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        if (boldFont != null) boldFont.Dispose();
    }
}

// ---------- CheckBox ----------
public class ThemedCheckBox : Control, IThemed
{
    ThemeColors _t;
    Color _surface = Color.Transparent;
    public string IconGlyph = "";
    bool _hover;
    bool _checked;
    bool _focus;

    public event EventHandler CheckedChanged;

    public ThemedCheckBox()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
        SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, true);
        TabStop = true;
        Cursor = Cursors.Hand;
    }

    public bool Checked
    {
        get { return _checked; }
        set
        {
            if (_checked == value) return;
            _checked = value;
            Invalidate();
            if (CheckedChanged != null) CheckedChanged(this, EventArgs.Empty);
        }
    }

    public void ApplyTheme(ThemeColors t, Color surface)
    {
        _t = t; _surface = surface;
        BackColor = surface;
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && ClientRectangle.Contains(e.Location))
        {
            Focus();
            Checked = !Checked;
        }
        base.OnMouseUp(e);
    }
    protected override bool IsInputKey(Keys keyData)
    {
        if (keyData == Keys.Space) return true;
        return base.IsInputKey(keyData);
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Space) { Checked = !Checked; e.Handled = true; }
        base.OnKeyDown(e);
    }
    protected override void OnGotFocus(EventArgs e) { _focus = true; Invalidate(); base.OnGotFocus(e); }
    protected override void OnLostFocus(EventArgs e) { _focus = false; Invalidate(); base.OnLostFocus(e); }
    protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        ThemeColors t = _t != null ? _t : ThemeManager.Build(false);
        using (SolidBrush b = new SolidBrush(_surface)) g.FillRectangle(b, ClientRectangle);

        int box = Ui.Px(14);
        int y = (Height - box) / 2;
        int iconW = 0;
        if (!string.IsNullOrEmpty(IconGlyph))
        {
            Icons.Draw(g, IconGlyph, new Rectangle(0, (Height - Ui.Px(16)) / 2, Ui.Px(16), Ui.Px(16)), t.Accent, Ui.Px(14));
            iconW = Ui.Px(16) + Ui.Px(6);
        }
        Rectangle rb = new Rectangle(iconW + Ui.Px(1), y, box, box);
        bool on = Checked;
        using (GraphicsPath p = Ui.Round(rb, Ui.Px(3)))
        {
            if (on)
            {
                using (SolidBrush b = new SolidBrush(Enabled ? t.Accent : Ui.Mix(t.Card, t.Border, 0.5)))
                    g.FillPath(b, p);
            }
            else
            {
                using (SolidBrush b = new SolidBrush(t.InputBack)) g.FillPath(b, p);
                using (Pen pen = new Pen(_hover && Enabled ? t.Accent : t.Border)) g.DrawPath(pen, p);
            }
        }
        if (on)
        {
            int pad = Ui.Px(4);
            using (Pen pen = new Pen(t.ButtonFore, Ui.Px(2)))
            {
                pen.StartCap = LineCap.Round; pen.EndCap = LineCap.Round;
                Point[] pts = new Point[] {
                    new Point(rb.X + pad, rb.Y + box / 2),
                    new Point(rb.X + box / 2 - Ui.Px(1), rb.Bottom - pad),
                    new Point(rb.Right - pad, rb.Y + pad)
                };
                g.DrawLines(pen, pts);
            }
        }
        int tx = rb.Right + Ui.Px(8);
        Rectangle rt = new Rectangle(tx, 0, Math.Max(0, Width - tx), Height);
        TextRenderer.DrawText(g, Text, Font, rt, Enabled ? t.Fore : t.Sub,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        if (_focus)
        {
            using (Pen pen = new Pen(Ui.Mix(t.Accent, t.Card, 0.45)))
            {
                pen.DashStyle = DashStyle.Dot;
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }
    }
}

// ---------- Spinner ----------
public class ThemedSpinner : Control, IThemed
{
    ThemeColors _t;
    Color _surface = Color.Transparent;
    bool _focus;
    string _typed = "";
    DateTime _typedAt = DateTime.MinValue;
    int _value, _min, _max, _inc = 1;

    public event EventHandler ValueChanged;

    public ThemedSpinner()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
        SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, true);
        TabStop = true;
        Cursor = Cursors.Default;
    }

    public int Minimum { get { return _min; } set { _min = value; Value = _value; } }
    public int Maximum { get { return _max; } set { _max = value; Value = _value; } }
    public int Increment { get { return _inc; } set { _inc = Math.Max(1, value); } }
    public string Suffix = "";

    public int Value
    {
        get { return _value; }
        set
        {
            int v = value;
            if (v < _min) v = _min;
            if (v > _max) v = _max;
            if (v == _value) return;
            _value = v;
            Invalidate();
            if (ValueChanged != null) ValueChanged(this, EventArgs.Empty);
        }
    }

    public void ApplyTheme(ThemeColors t, Color surface)
    {
        _t = t; _surface = surface;
        BackColor = surface;
        Invalidate();
    }

    int ButtonW { get { return Ui.Px(18); } }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        ThemeColors t = _t != null ? _t : ThemeManager.Build(false);
        using (SolidBrush b = new SolidBrush(_surface)) g.FillRectangle(b, ClientRectangle);

        Rectangle rr = new Rectangle(0, 0, Width - 1, Height - 1);
        using (GraphicsPath p = Ui.Round(rr, Ui.Px(4)))
        {
            using (SolidBrush b = new SolidBrush(Enabled ? t.InputBack : Ui.Mix(t.InputBack, t.Card, 0.4))) g.FillPath(b, p);
            using (Pen pen = new Pen(_focus ? t.Accent : t.Border)) g.DrawPath(pen, p);
        }
        Rectangle txt = new Rectangle(Ui.Px(7), 0, Width - ButtonW - Ui.Px(9), Height);
        string num = _value.ToString();
        TextRenderer.DrawText(g, num, Font, txt, Enabled ? t.Fore : t.Sub,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        if (!string.IsNullOrEmpty(Suffix))
        {
            Size ns = TextRenderer.MeasureText(num, Font);
            int sx = txt.X + ns.Width;
            Rectangle sr = new Rectangle(sx, 0, Math.Max(0, txt.Right - sx), Height);
            TextRenderer.DrawText(g, Suffix, Font, sr, Enabled ? t.Sub : t.Sub,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }

        Rectangle up = new Rectangle(Width - ButtonW, 0, ButtonW - 1, Height / 2);
        Rectangle dn = new Rectangle(Width - ButtonW, Height / 2, ButtonW - 1, Height - Height / 2);
        DrawArrow(g, up, true, t);
        DrawArrow(g, dn, false, t);
    }

    void DrawArrow(Graphics g, Rectangle r, bool up, ThemeColors t)
    {
        int cx = r.X + r.Width / 2, cy = r.Y + r.Height / 2;
        int w = Ui.Px(4), h = Ui.Px(3);
        Point[] pts = up
            ? new Point[] { new Point(cx - w, cy + h), new Point(cx + w, cy + h), new Point(cx, cy - h) }
            : new Point[] { new Point(cx - w, cy - h), new Point(cx + w, cy - h), new Point(cx, cy + h) };
        using (SolidBrush b = new SolidBrush(Enabled ? t.Sub : Ui.Mix(t.Sub, t.Card, 0.5))) g.FillPolygon(b, pts);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Focus();
        _typed = "";
        if (e.Button != MouseButtons.Left) return;
        if (e.X >= Width - ButtonW)
        {
            if (e.Y < Height / 2) Nudge(+_inc); else Nudge(-_inc);
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        Nudge(e.Delta > 0 ? +_inc : -_inc);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        bool overButtons = e.X >= Width - ButtonW;
        Cursor desired = overButtons ? Cursors.Hand : Cursors.IBeam;
        if (Cursor != desired) Cursor = desired;
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        Cursor = Cursors.Default;
    }

    void Nudge(int delta)
    {
        Value = _value + delta;
    }

    protected override bool IsInputKey(Keys keyData)
    {
        if (keyData == Keys.Up || keyData == Keys.Down || keyData == Keys.PageUp ||
            keyData == Keys.PageDown || keyData == Keys.Home || keyData == Keys.End) return true;
        return base.IsInputKey(keyData);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        switch (e.KeyCode)
        {
            case Keys.Up: Nudge(+_inc); e.Handled = true; break;
            case Keys.Down: Nudge(-_inc); e.Handled = true; break;
            case Keys.PageUp: Nudge(+_inc * 5); e.Handled = true; break;
            case Keys.PageDown: Nudge(-_inc * 5); e.Handled = true; break;
            case Keys.Home: Value = _min; e.Handled = true; break;
            case Keys.End: Value = _max; e.Handled = true; break;
        }
    }

    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        base.OnKeyPress(e);
        if (char.IsDigit(e.KeyChar))
        {
            if ((DateTime.Now - _typedAt).TotalMilliseconds > 1200) _typed = "";
            _typedAt = DateTime.Now;
            if (_typed.Length >= 5) _typed = "";
            _typed += e.KeyChar;
            int v;
            if (int.TryParse(_typed, out v)) Value = v;
            e.Handled = true;
        }
        else if (e.KeyChar == '\b')
        {
            if (_typed.Length > 0) _typed = _typed.Substring(0, _typed.Length - 1);
            e.Handled = true;
        }
        else if (e.KeyChar == (char)13 || e.KeyChar == (char)27)
        {
            _typed = "";
        }
    }

    protected override void OnGotFocus(EventArgs e) { _focus = true; Invalidate(); base.OnGotFocus(e); }
    protected override void OnLostFocus(EventArgs e) { _focus = false; Invalidate(); base.OnLostFocus(e); }
    protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }
}

// ---------- Select ----------
public class ThemedSelect : Control, IThemed
{
    ThemeColors _t;
    Color _surface = Color.Transparent;
    bool _hover;
    string[] _items = new string[0];
    Image[] _images = new Image[0];
    int _index;
    SelectPopup _popup;

    public event EventHandler SelectedIndexChanged;

    public ThemedSelect()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
        TabStop = true;
        Cursor = Cursors.Hand;
    }

    public string[] Items
    {
        get { return _items; }
        set
        {
            _items = value == null ? new string[0] : value;
            if (_index >= _items.Length) _index = Math.Max(0, _items.Length - 1);
            Invalidate();
        }
    }

    public Image[] ItemImages
    {
        get { return _images; }
        set { _images = value == null ? new Image[0] : value; Invalidate(); }
    }

    public int SelectedIndex
    {
        get { return _index; }
        set
        {
            int v = value;
            if (v < 0) v = 0;
            if (_items.Length > 0 && v >= _items.Length) v = _items.Length - 1;
            if (v == _index) return;
            _index = v;
            Invalidate();
            if (SelectedIndexChanged != null) SelectedIndexChanged(this, EventArgs.Empty);
        }
    }

    public void ApplyTheme(ThemeColors t, Color surface)
    {
        _t = t; _surface = surface;
        BackColor = surface;
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        ThemeColors t = _t != null ? _t : ThemeManager.Build(false);
        using (SolidBrush b = new SolidBrush(_surface)) g.FillRectangle(b, ClientRectangle);
        Rectangle rr = new Rectangle(0, 0, Width - 1, Height - 1);
        using (GraphicsPath p = Ui.Round(rr, Ui.Px(4)))
        {
            using (SolidBrush b = new SolidBrush(t.InputBack)) g.FillPath(b, p);
            using (Pen pen = new Pen(_hover ? t.Accent : t.Border)) g.DrawPath(pen, p);
        }
        Image img = (_index >= 0 && _index < _images.Length) ? _images[_index] : null;
        int left = Ui.Px(7);
        if (img != null)
        {
            Icons.DrawImage(g, img, new Rectangle(left, (Height - Ui.Px(14)) / 2, Ui.Px(22), Ui.Px(14)));
            left += Ui.Px(22) + Ui.Px(6);
        }
        Rectangle txt = new Rectangle(left, 0, Width - left - Ui.Px(20), Height);
        string text = (_items.Length > 0 && _index >= 0 && _index < _items.Length) ? _items[_index] : "";
        TextRenderer.DrawText(g, text, Font, txt, t.Fore, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        int cx = Width - Ui.Px(12), cy = Height / 2;
        Point[] pts = new Point[] {
            new Point(cx - Ui.Px(4), cy - Ui.Px(2)),
            new Point(cx + Ui.Px(4), cy - Ui.Px(2)),
            new Point(cx, cy + Ui.Px(3))
        };
        using (SolidBrush b = new SolidBrush(t.Sub)) g.FillPolygon(b, pts);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Focus();
        if (e.Button == MouseButtons.Left) OpenPopup();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up) { OpenPopup(); e.Handled = true; }
        if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter) { OpenPopup(); e.Handled = true; }
    }

    void OpenPopup()
    {
        if (_popup != null) { _popup.Close(); _popup = null; }
        if (_items.Length == 0) return;
        ThemeColors t = _t != null ? _t : ThemeManager.Build(false);
        _popup = new SelectPopup(_items, _index, t, Font, _images);
        Rectangle scr = Screen.FromControl(this).WorkingArea;
        Point p = PointToScreen(new Point(0, Height));
        int rowH = Ui.Px(24), pad = Ui.Px(4);
        int h = _items.Length * rowH + pad * 2;
        int w = Width;
        if (p.X + w > scr.Right) p.X = scr.Right - w;
        if (p.Y + h > scr.Bottom) p.Y = PointToScreen(Point.Empty).Y - h;
        _popup.StartPosition = FormStartPosition.Manual;
        _popup.Location = p;
        _popup.ClientSize = new Size(w, h);
        _popup.RowHeight = rowH;
        _popup.Pad = pad;
        _popup.Show();
        _popup.SelectRow(_index);
        _popup.RowChosen += delegate(int idx)
        {
            SelectedIndex = idx;
            if (_popup != null) { _popup.Close(); _popup = null; }
            Focus();
        };
    }
}

public class SelectPopup : Form
{
    string[] _items;
    Image[] _images;
    int _selected = -1;
    int _hover = -1;
    ThemeColors _t;
    Font _font;
    public int RowHeight = 24;
    public int Pad = 4;
    public event Action<int> RowChosen;

    public SelectPopup(string[] items, int selected, ThemeColors t, Font font, Image[] images)
    {
        _items = items; _selected = selected; _t = t; _font = font; _images = images;
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        StartPosition = FormStartPosition.Manual;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
    }

    public void SelectRow(int i) { _selected = i; _hover = i; Invalidate(); }

    protected override void OnDeactivate(EventArgs e)
    {
        base.OnDeactivate(e);
        Close();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.KeyCode == Keys.Escape) Close();
        if (e.KeyCode == Keys.Down && _hover < _items.Length - 1) { _hover++; Invalidate(); }
        if (e.KeyCode == Keys.Up && _hover > 0) { _hover--; Invalidate(); }
        if (e.KeyCode == Keys.Enter && _hover >= 0) Choose(_hover);
    }

    void Choose(int i)
    {
        if (RowChosen != null) RowChosen(i);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        int i = (e.Y - Pad) / RowHeight;
        if (i != _hover) { _hover = i; Invalidate(); }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        int i = (e.Y - Pad) / RowHeight;
        if (i >= 0 && i < _items.Length) Choose(i);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using (SolidBrush b = new SolidBrush(_t.Card)) g.FillRectangle(b, ClientRectangle);
        for (int i = 0; i < _items.Length; i++)
        {
            Rectangle r = new Rectangle(Pad, Pad + i * RowHeight, Width - Pad * 2, RowHeight);
            if (i == _hover) { using (SolidBrush b = new SolidBrush(_t.SelBack)) g.FillRectangle(b, r); }
            if (i == _selected) { using (SolidBrush b = new SolidBrush(_t.Accent)) g.FillRectangle(b, new Rectangle(r.X, r.Y + Ui.Px(4), Ui.Px(2), r.Height - Ui.Px(8))); }
            int textX = r.X + Ui.Px(8);
            if (_images != null && i < _images.Length && _images[i] != null)
            {
                Icons.DrawImage(g, _images[i], new Rectangle(textX, r.Y + (r.Height - Ui.Px(14)) / 2, Ui.Px(22), Ui.Px(14)));
                textX += Ui.Px(22) + Ui.Px(6);
            }
            TextRenderer.DrawText(g, _items[i], _font, new Rectangle(textX, r.Y, r.Right - textX - Ui.Px(2), r.Height),
                _t.Fore, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }
        using (Pen pen = new Pen(_t.Border)) g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
    }
}


// ---------- Custom theme-aware title bar ----------
public class ThemedCaption : Control, IThemed
{
    [DllImport("user32.dll")]
    static extern bool ReleaseCapture();
    [DllImport("user32.dll")]
    static extern IntPtr SendMessage(IntPtr h, uint msg, IntPtr wParam, IntPtr lParam);

    public string Title = "MacBookToolkit";
    public string VersionText = "";
    public string VersionTip = "";
    public event EventHandler CloseClick;
    public event EventHandler MinimizeClick;
    public event EventHandler VersionClick;

    ThemeColors _t;
    ToolTip _tip;
    bool _closeHover, _minHover, _versionHover;

    public ThemedCaption()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        _tip = new ToolTip();
        _tip.InitialDelay = 400;
        _tip.AutoPopDelay = 15000;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _tip != null) { _tip.Dispose(); _tip = null; }
        base.Dispose(disposing);
    }

    public void ApplyTheme(ThemeColors t, Color surface)
    {
        _t = t;
        BackColor = t.Card;
        Invalidate();
    }

    int BtnW { get { return Ui.Px(34); } }

    Rectangle MinRect { get { return new Rectangle(Width - BtnW * 2, 0, BtnW, Height); } }
    Rectangle CloseRect { get { return new Rectangle(Width - BtnW, 0, BtnW, Height); } }
    Rectangle VersionRect { get { return new Rectangle(Width - BtnW * 2 - Ui.Px(64), Ui.Px(4), Ui.Px(60), Height - Ui.Px(8)); } }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        ThemeColors t = _t != null ? _t : ThemeManager.Build(false);
        using (SolidBrush b = new SolidBrush(t.Card)) g.FillRectangle(b, ClientRectangle);

        int iconSize = Ui.Px(16);
        try
        {
            if (Parent is Form && ((Form)Parent).Icon != null)
            {
                g.DrawIcon(((Form)Parent).Icon, new Rectangle(Ui.Px(8), (Height - iconSize) / 2, iconSize, iconSize));
            }
        }
        catch { }

        Color titleColor = t.Fore;
        using (Font f = new Font("Segoe UI", 9.5f, FontStyle.Bold))
        {
            Rectangle rt = new Rectangle(Ui.Px(32), 0, Width - Ui.Px(32) - BtnW * 2 - Ui.Px(8), Height);
            TextRenderer.DrawText(g, Title, f, rt, titleColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
        }
        if (!string.IsNullOrEmpty(VersionText))
        {
            using (Font f = new Font("Segoe UI", 8f))
            {
                Color vc = _versionHover ? t.Accent : t.Sub;
                TextRenderer.DrawText(g, VersionText, f, VersionRect, vc,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                if (_versionHover)
                {
                    using (Pen pen = new Pen(t.Accent, Math.Max(1f, Ui.Px(1f))))
                    {
                        Size ts = TextRenderer.MeasureText(VersionText, f);
                        g.DrawLine(pen, VersionRect.Right - ts.Width, VersionRect.Bottom - Ui.Px(6), VersionRect.Right, VersionRect.Bottom - Ui.Px(6));
                    }
                }
            }
        }

        // minimize button
        if (_minHover) { using (SolidBrush b = new SolidBrush(Ui.Mix(t.Card, t.Sub, 0.18))) g.FillRectangle(b, MinRect); }
        using (Pen pen = new Pen(_minHover ? t.Fore : t.Sub, Math.Max(1f, Ui.Px(1.2f))))
        {
            int cx = MinRect.X + MinRect.Width / 2, cy = MinRect.Y + MinRect.Height / 2;
            g.DrawLine(pen, cx - Ui.Px(5), cy, cx + Ui.Px(5), cy);
        }

        // close button
        if (_closeHover) { using (SolidBrush b = new SolidBrush(Color.FromArgb(232, 17, 35))) g.FillRectangle(b, CloseRect); }
        using (Pen pen = new Pen(_closeHover ? Color.White : t.Sub, Math.Max(1f, Ui.Px(1.3f))))
        {
            int cx = CloseRect.X + CloseRect.Width / 2, cy = CloseRect.Y + CloseRect.Height / 2, d = Ui.Px(5);
            g.DrawLine(pen, cx - d, cy - d, cx + d, cy + d);
            g.DrawLine(pen, cx - d, cy + d, cx + d, cy - d);
        }

        using (Pen pen = new Pen(t.Border)) g.DrawLine(pen, 0, Height - 1, Width, Height - 1);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        bool min = MinRect.Contains(e.Location);
        bool close = CloseRect.Contains(e.Location);
        bool version = !string.IsNullOrEmpty(VersionText) && VersionRect.Contains(e.Location);
        if (version != _versionHover)
        {
            _versionHover = version;
            if (version) { if (_tip != null) _tip.SetToolTip(this, VersionTip); }
            else { if (_tip != null) _tip.SetToolTip(this, ""); }
            Invalidate();
        }
        if (min != _minHover || close != _closeHover)
        {
            _minHover = min; _closeHover = close;
            Invalidate();
        }
        Cursor desired = version ? Cursors.Hand : ((min || close) ? Cursors.Default : Cursors.SizeAll);
        if (Cursor != desired) Cursor = desired;
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _minHover = _closeHover = _versionHover = false;
        if (_tip != null) _tip.SetToolTip(this, "");
        Cursor = Cursors.Default;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left) return;
        if (VersionRect.Contains(e.Location)) { if (VersionClick != null) VersionClick(this, EventArgs.Empty); return; }
        if (CloseRect.Contains(e.Location)) { if (CloseClick != null) CloseClick(this, EventArgs.Empty); return; }
        if (MinRect.Contains(e.Location)) { if (MinimizeClick != null) MinimizeClick(this, EventArgs.Empty); return; }
        if (Parent != null)
        {
            ReleaseCapture();
            SendMessage(Parent.Handle, 0x00A1, (IntPtr)0x0002, IntPtr.Zero);
        }
    }

    protected override void OnDoubleClick(EventArgs e)
    {
        // fixed-size utility window: double click does not maximize
    }
}

// ---------- Button ----------
public class ThemedButton : Button, IThemed
{
    ThemeColors _t;
    Color _surface = Color.Transparent;
    bool _hover, _pressed;
    public bool Secondary = false;
    public string IconGlyph = "";

    public ThemedButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
    }

    public void ApplyTheme(ThemeColors t, Color surface)
    {
        _t = t; _surface = surface;
        BackColor = surface;
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hover = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { _pressed = true; Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e); }
    protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        ThemeColors t = _t != null ? _t : ThemeManager.Build(false);
        Color back, fore, border;
        if (Secondary)
        {
            back = Enabled ? (_pressed ? Ui.Mix(t.Card, t.Border, 0.8) : (_hover ? Ui.Mix(t.Card, t.Accent, 0.12) : t.Card))
                           : Ui.Mix(t.Card, t.Border, 0.4);
            fore = Enabled ? t.Fore : t.Sub;
            border = _hover && Enabled ? t.Accent : t.Border;
        }
        else
        {
            back = Enabled ? (_pressed ? Ui.Lighten(t.ButtonBack, -0.18) : (_hover ? t.ButtonHover : t.ButtonBack))
                           : Ui.Mix(t.Card, t.Border, 0.6);
            fore = Enabled ? t.ButtonFore : t.Sub;
            border = back;
        }
        Rectangle rr = new Rectangle(0, 0, Width - 1, Height - 1);
        using (GraphicsPath p = Ui.Round(rr, Ui.Px(5)))
        {
            using (SolidBrush b = new SolidBrush(back)) g.FillPath(b, p);
            if (Secondary)
            {
                using (Pen pen = new Pen(border)) g.DrawPath(pen, p);
            }
        }
        Size ts = TextRenderer.MeasureText(Text, Font);
        int iconW = string.IsNullOrEmpty(IconGlyph) ? 0 : Ui.Px(16) + Ui.Px(7);
        int totalW = iconW + ts.Width;
        int startX = (Width - totalW) / 2;
        if (totalW > Width - Ui.Px(16)) startX = Ui.Px(8);
        if (iconW > 0)
        {
            Icons.Draw(g, IconGlyph, new Rectangle(startX, (Height - Ui.Px(16)) / 2, Ui.Px(16), Ui.Px(16)), fore, Ui.Px(14));
        }
        Rectangle tr = new Rectangle(startX + iconW, 0, Math.Max(0, Width - startX - iconW - Ui.Px(6)), Height);
        TextRenderer.DrawText(g, Text, Font, tr, fore,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
    }
}

// ---------- Small language flags ----------
public static class Flags
{
    public static Bitmap Draw(string code)
    {
        int w = Ui.Px(20), h = Ui.Px(13);
        Bitmap bmp = new Bitmap(w, h);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);
            Rectangle r = new Rectangle(0, 0, w, h);
            if (code == "ru")
            {
                using (SolidBrush b = new SolidBrush(Color.White)) g.FillRectangle(b, new Rectangle(0, 0, w, h / 3));
                using (SolidBrush b = new SolidBrush(Color.FromArgb(0, 57, 166))) g.FillRectangle(b, new Rectangle(0, h / 3, w, h / 3));
                using (SolidBrush b = new SolidBrush(Color.FromArgb(213, 43, 30))) g.FillRectangle(b, new Rectangle(0, 2 * h / 3, w, h - 2 * h / 3));
            }
            else if (code == "uk")
            {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(0, 87, 183))) g.FillRectangle(b, new Rectangle(0, 0, w, h / 2));
                using (SolidBrush b = new SolidBrush(Color.FromArgb(255, 215, 0))) g.FillRectangle(b, new Rectangle(0, h / 2, w, h - h / 2));
            }
            else
            {
                using (SolidBrush b = new SolidBrush(Color.FromArgb(1, 33, 105))) g.FillRectangle(b, r);
                using (Pen wp = new Pen(Color.White, Math.Max(3f, Ui.Px(2.6f)))) { g.DrawLine(wp, w / 2, 0, w / 2, h); g.DrawLine(wp, 0, h / 2, w, h / 2); }
                using (Pen rp = new Pen(Color.FromArgb(200, 16, 46), Math.Max(2f, Ui.Px(1.6f)))) { g.DrawLine(rp, w / 2, 0, w / 2, h); g.DrawLine(rp, 0, h / 2, w, h / 2); }
                using (Pen wp2 = new Pen(Color.FromArgb(210, 255, 255, 255), Math.Max(1f, Ui.Px(0.9f)))) { g.DrawLine(wp2, 0, 0, w, h); g.DrawLine(wp2, 0, h, w, 0); }
            }
            using (Pen pen = new Pen(Color.FromArgb(90, 0, 0, 0))) g.DrawRectangle(pen, 0, 0, w - 1, h - 1);
        }
        return bmp;
    }
}
