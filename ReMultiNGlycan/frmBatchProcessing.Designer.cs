namespace COL.MultiGlycan
{
    partial class frmBatchProcessing
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
            this.bgWorker_Process = new System.ComponentModel.BackgroundWorker();
            this.lblCurrentFile = new System.Windows.Forms.Label();
            this.lblScan = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.lblPercentage1 = new System.Windows.Forms.Label();
            this.lblStatus1 = new System.Windows.Forms.Label();
            this.progressBar2 = new System.Windows.Forms.ProgressBar();
            this.progressBar3 = new System.Windows.Forms.ProgressBar();
            this.lblPercentage2 = new System.Windows.Forms.Label();
            this.lblPercentage3 = new System.Windows.Forms.Label();
            this.lblFileName1 = new System.Windows.Forms.Label();
            this.lblFileName2 = new System.Windows.Forms.Label();
            this.lblFileName3 = new System.Windows.Forms.Label();
            this.lblStatus2 = new System.Windows.Forms.Label();
            this.lblStatus3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // bgWorker_Process
            // 
            this.bgWorker_Process.WorkerReportsProgress = true;
            this.bgWorker_Process.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgWorker_Process_DoWork);
            this.bgWorker_Process.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgWorker_Process_ProgressChanged);
            this.bgWorker_Process.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgWorker_Process_RunWorkerCompleted);
            // 
            // lblCurrentFile
            // 
            this.lblCurrentFile.AutoSize = true;
            this.lblCurrentFile.Location = new System.Drawing.Point(141, 9);
            this.lblCurrentFile.Name = "lblCurrentFile";
            this.lblCurrentFile.Size = new System.Drawing.Size(30, 13);
            this.lblCurrentFile.TabIndex = 0;
            this.lblCurrentFile.Text = "0 / 0";
            // 
            // lblScan
            // 
            this.lblScan.AutoSize = true;
            this.lblScan.Location = new System.Drawing.Point(0, 9);
            this.lblScan.Name = "lblScan";
            this.lblScan.Size = new System.Drawing.Size(143, 13);
            this.lblScan.TabIndex = 1;
            this.lblScan.Text = "Processed Files / Total Files:";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(3, 47);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(317, 23);
            this.progressBar1.TabIndex = 2;
            // 
            // lblPercentage1
            // 
            this.lblPercentage1.AutoSize = true;
            this.lblPercentage1.Location = new System.Drawing.Point(321, 52);
            this.lblPercentage1.Name = "lblPercentage1";
            this.lblPercentage1.Size = new System.Drawing.Size(24, 13);
            this.lblPercentage1.TabIndex = 3;
            this.lblPercentage1.Text = "0 %";
            // 
            // lblStatus1
            // 
            this.lblStatus1.AutoSize = true;
            this.lblStatus1.Location = new System.Drawing.Point(192, 31);
            this.lblStatus1.Name = "lblStatus1";
            this.lblStatus1.Size = new System.Drawing.Size(74, 13);
            this.lblStatus1.TabIndex = 6;
            this.lblStatus1.Text = "Status: Ready";
            // 
            // progressBar2
            // 
            this.progressBar2.Location = new System.Drawing.Point(3, 108);
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.Size = new System.Drawing.Size(317, 23);
            this.progressBar2.TabIndex = 7;
            // 
            // progressBar3
            // 
            this.progressBar3.Location = new System.Drawing.Point(3, 169);
            this.progressBar3.Name = "progressBar3";
            this.progressBar3.Size = new System.Drawing.Size(317, 23);
            this.progressBar3.TabIndex = 8;
            // 
            // lblPercentage2
            // 
            this.lblPercentage2.AutoSize = true;
            this.lblPercentage2.Location = new System.Drawing.Point(321, 113);
            this.lblPercentage2.Name = "lblPercentage2";
            this.lblPercentage2.Size = new System.Drawing.Size(24, 13);
            this.lblPercentage2.TabIndex = 10;
            this.lblPercentage2.Text = "0 %";
            // 
            // lblPercentage3
            // 
            this.lblPercentage3.AutoSize = true;
            this.lblPercentage3.Location = new System.Drawing.Point(321, 174);
            this.lblPercentage3.Name = "lblPercentage3";
            this.lblPercentage3.Size = new System.Drawing.Size(24, 13);
            this.lblPercentage3.TabIndex = 11;
            this.lblPercentage3.Text = "0 %";
            // 
            // lblFileName1
            // 
            this.lblFileName1.AutoSize = true;
            this.lblFileName1.Location = new System.Drawing.Point(0, 31);
            this.lblFileName1.Name = "lblFileName1";
            this.lblFileName1.Size = new System.Drawing.Size(54, 13);
            this.lblFileName1.TabIndex = 13;
            this.lblFileName1.Text = "FileName:";
            // 
            // lblFileName2
            // 
            this.lblFileName2.AutoSize = true;
            this.lblFileName2.Location = new System.Drawing.Point(0, 92);
            this.lblFileName2.Name = "lblFileName2";
            this.lblFileName2.Size = new System.Drawing.Size(54, 13);
            this.lblFileName2.TabIndex = 14;
            this.lblFileName2.Text = "FileName:";
            // 
            // lblFileName3
            // 
            this.lblFileName3.AutoSize = true;
            this.lblFileName3.Location = new System.Drawing.Point(0, 153);
            this.lblFileName3.Name = "lblFileName3";
            this.lblFileName3.Size = new System.Drawing.Size(54, 13);
            this.lblFileName3.TabIndex = 15;
            this.lblFileName3.Text = "FileName:";
            // 
            // lblStatus2
            // 
            this.lblStatus2.AutoSize = true;
            this.lblStatus2.Location = new System.Drawing.Point(192, 92);
            this.lblStatus2.Name = "lblStatus2";
            this.lblStatus2.Size = new System.Drawing.Size(74, 13);
            this.lblStatus2.TabIndex = 17;
            this.lblStatus2.Text = "Status: Ready";
            // 
            // lblStatus3
            // 
            this.lblStatus3.AutoSize = true;
            this.lblStatus3.Location = new System.Drawing.Point(192, 153);
            this.lblStatus3.Name = "lblStatus3";
            this.lblStatus3.Size = new System.Drawing.Size(74, 13);
            this.lblStatus3.TabIndex = 18;
            this.lblStatus3.Text = "Status: Ready";
            // 
            // frmBatchProcessing
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(351, 207);
            this.Controls.Add(this.lblStatus3);
            this.Controls.Add(this.lblStatus2);
            this.Controls.Add(this.lblFileName3);
            this.Controls.Add(this.lblFileName2);
            this.Controls.Add(this.lblFileName1);
            this.Controls.Add(this.lblPercentage3);
            this.Controls.Add(this.lblPercentage2);
            this.Controls.Add(this.progressBar3);
            this.Controls.Add(this.progressBar2);
            this.Controls.Add(this.lblStatus1);
            this.Controls.Add(this.lblPercentage1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.lblScan);
            this.Controls.Add(this.lblCurrentFile);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmBatchProcessing";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Processing";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.ComponentModel.BackgroundWorker bgWorker_Process;
        private System.Windows.Forms.Label lblCurrentFile;
        private System.Windows.Forms.Label lblScan;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label lblPercentage1;
        private System.Windows.Forms.Label lblStatus1;
        private System.Windows.Forms.ProgressBar progressBar2;
        private System.Windows.Forms.ProgressBar progressBar3;
        private System.Windows.Forms.Label lblPercentage2;
        private System.Windows.Forms.Label lblPercentage3;
        private System.Windows.Forms.Label lblFileName1;
        private System.Windows.Forms.Label lblFileName2;
        private System.Windows.Forms.Label lblFileName3;
        private System.Windows.Forms.Label lblStatus2;
        private System.Windows.Forms.Label lblStatus3;
    }
}