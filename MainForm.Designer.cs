namespace RaksasiSaveEditor
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        // Layout shell only. The 70 field cards are generated at runtime from
        // RaksasiFields.All (see MainForm.BuildPages), so adding a field to
        // that table is all it takes to show it in the UI.
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            heroPanel = new HeroPanel();
            btnImport = new GlowButton();
            btnExport = new GlowButton();
            tabStrip = new PillTabStrip();
            contentHost = new System.Windows.Forms.Panel();
            emptyState = new EmptyStatePanel();
            footerBar = new FooterBar();
            heroPanel.SuspendLayout();
            contentHost.SuspendLayout();
            SuspendLayout();
            //
            // heroPanel
            //
            heroPanel.BackColor = System.Drawing.Color.FromArgb(10, 8, 18);
            heroPanel.Controls.Add(btnImport);
            heroPanel.Controls.Add(btnExport);
            heroPanel.Dock = System.Windows.Forms.DockStyle.Top;
            heroPanel.Location = new System.Drawing.Point(0, 0);
            heroPanel.Name = "heroPanel";
            heroPanel.Size = new System.Drawing.Size(1280, 150);
            heroPanel.TabIndex = 0;
            //
            // btnImport
            //
            btnImport.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnImport.BackColor = System.Drawing.Color.Transparent;
            btnImport.Glyph = "";
            btnImport.Location = new System.Drawing.Point(884, 48);
            btnImport.Name = "btnImport";
            btnImport.Size = new System.Drawing.Size(180, 54);
            btnImport.TabIndex = 0;
            btnImport.Text = "Import Save";
            btnImport.Click += btnImport_Click;
            //
            // btnExport
            //
            btnExport.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnExport.BackColor = System.Drawing.Color.Transparent;
            btnExport.Glyph = "";
            btnExport.Location = new System.Drawing.Point(1072, 48);
            btnExport.Name = "btnExport";
            btnExport.Primary = true;
            btnExport.Size = new System.Drawing.Size(182, 54);
            btnExport.TabIndex = 1;
            btnExport.Text = "Export Save";
            btnExport.Click += btnExport_Click;
            //
            // tabStrip
            //
            tabStrip.BackColor = System.Drawing.Color.FromArgb(10, 8, 18);
            tabStrip.Dock = System.Windows.Forms.DockStyle.Top;
            tabStrip.Location = new System.Drawing.Point(0, 150);
            tabStrip.Name = "tabStrip";
            tabStrip.Size = new System.Drawing.Size(1280, 66);
            tabStrip.TabIndex = 1;
            //
            // contentHost
            //
            contentHost.BackColor = System.Drawing.Color.FromArgb(10, 8, 18);
            contentHost.Controls.Add(emptyState);
            contentHost.Dock = System.Windows.Forms.DockStyle.Fill;
            contentHost.Location = new System.Drawing.Point(0, 216);
            contentHost.Name = "contentHost";
            contentHost.Padding = new System.Windows.Forms.Padding(32, 10, 24, 0);
            contentHost.Size = new System.Drawing.Size(1280, 552);
            contentHost.TabIndex = 2;
            //
            // emptyState
            //
            emptyState.BackColor = System.Drawing.Color.FromArgb(10, 8, 18);
            emptyState.Dock = System.Windows.Forms.DockStyle.Fill;
            emptyState.Location = new System.Drawing.Point(32, 10);
            emptyState.Name = "emptyState";
            emptyState.Size = new System.Drawing.Size(1224, 542);
            emptyState.TabIndex = 0;
            //
            // footerBar
            //
            footerBar.BackColor = System.Drawing.Color.FromArgb(10, 8, 18);
            footerBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            footerBar.Location = new System.Drawing.Point(0, 768);
            footerBar.Name = "footerBar";
            footerBar.Size = new System.Drawing.Size(1280, 52);
            footerBar.TabIndex = 3;
            //
            // MainForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.FromArgb(10, 8, 18);
            ClientSize = new System.Drawing.Size(1280, 820);
            Controls.Add(contentHost);
            Controls.Add(tabStrip);
            Controls.Add(heroPanel);
            Controls.Add(footerBar);
            Font = new System.Drawing.Font("Segoe UI", 9F);
            ForeColor = System.Drawing.Color.FromArgb(241, 238, 248);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MinimumSize = new System.Drawing.Size(1060, 720);
            Name = "MainForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Devil Slayer Raksasi Save Editor PS4";
            heroPanel.ResumeLayout(false);
            contentHost.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private HeroPanel heroPanel;
        private GlowButton btnImport;
        private GlowButton btnExport;
        private PillTabStrip tabStrip;
        private System.Windows.Forms.Panel contentHost;
        private EmptyStatePanel emptyState;
        private FooterBar footerBar;
    }
}
