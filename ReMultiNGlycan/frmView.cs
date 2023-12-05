using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace COL.MultiGlycan
{
	public partial class frmView : Form
	{
		private List<Color> LstColor;
		private DataTable dtPeakList;
		private Dictionary<string, Dictionary<float, List<string>>> dictValue;      //Key:Glycan  - Value:<Key: mz,  Value:time-intensity >
		private Dictionary<float, List<string>> mz2GlycanAdductCharge; // Key: m/z  Value: Glycan_Adduct_Charge
		private Dictionary<string, List<string>> MergeResult;

		public frmView()
		{
			InitializeComponent();

			this.Font = new System.Drawing.Font("Arial", 9);

			LstColor = new List<Color>();
			//zgcGlycan.IsEnableHZoom = false;
			//zgcGlycan.IsEnableVZoom = false;
			//zgcGlycan.IsEnableWheelZoom = false;

			//foreach (KnownColor knowColor in Enum.GetValues(typeof(KnownColor)))
			//{
			//    LstColor.Add( Color.FromKnownColor(knowColor));
			//}

			//zgcGlycan.PointValueFormat = "0.000";
			//zgcGlycan.PointDateFormat = "d";
			LstColor.Add(Color.DarkCyan);
			LstColor.Add(Color.DarkGoldenrod);
			LstColor.Add(Color.DarkGray);
			LstColor.Add(Color.DarkGreen);
			LstColor.Add(Color.DarkKhaki);
			LstColor.Add(Color.DarkMagenta);
			LstColor.Add(Color.DarkOliveGreen);
			LstColor.Add(Color.DarkOrchid);
			LstColor.Add(Color.DarkRed);
			LstColor.Add(Color.DarkSalmon);
			LstColor.Add(Color.DarkSeaGreen);
			LstColor.Add(Color.DarkSlateBlue);
			LstColor.Add(Color.DarkSlateGray);
			LstColor.Add(Color.DarkTurquoise);
			LstColor.Add(Color.DarkViolet);
			LstColor.Add(Color.DeepPink);
			LstColor.Add(Color.DeepSkyBlue);
		}

		private void btnLoad_Click(object sender, EventArgs e)
		{
			openFileDialog1.Filter = "Result File(_FullList.csv)|*_FullList.csv";

			if (openFileDialog1.ShowDialog() != DialogResult.OK)
			{
				return;
			}

			if (!File.Exists(openFileDialog1.FileName.Replace("_FullList", "")))
			{
				MessageBox.Show("Merge Result File Missing, Please put merge and full reuslt in the same folder");
				return;
			}
			dictValue = new Dictionary<string, Dictionary<float, List<string>>>();
			mz2GlycanAdductCharge = new Dictionary<float, List<string>>();
			cboGlycan.Items.Clear();

			ArrayList alstGlycans = new ArrayList();

			//Read Merge Result
			MergeResult = new Dictionary<string, List<string>>();
			StreamReader sr = new StreamReader(openFileDialog1.FileName.Replace("_FullList", ""));
			string[] tmp = null;
			string GlycanKeyIdx = "HexNac-Hex-deHex-NeuAc-NeuGc";
			Dictionary<string, int> dictTitle = new Dictionary<string, int>();
			do
			{
				tmp = sr.ReadLine().Split(',');
				if (!tmp[0].StartsWith("Start Time"))
				{
					continue;
				}
				for (int i = 0; i < tmp.Length; i++)
				{
					dictTitle.Add(tmp[i], i);
					if (tmp[i].ToLower().Contains("hexnac-hex-dehex-sia"))
					{
						GlycanKeyIdx = "HexNac-Hex-deHex-Sia";
					}
				}
				break;
			} while (true);

			while (!sr.EndOfStream)
			{
				tmp = sr.ReadLine().Split(',');
				string Key = tmp[dictTitle[GlycanKeyIdx]]; // hex-hexnac-dehax-sia
				string Time = tmp[dictTitle["Start Time"]] + ":" + tmp[dictTitle["End Time"]] + ":" + tmp[dictTitle["Peak Intensity"]];
				if (!MergeResult.ContainsKey(Key))
				{
					MergeResult.Add(Key, new List<string>());
				}
				MergeResult[Key].Add(Time);
			}

			sr.Close();

			//Read Full File
			sr = new StreamReader(openFileDialog1.FileName);
			tmp = sr.ReadLine().Split(',');
			dictTitle = new Dictionary<string, int>();
			//Get Title mapping
			for (int i = 0; i < tmp.Length; i++)
			{
				dictTitle.Add(tmp[i], i);
			}

			while (!sr.EndOfStream)
			{
				tmp = sr.ReadLine().Split(',');
				int Charge = Convert.ToInt32(Math.Round(Convert.ToSingle(tmp[dictTitle["Composition mono"]]) / Convert.ToSingle(tmp[dictTitle["m/z"]]), 0));
				string Key = tmp[dictTitle[GlycanKeyIdx]] + "-" + Charge.ToString(); // hex-hexnac-dehax-sia
				string Adduct = tmp[dictTitle["Adduct"]];
				if (chkMergeCharge.Checked)
				{
					Key = tmp[dictTitle[GlycanKeyIdx]];
				}
				if (!dictValue.ContainsKey(Key))
				{
					dictValue.Add(Key, new Dictionary<float, List<string>>());
					alstGlycans.Add(Key);
				}
				int mz = (int)Convert.ToSingle(tmp[dictTitle["m/z"]].ToString());

				if (!dictValue[Key].ContainsKey(mz))
				{
					dictValue[Key].Add(mz, new List<string>());
				}
				dictValue[Key][mz].Add(tmp[dictTitle["Time"]] + "-" + tmp[dictTitle["Abundance"]]); // scan time - abuntance
				if (!mz2GlycanAdductCharge.ContainsKey(mz))
				{
					mz2GlycanAdductCharge.Add(mz, new List<string>());
				}
				if (!mz2GlycanAdductCharge[mz].Contains(Key + " + " + Adduct + " z=" + Charge.ToString()))
				{
					mz2GlycanAdductCharge[mz].Add(Key + " + " + Adduct + " z=" + Charge.ToString()); //Glycan_Adduct_Charge
				}
			}

			sr.Close();
			alstGlycans.Sort();
			cboGlycan.Items.AddRange(alstGlycans.ToArray());
			cboGlycan.SelectedIndex = 0;

			btnSaveAll.Enabled = true;
		}

		private void cboGlycan_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (cboGlycan.SelectedItem == null)
			{
				return;
			}
			dtPeakList = new DataTable();
			DataColumn dcStartTime = new DataColumn("Start Time", typeof(Single));
			DataColumn dcEndTime = new DataColumn("End Time", typeof(Single));
			//DataColumn dcApex = new DataColumn("Apex", typeof(Single));
			//DataColumn dcPointCount = new DataColumn("Num. Pt", typeof(Int32));
			//DataColumn dcPeakArea = new DataColumn("Area", typeof(String));
			DataColumn dcPeakSumIntensity = new DataColumn("Sum Int.", typeof(String));
			dtPeakList.Columns.Add(dcStartTime);
			dtPeakList.Columns.Add(dcEndTime);
			//dtPeakList.Columns.Add(dcApex);
			//dtPeakList.Columns.Add(dcPointCount);
			//dtPeakList.Columns.Add(dcPeakArea);
			dtPeakList.Columns.Add(dcPeakSumIntensity);
			chkboxlstPeak.Items.Clear();
			btnUpdate_Click(sender, e);
		}

		private void rdoFullLC_CheckedChanged(object sender, EventArgs e)
		{
			if (rdoFullLC.Checked)
			{
				PlotFullLC();
			}
			else
			{
				cboGlycan_SelectedIndexChanged(sender, e);
			}
		}

		public void ReSizeZedGraphForm3DPlot(object sender, EventArgs e)
		{
			//zgcGlycan.MasterPane.PaneList[0].XAxis.Scale.Max = ((COL.ElutionViewer.RegionSize)e).RightBound;
			//zgcGlycan.MasterPane.PaneList[0].XAxis.Scale.Min = ((COL.ElutionViewer.RegionSize)e).LeftBound;
			//zgcGlycan.AxisChange();
		}

		private void chkMergeCharge_CheckedChanged(object sender, EventArgs e)
		{
			cboGlycan.Items.Clear();
		}

		private void DrawChart(string argKey)
		{
			Dictionary<float, List<string>> keyValue = dictValue[argKey];

			string[] tmpLst = null;

			#region chart

			if (cht.ChartAreas.Count == 0)
			{
				cht.ChartAreas.Add("Default");
			}
			cht.ChartAreas["Default"].AxisX.Title = "Scan time (min)";
			cht.ChartAreas["Default"].AxisX.TitleFont = new Font("Arial", 10, FontStyle.Bold);
			cht.ChartAreas["Default"].AxisX.LabelStyle.Format = "{F2}";
			cht.ChartAreas["Default"].AxisX.LabelStyle.Font = new Font("Arial", 10, FontStyle.Bold);
			cht.ChartAreas["Default"].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
			cht.ChartAreas["Default"].AxisX.IsMarginVisible = true;
			cht.ChartAreas["Default"].AxisY.Title = "Abundance";
			cht.ChartAreas["Default"].AxisY.TitleFont = new Font("Arial", 10, FontStyle.Bold);
			cht.ChartAreas["Default"].AxisY.LabelStyle.Format = "{0.#E+00}";
			cht.ChartAreas["Default"].AxisY.LabelStyle.Font = new Font("Arial", 10, FontStyle.Bold);
			cht.ChartAreas["Default"].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
			cht.ChartAreas["Default"].AxisY.IsMarginVisible = true;

			if (cht.Titles.Count == 0)
			{
				cht.Titles.Add("Default");
			}
			cht.Titles[0].Text = "Glycan: " + argKey;
			cht.Titles[0].Font = new Font("Arial", 16, FontStyle.Bold);
			if (cht.Legends.Count == 0)
			{
				cht.Legends.Add("Default");
			}
			cht.Legends["Default"].Docking = Docking.Bottom;
			cht.Legends["Default"].Alignment = StringAlignment.Center;
			cht.Legends["Default"].LegendStyle = LegendStyle.Row;
			cht.Legends["Default"].Font = new Font("Arial", 12, FontStyle.Bold);

			cht.Series.Clear();
			double MaxIntensity = 0.0;
			Dictionary<float, List<LCPointPair>> ChartRaw = new Dictionary<float, List<LCPointPair>>();
			Dictionary<float, float> _SumIntensities = new Dictionary<float, float>();
			List<float> lstmz = new List<float>();

			////Gather Data
			foreach (float mz in keyValue.Keys)
			{
				List<LCPointPair> RawPPL = new List<LCPointPair>();
				foreach (string tmp in keyValue[mz])
				{
					tmpLst = tmp.Split('-');
					float time = Convert.ToSingle(tmpLst[0]);
					float intensity = Convert.ToSingle(tmpLst[1]);

					RawPPL.Add(new LCPointPair(time, intensity));
					if (MaxIntensity <= Convert.ToDouble(tmpLst[1]))
					{
						MaxIntensity = Convert.ToDouble(tmpLst[1]);
					}
					if (!_SumIntensities.ContainsKey(time))
					{
						_SumIntensities.Add(time, 0.0f);
					}
					_SumIntensities[time] = _SumIntensities[time] + intensity;
				}
				Series series = cht.Series.Add(mz.ToString());
				series.ChartType = SeriesChartType.Spline;
				for (int i = 0; i < RawPPL.Count; i++)
				{
					series.Points.AddXY(RawPPL[i].Time, RawPPL[i].Intensity);
				}
				series.MarkerSize = 5;
				series.MarkerStyle = MarkerStyle.Circle;
				lstmz.Add(mz);
			}
			lstmz.Sort();

			//Create Check List Box
			if (!chkboxlstPeak.Items.Contains("Merge Smooth"))
			{
				chkboxlstPeak.Items.Add("Merge Smooth", true);
			}
			//if (!chkboxlstPeak.Items.Contains("Peak Area"))
			//{
			//    chkboxlstPeak.Items.Add("Peak Area", true);
			//}
			foreach (float mz in lstmz)
			{
				List<string> lstGlycanAdductCharges = mz2GlycanAdductCharge[mz];
				foreach (string GlycanAdductCharge in lstGlycanAdductCharges)
				{
					string[] tmpAry = GlycanAdductCharge.Split(' ');
					if (chkboxlstPeak.Items.Contains(mz.ToString() + " [" + tmpAry[0] + " + " + tmpAry[2] + "]" + tmpAry[3].Replace("z=", "") + "+"))
					{
						continue;
					}
					if (tmpAry[0] == argKey)
					{
						chkboxlstPeak.Items.Add(mz.ToString() + " [" + tmpAry[0] + " + " + tmpAry[2] + "]" + tmpAry[3].Replace("z=", "") + "+", true);
					}
				}
			}
			//Create Series

			//Smoothed
			List<MassLib.MSPoint> lstSmoothPnts = new List<MassLib.MSPoint>();
			foreach (float time in _SumIntensities.Keys)
			{
				lstSmoothPnts.Add(new MassLib.MSPoint(time, _SumIntensities[time]));
			}
			lstSmoothPnts.Sort(delegate (MassLib.MSPoint Pnt1, MassLib.MSPoint Pnt2)
			{
				return Pnt1.Mass.CompareTo(Pnt2.Mass);
			});
			lstSmoothPnts = MassLib.Smooth.SavitzkyGolay.Smooth(lstSmoothPnts, MassLib.Smooth.SavitzkyGolay.FILTER_WIDTH.FILTER_WIDTH_7);

			Series seriesSmooth = cht.Series.Add("Merge Smooth");
			seriesSmooth.ChartType = SeriesChartType.Spline;
			foreach (MassLib.MSPoint MSPoint in lstSmoothPnts)
			{
				seriesSmooth.Points.AddXY(MSPoint.Mass, MSPoint.Intensity);
			}
			seriesSmooth.Color = Color.DarkGreen;
			seriesSmooth.BorderWidth = 2;
			seriesSmooth.MarkerSize = 5;
			seriesSmooth.MarkerStyle = MarkerStyle.Circle;
			cht.Series["Merge Smooth"].Enabled = chkboxlstPeak.CheckedItems.Contains("Merge Smooth");

			//Add Each Charge and adduct
			//int ColorIdx = 0;
			foreach (float mz in lstmz)
			{
				string Key = "";
				//Get Key
				List<string> lstGlycanAdductCharges = mz2GlycanAdductCharge[mz];
				foreach (string GlycanAdductCharge in lstGlycanAdductCharges)
				{
					string[] tmpAry = GlycanAdductCharge.Split(' ');
					if (tmpAry[0] == argKey)
					{
						Key = mz.ToString() + " [" + tmpAry[0] + " + " + tmpAry[2] + "]" + tmpAry[3].Replace("z=", "") + "+";
					}
				}
				cht.Series[mz.ToString()].Enabled = chkboxlstPeak.CheckedItems.Contains(Key);
			}

			//Data Table
			dtPeakList.Rows.Clear();
			if (MergeResult.ContainsKey(argKey))
			{
				foreach (string strTmp in MergeResult[argKey])
				{
					string[] strArry = strTmp.Split(':');
					DataRow row = dtPeakList.NewRow();
					row[0] = Convert.ToSingle(strArry[0]);
					row[1] = Convert.ToSingle(strArry[1]);
					row[2] = strArry[2];
					dtPeakList.Rows.Add(row);
				}
			}
			else
			{
				DataRow row = dtPeakList.NewRow();
				row[0] = 0.0f;
				row[1] = 0.0f;
				row[2] = "No merged result";
				dtPeakList.Rows.Add(row);
			}

			#region Old Update Table

			// List<MassLib.LCPeak> lcPk = null;
			//if (dtPeakList.Rows.Count == 0) //Create Datatable automatically
			//{
			//    lcPk = MassLib.LCPeakDetection.PeakFinding(lstSmoothPnts, 0.1f, 0.01f);
			//    dtPeakList.Rows.Clear();
			//    lcPk.Sort(delegate(MassLib.LCPeak P1, MassLib.LCPeak P2) { return P1.MZ.CompareTo(P2.MZ); });

			//    int SumRawPoint = 0;
			//    double  SumPeakArea = 0.0;
			//    double SumPeakIntensity = 0.0;
			//    foreach (MassLib.LCPeak pk in lcPk)
			//    {
			//        DataRow row = dtPeakList.NewRow();
			//        row[0] = pk.StartTime;
			//        row[1] = pk.EndTime;
			//        row[2] = pk.Apex.Mass;
			//        row[3] = pk.RawPoint.Count;
			//        SumRawPoint = SumRawPoint + pk.RawPoint.Count;
			//        row[4] = pk.PeakArea;
			//        SumPeakArea = SumPeakArea + pk.PeakArea;
			//        row[5] = pk.SumOfIntensity;
			//        SumPeakIntensity = SumPeakIntensity + pk.SumOfIntensity;
			//        dtPeakList.Rows.Add(row);
			//    }
			//    DataRow SumRow = dtPeakList.NewRow();
			//    SumRow[3] = SumRawPoint;
			//    SumRow[4] = SumPeakArea;
			//    SumRow[5] = SumPeakIntensity;
			//    dtPeakList.Rows.Add(SumRow);
			//}
			//else //Update data table
			//{
			//    lcPk = new List<MassLib.LCPeak>();
			//    for (int i = 0; i < dtPeakList.Rows.Count-1; i++)
			//    {
			//        DataRow row = dtPeakList.Rows[i];
			//        List<COL.MassLib.MSPoint> MSPs = new List<MassLib.MSPoint>();
			//        //foreach (float time in _SumIntensities.Keys)
			//        foreach (MassLib.MSPoint pnt in lstSmoothPnts)
			//        {
			//            if (Convert.ToSingle(row[0]) <= pnt.Mass && pnt.Mass <= Convert.ToSingle(row[1]))
			//            {
			//                MSPs.Add(pnt);
			//            }
			//        }
			//        MSPs.Sort(delegate(MassLib.MSPoint P1, MassLib.MSPoint P2) { return P1.Mass.CompareTo(P2.Mass); });
			//        lcPk.Add(new MassLib.LCPeak(MSPs[0].Mass, MSPs[MSPs.Count - 1].Mass, MSPs));
			//    }
			//    dtPeakList.Rows.Clear();
			//    lcPk.Sort(delegate(MassLib.LCPeak P1, MassLib.LCPeak P2) { return P1.MZ.CompareTo(P2.MZ); });

			//    int SumRawPoint = 0;
			//    double SumPeakArea = 0.0;
			//    double SumPeakIntensity = 0.0;
			//    foreach (MassLib.LCPeak pk in lcPk)
			//    {
			//        DataRow row = dtPeakList.NewRow();
			//        row[0] = pk.StartTime;
			//        row[1] = pk.EndTime;
			//        row[2] = pk.Apex.Mass;
			//        row[3] = pk.RawPoint.Count;
			//        SumRawPoint = SumRawPoint + pk.RawPoint.Count;
			//        row[4] = pk.PeakArea;
			//        SumPeakArea = SumPeakArea + pk.PeakArea;
			//        row[5] = pk.SumOfIntensity;
			//        SumPeakIntensity = SumPeakIntensity + pk.SumOfIntensity;
			//        dtPeakList.Rows.Add(row);
			//    }
			//    DataRow SumRow = dtPeakList.NewRow();
			//    SumRow[3] = SumRawPoint;
			//    SumRow[4] = SumPeakArea;
			//    SumRow[5] = SumPeakIntensity;
			//    dtPeakList.Rows.Add(SumRow);
			//}

			#endregion Old Update Table

			dgvPeakList.DataSource = dtPeakList;
			dgvPeakList.Columns[0].DefaultCellStyle.Format = "0.###";
			dgvPeakList.Columns[0].Width = 50;
			dgvPeakList.Columns[1].DefaultCellStyle.Format = "0.###";
			dgvPeakList.Columns[1].Width = 50;
			//dgvPeakList.Columns[2].DefaultCellStyle.Format = "0.###";
			dgvPeakList.Columns[2].Width = 100;
			//dgvPeakList.Columns[3].Width = 40;

			//Change sum row to scientific notation
			//dgvPeakList.Rows[dgvPeakList.Rows.Count - 2].Cells[4].Value = string.Format("{0:E2}", Convert.ToDouble(dgvPeakList.Rows[dgvPeakList.Rows.Count - 2].Cells[4].Value));
			//dgvPeakList.Rows[dgvPeakList.Rows.Count - 2].Cells[5].Value = string.Format("{0:E2}", Convert.ToDouble(dgvPeakList.Rows[dgvPeakList.Rows.Count - 2].Cells[5].Value));

			//Merge Peak
			//int MergePeakIdx = 1;
			//foreach (MassLib.LCPeak pk in lcPk)
			//{
			//    PointPairList pplSmoothSumIntensity = new PointPairList();
			//    foreach (MassLib.MSPoint pek in lstSmoothPnts)
			//    {
			//        if (pek.Mass >= pk.StartTime && pek.Mass <= pk.EndTime)
			//        {
			//            pplSmoothSumIntensity.Add(new PointPair(pek.Mass, pek.Intensity));
			//        }
			//    }
			//    if (!chkboxlstPeak.CheckedItems.Contains("Merge Smooth"))
			//    {
			//        //Remove Merged
			//        if (GP.CurveList.Contains(GP.CurveList["Merge Smooth-" + MergePeakIdx.ToString()]))
			//        {
			//            GP.CurveList.Remove(GP.CurveList["Merge Smooth-" + MergePeakIdx.ToString()]);
			//        }
			//        //Remove first Merged peak
			//        if (GP.CurveList.Contains(GP.CurveList["Merge Smooth"]))
			//        {
			//            GP.CurveList.Remove(GP.CurveList["Merge Smooth"]);
			//        }
			//    }
			//    else
			//    {
			//        LineItem MergeSmoothLine = null;
			//        if (MergePeakIdx == 1)
			//        {
			//            MergeSmoothLine = GP.AddCurve("Merge Smooth", pplSmoothSumIntensity, Color.DarkGreen);
			//        }
			//        else
			//        {
			//            MergeSmoothLine = GP.AddCurve("Merge Smooth-"+MergePeakIdx.ToString(), pplSmoothSumIntensity, Color.DarkGreen);
			//        }
			//        MergeSmoothLine.Line.Width = 3.0f;
			//        MergeSmoothLine.Symbol.Size = 2.0f;
			//        if (MergePeakIdx != 1)
			//        {
			//            MergeSmoothLine.Label.IsVisible = false;
			//        }
			//    }
			//    MergePeakIdx++;
			//}

			//if (!chkboxlstPeak.CheckedItems.Contains("Peak Area"))
			//{
			//    if (cht.Series.Contains(cht.Series["Peak Area"]))
			//    {
			//        cht.Series.Remove(cht.Series["Peak Area"]);
			//    }
			//}
			//else
			//{
			//    if(MergeResult.ContainsKey(argKey))
			//    {
			//        //Plot Peak Area
			//        ColorIdx = 0;
			//        foreach (string strMerge in MergeResult[argKey])
			//        {
			//            List<LCPointPair> pplArea = new List<LCPointPair>();
			//            float StartTime = Convert.ToSingle(strMerge.Split(':')[0]);
			//            float EndTime = Convert.ToSingle(strMerge.Split(':')[1]);
			//            Series ItemPeakArea = cht.Series.Add("Peak Area");
			//            foreach (float time in _SumIntensities.Keys)
			//            {
			//                if (time >= StartTime && time <= EndTime)
			//                {
			//                    ItemPeakArea.Points.AddXY(time, _SumIntensities[time]);
			//                }
			//            }
			//            ItemPeakArea.Points.OrderBy(x => x.XValue);
			//            ItemPeakArea.Color = LstColor[ColorIdx];

			//            lItemPeakArea.Line.Fill = new Fill(Color.FromArgb(100, LstColor[ColorIdx].R, LstColor[ColorIdx].G, LstColor[ColorIdx].B));
			//            lItemPeakArea.Line.Color = Color.White;
			//            lItemPeakArea.Symbol.Fill.IsVisible = false;
			//            lItemPeakArea.Symbol.IsVisible = false;
			//            lItemPeakArea.Label.IsVisible = false;
			//            ColorIdx = (ColorIdx + 1) % 17;
			//        }
			//    }
			//}

			//    //Plot Apex
			//    PointPairList PeakApx = new PointPairList();
			//    foreach (MassLib.LCPeak pek in lcPk)
			//    {
			//        PeakApx.Add(new PointPair(pek.Apex.Mass, pek.Apex.Intensity));
			//    }
			//    LineItem PeakedPeak = GP.AddCurve("Peak Apex", PeakApx, Color.DarkOrange);
			//    PeakedPeak.Line.IsVisible = false;
			//    PeakedPeak.Symbol.Size = 10.0f;
			//    PeakedPeak.Symbol.Fill.IsVisible = true;
			//    PeakedPeak.Symbol.Fill.Color = Color.DarkOrange;
			//    PeakedPeak.Symbol.Type = SymbolType.Circle;
			//}

			#endregion chart
		}

		private int GetCheckListBoxIdx(string argKey)
		{
			for (int i = 0; i < chkboxlstPeak.Items.Count; i++)
			{
				if (chkboxlstPeak.Items[i].ToString() == argKey)
				{
					return i;
				}
			}
			return -1;
		}

		private void DrawE3D(string argKey)
		{
			COL.ElutionViewer.MSPointSet3D Eluction3DRaw = new ElutionViewer.MSPointSet3D();
			Dictionary<float, List<string>> keyValue = dictValue[argKey];
			string[] tmpLst = null;
			foreach (float mz in keyValue.Keys)
			{
				foreach (string tmp in keyValue[mz])
				{
					tmpLst = tmp.Split('-');
					float time = Convert.ToSingle(tmpLst[0]);
					float intensity = Convert.ToSingle(tmpLst[1]);
					Eluction3DRaw.Add(time, mz, intensity);
				}
			}
			eluctionViewer1.SetData(COL.ElutionViewer.EViewerHandler.Create3DHandler(Eluction3DRaw));
		}

		private void btnUpdate_Click(object sender, EventArgs e)
		{
			if (cboGlycan.SelectedItem == null)
			{
				return;
			}
			string key = cboGlycan.SelectedItem.ToString();
			DrawChart(key);
			DrawE3D(key);
		}

		private void PlotFullLC()
		{
			//ZedGraph.GraphPane zgp = zgcGlycan.GraphPane;
			//zgp.CurveList.Clear();
			//foreach (string key in dictValue.Keys)
			//{
			//    if (dictValue[key].Count == 1)
			//    {
			//        continue;
			//    }
			//    PointPairList ppl = new PointPairList();
			//    string[] tmpLst = null;
			//    double mz = Convert.ToDouble(dictValue[key][0].Split('-')[2]);
			//    foreach (string tmp in  dictValue[key])
			//    {
			//        tmpLst = tmp.Split('-');
			//        ppl.Add(new PointPair(Convert.ToDouble(tmpLst[0]),
			//                                                mz));
			//    }
			//    TextObj txtLabel = new TextObj(key,ppl[ppl.Count-1].X,ppl[ppl.Count-1].Y);
			//    txtLabel.FontSpec.Border.IsVisible = false;
			//    txtLabel.FontSpec.Fill.IsVisible = false;
			//    txtLabel.Location.AlignH = AlignH.Left;
			//    txtLabel.Location.AlignV = AlignV.Center;
			//    txtLabel.FontSpec.Size = 5.0f;
			//    zgp.GraphObjList.Add(txtLabel);
			//    zgp.AddCurve(key, ppl, Color.Blue,SymbolType.None);
			//    ((ZedGraph.LineItem)zgp.CurveList[zgp.CurveList.Count - 1]).Line.Width = 3.0f;
			//}
			//zgp.Legend.IsVisible = false;
			//zgp.AxisChange();
		}

		private void btnSaveAll_Click(object sender, EventArgs e)
		{
			//string strPath = Path.GetDirectoryName(openFileDialog1.FileName) + "\\Glycan_EluteProfile(" + Path.GetFileNameWithoutExtension(openFileDialog1.FileName) + ")";
			//if (!Directory.Exists(strPath))
			//{
			//    Directory.CreateDirectory(strPath);
			//}
			//for (int i = 0; i < cboGlycan.Items.Count; i++)
			//{
			//    string key = cboGlycan.Items[i].ToString();
			//    List<string> keyValue = dictValue[key];
			//    PointPairList ppl = new PointPairList();
			//    string[] tmpLst = null;
			//    foreach (string tmp in keyValue)
			//    {
			//        tmpLst = tmp.Split('-');
			//        ppl.Add(new PointPair(Convert.ToDouble(tmpLst[0]),
			//                                                Convert.ToDouble(tmpLst[1])));
			//    }
			//    List<COL.MassLib.MSPeak> tmpPeak = new List<COL.MassLib.MSPeak>();
			//    foreach (string tmp in keyValue)
			//    {
			//        tmpLst = tmp.Split('-');
			//        tmpPeak.Add(new COL.MassLib.MSPeak(Convert.ToSingle(tmpLst[0]),
			//                                                Convert.ToSingle(tmpLst[1])));
			//    }
			//    List<COL.MassLib.MSPeak> Smoothed = COL.MassLib.Smooth.SavitzkyGolay.Smooth(tmpPeak, COL.MassLib.Smooth.SavitzkyGolay.FILTER_WIDTH.FILTER_WIDTH_11);
			//    SmoothPPL = new PointPairList();
			//    for (int j = 0; j < Smoothed.Count; j++)
			//    {
			//        SmoothPPL.Add(new PointPair(Convert.ToDouble(ppl[j].X),
			//                            Convert.ToDouble(Smoothed[j].MonoIntensity)));
			//    }
			//    GraphPane GP = zgcGlycan.GraphPane;
			//    GP.Title.Text = "Glycan: " + key;
			//    GP.XAxis.Title.Text = "Time";
			//    GP.YAxis.Title.Text = "Abundance";
			//    GP.CurveList.Clear();
			//    GP.AddCurve(key, ppl, Color.Blue, SymbolType.Circle);
			//    LineItem SmoothLine = GP.AddCurve("Smooth", SmoothPPL, Color.Red, SymbolType.XCross);
			//    SmoothLine.Symbol.Size = 5.0f;
			//    SmoothLine.Line.Width = 2.0f;
			//    GP.AxisChange();
			//    GP.GetImage().Save(strPath + "\\" + key + ".png", System.Drawing.Imaging.ImageFormat.Png);
			//}
			////Export Merge Result
			//openFileDialog1.Title = "Load Merge Result";
			//if(openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			//{
			//    StreamReader sr = new StreamReader(openFileDialog1.FileName);
			//    sr.ReadLine(); // title
			//    string PreviousKey ="";
			//    string Key ;
			//    List<PointPairList> lstPPL = new List<PointPairList>();
			//    do
			//    {
			//        string[] tmpAry = sr.ReadLine().Split(',');
			//        Key = Convert.ToInt32(tmpAry[6].Trim()).ToString("00") + "-" + Convert.ToInt32(tmpAry[7]).ToString("00") + "-" + Convert.ToInt32(tmpAry[8]).ToString("00") + "-" + Convert.ToInt32(tmpAry[9]).ToString("00"); // hex-hexnac-dehax-sia
			//        if (Key == "06-06-00-00")
			//        {
			//            Console.Write("aaa");

			//        }
			//        double startTime = Convert.ToDouble(tmpAry[0]);
			//        double endTime = Convert.ToDouble(tmpAry[1]);
			//        if (PreviousKey == "")
			//        {
			//            PreviousKey = Key;
			//        }
			//        if(Key!= PreviousKey) //Export
			//        {
			//            GraphPane GP = zgcGlycan.GraphPane;
			//            GP.Title.Text = "Glycan: " + PreviousKey;
			//            GP.XAxis.Title.Text = "Time";
			//            GP.YAxis.Title.Text = "Abundance";
			//            GP.CurveList.Clear();
			//            for (int i = 0; i < lstPPL.Count; i++)
			//            {
			//                PointPairList ppl = lstPPL[i];
			//                GP.AddCurve("Cluster-" + i.ToString() + "(" + ppl[0].X.ToString("00.00") + "-" + ppl[ppl.Count - 1].X.ToString("00.00") + ")", ppl, Color.Blue, SymbolType.Circle);
			//            }
			//            GP.AxisChange();
			//            GP.GetImage().Save(strPath + "\\Merge_" + PreviousKey + ".png", System.Drawing.Imaging.ImageFormat.Png);
			//            lstPPL.Clear();
			//            PreviousKey = Key;
			//        }
			//        PointPairList tmpppl = new PointPairList();
			//        for(int i = 1;i<=5;i++)
			//        {
			//            string KeyPlusCharge = Key+"-"+i.ToString();
			//            if(dictValue.ContainsKey(KeyPlusCharge))
			//            {
			//                List<string> keyValue = dictValue[KeyPlusCharge];
			//                string[] tmpLst;
			//                foreach (string tmp in keyValue)
			//                {
			//                    tmpLst = tmp.Split('-');
			//                    double time =Convert.ToDouble(tmpLst[0]);
			//                    if (startTime <= time && time <= endTime)
			//                    {
			//                        tmpppl.Add(new PointPair(time,
			//                                                                Convert.ToDouble(tmpLst[1])));
			//                    }
			//                }
			//            }
			//        }
			//        tmpppl.Sort();
			//        lstPPL.Add(tmpppl);
			//    }while (!sr.EndOfStream);
			//    sr.Close();

			//    GraphPane lstGP = zgcGlycan.GraphPane;
			//    lstGP.Title.Text = "Glycan: " + Key;
			//    lstGP.XAxis.Title.Text = "Time";
			//    lstGP.YAxis.Title.Text = "Abundance";
			//    lstGP.CurveList.Clear();
			//    for (int i = 0; i < lstPPL.Count; i++)
			//    {
			//        PointPairList ppl = lstPPL[i];
			//        lstGP.AddCurve("Cluster-" + i.ToString() + "(" + ppl[0].X.ToString("00.00") + "-" + ppl[ppl.Count - 1].X.ToString("00.00") + ")", ppl, Color.Blue, SymbolType.Circle);
			//    }
			//    lstGP.AxisChange();
			//    lstGP.GetImage().Save(strPath + "\\Merge_" + Key + ".png", System.Drawing.Imaging.ImageFormat.Png);
			//    lstPPL.Clear();
			//}
			//MessageBox.Show("Done");
		}

		private void btnSaveWholeProfile_Click(object sender, EventArgs e)
		{
			//if (dictValue.Keys.Count == 0)
			//{
			//    return;
			//}
			//string strPath = Path.GetDirectoryName(openFileDialog1.FileName) + "\\Glycan_EluteProfile(" + Path.GetFileNameWithoutExtension(openFileDialog1.FileName) + ")";
			//if (!Directory.Exists(strPath))
			//{
			//    Directory.CreateDirectory(strPath);
			//}
			//ZedGraph.GraphPane zgp = new GraphPane(new RectangleF(0, 0, 5000,5000), Path.GetFileNameWithoutExtension(openFileDialog1.FileName), "Time(min)", "m/z");
			//zgp.CurveList.Clear();
			//foreach (string key in dictValue.Keys)
			//{
			//    if (dictValue[key].Count == 1)
			//    {
			//        continue;
			//    }
			//    PointPairList ppl = new PointPairList();
			//    string[] tmpLst = null;
			//    double mz = Convert.ToDouble(dictValue[key][0].Split('-')[2]);
			//    foreach (string tmp in dictValue[key])
			//    {
			//        tmpLst = tmp.Split('-');
			//        ppl.Add(new PointPair(Convert.ToDouble(tmpLst[0]),
			//                                                mz));
			//    }
			//    TextObj txtLabel = new TextObj(key, ppl[ppl.Count - 1].X, ppl[ppl.Count - 1].Y);
			//    txtLabel.FontSpec.Border.IsVisible = false;
			//    txtLabel.FontSpec.Fill.IsVisible = false;
			//    txtLabel.Location.AlignH = AlignH.Left;
			//    txtLabel.Location.AlignV = AlignV.Center;
			//    txtLabel.FontSpec.Size = 5.0f;
			//    zgp.GraphObjList.Add(txtLabel);
			//    zgp.AddCurve(key, ppl, Color.Blue, SymbolType.None);
			//    ((ZedGraph.LineItem)zgp.CurveList[zgp.CurveList.Count - 1]).Line.Width = 3.0f;
			//}
			//zgp.Legend.IsVisible = false;
			//zgp.AxisChange();

			//zgp.GetImage().Save(strPath + "\\Eluction_Profile.png", System.Drawing.Imaging.ImageFormat.Png);
		}

		private void chkboxlstPeak_SelectedIndexChanged(object sender, EventArgs e)
		{
			btnUpdate_Click(this, e);
		}

		private void chkAdductPeak_CheckedChanged(object sender, EventArgs e)
		{
			for (int i = 0; i < chkboxlstPeak.Items.Count; i++)
			{
				if (chkboxlstPeak.Items[i].ToString() != "Merge Smooth" && chkboxlstPeak.Items[i].ToString() != "Peak Area")
				{
					if (chkAdductPeak.Checked)
					{
						chkboxlstPeak.SetItemCheckState(i, CheckState.Checked);
					}
					else
					{
						chkboxlstPeak.SetItemCheckState(i, CheckState.Unchecked);
					}
				}
			}
			btnUpdate_Click(this, e);
		}

		private void mergeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (dgvPeakList.SelectedRows.Count > 1)
			{
				int StartTimeIdx = 0;
				int EndTimeIdx = 0;

				for (int i = 0; i < dgvPeakList.SelectedRows.Count; i++)
				{
					if (Convert.ToSingle(dgvPeakList.SelectedRows[i].Cells[0].Value) <= Convert.ToSingle(dgvPeakList.SelectedRows[StartTimeIdx].Cells[0].Value))
					{
						StartTimeIdx = i;
					}
					if (Convert.ToSingle(dgvPeakList.SelectedRows[i].Cells[1].Value) >= Convert.ToSingle(dgvPeakList.SelectedRows[EndTimeIdx].Cells[0].Value))
					{
						EndTimeIdx = i;
					}
				}

				//Update  End time to Start row
				dgvPeakList.SelectedRows[StartTimeIdx].Cells[1].Value = Convert.ToSingle(dgvPeakList.SelectedRows[EndTimeIdx].Cells[1].Value);

				float StartTime = Convert.ToSingle(dgvPeakList.SelectedRows[StartTimeIdx].Cells[0].Value);

				//Delete
				do
				{
					if (Convert.ToSingle(dgvPeakList.SelectedRows[0].Cells[0].Value) == StartTime)
					{
						dgvPeakList.Rows[dgvPeakList.Rows.IndexOf(dgvPeakList.SelectedRows[0])].Selected = false;
					}
					else
					{
						dgvPeakList.Rows.Remove(dgvPeakList.SelectedRows[0]);
					}
				} while (dgvPeakList.SelectedRows.Count != 0);
				btnUpdate_Click(sender, e);
			}
		}

		private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (dgvPeakList.SelectedRows.Count <= 0)
			{
				return;
			}
			do
			{
				dgvPeakList.Rows.Remove(dgvPeakList.SelectedRows[0]);
			} while (dgvPeakList.SelectedRows.Count != 0);
			btnUpdate_Click(sender, e);
		}

		private void chkGetAbundance_CheckedChanged(object sender, EventArgs e)
		{
			eluctionViewer1.UseMousrGetAbundance = chkGetAbundance.Checked;
		}

		public struct LCPointPair
		{
			private double _time;
			private double _intensity;

			public LCPointPair(double argTime, double argIntensity)
			{
				_time = argTime;
				_intensity = argIntensity;
			}

			public double Time
			{
				get => _time;
				set => _time = value;
			}

			public double Intensity
			{
				get => _intensity;
				set => _intensity = value;
			}
		}
	}
}