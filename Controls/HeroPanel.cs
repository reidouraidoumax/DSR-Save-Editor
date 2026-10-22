using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RaksasiSaveEditor
{
    public enum ChecksumState
    {
        None,
        Valid,
        Mismatch,
        Fixed
    }

    /// <summary>
    /// Top banner: gradient with a turquoise and an ember glow, a faint dot
    /// grid, the Raksasi logo, and quick stats for the loaded save. The
    /// Import/Export buttons are ordinary child controls (see the Designer).
    /// </summary>
    public class HeroPanel : Panel
    {
        private Image _logo;
        private string _fileName = "—";
        private string _souls = "—";
        private string _kills = "—";
        private ChecksumState _checksum = ChecksumState.None;

        public HeroPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Bg;
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image Logo
        {
            get { return _logo; }
            set { _logo = value; Invalidate(); }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SaveFileName
        {
            get { return _fileName; }
            set { _fileName = string.IsNullOrEmpty(value) ? "—" : value; Invalidate(); }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Souls
        {
            get { return _souls; }
            set { _souls = string.IsNullOrEmpty(value) ? "—" : value; Invalidate(); }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Kills
        {
            get { return _kills; }
            set { _kills = string.IsNullOrEmpty(value) ? "—" : value; Invalidate(); }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ChecksumState Checksum
        {
            get { return _checksum; }
            set { _checksum = value; Invalidate(); }
        }

        /// <summary>Left edge of the right-hand button group; stats never draw past it.</summary>
        private int ContentRightLimit()
        {
            int limit = Width - Theme.S(32);
            foreach (Control c in Controls)
            {
                if (c.Visible)
                {
                    limit = Math.Min(limit, c.Left - Theme.S(24));
                }
            }
            return limit;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle r = ClientRectangle;
            if (r.Width <= 0 || r.Height <= 0)
            {
                return;
            }

            using (var bg = new LinearGradientBrush(r, Theme.HeroTop, Theme.Bg, 90f))
            {
                g.FillRectangle(bg, r);
            }

            g.SmoothingMode = SmoothingMode.AntiAlias;
            // Turquoise glow behind the logo, ember glow top-right.
            PaintGlow(g, new RectangleF(-Theme.SF(160), -r.Height * 0.6f, Theme.SF(900), r.Height * 2.3f), Theme.Alpha(Theme.Accent, 60));
            PaintGlow(g, new RectangleF(r.Width - Theme.SF(560), -r.Height * 1.1f, Theme.SF(820), r.Height * 2.0f), Theme.Alpha(Theme.Ember, 58));

            // Dot grid that fades in toward the right.
            int step = Theme.S(18);
            float startX = r.Width * 0.35f;
            using (var dot = new SolidBrush(Color.White))
            {
                for (int x = (int)startX - ((int)startX % step); x < r.Width; x += step)
                {
                    float t = (x - startX) / Math.Max(1f, r.Width - startX);
                    int a = (int)(14 * Math.Max(0f, Math.Min(1f, t * 1.6f)));
                    if (a <= 0)
                    {
                        continue;
                    }
                    dot.Color = Theme.Alpha(Color.White, a);
                    for (int y = step / 2; y < r.Height; y += step)
                    {
                        g.FillRectangle(dot, x, y, Math.Max(1, Theme.S(1.4f)), Math.Max(1, Theme.S(1.4f)));
                    }
                }
            }

            using (var line = new Pen(Theme.Line))
            {
                g.DrawLine(line, 0, r.Height - 1, r.Width, r.Height - 1);
            }
        }

        private static void PaintGlow(Graphics g, RectangleF ellipse, Color center)
        {
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(ellipse);
                using (var brush = new PathGradientBrush(path))
                {
                    brush.CenterColor = center;
                    brush.SurroundColors = new[] { Color.FromArgb(0, center.R, center.G, center.B) };
                    brush.FocusScales = new PointF(0.05f, 0.05f);
                    g.FillPath(brush, path);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Theme.Smooth(g);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            int left = Theme.S(30);
            // Slightly smaller logo when the window is narrow, to leave room for stats.
            int logoH = Width < Theme.S(1240) ? Height - Theme.S(52) : Height - Theme.S(30);
            int logoW;
            if (_logo != null)
            {
                logoW = (int)(logoH * (_logo.Width / (float)_logo.Height));
                g.DrawImage(_logo, new Rectangle(left, (Height - logoH) / 2, logoW, logoH));
            }
            else
            {
                // Design-time placeholder.
                logoW = Theme.S(280);
                Theme.DrawText(g, "Devil Slayer Raksasi", Theme.UiBold(20f), Theme.Accent,
                    new Rectangle(left, 0, logoW, Height), TextFormatFlags.VerticalCenter);
            }

            int x = left + logoW + Theme.S(24);
            int limit = ContentRightLimit();
            if (x + Theme.S(80) > limit)
            {
                return;
            }

            int dividerTop = Height / 2 - Theme.S(34);
            using (var pen = new Pen(Theme.Line))
            {
                g.DrawLine(pen, x, dividerTop, x, dividerTop + Theme.S(68));
            }
            x += Theme.S(28);

            string cs;
            Color csColor;
            Color? dot;
            switch (_checksum)
            {
                case ChecksumState.Valid: cs = "Valid"; csColor = Theme.Accent; dot = Theme.Accent; break;
                case ChecksumState.Fixed: cs = "Fixed"; csColor = Theme.Accent; dot = Theme.Accent; break;
                case ChecksumState.Mismatch: cs = "Mismatch"; csColor = Theme.Amber; dot = Theme.Amber; break;
                default: cs = "—"; csColor = Theme.Text; dot = null; break;
            }

            // Most important first: anything that doesn't fit is dropped from the end.
            x = DrawStat(g, x, limit, "SAVE FILE", _fileName, Theme.Text, null);
            if (x < 0) return;
            x = DrawStat(g, x, limit, "CHECKSUM", cs, csColor, dot);
            if (x < 0) return;
            x = DrawStat(g, x, limit, "SOULS", _souls, Theme.Text, null);
            if (x < 0) return;
            DrawStat(g, x, limit, "KILLS", _kills, Theme.Text, null);
        }

        /// <summary>Draws one label/value stat; returns the next x, or -1 if it didn't fit.</summary>
        private int DrawStat(Graphics g, int x, int limit, string label, string value, Color valueColor, Color? dot)
        {
            Font labelFont = Theme.UiSemi(7.5f);
            Font valueFont = dot.HasValue ? Theme.UiSemi(12f) : Theme.UiSemi(15f);

            int dotW = dot.HasValue ? Theme.S(16) : 0;
            Size ls = Theme.Measure(label, labelFont);
            Size vs = Theme.Measure(value, valueFont);
            int w = Math.Max(ls.Width, vs.Width + dotW);
            if (x + w > limit)
            {
                return -1;
            }

            int top = Height / 2 - Theme.S(26);
            Theme.DrawText(g, label, labelFont, Theme.Text3, new Rectangle(x, top, w + 4, ls.Height), TextFormatFlags.Left);

            int valueTop = top + ls.Height + Theme.S(6);
            int valueH = Theme.Measure("0", Theme.UiSemi(15f)).Height;
            if (dot.HasValue)
            {
                float d = Theme.SF(8);
                float cy = valueTop + valueH / 2f;
                using (var glow = new SolidBrush(Theme.Alpha(dot.Value, 60)))
                using (var b = new SolidBrush(dot.Value))
                {
                    g.FillEllipse(glow, x - d * 0.5f, cy - d, d * 2, d * 2);
                    g.FillEllipse(b, x, cy - d / 2, d, d);
                }
            }
            Theme.DrawText(g, value, valueFont, valueColor, new Rectangle(x + dotW, valueTop, vs.Width + 4, valueH),
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            return x + w + Theme.S(30);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            Invalidate();
        }
    }
}
