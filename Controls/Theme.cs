using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RaksasiSaveEditor
{
    /// <summary>
    /// "Spirit Lantern" design tokens: colors, fonts, spacing helpers and a
    /// few Win32 tweaks (dark title bar, dark scrollbars). Every custom
    /// control reads from here, so restyling the app means editing this file.
    /// </summary>
    internal static class Theme
    {
        // ---- Colors ------------------------------------------------------------
        public static readonly Color Bg = Hex(0x0A0812);         // window background
        public static readonly Color HeroTop = Hex(0x120E22);    // top of the hero gradient
        public static readonly Color Card = Hex(0x130F1E);       // card surface
        public static readonly Color CardHover = Hex(0x171226);
        public static readonly Color Card2 = Hex(0x191428);      // raised surface (pills, buttons)
        public static readonly Color InputBg = Hex(0x0D0A17);    // value box
        public static readonly Color Line = Hex(0x2A2340);       // hairlines / borders
        public static readonly Color Line2 = Hex(0x3A3156);      // stronger borders
        public static readonly Color Text = Hex(0xF1EEF8);
        public static readonly Color Text2 = Hex(0xA39CB8);
        public static readonly Color Text3 = Hex(0x6D6585);
        public static readonly Color Accent = Hex(0x40E0D0);     // Raksasi turquoise
        public static readonly Color AccentHi = Hex(0x5FF0E1);
        public static readonly Color AccentLo = Hex(0x2EC4B5);
        public static readonly Color AccentText = Hex(0x3CCFC0);  // example lines
        public static readonly Color AccentInk = Hex(0x06221F);  // text on turquoise
        public static readonly Color Ember = Hex(0xE2445C);      // devil red glow / invalid
        public static readonly Color Amber = Hex(0xF5B84A);      // pending edits

        public static Color Hex(int rgb) => Color.FromArgb(255, (rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);
        public static Color Alpha(Color c, int a) => Color.FromArgb(Math.Max(0, Math.Min(255, a)), c.R, c.G, c.B);

        public static Color Mix(Color a, Color b, float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            return Color.FromArgb(
                (int)(a.A + (b.A - a.A) * t),
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t));
        }

        // ---- DPI ---------------------------------------------------------------
        private static float _scale = -1f;

        /// <summary>System DPI scale (1.0 at 96 DPI). The app is SystemAware, so one value fits all.</summary>
        public static float Scale
        {
            get
            {
                if (_scale < 0f)
                {
                    try
                    {
                        using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
                        {
                            _scale = g.DpiX / 96f;
                        }
                    }
                    catch
                    {
                        _scale = 1f;
                    }
                    if (_scale <= 0f)
                    {
                        _scale = 1f;
                    }
                }
                return _scale;
            }
        }

        /// <summary>Scales a 96-DPI pixel value to the current DPI.</summary>
        public static int S(float px) => (int)Math.Round(px * Scale);
        public static float SF(float px) => px * Scale;

        // ---- Fonts -------------------------------------------------------------
        private static readonly Dictionary<string, Font> FontCache = new Dictionary<string, Font>();
        private static HashSet<string> _installed;

        private static string Pick(params string[] families)
        {
            if (_installed == null)
            {
                _installed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var fonts = new InstalledFontCollection())
                {
                    foreach (FontFamily f in fonts.Families)
                    {
                        _installed.Add(f.Name);
                    }
                }
            }
            foreach (string name in families)
            {
                if (_installed.Contains(name))
                {
                    return name;
                }
            }
            return FontFamily.GenericSansSerif.Name;
        }

        private static Font Get(string family, float size, FontStyle style)
        {
            string key = family + "|" + size + "|" + (int)style;
            Font f;
            if (!FontCache.TryGetValue(key, out f))
            {
                f = new Font(family, size, style, GraphicsUnit.Point);
                FontCache[key] = f;
            }
            return f;
        }

        private static string _ui, _uiSemi, _mono, _icons;
        private static string UiFamily => _ui ?? (_ui = Pick("Segoe UI", "Inter", "Arial"));
        private static string UiSemiFamily => _uiSemi ?? (_uiSemi = Pick("Segoe UI Semibold", "Inter SemiBold", "Segoe UI", "Inter"));
        private static string MonoFamily => _mono ?? (_mono = Pick("Cascadia Mono", "Cascadia Code", "Consolas", "DejaVu Sans Mono", "Courier New"));
        private static string IconFamily => _icons ?? (_icons = Pick("Segoe Fluent Icons", "Segoe MDL2 Assets"));

        /// <summary>True when a Windows icon font is available (Win10+).</summary>
        public static bool HasIconFont => IconFamily == "Segoe Fluent Icons" || IconFamily == "Segoe MDL2 Assets";

        public static Font Ui(float size) => Get(UiFamily, size, FontStyle.Regular);
        public static Font UiItalic(float size) => Get(UiFamily, size, FontStyle.Italic);

        /// <summary>Semibold UI font. Falls back to bold if no semibold face is installed.</summary>
        public static Font UiSemi(float size)
        {
            bool isSemiFace = UiSemiFamily.IndexOf("Semibold", StringComparison.OrdinalIgnoreCase) >= 0;
            return Get(UiSemiFamily, size, isSemiFace ? FontStyle.Regular : FontStyle.Bold);
        }

        public static Font UiBold(float size) => Get(UiFamily, size, FontStyle.Bold);
        public static Font Mono(float size) => Get(MonoFamily, size, FontStyle.Regular);
        public static Font MonoBold(float size) => Get(MonoFamily, size, FontStyle.Bold);
        public static Font Icons(float size) => Get(IconFamily, size, FontStyle.Regular);

        // Segoe MDL2 Assets / Segoe Fluent Icons code points (same in both fonts).
        public const string GlyphResources = ""; // PaymentCard
        public const string GlyphVitals = "";    // Heart
        public const string GlyphRecovery = "";  // Refresh
        public const string GlyphOffense = "";   // LightningBolt
        public const string GlyphDefense = "";   // Shield
        public const string GlyphMovement = "";  // Walk
        public const string GlyphTalents = "";   // FavoriteStar
        public const string GlyphMeta = "";      // AreaChart
        public const string GlyphImport = "";    // Import
        public const string GlyphExport = "";    // Export
        public const string GlyphFile = "";      // Document

        public static string GlyphForCategory(string category)
        {
            switch (category)
            {
                case "Resources": return GlyphResources;
                case "Vitals": return GlyphVitals;
                case "Recovery": return GlyphRecovery;
                case "Offense": return GlyphOffense;
                case "Defense": return GlyphDefense;
                case "Movement": return GlyphMovement;
                case "Talents": return GlyphTalents;
                default: return GlyphMeta;
            }
        }

        public static string DescriptionForCategory(string category)
        {
            switch (category)
            {
                case "Resources": return "Money, souls and keys.";
                case "Vitals": return "Health, spirit, stamina and armor pools.";
                case "Recovery": return "Regeneration and healing rates.";
                case "Offense": return "Attack power, elemental damage and damage multipliers.";
                case "Defense": return "Evasion, status immunities and protections.";
                case "Movement": return "Speed, run cost, sight and traversal.";
                case "Talents": return "Talent and item unlocks.";
                default: return "Counters, spawn rates and loot rates.";
            }
        }

        // ---- Drawing helpers ---------------------------------------------------
        public static GraphicsPath Round(RectangleF r, float radius)
        {
            var path = new GraphicsPath();
            float d = Math.Min(radius * 2f, Math.Min(r.Width, r.Height));
            if (d <= 0.5f)
            {
                path.AddRectangle(r);
                return path;
            }
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static void Smooth(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        }

        public static void FillRound(Graphics g, Color c, RectangleF r, float radius)
        {
            using (var path = Round(r, radius))
            using (var b = new SolidBrush(c))
            {
                g.FillPath(b, path);
            }
        }

        public static void StrokeRound(Graphics g, Color c, RectangleF r, float radius, float width = 1f, bool dashed = false)
        {
            using (var path = Round(r, radius))
            using (var p = new Pen(c, width))
            {
                if (dashed)
                {
                    p.DashStyle = DashStyle.Custom;
                    p.DashPattern = new[] { 4f, 3f };
                }
                g.DrawPath(p, path);
            }
        }

        /// <summary>Soft outer glow made of a few expanding translucent rounded rects.</summary>
        public static void Glow(Graphics g, Color c, RectangleF r, float radius, int strength, float spread)
        {
            const int steps = 6;
            for (int i = steps; i >= 1; i--)
            {
                float grow = spread * i / steps;
                int a = strength * (steps - i + 1) / (steps * 3);
                RectangleF rr = RectangleF.Inflate(r, grow, grow);
                rr.Offset(0, grow * 0.35f);
                FillRound(g, Alpha(c, a), rr, radius + grow);
            }
        }

        public static void DrawText(Graphics g, string text, Font font, Color color, Rectangle bounds, TextFormatFlags flags)
        {
            TextRenderer.DrawText(g, text, font, bounds, color,
                flags | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        }

        public static Size Measure(string text, Font font)
        {
            return TextRenderer.MeasureText(text, font, new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);
        }

        // ---- Win32 niceties (silently ignored where unsupported) ----------------
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hwnd, string appName, string idList);

        /// <summary>Dark title bar (Win10 1809+), tinted to the hero color on Windows 11.</summary>
        public static void ApplyDarkTitleBar(Form form)
        {
            try
            {
                int on = 1;
                if (DwmSetWindowAttribute(form.Handle, 20, ref on, 4) != 0)
                {
                    DwmSetWindowAttribute(form.Handle, 19, ref on, 4); // pre-20H1 attribute id
                }
                int caption = HeroTop.R | (HeroTop.G << 8) | (HeroTop.B << 16); // COLORREF is BGR
                DwmSetWindowAttribute(form.Handle, 35, ref caption, 4);         // DWMWA_CAPTION_COLOR (Win11)
                int text = Text.R | (Text.G << 8) | (Text.B << 16);
                DwmSetWindowAttribute(form.Handle, 36, ref text, 4);            // DWMWA_TEXT_COLOR (Win11)
            }
            catch
            {
                // Not on Windows 10/11 - keep the default caption.
            }
        }

        /// <summary>Dark scrollbars for a scrollable control (Win10 1809+).</summary>
        public static void ApplyDarkScrollbars(Control c)
        {
            try
            {
                SetWindowTheme(c.Handle, "DarkMode_Explorer", null);
            }
            catch
            {
                // Unsupported - default scrollbars are fine.
            }
        }
    }
}
