using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace RaksasiSaveEditor
{
    /// <summary>
    /// One category's scrollable page: a title header followed by a
    /// responsive grid of <see cref="FieldCard"/>s (1-4 columns depending on
    /// the window width).
    /// </summary>
    public class CategoryPage : Panel
    {
        private readonly string _category;
        private readonly List<FieldCard> _cards = new List<FieldCard>();
        private readonly Header _header;

        public CategoryPage(string category)
        {
            _category = category;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            BackColor = Theme.Bg;
            // Vertical scrolling only: the grid always fits the width.
            AutoScroll = false;
            HorizontalScroll.Maximum = 0;
            HorizontalScroll.Visible = false;
            HorizontalScroll.Enabled = false;
            AutoScroll = true;
            _header = new Header(category);
            Controls.Add(_header);
        }

        public string Category => _category;
        public IList<FieldCard> Cards => _cards;

        public void AddCard(FieldCard card)
        {
            _cards.Add(card);
            Controls.Add(card);
        }

        /// <summary>Refreshes the "N fields - X in save - Y not set" summary.</summary>
        public void RefreshSummary()
        {
            int inSave = 0, notSet = 0, edited = 0;
            foreach (FieldCard c in _cards)
            {
                if (c.OriginalValue != null) inSave++; else notSet++;
                if (c.HasPendingEdit) edited++;
            }
            string s = _cards.Count + " fields   ·   " + inSave + " in save   ·   " + notSet + " not set";
            if (edited > 0) s += "   ·   " + edited + " edited";
            _header.Summary = s;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Theme.ApplyDarkScrollbars(this);
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            LayoutCards();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible) LayoutCards();
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            LayoutCards(); // scrollbars appearing/disappearing change the client width
        }

        private bool _laying;

        public void LayoutCards()
        {
            if (_cards.Count == 0 || _laying) return;
            _laying = true;
            SuspendLayout();
            try
            {
                int gap = Theme.S(14);
                int minCard = Theme.S(300);
                int cardH = _cards[0].Height;
                int headerH = Theme.S(64);
                int sbw = SystemInformation.VerticalScrollBarWidth;

                // Outer size = the area with no scrollbars at all, so the
                // result never flip-flops as scrollbars appear/disappear.
                int baseW = Width;

                int cols = Math.Max(1, Math.Min(4, (baseW + gap) / (minCard + gap)));
                int rows = (_cards.Count + cols - 1) / cols;
                int contentH = headerH + rows * cardH + (rows - 1) * gap + gap;
                int w = baseW - 1;
                if (contentH > Height)
                {
                    w = baseW - sbw - Theme.S(8);
                    cols = Math.Max(1, Math.Min(4, (w + gap) / (minCard + gap)));
                    rows = (_cards.Count + cols - 1) / cols;
                    contentH = headerH + rows * cardH + (rows - 1) * gap + gap;
                }

                int cardW = (w - gap * (cols - 1)) / cols;
                Point o = AutoScrollPosition;
                _header.SetBounds(o.X, o.Y, w, headerH);
                for (int i = 0; i < _cards.Count; i++)
                {
                    int r = i / cols;
                    int c = i % cols;
                    _cards[i].SetBounds(o.X + c * (cardW + gap), o.Y + headerH + r * (cardH + gap), cardW, cardH);
                }
                AutoScrollMinSize = new Size(0, contentH);
            }
            finally
            {
                ResumeLayout(false);
                _laying = false;
            }
        }

        /// <summary>Category title, description and field summary.</summary>
        private class Header : Control
        {
            private readonly string _title;
            private string _summary = "";

            public Header(string title)
            {
                _title = title;
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                BackColor = Theme.Bg;
            }

            [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public string Summary
            {
                get { return _summary; }
                set { _summary = value ?? ""; Invalidate(); }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.Clear(BackColor);
                Theme.Smooth(g);

                Font titleFont = Theme.UiSemi(16f);
                Size ts = Theme.Measure(_title, titleFont);
                int top = Theme.S(10);
                Theme.DrawText(g, _title, titleFont, Theme.Text, new Rectangle(0, top, ts.Width + 4, ts.Height), TextFormatFlags.Left);

                Font descFont = Theme.Ui(9.5f);
                string desc = Theme.DescriptionForCategory(_title);
                Theme.DrawText(g, desc, descFont, Theme.Text2,
                    new Rectangle(ts.Width + Theme.S(14), top, Width, ts.Height),
                    TextFormatFlags.Left | TextFormatFlags.Bottom);

                Font sumFont = Theme.Ui(9f);
                Size ss = Theme.Measure(_summary, sumFont);
                int descRight = ts.Width + Theme.S(14) + Theme.Measure(desc, descFont).Width;
                if (Width - ss.Width > descRight + Theme.S(24))
                {
                    Theme.DrawText(g, _summary, sumFont, Theme.Text3,
                        new Rectangle(Width - ss.Width - 2, top, ss.Width + 2, ts.Height),
                        TextFormatFlags.Left | TextFormatFlags.Bottom);
                }
            }
        }
    }
}
