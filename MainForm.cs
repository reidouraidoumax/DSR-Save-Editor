using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RaksasiSaveEditor
{
    public partial class MainForm : Form
    {
        private RaksasiSave _save;
        private string _currentPath;
        private readonly Dictionary<string, FieldCard> _cards = new Dictionary<string, FieldCard>();
        private readonly Dictionary<string, CategoryPage> _pages = new Dictionary<string, CategoryPage>();

        public MainForm()
        {
            InitializeComponent();

            heroPanel.Logo = Properties.Resources.raksasi_logo;
            BuildPages();

            tabStrip.SetItems(RaksasiFields.CategoryOrder);
            tabStrip.SelectedIndexChanged += (s, e) => ShowPage(tabStrip.SelectedItem);
            emptyState.ImportClicked += btnImport_Click;

            KeyPreview = true;
            ShowPage(tabStrip.SelectedItem);
            UpdateChrome();
        }

        // ------------------------------------------------------------------
        // UI construction
        // ------------------------------------------------------------------

        /// <summary>One scrollable page per category, one card per known field.</summary>
        private void BuildPages()
        {
            contentHost.SuspendLayout();
            foreach (string category in RaksasiFields.CategoryOrder)
            {
                var page = new CategoryPage(category)
                {
                    Dock = DockStyle.Fill,
                    Visible = false
                };
                foreach (RaksasiField field in RaksasiFields.InCategory(category))
                {
                    var card = new FieldCard(field);
                    card.ValueChanged += Card_ValueChanged;
                    page.AddCard(card);
                    _cards[field.Key] = card;
                }
                page.RefreshSummary();
                _pages[category] = page;
                contentHost.Controls.Add(page);
            }
            contentHost.ResumeLayout(true);
        }

        private void ShowPage(string category)
        {
            if (category == null)
            {
                return;
            }

            contentHost.SuspendLayout();
            bool hasSave = _save != null;
            emptyState.Visible = !hasSave;
            foreach (var kv in _pages)
            {
                bool show = hasSave && kv.Key == category;
                kv.Value.Visible = show;
                if (show)
                {
                    kv.Value.BringToFront();
                    kv.Value.AutoScrollPosition = new Point(0, 0);
                    kv.Value.LayoutCards();
                }
            }
            contentHost.ResumeLayout(true);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Theme.ApplyDarkTitleBar(this);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // At high DPI the scaled default size can exceed the screen - fit it.
            Rectangle area = Screen.FromControl(this).WorkingArea;
            if (Width > area.Width || Height > area.Height)
            {
                Size = new Size(Math.Min(Width, area.Width), Math.Min(Height, area.Height));
                Location = new Point(area.X + (area.Width - Width) / 2, area.Y + (area.Height - Height) / 2);
            }
        }

        // ------------------------------------------------------------------
        // State -> chrome (badges, footer, hero, buttons)
        // ------------------------------------------------------------------

        private int PendingEditCount => _cards.Values.Count(c => c.HasPendingEdit);

        private void UpdateChrome()
        {
            btnExport.Enabled = _save != null;

            foreach (var kv in _pages)
            {
                int edits = kv.Value.Cards.Count(c => c.HasPendingEdit);
                tabStrip.SetBadge(kv.Key, edits);
                kv.Value.RefreshSummary();
            }
            footerBar.PendingEdits = PendingEditCount;
        }

        private void Card_ValueChanged(object sender, EventArgs e)
        {
            var card = (FieldCard)sender;
            CategoryPage page;
            if (_pages.TryGetValue(card.Field.Category, out page))
            {
                tabStrip.SetBadge(page.Category, page.Cards.Count(c => c.HasPendingEdit));
                page.RefreshSummary();
            }
            footerBar.PendingEdits = PendingEditCount;
        }

        private void UpdateHeroStats(ChecksumState checksum)
        {
            heroPanel.SaveFileName = _currentPath == null ? null : Path.GetFileName(_currentPath);
            heroPanel.Souls = FormatStat(_cards["CUR_SOUL"].OriginalValue);
            heroPanel.Kills = FormatStat(_cards["KILL_COUNT"].OriginalValue);
            heroPanel.Checksum = checksum;
        }

        private static string FormatStat(string raw)
        {
            long n;
            if (raw != null && long.TryParse(raw, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out n))
            {
                return n.ToString("N0", CultureInfo.CurrentCulture);
            }
            return raw;
        }

        /// <summary>Pushes the save's current attrs values into every card.</summary>
        private void BindCardsFromSave()
        {
            var attrs = _save.FindPlayerAttrsBlock();
            foreach (RaksasiField field in RaksasiFields.All)
            {
                _cards[field.Key].Bind(_save.GetAttr(field.Key, attrs));
            }
        }

        // ------------------------------------------------------------------
        // Import / export
        // ------------------------------------------------------------------

        private bool ConfirmDiscardEdits(string action)
        {
            int pending = PendingEditCount;
            if (pending == 0)
            {
                return true;
            }
            return MessageBox.Show(
                this,
                string.Format("You have {0} pending edit{1} that haven't been exported.\n\n{2} anyway?",
                    pending, pending == 1 ? "" : "s", action),
                "Unsaved edits",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) == DialogResult.Yes;
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            if (!ConfirmDiscardEdits("Import another save"))
            {
                return;
            }

            using (var ofd = new OpenFileDialog
            {
                Title = "Import Devil Slayer Raksasi save",
                Filter = "Raksasi save files (*.dat)|*.dat|All files (*.*)|*.*"
            })
            {
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    LoadSave(ofd.FileName);
                }
            }
        }

        /// <summary>Loads a save file into the editor. Returns false (and reports why) on failure.</summary>
        public bool LoadSave(string path)
        {
            try
            {
                var loaded = RaksasiSave.Load(path);
                bool checksumOk = loaded.VerifyChecksum();

                // Will throw if this isn't a recognizable player save - do this
                // before committing to _save so a bad file doesn't half-load.
                loaded.FindPlayerAttrsBlock();

                _save = loaded;
                _currentPath = path;
                BindCardsFromSave();

                UpdateHeroStats(checksumOk ? ChecksumState.Valid : ChecksumState.Mismatch);
                footerBar.StatusText = checksumOk
                    ? "Loaded " + Path.GetFileName(path) + "   ·   checksum OK on disk"
                    : "Loaded " + Path.GetFileName(path) + "   ·   checksum mismatch (harmless - recalculated on export)";

                UpdateChrome();
                ShowPage(tabStrip.SelectedItem);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "Couldn't read this file as a Devil Slayer Raksasi save:\n\n" + ex.Message,
                    "Import failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (_save == null)
            {
                MessageBox.Show(this, "Import a save first.", "Nothing to export",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Validate every field before touching any bytes, so a typo
            // doesn't leave the save half-edited.
            foreach (RaksasiField field in RaksasiFields.All)
            {
                FieldCard card = _cards[field.Key];
                if (!card.IsValid)
                {
                    tabStrip.SelectedIndex = RaksasiFields.CategoryOrder.IndexOf(field.Category);
                    card.FocusValue();
                    MessageBox.Show(
                        this,
                        string.Format(
                            "Invalid value for {0}: \"{1}\"\n\nUse a plain number, e.g. 123, -0.5, or 3703.1498881687 " +
                            "(no letters, commas, or scientific notation).",
                            field.Key, card.CurrentText),
                        "Invalid value",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
            }

            // Only fields whose value actually changed (or that are being
            // created) are written; everything else stays byte-for-byte as is.
            var toApply = RaksasiFields.All
                .Select(f => _cards[f.Key])
                .Where(c => c.HasPendingEdit)
                .Select(c => new KeyValuePair<string, string>(c.Key, c.CurrentText))
                .ToList();

            using (var sfd = new SaveFileDialog
            {
                Title = "Export Devil Slayer Raksasi save",
                Filter = "Raksasi save files (*.dat)|*.dat|All files (*.*)|*.*",
                FileName = string.IsNullOrEmpty(_currentPath) ? "slot1.dat" : Path.GetFileName(_currentPath)
            })
            {
                if (sfd.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    foreach (var kv in toApply)
                    {
                        _save.SetFieldEverywhere(kv.Key, kv.Value);
                    }

                    _save.SaveToFile(sfd.FileName); // FixChecksum() runs inside SaveToFile

                    // Reflect the freshly-written state (including newly
                    // created fields) back into the UI.
                    _currentPath = sfd.FileName;
                    BindCardsFromSave();
                    UpdateHeroStats(ChecksumState.Fixed);
                    footerBar.StatusText = string.Format("Exported {0}   ·   {1} field{2} written   ·   checksum recalculated",
                        Path.GetFileName(sfd.FileName), toApply.Count, toApply.Count == 1 ? "" : "s");
                    UpdateChrome();

                    MessageBox.Show(
                        this,
                        "Save exported successfully. The checksum was recalculated automatically.",
                        "Export complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        this,
                        "Failed to export save:\n\n" + ex.Message,
                        "Export failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        // ------------------------------------------------------------------
        // Keyboard shortcuts & closing
        // ------------------------------------------------------------------

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.O:
                    btnImport_Click(this, EventArgs.Empty);
                    return true;
                case Keys.Control | Keys.S:
                    if (btnExport.Enabled) btnExport_Click(this, EventArgs.Empty);
                    return true;
                case Keys.Control | Keys.Tab:
                    tabStrip.SelectedIndex = (tabStrip.SelectedIndex + 1) % RaksasiFields.CategoryOrder.Count;
                    return true;
                case Keys.Control | Keys.Shift | Keys.Tab:
                    tabStrip.SelectedIndex = (tabStrip.SelectedIndex + RaksasiFields.CategoryOrder.Count - 1) % RaksasiFields.CategoryOrder.Count;
                    return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !ConfirmDiscardEdits("Close the editor"))
            {
                e.Cancel = true;
            }
            base.OnFormClosing(e);
        }
    }
}
