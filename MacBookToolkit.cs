using System;
using System.Drawing;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Management;
using System.IO;
using System.Diagnostics;
using System.Text;
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
    public string Lang = "ru";

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
        try
        {
            string fp = FilePath();
            if (File.Exists(fp))
            {
                string[] lines = File.ReadAllLines(fp);
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
                        case "Lang": s.Lang = v; break;
                    }
                }
            }
        }
        catch { }
        return s;
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
            sb.AppendLine("Lang=" + Lang.ToString());
            File.WriteAllText(FilePath(), sb.ToString());
        }
        catch { }
    }
}

public static class L10n
{
    public static string Lang = "ru";

    static Dictionary<string, string> ru = new Dictionary<string, string>()
    {
        {"tab_screen","Экран"}, {"tab_touch","Тачпад"}, {"tab_key","Клавиатура"}, {"tab_power","Питание"},
        {"chk_screen","Затемнять экран при простое"},
        {"lbl_idle","Простой до затемнения (сек):"},
        {"lbl_dim","Яркость затемнения (%):"},
        {"lbl_normal","Яркость при активности (%):"},
        {"chk_touch","Не реагировать на тачпад во время печати"},
        {"lbl_block","Блокировка после нажатия клавиши (мс):"},
        {"chk_key","Подавлять дребезг клавиш (дубли букв)"},
        {"lbl_up","Окно после отпускания (мс):"},
        {"lbl_down","Окно между нажатиями (мс):"},
        {"lbl_screenoff","Отключать экран через (мин):"},
        {"chk_autostart","Запускать при входе в Windows"},
        {"chk_tray","Показывать иконку в трее"},
        {"btn_save","Сохранить и применить"},
        {"btn_reset","Сброс к умолчанию"},
        {"lbl_lang","Язык:"},
        {"reset_title","Сброс к умолчанию"},
        {"reset_confirm","Вернуть ВСЕ настройки к подобранным значениям?\n\nЭкран: 15 с / 10% / 100%\nТачпад: 700 мс\nКлавиатура: 70 / 25 мс\nПитание: экран через 2 мин"},
        {"reset_done","Готово — настройки сброшены к умолчанию."},
        {"tray_show","Показать"}, {"tray_exit","Выход"},
        {"tray_min","Свернуто в трей. Настройки работают."},
        {"tip_chk_screen","Когда вы не трогаете мышь/клавиатуру заданное время, яркость экрана снижается. При активности возвращается обычная яркость."},
        {"tip_idle","Через сколько секунд бездействия включать затемнение. Минимум 1 сек."},
        {"tip_dim","До какой яркости (в %) затемнять экран. 10% — очень тускло, но читаемо."},
        {"tip_normal","Яркость при активности. 100% — максимум."},
        {"tip_chk_touch","Пока вы печатаете, тачпад не двигает курсор. Защищает от случайных касаний ладонью."},
        {"tip_block","Сколько миллисекунд после последнего нажатия клавиши тачпад игнорирует касания."},
        {"tip_chk_key","Убирает «дребезг» butterfly-клавиатуры — когда одна клавиша срабатывает дважды."},
        {"tip_up","Повторное нажатие той же клавиши быстрее этого времени считается дребезгом и отбрасывается."},
        {"tip_down","Окно между двумя нажатиями одной клавиши без отпускания. Слишком большое может мешать быстрому набору."},
        {"tip_screenoff","Через сколько минут простоя Windows выключает экран."},
        {"tip_autostart","Запускать программу автоматически при входе в Windows."},
        {"tip_tray","Показывать иконку программы в системном трее (рядом с часами)."},
        {"tip_save","Сохранить настройки и применить их сразу."},
        {"tip_reset","Вернуть ВСЕ настройки к подобранным значениям по умолчанию."}
    };

    static Dictionary<string, string> uk = new Dictionary<string, string>()
    {
        {"tab_screen","Екран"}, {"tab_touch","Тачпад"}, {"tab_key","Клавіатура"}, {"tab_power","Живлення"},
        {"chk_screen","Затемнювати екран при простої"},
        {"lbl_idle","Простій до затемнення (сек):"},
        {"lbl_dim","Яскравість затемнення (%):"},
        {"lbl_normal","Яскравість при активності (%):"},
        {"chk_touch","Не реагувати на тачпад під час друку"},
        {"lbl_block","Блокування після натискання клавіші (мс):"},
        {"chk_key","Придушувати брязкіт клавіш (дублі літер)"},
        {"lbl_up","Вікно після відпускання (мс):"},
        {"lbl_down","Вікно між натисканнями (мс):"},
        {"lbl_screenoff","Вимикати екран через (хв):"},
        {"chk_autostart","Запускати при вході у Windows"},
        {"chk_tray","Показувати іконку в треї"},
        {"btn_save","Зберегти та застосувати"},
        {"btn_reset","Скидання до стандартних"},
        {"lbl_lang","Мова:"},
        {"reset_title","Скидання"},
        {"reset_confirm","Повернути ВСІ налаштування до підібраних значень?\n\nЕкран: 15 с / 10% / 100%\nТачпад: 700 мс\nКлавіатура: 70 / 25 мс\nЖивлення: екран через 2 хв"},
        {"reset_done","Готово — налаштування скинуті до стандартних."},
        {"tray_show","Показати"}, {"tray_exit","Вихід"},
        {"tray_min","Згорнуто в трей. Налаштування працюють."},
        {"tip_chk_screen","Коли ви не торкаєтесь миші/клавіатури заданий час, яскравість екрана знижується. При активності повертається звичайна."},
        {"tip_idle","Через скільки секунд простою вмикати затемнення. Мінімум 1 сек."},
        {"tip_dim","До якої яскравості (у %) затемнювати екран. 10% — дуже тьмяно, але читабельно."},
        {"tip_normal","Яскравість при активності. 100% — максимум."},
        {"tip_chk_touch","Поки ви друкуєте, тачпад не рухає курсор. Захищає від випадкових торкань долонею."},
        {"tip_block","Скільки мілісекунд після останнього натискання клавіші тачпад ігнорує торкання."},
        {"tip_chk_key","Прибирає «брязкіт» butterfly-клавіатури — коли одна клавіша спрацьовує двічі."},
        {"tip_up","Повторне натискання тієї ж клавіші швидше цього часу вважається брязкотом і відкидається."},
        {"tip_down","Вікно між двома натисканнями однієї клавіші без відпускання. Занадто велике може заважати швидкому набору."},
        {"tip_screenoff","Через скільки хвилин простою Windows вимикає екран."},
        {"tip_autostart","Запускати програму автоматично при вході у Windows."},
        {"tip_tray","Показувати іконку програми в системному треї (біля годинника)."},
        {"tip_save","Зберегти налаштування та застосувати їх одразу."},
        {"tip_reset","Повернути ВСІ налаштування до підібраних значень за замовчуванням."}
    };

    static Dictionary<string, string> en = new Dictionary<string, string>()
    {
        {"tab_screen","Screen"}, {"tab_touch","Touchpad"}, {"tab_key","Keyboard"}, {"tab_power","Power"},
        {"chk_screen","Dim screen when idle"},
        {"lbl_idle","Idle before dimming (sec):"},
        {"lbl_dim","Dim brightness (%):"},
        {"lbl_normal","Active brightness (%):"},
        {"chk_touch","Ignore touchpad while typing"},
        {"lbl_block","Block after keypress (ms):"},
        {"chk_key","Suppress key chatter (double letters)"},
        {"lbl_up","Window after release (ms):"},
        {"lbl_down","Window between presses (ms):"},
        {"lbl_screenoff","Turn off screen after (min):"},
        {"chk_autostart","Launch at Windows startup"},
        {"chk_tray","Show tray icon"},
        {"btn_save","Save & apply"},
        {"btn_reset","Reset to defaults"},
        {"lbl_lang","Language:"},
        {"reset_title","Reset to defaults"},
        {"reset_confirm","Restore ALL settings to the tuned values?\n\nScreen: 15 s / 10% / 100%\nTouchpad: 700 ms\nKeyboard: 70 / 25 ms\nPower: screen off in 2 min"},
        {"reset_done","Done — settings restored to defaults."},
        {"tray_show","Show"}, {"tray_exit","Exit"},
        {"tray_min","Minimized to tray. Settings are running."},
        {"tip_chk_screen","When you do not touch the mouse/keyboard for the set time, screen brightness is reduced. It restores on activity."},
        {"tip_idle","Seconds of inactivity before dimming starts. Minimum 1 s."},
        {"tip_dim","Brightness (%) to dim to. 10% is very dim but readable."},
        {"tip_normal","Brightness (%) when active. 100% is maximum."},
        {"tip_chk_touch","While typing, the touchpad will not move the cursor. Protects from accidental palm touches."},
        {"tip_block","Milliseconds after the last keypress during which the touchpad ignores touches."},
        {"tip_chk_key","Removes butterfly-keyboard chatter (a key firing twice)."},
        {"tip_up","A repeat press of the same key within this time is treated as chatter and dropped."},
        {"tip_down","Window between two presses of one key without release. Too large may hurt fast typing."},
        {"tip_screenoff","Minutes of inactivity before Windows turns the screen off."},
        {"tip_autostart","Start the app automatically at Windows login."},
        {"tip_tray","Show the app icon in the system tray (near the clock)."},
        {"tip_save","Save settings and apply them immediately."},
        {"tip_reset","Restore ALL settings to the tuned defaults."}
    };

    public static string T(string key)
    {
        Dictionary<string, string> d = ru;
        if (Lang == "en") d = en;
        else if (Lang == "uk") d = uk;
        string v;
        if (d.TryGetValue(key, out v)) return v;
        return key;
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

    [StructLayout(LayoutKind.Sequential)]
    struct LASTINPUTINFO { public uint cbSize; public uint dwTime; }
    [StructLayout(LayoutKind.Sequential)]
    struct KBDLLHOOKSTRUCT { public int vkCode; public int scanCode; public int flags; public int time; public IntPtr dwExtraInfo; }

    const int WH_KEYBOARD_LL = 13;
    const int WH_MOUSE_LL = 14;
    const int WM_KEYDOWN = 0x0100;
    const int WM_SYSKEYDOWN = 0x0104;
    const int WM_KEYUP = 0x0101;
    const int WM_SYSKEYUP = 0x0105;
    const int LLKHF_INJECTED = 0x10;

    static LowLevelProc _kbProc;
    static LowLevelProc _msProc;
    static IntPtr _kbHook = IntPtr.Zero;
    static IntPtr _msHook = IntPtr.Zero;
    static uint _lastKeyTick = 0;
    static uint[] _lastKeyup = new uint[256];
    static uint[] _lastKeydown = new uint[256];
    static Settings _s;
    static bool _dimmed = false;

    TabControl tabs;
    CheckBox chkScreen;
    NumericUpDown numIdle;
    NumericUpDown numDim;
    NumericUpDown numNormal;
    CheckBox chkTouch;
    NumericUpDown numBlock;
    CheckBox chkKey;
    NumericUpDown numUp;
    NumericUpDown numDown;
    NumericUpDown numScreenOff;
    CheckBox chkAutoStart;
    CheckBox chkTray;
    ComboBox cbLang;
    Label lblLang;
    Label lblIdle;
    Label lblDim;
    Label lblNormal;
    Label lblBlock;
    Label lblUp;
    Label lblDown;
    Label lblScreenOff;
    Button btnSave;
    Button btnReset;
    NotifyIcon tray;
    ToolTip tip;
    MenuItem miShow;
    MenuItem miExit;
    Timer idleTimer;
    bool _loading = false;

    public MainForm()
    {
        _s = Settings.Load();
        L10n.Lang = _s.Lang;
        BuildUi();
        ApplyLanguage();
        InstallHooks();
        ApplyPower();
        SetAutoStart(_s.AutoStart);
        idleTimer = new Timer();
        idleTimer.Interval = 500;
        idleTimer.Tick += IdleTick;
        idleTimer.Start();
        SetBrightness(_s.ScreenEnabled ? _s.NormalPct : -1);
    }

    Label AddLabel(string key, int y, Control parent)
    {
        Label l = new Label();
        l.Text = L10n.T(key);
        l.Location = new Point(40, y);
        l.AutoSize = true;
        parent.Controls.Add(l);
        return l;
    }

    NumericUpDown AddNum(int y, int min, int max, int step, int val, Control parent)
    {
        NumericUpDown n = new NumericUpDown();
        n.Minimum = min;
        n.Maximum = max;
        n.Increment = step;
        n.Value = Math.Max(min, Math.Min(max, val));
        n.Width = 80;
        n.Location = new Point(360, y - 3);
        parent.Controls.Add(n);
        return n;
    }

    CheckBox AddCheck(string key, int x, int y, bool val)
    {
        CheckBox c = new CheckBox();
        c.Text = L10n.T(key);
        c.Location = new Point(x, y);
        c.AutoSize = true;
        c.Checked = val;
        return c;
    }

    void BuildUi()
    {
        Text = "MacBookToolkit";
        ClientSize = new Size(600, 580);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9f);
        try { Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

        tip = new ToolTip();
        tip.AutoPopDelay = 15000;
        tip.InitialDelay = 400;
        tip.ReshowDelay = 100;

        tabs = new TabControl();
        tabs.SetBounds(12, 12, 576, 460);

        TabPage tpScreen = new TabPage("Экран");
        chkScreen = AddCheck("chk_screen", 15, 15, _s.ScreenEnabled);
        tpScreen.Controls.Add(chkScreen);
        lblIdle = AddLabel("lbl_idle", 60, tpScreen);
        numIdle = AddNum(60, 1, 300, 1, _s.IdleSec, tpScreen);
        lblDim = AddLabel("lbl_dim", 105, tpScreen);
        numDim = AddNum(105, 1, 100, 1, _s.DimPct, tpScreen);
        lblNormal = AddLabel("lbl_normal", 150, tpScreen);
        numNormal = AddNum(150, 1, 100, 1, _s.NormalPct, tpScreen);

        TabPage tpTouch = new TabPage("Тачпад");
        chkTouch = AddCheck("chk_touch", 15, 15, _s.TouchEnabled);
        tpTouch.Controls.Add(chkTouch);
        lblBlock = AddLabel("lbl_block", 60, tpTouch);
        numBlock = AddNum(60, 50, 2000, 50, _s.BlockMs, tpTouch);

        TabPage tpKey = new TabPage("Клавиатура");
        chkKey = AddCheck("chk_key", 15, 15, _s.KeyEnabled);
        tpKey.Controls.Add(chkKey);
        lblUp = AddLabel("lbl_up", 60, tpKey);
        numUp = AddNum(60, 0, 200, 5, _s.DebounceUpMs, tpKey);
        lblDown = AddLabel("lbl_down", 105, tpKey);
        numDown = AddNum(105, 0, 100, 5, _s.DebounceDownMs, tpKey);

        TabPage tpPower = new TabPage("Питание");
        lblScreenOff = AddLabel("lbl_screenoff", 15, tpPower);
        numScreenOff = AddNum(15, 1, 60, 1, _s.ScreenOffMin, tpPower);

        tabs.TabPages.Add(tpScreen);
        tabs.TabPages.Add(tpTouch);
        tabs.TabPages.Add(tpKey);
        tabs.TabPages.Add(tpPower);
        Controls.Add(tabs);

        chkAutoStart = AddCheck("chk_autostart", 18, 480, _s.AutoStart);
        Controls.Add(chkAutoStart);
        chkTray = AddCheck("chk_tray", 18, 505, _s.ShowTray);
        Controls.Add(chkTray);

        lblLang = new Label();
        lblLang.Text = L10n.T("lbl_lang");
        lblLang.Location = new Point(18, 535);
        lblLang.AutoSize = true;
        Controls.Add(lblLang);

        cbLang = new ComboBox();
        cbLang.DropDownStyle = ComboBoxStyle.DropDownList;
        cbLang.Items.Add("Русский");
        cbLang.Items.Add("Українська");
        cbLang.Items.Add("English");
        cbLang.Location = new Point(90, 532);
        cbLang.Width = 160;
        cbLang.SelectedIndex = _s.Lang == "en" ? 2 : (_s.Lang == "uk" ? 1 : 0);
        cbLang.SelectedIndexChanged += LangChanged;
        Controls.Add(cbLang);

        btnSave = new Button();
        btnSave.Text = L10n.T("btn_save");
        btnSave.Location = new Point(300, 478);
        btnSave.Size = new Size(270, 36);
        btnSave.Click += SaveClick;
        Controls.Add(btnSave);

        btnReset = new Button();
        btnReset.Text = L10n.T("btn_reset");
        btnReset.Location = new Point(300, 520);
        btnReset.Size = new Size(270, 36);
        btnReset.Click += ResetClick;
        Controls.Add(btnReset);

        tray = new NotifyIcon();
        tray.Icon = this.Icon != null ? this.Icon : SystemIcons.Application;
        tray.Text = "MacBookToolkit";
        tray.Visible = _s.ShowTray;
        ContextMenu cm = new ContextMenu();
        miShow = new MenuItem("Показать");
        miShow.Click += ShowClick;
        miExit = new MenuItem("Выход");
        miExit.Click += ExitClick;
        cm.MenuItems.Add(miShow);
        cm.MenuItems.Add(miExit);
        tray.ContextMenu = cm;
        tray.DoubleClick += ShowClick;
        Resize += FormResize;

        tip.SetToolTip(chkScreen, L10n.T("tip_chk_screen"));
        tip.SetToolTip(numIdle, L10n.T("tip_idle"));
        tip.SetToolTip(numDim, L10n.T("tip_dim"));
        tip.SetToolTip(numNormal, L10n.T("tip_normal"));
        tip.SetToolTip(chkTouch, L10n.T("tip_chk_touch"));
        tip.SetToolTip(numBlock, L10n.T("tip_block"));
        tip.SetToolTip(chkKey, L10n.T("tip_chk_key"));
        tip.SetToolTip(numUp, L10n.T("tip_up"));
        tip.SetToolTip(numDown, L10n.T("tip_down"));
        tip.SetToolTip(numScreenOff, L10n.T("tip_screenoff"));
        tip.SetToolTip(chkAutoStart, L10n.T("tip_autostart"));
        tip.SetToolTip(chkTray, L10n.T("tip_tray"));
        tip.SetToolTip(btnSave, L10n.T("tip_save"));
        tip.SetToolTip(btnReset, L10n.T("tip_reset"));
    }

    void ApplyLanguage()
    {
        L10n.Lang = _s.Lang;
        tabs.TabPages[0].Text = L10n.T("tab_screen");
        tabs.TabPages[1].Text = L10n.T("tab_touch");
        tabs.TabPages[2].Text = L10n.T("tab_key");
        tabs.TabPages[3].Text = L10n.T("tab_power");
        chkScreen.Text = L10n.T("chk_screen");
        lblIdle.Text = L10n.T("lbl_idle");
        lblDim.Text = L10n.T("lbl_dim");
        lblNormal.Text = L10n.T("lbl_normal");
        chkTouch.Text = L10n.T("chk_touch");
        lblBlock.Text = L10n.T("lbl_block");
        chkKey.Text = L10n.T("chk_key");
        lblUp.Text = L10n.T("lbl_up");
        lblDown.Text = L10n.T("lbl_down");
        lblScreenOff.Text = L10n.T("lbl_screenoff");
        chkAutoStart.Text = L10n.T("chk_autostart");
        chkTray.Text = L10n.T("chk_tray");
        lblLang.Text = L10n.T("lbl_lang");
        btnSave.Text = L10n.T("btn_save");
        btnReset.Text = L10n.T("btn_reset");
        miShow.Text = L10n.T("tray_show");
        miExit.Text = L10n.T("tray_exit");
        tip.SetToolTip(chkScreen, L10n.T("tip_chk_screen"));
        tip.SetToolTip(numIdle, L10n.T("tip_idle"));
        tip.SetToolTip(numDim, L10n.T("tip_dim"));
        tip.SetToolTip(numNormal, L10n.T("tip_normal"));
        tip.SetToolTip(chkTouch, L10n.T("tip_chk_touch"));
        tip.SetToolTip(numBlock, L10n.T("tip_block"));
        tip.SetToolTip(chkKey, L10n.T("tip_chk_key"));
        tip.SetToolTip(numUp, L10n.T("tip_up"));
        tip.SetToolTip(numDown, L10n.T("tip_down"));
        tip.SetToolTip(numScreenOff, L10n.T("tip_screenoff"));
        tip.SetToolTip(chkAutoStart, L10n.T("tip_autostart"));
        tip.SetToolTip(chkTray, L10n.T("tip_tray"));
        tip.SetToolTip(btnSave, L10n.T("tip_save"));
        tip.SetToolTip(btnReset, L10n.T("tip_reset"));
        _loading = false;
    }

    void LangChanged(object sender, EventArgs e)
    {
        if (_loading) return;
        _s.Lang = cbLang.SelectedIndex == 2 ? "en" : (cbLang.SelectedIndex == 1 ? "uk" : "ru");
        _s.Save();
        ApplyLanguage();
    }

    void SaveClick(object sender, EventArgs e)
    {
        _s.ScreenEnabled = chkScreen.Checked;
        _s.IdleSec = (int)numIdle.Value;
        _s.DimPct = (int)numDim.Value;
        _s.NormalPct = (int)numNormal.Value;
        _s.TouchEnabled = chkTouch.Checked;
        _s.BlockMs = (int)numBlock.Value;
        _s.KeyEnabled = chkKey.Checked;
        _s.DebounceUpMs = (int)numUp.Value;
        _s.DebounceDownMs = (int)numDown.Value;
        _s.ScreenOffMin = (int)numScreenOff.Value;
        _s.AutoStart = chkAutoStart.Checked;
        _s.ShowTray = chkTray.Checked;
        _s.Save();
        ApplyPower();
        SetAutoStart(_s.AutoStart);
        tray.Visible = _s.ShowTray;
        if (_s.ScreenEnabled) SetBrightness(_s.NormalPct);
    }

    void ResetClick(object sender, EventArgs e)
    {
        if (MessageBox.Show(L10n.T("reset_confirm"), L10n.T("reset_title"),
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;
        Settings d = new Settings();
        d.Lang = _s.Lang;
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
        _loading = false;
        _s.Save();
        ApplyPower();
        SetAutoStart(_s.AutoStart);
        tray.Visible = _s.ShowTray;
        SetBrightness(d.ScreenEnabled ? d.NormalPct : -1);
        _dimmed = false;
        MessageBox.Show(L10n.T("reset_done"), "MacBookToolkit");
    }

    void ShowClick(object sender, EventArgs e)
    {
        Show();
        WindowState = FormWindowState.Normal;
        Activate();
    }

    void ExitClick(object sender, EventArgs e)
    {
        tray.Visible = false;
        idleTimer.Stop();
        RemoveHooks();
        Application.Exit();
    }

    void FormResize(object sender, EventArgs e)
    {
        if (WindowState == FormWindowState.Minimized && _s.ShowTray)
        {
            Hide();
            tray.ShowBalloonTip(1500, "MacBookToolkit", L10n.T("tray_min"), ToolTipIcon.Info);
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing && _s.ShowTray)
        {
            e.Cancel = true;
            Hide();
            tray.ShowBalloonTip(1500, "MacBookToolkit", L10n.T("tray_min"), ToolTipIcon.Info);
            return;
        }
        base.OnFormClosing(e);
    }

    void ApplyPower()
    {
        try
        {
            int sec = _s.ScreenOffMin * 60;
            Run("powercfg", "/setacvalueindex SCHEME_CURRENT SUB_VIDEO VIDEOIDLE " + sec.ToString());
            Run("powercfg", "/setdcvalueindex SCHEME_CURRENT SUB_VIDEO VIDEOIDLE " + sec.ToString());
            Run("powercfg", "/setactive SCHEME_CURRENT");
        }
        catch { }
    }

    static void Run(string exe, string args)
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo(exe, args);
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            Process p = Process.Start(psi);
            if (p != null) p.WaitForExit(3000);
        }
        catch { }
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
        catch { }
    }

    static void SetBrightness(int pct)
    {
        if (pct < 0) return;
        try
        {
            ManagementObjectSearcher s = new ManagementObjectSearcher("root\\wmi", "SELECT * FROM WmiMonitorBrightnessMethods");
            foreach (ManagementObject m in s.Get())
            {
                m.InvokeMethod("WmiSetBrightness", new object[] { (uint)0, (byte)pct });
                break;
            }
        }
        catch { }
    }

    static uint IdleMs()
    {
        LASTINPUTINFO lii = new LASTINPUTINFO();
        lii.cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO));
        if (!GetLastInputInfo(ref lii)) return 0;
        return (uint)(Environment.TickCount - (int)lii.dwTime);
    }

    void IdleTick(object sender, EventArgs e)
    {
        if (!_s.ScreenEnabled) return;
        uint idle = IdleMs();
        if (idle >= (uint)(_s.IdleSec * 1000) && !_dimmed)
        {
            SetBrightness(_s.DimPct);
            _dimmed = true;
        }
        else if (idle < 2000 && _dimmed)
        {
            SetBrightness(_s.NormalPct);
            _dimmed = false;
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
                return true;
            default:
                return false;
        }
    }

    static IntPtr KbCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            KBDLLHOOKSTRUCT k = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
            int vk = k.vkCode & 0xFF;
            bool injected = (k.flags & LLKHF_INJECTED) != 0;
            uint now = (uint)Environment.TickCount;
            bool isDown = (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN);
            bool isUp = (wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYUP);

            if (!injected)
            {
                if (isDown) _lastKeyTick = now;
                if (_s.KeyEnabled && !Excluded(vk))
                {
                    if (isUp) _lastKeyup[vk] = now;
                    else if (isDown)
                    {
                        uint dtUp = _lastKeyup[vk] == 0 ? uint.MaxValue : (uint)(now - _lastKeyup[vk]);
                        uint dtDown = _lastKeydown[vk] == 0 ? uint.MaxValue : (uint)(now - _lastKeydown[vk]);
                        if (dtUp < (uint)_s.DebounceUpMs || dtDown < (uint)_s.DebounceDownMs)
                            return (IntPtr)1;
                        _lastKeydown[vk] = now;
                    }
                }
            }
        }
        return CallNextHookEx(_kbHook, nCode, wParam, lParam);
    }

    static IntPtr MsCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && _s.TouchEnabled && _s.BlockMs > 0 && _lastKeyTick != 0)
        {
            uint dt = (uint)Environment.TickCount - _lastKeyTick;
            if (dt < (uint)_s.BlockMs) return (IntPtr)1;
        }
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
        catch { }
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
}

public class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}
