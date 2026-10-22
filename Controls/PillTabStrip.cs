using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace RaksasiSaveEditor
{
    /// <summary>
    /// Row of rounded "pill" tabs with icons. The selected pill is filled
    /// turquoise with a glow; a small amber badge shows how many pending
    /// edits each category has.
    /// </summary>
    public class PillTabStrip : Control
    {
        private readonly List<string> _items = new List<string>();
        private readonly List<int> _badges = new List<int>();
        private readonly List<RectangleF> _rects = new List<RectangleF>();
        private int _selected;
        private int _hover = -1;
        private bool _showIcons = true;

        public event EventHandler SelectedIndexChanged;

        public PillTabStrip()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            BackColor = Theme.Bg;
            Cursor = Cursors.Hand;
            Size = new Size(800, 64);
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex
        {
            get { return _selected; }
            set
            {
                if (_items.Count == 0) return;
                int v = Math.Max(0, Math.Min(_items.Count - 1, value));
                if (v == _selected) return;
                _selected = v;
                Invalidate();
                EventHandler h = SelectedIndexChanged;
                if (h != null) h(this, EventArgs.Empty);
            }
        }

        [Browsable(false)]
        public string SelectedItem => _selected >= 0 && _selected < _items.Count ? _items[_selected] : null;

        public void SetItems(IEnumerable<string> items)
        {
            _items.Clear();
            _badges.Clear();
            foreach (string s in items)
            {
                _items.Add(s);
                _badges.Add(0);
            }
            _selected = 0;
            PerformLayout();
            Invalidate();
        }

        public void SetBadge(string item, int count)
        {
            int i = _items.IndexOf(item);
            if (i >= 0 && _badges[i] != count)
            {
                _badges[i] = count;
                PerformLayout();
                Invalidate();
            }
        }

        private Font LabelFont => Theme.UiSemi(10f);
        private Font IconFont => Theme.Icons(10.5f);
        private Font BadgeFont => Theme.UiBold(7.5f);

        private float PillWidth(int i, float padX, bool icons)
        {
            float w = Theme.Measure(_items[i], LabelFont).Width + padX * 2;
            if (icons && Theme.HasIconFont)
            {
                w += Theme.Measure(Theme.GlyphForCategory(_items[i]), IconFont).Width + Theme.SF(8);
            }
            if (_badges[i] > 0)
            {
                w += BadgeWidth(i) + Theme.SF(7);
            }
            return w;
        }

        private float BadgeWidth(int i)
        {
            return Math.Max(Theme.SF(18), Theme.Measure(_badges[i].ToString(), BadgeFont).Width + Theme.SF(10));
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            _rects.Clear();
            if (_items.Count == 0) return;

            float left = Theme.SF(32);
            float avail = Width - left - Theme.SF(32);
            float gap = Theme.SF(8);
            float padX = Theme.SF(16);

            // Shrink gracefully: drop icons first, then tighten padding.
            _showIcons = true;
            float total = TotalWidth(padX, gap, true);
            if (total > avail)
            {
                _showIcons = false;
                total = TotalWidth(padX, gap, false);
            }
            if (total > avail)
            {
                float labels = TotalWidth(0, gap, false);
                padX = Math.Max(Theme.SF(6), (avail - labels) / (_items.Count * 2));
            }

            float x = left;
            float h = Theme.SF(38);
            float y = Theme.SF(14);
            for (int i = 0; i < _items.Count; i++)
            {
                float w = PillWidth(i, padX, _showIcons);
                _rects.Add(new RectangleF(x, y, w, h));
                x += w + gap;
            }
        }

        private float TotalWidth(float padX, float gap, bool icons)
        {
            float t = 0;
            for (int i = 0; i < _items.Count; i++)
            {
                t += PillWidth(i, padX, icons);
            }
            return t + gap * (_items.Count - 1);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            Theme.Smooth(g);
            if (_rects.Count != _items.Count) PerformLayout();

            for (int i = 0; i < _rects.Count; i++)
            {
                RectangleF r = _rects[i];
                float radius = r.Height / 2f;
                bool sel = i == _selected;
                bool hov = i == _hover;

                Color fore;
                if (sel)
                {
                    Theme.Glow(g, Theme.Accent, r, radius, 120, Theme.SF(6));
                    Theme.FillRound(g, Theme.Accent, r, radius);
                    fore = Theme.AccentInk;
                }
                else
                {
                    Theme.FillRound(g, hov ? Theme.CardHover : Theme.Card, r, radius);
                    Theme.StrokeRound(g, hov ? Theme.Line2 : Theme.Line, r, radius);
                    fore = hov ? Theme.Text : Theme.Text2;
                }

                if (sel && Focused && ShowFocusCues)
                {
                    Theme.StrokeRound(g, Theme.Text, RectangleF.Inflate(r, 2, 2), radius + 2);
                }

                // Content: [icon] label [badge], centered in the pill.
                bool icons = _showIcons && Theme.HasIconFont;
                string glyph = Theme.GlyphForCategory(_items[i]);
                int iconW = icons ? Theme.Measure(glyph, IconFont).Width : 0;
                int iconGap = icons ? Theme.S(8) : 0;
                int labelW = Theme.Measure(_items[i], LabelFont).Width;
                float badgeW = _badges[i] > 0 ? BadgeWidth(i) : 0;
                float badgeGap = _badges[i] > 0 ? Theme.SF(7) : 0;
                float content = iconW + iconGap + labelW + badgeGap + badgeW;
                int cx = (int)(r.X + (r.Width - content) / 2f);
                Rectangle rr = Rectangle.Round(r);

                if (icons)
                {
                    Theme.DrawText(g, glyph, IconFont, fore, new Rectangle(cx, rr.Y + 1, iconW + 2, rr.Height),
                        TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                }
                Theme.DrawText(g, _items[i], LabelFont, fore, new Rectangle(cx + iconW + iconGap, rr.Y, labelW + 4, rr.Height),
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left);

                if (_badges[i] > 0)
                {
                    float bh = Theme.SF(18);
                    RectangleF b = new RectangleF(cx + iconW + iconGap + labelW + badgeGap, r.Y + (r.Height - bh) / 2f, badgeW, bh);
                    Theme.FillRound(g, sel ? Theme.AccentInk : Theme.Amber, b, bh / 2f);
                    Theme.DrawText(g, _badges[i].ToString(), BadgeFont, sel ? Theme.Amber : Theme.AccentInk, Rectangle.Round(b),
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            }
        }

        private int HitTest(Point p)
        {
            for (int i = 0; i < _rects.Count; i++)
            {
                if (_rects[i].Contains(p)) return i;
            }
            return -1;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            int h = HitTest(e.Location);
            if (h != _hover)
            {
                _hover = h;
                Cursor = h >= 0 ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hover = -1;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            int h = HitTest(e.Location);
            if (h >= 0 && e.Button == MouseButtons.Left)
            {
                SelectedIndex = h;
            }
            base.OnMouseDown(e);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.Left || keyData == Keys.Right) return true;
            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left) SelectedIndex = _selected - 1;
            else if (e.KeyCode == Keys.Right) SelectedIndex = _selected + 1;
            base.OnKeyDown(e);
        }

        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }
    }
}
