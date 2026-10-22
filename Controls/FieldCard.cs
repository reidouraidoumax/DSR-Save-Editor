using System;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RaksasiSaveEditor
{
    /// <summary>Shared text/validation rules for field values.</summary>
    internal static class FieldText
    {
        // Only accepts a plain Lua number literal: optional leading '-', digits,
        // optional '.digits'. No scientific notation, no thousands separators.
        public static readonly Regex NumberPattern = new Regex(@"^-?[0-9]+(\.[0-9]+)?$");

        /// <summary>
        /// Same wording as the original editor's example labels, e.g.
        /// "Example: 254  (Dungeon keys)  [whole number]". Former 0/1 toggle
        /// fields are now plain whole numbers, since they can go above 1.
        /// </summary>
        public static string Example(RaksasiField f)
        {
            string kind = f.Type == FieldType.Float ? "decimal number" : "whole number";
            string note = string.IsNullOrEmpty(f.Note) ? "" : "  (" + f.Note + ")";
            return "Example: " + f.Example + note + "  [" + kind + "]";
        }
    }

    public enum FieldState
    {
        NoSave,     // nothing loaded yet
        InSave,     // present in the save, unchanged
        NotSet,     // missing from the save, still blank
        Edited,     // present in the save, value changed
        Create,     // missing from the save, user typed a value -> will be created
        Invalid     // not a plain number
    }

    /// <summary>
    /// One editable field: original key name, the example line under it,
    /// and a value box with -/+ steppers. Blank box on a field the save
    /// doesn't have = "not set"; typing a number there creates the field on
    /// export. Blank box on a field the save DOES have = left untouched.
    /// </summary>
    public class FieldCard : Control
    {
        private readonly TextBox _box;
        private readonly RaksasiField _field;
        private string _original;
        private bool _loaded;
        private bool _hover;
        private int _hoverButton; // -1 minus, +1 plus, 0 none
        private bool _boxFocused;
        private bool _suppress;

        public event EventHandler ValueChanged;

        public FieldCard(RaksasiField field)
        {
            _field = field;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Bg;

            _box = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = Theme.InputBg,
                ForeColor = Theme.Text,
                Font = Theme.Mono(12.5f),
                TextAlign = HorizontalAlignment.Center,
                MaxLength = 40
            };
            _box.TextChanged += (s, e) => OnBoxChanged();
            _box.KeyDown += Box_KeyDown;
            _box.KeyPress += Box_KeyPress;
            _box.GotFocus += (s, e) => { _boxFocused = true; Invalidate(); };
            _box.LostFocus += (s, e) => { _boxFocused = false; Invalidate(); };
            _box.MouseEnter += (s, e) => { _hover = true; Invalidate(); };
            _box.MouseLeave += (s, e) => { _hover = ClientRectangle.Contains(PointToClient(Cursor.Position)); Invalidate(); };
            Controls.Add(_box);
            Height = Theme.S(126);
            UpdatePlaceholder();
        }

        public RaksasiField Field => _field;
        public string Key => _field.Key;

        /// <summary>Value as it is in the loaded save (null = field not in save).</summary>
        public string OriginalValue => _original;

        public string CurrentText => _box.Text.Trim();

        public bool IsValid => CurrentText.Length == 0 || FieldText.NumberPattern.IsMatch(CurrentText);

        /// <summary>True when export should write this field.</summary>
        public bool HasPendingEdit =>
            _loaded && CurrentText.Length > 0 && IsValid && !string.Equals(CurrentText, _original ?? "", StringComparison.Ordinal);

        public FieldState State
        {
            get
            {
                if (!_loaded) return FieldState.NoSave;
                if (!IsValid) return FieldState.Invalid;
                if (HasPendingEdit) return _original == null ? FieldState.Create : FieldState.Edited;
                return _original == null ? FieldState.NotSet : FieldState.InSave;
            }
        }

        /// <summary>Loads the value from a save (null = not present in the save).</summary>
        public void Bind(string originalValue)
        {
            _original = originalValue;
            _loaded = true;
            _suppress = true;
            _box.Text = originalValue ?? "";
            _suppress = false;
            UpdatePlaceholder();
            Invalidate();
        }

        public void FocusValue()
        {
            _box.Focus();
            _box.SelectAll();
        }

        private void UpdatePlaceholder()
        {
#if !MONO_CHECK
            _box.PlaceholderText = _original == null
                ? "not set - type to create"
                : "blank - left unchanged";
#endif
        }

        private void OnBoxChanged()
        {
            Invalidate();
            if (_suppress) return;
            EventHandler h = ValueChanged;
            if (h != null) h(this, EventArgs.Empty);
        }

        // ---- Stepping ----------------------------------------------------------
        private void Step(int direction)
        {
            string t = CurrentText;
            decimal v = 0m;
            if (t.Length > 0 && !decimal.TryParse(t, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture, out v))
            {
                return; // not a number yet - leave it for the user to fix
            }
            decimal step = _field.Type == FieldType.Float ? 0.1m : 1m;
            try
            {
                v += direction * step;
            }
            catch (OverflowException)
            {
                return;
            }
            _box.Text = v.ToString(CultureInfo.InvariantCulture);
            _box.SelectionStart = _box.Text.Length;
            _box.Focus();
        }

        private void Box_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                Step(e.KeyCode == Keys.Up ? 1 : -1);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void Box_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            if (char.IsControl(c) || char.IsDigit(c) || c == '-' || c == '.')
            {
                return;
            }
            e.Handled = true; // block letters, commas, spaces...
        }

        // ---- Layout & painting ---------------------------------------------------
        private int Pad => Theme.S(16);
        private Rectangle InputRect => new Rectangle(Pad, Theme.S(68), Width - Pad * 2, Theme.S(40));
        private int ButtonW => Theme.S(40);
        private Rectangle MinusRect { get { Rectangle r = InputRect; return new Rectangle(r.X, r.Y, ButtonW, r.Height); } }
        private Rectangle PlusRect { get { Rectangle r = InputRect; return new Rectangle(r.Right - ButtonW, r.Y, ButtonW, r.Height); } }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            if (_box == null) return;
            Rectangle r = InputRect;
            int h = _box.PreferredHeight;
            _box.SetBounds(r.X + ButtonW + Theme.S(6), r.Y + (r.Height - h) / 2 + 1, Math.Max(10, r.Width - ButtonW * 2 - Theme.S(12)), h);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            PerformLayout();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            Theme.Smooth(g);

            FieldState state = State;
            RectangleF card = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            float radius = Theme.SF(14);

            Color border;
            bool dashed = false;
            switch (state)
            {
                case FieldState.Edited: border = Theme.Alpha(Theme.Amber, 140); break;
                case FieldState.Create: border = Theme.Alpha(Theme.Accent, 140); break;
                case FieldState.Invalid: border = Theme.Alpha(Theme.Ember, 200); break;
                case FieldState.NotSet: border = Theme.Line2; dashed = true; break;
                default: border = _hover ? Theme.Line2 : Theme.Line; break;
            }

            Theme.FillRound(g, _hover ? Theme.CardHover : Theme.Card, card, radius);
            Theme.StrokeRound(g, border, card, radius, 1f, dashed);

            // --- Header: original key name + status chip -----------------------
            string chip;
            Color chipColor;
            switch (state)
            {
                case FieldState.InSave: chip = "IN SAVE"; chipColor = Theme.Text3; break;
                case FieldState.NotSet: chip = "NOT SET"; chipColor = Theme.Text3; break;
                case FieldState.Edited: chip = "EDITED"; chipColor = Theme.Amber; break;
                case FieldState.Create: chip = "WILL CREATE"; chipColor = Theme.Accent; break;
                case FieldState.Invalid: chip = "INVALID"; chipColor = Theme.Ember; break;
                default: chip = ""; chipColor = Theme.Text3; break;
            }

            Font chipFont = Theme.UiBold(7f);
            int chipRight = Width - Pad;
            int chipLeft = chipRight;
            int headerTop = Theme.S(14);
            int headerH = Theme.S(22);
            if (chip.Length > 0)
            {
                Size cs = Theme.Measure(chip, chipFont);
                int cw = cs.Width + Theme.S(14);
                int ch = Theme.S(18);
                chipLeft = chipRight - cw;
                RectangleF cr = new RectangleF(chipLeft, headerTop + (headerH - ch) / 2f, cw, ch);
                Theme.FillRound(g, Theme.Alpha(chipColor, 28), cr, Theme.SF(5));
                Theme.StrokeRound(g, Theme.Alpha(chipColor, 110), cr, Theme.SF(5), 1f, state == FieldState.NotSet);
                Theme.DrawText(g, chip, chipFont, chipColor, Rectangle.Round(cr),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            Theme.DrawText(g, _field.Key, Theme.UiSemi(11f), Theme.Text,
                new Rectangle(Pad, headerTop, Math.Max(10, chipLeft - Pad - Theme.S(8)), headerH),
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);

            // --- Example line (original wording), plus "was X" when edited ------
            int exTop = Theme.S(40);
            int exH = Theme.S(18);
            int exRight = Width - Pad;
            if (state == FieldState.Edited)
            {
                string was = "was " + _original;
                Font wasFont = Theme.Mono(8.5f);
                Size ws = Theme.Measure(was, wasFont);
                Theme.DrawText(g, was, wasFont, Theme.Amber, new Rectangle(exRight - ws.Width, exTop, ws.Width + 2, exH),
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                exRight -= ws.Width + Theme.S(10);
            }
            Theme.DrawText(g, FieldText.Example(_field), Theme.Ui(8.75f), Theme.AccentText,
                new Rectangle(Pad, exTop, Math.Max(10, exRight - Pad), exH),
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);

            // --- Value box with -/+ ---------------------------------------------
            Rectangle ir = InputRect;
            float ir8 = Theme.SF(10);
            Color inputBorder = _boxFocused ? Theme.Accent
                : state == FieldState.Invalid ? Theme.Ember
                : state == FieldState.Edited ? Theme.Alpha(Theme.Amber, 170)
                : state == FieldState.Create ? Theme.Alpha(Theme.Accent, 170)
                : Theme.Line;

            if (_boxFocused)
            {
                Theme.FillRound(g, Theme.Alpha(Theme.Accent, 34), RectangleF.Inflate(ir, Theme.SF(3), Theme.SF(3)), ir8 + Theme.SF(3));
            }
            Theme.FillRound(g, Theme.InputBg, ir, ir8);
            Theme.StrokeRound(g, inputBorder, ir, ir8, 1f, state == FieldState.NotSet && !_boxFocused);

            using (var sep = new Pen(Theme.Line))
            {
                g.DrawLine(sep, MinusRect.Right, ir.Y + Theme.S(8), MinusRect.Right, ir.Bottom - Theme.S(8));
                g.DrawLine(sep, PlusRect.Left, ir.Y + Theme.S(8), PlusRect.Left, ir.Bottom - Theme.S(8));
            }

            Font stepFont = Theme.Ui(13f);
            Theme.DrawText(g, "−", stepFont, _hoverButton == -1 ? Theme.Accent : Theme.Text3, MinusRect,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            Theme.DrawText(g, "+", stepFont, _hoverButton == 1 ? Theme.Accent : Theme.Text3, PlusRect,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        // ---- Mouse -----------------------------------------------------------------
        protected override void OnMouseMove(MouseEventArgs e)
        {
            int hb = MinusRect.Contains(e.Location) ? -1 : PlusRect.Contains(e.Location) ? 1 : 0;
            if (hb != _hoverButton || !_hover)
            {
                _hoverButton = hb;
                _hover = true;
                Cursor = hb != 0 ? Cursors.Hand : InputRect.Contains(e.Location) ? Cursors.IBeam : Cursors.Default;
                Invalidate();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            // Leaving the card for its own textbox still counts as hovering.
            Point p = PointToClient(Cursor.Position);
            _hover = ClientRectangle.Contains(p);
            _hoverButton = 0;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _loaded)
            {
                if (MinusRect.Contains(e.Location)) Step(-1);
                else if (PlusRect.Contains(e.Location)) Step(1);
                else _box.Focus();
            }
            base.OnMouseDown(e);
        }
    }
}
