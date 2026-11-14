using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Win32;
using Windows.Devices.Sensors;

public static class BatteryInfo
{
    public static bool TryRead(out int design, out int full, out int cycles, out int charge)
    {
        design = 0; full = 0; cycles = 0; charge = 0;
        string tmp = Path.Combine(Path.GetTempPath(), "MacBookToolkit_battery.xml");
        try
        {
            if (File.Exists(tmp)) File.Delete(tmp);
            System.Diagnostics.Process p = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "powercfg.exe",
                Arguments = "/batteryreport /XML /OUTPUT \"" + tmp + "\"",
                UseShellExecute = false,
                CreateNoWindow = true
            });
            if (p != null) p.WaitForExit(15000);
            if (!File.Exists(tmp)) return false;
            if (ParseReport(tmp, out design, out full, out cycles, out charge))
            {
                if (charge <= 0) charge = WmiCharge();
                if (charge <= 0)
                {
                    try
                    {
                        float pct = SystemInformation.PowerStatus.BatteryLifePercent;
                        if (pct >= 0f && pct <= 1f) charge = (int)Math.Round(pct * 100f);
                    }
                    catch { }
                }
                return true;
            }
        }
        catch { }
        finally
        {
            try { if (File.Exists(tmp)) File.Delete(tmp); } catch { }
        }
        return false;
    }

    public static bool ParseReport(string xmlPath, out int design, out int full, out int cycles, out int charge)
    {
        design = 0; full = 0; cycles = 0; charge = 0;
        try
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlPath);
            XmlNode battery = doc.SelectSingleNode("//*[local-name()='Battery']");
            if (battery == null) return false;
            design = ReadInt(battery, "DesignCapacity");
            full = ReadInt(battery, "FullChargeCapacity");
            cycles = ReadInt(battery, "CycleCount");
            charge = ReadInt(battery, "ChargeCapacity");
            return design > 0 && full > 0;
        }
        catch { return false; }
    }

    static int ReadInt(XmlNode parent, string name)
    {
        XmlNode n = parent.SelectSingleNode(".//*[local-name()='" + name + "']");
        int v;
        if (n != null && int.TryParse(n.InnerText.Trim(), out v)) return v;
        return 0;
    }

    public static int WmiCharge()
    {
        try
        {
            using (ManagementObjectSearcher s = new ManagementObjectSearcher("SELECT EstimatedChargeRemaining FROM Win32_Battery"))
            using (ManagementObjectCollection col = s.Get())
            {
                foreach (ManagementObject m in col)
                {
                    using (m)
                    {
                        object v = m["EstimatedChargeRemaining"];
                        if (v != null) return Convert.ToInt32(v);
                    }
                }
            }
        }
        catch { }
        return 0;
    }

    public static int WearPercent(int design, int full)
    {
        if (design <= 0 || full <= 0) return 0;
        int wear = (int)Math.Round((1.0 - (double)full / design) * 100.0);
        if (wear < 0) wear = 0;
        return wear;
    }
}

public static class BatteryPower
{
    public static bool TryRead(out bool charging, out bool discharging, out int chargeRateMw, out int dischargeRateMw, out int remainingCapacity, out int voltageMv)
    {
        charging = false; discharging = false; chargeRateMw = 0; dischargeRateMw = 0; remainingCapacity = 0; voltageMv = 0;
        try
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\wmi", "SELECT * FROM BatteryStatus"))
            {
                foreach (ManagementObject item in searcher.Get())
                {
                    charging = Convert.ToBoolean(item["Charging"]);
                    discharging = Convert.ToBoolean(item["Discharging"]);
                    chargeRateMw = Convert.ToInt32(item["ChargeRate"]);
                    dischargeRateMw = Convert.ToInt32(item["DischargeRate"]);
                    remainingCapacity = Convert.ToInt32(item["RemainingCapacity"]);
                    voltageMv = Convert.ToInt32(item["Voltage"]);
                    return true;
                }
            }
        }
        catch { }
        return false;
    }
}

public static class BootCampCheck
{
    public static bool DriversPresent()
    {
        string root = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        string drv = Path.Combine(root, "System32", "drivers");
        bool smc = File.Exists(Path.Combine(drv, "AppleSMC.sys"));
        bool keyboard = File.Exists(Path.Combine(drv, "AppleKeyboardInternalUSB.sys"));
        bool trackpad = File.Exists(Path.Combine(drv, "ApplePrecisionTrackpadUSB.sys"));
        string bootcamp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Boot Camp");
        bool bootcampDir = Directory.Exists(bootcamp);
        return smc || bootcampDir || (keyboard && trackpad);
    }
}

public class UpdateInfo
{
    public string Version = "";
    public string PageUrl = "";
    public string AssetUrl = "";
}

public static class UpdateChecker
{
    public static UpdateInfo Parse(string json)
    {
        UpdateInfo info = new UpdateInfo();
        if (string.IsNullOrEmpty(json)) return info;
        Match tag = Regex.Match(json, "\"tag_name\"\\s*:\\s*\"([^\"]+)\"");
        Match page = Regex.Match(json, "\"html_url\"\\s*:\\s*\"([^\"]+)\"");
        if (tag.Success) info.Version = tag.Groups[1].Value.TrimStart('v', 'V');
        if (page.Success) info.PageUrl = page.Groups[1].Value;
        Match asset = Regex.Match(json, "\"browser_download_url\"\\s*:\\s*\"([^\"]+\\.exe)\"");
        if (asset.Success) info.AssetUrl = asset.Groups[1].Value;
        return info;
    }

    public static bool IsNewer(string current, string candidate)
    {
        Version a, b;
        if (!Version.TryParse(current, out a)) return false;
        if (!Version.TryParse(candidate, out b)) return false;
        return b > a;
    }

    public static void CheckAsync(Action<UpdateInfo> done)
    {
        System.Threading.ThreadPool.QueueUserWorkItem(delegate
        {
            UpdateInfo info = new UpdateInfo();
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://api.github.com/repos/the-big-paul/MacBookToolkit/releases/latest");
                req.UserAgent = "MacBookToolkit";
                req.Timeout = 8000;
                using (WebResponse resp = req.GetResponse())
                using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
                {
                    info = Parse(sr.ReadToEnd());
                }
            }
            catch { }
            if (done != null) done(info);
        });
    }
}

public class NightLightForm : Form
{
    public NightLightForm()
    {
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        StartPosition = FormStartPosition.Manual;
        Bounds = SystemInformation.VirtualScreen;
        BackColor = Color.FromArgb(255, 176, 96);
        Opacity = 0.16;
        try { SetStyle(ControlStyles.Selectable, false); } catch { }
    }

    protected override bool ShowWithoutActivation { get { return true; } }

    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams cp = base.CreateParams;
            cp.ExStyle |= 0x00000020 | 0x00080000 | 0x08000000 | 0x00000080; // TRANSPARENT | LAYERED | NOACTIVATE | TOOLWINDOW
            return cp;
        }
    }
}

public static class QuickActions
{
    [DllImport("user32.dll")]
    static extern bool LockWorkStation();
    [DllImport("powrprof.dll", SetLastError = true)]
    static extern bool SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);

    public static void Lock() { try { LockWorkStation(); } catch { } }
    public static void Sleep() { try { SetSuspendState(false, true, false); } catch { } }
    public static void Hibernate() { try { SetSuspendState(true, true, false); } catch { } }
}

public static class AmbientLight
{
    static LightSensor _sensor;
    static bool _tried;

    static void Init()
    {
        if (_tried) return;
        _tried = true;
        try { _sensor = LightSensor.GetDefault(); }
        catch { _sensor = null; }
    }

    public static bool Available
    {
        get { Init(); return _sensor != null; }
    }

    public static bool TryReadLux(out double lux)
    {
        lux = 0;
        Init();
        if (_sensor == null) return false;
        try
        {
            LightSensorReading reading = _sensor.GetCurrentReading();
            if (reading == null) return false;
            lux = reading.IlluminanceInLux;
            return true;
        }
        catch { return false; }
    }

    public static int MapLuxToPercent(double lux, bool onBattery)
    {
        int pct;
        if (lux < 10) pct = 22;
        else if (lux < 50) pct = 35;
        else if (lux < 200) pct = 55;
        else if (lux < 500) pct = 75;
        else pct = 100;
        if (onBattery) pct = (pct * 85 + 50) / 100;
        if (pct < 10) pct = 10;
        if (pct > 100) pct = 100;
        return pct;
    }
}

public static class BootCampControl
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern IntPtr CreateFileW(string name, uint access, uint share, IntPtr sec, uint disp, uint flags, IntPtr templ);
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool DeviceIoControl(IntPtr h, uint code, byte[] inBuf, uint inSize, byte[] outBuf, uint outSize, out uint returned, IntPtr ov);
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool CloseHandle(IntPtr h);

    const uint GENERIC_READ = 0x80000000, GENERIC_WRITE = 0x40000000, FILE_SHARE = 3, OPEN_EXISTING = 3;
    const uint IOCTL_FN = 0xB403201C;
    const uint IOCTL_ACPI_BRIGHTNESS = 0xB4032048;
    const string KEY_PATH = @"Software\Apple Inc.\Apple Keyboard Support";
    const string FN_VALUE = "OSXFnBehavior";

    static readonly string[] KeyboardDevices = new string[]
    {
        @"\\.\AppleDFR",
        @"\\.\AppleSPIKeyboard",
        @"\\.\AppleKeyboardInternalUSB",
        @"\\.\AppleKeyboard"
    };

    public static bool ReadFnStandard()
    {
        try
        {
            using (RegistryKey k = Registry.CurrentUser.OpenSubKey(KEY_PATH))
            {
                if (k == null) return false;
                object v = k.GetValue(FN_VALUE);
                // OSXFnBehavior: 1 = Mac behaviour (hardware keys without Fn), 0 = standard F1-F12.
                return v != null && Convert.ToInt32(v) == 0;
            }
        }
        catch { return false; }
    }

    public static bool SetFnStandard(bool standard)
    {
        try
        {
            using (RegistryKey k = Registry.CurrentUser.CreateSubKey(KEY_PATH))
            {
                if (k != null) k.SetValue(FN_VALUE, standard ? 0 : 1, RegistryValueKind.DWord);
            }
        }
        catch { }

        bool applied = false;
        byte[] buffer = BitConverter.GetBytes(standard ? 0 : 1);
        byte[] brightness = BitConverter.GetBytes(1);
        foreach (string dev in KeyboardDevices)
        {
            IntPtr h = new IntPtr(-1);
            try
            {
                h = CreateFileW(dev, GENERIC_READ | GENERIC_WRITE, FILE_SHARE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
                if (h == new IntPtr(-1)) continue;
                uint returned;
                if (DeviceIoControl(h, IOCTL_FN, buffer, 4, null, 0, out returned, IntPtr.Zero))
                {
                    uint br;
                    DeviceIoControl(h, IOCTL_ACPI_BRIGHTNESS, brightness, 4, null, 0, out br, IntPtr.Zero);
                    applied = true;
                }
            }
            catch { }
            finally
            {
                if (h != new IntPtr(-1)) CloseHandle(h);
            }
        }
        return applied;
    }

    public static bool IsPrecisionTouchpad()
    {
        try
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                "SELECT Name FROM Win32_PnPEntity WHERE Name LIKE '%Precision%'"))
            {
                foreach (ManagementObject item in searcher.Get())
                {
                    string name = Convert.ToString(item["Name"]);
                    if (string.IsNullOrEmpty(name)) continue;
                    bool trackpad = name.IndexOf("Trackpad", StringComparison.OrdinalIgnoreCase) >= 0;
                    bool touchpad = name.IndexOf("Touchpad", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (trackpad || touchpad) return true;
                }
            }
        }
        catch { }
        try
        {
            using (RegistryKey k = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\PrecisionTouchpad"))
            {
                if (k != null) return true;
            }
        }
        catch { }
        return false;
    }
}
