using System;
using System.Management;

public static class CpuTemp
{
    public static double FromKelvinTenths(int raw)
    {
        return raw / 10.0 - 273.15;
    }

    public static bool TryReadAverage(out double celsius)
    {
        celsius = 0;
        try
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                "root\\WMI", "SELECT CurrentTemperature FROM MSAcpi_ThermalZoneTemperature"))
            using (ManagementObjectCollection zones = searcher.Get())
            {
                double sum = 0;
                int n = 0;
                foreach (ManagementObject zone in zones)
                {
                    using (zone)
                    {
                        object raw = zone["CurrentTemperature"];
                        if (raw == null) continue;
                        double c = FromKelvinTenths(Convert.ToInt32(raw));
                        if (c > -20 && c < 150) { sum += c; n++; }
                    }
                }
                if (n > 0)
                {
                    celsius = sum / n;
                    return true;
                }
            }
        }
        catch { }
        return false;
    }
}
