using System;
using System.Drawing;
using System.Windows.Forms;

namespace RaksasiSaveEditor
{
    /// <summary>Shown in place of the field pages until a save is imported.</summary>
    public class EmptyStatePanel : Control
    {
        private readonly GlowButton _button;

        public event EventHandler ImportClicked;

        public EmptyStatePanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Bg;

            _button = new GlowButton
            {
                Primary = true,
                Text = "Import Save",
                Glyph = Theme.GlyphImport,
                Size = new Size(Theme.S(190), Theme.S(54))
            };
            _button.Click += (s, e) =>
            {
                EventHandler h = ImportClicked;
                if (h != null) h(this, EventArgs.Empty);
            };
            Controls.Add(_button);
        }

        private int BlockTop => Math.Max(Theme.S(20), Height / 2 - Theme.S(150));

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            _button.Location = new Point((Width - _button.Width) / 2, BlockTop + Theme.S(196));
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            Theme.Smooth(g);

            int top = BlockTop;
            int cx = Width / 2;

            // Lantern-like glowing ring with a document icon.
            float d = Theme.SF(84);
            RectangleF ring = new RectangleF(cx - d / 2, top, d, d);
            for (int i = 5; i >= 1; i--)
            {
                float grow = Theme.SF(5) * i;
                using (var b = new SolidBrush(Theme.Alpha(Theme.Accent, 7 * (6 - i))))
                {
                    g.FillEllipse(b, RectangleF.Inflate(ring, grow, grow));
                }
            }
            using (var b = new SolidBrush(Theme.Card2))
            {
                g.FillEllipse(b, ring);
            }
            using (var p = new Pen(Theme.Alpha(Theme.Accent, 150), 1.5f))
            {
                g.DrawEllipse(p, ring);
            }
            if (Theme.HasIconFont)
            {
                Theme.DrawText(g, Theme.GlyphFile, Theme.Icons(22f), Theme.Accent, Rectangle.Round(ring),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            Font titleFont = Theme.UiSemi(17f);
            Theme.DrawText(g, "No save loaded", titleFont, Theme.Text,
                new Rectangle(0, top + Theme.S(108), Width, Theme.S(34)),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            Theme.DrawText(g, "Import a Devil Slayer Raksasi .dat save (PS4 or PC) to start editing.\n" +
                              "Fields missing from your save can be created by typing a number into them.",
                Theme.Ui(10f), Theme.Text2,
                new Rectangle(Theme.S(20), top + Theme.S(146), Width - Theme.S(40), Theme.S(44)),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.WordBreak);
        }
    }
}
