using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public class IconGen
{
    static void Main(string[] args)
    {
        string dir = args.Length > 0 ? args[0] : ".";
        Bitmap baseBmp = LoadLargest(Path.Combine(dir, "app-base.ico"));
        int[] sizes = new int[] { 16, 20, 24, 32, 40, 48, 64, 128, 256 };
        Bitmap letters256 = BuildLetters(baseBmp);
        Bitmap outline256 = BuildOutline(letters256, 2);
        Bitmap[] frames = new Bitmap[sizes.Length];
        for (int i = 0; i < sizes.Length; i++) frames[i] = Compose(baseBmp, outline256, letters256, sizes[i]);
        WriteIco(Path.Combine(dir, "app.ico"), frames, sizes);
        WritePreview(frames, sizes, Path.Combine(dir, "review", "icon-preview.png"));
        Console.WriteLine("icons written");
    }

    static Bitmap Compose(Bitmap baseBmp, Bitmap outline, Bitmap letters, int s)
    {
        Bitmap bmp = new Bitmap(s, s);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.Clear(Color.Transparent);
            g.DrawImage(baseBmp, new Rectangle(0, 0, s, s));
            DrawGear(g, s);
            g.DrawImage(outline, new Rectangle(0, 0, s, s));
            g.DrawImage(letters, new Rectangle(0, 0, s, s));
        }
        return bmp;
    }

    static void DrawGear(Graphics g, int s)
    {
        float d = s * 0.42f;
        float margin = s * 0.012f;
        RectangleF r = new RectangleF(s - d - margin, margin, d, d);
        Color dark = Color.FromArgb(6, 32, 58);
        try
        {
            using (FontFamily fam = new FontFamily("Segoe Fluent Icons"))
            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                using (Font white = new Font(fam, d * 0.70f, FontStyle.Regular, GraphicsUnit.Pixel))
                using (Font darkFont = new Font(fam, d * 0.60f, FontStyle.Regular, GraphicsUnit.Pixel))
                using (SolidBrush wb = new SolidBrush(Color.FromArgb(235, 255, 255, 255)))
                using (SolidBrush db = new SolidBrush(dark))
                {
                    g.DrawString("\uE713", white, wb, r, sf);
                    g.DrawString("\uE713", darkFont, db, r, sf);
                }
            }
        }
        catch { }
    }

    static Bitmap BuildLetters(Bitmap src)
    {
        Bitmap layer = new Bitmap(src.Width, src.Height);
        for (int y = 0; y < src.Height; y++)
        {
            for (int x = 0; x < src.Width; x++)
            {
                Color c = src.GetPixel(x, y);
                bool white = c.R > 210 && c.G > 210 && c.B > 210;
                bool lightBlue = c.B > 185 && c.G > 135 && c.R > 80 && c.R < 235;
                if (white || lightBlue) layer.SetPixel(x, y, c);
            }
        }
        return layer;
    }

    static Bitmap BuildOutline(Bitmap letters, int radius)
    {
        Bitmap outline = new Bitmap(letters.Width, letters.Height);
        Color dark = Color.FromArgb(6, 32, 58);
        for (int y = 0; y < letters.Height; y++)
        {
            for (int x = 0; x < letters.Width; x++)
            {
                bool edge = false;
                for (int dy = -radius; dy <= radius && !edge; dy++)
                {
                    for (int dx = -radius; dx <= radius; dx++)
                    {
                        if (dx * dx + dy * dy > radius * radius) continue;
                        int nx = x + dx, ny = y + dy;
                        if (nx < 0 || ny < 0 || nx >= letters.Width || ny >= letters.Height) continue;
                        if (letters.GetPixel(nx, ny).A > 0) { edge = true; break; }
                    }
                }
                if (edge) outline.SetPixel(x, y, dark);
            }
        }
        return outline;
    }

    static Bitmap LoadLargest(string path)
    {
        byte[] all = File.ReadAllBytes(path);
        int count = BitConverter.ToUInt16(all, 4);
        int best = -1, bestSize = -1;
        for (int i = 0; i < count; i++)
        {
            int o = 6 + i * 16;
            int w = all[o]; if (w == 0) w = 256;
            if (w > bestSize) { bestSize = w; best = i; }
        }
        int e = 6 + best * 16;
        uint len = BitConverter.ToUInt32(all, e + 8);
        uint off = BitConverter.ToUInt32(all, e + 12);
        using (MemoryStream ms = new MemoryStream(all, (int)off, (int)len, false))
        using (Image img = Image.FromStream(ms))
        {
            return new Bitmap(img);
        }
    }

    static void WriteIco(string path, Bitmap[] frames, int[] sizes)
    {
        byte[][] pngs = new byte[frames.Length][];
        for (int i = 0; i < frames.Length; i++)
        {
            using (MemoryStream ms = new MemoryStream()) { frames[i].Save(ms, ImageFormat.Png); pngs[i] = ms.ToArray(); }
        }
        using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
        using (BinaryWriter bw = new BinaryWriter(fs))
        {
            bw.Write((ushort)0); bw.Write((ushort)1); bw.Write((ushort)frames.Length);
            int offset = 6 + frames.Length * 16;
            for (int i = 0; i < frames.Length; i++)
            {
                int s = sizes[i];
                bw.Write((byte)(s >= 256 ? 0 : s)); bw.Write((byte)(s >= 256 ? 0 : s));
                bw.Write((byte)0); bw.Write((byte)0); bw.Write((ushort)1); bw.Write((ushort)32);
                bw.Write((uint)pngs[i].Length); bw.Write((uint)offset);
                offset += pngs[i].Length;
            }
            for (int i = 0; i < frames.Length; i++) bw.Write(pngs[i]);
        }
    }

    static void WritePreview(Bitmap[] frames, int[] sizes, string path)
    {
        int w = 1100, h = 430;
        using (Bitmap bmp = new Bitmap(w, h))
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.Clear(Color.FromArgb(243, 243, 243));
            using (SolidBrush dark = new SolidBrush(Color.FromArgb(32, 32, 32))) g.FillRectangle(dark, 0, h / 2, w, h / 2);
            int x = 20;
            int[] draw = new int[] { 16, 20, 24, 32, 40, 48, 64 };
            for (int k = 0; k < draw.Length; k++)
            {
                Bitmap f = frames[Array.IndexOf(sizes, draw[k])];
                int zoom = 4;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(f, new Rectangle(x, 50, draw[k] * zoom, draw[k] * zoom));
                g.DrawImage(f, new Rectangle(x, h / 2 + 30, draw[k] * zoom, draw[k] * zoom));
                using (Font small = new Font("Segoe UI", 9)) g.DrawString(draw[k] + " px", small, Brushes.Gray, x, 18);
                x += draw[k] * zoom + 20;
            }
            bmp.Save(path, ImageFormat.Png);
        }
    }
}
