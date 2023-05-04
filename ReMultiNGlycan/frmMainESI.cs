using COL.GlycoLib;
using COL.MassLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace COL.MultiGlycan
{
	public partial class frmMainESI : Form
	{
		private frmPeakParameters frmPeakpara;
		private bool DoLog = false;
		private int _endScan = 0;
		private COL.MassLib.ThermoRawReader raw;

		public frmMainESI()
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
			openFileDialog1.Filter = "RAW Files (*.raw; *.mzXML)|*.raw;*.mzxml";
			openFileDialog1.Filter = "RAW Files (*.raw)|*.raw";
			openFileDialog1.FileName = "";
			if (openFileDialog1.ShowDialog() == DialogResult.OK)
			{
				txtRawFile.Text = openFileDialog1.FileName;

				if (Path.GetExtension(openFileDialog1.FileName) == ".raw")
				{
					raw = new COL.MassLib.ThermoRawReader(txtRawFile.Text);
					_endScan = raw.NumberOfScans;
				}
				//else
				//{
				//    COL.MassLib.RawReader raw = new COL.MassLib.RawReader(txtRawFile.Text,"mzxml");
				//    _endScan = raw.NumberOfScans;
				//}

				lblLastScanTime.Text = "Last Scan Time:" + raw.ReadScan(raw.NumberOfScans).Time.ToString();
				txtEndScan.Text = _endScan.ToString();
				txtLCTime.Text = raw.ReadScan(raw.NumberOfScans).Time.ToString("0");
			}
		}

		private void rdoDefaultList_CheckedChanged(object sender, EventArgs e)
		{
			rdoUserList.Checked = !rdoDefaultList.Checked;
			txtGlycanList.Enabled = !rdoDefaultList.Checked;
			btnBrowseGlycan.Enabled = !rdoDefaultList.Checked;
		}

		private void btnMerge_Click(object sender, EventArgs e)
		{
			PreparingMultiGlycan(null);
		}

		private void PreparingMultiGlycan(Dictionary<COL.GlycoLib.enumLabelingTag, float> argLabeling)
		{
			DoLog = chkLog.Checked;
			//saveFileDialog1.Filter = "Excel Files (*.xslx)|*.xslx";
			//saveFileDialog1.Filter = "CSV Files (*.csv)|*.csv";

			DateTime time = DateTime.Now;             // Use current time
			string TimeFormat = "yyMMdd HHmm";            // Use this format
			if (DoLog)
			{
				Logger.WriteLog(System.Environment.NewLine + System.Environment.NewLine + "-----------------------------------------------------------");
				Logger.WriteLog("Start Process");
			}

			saveFileDialog1.FileName = Path.GetDirectoryName(txtRawFile.Text) + "\\" + Path.GetFileNameWithoutExtension(txtRawFile.Text) + "-" + time.ToString(TimeFormat);

			if (txtRawFile.Text == "" || (rdoUserList.Checked && txtGlycanList.Text == "") || txtMaxLCTime.Text == "")
			{
				MessageBox.Show("Please check input values.");
				if (DoLog)
				{
					Logger.WriteLog("End Process- because input value not complete");
				}
				return;
			}

			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
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
				if (argLabeling.Count != 0)
				{
					ESI = new MultiGlycanESI(txtRawFile.Text, Convert.ToInt32(txtStartScan.Text), Convert.ToInt32(txtEndScan.Text), glycanlist, Convert.ToDouble(txtPPM.Text), Convert.ToDouble(txtGlycanPPM.Text), chkPermethylated.Checked, chkReducedReducingEnd.Checked, cboSia.SelectedIndex, argLabeling, AdductLabel, AdductMasses, DoLog, rdoPositive.Checked);
				}
				else
				{
					ESI = new MultiGlycanESI(txtRawFile.Text, Convert.ToInt32(txtStartScan.Text), Convert.ToInt32(txtEndScan.Text), glycanlist, Convert.ToDouble(txtPPM.Text), Convert.ToDouble(txtGlycanPPM.Text), chkPermethylated.Checked, chkReducedReducingEnd.Checked, cboSia.SelectedIndex, DoLog, rdoPositive.Checked);
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
				ESI.PositiveChargeMode = rdoPositive.Checked;
				ESI.MergeDifferentChargeIntoOne = chkMergeDffCharge.Checked;
				ESI.ExportFilePath = saveFileDialog1.FileName;
				ESI.MaxLCBackMin = Convert.ToSingle(txtMaxLCTime.Text);
				ESI.MaxLCFrontMin = Convert.ToSingle(txtMinLCTime.Text);
				ESI.IsotopePPM = Convert.ToSingle(txtIsotopeEnvTolerence.Text);
				ESI.MininumIsotopePeakCount = Convert.ToInt32(txtIsotopeEnvMinPeakCount.Text);
				ESI.PeakSNRatio = Convert.ToSingle(txtSN.Text);
				ESI.IsMatchMonoisotopicOnly = chkMonoOnly.Checked;
				ESI.ApplyLinearRegLC = chkApplyLinearRegLC.Checked;
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
				if (chkMZMatch.Checked)
				{
					ESI.IncludeMZMatch = true;
				}
				ESI.ForceProtonatedGlycan = chkForceProtonated.Checked;
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
				frmProcessing frmProcess = new frmProcessing(ESI, DoLog);
				frmProcess.ShowDialog();
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

		private void rdoAllRaw_CheckedChanged(object sender, EventArgs e)
		{
			rdoScanNum.Checked = !rdoAllRaw.Checked;
			txtStartScan.Enabled = !rdoAllRaw.Checked;
			txtEndScan.Enabled = !rdoAllRaw.Checked;
			txtStartScan.Text = "1";
			txtEndScan.Text = _endScan.ToString();
		}

		private void btnSetting_Click(object sender, EventArgs e)
		{
			frmPeakpara = new frmPeakParameters();
			frmPeakpara.ShowDialog();
			btnNext.Enabled = true;
			btnQuan.Enabled = true;
		}

		private void btnMergeTest_Click(object sender, EventArgs e)
		{
			GenerateImages.GenQuantImg(@"E:\!MultiGlycanbatch\1\1_Quant.csv", enumGlycanLabelingMethod.MultiplexPermethylated, @"E:\!MultiGlycanbatch\1");
			//StreamReader sr = new StreamReader(@"D:\Dropbox\for_Yunli_Hu\b1_19_1_07142012-121002 1349_FullList.csv");
			//string tmp = sr.ReadLine();
			//List<ClusteredPeak> clu = new List<ClusteredPeak>();
			//do
			//{
			//    tmp = sr.ReadLine();
			//    string[] tmpArray = tmp.Split(',');
			//    ClusteredPeak tnpCluPeak = new ClusteredPeak(Convert.ToInt32(tmpArray[1]));
			//    tnpCluPeak.StartTime = Convert.ToDouble(tmpArray[0]);
			//    tnpCluPeak.EndTime = Convert.ToDouble(tmpArray[0]);

			//    tnpCluPeak.EndScan = Convert.ToInt32(tmpArray[1]);
			//    tnpCluPeak.Intensity = Convert.ToSingle(tmpArray[2]);
			//    tnpCluPeak.GlycanComposition = new COL.GlycoLib.GlycanCompound(
			//                                                                    Convert.ToInt32(tmpArray[8]),
			//                                                                    Convert.ToInt32(tmpArray[9]),
			//                                                                    Convert.ToInt32(tmpArray[10]),
			//                                                                    Convert.ToInt32(tmpArray[11]));
			//    tnpCluPeak.Charge = Convert.ToInt32(Math.Ceiling(  Convert.ToSingle(tmpArray[12]) / Convert.ToSingle(tmpArray[3])));

			//    clu.Add(tnpCluPeak);
			//} while (!sr.EndOfStream);
			//sr.Close();
			////MultiNGlycanESI.MergeCluster(clu, 8.0);
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

		public string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

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

		#endregion Assembly Attribute Accessors

		private class PeaksFromResult
		{
			private float _Time;
			private float _Intensity;
			private string _Adduct;

			public float Time => _Time;

			public float Intensity => _Intensity;

			public string Adduct => _Adduct;

			public PeaksFromResult(float argTime, float argIntensity, string argAdduct)
			{
				_Time = argTime;
				_Intensity = argIntensity;
				_Adduct = argAdduct;
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				StreamReader sr = new StreamReader(openFileDialog1.FileName);
				sr.ReadLine();//Title

				Dictionary<string, List<PeaksFromResult>> dictAllPeaks = new Dictionary<string, List<PeaksFromResult>>();
				do
				{
					string[] tmpAry = sr.ReadLine().Split(',');
					PeaksFromResult PKResult = new PeaksFromResult(Convert.ToSingle(tmpAry[0]),
																											   Convert.ToSingle(tmpAry[3]),
																											   tmpAry[6]);
					string GlycanKey = tmpAry[5];

					if (!dictAllPeaks.ContainsKey(GlycanKey))
					{
						dictAllPeaks.Add(GlycanKey, new List<PeaksFromResult>());
					}
					dictAllPeaks[GlycanKey].Add(PKResult);
				} while (!sr.EndOfStream);

				foreach (string GKey in dictAllPeaks.Keys)
				{
					Dictionary<float, float> MergeIntensity = new Dictionary<float, float>();
					List<float> Time = new List<float>();
					foreach (PeaksFromResult PKR in dictAllPeaks[GKey])
					{
						if (!MergeIntensity.ContainsKey(PKR.Time))
						{
							MergeIntensity.Add(PKR.Time, 0);
						}
						MergeIntensity[PKR.Time] = MergeIntensity[PKR.Time] + PKR.Intensity;

						if (!Time.Contains(PKR.Time))
						{
							Time.Add(PKR.Time);
						}
					}

					//Merge Intensity
					Time.Sort();
					float[] ArryIntesity = new float[Time.Count];
					float[] ArryTime = Time.ToArray();
					for (int i = 0; i < Time.Count; i++)
					{
						ArryIntesity[i] = MergeIntensity[Time[i]];
					}

					List<float[]> PeaksTime = new List<float[]>();
					List<float[]> PeaksIntensity = new List<float[]>();
					do
					{
						//Iter to find peak
						int MaxIdx = FindMaxIdx(ArryIntesity);
						int PeakStart = MaxIdx - 1;
						if (PeakStart < 0)
						{
							PeakStart = 0;
						}
						int PeakEnd = MaxIdx + 1;
						if (PeakEnd > ArryTime.Length - 1)
						{
							PeakEnd = ArryTime.Length - 1;
						}
						//PeakStartPoint
						while (PeakStart > 0)
						{
							if (ArryTime[PeakStart] - ArryTime[PeakStart - 1] < 0.5 && ArryTime[MaxIdx] - ArryTime[PeakStart] < 5.0)
							{
								PeakStart = PeakStart - 1;
							}
							else
							{
								break;
							}
						}

						//PeakEndPoint
						while (PeakEnd < ArryTime.Length - 1)
						{
							if (ArryTime[PeakEnd + 1] - ArryTime[PeakEnd] < 0.5 && ArryTime[PeakEnd] - ArryTime[MaxIdx] < 5.0)
							{
								PeakEnd = PeakEnd + 1;
							}
							else
							{
								break;
							}
						}

						//Peak Array
						float[] PeakTime = new float[PeakEnd - PeakStart + 1];
						float[] PeakInt = new float[PeakEnd - PeakStart + 1];
						Array.Copy(ArryTime, PeakStart, PeakTime, 0, PeakEnd - PeakStart + 1);
						Array.Copy(ArryIntesity, PeakStart, PeakInt, 0, PeakEnd - PeakStart + 1);
						PeaksTime.Add(PeakTime);
						PeaksIntensity.Add(PeakInt);

						//MergeRest
						int SizeOfRestArray = ArryTime.Length - PeakEnd + PeakStart - 1;
						float[] NewArryTime = new float[SizeOfRestArray];
						float[] NewArryIntensity = new float[SizeOfRestArray];
						Array.Copy(ArryTime, 0, NewArryTime, 0, PeakStart);
						Array.Copy(ArryTime, PeakEnd + 1, NewArryTime, PeakStart, ArryTime.Length - 1 - PeakEnd);
						Array.Copy(ArryIntesity, 0, NewArryIntensity, 0, PeakStart);
						Array.Copy(ArryIntesity, PeakEnd + 1, NewArryIntensity, PeakStart, ArryTime.Length - 1 - PeakEnd);

						ArryTime = NewArryTime;
						ArryIntesity = NewArryIntensity;
					} while (ArryTime.Length != 0);
				}
			}
		}

		private int FindMaxIdx(float[] argArry)
		{
			int MaxIdx = 0;
			for (int i = 1; i < argArry.Length; i++)
			{
				if (argArry[i] > argArry[MaxIdx])
				{
					MaxIdx = i;
				}
			}
			return MaxIdx;
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

				if (chkQuan13CH3.Checked)
					labelingRatio.Add(GlycoLib.enumLabelingTag.MP_13CH3, Convert.ToSingle(txtQuan13CH3.Text));
				if (chkQuan13CHD2.Checked)
					labelingRatio.Add(GlycoLib.enumLabelingTag.MP_13CHD2, Convert.ToSingle(txtQuan13CHD2.Text));
				if (chkQuan13CD3.Checked)
					labelingRatio.Add(GlycoLib.enumLabelingTag.MP_13CD3, Convert.ToSingle(txtQuan13CD3.Text));
				if (chkQuanCH3.Checked)
					labelingRatio.Add(GlycoLib.enumLabelingTag.MP_CH3, Convert.ToSingle(txtQuanCH3.Text));
				if (chkQuanCH2D.Checked)
					labelingRatio.Add(GlycoLib.enumLabelingTag.MP_CH2D, Convert.ToSingle(txtQuanCH2D.Text));
				if (chkQuanCHD2.Checked)
					labelingRatio.Add(GlycoLib.enumLabelingTag.MP_CHD2, Convert.ToSingle(txtQuanCHD2.Text));
				if (chkQuanCD3.Checked)
					labelingRatio.Add(GlycoLib.enumLabelingTag.MP_CD3, Convert.ToSingle(txtQuanCD3.Text));
			}
			else
			{
				labelingRatio.Add(GlycoLib.enumLabelingTag.None, 1.0f);
			}

			PreparingMultiGlycan(labelingRatio);
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

		private void button3_Click(object sender, EventArgs e)
		{
			MassLib.ThermoRawReader Raw = new ThermoRawReader(@"E:\Dropbox\Chuan-Yih\PerMethQuant\RiboB_7plex_05302014.raw");
			List<MSScan> scans = Raw.ReadScans(766, 1066);
			List<MSPeak> Peaks = new List<MSPeak>();
			foreach (MSScan s in scans)
			{
				foreach (MSPeak p in s.MSPeaks)
				{
					if (p.ChargeState == 2)
					{
						Peaks.Add(p);
					}
					//if (p.MonoisotopicMZ >= 834.62 && p.MonoisotopicMZ <= 834.65)
					//{
					//    Peaks.Add(p);
					//}
				}
			}
		}

		private void batchModeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			frmBatch BatchMode = new frmBatch();
			this.Visible = false;
			BatchMode.ShowDialog();
			this.Visible = true;
		}

		private void rdoPositive_CheckedChanged(object sender, EventArgs e)
		{
			chkAdductK.Enabled = rdoPositive.Checked;
			chkAdductNa.Enabled = rdoPositive.Checked;
			chkAdductNH4.Enabled = rdoPositive.Checked;
			chkAdductUser.Enabled = rdoPositive.Checked;
		}
	}
}