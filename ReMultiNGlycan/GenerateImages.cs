using COL.GlycoLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace COL.MultiGlycan
{
	public static class GenerateImages
	{
		private static readonly int MaxDegreeParallelism = 8;

		public static void GenGlycanLcImg(MultiGlycanESI argMultiGlycanESI)
		{
			string dir = argMultiGlycanESI.ExportFilePath + "\\Pic";
			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}
			var LstColor = new List<Color>() { Color.DarkCyan, Color.DarkGoldenrod, Color.DarkGray, Color.DarkGreen, Color.DarkKhaki, Color.DarkMagenta, Color.DarkOliveGreen, Color.DarkOrchid, Color.DarkRed, Color.DarkSalmon, Color.DarkSeaGreen, Color.DarkSlateBlue, Color.DarkSlateGray, Color.DarkTurquoise, Color.DarkViolet, Color.DeepPink, Color.DeepSkyBlue };
			var dictCluster = new Dictionary<string, List<ClusteredPeak>>();
			foreach (var clsPeak in argMultiGlycanESI.MergedResultList)
			{
				string Key = clsPeak.GlycanKey;
				if (argMultiGlycanESI.LabelingMethod != enumGlycanLabelingMethod.None)
				{
					Key = clsPeak.GlycanKey + "-" + clsPeak.GlycanComposition.LabelingTag;
				}
				if (!dictCluster.ContainsKey(Key))
				{
					dictCluster.Add(Key, new List<ClusteredPeak>());
				}
				dictCluster[Key].Add(clsPeak);
			}
			//Parallel.ForEach(dictCluster.Keys, new ParallelOptions() {MaxDegreeOfParallelism = MaxDegreeParallelism},Gkey =>
			foreach (string Gkey in dictCluster.Keys)
			{
				string ProcessingGlycanKey = "";
				try
				{
					var dictAdductPoints = new Dictionary<string, List<LCPointPair>>();
					var MergeIntensity = new Dictionary<float, float>();
					var Time = new List<float>();
					float maxIntensity = 0;
					var lstPeakMargin = new List<Tuple<double, double>>();
					foreach (var clsPeak in dictCluster[Gkey])
					{
						lstPeakMargin.Add(new Tuple<double, double>(clsPeak.StartTime, clsPeak.EndTime));
						foreach (var Peak in clsPeak.MatchedPeaksInScan)
						{
							if (!dictAdductPoints.ContainsKey(Peak.AdductString))
							{
								dictAdductPoints.Add(Peak.AdductString, new List<LCPointPair>());
							}
							float TimeKey = Convert.ToSingle(Peak.ScanTime.ToString("0.00"));

							dictAdductPoints[Peak.AdductString].Add(new LCPointPair(Peak.ScanTime, Peak.MSPoints[0].Intensity));
							if (!MergeIntensity.ContainsKey(TimeKey))
							{
								MergeIntensity.Add(TimeKey, 0);
							}
							MergeIntensity[TimeKey] = MergeIntensity[TimeKey] + Peak.MSPoints[0].Intensity;
							if (maxIntensity <= MergeIntensity[TimeKey])
							{
								maxIntensity = MergeIntensity[TimeKey];
							}
							if (!Time.Contains(TimeKey))
							{
								Time.Add(TimeKey);
							}
						}
					}

					#region LC Images

					using (var cht = new Chart())
					{
						//---------------Generate Graph-----------------
						ProcessingGlycanKey = Gkey;

						cht.Size = new Size(2400, 1200);

						cht.ChartAreas.Add("Default");
						cht.ChartAreas["Default"].AxisX.Title = "Scan time (min)";
						cht.ChartAreas["Default"].AxisX.TitleFont = new Font("Arial", 24, FontStyle.Bold);
						cht.ChartAreas["Default"].AxisX.LabelStyle.Format = "{F2}";
						cht.ChartAreas["Default"].AxisX.LabelStyle.Font = new Font("Arial", 24, FontStyle.Bold);
						cht.ChartAreas["Default"].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
						cht.ChartAreas["Default"].AxisX.IsMarginVisible = true;
						cht.ChartAreas["Default"].AxisY.Title = "Abundance";
						cht.ChartAreas["Default"].AxisY.TitleFont = new Font("Arial", 24, FontStyle.Bold);
						cht.ChartAreas["Default"].AxisY.LabelStyle.Format = "{0.#E+00}";
						cht.ChartAreas["Default"].AxisY.LabelStyle.Font = new Font("Arial", 24, FontStyle.Bold);
						cht.ChartAreas["Default"].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
						cht.ChartAreas["Default"].AxisY.IsMarginVisible = true;

						cht.Titles.Add("Default");
						cht.Titles[0].Text = "Glycan: " + Gkey;
						cht.Titles[0].Font = new Font("Arial", 24, FontStyle.Bold);

						cht.Legends.Add("Default");
						cht.Legends["Default"].Docking = Docking.Bottom;
						cht.Legends["Default"].Alignment = StringAlignment.Center;
						cht.Legends["Default"].LegendStyle = LegendStyle.Row;
						cht.Legends["Default"].Font = new Font("Arial", 24, FontStyle.Bold);
						int ColorIdx = 0;
						// int SymbolIdx = 0;

						foreach (string Adduct in dictAdductPoints.Keys)
						{
							dictAdductPoints[Adduct].OrderBy(x => x.Time); //Sort by time
							var series = cht.Series.Add(Adduct);
							series.ChartArea = "Default";
							series.ChartType = SeriesChartType.Spline;
							series.MarkerStyle = MarkerStyle.Circle;
							series.MarkerSize = 15;
							series.BorderWidth = 2;
							series.Color = LstColor[ColorIdx];

							foreach (var pp in dictAdductPoints[Adduct])
							{
								float TimeKey = Convert.ToSingle(pp.Time.ToString("0.00"));
								if (series.Points.Where(x => x.XValue == TimeKey).Count() != 0)
								{
									series.Points.Where(x => x.XValue == TimeKey).ToList()[0].YValues[0] += pp.Intensity;
								}
								else
								{
									series.Points.AddXY(TimeKey, pp.Intensity);
								}
							}
							series.Points.OrderBy(x => x.XValue);
							ColorIdx = (ColorIdx + 1) % LstColor.Count;
						}
						//Merge Intensity
						Time.Sort();
						var PPLMerge = new List<LCPointPair>();
						foreach (float tim in Time)
						{
							PPLMerge.Add(new LCPointPair(Convert.ToSingle(tim.ToString("0.00")), MergeIntensity[tim]));
						}

						var merge = cht.Series.Add("Merge");
						merge.ChartArea = "Default";
						merge.ChartType = SeriesChartType.Spline;
						merge.BorderWidth = 2;
						merge.MarkerStyle = MarkerStyle.Diamond;
						merge.MarkerSize = 15;
						merge.Color = Color.Black;
						merge.BorderDashStyle = ChartDashStyle.Dot;
						foreach (var pp in PPLMerge)
						{
							merge.Points.AddXY(pp.Time, pp.Intensity);
						}
						merge.Points.OrderBy(x => x.XValue);
						cht.SaveImage(dir + "\\" + Gkey + ".png", ChartImageFormat.Png);
					}

					#endregion LC Images
				}
				catch (Exception ex)
				{
					throw new Exception("GetLC Pic failed " + ProcessingGlycanKey + "  Err Msg:" + ex);
				}
			}//);
		}

		public static void GenGlycanLcImg(string argAllFile, string argExportFolder, out List<string> errorMsgs)
		{
			string Dir = argExportFolder + "\\Pic";
			if (!Directory.Exists(Dir))
			{
				Directory.CreateDirectory(Dir);
			}
			var LstColor = new List<Color>() { Color.DarkCyan, Color.DarkGoldenrod, Color.DarkGray, Color.DarkGreen, Color.DarkKhaki, Color.DarkMagenta, Color.DarkOliveGreen, Color.DarkOrchid, Color.DarkRed, Color.DarkSalmon, Color.DarkSeaGreen, Color.DarkSlateBlue, Color.DarkSlateGray, Color.DarkTurquoise, Color.DarkViolet, Color.DeepPink, Color.DeepSkyBlue };
			//List<ZedGraph.SymbolType> LstSymbol = new List<ZedGraph.SymbolType>() { SymbolType.Circle, SymbolType.Triangle, SymbolType.TriangleDown, SymbolType.XCross, SymbolType.Diamond, SymbolType.Plus, SymbolType.Square, SymbolType.Star, SymbolType.VDash };

			//Get Title
			var dictTitle = new Dictionary<string, int>();
			var sr = new StreamReader(argAllFile);
			string tmp = "";
			bool isLabeling = false;
			tmp = sr.ReadLine();
			for (int i = 0; i < tmp.Split(',').Length; i++)
			{
				dictTitle.Add(tmp.Split(',')[i], i);
			}
			if (dictTitle.ContainsKey("Label Tag"))
			{
				isLabeling = true;
			}
			var dictData =
				new Dictionary<string, Dictionary<string, Dictionary<float, float>>>();
			//                Key-Label_Tag,      Adduct                      time    , intensity
			do
			{
				tmp = sr.ReadLine();
				if (tmp == null)
				{
					break;
				}
				string[] tmpAry = tmp.Split(',');
				string GlycanKey = tmpAry[dictTitle["HexNac-Hex-deHex-NeuAc-NeuGc"]];
				string Adduct = "";
				if (isLabeling)
				{
					GlycanKey = GlycanKey + "-" + tmpAry[dictTitle["Label Tag"]];
				}
				for (int i = 0; i < tmpAry[dictTitle["Adduct"]].Trim().Split(';').Length; i++)
				{
					if (tmpAry[dictTitle["Adduct"]].Trim().Split(';')[i] == "")
					{
						continue;
					}
					Adduct = Adduct + tmpAry[dictTitle["Adduct"]].Trim().Split(';')[i].Trim().Split(' ')[0] + "+";
				}
				Adduct = Adduct.Substring(0, Adduct.Length - 1);

				float time = Convert.ToSingle(tmpAry[dictTitle["Time"]]);
				float intensity = Convert.ToSingle(tmpAry[dictTitle["Abundance"]]);

				if (!dictData.ContainsKey(GlycanKey))
				{
					dictData.Add(GlycanKey, new Dictionary<string, Dictionary<float, float>>());
				}
				if (!dictData[GlycanKey].ContainsKey(Adduct))
				{
					dictData[GlycanKey].Add(Adduct, new Dictionary<float, float>());
				}
				if (!dictData[GlycanKey][Adduct].ContainsKey(time))
				{
					dictData[GlycanKey][Adduct].Add(time, 0);
				}
				dictData[GlycanKey][Adduct][time] = dictData[GlycanKey][Adduct][time] + intensity;
			} while (!sr.EndOfStream);
			sr.Close();

			string ProcessingGlycanKey = "";

			#region Get Data

			var imgErrorMsg = new List<string>();
			//foreach (string Gkey in dictData.Keys)
			Parallel.ForEach(dictData.Keys, new ParallelOptions() { MaxDegreeOfParallelism = MaxDegreeParallelism }, Gkey =>
			{
				try
				{
					var dictAdductPoints = new Dictionary<string, List<LCPointPair>>();
					var MergeIntensity = new Dictionary<float, float>();
					var Time = new List<float>();

					foreach (string adductKey in dictData[Gkey].Keys)
					{
						string adduct = "";
						adduct = adduct + adductKey + "+";
						adduct = adduct.Substring(0, adduct.Length - 1);
						if (!dictAdductPoints.ContainsKey(adduct))
						{
							dictAdductPoints.Add(adduct, new List<LCPointPair>());
						}
						foreach (float TimeKey in dictData[Gkey][adductKey].Keys)
						{
							dictAdductPoints[adduct].Add(new LCPointPair(TimeKey, dictData[Gkey][adductKey][TimeKey]));
							if (!MergeIntensity.ContainsKey(TimeKey))
							{
								MergeIntensity.Add(TimeKey, 0);
							}
							MergeIntensity[TimeKey] = MergeIntensity[TimeKey] + dictData[Gkey][adductKey][TimeKey];

							if (!Time.Contains(TimeKey))
							{
								Time.Add(TimeKey);
							}
						}
					}

					#endregion Get Data

					#region LC Images

					using (var cht = new Chart())
					{
						ProcessingGlycanKey = Gkey;

						cht.Size = new Size(2400, 1200);

						cht.ChartAreas.Add("Default");
						cht.ChartAreas["Default"].AxisX.Title = "Scan time (min)";
						cht.ChartAreas["Default"].AxisX.TitleFont = new Font("Arial", 24, FontStyle.Bold);
						cht.ChartAreas["Default"].AxisX.LabelStyle.Format = "{F2}";
						cht.ChartAreas["Default"].AxisX.LabelStyle.Font = new Font("Arial", 24, FontStyle.Bold);
						cht.ChartAreas["Default"].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
						cht.ChartAreas["Default"].AxisX.IsMarginVisible = true;
						cht.ChartAreas["Default"].AxisY.Title = "Abundance";
						cht.ChartAreas["Default"].AxisY.TitleFont = new Font("Arial", 24, FontStyle.Bold);
						cht.ChartAreas["Default"].AxisY.LabelStyle.Format = "{0.#E+00}";
						cht.ChartAreas["Default"].AxisY.LabelStyle.Font = new Font("Arial", 24, FontStyle.Bold);
						cht.ChartAreas["Default"].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
						cht.ChartAreas["Default"].AxisY.IsMarginVisible = true;

						cht.Titles.Add("Default");
						cht.Titles[0].Text = "Glycan: " + Gkey;
						cht.Titles[0].Font = new Font("Arial", 24, FontStyle.Bold);

						cht.Legends.Add("Default");
						cht.Legends["Default"].Docking = Docking.Bottom;
						cht.Legends["Default"].Alignment = StringAlignment.Center;
						cht.Legends["Default"].LegendStyle = LegendStyle.Row;
						cht.Legends["Default"].Font = new Font("Arial", 24, FontStyle.Bold);
						//---------------Generate Graph-----------------
						int ColorIdx = 0;
						//int SymbolIdx = 0;
						foreach (string Adduct in dictAdductPoints.Keys)
						{
							dictAdductPoints[Adduct].OrderBy(x => x.Time);
							var Times = new List<double>();
							var Intensities = new List<double>();
							foreach (var pp in dictAdductPoints[Adduct])
							{
								if (Times.Contains(pp.Time))
								{
									int idx = Times.IndexOf(pp.Time);
									Intensities[idx] = Intensities[idx] + pp.Intensity;
								}
								else
								{
									Times.Add(pp.Time);
									Intensities.Add(pp.Intensity);
								}
							}
							var series = cht.Series.Add(Adduct);
							series.ChartArea = "Default";
							series.ChartType = SeriesChartType.Spline;
							series.MarkerStyle = MarkerStyle.Circle;
							series.MarkerSize = 15;
							series.BorderWidth = 2;
							series.Color = LstColor[ColorIdx];
							for (int i = 0; i < Times.Count; i++)
							{
								series.Points.AddXY(Times[i], Intensities[i]);
							}

							ColorIdx = ColorIdx + 1;
						}
						//Merge Intensity
						Time.Sort();
						var PPLMerge = new List<LCPointPair>();
						foreach (float tim in Time)
						{
							PPLMerge.Add(new LCPointPair(Convert.ToSingle(tim.ToString("0.00")), MergeIntensity[tim]));
						}
						var merge = cht.Series.Add("Merge");
						merge.ChartArea = "Default";
						merge.ChartType = SeriesChartType.Spline;
						merge.BorderWidth = 2;
						merge.Color = Color.Black;
						merge.MarkerStyle = MarkerStyle.Diamond;
						merge.MarkerSize = 15;

						foreach (var pp in PPLMerge)
						{
							merge.Points.AddXY(pp.Time, pp.Intensity);
						}
						cht.SaveImage(Dir + "\\" + Gkey + ".png", ChartImageFormat.Png);
					}

					#endregion LC Images
				}
				catch (Exception ex)
				{
					var st = new StackTrace(ex, true);
					// Get the top stack frame
					var frame = st.GetFrame(0);
					// Get the line number from the stack frame
					var line = frame.GetFileLineNumber();
					imgErrorMsg.Add("GetLC Pic failed " + Dir + "\\" + ProcessingGlycanKey + "  @ " + line + "  Err Msg:" + ex);
				}
			});
			errorMsgs = imgErrorMsg;
		}

		public static void GenQuantImg(string argQuantFile, enumGlycanLabelingMethod argLabelingMethod,
			string argExportFolder)
		{
			string Dir = argExportFolder + "\\Pic";
			if (!Directory.Exists(Dir))
			{
				Directory.CreateDirectory(Dir);
			}

			var LstColor = new List<Color>()
			{
				Color.DarkCyan,
				Color.DarkGoldenrod,
				Color.DarkGray,
				Color.DarkGreen,
				Color.DarkKhaki,
				Color.DarkMagenta,
				Color.DarkOliveGreen,
				Color.DarkOrchid,
				Color.DarkRed,
				Color.DarkSalmon,
				Color.DarkSeaGreen,
				Color.DarkSlateBlue,
				Color.DarkSlateGray,
				Color.DarkTurquoise,
				Color.DarkViolet,
				Color.DeepPink,
				Color.DeepSkyBlue
			};

			//Get Title

			var dictTitle = new Dictionary<string, int>();
			var sr = new StreamReader(argQuantFile);
			string tmp = "";
			//bool isLabeling = false;
			tmp = sr.ReadLine();

			#region Read Title

			string[] tmpAry = tmp.Split(',');
			//string LabelTitle = "";
			dictTitle.Add("Glycan", 0);
			var lstLabelingTag = new List<string>();
			if (argLabelingMethod == enumGlycanLabelingMethod.DRAG)
			{
				lstLabelingTag.Add("DRAG_Light(Adjusted 1)");
				lstLabelingTag.Add("DRAG_Heavy(Adjusted 1)");
			}
			else if (argLabelingMethod == enumGlycanLabelingMethod.MultiplexPermethylated)
			{
				lstLabelingTag.Add("MP_CH3");
				lstLabelingTag.Add("MP_CH2D");
				lstLabelingTag.Add("MP_CHD2");
				lstLabelingTag.Add("MP_CD3");
				lstLabelingTag.Add("MP_13CH3");
				lstLabelingTag.Add("MP_13CH2D");
				lstLabelingTag.Add("MP_13CHD2");
				lstLabelingTag.Add("MP_13CD3");
			}
			for (int i = 1; i < tmpAry.Length; i++)
			{
				if (argLabelingMethod == enumGlycanLabelingMethod.MultiplexPermethylated &&
					tmpAry[i].StartsWith("MP"))
				{
					dictTitle.Add(tmpAry[i], i + 3);
					// MP_CH3	Normalization Factor	 Estimated Purity	Normalizted and Adjusted Abundance (Adjusted Factor=1)
				}
				else if (argLabelingMethod == enumGlycanLabelingMethod.DRAG &&
						 tmpAry[i].Contains("DRAG_Light(Adjusted 1)") ||
						 tmpAry[i].Contains("DRAG_Heavy(Adjusted 1)"))
				{
					dictTitle.Add(tmpAry[i], i);
				}
			}

			#endregion Read Title

			#region Get Data

			var dictData =
				new Dictionary<string, Dictionary<string, double>>();
			while (!sr.EndOfStream)
			{
				tmp = sr.ReadLine();
				tmpAry = tmp.Split(',');
				string GlycanKey = tmpAry[dictTitle["Glycan"]];
				if (!dictData.ContainsKey(GlycanKey))
				{
					dictData.Add(GlycanKey, new Dictionary<string, double>());
				}
				foreach (string LabelTag in lstLabelingTag)
				{
					if (!dictTitle.ContainsKey(LabelTag))
					{
						continue;
					}
					if (tmpAry[dictTitle[LabelTag]] != "N/A")
					{
						double intensity = Convert.ToDouble(tmpAry[dictTitle[LabelTag]]);
						if (intensity < 0)
						{
							intensity = 0;
						}
						dictData[GlycanKey].Add(LabelTag, intensity);
					}
				}
			}
			sr.Close();

			#endregion Get Data

			#region Generate Quant Images

			//foreach(string Gkey in dictData.Keys)
			//ZedGraph.ZedGraphControl zgcGlycan = null;
			Parallel.ForEach(dictData.Keys, new ParallelOptions() { MaxDegreeOfParallelism = MaxDegreeParallelism }, Gkey =>
			{
				//ZedGraphControl zgcGlycan = null;
				try
				{
					var cht = new Chart();
					cht.Size = new Size(2400, 1200);

					cht.ChartAreas.Add("Default");
					cht.ChartAreas["Default"].AxisX.Title = "Labeling";
					cht.ChartAreas["Default"].AxisX.TitleFont = new Font("Arial", 24, FontStyle.Bold);
					cht.ChartAreas["Default"].AxisX.LabelStyle.Format = "{F2}";
					cht.ChartAreas["Default"].AxisX.LabelStyle.Font = new Font("Arial", 24, FontStyle.Bold);
					cht.ChartAreas["Default"].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
					cht.ChartAreas["Default"].AxisY.Title = "Abundance(%)";
					cht.ChartAreas["Default"].AxisY.TitleFont = new Font("Arial", 24, FontStyle.Bold);
					cht.ChartAreas["Default"].AxisY.LabelStyle.Format = "{0.#E+00}";
					cht.ChartAreas["Default"].AxisY.LabelStyle.Font = new Font("Arial", 24, FontStyle.Bold);
					cht.ChartAreas["Default"].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

					cht.Titles.Add("Default");
					cht.Titles[0].Text = "Glycan: " + Gkey;
					cht.Titles[0].Font = new Font("Arial", 24, FontStyle.Bold);

					cht.Legends.Add("Default");
					cht.Legends["Default"].Docking = Docking.Bottom;
					cht.Legends["Default"].Alignment = StringAlignment.Center;
					cht.Legends["Default"].LegendStyle = LegendStyle.Row;
					cht.Legends["Default"].Font = new Font("Arial", 24, FontStyle.Bold);

					var dictLabelIntensity = new Dictionary<enumLabelingTag, double>();
					double YMax = dictData[Gkey].Values.Max();

					var labels = new List<string>();
					var ppl = new List<LCPointPair>();

					int i = 0;
					foreach (string labelTag in lstLabelingTag)
					{
						labels.Add(labelTag);
						if (!dictTitle.ContainsKey(labelTag) || !dictData[Gkey].ContainsKey(labelTag))
						{
							ppl.Add(new LCPointPair(i, 0));
						}
						else
						{
							ppl.Add(new LCPointPair(i, dictData[Gkey][labelTag] / YMax * 100));
						}
						i++;
					}
					var myBar = cht.Series.Add("Data");
					myBar.ChartType = SeriesChartType.Bar;
					for (int j = 0; j < ppl.Count; j++)
					{
						myBar.Points.AddY(ppl[j].Intensity);
						myBar.Points[j].AxisLabel = lstLabelingTag[j];
					}

					cht.SaveImage(Dir + "\\Quant-" + Gkey + ".png", ChartImageFormat.Png);
				}
				catch (Exception ex)
				{
					throw ex;
				}

				#endregion Generate Quant Images
			});
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