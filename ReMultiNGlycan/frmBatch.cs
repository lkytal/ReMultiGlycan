using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using COL.MassLib;

namespace COL.MultiGlycan
{
    public partial class frmBatch : Form
    {
        //frmPeakParameters frmPeakpara;
        bool DoLog = false;
        //private int _endScan = 0;
        //ThermoRawReader raw;
        public frmBatch()
        {
            InitializeComponent();
            this.Text = this.Text + "  " + AssemblyVersion.Split('.')[0] + "." + AssemblyVersion.Split('.')[1] + "." + AssemblyVersion.Split('.')[2];// +" (build: " + AssemblyVersion.Split('.')[2] + ")"; 
            cboSia.SelectedIndex = 0;
            //int MaxCPU = Environment.ProcessorCount;
            //for (int i = 1; i <= MaxCPU; i++)
            //{
            //    cboCPU.Items.Add(i); 
            //}
            //cboCPU.SelectedIndex = (int)Math.Floor(cboCPU.Items.Count / 2.0f)-1;   
          
        }


        private void btnBrowseRaw_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtRawFile.Text = folderBrowserDialog1.SelectedPath;
                lstFileList.Items.Clear();
                lstFileToProcess.Items.Clear();
                btnRemoveFile.Enabled = false;
                btnRemoveAllFiles.Enabled = false;
                
                foreach (string strFile in System.IO.Directory.GetFiles(folderBrowserDialog1.SelectedPath))
                {
                    if (Path.GetExtension(strFile).ToLower() == ".raw" ||
                        Path.GetExtension(strFile).ToLower() == ".mzxml")
                    {
                        lstFileList.Items.Add(Path.GetFileName(strFile));
                    }
                }
                btnAddFile.Enabled = false;
                btnAddAllFiles.Enabled = false;
                if (lstFileList.Items.Count != 0)
                {
                    btnAddFile.Enabled = true;
                    btnAddAllFiles.Enabled = true;
                }
            }
        }

        private void rdoDefaultList_CheckedChanged(object sender, EventArgs e)
        {
            rdoUserList.Checked = !rdoDefaultList.Checked;
            txtGlycanList.Enabled = !rdoDefaultList.Checked;
            btnBrowseGlycan.Enabled = !rdoDefaultList.Checked;            
        }


        private void PreparingMultiGlycan(Dictionary<COL.GlycoLib.enumLabelingTag,float> argLabeling, List<string> argRawFiles )
        {
         
            //saveFileDialog1.Filter = "Excel Files (*.xslx)|*.xslx";
            //saveFileDialog1.Filter = "CSV Files (*.csv)|*.csv";

            DateTime time = DateTime.Now;             // Use current time
            //string TimeFormat = "yyMMdd HHmm";            // Use this format
            if (DoLog)
            {
                Logger.WriteLog(System.Environment.NewLine + System.Environment.NewLine + "-----------------------------------------------------------");
                Logger.WriteLog("Start Process");
            }

            //saveFileDialog1.FileName = Path.GetDirectoryName(txtRawFile.Text) + "\\" + Path.GetFileNameWithoutExtension(txtRawFile.Text) + "-" + time.ToString(TimeFormat);


            if (txtRawFile.Text == "" || (rdoUserList.Checked && txtGlycanList.Text == "") || txtMaxLCTime.Text == "")
            {
                MessageBox.Show("Please check input values.");
                if (DoLog)
                {
                    Logger.WriteLog("End Process- because input value not complete");
                }
                return;
            }
            //folderBrowserDialog1.SelectedPath = txtRawFile.Text;
            string defaultOutputFolder = txtRawFile.Text + "\\" +DateTime.Now.ToString("yyMMdd-HHmm");
            if (!Directory.Exists(defaultOutputFolder))
            {
                Directory.CreateDirectory(defaultOutputFolder);
            }
            folderBrowserDialog1.SelectedPath = defaultOutputFolder;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                string glycanlist = System.Windows.Forms.Application.StartupPath + "\\Default_Combination.csv";
                if (!rdoDefaultList.Checked)
                {
                    glycanlist = txtGlycanList.Text;
                }

                if (DoLog)
                {

                    Logger.WriteLog("Start initial program");

                }
                List<float> AdductMasses = new List<float>();
                Dictionary<float, string> AdductLabel = new Dictionary<float, string>();
                if (chkAdductK.Checked)
                {
                    AdductMasses.Add(MassLib.Atoms.Potassium);
                    AdductLabel.Add(MassLib.Atoms.Potassium, "K");
                }
                if (chkAdductNH4.Checked)
                {
                    AdductMasses.Add(MassLib.Atoms.NitrogenMass + 4 * MassLib.Atoms.HydrogenMass);
                    AdductLabel.Add(MassLib.Atoms.NitrogenMass + 4 * MassLib.Atoms.HydrogenMass, "NH4");
                }
                if (chkAdductNa.Checked)
                {
                    AdductMasses.Add(MassLib.Atoms.SodiumMass);
                    AdductLabel.Add(MassLib.Atoms.SodiumMass, "Na");
                }
                if (chkAdductProton.Checked)
                {
                    AdductMasses.Add(MassLib.Atoms.ProtonMass);
                    AdductLabel.Add(MassLib.Atoms.ProtonMass, "H");
                }
                float outMass = 0.0f;
                if (chkAdductUser.Checked && float.TryParse(txtAdductMass.Text, out outMass))
                {
                    AdductMasses.Add(outMass);
                    AdductLabel.Add(outMass, "User");
                }

                // MultiNGlycanESIMultiThreads MultiESIs = new MultiNGlycanESIMultiThreads(glycanlist, txtRawFile.Text, Convert.ToInt32(cboCPU.SelectedItem), _peakParameter, _transformParameters);
                MultiGlycanESI ESI = null;
                if (argLabeling.Count!=0)
                {
                    ESI = new MultiGlycanESI(txtRawFile.Text, 0,99999, glycanlist, Convert.ToDouble(txtPPM.Text), Convert.ToDouble(txtGlycanPPM.Text), chkPermethylated.Checked, chkReducedReducingEnd.Checked,cboSia.SelectedIndex, argLabeling, AdductLabel,AdductMasses, DoLog);
                }
                else
                {
                    ESI = new MultiGlycanESI(txtRawFile.Text,0, 99999, glycanlist, Convert.ToDouble(txtPPM.Text), Convert.ToDouble(txtGlycanPPM.Text), chkPermethylated.Checked, chkReducedReducingEnd.Checked, cboSia.SelectedIndex, DoLog);
                }
                ESI.LabelingMethod = GlycoLib.enumGlycanLabelingMethod.None;
                if (rdoDRAG.Checked)
                {
                    ESI.LabelingMethod = GlycoLib.enumGlycanLabelingMethod.DRAG;
                }
                else if (rdoMultiplePemrthylated.Checked)
                {
                    ESI.LabelingMethod = GlycoLib.enumGlycanLabelingMethod.MultiplexPermethylated;
                }

                ESI.MergeDifferentChargeIntoOne = chkMergeDffCharge.Checked;
                ESI.ExportFilePath = folderBrowserDialog1.SelectedPath;
                ESI.MaxLCBackMin = Convert.ToSingle(txtMaxLCTime.Text);
                ESI.MaxLCFrontMin = Convert.ToSingle(txtMinLCTime.Text);
                ESI.IsotopePPM = Convert.ToSingle(txtIsotopeEnvTolerence.Text);
                ESI.MininumIsotopePeakCount = Convert.ToInt32(txtIsotopeEnvMinPeakCount.Text);
                ESI.PeakSNRatio = Convert.ToSingle(txtSN.Text);
                ESI.IsMatchMonoisotopicOnly = chkMonoOnly.Checked;
                ESI.ApplyLinearRegLC = chkApplyLinearRegLC.Checked;
                ESI.ForceProtonatedGlycan = chkForceProtonated.Checked;
                ESI.MinPeakHeightPrecentage = Convert.ToSingle(txtMinPeakHeight.Text);
                if (chkAbundance.Checked)
                {
                    ESI.MinAbundance = Convert.ToDouble(txtAbundanceMin.Text);
                }
                else
                {
                    ESI.MinAbundance = 0;
                }
                if (chkMinLengthOfLC.Checked)
                {
                    ESI.MinLengthOfLC = Convert.ToSingle(txtScanCount.Text);
                }
                else
                {
                    ESI.MinLengthOfLC = 0;
                }

               
               // ESI.AdductMass = AdductMasses;
                //ESI.AdductMassToLabel = AdductLabel;
                ESI.IndividualImgs = chkIndividualImg.Checked;
                ESI.QuantificationImgs = chkQuantImgs.Checked;
                ESI.TotalLCTime = Convert.ToSingle(txtLCTime.Text);
                ESI.LCTimeTolerance = Convert.ToSingle(txtLCTolerance.Text) / 100.0f;
                if (DoLog)
                {
                    Logger.WriteLog("Initial program complete");
                }
                this.Visible = false;
                int concurrent = Convert.ToInt32(cboConcurrent.Text);
                frmBatchProcessing frmProcess = new frmBatchProcessing(ESI, argRawFiles,  concurrent,DoLog);
                frmProcess.ProtonatedResult = chkProtonatedResult.Checked;
                if (concurrent == 1)
                {
                    frmProcess.Size = new Size(370, 120);
                }
                else if (concurrent ==2)
                {
                    frmProcess.Size = new Size(370, 180);
                }
                else
                {
                    frmProcess.Size = new Size(370, 240);
                }
                frmProcess.ShowDialog();
                this.Visible = true;
                if (DoLog)
                {
                    Logger.WriteLog("Finish process");
                }
            }          
        }
        private void btnBrowseGlycan_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "CSV Files (.csv)|*.csv";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {                
                txtGlycanList.Text = openFileDialog1.FileName;
            }
        }
      

        private void chkAdductUser_CheckedChanged(object sender, EventArgs e)
        {
            txtAdductMass.Enabled = chkAdductUser.Checked;
        }

        private void eluctionProfileViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            frmView frView = new frmView();
            frView.ShowDialog();
            this.Visible = true;
        }

 

        private void chkAbundance_CheckedChanged(object sender, EventArgs e)
        {
            txtAbundanceMin.Enabled = chkAbundance.Checked;
        }

        private void chkScanCount_CheckedChanged(object sender, EventArgs e)
        {
            txtScanCount.Enabled = chkMinLengthOfLC.Checked;
        }

        private void massCalculatorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmCalculator frmCalc = new frmCalculator();
            frmCalc.Show();
        }


        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion

    
        private class PeaksFromResult
        {
            float _Time;
            float _Intensity;
            string _Adduct;

            public float Time
            {
                get { return _Time; }
            }
            public float Intensity
            {
                get { return _Intensity; }
            }
            public string Adduct
            {
                get { return _Adduct; }
            }
            public PeaksFromResult(float argTime, float argIntensity, string argAdduct)
            {
                _Time = argTime;
                _Intensity = argIntensity;
                _Adduct = argAdduct;
            }

        }

        private void chkQuanCH3_CheckedChanged(object sender, EventArgs e)
        {
            txtQuanCH3.Enabled = chkQuanCH3.Checked;
        }

        private void chkQuanCH2D_CheckedChanged(object sender, EventArgs e)
        {
            txtQuanCH2D.Enabled = chkQuanCH2D.Checked;
        }

        private void chkQuanCHD2_CheckedChanged(object sender, EventArgs e)
        {
            txtQuanCHD2.Enabled = chkQuanCHD2.Checked;
        }

        private void chkQuanCD3_CheckedChanged(object sender, EventArgs e)
        {
            txtQuanCD3.Enabled = chkQuanCD3.Checked;
        }

        private void chkQuan13CH3_CheckedChanged(object sender, EventArgs e)
        {
            txtQuan13CH3.Enabled = chkQuan13CH3.Checked;
        }

        private void chkQuan13CH2D_CheckedChanged(object sender, EventArgs e)
        {
            txtQuan13CD3.Enabled = chkQuan13CD3.Checked;
        }

        private void chkQuan13CHD2_CheckedChanged(object sender, EventArgs e)
        {
            txtQuan13CHD2.Enabled = chkQuan13CHD2.Checked;
        }

        private void btnQuan_Click(object sender, EventArgs e)
        {
            Dictionary<COL.GlycoLib.enumLabelingTag, float> labelingRatio = new Dictionary<GlycoLib.enumLabelingTag, float>();
            
            if (rdoDRAG.Checked)
            {
                if (chkPermethylated.Checked)
                {
                    MessageBox.Show("Permethylated check box should not be checked");
                    chkPermethylated.Checked = false;
                }
                if (chkReducedReducingEnd.Checked)
                {
                    MessageBox.Show("Reduced reducing end check box should not be checked");
                    chkReducedReducingEnd.Checked = false;
                }
                if (chkDRAG_Light.Checked)
                {
                    labelingRatio.Add(GlycoLib.enumLabelingTag.DRAG_Light, Convert.ToSingle(txtDRAGLight.Text));
                }
                if (chkDRAG_Heavy.Checked)
                {
                    labelingRatio.Add(GlycoLib.enumLabelingTag.DRAG_Heavy, Convert.ToSingle(txtDRAGHeavy.Text));
                }
            }
            else if (rdoMultiplePemrthylated.Checked) // MP
            {
                if (!chkPermethylated.Checked)
                {
                    MessageBox.Show("Permethylated check box should be checked");
                    chkPermethylated.Checked = true;
                }
                if (chkQuan13CD3.Checked)
                {
                    labelingRatio.Add(GlycoLib.enumLabelingTag.MP_13CD3, Convert.ToSingle(txtQuan13CD3.Text));
                }
                if (chkQuan13CH3.Checked)
                {
                    labelingRatio.Add(GlycoLib.enumLabelingTag.MP_13CH3, Convert.ToSingle(txtQuan13CH3.Text));
                }
                if (chkQuan13CHD2.Checked)
                {
                    labelingRatio.Add(GlycoLib.enumLabelingTag.MP_13CHD2, Convert.ToSingle(txtQuan13CHD2.Text));
                }
                if (chkQuanCD3.Checked)
                {
                    labelingRatio.Add(GlycoLib.enumLabelingTag.MP_CD3, Convert.ToSingle(txtQuanCD3.Text));
                }
                if (chkQuanCH2D.Checked)
                {
                    labelingRatio.Add(GlycoLib.enumLabelingTag.MP_CH2D, Convert.ToSingle(txtQuanCH2D.Text));
                }
                if (chkQuanCH3.Checked)
                {
                    labelingRatio.Add(GlycoLib.enumLabelingTag.MP_CH3, Convert.ToSingle(txtQuanCH3.Text));
                }
                if (chkQuanCHD2.Checked)
                {
                    labelingRatio.Add(GlycoLib.enumLabelingTag.MP_CHD2, Convert.ToSingle(txtQuanCHD2.Text));
                }
            }
            else
            {
                labelingRatio.Add(GlycoLib.enumLabelingTag.None, 1.0f);
            }
            List<string> lstRawFiles = new List<string>();
            foreach (string strFile in lstFileToProcess.Items)
            {
                lstRawFiles.Add(txtRawFile.Text + "\\" + strFile);
            }

            PreparingMultiGlycan(labelingRatio, lstRawFiles);
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 1;
        }

        private void rdoNeutral_CheckedChanged(object sender, EventArgs e)
        {
            grpDRAG.Enabled = false;
            grpMultiplexPreM.Enabled = false;
            chkQuantImgs.Checked = false;
            chkQuantImgs.Enabled = false;
        }

        private void rdoDRAG_CheckedChanged(object sender, EventArgs e)
        {
            grpDRAG.Enabled = true;
            grpMultiplexPreM.Enabled = false;
            chkReducedReducingEnd.Checked = false;
            chkPermethylated.Checked = false;
            chkQuantImgs.Checked = true;
            chkQuantImgs.Enabled = true;
        }

        private void rdoMultiplePemrthylated_CheckedChanged(object sender, EventArgs e)
        {
            grpDRAG.Enabled = false;
            grpMultiplexPreM.Enabled = true;
            chkQuantImgs.Checked = true;
            chkQuantImgs.Enabled = true;
        }


        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rdoLabelFree.Checked)
            {
                chkQuantImgs.Checked = false;
                chkQuantImgs.Enabled = false;
            }
        }

     

        private void btnAddAllFiles_Click(object sender, EventArgs e)
        {
            lstFileToProcess.Items.AddRange(lstFileList.Items);
            lstFileList.Items.Clear();
            btnRemoveAllFiles.Enabled = true;
            btnRemoveFile.Enabled = true;
            btnAddAllFiles.Enabled = false;
            btnAddFile.Enabled = false;
        }

        private void btnAddFile_Click(object sender, EventArgs e)
        {
            int[] AddIdxs = new int[lstFileList.SelectedIndices.Count];
            int AddIdx = 0;
            foreach (int selectIdx in lstFileList.SelectedIndices)
            {
                lstFileToProcess.Items.Add(lstFileList.Items[selectIdx]);
                AddIdxs[AddIdx] = selectIdx;
                AddIdx = AddIdx + 1;
            }
            for (int i = AddIdxs.Length - 1; i >= 0; i--)
            {
                lstFileList.Items.RemoveAt(AddIdxs[i]);
            }
            if (lstFileList.Items.Count == 0)
            {
                btnAddAllFiles.Enabled = false;
                btnAddFile.Enabled = false;
            }
            if (lstFileToProcess.Items.Count != 0)
            {
                btnRemoveAllFiles.Enabled = true;
                btnRemoveFile.Enabled = true;
            }
        }

        private void btnRemoveFile_Click(object sender, EventArgs e)
        {
            int[] RemoveIdxs = new int[lstFileToProcess.SelectedIndices.Count];
            int rveIdx = 0;
            foreach (int selectIdx in lstFileToProcess.SelectedIndices)
            {
                lstFileList.Items.Add(lstFileToProcess.Items[selectIdx]);
                RemoveIdxs[rveIdx] = selectIdx;
                rveIdx = rveIdx + 1;
            }
            for (int i = RemoveIdxs.Length - 1; i >= 0; i--)
            {
                lstFileToProcess.Items.RemoveAt(RemoveIdxs[i]);
            }
            if (lstFileList.Items.Count != 0)
            {
                btnAddAllFiles.Enabled = true;
                btnAddFile.Enabled = true;
            }
            if (lstFileToProcess.Items.Count == 0)
            {
                btnRemoveAllFiles.Enabled = false;
                btnRemoveFile.Enabled = false;
            }
        }

        private void btnRemoveAllFiles_Click(object sender, EventArgs e)
        {
            lstFileList.Items.AddRange(lstFileToProcess.Items);
            lstFileToProcess.Items.Clear();
            btnRemoveAllFiles.Enabled = false;
            btnRemoveFile.Enabled = false;
            btnAddAllFiles.Enabled = true;
            btnAddFile.Enabled = true;
        }
        Dictionary<string, string> MergeResultfiles = new Dictionary<string, string>();
        private void btnMergeBrowseRaw_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtMergeRawFile.Text = folderBrowserDialog1.SelectedPath;
                lstMergeFileList.Items.Clear();
                lstMergeFileToProcess.Items.Clear();
                btnMergeRemoveFile.Enabled = false;
                btnMergeRemoveAllFiles.Enabled = false;
         
                foreach (string strFile in System.IO.Directory.GetFiles(folderBrowserDialog1.SelectedPath,"*.csv", System.IO.SearchOption.AllDirectories))
                {
                    if (strFile.EndsWith("_FullList.csv"))
                    {
                        MergeResultfiles.Add(Path.GetFileName(strFile), strFile);
                        lstMergeFileList.Items.Add(Path.GetFileName(strFile));
                    }
                }

                btnMergeAddFile.Enabled = false;
                btnMergeAddAllFiles.Enabled = false;
                if (lstMergeFileList.Items.Count != 0)
                {
                    btnMergeAddFile.Enabled = true;
                    btnMergeAddAllFiles.Enabled = true;
                }
            }
        }

        private void btnMergeAddAllFiles_Click(object sender, EventArgs e)
        {
            lstMergeFileToProcess.Items.AddRange(lstMergeFileList.Items);
            lstMergeFileList.Items.Clear();
            btnMergeRemoveAllFiles.Enabled = true;
            btnMergeRemoveFile.Enabled = true;
            btnMergeAddAllFiles.Enabled = false;
            btnMergeAddFile.Enabled = false;
        }

        private void btnMergeAddFile_Click(object sender, EventArgs e)
        {
            int[] AddIdxs = new int[lstMergeFileList.SelectedIndices.Count];
            int AddIdx = 0;
            foreach (int selectIdx in lstMergeFileList.SelectedIndices)
            {
                lstMergeFileToProcess.Items.Add(lstMergeFileList.Items[selectIdx]);
                AddIdxs[AddIdx] = selectIdx;
                AddIdx = AddIdx + 1;
            }
            for (int i = AddIdxs.Length - 1; i >= 0; i--)
            {
                lstMergeFileList.Items.RemoveAt(AddIdxs[i]);
            }
            if (lstMergeFileList.Items.Count == 0)
            {
                btnMergeAddAllFiles.Enabled = false;
                btnMergeAddFile.Enabled = false;
            }
            if (lstMergeFileToProcess.Items.Count != 0)
            {
                btnMergeRemoveAllFiles.Enabled = true;
                btnMergeRemoveFile.Enabled = true;
            }
        }

        private void btnMergeRemoveFile_Click(object sender, EventArgs e)
        {
            int[] RemoveIdxs = new int[lstMergeFileToProcess.SelectedIndices.Count];
            int rveIdx = 0;
            foreach (int selectIdx in lstMergeFileToProcess.SelectedIndices)
            {
                lstMergeFileList.Items.Add(lstMergeFileToProcess.Items[selectIdx]);
                RemoveIdxs[rveIdx] = selectIdx;
                rveIdx = rveIdx + 1;
            }
            for (int i = RemoveIdxs.Length - 1; i >= 0; i--)
            {
                lstMergeFileToProcess.Items.RemoveAt(RemoveIdxs[i]);
            }
            if (lstMergeFileList.Items.Count != 0)
            {
                btnMergeAddAllFiles.Enabled = true;
                btnMergeAddFile.Enabled = true;
            }
            if (lstMergeFileToProcess.Items.Count == 0)
            {
                btnMergeRemoveAllFiles.Enabled = false;
                btnMergeRemoveFile.Enabled = false;
            }
        }

        private void btnMergeRemoveAllFiles_Click(object sender, EventArgs e)
        {
            lstMergeFileList.Items.AddRange(lstMergeFileToProcess.Items);
            lstMergeFileToProcess.Items.Clear();
            btnMergeRemoveAllFiles.Enabled = false;
            btnMergeRemoveFile.Enabled = false;
            btnMergeAddAllFiles.Enabled = true;
            btnMergeAddFile.Enabled = true;

        }

        private void btnReMerge_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = folderBrowserDialog1.SelectedPath + "\\MergeResult_FullList.csv";
            saveFileDialog1.Filter = "CSV Files (*.csv)|*.csv";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                List<string> files = new List<string>();
                foreach (string file in lstMergeFileToProcess.Items)
                {
                    files.Add(MergeResultfiles[file]);
                }
                MergeQuantitationResults.MergeFullList(files, saveFileDialog1.FileName);

                files.Clear();
                foreach (string file in lstMergeFileToProcess.Items)
                {
                    files.Add(MergeResultfiles[file].Replace("_FullList.csv", ".csv"));
                }
                MergeQuantitationResults.MergeConservedList(files,saveFileDialog1.FileName.Replace("_FullList.csv",".csv"));


                MessageBox.Show("Merge Complete");
            }

        }





      
    }
}
