using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RaksasiSaveEditor
{
    /// <summary>
    /// Rounded button. Primary = turquoise gradient with a soft glow;
    /// secondary = dark glass with a hairline border. The control's bounds
    /// include a few pixels of room for the glow, and its background is
    /// transparent so it sits cleanly on the painted hero banner.
    /// </summary>
    public class GlowButton : Control
    {
        private bool _primary;
        private string _glyph = "";
        private bool _hover;
        private bool _pressed;

        public GlowButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor | ControlStyles.Selectable, true);
            BackColor = Color.Transparent;
            ForeColor = Theme.Text;
            Cursor = Cursors.Hand;
            Size = new Size(170, 52);
        }

        [Category("Appearance"), DefaultValue(false)]
        public bool Primary
        {
            get { return _primary; }
            set { _primary = value; Invalidate(); }
        }

        /// <summary>Segoe MDL2 / Fluent icon code point drawn before the text.</summary>
        [Category("Appearance"), DefaultValue("")]
        public string Glyph
        {
            get { return _glyph; }
            set { _glyph = value ?? ""; Invalidate(); }
        }

        protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); Invalidate(); }
        protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); Invalidate(); }
        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { _pressed = true; Invalidate(); }
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _pressed = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                OnClick(EventArgs.Empty);
            }
            base.OnKeyUp(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Theme.Smooth(g);

            float pad = Theme.SF(6);
            RectangleF r = new RectangleF(pad, pad * 0.6f, Width - pad * 2 - 1, Height - pad * 2 - 1);
            float radius = Theme.SF(12);
            if (_pressed)
            {
                r.Offset(0, 1);
            }

            if (_primary)
            {
                if (Enabled)
                {
                    Theme.Glow(g, Theme.Accent, r, radius, _hover ? 150 : 110, Theme.SF(_hover ? 7 : 5));
                }
                Color top = Enabled ? (_hover ? Theme.Mix(Theme.AccentHi, Color.White, 0.15f) : Theme.AccentHi) : Theme.Line2;
                Color bottom = Enabled ? (_hover ? Theme.Accent : Theme.AccentLo) : Theme.Line;
                using (var path = Theme.Round(r, radius))
                using (var brush = new LinearGradientBrush(new RectangleF(r.X, r.Y - 1, r.Width, r.Height + 2), top, bottom, 90f))
                {
                    g.FillPath(brush, path);
                }
                if (Enabled)
                {
                    using (var pen = new Pen(Theme.Alpha(Color.White, 90), 1f))
                    {
                        g.DrawLine(pen, r.X + radius, r.Y + 1, r.Right - radius, r.Y + 1); // top highlight
                    }
                }
            }
            else
            {
                Theme.FillRound(g, Theme.Alpha(_hover ? Theme.CardHover : Theme.Card2, Enabled ? 230 : 140), r, radius);
                Theme.StrokeRound(g, _hover && Enabled ? Theme.Line2 : Theme.Line, r, radius);
            }

            if (Focused && ShowFocusCues)
            {
                Theme.StrokeRound(g, Theme.Alpha(Theme.Accent, 160), RectangleF.Inflate(r, 2, 2), radius + 2);
            }

            Color fore = _primary
                ? (Enabled ? Theme.AccentInk : Theme.Text3)
                : (Enabled ? Theme.Text : Theme.Text3);

            Font font = Theme.UiSemi(10.5f);
            Size textSize = Theme.Measure(Text, font);
            bool showGlyph = _glyph.Length > 0 && Theme.HasIconFont;
            Font iconFont = Theme.Icons(11f);
            int iconW = showGlyph ? Theme.Measure(_glyph, iconFont).Width : 0;
            int gap = showGlyph ? Theme.S(9) : 0;
            int total = iconW + gap + textSize.Width;
            int x = (int)(r.X + (r.Width - total) / 2f);
            Rectangle rr = Rectangle.Round(r);

            if (showGlyph)
            {
                Theme.DrawText(g, _glyph, iconFont, fore, new Rectangle(x, rr.Y, iconW + 2, rr.Height),
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            }
            Theme.DrawText(g, Text, font, fore, new Rectangle(x + iconW + gap, rr.Y, textSize.Width + 4, rr.Height),
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
        }
    }
}
