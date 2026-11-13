using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public class IconGen
{
    static readonly Color Blue = Color.FromArgb(0, 103, 192);
    static readonly Color Plus = Color.FromArgb(156, 201, 245);
    static readonly Color White = Color.White;

    static void Main(string[] args)
    {
        string dir = args.Length > 0 ? args[0] : ".";
        string outIco = Path.Combine(dir, "app.ico");
        int[] sizes = new int[] { 16, 20, 24, 32, 40, 48, 64, 128, 256 };
        Bitmap[] frames = new Bitmap[sizes.Length];
        for (int i = 0; i < sizes.Length; i++) frames[i] = Draw(sizes[i]);
        WriteIco(outIco, frames, sizes);
        WritePreview(frames, sizes, Path.Combine(dir, "review", "icon-preview.png"));
        Console.WriteLine("icons written");
    }

    static Font MakeFont(float px)
    {
        string[] names = new string[] { "Segoe UI Black", "Arial Black", "Segoe UI" };
        foreach (string n in names)
        {
            try { return new Font(n, px, FontStyle.Bold, GraphicsUnit.Pixel); }
            catch { }
        }
        return new Font(FontFamily.GenericSansSerif, px, FontStyle.Bold, GraphicsUnit.Pixel);
    }

    static Bitmap Draw(int s)
    {
        Bitmap bmp = new Bitmap(s, s);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.Clear(Color.Transparent);

            RectangleF rr = new RectangleF(0, 0, s, s);
            using (GraphicsPath bg = Round(rr, s * 0.18f))
            using (SolidBrush b = new SolidBrush(Blue)) g.FillPath(b, bg);

            int tiny = s <= 20 ? 1 : 0;
            // gear badge top-right (behind the letters)
            float d = s * (tiny == 1 ? 0.52f : 0.50f);
            float cx = s - d / 2 - s * 0.01f, cy = d / 2 + s * 0.01f;
            RectangleF circle = new RectangleF(cx - d / 2, cy - d / 2, d, d);
            using (SolidBrush w = new SolidBrush(White)) g.FillEllipse(w, circle);
            using (Pen pen = new Pen(Blue, Math.Max(1f, s * 0.012f))) g.DrawEllipse(pen, circle);
            float gearPx = d * 0.62f;
            try
            {
                using (FontFamily fam = new FontFamily("Segoe Fluent Icons"))
                using (Font gf = new Font(fam, gearPx, FontStyle.Regular, GraphicsUnit.Pixel))
                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    using (SolidBrush b = new SolidBrush(Blue)) g.DrawString("\uE713", gf, b, circle, sf);
                }
            }
            catch { }

            // letter M (on top of the gear if they overlap)
            float mpx = s * 0.66f;
            using (Font mf = MakeFont(mpx))
            {
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                RectangleF mr = new RectangleF(s * 0.05f, s * 0.18f, s * 0.62f, s * 0.64f);
                using (SolidBrush wb = new SolidBrush(White)) g.DrawString("M", mf, wb, mr, sf);
            }

            // light-blue plus at the right
            float bar = s * 0.05f;
            float plen = s * 0.17f;
            float px = s * 0.74f, py = s * 0.52f;
            using (SolidBrush pb = new SolidBrush(Plus))
            {
                g.FillRectangle(pb, px, py - bar / 2, plen, bar);
                g.FillRectangle(pb, px + plen / 2 - bar / 2, py - plen / 2, bar, plen);
            }
        }
        return bmp;
    }

    static GraphicsPath Round(RectangleF r, float radius)
    {
        float dd = radius * 2;
        GraphicsPath p = new GraphicsPath();
        p.AddArc(r.X, r.Y, dd, dd, 180, 90);
        p.AddArc(r.Right - dd, r.Y, dd, dd, 270, 90);
        p.AddArc(r.Right - dd, r.Bottom - dd, dd, dd, 0, 90);
        p.AddArc(r.X, r.Bottom - dd, dd, dd, 90, 90);
        p.CloseFigure();
        return p;
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
            g.SmoothingMode = SmoothingMode.AntiAlias;
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
