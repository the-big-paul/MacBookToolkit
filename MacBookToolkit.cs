using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

public class Settings
{
    public bool ScreenEnabled = true;
    public int IdleSec = 15;
    public int DimPct = 10;
    public int NormalPct = 100;
    public bool TouchEnabled = true;
    public int BlockMs = 700;
    public bool KeyEnabled = true;
    public int DebounceUpMs = 70;
    public int DebounceDownMs = 25;
    public int ScreenOffMin = 2;
    public bool AutoStart = true;
    public bool ShowTray = true;
    public bool TopMost = false;
    public bool ProfileEnabled = false;
    public bool NightLight = false;
    public bool AutoUpdateCheck = true;
    public string Lang = "ru";
    public string Theme = "system";
    public string Accent = "system";

    public const int MaxDebounceDownMs = 30;

    public bool Existed = false;
    public bool ParseFailed = false;

    public static string Dir()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MacBookToolkit");
    }

    public static string FilePath()
    {
        return Path.Combine(Dir(), "settings.ini");
    }

    public static Settings Load()
    {
        Settings s = new Settings();
        string fp = FilePath();
        if (!File.Exists(fp)) return s;
        try
        {
            s = FromLines(File.ReadAllLines(fp));
        }
        catch (Exception ex)
        {
            s = new Settings();
            s.ParseFailed = true;
            Program.LogError("settings load", ex);
        }
        s.Existed = true;
        return s;
    }

    public static Settings FromLines(string[] lines)
    {
        Settings s = new Settings();
        if (lines == null) return s;
        foreach (string line in lines)
        {
            string[] p = line.Split('=');
            if (p.Length != 2) continue;
            string k = p[0].Trim();
            string v = p[1].Trim();
            bool b; int i;
            switch (k)
            {
                case "ScreenEnabled": if (bool.TryParse(v, out b)) s.ScreenEnabled = b; break;
                case "IdleSec": if (int.TryParse(v, out i)) s.IdleSec = i; break;
                case "DimPct": if (int.TryParse(v, out i)) s.DimPct = i; break;
                case "NormalPct": if (int.TryParse(v, out i)) s.NormalPct = i; break;
                case "TouchEnabled": if (bool.TryParse(v, out b)) s.TouchEnabled = b; break;
                case "BlockMs": if (int.TryParse(v, out i)) s.BlockMs = i; break;
                case "KeyEnabled": if (bool.TryParse(v, out b)) s.KeyEnabled = b; break;
                case "DebounceUpMs": if (int.TryParse(v, out i)) s.DebounceUpMs = i; break;
                case "DebounceDownMs": if (int.TryParse(v, out i)) s.DebounceDownMs = i; break;
                case "ScreenOffMin": if (int.TryParse(v, out i)) s.ScreenOffMin = i; break;
                case "AutoStart": if (bool.TryParse(v, out b)) s.AutoStart = b; break;
                case "ShowTray": if (bool.TryParse(v, out b)) s.ShowTray = b; break;
                case "TopMost": if (bool.TryParse(v, out b)) s.TopMost = b; break;
                case "ProfileEnabled": if (bool.TryParse(v, out b)) s.ProfileEnabled = b; break;
                case "NightLight": if (bool.TryParse(v, out b)) s.NightLight = b; break;
                case "AutoUpdateCheck": if (bool.TryParse(v, out b)) s.AutoUpdateCheck = b; break;
                case "Lang": s.Lang = v; break;
                case "Theme": s.Theme = v; break;
                case "Accent": s.Accent = v; break;
            }
        }
        s.Normalize();
        return s;
    }

    static int ClampInt(int v, int min, int max)
    {
        return v < min ? min : (v > max ? max : v);
    }

    public void Normalize()
    {
        IdleSec = ClampInt(IdleSec, 1, 300);
        DimPct = ClampInt(DimPct, 1, 100);
        NormalPct = ClampInt(NormalPct, 1, 100);
        BlockMs = ClampInt(BlockMs, 50, 2000);
        DebounceUpMs = ClampInt(DebounceUpMs, 0, 200);
        DebounceDownMs = ClampInt(DebounceDownMs, 0, MaxDebounceDownMs);
        ScreenOffMin = ClampInt(ScreenOffMin, 1, 60);
        if (DimPct > NormalPct) DimPct = NormalPct;
        if (Lang != "ru" && Lang != "uk" && Lang != "en") Lang = "ru";
        if (Theme != "system" && Theme != "light" && Theme != "dark") Theme = "system";
        bool knownAccent = false;
        for (int i = 0; i < ThemeManager.AccentKeys.Length; i++) if (ThemeManager.AccentKeys[i] == Accent) { knownAccent = true; break; }
        if (!knownAccent) Accent = "system";
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Dir());
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ScreenEnabled=" + ScreenEnabled.ToString());
            sb.AppendLine("IdleSec=" + IdleSec.ToString());
            sb.AppendLine("DimPct=" + DimPct.ToString());
            sb.AppendLine("NormalPct=" + NormalPct.ToString());
            sb.AppendLine("TouchEnabled=" + TouchEnabled.ToString());
            sb.AppendLine("BlockMs=" + BlockMs.ToString());
            sb.AppendLine("KeyEnabled=" + KeyEnabled.ToString());
            sb.AppendLine("DebounceUpMs=" + DebounceUpMs.ToString());
            sb.AppendLine("DebounceDownMs=" + DebounceDownMs.ToString());
            sb.AppendLine("ScreenOffMin=" + ScreenOffMin.ToString());
            sb.AppendLine("AutoStart=" + AutoStart.ToString());
            sb.AppendLine("ShowTray=" + ShowTray.ToString());
            sb.AppendLine("TopMost=" + TopMost.ToString());
            sb.AppendLine("ProfileEnabled=" + ProfileEnabled.ToString());
            sb.AppendLine("NightLight=" + NightLight.ToString());
            sb.AppendLine("AutoUpdateCheck=" + AutoUpdateCheck.ToString());
            sb.AppendLine("Lang=" + Lang.ToString());
            sb.AppendLine("Theme=" + Theme.ToString());
            sb.AppendLine("Accent=" + Accent.ToString());
            string fp = FilePath();
            string tmp = fp + ".tmp";
            File.WriteAllText(tmp, sb.ToString());
            if (File.Exists(fp)) File.Replace(tmp, fp, null);
            else File.Move(tmp, fp);
        }
        catch (Exception ex) { Program.LogError("settings save", ex); }
    }
}

public class KeyDebouncer
{
    readonly bool[] _hasUp = new bool[256];
    readonly bool[] _hasDown = new bool[256];
    readonly uint[] _lastUp = new uint[256];
    readonly uint[] _lastDown = new uint[256];

    public int DroppedUp;
    public int DroppedDown;

    public void ResetDrops()
    {
        DroppedUp = 0;
        DroppedDown = 0;
    }

    public void NoteKeyUp(int vk, uint now)
    {
        int key = vk & 0xFF;
        _lastUp[key] = now;
        _hasUp[key] = true;
    }

    public bool ShouldDropKeyDown(int vk, uint now, int upMs, int downMs)
    {
        int key = vk & 0xFF;
        uint dtUp = _hasUp[key] ? unchecked(now - _lastUp[key]) : uint.MaxValue;
        uint dtDown = _hasDown[key] ? unchecked(now - _lastDown[key]) : uint.MaxValue;
        bool upHit = dtUp < (uint)upMs;
        bool downHit = dtDown < (uint)downMs;
        if (upHit || downHit)
        {
            if (upHit) DroppedUp++;
            if (downHit) DroppedDown++;
            return true;
        }
        _lastDown[key] = now;
        _hasDown[key] = true;
        return false;
    }
}

public static class AutoTuner
{
    public const int DropThreshold = 6;

    public static bool Apply(Settings s, int droppedUp, int droppedDown)
    {
        bool changed = false;
        if (droppedDown >= DropThreshold)
        {
            int v = Math.Max(5, s.DebounceDownMs - 5);
            if (v != s.DebounceDownMs) { s.DebounceDownMs = v; changed = true; }
        }
        if (droppedUp >= DropThreshold)
        {
            int v = Math.Max(20, s.DebounceUpMs - 10);
            if (v != s.DebounceUpMs) { s.DebounceUpMs = v; changed = true; }
        }
        return changed;
    }
}

public class MainForm : Form
{
    delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr SetWindowsHookEx(int idHook, LowLevelProc lpfn, IntPtr hMod, uint dwThreadId);
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool UnhookWindowsHookEx(IntPtr hhk);
    [DllImport("user32.dll")]
    static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
    [DllImport("kernel32.dll")]
    static extern IntPtr GetModuleHandle(string lpModuleName);
    [DllImport("user32.dll")]
    static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
    [DllImport("dwmapi.dll")]
    static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);
    [DllImport("user32.dll")]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr after, int x, int y, int cx, int cy, uint flags);
    [DllImport("user32.dll")]
    static extern bool RegisterHotKey(IntPtr hWnd, int id, uint modifiers, uint vk);
    [DllImport("user32.dll")]
    static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [StructLayout(LayoutKind.Sequential)]
    struct LASTINPUTINFO { public uint cbSize; public uint dwTime; }
    [StructLayout(LayoutKind.Sequential)]
    struct KBDLLHOOKSTRUCT { public int vkCode; public int scanCode; public int flags; public int time; public IntPtr dwExtraInfo; }
    [StructLayout(LayoutKind.Sequential)]
    struct MSLLHOOKSTRUCT { public int ptX; public int ptY; public uint mouseData; public uint flags; public uint time; public IntPtr dwExtraInfo; }

    const int WH_KEYBOARD_LL = 13;
    const int WH_MOUSE_LL = 14;
    const int WM_KEYDOWN = 0x0100;
    const int WM_SYSKEYDOWN = 0x0104;
    const int WM_KEYUP = 0x0101;
    const int WM_SYSKEYUP = 0x0105;
    const int LLKHF_INJECTED = 0x10;
    const uint LLMHF_INJECTED = 0x01;
    const int WM_MOUSEMOVE = 0x0200;
    const int WM_LBUTTONDOWN = 0x0201;
    const int WM_RBUTTONDOWN = 0x0204;
    const int WM_MBUTTONDOWN = 0x0207;
    const int WM_XBUTTONDOWN = 0x020B;

    static LowLevelProc _kbProc;
    static LowLevelProc _msProc;
    static IntPtr _kbHook = IntPtr.Zero;
    static IntPtr _msHook = IntPtr.Zero;
    static uint _lastKeyTick = 0;
    static bool _haveKeyTick = false;
    static readonly KeyDebouncer _debounce = new KeyDebouncer();
    static Settings _s;
    static bool _dimmed = false;
    static bool _brightnessErrorLogged = false;
    static bool _systemApplied = false;

    ThemeColors _theme;
    Dictionary<Control, string> _textKeys = new Dictionary<Control, string>();
    Dictionary<Control, string> _tipKeys = new Dictionary<Control, string>();
    Dictionary<ThemedSpinner, string> _unitKeys = new Dictionary<ThemedSpinner, string>();

    ThemedCard cardScreen, cardKey, cardTouch, cardOptions, cardSystem;
    ThemedCaption caption;
    ThemedLabel lblIdle, lblDim, lblNormal;
    ThemedLabel lblBlock, lblUp, lblDown, lblScreenOff, lblTheme, lblAccent, lblLang;
    ThemedCheckBox chkScreen, chkTouch, chkKey, chkTopMost, chkAutoStart, chkTray;
    ThemedCheckBox chkProfile, chkNight, chkAutoUpdate;
    ThemedLabel lblBattery, lblDrivers;
    ThemedSpinner numIdle, numDim, numNormal, numBlock, numUp, numDown, numScreenOff;
    ThemedSelect cbLang, cbTheme, cbAccent;
    ThemedButton btnSave, btnReset;
    NotifyIcon tray;
    MenuItem miShow, miTopMost, miAutoStart, miExit;
    MenuItem miNoDim, miLock, miSleep, miHibernate, miNight, miKbdLock, miCheckUpd, miUpdate, miBattery;
    System.Windows.Forms.Timer idleTimer;
    System.Windows.Forms.Timer themeWatch;
    System.Windows.Forms.Timer tipTimer;
    System.Windows.Forms.Timer tuneTimer;
    System.Windows.Forms.Timer batteryTimer;
    DateTime _lastBatteryRefresh = DateTime.MinValue;
    NightLightForm _night;
    bool _suppressDim;
    DateTime _suppressUntil = DateTime.MinValue;
    static bool _kbdLocked;
    bool _profileBattery;
    bool _warnedWear;
    bool _warnedDrivers;
    UpdateInfo _update;
    Control _tipControl;
    string _tipKey;
    Point _tipPos;
    bool _tipShown;
    int _tipAt;
    bool _loading = false;
    bool _brightnessOk = false;
    int _brightnessTries = 0;

    public MainForm()
    {
        _s = Settings.Load();
        Texts.Lang = _s.Lang;
        AutoScaleMode = AutoScaleMode.None;
        Font = SystemFonts.MessageBoxFont;
        BuildUi();
        ApplyLanguage();
        ApplyThemeNow();
        ApplyTopMost(_s.TopMost, false);
        InstallHooks();
        idleTimer = new System.Windows.Forms.Timer();
        idleTimer.Interval = 500;
        idleTimer.Tick += IdleTick;
        idleTimer.Start();
        themeWatch = new System.Windows.Forms.Timer();
        themeWatch.Interval = 1500;
        themeWatch.Tick += delegate { WatchSystemTheme(); };
        themeWatch.Start();
        tipTimer = new System.Windows.Forms.Timer();
        tipTimer.Interval = 120;
        tipTimer.Tick += TipTick;
        tipTimer.Start();
        tuneTimer = new System.Windows.Forms.Timer();
        tuneTimer.Interval = 15000;
        tuneTimer.Tick += TuneTick;
        tuneTimer.Start();
        batteryTimer = new System.Windows.Forms.Timer();
        batteryTimer.Interval = 5000;
        batteryTimer.Tick += delegate
        {
            if ((DateTime.Now - _lastBatteryRefresh).TotalSeconds >= 60)
            {
                _lastBatteryRefresh = DateTime.Now;
                RefreshBatteryStatus();
            }
        };
        batteryTimer.Start();
        Move += delegate { TipBalloon.HideTip(); };
    }

    // ---------- UI building ----------
    void Add(Control parent, Control child)
    {
        parent.Controls.Add(child);
    }

    ThemedCard Card(string key, string icon, double x, double y, double w, double h)
    {
        ThemedCard c = new ThemedCard();
        c.TitleKey = key;
        c.Title = Texts.T(key);
        c.TitleIcon = icon;
        c.SetBounds(Ui.Px(x), Ui.Px(y), Ui.Px(w), Ui.Px(h));
        _textKeys[c] = key;
        Controls.Add(c);
        return c;
    }

    ThemedLabel Lbl(string key, string icon, double x, double y, double w, double h, Control parent)
    {
        ThemedLabel l = new ThemedLabel();
        l.IconGlyph = icon;
        l.Text = Texts.T(key);
        l.MaxWidth = (int)Ui.Px(w);
        l.SetBounds(Ui.Px(x), Ui.Px(y), Ui.Px(w), Ui.Px(h));
        _textKeys[l] = key;
        Add(parent, l);
        return l;
    }

    ThemedCheckBox Chk(double x, double y, Control parent, bool val)
    {
        ThemedCheckBox c = new ThemedCheckBox();
        c.Checked = val;
        c.SetBounds(Ui.Px(x), Ui.Px(y), Ui.Px(24), Ui.Px(30));
        Add(parent, c);
        return c;
    }

    void LinkToggle(ThemedLabel label, ThemedCheckBox chk)
    {
        label.Cursor = Cursors.Hand;
        label.Click += delegate { chk.Checked = !chk.Checked; };
    }

    ThemedSpinner Spn(double x, double y, double w, int min, int max, int step, int val, Control parent, string unitKey)
    {
        ThemedSpinner s = new ThemedSpinner();
        s.Minimum = min;
        s.Maximum = max;
        s.Increment = step;
        s.Value = Math.Max(min, Math.Min(max, val));
        s.Suffix = " " + Texts.T(unitKey);
        s.SetBounds(Ui.Px(x), Ui.Px(y), Ui.Px(w), Ui.Px(30));
        _unitKeys[s] = unitKey;
        Add(parent, s);
        return s;
    }

    ThemedSelect Sel(double x, double y, double w, string[] items, Control parent)
    {
        ThemedSelect s = new ThemedSelect();
        s.SetBounds(Ui.Px(x), Ui.Px(y), Ui.Px(w), Ui.Px(30));
        s.Items = items;
        Add(parent, s);
        return s;
    }

    ThemedButton Btn(string key, string icon, double x, double y, double w, double h)
    {
        ThemedButton b = new ThemedButton();
        b.Text = Texts.T(key);
        b.IconGlyph = icon;
        b.SetBounds(Ui.Px(x), Ui.Px(y), Ui.Px(w), Ui.Px(h));
        _textKeys[b] = key;
        Controls.Add(b);
        return b;
    }

    void Tip(Control c, string key)
    {
        _tipKeys[c] = key;
        c.MouseEnter += delegate
        {
            _tipControl = c;
            _tipKey = key;
            _tipPos = Cursor.Position;
            _tipAt = Environment.TickCount;
            _tipShown = false;
        };
        c.MouseMove += delegate { if (_tipControl == c) _tipPos = Cursor.Position; };
        c.MouseLeave += delegate
        {
            if (_tipControl == c)
            {
                _tipControl = null;
                TipBalloon.HideTip();
            }
        };
        c.MouseDown += delegate { TipBalloon.HideTip(); };
    }

    void TipTick(object sender, EventArgs e)
    {
        if (_tipControl == null || _tipShown) return;
        if (ThemedSelect.PopupOpen > 0) return;
        if (Environment.TickCount - _tipAt < 420) return;
        _tipShown = true;
        string key;
        if (_tipKeys.TryGetValue(_tipControl, out key))
        {
            TipBalloon.ShowTip(Texts.T(key), _tipPos, _theme != null ? _theme : ThemeManager.Build(false));
        }
    }

    void ApplyCardLinks()
    {
        if (cardScreen == null) return;
        cardScreen.Links.Clear();
        cardScreen.Links.Add(new CardLink { Text = Texts.T("sec_display_left"), Uri = "ms-settings:powersleep" });
        cardScreen.Links.Add(new CardLink { Text = Texts.T("sec_display_and") });
        cardScreen.Links.Add(new CardLink { Text = Texts.T("sec_display_right"), Uri = "ms-settings:display" });
        cardKey.Links.Clear();
        cardKey.Links.Add(new CardLink { Text = Texts.T("sec_key"), Uri = "ms-settings:keyboard-advanced" });
        cardTouch.Links.Clear();
        cardTouch.Links.Add(new CardLink { Text = Texts.T("sec_touch"), Uri = "ms-settings:devices-touchpad" });
        cardOptions.Links.Clear();
        cardOptions.Links.Add(new CardLink { Text = Texts.T("sec_options"), Uri = "ms-settings:" });
        cardScreen.Invalidate(); cardKey.Invalidate(); cardTouch.Invalidate(); cardOptions.Invalidate();
    }

    void FitLabels()
    {
        foreach (KeyValuePair<Control, string> kv in _textKeys)
        {
            ThemedLabel lbl = kv.Key as ThemedLabel;
            if (lbl != null) lbl.FitContent();
        }
    }

    void TuneTick(object sender, EventArgs e)
    {
        int up = _debounce.DroppedUp;
        int down = _debounce.DroppedDown;
        if (up == 0 && down == 0) return;
        _debounce.ResetDrops();
        if (!AutoTuner.Apply(_s, up, down)) return;
        _s.Save();
        _loading = true;
        numUp.Value = _s.DebounceUpMs;
        numDown.Value = _s.DebounceDownMs;
        _loading = false;
        Program.LogNote("autotune", "drops up=" + up + " down=" + down +
            " -> up=" + _s.DebounceUpMs + " down=" + _s.DebounceDownMs);
    }

    void BuildUi()
    {
        Text = "MacBookToolkit";
        FormBorderStyle = FormBorderStyle.None;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(Ui.Px(700), Ui.Px(608));
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        caption = new ThemedCaption();
        caption.Title = "MacBookToolkit";
        caption.VersionText = "v" + (string.IsNullOrEmpty(Application.ProductVersion) ? "1.2" : Application.ProductVersion);
        caption.VersionTip = Texts.T("version_tip");
        caption.SetBounds(0, 0, Ui.Px(700), Ui.Px(40));
        caption.CloseClick += delegate { Close(); };
        caption.MinimizeClick += delegate { WindowState = FormWindowState.Minimized; };
        caption.VersionClick += delegate { OpenRepository(); };
        Controls.Add(caption);

        ThemedLabel lb;

        cardScreen = Card("sec_display", "", 16, 52, 328, 208);
        lb = Lbl("chk_screen", "\uE706", 14, 32, 270, 30, cardScreen);
        chkScreen = Chk(292, 32, cardScreen, _s.ScreenEnabled); LinkToggle(lb, chkScreen);
        Tip(lb, "tip_chk_screen"); Tip(chkScreen, "tip_chk_screen");
        lblIdle = Lbl("lbl_idle", "\uE823", 14, 66, 206, 30, cardScreen);
        numIdle = Spn(234, 66, 80, 1, 300, 1, _s.IdleSec, cardScreen, "unit_sec");
        lblDim = Lbl("lbl_dim", "\uE793", 14, 100, 206, 30, cardScreen);
        numDim = Spn(234, 100, 80, 1, 100, 1, _s.DimPct, cardScreen, "unit_pct");
        lblNormal = Lbl("lbl_normal", "\uE7F4", 14, 134, 206, 30, cardScreen);
        numNormal = Spn(234, 134, 80, 1, 100, 1, _s.NormalPct, cardScreen, "unit_pct");
        lblScreenOff = Lbl("lbl_screenoff", "\uE7E8", 14, 168, 206, 30, cardScreen);
        numScreenOff = Spn(234, 168, 80, 1, 60, 1, _s.ScreenOffMin, cardScreen, "unit_min");

        cardKey = Card("sec_key", "", 16, 270, 328, 174);
        lb = Lbl("chk_key", "\uE765", 14, 32, 270, 30, cardKey);
        chkKey = Chk(292, 32, cardKey, _s.KeyEnabled); LinkToggle(lb, chkKey);
        Tip(lb, "tip_chk_key"); Tip(chkKey, "tip_chk_key");
        lblUp = Lbl("lbl_up", "\uE916", 14, 66, 206, 30, cardKey);
        numUp = Spn(234, 66, 80, 0, 200, 5, _s.DebounceUpMs, cardKey, "unit_ms");
        lblDown = Lbl("lbl_down", "\uE945", 14, 100, 206, 30, cardKey);
        numDown = Spn(234, 100, 80, 0, Settings.MaxDebounceDownMs, 5, _s.DebounceDownMs, cardKey, "unit_ms");

        cardTouch = Card("sec_touch", "", 356, 52, 328, 106);
        lb = Lbl("chk_touch", "\uE7C9", 14, 32, 270, 30, cardTouch);
        chkTouch = Chk(292, 32, cardTouch, _s.TouchEnabled); LinkToggle(lb, chkTouch);
        Tip(lb, "tip_chk_touch"); Tip(chkTouch, "tip_chk_touch");
        lblBlock = Lbl("lbl_block", "\uE8B0", 14, 66, 206, 30, cardTouch);
        numBlock = Spn(234, 66, 80, 50, 2000, 50, _s.BlockMs, cardTouch, "unit_ms");

        cardOptions = Card("sec_options", "", 356, 168, 328, 276);
        lblTheme = Lbl("lbl_theme", "\uE790", 14, 32, 140, 30, cardOptions);
        cbTheme = Sel(158, 32, 156, ThemeItems(), cardOptions);
        lblAccent = Lbl("lbl_accent", "", 14, 66, 140, 30, cardOptions);
        lblAccent.IconDrop = true;
        cbAccent = Sel(158, 66, 156, AccentItems(), cardOptions);
        cbAccent.ItemImages = AccentSwatches();
        lblLang = Lbl("lbl_lang", "\uE774", 14, 100, 140, 30, cardOptions);
        cbLang = Sel(158, 100, 156, new string[] { "Русский", "Українська", "English" }, cardOptions);
        cbLang.ItemImages = new Image[] { Flags.Draw("ru"), Flags.Draw("uk"), Flags.Draw("en") };
        lb = Lbl("chk_topmost", "\uE718", 14, 134, 270, 30, cardOptions);
        chkTopMost = Chk(292, 134, cardOptions, _s.TopMost); LinkToggle(lb, chkTopMost);
        Tip(lb, "tip_topmost"); Tip(chkTopMost, "tip_topmost");
        lb = Lbl("chk_autostart", "\uE768", 14, 168, 270, 30, cardOptions);
        chkAutoStart = Chk(292, 168, cardOptions, _s.AutoStart); LinkToggle(lb, chkAutoStart);
        Tip(lb, "tip_autostart"); Tip(chkAutoStart, "tip_autostart");
        lb = Lbl("chk_tray", "\uE8A5", 14, 202, 270, 30, cardOptions);
        chkTray = Chk(292, 202, cardOptions, _s.ShowTray); LinkToggle(lb, chkTray);
        Tip(lb, "tip_tray"); Tip(chkTray, "tip_tray");
        lb = Lbl("chk_autoupdate", "\uE72C", 14, 236, 270, 30, cardOptions);
        chkAutoUpdate = Chk(292, 236, cardOptions, _s.AutoUpdateCheck); LinkToggle(lb, chkAutoUpdate);
        Tip(lb, "tip_autoupdate"); Tip(chkAutoUpdate, "tip_autoupdate");
        ApplyCardLinks();

        ThemedCard cardStatus = new ThemedCard();
        cardStatus.SetBounds(Ui.Px(16), Ui.Px(508), Ui.Px(668), Ui.Px(84));
        Controls.Add(cardStatus);
        lblDrivers = Lbl("status_drivers_na", "\uE713", 14, 8, 620, 30, cardStatus);
        _textKeys.Remove(lblDrivers);
        Tip(lblDrivers, "tip_drivers");
        lblBattery = Lbl("status_battery_na", "\uE7E8", 14, 42, 620, 30, cardStatus);
        _textKeys.Remove(lblBattery);
        Tip(lblBattery, "tip_battery");

        btnSave = Btn("btn_save", "\uE74E", 16, 456, 328, 40);
        btnReset = Btn("btn_reset", "\uE72C", 356, 456, 328, 40);
        btnReset.Secondary = true;

        chkTopMost.CheckedChanged += delegate { if (!_loading) ApplyTopMost(chkTopMost.Checked, true); };
        cbLang.SelectedIndexChanged += LangChanged;
        cbTheme.SelectedIndexChanged += ThemeChanged;
        cbAccent.SelectedIndexChanged += AccentChanged;
        btnSave.Click += SaveClick;
        btnReset.Click += ResetClick;

        _loading = true;
        cbLang.SelectedIndex = _s.Lang == "en" ? 2 : (_s.Lang == "uk" ? 1 : 0);
        cbTheme.SelectedIndex = _s.Theme == "light" ? 1 : (_s.Theme == "dark" ? 2 : 0);
        cbAccent.SelectedIndex = Math.Max(0, Array.IndexOf(ThemeManager.AccentKeys, _s.Accent));
        _loading = false;

        Tip(lblIdle, "tip_idle"); Tip(numIdle, "tip_idle");
        Tip(lblDim, "tip_dim"); Tip(numDim, "tip_dim");
        Tip(lblNormal, "tip_normal"); Tip(numNormal, "tip_normal");
        Tip(lblScreenOff, "tip_screenoff"); Tip(numScreenOff, "tip_screenoff");
        Tip(lblUp, "tip_up"); Tip(numUp, "tip_up");
        Tip(lblDown, "tip_down"); Tip(numDown, "tip_down");
        Tip(lblBlock, "tip_block"); Tip(numBlock, "tip_block");
        Tip(lblTheme, "tip_theme"); Tip(cbTheme, "tip_theme");
        Tip(lblAccent, "tip_accent"); Tip(cbAccent, "tip_accent");
        Tip(lblLang, "tip_lang"); Tip(cbLang, "tip_lang");
        Tip(btnSave, "tip_save"); Tip(btnReset, "tip_reset");

        RefreshAccentVisuals();
        RefreshBatteryStatus();
        RefreshDriverStatus();
        BuildTray();
    }

    string[] ThemeItems()
    {
        return new string[] { Texts.T("theme_system"), Texts.T("theme_light"), Texts.T("theme_dark") };
    }

    string[] AccentItems()
    {
        return new string[]
        {
            Texts.T("accent_system"), Texts.T("accent_blue"), Texts.T("accent_purple"), Texts.T("accent_pink"),
            Texts.T("accent_red"), Texts.T("accent_orange"), Texts.T("accent_yellow"), Texts.T("accent_green"),
            Texts.T("accent_teal"), Texts.T("accent_gray")
        };
    }

    Image[] AccentSwatches()
    {
        Image[] imgs = new Image[ThemeManager.AccentKeys.Length];
        for (int i = 0; i < ThemeManager.AccentKeys.Length; i++) imgs[i] = Swatches.Accent(ThemeManager.AccentKeys[i]);
        return imgs;
    }

    void RefreshAccentVisuals()
    {
        if (lblAccent != null) { lblAccent.IconDrop = true; lblAccent.IconImage = null; }
        if (cbAccent != null) cbAccent.ItemImages = AccentSwatches();
    }

    void BuildTray()
    {
        tray = new NotifyIcon();
        tray.Icon = this.Icon != null ? this.Icon : SystemIcons.Application;
        tray.Text = "MacBookToolkit";
        ContextMenu cm = new ContextMenu();
        miShow = new MenuItem(Texts.T("tray_show"));
        miShow.Click += delegate { ShowFromTray(); };
        miTopMost = new MenuItem(Texts.T("tray_topmost"));
        miTopMost.Click += delegate { ApplyTopMost(!TopMost, true); };
        miAutoStart = new MenuItem(Texts.T("tray_autostart"));
        miAutoStart.Click += delegate
        {
            _s.AutoStart = !_s.AutoStart;
            _s.Save();
            SetAutoStart(_s.AutoStart);
            _loading = true; chkAutoStart.Checked = _s.AutoStart; _loading = false;
            SyncTrayMenu();
        };
        miExit = new MenuItem(Texts.T("tray_exit"));
        miExit.Click += ExitClick;

        miNoDim = new MenuItem(Texts.T("tray_nodim"));
        miNoDim.Click += delegate { ToggleSuppressDim(); };
        miKbdLock = new MenuItem(Texts.T("tray_kbdlock"));
        miKbdLock.Click += delegate { ToggleKeyboardLock(); };

        miLock = new MenuItem(Texts.T("tray_lock"));
        miLock.Click += delegate { QuickActions.Lock(); };
        miSleep = new MenuItem(Texts.T("tray_sleep"));
        miSleep.Click += delegate { QuickActions.Sleep(); };
        miHibernate = new MenuItem(Texts.T("tray_hibernate"));
        miHibernate.Click += delegate { QuickActions.Hibernate(); };

        miBattery = new MenuItem(Texts.T("status_battery_na"));
        miBattery.Enabled = false;
        miCheckUpd = new MenuItem(Texts.T("tray_checkupd"));
        miCheckUpd.Click += delegate { CheckUpdates(true); };
        miUpdate = new MenuItem(Texts.T("tray_update").Replace("{0}", "").Trim());
        miUpdate.Visible = false;
        miUpdate.Click += delegate
        {
            if (_update == null) return;
            if (!string.IsNullOrEmpty(_update.AssetUrl)) { DownloadUpdate(); }
            else if (!string.IsNullOrEmpty(_update.PageUrl))
            {
                try { Process.Start(new ProcessStartInfo(_update.PageUrl) { UseShellExecute = true }); } catch { }
            }
        };

        cm.MenuItems.Add(miShow);
        cm.MenuItems.Add(miTopMost);
        cm.MenuItems.Add(miAutoStart);
        cm.MenuItems.Add(new MenuItem("-"));
        cm.MenuItems.Add(miNoDim);
        cm.MenuItems.Add(miKbdLock);
        cm.MenuItems.Add(new MenuItem("-"));
        cm.MenuItems.Add(miLock);
        cm.MenuItems.Add(miSleep);
        cm.MenuItems.Add(miHibernate);
        cm.MenuItems.Add(new MenuItem("-"));
        cm.MenuItems.Add(miBattery);
        cm.MenuItems.Add(miCheckUpd);
        cm.MenuItems.Add(miUpdate);
        cm.MenuItems.Add(new MenuItem("-"));
        cm.MenuItems.Add(miExit);
        tray.ContextMenu = cm;
        tray.MouseClick += delegate(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) ShowFromTray();
        };
        tray.DoubleClick += delegate { ShowFromTray(); };
        tray.Visible = _s.ShowTray;
        SyncTrayMenu();
    }

    void SyncTrayMenu()
    {
        if (miTopMost != null) miTopMost.Checked = TopMost;
        if (miAutoStart != null) miAutoStart.Checked = _s.AutoStart;
        if (miKbdLock != null) miKbdLock.Checked = _kbdLocked;
        if (miNoDim != null) miNoDim.Checked = _suppressDim;
    }

    void ShowFromTray()
    {
        Show();
        WindowState = FormWindowState.Normal;
        Activate();
        BringToFront();
    }

    void OpenRepository()
    {
        try
        {
            Process.Start(new ProcessStartInfo("https://github.com/the-big-paul/MacBookToolkit") { UseShellExecute = true });
        }
        catch (Exception ex) { Program.LogError("open repository", ex); }
    }

    // ---------- profiles, battery, night light, updates ----------
    bool IsOnBattery()
    {
        try { return SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline; }
        catch { return false; }
    }

    int EffIdleSec { get { return (_s.ProfileEnabled && IsOnBattery()) ? 15 : _s.IdleSec; } }
    int EffDimPct { get { return (_s.ProfileEnabled && IsOnBattery()) ? 10 : _s.DimPct; } }
    int EffNormalPct { get { return (_s.ProfileEnabled && IsOnBattery()) ? 60 : _s.NormalPct; } }
    int EffScreenOffMin { get { return (_s.ProfileEnabled && IsOnBattery()) ? 5 : _s.ScreenOffMin; } }

    void RefreshBatteryStatus()
    {
        if (lblBattery == null) return;
        int design, full, cycles, charge;
        if (BatteryInfo.TryRead(out design, out full, out cycles, out charge))
        {
            int wear = BatteryInfo.WearPercent(design, full);
            string cyc = cycles > 0 ? cycles.ToString() : "—";
            lblBattery.Text = string.Format(Texts.T("status_battery"), charge, wear, cyc);
            if (!_warnedWear && wear >= 20)
            {
                _warnedWear = true;
                if (tray != null) tray.ShowBalloonTip(4000, "MacBookToolkit", string.Format(Texts.T("msg_wear"), wear), ToolTipIcon.Warning);
            }
        }
        else
        {
            int c = BatteryInfo.WmiCharge();
            lblBattery.Text = Texts.T("status_battery_na");
            if (c > 0) lblBattery.Text = string.Format(Texts.T("status_battery"), c, "—", "—");
        }
        if (miBattery != null) miBattery.Text = lblBattery.Text;
        lblBattery.FitContent();
    }

    void RefreshDriverStatus()
    {
        if (lblDrivers == null) return;
        bool ok = BootCampCheck.DriversPresent();
        lblDrivers.Text = Texts.T(ok ? "status_drivers_ok" : "status_drivers_na");
        lblDrivers.FitContent();
        if (!ok && !_warnedDrivers)
        {
            _warnedDrivers = true;
            if (tray != null) tray.ShowBalloonTip(4000, "MacBookToolkit", Texts.T("msg_drivers"), ToolTipIcon.Warning);
        }
    }

    void ApplyProfileIfNeeded()
    {
        bool battery = IsOnBattery();
        if (battery == _profileBattery) return;
        _profileBattery = battery;
        if (!_s.ProfileEnabled) return;
        _brightnessOk = SetBrightness(EffNormalPct);
        _dimmed = !_brightnessOk;
        ThreadPool.QueueUserWorkItem(delegate(object state) { ApplyPower(); });
        Program.LogNote("profile", (battery ? "battery" : "ac") + " profile applied, brightness=" + EffNormalPct + "%");
    }

    void ToggleNightLight(bool on)
    {
        _s.NightLight = on;
        _s.Save();
        if (on)
        {
            if (_night == null) { _night = new NightLightForm(); _night.Show(); }
        }
        else if (_night != null) { _night.Close(); _night.Dispose(); _night = null; }
        if (miNight != null) miNight.Checked = on;
        if (chkNight != null) { _loading = true; chkNight.Checked = on; _loading = false; }
        if (tray != null) tray.ShowBalloonTip(1200, "MacBookToolkit", Texts.T(on ? "msg_night_on" : "msg_night_off"), ToolTipIcon.Info);
    }

    void ToggleKeyboardLock()
    {
        _kbdLocked = !_kbdLocked;
        if (miKbdLock != null) miKbdLock.Checked = _kbdLocked;
        if (tray != null) tray.ShowBalloonTip(2500, "MacBookToolkit", Texts.T(_kbdLocked ? "msg_kbd_lock" : "msg_kbd_unlock"), ToolTipIcon.Info);
    }

    void ToggleSuppressDim()
    {
        if (_suppressDim) { _suppressDim = false; _suppressUntil = DateTime.MinValue; }
        else { _suppressDim = true; _suppressUntil = DateTime.Now.AddMinutes(10); }
        if (miNoDim != null) miNoDim.Checked = _suppressDim;
        if (_suppressDim && _dimmed) RestoreBrightnessIfDimmed();
    }

    void CheckUpdates(bool manual)
    {
        UpdateChecker.CheckAsync(delegate(UpdateInfo info)
        {
            try { BeginInvoke(new Action(delegate { ApplyUpdateInfo(info, manual); })); } catch { }
        });
    }

    void ApplyUpdateInfo(UpdateInfo info, bool manual)
    {
        _update = info;
        string current = "1.2.0.0";
        bool newer = info != null && !string.IsNullOrEmpty(info.Version) && UpdateChecker.IsNewer(current, info.Version);
        if (miUpdate != null)
        {
            miUpdate.Visible = newer;
            miUpdate.Text = string.Format(Texts.T("tray_update"), info != null ? info.Version : "");
            if (newer && info.AssetUrl != "") miUpdate.Text = string.Format(Texts.T("tray_download"), info.Version);
        }
        if (manual || newer)
        {
            string msg = newer ? string.Format(Texts.T("msg_upd_found"), info.Version) : Texts.T("msg_upd_none");
            if (tray != null) tray.ShowBalloonTip(4000, "MacBookToolkit", msg, newer ? ToolTipIcon.Info : ToolTipIcon.None);
        }
    }

    void DownloadUpdate()
    {
        if (_update == null || string.IsNullOrEmpty(_update.AssetUrl)) return;
        string url = _update.AssetUrl;
        ThreadPool.QueueUserWorkItem(delegate
        {
            try
            {
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                Directory.CreateDirectory(dir);
                string file = Path.Combine(dir, "MacBookToolkit-" + _update.Version + ".exe");
                using (WebClient wc = new WebClient()) wc.DownloadFile(url, file);
                try { BeginInvoke(new Action(delegate { tray.ShowBalloonTip(5000, "MacBookToolkit", string.Format(Texts.T("msg_upd_downloaded"), file), ToolTipIcon.Info); })); } catch { }
                System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + file + "\"");
            }
            catch { }
        });
    }

    // ---------- hotkeys ----------
    const int HOTKEY_DIM = 0xB001, HOTKEY_TOUCH = 0xB002, HOTKEY_TOP = 0xB003, HOTKEY_KBD = 0xB005;

    void RegisterHotkeys()
    {
        if (!IsHandleCreated) return;
        try
        {
            RegisterHotKey(Handle, HOTKEY_DIM, 0x0003, 0x44);
            RegisterHotKey(Handle, HOTKEY_TOUCH, 0x0003, 0x54);
            RegisterHotKey(Handle, HOTKEY_TOP, 0x0003, 0x50);
            RegisterHotKey(Handle, HOTKEY_KBD, 0x0003, 0x4B);
        }
        catch { }
    }

    void UnregisterHotkeys()
    {
        if (!IsHandleCreated) return;
        try
        {
            UnregisterHotKey(Handle, HOTKEY_DIM);
            UnregisterHotKey(Handle, HOTKEY_TOUCH);
            UnregisterHotKey(Handle, HOTKEY_TOP);
            UnregisterHotKey(Handle, HOTKEY_KBD);
        }
        catch { }
    }

    void HandleHotkey(int id)
    {
        if (id == HOTKEY_DIM) { _loading = true; chkScreen.Checked = !chkScreen.Checked; _loading = false; }
        else if (id == HOTKEY_TOUCH) { _loading = true; chkTouch.Checked = !chkTouch.Checked; _loading = false; }
        else if (id == HOTKEY_TOP) ApplyTopMost(!TopMost, true);
        else if (id == HOTKEY_KBD) ToggleKeyboardLock();
    }

    // ---------- language / theme ----------
    void ApplyLanguage()
    {
        _loading = true;
        Texts.Lang = _s.Lang;
        Text = "MacBookToolkit";
        foreach (KeyValuePair<Control, string> kv in _textKeys)
        {
            ThemedCard card = kv.Key as ThemedCard;
            if (card != null) card.Title = Texts.T(kv.Value);
            else kv.Key.Text = Texts.T(kv.Value);
        }
        if (caption != null)
        {
            caption.VersionText = "v" + (string.IsNullOrEmpty(Application.ProductVersion) ? "1.2" : Application.ProductVersion);
            caption.VersionTip = Texts.T("version_tip");
            caption.Invalidate();
        }
        if (cbTheme != null)
        {
            int idx = cbTheme.SelectedIndex;
            cbTheme.Items = ThemeItems();
            cbTheme.SelectedIndex = idx < 0 ? 0 : idx;
        }
        if (cbAccent != null)
        {
            int idxA = cbAccent.SelectedIndex;
            cbAccent.Items = AccentItems();
            cbAccent.SelectedIndex = idxA < 0 ? 0 : idxA;
            cbAccent.ItemImages = AccentSwatches();
        }
        RefreshAccentVisuals();
        ApplyCardLinks();
        foreach (KeyValuePair<ThemedSpinner, string> kv in _unitKeys)
        {
            kv.Key.Suffix = " " + Texts.T(kv.Value);
            kv.Key.Invalidate();
        }
        if (miShow != null) miShow.Text = Texts.T("tray_show");
        if (miTopMost != null) miTopMost.Text = Texts.T("tray_topmost");
        if (miAutoStart != null) miAutoStart.Text = Texts.T("tray_autostart");
        if (miExit != null) miExit.Text = Texts.T("tray_exit");
        if (miNoDim != null) miNoDim.Text = Texts.T("tray_nodim");
        if (miKbdLock != null) miKbdLock.Text = Texts.T("tray_kbdlock");
        if (miLock != null) miLock.Text = Texts.T("tray_lock");
        if (miSleep != null) miSleep.Text = Texts.T("tray_sleep");
        if (miHibernate != null) miHibernate.Text = Texts.T("tray_hibernate");
        if (miCheckUpd != null) miCheckUpd.Text = Texts.T("tray_checkupd");
        if (_update != null && miUpdate != null && miUpdate.Visible)
            miUpdate.Text = string.Format(Texts.T(string.IsNullOrEmpty(_update.AssetUrl) ? "tray_update" : "tray_download"), _update.Version);
        RefreshBatteryStatus();
        RefreshDriverStatus();
        _loading = false;
        FitLabels();
        Invalidate(true);
    }

    void ThemeChanged(object sender, EventArgs e)
    {
        if (_loading) return;
        _s.Theme = cbTheme.SelectedIndex == 1 ? "light" : (cbTheme.SelectedIndex == 2 ? "dark" : "system");
        _s.Save();
        ApplyThemeNow();
    }

    void WatchSystemTheme()
    {
        if (_s == null || _s.Theme != "system" || _theme == null) return;
        ThemeColors now = ThemeManager.Resolve(_s.Theme, _s.Accent);
        if (now.Dark != _theme.Dark || now.Accent.ToArgb() != _theme.Accent.ToArgb())
        {
            ApplyThemeNow();
            Program.LogNote("theme", "system theme/accent changed, reapplied (dark=" + now.Dark + ")");
        }
    }

    void AccentChanged(object sender, EventArgs e)
    {
        if (_loading) return;
        _s.Accent = ThemeManager.AccentKeys[Math.Max(0, cbAccent.SelectedIndex)];
        _s.Save();
        RefreshAccentVisuals();
        ApplyThemeNow();
    }

    void ApplyThemeNow()
    {
        _theme = ThemeManager.Resolve(_s.Theme, _s.Accent);
        ThemeManager.ApplyTheme(this, _theme);
        RefreshAccentVisuals();
        ApplyTitleBarTheme();
        Invalidate(true);
    }

    void ApplyTitleBarTheme()
    {
        if (!IsHandleCreated) return;
        try
        {
            int dark = (_theme != null && _theme.Dark) ? 1 : 0;
            DwmSetWindowAttribute(Handle, 20, ref dark, 4);
            DwmSetWindowAttribute(Handle, 19, ref dark, 4);
            int round = 2;
            DwmSetWindowAttribute(Handle, 33, ref round, 4);
            SetWindowPos(Handle, IntPtr.Zero, 0, 0, 0, 0, 0x0001 | 0x0002 | 0x0004 | 0x0010 | 0x0020);
        }
        catch { }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        ApplyTitleBarTheme();
        RegisterHotkeys();
    }

    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams cp = base.CreateParams;
            cp.ClassStyle |= 0x20000;
            return cp;
        }
    }

    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);
        if (m.Msg == 0x0312) { HandleHotkey(m.WParam.ToInt32()); return; }
        if (m.Msg == 0x001A || m.Msg == 0x031A || m.Msg == 0x0320)
        {
            if (_s != null && _s.Theme == "system" && IsHandleCreated)
            {
                try { BeginInvoke(new Action(ApplyThemeNow)); } catch { }
            }
        }
    }

    // ---------- settings actions ----------
    void ApplyTopMost(bool on, bool persist)
    {
        TopMost = on;
        _s.TopMost = on;
        if (chkTopMost != null) { _loading = true; chkTopMost.Checked = on; _loading = false; }
        SyncTrayMenu();
        if (persist) _s.Save();
    }

    void LangChanged(object sender, EventArgs e)
    {
        if (_loading) return;
        _s.Lang = cbLang.SelectedIndex == 2 ? "en" : (cbLang.SelectedIndex == 1 ? "uk" : "ru");
        bool top = chkTopMost.Checked;
        ApplyLanguage();
        ApplyThemeNow();
        ApplyTopMost(top, true);
    }

    void SaveClick(object sender, EventArgs e)
    {
        _s.ScreenEnabled = chkScreen.Checked;
        _s.IdleSec = numIdle.Value;
        _s.DimPct = numDim.Value;
        _s.NormalPct = numNormal.Value;
        _s.TouchEnabled = chkTouch.Checked;
        _s.BlockMs = numBlock.Value;
        _s.KeyEnabled = chkKey.Checked;
        _s.DebounceUpMs = numUp.Value;
        _s.DebounceDownMs = numDown.Value;
        _s.ScreenOffMin = numScreenOff.Value;
        _s.AutoStart = chkAutoStart.Checked;
        _s.ShowTray = chkTray.Checked;
        _s.TopMost = chkTopMost.Checked;
        _s.Accent = ThemeManager.AccentKeys[Math.Max(0, cbAccent.SelectedIndex)];
        _s.AutoUpdateCheck = chkAutoUpdate.Checked;
        if (_s.DimPct > _s.NormalPct) { _s.DimPct = _s.NormalPct; numDim.Value = _s.DimPct; }
        _s.Save();
        SetAutoStart(_s.AutoStart);
        tray.Visible = _s.ShowTray;
        ApplyTopMost(_s.TopMost, false);
        ApplyThemeNow();
        RefreshAccentVisuals();
        _brightnessOk = SetBrightness(EffNormalPct);
        _dimmed = !_brightnessOk;
        ThreadPool.QueueUserWorkItem(delegate(object state) { ApplyPower(); });
        SyncTrayMenu();
        Program.LogNote("save", "settings saved and applied");
    }

    void ResetClick(object sender, EventArgs e)
    {
        if (MessageBox.Show(Texts.T("reset_confirm"), Texts.T("reset_title"),
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        Settings d = new Settings();
        d.Lang = _s.Lang;
        d.Theme = _s.Theme;
        d.Accent = _s.Accent;
        _s = d;
        _loading = true;
        chkScreen.Checked = d.ScreenEnabled;
        numIdle.Value = d.IdleSec;
        numDim.Value = d.DimPct;
        numNormal.Value = d.NormalPct;
        chkTouch.Checked = d.TouchEnabled;
        numBlock.Value = d.BlockMs;
        chkKey.Checked = d.KeyEnabled;
        numUp.Value = d.DebounceUpMs;
        numDown.Value = d.DebounceDownMs;
        numScreenOff.Value = d.ScreenOffMin;
        chkAutoStart.Checked = d.AutoStart;
        chkTray.Checked = d.ShowTray;
        chkAutoUpdate.Checked = d.AutoUpdateCheck;
        _loading = false;
        _s.Save();
        SetAutoStart(_s.AutoStart);
        tray.Visible = _s.ShowTray;
        ApplyTopMost(_s.TopMost, false);
        ApplyThemeNow();
        RefreshAccentVisuals();
        _brightnessOk = SetBrightness(EffNormalPct);
        _dimmed = !_brightnessOk;
        ThreadPool.QueueUserWorkItem(delegate(object state) { ApplyPower(); });
        SyncTrayMenu();
        MessageBox.Show(Texts.T("reset_done"), "MacBookToolkit");
    }

    // ---------- lifecycle ----------
    void ExitClick(object sender, EventArgs e)
    {
        tray.Visible = false;
        idleTimer.Stop();
        RestoreBrightnessIfDimmed();
        RemoveHooks();
        Application.Exit();
    }

    void FormResize(object sender, EventArgs e)
    {
        if (WindowState == FormWindowState.Minimized && _s.ShowTray)
        {
            Hide();
            tray.ShowBalloonTip(1500, "MacBookToolkit", Texts.T("tray_min"), ToolTipIcon.Info);
        }
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        if (_systemApplied || _s == null) return;
        _systemApplied = true;
        FitLabels();
        Calibrate();
        ApplyTitleBarTheme();
        Resize += FormResize;
        RefreshBatteryStatus();
        RefreshDriverStatus();
        if (_s.AutoUpdateCheck) CheckUpdates(false);
    }

    void Calibrate()
    {
        _dimmed = false;
        _brightnessTries = 0;
        _brightnessOk = SetBrightness(EffNormalPct);
        if (_brightnessOk) _brightnessTries = 99;
        SetAutoStart(_s.AutoStart);
        tray.Visible = _s.ShowTray;
        ApplyTopMost(_s.TopMost, false);
        ThreadPool.QueueUserWorkItem(delegate(object state) { ApplyPower(); });
        Program.LogNote("calibrate", "brightness=" + EffNormalPct + "%; video idle=" +
            EffScreenOffMin + " min; autostart=" + _s.AutoStart + "; topmost=" + _s.TopMost +
            "; theme=" + _s.Theme + "; accent=" + _s.Accent + "; lang=" + _s.Lang);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing && _s.ShowTray)
        {
            e.Cancel = true;
            Hide();
            tray.ShowBalloonTip(1500, "MacBookToolkit", Texts.T("tray_min"), ToolTipIcon.Info);
            return;
        }
        RestoreBrightnessIfDimmed();
        base.OnFormClosing(e);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        try
        {
            if (idleTimer != null) { idleTimer.Stop(); idleTimer.Dispose(); idleTimer = null; }
            if (themeWatch != null) { themeWatch.Stop(); themeWatch.Dispose(); themeWatch = null; }
            if (tipTimer != null) { tipTimer.Stop(); tipTimer.Dispose(); tipTimer = null; }
            if (tuneTimer != null) { tuneTimer.Stop(); tuneTimer.Dispose(); tuneTimer = null; }
            if (batteryTimer != null) { batteryTimer.Stop(); batteryTimer.Dispose(); batteryTimer = null; }
            if (_night != null) { _night.Close(); _night.Dispose(); _night = null; }
            _kbdLocked = false;
            UnregisterHotkeys();
            TipBalloon.HideTip();
            RemoveHooks();
            if (tray != null) { tray.Visible = false; tray.Dispose(); tray = null; }
            if (Font != null) Font.Dispose();
            if (Icon != null) Icon.Dispose();
        }
        catch { }
        base.OnFormClosed(e);
    }

    // ---------- system integration ----------
    void ApplyPower()
    {
        try
        {
            int sec = EffScreenOffMin * 60;
            Run("powercfg", "/setacvalueindex SCHEME_CURRENT SUB_VIDEO VIDEOIDLE " + sec.ToString());
            Run("powercfg", "/setdcvalueindex SCHEME_CURRENT SUB_VIDEO VIDEOIDLE " + sec.ToString());
            Run("powercfg", "/setactive SCHEME_CURRENT");
        }
        catch (Exception ex) { Program.LogError("apply power", ex); }
    }

    static void Run(string exe, string args)
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo(exe, args);
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            using (Process p = Process.Start(psi))
            {
                if (p == null) { Program.LogNote("run", exe + ": not started"); return; }
                if (!p.WaitForExit(5000)) { Program.LogNote("run", exe + " " + args + ": timeout"); return; }
                if (p.ExitCode != 0) Program.LogNote("run", exe + " " + args + ": exit " + p.ExitCode);
            }
        }
        catch (Exception ex) { Program.LogError("run " + exe, ex); }
    }

    void SetAutoStart(bool on)
    {
        try
        {
            using (RegistryKey k = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if (k == null) return;
                if (on) k.SetValue("MacBookToolkit", "\"" + Application.ExecutablePath + "\"");
                else k.DeleteValue("MacBookToolkit", false);
            }
        }
        catch (Exception ex) { Program.LogError("autostart", ex); }
    }

    static bool SetBrightness(int pct)
    {
        if (pct < 0) return false;
        if (pct > 100) pct = 100;
        try
        {
            int ok = 0;
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\wmi", "SELECT * FROM WmiMonitorBrightnessMethods"))
            using (ManagementObjectCollection monitors = searcher.Get())
            {
                foreach (ManagementObject m in monitors)
                {
                    using (m)
                    {
                        try { m.InvokeMethod("WmiSetBrightness", new object[] { (uint)0, (byte)pct }); ok++; }
                        catch (Exception ex) { Program.LogNote("brightness", "one monitor failed: " + ex.Message); }
                    }
                }
            }
            if (ok > 0) { _brightnessErrorLogged = false; return true; }
            if (!_brightnessErrorLogged) { _brightnessErrorLogged = true; Program.LogNote("brightness", "no WMI brightness control available"); }
            return false;
        }
        catch (Exception ex)
        {
            if (!_brightnessErrorLogged) { _brightnessErrorLogged = true; Program.LogError("brightness", ex); }
            return false;
        }
    }

    static uint IdleMs()
    {
        LASTINPUTINFO lii = new LASTINPUTINFO();
        lii.cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO));
        if (!GetLastInputInfo(ref lii)) return 0;
        return (uint)(Environment.TickCount - (int)lii.dwTime);
    }

    static bool WantsDim(uint idleMs, Settings s)
    {
        return s.ScreenEnabled && idleMs >= (uint)(s.IdleSec * 1000);
    }

    void IdleTick(object sender, EventArgs e)
    {
        if (!_brightnessOk && _brightnessTries < 5)
        {
            _brightnessTries++;
            if (SetBrightness(EffNormalPct)) { _brightnessOk = true; _dimmed = false; }
        }
        if (_suppressDim && DateTime.Now > _suppressUntil) { _suppressDim = false; if (miNoDim != null) miNoDim.Checked = false; }
        if (_suppressDim)
        {
            if (_dimmed && SetBrightness(EffNormalPct)) _dimmed = false;
            return;
        }
        bool wantDim = _s.ScreenEnabled && IdleMs() >= (uint)(EffIdleSec * 1000) && _brightnessOk;
        if (wantDim && !_dimmed)
        {
            if (SetBrightness(EffDimPct)) _dimmed = true;
        }
        else if (!wantDim && _dimmed)
        {
            if (SetBrightness(EffNormalPct)) _dimmed = false;
        }
    }

    static bool Excluded(int vk)
    {
        switch (vk)
        {
            case 0x08: case 0x09: case 0x0D: case 0x1B: case 0x2E:
            case 0x25: case 0x26: case 0x27: case 0x28:
            case 0x21: case 0x22: case 0x23: case 0x24:
            case 0x10: case 0x11: case 0x12:
            case 0xA0: case 0xA1: case 0xA2: case 0xA3: case 0xA4: case 0xA5:
            case 0x5B: case 0x5C: case 0x5D: case 0x2C:
                return true;
            default:
                return false;
        }
    }

    static IntPtr KbCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        try
        {
            if (nCode >= 0 && _s != null)
            {
                KBDLLHOOKSTRUCT k = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
                int vk = k.vkCode & 0xFF;
                bool injected = (k.flags & LLKHF_INJECTED) != 0;
                uint now = (uint)Environment.TickCount;
                bool isDown = (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN);
                bool isUp = (wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYUP);

                if (_kbdLocked && !injected) return (IntPtr)1;
                if (!injected)
                {
                    if (isDown) { _lastKeyTick = now; _haveKeyTick = true; }
                    if (_s.KeyEnabled && !Excluded(vk))
                    {
                        if (isUp) _debounce.NoteKeyUp(vk, now);
                        else if (isDown && _debounce.ShouldDropKeyDown(vk, now, _s.DebounceUpMs, _s.DebounceDownMs))
                            return (IntPtr)1;
                    }
                }
            }
        }
        catch (Exception ex) { Program.LogError("keyboard hook", ex); }
        return CallNextHookEx(_kbHook, nCode, wParam, lParam);
    }

    static IntPtr MsCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        try
        {
            if (nCode >= 0 && _s != null && _s.TouchEnabled && _s.BlockMs > 0 && _haveKeyTick)
            {
                int msg = wParam.ToInt32();
                bool blockable = msg == WM_MOUSEMOVE || msg == WM_LBUTTONDOWN || msg == WM_RBUTTONDOWN ||
                                 msg == WM_MBUTTONDOWN || msg == WM_XBUTTONDOWN;
                if (blockable)
                {
                    MSLLHOOKSTRUCT m = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                    bool injected = (m.flags & LLMHF_INJECTED) != 0;
                    if (!injected)
                    {
                        uint dt = unchecked((uint)Environment.TickCount - _lastKeyTick);
                        if (dt < (uint)_s.BlockMs) return (IntPtr)1;
                    }
                }
            }
        }
        catch (Exception ex) { Program.LogError("mouse hook", ex); }
        return CallNextHookEx(_msHook, nCode, wParam, lParam);
    }

    void InstallHooks()
    {
        try
        {
            _kbProc = new LowLevelProc(KbCallback);
            _msProc = new LowLevelProc(MsCallback);
            IntPtr hMod = GetModuleHandle(null);
            _kbHook = SetWindowsHookEx(WH_KEYBOARD_LL, _kbProc, hMod, 0);
            _msHook = SetWindowsHookEx(WH_MOUSE_LL, _msProc, hMod, 0);
        }
        catch (Exception ex) { Program.LogError("install hooks", ex); }
        if (_kbHook == IntPtr.Zero)
        {
            Program.LogNote("install hooks", "keyboard hook failed, win32 error " + Marshal.GetLastWin32Error());
            _s.KeyEnabled = false;
            chkKey.Checked = false;
            chkKey.Enabled = false;
        }
        if (_msHook == IntPtr.Zero)
        {
            Program.LogNote("install hooks", "mouse hook failed, win32 error " + Marshal.GetLastWin32Error());
            _s.TouchEnabled = false;
            chkTouch.Checked = false;
            chkTouch.Enabled = false;
        }
    }

    void RemoveHooks()
    {
        try
        {
            if (_kbHook != IntPtr.Zero) { UnhookWindowsHookEx(_kbHook); _kbHook = IntPtr.Zero; }
            if (_msHook != IntPtr.Zero) { UnhookWindowsHookEx(_msHook); _msHook = IntPtr.Zero; }
        }
        catch { }
    }

    public static void RestoreBrightnessIfDimmed()
    {
        try
        {
            if (_dimmed && _s != null) { SetBrightness(_s.NormalPct); _dimmed = false; }
        }
        catch { }
    }
}

public class Program
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    static extern IntPtr FindWindow(string cls, string title);
    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr h, int cmd);
    [DllImport("user32.dll")]
    static extern bool SetForegroundWindow(IntPtr h);
    [DllImport("user32.dll")]
    static extern IntPtr SendMessageTimeout(IntPtr h, uint msg, IntPtr wParam, IntPtr lParam, uint flags, uint timeout, out IntPtr result);
    [DllImport("user32.dll")]
    static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += delegate(object s, ThreadExceptionEventArgs e)
        {
            MainForm.RestoreBrightnessIfDimmed();
            LogError("UI thread", e.Exception);
        };
        AppDomain.CurrentDomain.UnhandledException += delegate(object s, UnhandledExceptionEventArgs e)
        {
            MainForm.RestoreBrightnessIfDimmed();
            LogError("AppDomain", e.ExceptionObject as Exception);
        };
        Ui.InitDpi();

        bool createdNew;
        using (Mutex mutex = new Mutex(true, "MacBookToolkit_SingleInstance", out createdNew))
        {
            if (!createdNew)
            {
                IntPtr existing = WaitForWindow(4000);
                if (existing != IntPtr.Zero)
                {
                    if (IsResponsive(existing))
                    {
                        ShowWindow(existing, 9);
                        SetForegroundWindow(existing);
                        return;
                    }
                    DialogResult r = MessageBox.Show(Texts.T("hung_body"), Texts.T("hung_title"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (r != DialogResult.Yes) return;
                    KillOwner(existing);
                    Thread.Sleep(700);
                }
            }
            Application.Run(new MainForm());
        }
    }

    static IntPtr WaitForWindow(int totalMs)
    {
        Stopwatch sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < totalMs)
        {
            IntPtr h = FindWindow(null, "MacBookToolkit");
            if (h != IntPtr.Zero) return h;
            Thread.Sleep(150);
        }
        return IntPtr.Zero;
    }

    static bool IsResponsive(IntPtr hwnd)
    {
        try
        {
            IntPtr res;
            return SendMessageTimeout(hwnd, 0, IntPtr.Zero, IntPtr.Zero, 0x0002, 1200, out res) != IntPtr.Zero;
        }
        catch { return false; }
    }

    static void KillOwner(IntPtr hwnd)
    {
        try
        {
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);
            using (Process p = Process.GetProcessById((int)pid))
            {
                p.Kill();
                p.WaitForExit(3000);
            }
            LogNote("single instance", "terminated hung instance pid=" + pid);
        }
        catch (Exception ex) { LogError("terminate hung", ex); }
    }

    public static void LogNote(string source, string note)
    {
        WriteLog("[" + source + "] " + note);
    }

    public static void LogError(string source, Exception ex)
    {
        WriteLog("[" + source + "] " + (ex == null ? "error" : ex.ToString()));
    }

    static void WriteLog(string text)
    {
        try
        {
            string dir = Settings.Dir();
            Directory.CreateDirectory(dir);
            File.AppendAllText(Path.Combine(dir, "errors.log"), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + text + Environment.NewLine);
        }
        catch { }
    }
}
