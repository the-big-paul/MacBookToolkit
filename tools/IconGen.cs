using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public class IconGen
{
    static readonly Color Accent = Color.FromArgb(0, 103, 192);

    static void Main(string[] args)
    {
        string dir = args.Length > 0 ? args[0] : ".";
        string baseIco = Path.Combine(dir, "app-base.ico");
        string outIco = Path.Combine(dir, "app.ico");
        Bitmap baseBmp = LoadLargest(baseIco);
        int[] sizes = new int[] { 16, 20, 24, 32, 40, 48, 64, 128, 256 };
        Bitmap[] frames = new Bitmap[sizes.Length];
        for (int i = 0; i < sizes.Length; i++)
        {
            int s = sizes[i];
            Bitmap bmp = new Bitmap(s, s);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.Clear(Color.Transparent);
                g.DrawImage(baseBmp, new Rectangle(0, 0, s, s));
                DrawBadge(g, s);
            }
            frames[i] = bmp;
        }
        WriteIco(outIco, frames, sizes);
        WritePreview(frames, sizes, Path.Combine(dir, "review", "icon-preview.png"));
        Console.WriteLine("icons written");
    }

    static void DrawBadge(Graphics g, int s)
    {
        bool tiny = s <= 20;
        float d = s * (tiny ? 0.50f : 0.42f);
        float margin = s * 0.015f;
        RectangleF circle = new RectangleF(s - d - margin, s - d - margin, d, d);
        using (SolidBrush b = new SolidBrush(Accent)) g.FillEllipse(b, circle);
        if (!tiny)
        {
            float ring = Math.Max(1f, s * 0.016f);
            using (Pen pen = new Pen(Color.White, ring))
                g.DrawEllipse(pen, circle.X + ring / 2, circle.Y + ring / 2, circle.Width - ring, circle.Height - ring);
        }
        string glyph = "\uE713";
        float px = d * (tiny ? 0.82f : 0.60f);
        try
        {
            using (FontFamily fam = new FontFamily("Segoe Fluent Icons"))
            using (Font font = new Font(fam, px, FontStyle.Regular, GraphicsUnit.Pixel))
            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                using (SolidBrush w = new SolidBrush(Color.White))
                    g.DrawString(glyph, font, w, circle, sf);
            }
        }
        catch { }
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
            using (MemoryStream ms = new MemoryStream())
            {
                frames[i].Save(ms, ImageFormat.Png);
                pngs[i] = ms.ToArray();
            }
        }
        using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
        using (BinaryWriter bw = new BinaryWriter(fs))
        {
            bw.Write((ushort)0); bw.Write((ushort)1); bw.Write((ushort)frames.Length);
            int offset = 6 + frames.Length * 16;
            for (int i = 0; i < frames.Length; i++)
            {
                int s = sizes[i];
                bw.Write((byte)(s >= 256 ? 0 : s));
                bw.Write((byte)(s >= 256 ? 0 : s));
                bw.Write((byte)0); bw.Write((byte)0);
                bw.Write((ushort)1); bw.Write((ushort)32);
                bw.Write((uint)pngs[i].Length);
                bw.Write((uint)offset);
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
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.FromArgb(243, 243, 243));
            using (SolidBrush dark = new SolidBrush(Color.FromArgb(32, 32, 32))) g.FillRectangle(dark, 0, h / 2, w, h / 2);
            int x = 20;
            int[] draw = new int[] { 16, 20, 24, 32, 40, 48, 64 };
            for (int k = 0; k < draw.Length; k++)
            {
                int idx = Array.IndexOf(sizes, draw[k]);
                Bitmap f = frames[idx];
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
