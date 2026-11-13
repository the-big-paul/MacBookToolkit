using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

public static class Tests
{
    static int pass = 0;
    static int fail = 0;
    static StringBuilder failures = new StringBuilder();

    static void Check(string name, bool ok, string detail)
    {
        if (ok) pass++;
        else { fail++; failures.AppendLine("FAIL: " + name + " | " + detail); }
    }

    [STAThread]
    static int Main()
    {
        TestDefaults();
        TestParsingAndClamping();
        TestMalformedInput();
        TestNormalizeIdempotent();
        TestDebouncer();
        TestDebouncerAutoRepeatLimit();
        TestDebouncerTickZero();
        TestExcludedKeys();
        TestDimClamp();
        TestLoadFlags();
        TestIdleDecision();
        TestLocalization();
        TestVersion();
        Console.WriteLine("PASS=" + pass + " FAIL=" + fail);
        if (fail > 0) Console.WriteLine(failures.ToString());
        return fail == 0 ? 0 : 1;
    }

    static void TestDefaults()
    {
        Settings s = new Settings();
        Check("defaults ScreenEnabled", s.ScreenEnabled == true, "ScreenEnabled");
        Check("defaults IdleSec", s.IdleSec == 15, "" + s.IdleSec);
        Check("defaults DimPct", s.DimPct == 10, "" + s.DimPct);
        Check("defaults NormalPct", s.NormalPct == 100, "" + s.NormalPct);
        Check("defaults BlockMs", s.BlockMs == 700, "" + s.BlockMs);
        Check("defaults DebounceUpMs", s.DebounceUpMs == 70, "" + s.DebounceUpMs);
        Check("defaults DebounceDownMs", s.DebounceDownMs == 25, "" + s.DebounceDownMs);
        Check("defaults ScreenOffMin", s.ScreenOffMin == 2, "" + s.ScreenOffMin);
        Check("defaults Lang", s.Lang == "ru", s.Lang);
        Check("MaxDebounceDownMs", Settings.MaxDebounceDownMs == 30, "" + Settings.MaxDebounceDownMs);
        Check("defaults AutoStart", s.AutoStart == true, "");
        Check("defaults ShowTray", s.ShowTray == true, "");
        Check("defaults TouchEnabled", s.TouchEnabled == true, "");
        Check("defaults KeyEnabled", s.KeyEnabled == true, "");
    }

    static void TestParsingAndClamping()
    {
        Settings lo = Settings.FromLines(new string[] {
            "ScreenEnabled=False", "IdleSec=0", "DimPct=999", "NormalPct=100",
            "BlockMs=1", "DebounceUpMs=-1", "DebounceDownMs=100",
            "ScreenOffMin=0", "Lang=xx"
        });
        Check("clamp IdleSec min", lo.IdleSec == 1, "" + lo.IdleSec);
        Check("clamp DimPct max", lo.DimPct == 100, "" + lo.DimPct);

        Settings np = Settings.FromLines(new string[] { "NormalPct=-10", "DimPct=50" });
        Check("clamp NormalPct min", np.NormalPct == 1, "" + np.NormalPct);
        Check("DimPct follows lowered NormalPct", np.DimPct == 1, "" + np.DimPct);
        Check("clamp BlockMs min", lo.BlockMs == 50, "" + lo.BlockMs);
        Check("clamp DebounceUpMs min", lo.DebounceUpMs == 0, "" + lo.DebounceUpMs);
        Check("clamp DebounceDownMs max", lo.DebounceDownMs == 30, "" + lo.DebounceDownMs);
        Check("clamp ScreenOffMin min", lo.ScreenOffMin == 1, "" + lo.ScreenOffMin);
        Check("invalid Lang -> ru", lo.Lang == "ru", lo.Lang);
        Check("bool false respected", lo.ScreenEnabled == false, "");

        Settings hi = Settings.FromLines(new string[] {
            "IdleSec=9999", "BlockMs=99999", "DebounceUpMs=9999", "ScreenOffMin=999",
            "NormalPct=999", "DimPct=0", "Lang=en"
        });
        Check("clamp IdleSec max", hi.IdleSec == 300, "" + hi.IdleSec);
        Check("clamp BlockMs max", hi.BlockMs == 2000, "" + hi.BlockMs);
        Check("clamp DebounceUpMs max", hi.DebounceUpMs == 200, "" + hi.DebounceUpMs);
        Check("clamp ScreenOffMin max", hi.ScreenOffMin == 60, "" + hi.ScreenOffMin);
        Check("clamp NormalPct max", hi.NormalPct == 100, "" + hi.NormalPct);
        Check("clamp DimPct min", hi.DimPct == 1, "" + hi.DimPct);
        Check("valid Lang kept", hi.Lang == "en", hi.Lang);

        Settings bools = Settings.FromLines(new string[] { "ScreenEnabled=FALSE", "TouchEnabled=false", "KeyEnabled=True", "AutoStart=false" });
        Check("bool FALSE parse", bools.ScreenEnabled == false, "");
        Check("bool false parse", bools.TouchEnabled == false, "");
        Check("bool True parse", bools.KeyEnabled == true, "");
        Check("bool false autostart", bools.AutoStart == false, "");
    }

    static void TestMalformedInput()
    {
        Settings s = Settings.FromLines(null);
        Check("null lines -> defaults", s.IdleSec == 15 && s.Lang == "ru", "" + s.IdleSec);
        Settings m = Settings.FromLines(new string[] {
            "garbage", "a=b=c", "=x", "IdleSec=abc", "", "IdleSec=30", "IdleSec=40"
        });
        Check("duplicate last wins", m.IdleSec == 40, "" + m.IdleSec);

        Settings emptyLang = Settings.FromLines(new string[] { "Lang=" });
        Check("empty Lang -> ru", emptyLang.Lang == "ru", emptyLang.Lang);

        Settings ukLang = Settings.FromLines(new string[] { "Lang=uk", "IdleSec=30" });
        Check("valid Lang kept (uk)", ukLang.Lang == "uk", ukLang.Lang);
    }

    static void TestNormalizeIdempotent()
    {
        Settings s = Settings.FromLines(new string[] { "IdleSec=10000", "Lang=zz" });
        int a = s.IdleSec; string l = s.Lang;
        s.Normalize();
        Check("normalize idempotent", s.IdleSec == a && s.Lang == l, "");
    }

    static void TestDebouncer()
    {
        KeyDebouncer d1 = new KeyDebouncer();
        Check("first press allowed", d1.ShouldDropKeyDown(65, 5000, 70, 25) == false, "");

        KeyDebouncer d2 = new KeyDebouncer();
        d2.NoteKeyUp(65, 1000);
        Check("chatter dropped (30ms after release)", d2.ShouldDropKeyDown(65, 1030, 70, 25) == true, "");

        KeyDebouncer d3 = new KeyDebouncer();
        d3.NoteKeyUp(65, 1000);
        Check("legit fast press (80ms) allowed", d3.ShouldDropKeyDown(65, 1080, 70, 25) == false, "");

        KeyDebouncer d4 = new KeyDebouncer();
        d4.NoteKeyUp(65, 1000);
        Check("other key unaffected", d4.ShouldDropKeyDown(66, 1030, 70, 25) == false, "");

        KeyDebouncer d5 = new KeyDebouncer();
        d5.NoteKeyUp(65, 0xFFFFFFF0);
        Check("tick wrap handled", d5.ShouldDropKeyDown(65, 0x10, 70, 25) == true, "");

        KeyDebouncer d6 = new KeyDebouncer();
        d6.NoteKeyUp(65, 1000);
        d6.ShouldDropKeyDown(65, 1080, 70, 25);   // accepted, lastDown=1080
        Check("auto-repeat 32ms allowed at downMs=25", d6.ShouldDropKeyDown(65, 1112, 70, 25) == false, "");
    }

    static void TestDebouncerAutoRepeatLimit()
    {
        // 32 ms repeat interval (Windows fastest) must survive the 30 ms max slider value
        KeyDebouncer a = new KeyDebouncer();
        a.NoteKeyUp(65, 1000);
        a.ShouldDropKeyDown(65, 1080, 70, 30);
        Check("repeat 32ms allowed at max downMs=30", a.ShouldDropKeyDown(65, 1112, 70, 30) == false, "");

        // values above 32 ms would break auto-repeat - this documents why the cap is 30
        KeyDebouncer b = new KeyDebouncer();
        b.NoteKeyUp(65, 1000);
        b.ShouldDropKeyDown(65, 1080, 70, 40);
        Check("repeat 32ms blocked at downMs=40 (why cap exists)", b.ShouldDropKeyDown(65, 1112, 70, 40) == true, "");
    }

    static void TestDebouncerTickZero()
    {
        // Environment.TickCount can legitimately be 0 once per ~49.7 days
        KeyDebouncer d = new KeyDebouncer();
        d.NoteKeyUp(65, 0);
        Check("tick 0: release at 0 tracked", d.ShouldDropKeyDown(65, 10, 70, 25) == true, "");
        KeyDebouncer e = new KeyDebouncer();
        Check("tick 0: first press allowed", e.ShouldDropKeyDown(65, 0, 70, 25) == false, "");
    }

    static void TestExcludedKeys()
    {
        MethodInfo m = typeof(MainForm).GetMethod("Excluded", BindingFlags.NonPublic | BindingFlags.Static);
        if (m == null) { Check("Excluded exists", false, "reflection failed"); return; }
        int[] excluded = { 0x08, 0x09, 0x0D, 0x1B, 0x2E, 0x25, 0x26, 0x27, 0x28, 0x21, 0x22, 0x23, 0x24, 0x10, 0x11, 0x12, 0xA0, 0xA5, 0x5B, 0x5C, 0x5D, 0x2C };
        bool all = true;
        foreach (int vk in excluded) { if ((bool)m.Invoke(null, new object[] { vk }) != true) { all = false; } }
        Check("excluded keys pass through", all, "");
        Check("normal letter A is debounced", (bool)m.Invoke(null, new object[] { 0x41 }) == false, "");
    }

    static void TestDimClamp()
    {
        Settings s = Settings.FromLines(new string[] { "DimPct=90", "NormalPct=50" });
        Check("DimPct clamped to NormalPct", s.DimPct == 50, "" + s.DimPct);
    }

    static void TestLoadFlags()
    {
        Settings s = new Settings();
        Check("fresh defaults: Existed=false", s.Existed == false, "");
        Check("fresh defaults: ParseFailed=false", s.ParseFailed == false, "");
    }

    static void TestIdleDecision()
    {
        MethodInfo m = typeof(MainForm).GetMethod("WantsDim", BindingFlags.NonPublic | BindingFlags.Static);
        if (m == null) { Check("WantsDim exists", false, "reflection failed"); return; }

        Settings s = new Settings();
        s.ScreenEnabled = true; s.IdleSec = 15;
        Check("no dim at 14999ms", (bool)m.Invoke(null, new object[] { (uint)14999, s }) == false, "");
        Check("dim at 15000ms", (bool)m.Invoke(null, new object[] { (uint)15000, s }) == true, "");
        Check("dim stays at 15001ms", (bool)m.Invoke(null, new object[] { (uint)15001, s }) == true, "");

        Settings off = new Settings();
        off.ScreenEnabled = false; off.IdleSec = 1;
        Check("disabled never dims", (bool)m.Invoke(null, new object[] { (uint)999999, off }) == false, "");

        Settings fast = new Settings();
        fast.ScreenEnabled = true; fast.IdleSec = 1;
        Check("idleSec=1: 999ms no dim", (bool)m.Invoke(null, new object[] { (uint)999, fast }) == false, "");
        Check("idleSec=1: 1000ms dim", (bool)m.Invoke(null, new object[] { (uint)1000, fast }) == true, "");
        Check("idleSec=1: 1500ms still dim (old flicker bug)", (bool)m.Invoke(null, new object[] { (uint)1500, fast }) == true, "");

        // monotone: once dim is wanted with growing idle it must never flip back
        bool mono = true;
        for (int sec = 1; sec <= 30 && mono; sec++)
        {
            Settings t = new Settings();
            t.ScreenEnabled = true; t.IdleSec = sec;
            bool seen = false;
            for (uint idle = 0; idle <= (uint)(sec * 1000 + 10000); idle += 250)
            {
                bool want = (bool)m.Invoke(null, new object[] { idle, t });
                if (seen && !want) { mono = false; break; }
                if (want) seen = true;
            }
        }
        Check("dim decision monotone (no flicker) for idleSec 1..30", mono, "");
    }

    static void TestLocalization()
    {
        Type t = typeof(L10n);
        Dictionary<string, string> ru = (Dictionary<string, string>)t.GetField("ru", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        Dictionary<string, string> uk = (Dictionary<string, string>)t.GetField("uk", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        Dictionary<string, string> en = (Dictionary<string, string>)t.GetField("en", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        Check("ru key count", ru.Count == 40, "" + ru.Count);
        Check("uk key count", uk.Count == 40, "" + uk.Count);
        Check("en key count", en.Count == 40, "" + en.Count);
        bool same = true;
        foreach (string k in ru.Keys) if (!uk.ContainsKey(k) || !en.ContainsKey(k)) same = false;
        foreach (string k in uk.Keys) if (!ru.ContainsKey(k)) same = false;
        Check("same key sets", same, "");
        bool noEmpty = true;
        foreach (string k in ru.Keys) if (string.IsNullOrEmpty(ru[k]) || string.IsNullOrEmpty(uk[k]) || string.IsNullOrEmpty(en[k])) noEmpty = false;
        Check("no empty translations", noEmpty, "");

        L10n.Lang = "en";
        Check("T en", L10n.T("btn_save") == "Save & apply", L10n.T("btn_save"));
        L10n.Lang = "ru";
        Check("T ru", L10n.T("btn_save") == "Сохранить и применить", L10n.T("btn_save"));
        L10n.Lang = "uk";
        Check("T uk", L10n.T("btn_save") == "Зберегти та застосувати", L10n.T("btn_save"));
        L10n.Lang = "zz";
        Check("T unknown lang falls back to ru", L10n.T("btn_save") == "Сохранить и применить", L10n.T("btn_save"));
        L10n.Lang = "ru";
        Check("T missing key returns key", L10n.T("no_such_key") == "no_such_key", "");
    }

    static void TestVersion()
    {
        Assembly asm = typeof(MainForm).Assembly;
        Check("assembly version 1.1.0.0", asm.GetName().Version.ToString() == "1.1.0.0", asm.GetName().Version.ToString());
        object[] fv = asm.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
        Check("file version attribute", fv.Length == 1 && ((AssemblyFileVersionAttribute)fv[0]).Version == "1.1.0.0", "");
        object[] iv = asm.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
        Check("informational version 1.1", iv.Length == 1 && ((AssemblyInformationalVersionAttribute)iv[0]).InformationalVersion == "1.1", "");
    }
}
