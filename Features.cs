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
            XmlDocument doc = new XmlDocument();
            doc.Load(tmp);
            XmlNode battery = doc.SelectSingleNode("//Battery");
            if (battery == null) return false;
            design = ReadInt(battery, "DesignCapacity");
            full = ReadInt(battery, "FullChargeCapacity");
            cycles = ReadInt(battery, "CycleCount");
            charge = ReadInt(battery, "ChargeCapacity");
            if (design > 0 && full > 0)
            {
                // charge capacity is not always present; fall back to WMI
                if (charge <= 0) charge = WmiCharge();
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

    static int ReadInt(XmlNode parent, string name)
    {
        XmlNode n = parent.SelectSingleNode(name);
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
