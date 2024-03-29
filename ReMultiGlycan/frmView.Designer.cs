﻿namespace COL.ReMultiGlycan
{
    partial class frmView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
			System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
			System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
			System.Windows.Forms.DataVisualization.Charting.Title title1 = new System.Windows.Forms.DataVisualization.Charting.Title();
			this.btnLoad = new System.Windows.Forms.Button();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.chkGetAbundance = new System.Windows.Forms.CheckBox();
			this.chkMergeCharge = new System.Windows.Forms.CheckBox();
			this.btnSaveWholeProfile = new System.Windows.Forms.Button();
			this.rdoSingle = new System.Windows.Forms.RadioButton();
			this.rdoFullLC = new System.Windows.Forms.RadioButton();
			this.btnSaveAll = new System.Windows.Forms.Button();
			this.cboGlycan = new System.Windows.Forms.ComboBox();
			this.splitContainer3 = new System.Windows.Forms.SplitContainer();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.eluctionViewer1 = new COL.ElutionViewer.EluctionViewer();
			this.cht = new System.Windows.Forms.DataVisualization.Charting.Chart();
			this.splitContainer4 = new System.Windows.Forms.SplitContainer();
			this.dgvPeakList = new System.Windows.Forms.DataGridView();
			this.ctxMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.mergeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.splitContainer5 = new System.Windows.Forms.SplitContainer();
			this.chkAdductPeak = new System.Windows.Forms.CheckBox();
			this.btnUpdate = new System.Windows.Forms.Button();
			this.chkboxlstPeak = new System.Windows.Forms.CheckedListBox();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
			this.splitContainer3.Panel1.SuspendLayout();
			this.splitContainer3.Panel2.SuspendLayout();
			this.splitContainer3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.cht)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
			this.splitContainer4.Panel1.SuspendLayout();
			this.splitContainer4.Panel2.SuspendLayout();
			this.splitContainer4.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvPeakList)).BeginInit();
			this.ctxMenuStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).BeginInit();
			this.splitContainer5.Panel1.SuspendLayout();
			this.splitContainer5.Panel2.SuspendLayout();
			this.splitContainer5.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnLoad
			// 
			this.btnLoad.Location = new System.Drawing.Point(9, 12);
			this.btnLoad.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
			this.btnLoad.Name = "btnLoad";
			this.btnLoad.Size = new System.Drawing.Size(303, 58);
			this.btnLoad.TabIndex = 0;
			this.btnLoad.Text = "Load Full Result";
			this.btnLoad.UseVisualStyleBackColor = true;
			this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Margin = new System.Windows.Forms.Padding(0);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.chkGetAbundance);
			this.splitContainer1.Panel1.Controls.Add(this.chkMergeCharge);
			this.splitContainer1.Panel1.Controls.Add(this.btnSaveWholeProfile);
			this.splitContainer1.Panel1.Controls.Add(this.rdoSingle);
			this.splitContainer1.Panel1.Controls.Add(this.rdoFullLC);
			this.splitContainer1.Panel1.Controls.Add(this.btnSaveAll);
			this.splitContainer1.Panel1.Controls.Add(this.cboGlycan);
			this.splitContainer1.Panel1.Controls.Add(this.btnLoad);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.AutoScroll = true;
			this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
			this.splitContainer1.Size = new System.Drawing.Size(2364, 1397);
			this.splitContainer1.SplitterDistance = 80;
			this.splitContainer1.SplitterWidth = 10;
			this.splitContainer1.TabIndex = 1;
			// 
			// chkGetAbundance
			// 
			this.chkGetAbundance.AutoSize = true;
			this.chkGetAbundance.Location = new System.Drawing.Point(1925, 17);
			this.chkGetAbundance.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
			this.chkGetAbundance.Name = "chkGetAbundance";
			this.chkGetAbundance.Size = new System.Drawing.Size(295, 41);
			this.chkGetAbundance.TabIndex = 5;
			this.chkGetAbundance.Text = "Get Abundance";
			this.chkGetAbundance.UseVisualStyleBackColor = true;
			this.chkGetAbundance.CheckedChanged += new System.EventHandler(this.chkGetAbundance_CheckedChanged);
			// 
			// chkMergeCharge
			// 
			this.chkMergeCharge.AutoSize = true;
			this.chkMergeCharge.Checked = true;
			this.chkMergeCharge.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkMergeCharge.Location = new System.Drawing.Point(330, 17);
			this.chkMergeCharge.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
			this.chkMergeCharge.Name = "chkMergeCharge";
			this.chkMergeCharge.Size = new System.Drawing.Size(277, 41);
			this.chkMergeCharge.TabIndex = 4;
			this.chkMergeCharge.Text = "Merge Charge";
			this.chkMergeCharge.UseVisualStyleBackColor = true;
			this.chkMergeCharge.CheckedChanged += new System.EventHandler(this.chkMergeCharge_CheckedChanged);
			// 
			// btnSaveWholeProfile
			// 
			this.btnSaveWholeProfile.Location = new System.Drawing.Point(2928, 8);
			this.btnSaveWholeProfile.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
			this.btnSaveWholeProfile.Name = "btnSaveWholeProfile";
			this.btnSaveWholeProfile.Size = new System.Drawing.Size(225, 64);
			this.btnSaveWholeProfile.TabIndex = 3;
			this.btnSaveWholeProfile.Text = "Save Whole";
			this.btnSaveWholeProfile.UseVisualStyleBackColor = true;
			this.btnSaveWholeProfile.Visible = false;
			this.btnSaveWholeProfile.Click += new System.EventHandler(this.btnSaveWholeProfile_Click);
			// 
			// rdoSingle
			// 
			this.rdoSingle.AutoSize = true;
			this.rdoSingle.Checked = true;
			this.rdoSingle.Location = new System.Drawing.Point(1098, 17);
			this.rdoSingle.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
			this.rdoSingle.Name = "rdoSingle";
			this.rdoSingle.Size = new System.Drawing.Size(293, 40);
			this.rdoSingle.TabIndex = 1;
			this.rdoSingle.TabStop = true;
			this.rdoSingle.Text = "Single Glycan";
			this.rdoSingle.UseVisualStyleBackColor = true;
			// 
			// rdoFullLC
			// 
			this.rdoFullLC.AutoSize = true;
			this.rdoFullLC.Location = new System.Drawing.Point(1455, 17);
			this.rdoFullLC.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
			this.rdoFullLC.Name = "rdoFullLC";
			this.rdoFullLC.Size = new System.Drawing.Size(401, 40);
			this.rdoFullLC.TabIndex = 2;
			this.rdoFullLC.Text = "Entire LC expriment";
			this.rdoFullLC.UseVisualStyleBackColor = true;
			this.rdoFullLC.Visible = false;
			this.rdoFullLC.CheckedChanged += new System.EventHandler(this.rdoFullLC_CheckedChanged);
			// 
			// btnSaveAll
			// 
			this.btnSaveAll.Enabled = false;
			this.btnSaveAll.Location = new System.Drawing.Point(2640, 11);
			this.btnSaveAll.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
			this.btnSaveAll.Name = "btnSaveAll";
			this.btnSaveAll.Size = new System.Drawing.Size(225, 64);
			this.btnSaveAll.TabIndex = 2;
			this.btnSaveAll.Text = "Save All";
			this.btnSaveAll.UseVisualStyleBackColor = true;
			this.btnSaveAll.Visible = false;
			this.btnSaveAll.Click += new System.EventHandler(this.btnSaveAll_Click);
			// 
			// cboGlycan
			// 
			this.cboGlycan.FormattingEnabled = true;
			this.cboGlycan.Location = new System.Drawing.Point(627, 15);
			this.cboGlycan.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
			this.cboGlycan.Name = "cboGlycan";
			this.cboGlycan.Size = new System.Drawing.Size(394, 44);
			this.cboGlycan.TabIndex = 1;
			this.cboGlycan.SelectedIndexChanged += new System.EventHandler(this.cboGlycan_SelectedIndexChanged);
			// 
			// splitContainer3
			// 
			this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer3.Location = new System.Drawing.Point(0, 0);
			this.splitContainer3.Margin = new System.Windows.Forms.Padding(0);
			this.splitContainer3.Name = "splitContainer3";
			// 
			// splitContainer3.Panel1
			// 
			this.splitContainer3.Panel1.Controls.Add(this.splitContainer2);
			// 
			// splitContainer3.Panel2
			// 
			this.splitContainer3.Panel2.Controls.Add(this.splitContainer4);
			this.splitContainer3.Size = new System.Drawing.Size(2364, 1307);
			this.splitContainer3.SplitterDistance = 800;
			this.splitContainer3.SplitterWidth = 10;
			this.splitContainer3.TabIndex = 2;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer2.Location = new System.Drawing.Point(0, 0);
			this.splitContainer2.Margin = new System.Windows.Forms.Padding(0);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.eluctionViewer1);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.cht);
			this.splitContainer2.Size = new System.Drawing.Size(800, 1307);
			this.splitContainer2.SplitterDistance = 400;
			this.splitContainer2.SplitterWidth = 10;
			this.splitContainer2.TabIndex = 1;
			// 
			// eluctionViewer1
			// 
			this.eluctionViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eluctionViewer1.Location = new System.Drawing.Point(0, 0);
			this.eluctionViewer1.Margin = new System.Windows.Forms.Padding(0);
			this.eluctionViewer1.Name = "eluctionViewer1";
			this.eluctionViewer1.Size = new System.Drawing.Size(800, 400);
			this.eluctionViewer1.TabIndex = 0;
			this.eluctionViewer1.StatusUpdated += new System.EventHandler(this.ReSizeZedGraphForm3DPlot);
			// 
			// cht
			// 
			chartArea1.Name = "Default";
			this.cht.ChartAreas.Add(chartArea1);
			this.cht.Dock = System.Windows.Forms.DockStyle.Fill;
			legend1.Name = "Default";
			this.cht.Legends.Add(legend1);
			this.cht.Location = new System.Drawing.Point(0, 0);
			this.cht.Margin = new System.Windows.Forms.Padding(0);
			this.cht.Name = "cht";
			series1.ChartArea = "Default";
			series1.Legend = "Default";
			series1.Name = "Series1";
			this.cht.Series.Add(series1);
			this.cht.Size = new System.Drawing.Size(800, 897);
			this.cht.TabIndex = 0;
			this.cht.Text = "chart1";
			title1.Name = "Default";
			this.cht.Titles.Add(title1);
			// 
			// splitContainer4
			// 
			this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer4.Location = new System.Drawing.Point(0, 0);
			this.splitContainer4.Margin = new System.Windows.Forms.Padding(0);
			this.splitContainer4.Name = "splitContainer4";
			this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer4.Panel1
			// 
			this.splitContainer4.Panel1.Controls.Add(this.dgvPeakList);
			// 
			// splitContainer4.Panel2
			// 
			this.splitContainer4.Panel2.Controls.Add(this.splitContainer5);
			this.splitContainer4.Size = new System.Drawing.Size(1554, 1307);
			this.splitContainer4.SplitterDistance = 741;
			this.splitContainer4.SplitterWidth = 10;
			this.splitContainer4.TabIndex = 1;
			// 
			// dgvPeakList
			// 
			this.dgvPeakList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvPeakList.ContextMenuStrip = this.ctxMenuStrip;
			this.dgvPeakList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgvPeakList.Location = new System.Drawing.Point(0, 0);
			this.dgvPeakList.Margin = new System.Windows.Forms.Padding(0);
			this.dgvPeakList.Name = "dgvPeakList";
			this.dgvPeakList.RowHeadersWidth = 123;
			this.dgvPeakList.Size = new System.Drawing.Size(1554, 741);
			this.dgvPeakList.TabIndex = 0;
			// 
			// ctxMenuStrip
			// 
			this.ctxMenuStrip.ImageScalingSize = new System.Drawing.Size(48, 48);
			this.ctxMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mergeToolStripMenuItem,
            this.deleteToolStripMenuItem});
			this.ctxMenuStrip.Name = "ctxMenuStrip";
			this.ctxMenuStrip.Size = new System.Drawing.Size(213, 112);
			// 
			// mergeToolStripMenuItem
			// 
			this.mergeToolStripMenuItem.Name = "mergeToolStripMenuItem";
			this.mergeToolStripMenuItem.Size = new System.Drawing.Size(212, 54);
			this.mergeToolStripMenuItem.Text = "Merge";
			this.mergeToolStripMenuItem.Click += new System.EventHandler(this.mergeToolStripMenuItem_Click);
			// 
			// deleteToolStripMenuItem
			// 
			this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
			this.deleteToolStripMenuItem.Size = new System.Drawing.Size(212, 54);
			this.deleteToolStripMenuItem.Text = "Delete";
			this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
			// 
			// splitContainer5
			// 
			this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer5.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer5.Location = new System.Drawing.Point(0, 0);
			this.splitContainer5.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
			this.splitContainer5.Name = "splitContainer5";
			this.splitContainer5.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer5.Panel1
			// 
			this.splitContainer5.Panel1.Controls.Add(this.chkAdductPeak);
			this.splitContainer5.Panel1.Controls.Add(this.btnUpdate);
			// 
			// splitContainer5.Panel2
			// 
			this.splitContainer5.Panel2.Controls.Add(this.chkboxlstPeak);
			this.splitContainer5.Size = new System.Drawing.Size(1554, 556);
			this.splitContainer5.SplitterDistance = 80;
			this.splitContainer5.SplitterWidth = 10;
			this.splitContainer5.TabIndex = 1;
			// 
			// chkAdductPeak
			// 
			this.chkAdductPeak.AutoSize = true;
			this.chkAdductPeak.Checked = true;
			this.chkAdductPeak.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkAdductPeak.Location = new System.Drawing.Point(279, 22);
			this.chkAdductPeak.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
			this.chkAdductPeak.Name = "chkAdductPeak";
			this.chkAdductPeak.Size = new System.Drawing.Size(349, 41);
			this.chkAdductPeak.TabIndex = 6;
			this.chkAdductPeak.Text = "Plot Adduct Peak";
			this.chkAdductPeak.UseVisualStyleBackColor = true;
			this.chkAdductPeak.CheckedChanged += new System.EventHandler(this.chkAdductPeak_CheckedChanged);
			// 
			// btnUpdate
			// 
			this.btnUpdate.Location = new System.Drawing.Point(21, 11);
			this.btnUpdate.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
			this.btnUpdate.Name = "btnUpdate";
			this.btnUpdate.Size = new System.Drawing.Size(225, 64);
			this.btnUpdate.TabIndex = 5;
			this.btnUpdate.Text = "Update";
			this.btnUpdate.UseVisualStyleBackColor = true;
			this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
			// 
			// chkboxlstPeak
			// 
			this.chkboxlstPeak.CheckOnClick = true;
			this.chkboxlstPeak.Dock = System.Windows.Forms.DockStyle.Fill;
			this.chkboxlstPeak.FormattingEnabled = true;
			this.chkboxlstPeak.Location = new System.Drawing.Point(0, 0);
			this.chkboxlstPeak.Margin = new System.Windows.Forms.Padding(0);
			this.chkboxlstPeak.Name = "chkboxlstPeak";
			this.chkboxlstPeak.Size = new System.Drawing.Size(1554, 466);
			this.chkboxlstPeak.TabIndex = 0;
			this.chkboxlstPeak.SelectedIndexChanged += new System.EventHandler(this.chkboxlstPeak_SelectedIndexChanged);
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.FileName = "openFileDialog1";
			// 
			// frmView
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.AutoScroll = true;
			this.AutoSize = true;
			this.ClientSize = new System.Drawing.Size(2364, 1397);
			this.Controls.Add(this.splitContainer1);
			this.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
			this.Name = "frmView";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "View";
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer3.Panel1.ResumeLayout(false);
			this.splitContainer3.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
			this.splitContainer3.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.cht)).EndInit();
			this.splitContainer4.Panel1.ResumeLayout(false);
			this.splitContainer4.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
			this.splitContainer4.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dgvPeakList)).EndInit();
			this.ctxMenuStrip.ResumeLayout(false);
			this.splitContainer5.Panel1.ResumeLayout(false);
			this.splitContainer5.Panel1.PerformLayout();
			this.splitContainer5.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).EndInit();
			this.splitContainer5.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ComboBox cboGlycan;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button btnSaveAll;
        private System.Windows.Forms.RadioButton rdoSingle;
        private System.Windows.Forms.RadioButton rdoFullLC;
        private System.Windows.Forms.Button btnSaveWholeProfile;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private ElutionViewer.EluctionViewer eluctionViewer1;
        private System.Windows.Forms.CheckBox chkMergeCharge;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.DataGridView dgvPeakList;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.CheckedListBox chkboxlstPeak;
        private System.Windows.Forms.SplitContainer splitContainer5;
        private System.Windows.Forms.CheckBox chkAdductPeak;
        private System.Windows.Forms.CheckBox chkGetAbundance;
        private System.Windows.Forms.DataVisualization.Charting.Chart cht;
		private System.Windows.Forms.ContextMenuStrip ctxMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem mergeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
	}
}