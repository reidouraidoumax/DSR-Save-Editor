using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace RaksasiSaveEditor
{
    /// <summary>
    /// Bottom bar: pending-edit pill, a legend for the card states, and a
    /// status message on the right.
    /// </summary>
    public class FooterBar : Control
    {
        private int _pending;
        private string _status = "Checksum is fixed automatically on export";

        public FooterBar()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Bg;
            Size = new Size(800, 52);
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int PendingEdits
        {
            get { return _pending; }
            set { _pending = value; Invalidate(); }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string StatusText
        {
            get { return _status; }
            set { _status = value ?? ""; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            Theme.Smooth(g);

            int margin = Theme.S(32);
            using (var line = new Pen(Theme.Line))
            {
                g.DrawLine(line, margin, 0, Width - margin, 0);
            }

            int midY = Height / 2;
            int x = margin;

            // Pending pill
            Font pillFont = Theme.UiSemi(9f);
            string pill = _pending > 0
                ? _pending + (_pending == 1 ? " pending edit" : " pending edits")
                : "No pending edits";
            Color pc = _pending > 0 ? Theme.Amber : Theme.Text3;
            Size ps = Theme.Measure(pill, pillFont);
            int dot = Theme.S(7);
            RectangleF pr = new RectangleF(x, midY - Theme.SF(13), ps.Width + dot + Theme.S(30), Theme.SF(26));
            Theme.FillRound(g, Theme.Alpha(pc, 30), pr, pr.Height / 2f);
            using (var b = new SolidBrush(pc))
            {
                g.FillEllipse(b, pr.X + Theme.S(12), midY - dot / 2f, dot, dot);
            }
            Theme.DrawText(g, pill, pillFont, pc, new Rectangle((int)pr.X + Theme.S(12) + dot + Theme.S(7), (int)pr.Y, ps.Width + 4, (int)pr.Height),
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            x = (int)pr.Right + Theme.S(22);

            // Status (right-aligned), measured first so the legend can yield to it.
            Font statusFont = Theme.Ui(9f);
            Size ss = Theme.Measure(_status, statusFont);
            int statusLeft = Width - margin - ss.Width;
            Theme.DrawText(g, _status, statusFont, Theme.Text3, new Rectangle(statusLeft, 0, ss.Width + 2, Height),
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left);

            // Legend
            x = Legend(g, x, statusLeft, midY, "In save", Theme.Line2, false, false);
            x = Legend(g, x, statusLeft, midY, "Not in save - type a number to create", Theme.Text3, true, false);
            x = Legend(g, x, statusLeft, midY, "Edited", Theme.Amber, false, true);
            Legend(g, x, statusLeft, midY, "Will be created", Theme.Accent, false, true);
        }

        private int Legend(Graphics g, int x, int limit, int midY, string text, Color c, bool dashed, bool tinted)
        {
            if (x < 0) return -1;
            Font f = Theme.Ui(9f);
            Size s = Theme.Measure(text, f);
            int sw = Theme.S(18), sh = Theme.S(11);
            int total = sw + Theme.S(7) + s.Width;
            if (x + total > limit - Theme.S(20)) return -1;

            RectangleF r = new RectangleF(x, midY - sh / 2f, sw, sh);
            if (tinted) Theme.FillRound(g, Theme.Alpha(c, 50), r, Theme.SF(3));
            Theme.StrokeRound(g, c, r, Theme.SF(3), 1f, dashed);
            Theme.DrawText(g, text, f, Theme.Text2, new Rectangle(x + sw + Theme.S(7), 0, s.Width + 2, Height),
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            return x + total + Theme.S(20);
        }
    }
}
