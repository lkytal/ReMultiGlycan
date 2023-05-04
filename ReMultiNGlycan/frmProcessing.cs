using COL.GlycoLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace COL.MultiGlycan
{
	public partial class frmProcessing : Form
	{
		private MultiGlycanESI _MultiNGlycan;
		private List<int> LstScanNumber;
		private int CurrentScan = 0;
		private DateTime Start;

		private bool DoLog = false;

		public frmProcessing(MultiGlycanESI argMultiNGlycan, bool argLog)
		{
			InitializeComponent();
			DoLog = argLog;

			_MultiNGlycan = argMultiNGlycan;
			_MultiNGlycan.MaxGlycanCharge = 5;
			int StartScan = argMultiNGlycan.StartScan;
			int EndScan = argMultiNGlycan.EndScan;
			LstScanNumber = new List<int>();

			for (int i = StartScan; i <= EndScan; i++)
			{
				if (_MultiNGlycan.RawReader.GetMsLevel(i) == 1)
				{
					LstScanNumber.Add(i);
				}
			}
			Start = DateTime.Now;
			if (DoLog)
			{
				Logger.WriteLog("Start process each scan");
			}
			try
			{
				bgWorker_Process.RunWorkerAsync();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		//public frmProcessing(MultiNGlycanESIMultiThreads argMultiNGlycan, int argExportScanFilter)
		//{
		//    InitializeComponent();
		//    //_MultiNGlycan = argMultiNGlycan;
		//    //int StartScan = argMultiNGlycan.StartScan;
		//    //int EndScan = argMultiNGlycan.EndScan;
		//    //LstScanNumber = new List<int>();
		//    //for (int i = StartScan; i <= EndScan; i++)
		//    //{
		//    //    LstScanNumber.Add(i);
		//    //}
		//    //Start = DateTime.Now;
		//    //_GlycanScanFilter = argExportScanFilter;
		//    //bgWorker_Process.RunWorkerAsync();
		//}
		private void bgWorker_Process_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				for (int i = 0; i < LstScanNumber.Count; i++)
				{
					_MultiNGlycan.ProcessSingleScan(LstScanNumber[i]);
					CurrentScan = i;
					bgWorker_Process.ReportProgress(Convert.ToInt32((i / (float)LstScanNumber.Count) * 100));
					if (DoLog)
					{
						Logger.WriteLog("Finish 1st scan:" + LstScanNumber[i].ToString());
					}
				}
				//ProcessSingleScanTwoPassID Merged to ProcessSingleScan
				//if (_MultiNGlycan.TwoPassedID)
				//{
				//    //
				//    for (int i = 0; i < LstScanNumber.Count; i++)
				//    {
				//        _MultiNGlycan.ProcessSingleScanTwoPassID(LstScanNumber[i]);
				//        CurrentScan = i;
				//        bgWorker_Process.ReportProgress(Convert.ToInt32((i / (float)LstScanNumber.Count) * 100));
				//        if (DoLog)
				//        {
				//            Logger.WriteLog("Finish 2ed scan:" + LstScanNumber[i].ToString());
				//        }
				//    }
				//}
			}
			catch (Exception EcpMsG)
			{
				throw new Exception(EcpMsG.Message);
			}
		}

		private void bgWorker_Process_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			lblCurrentScan.Text = (CurrentScan + 1).ToString() + " / " + LstScanNumber.Count.ToString();
			progressBar1.Value = e.ProgressPercentage;
			lblPercentage.Text = e.ProgressPercentage.ToString() + "%";
			lblNumberOfCluster.Text = _MultiNGlycan.MatchedPeakInScan.Count.ToString();
			lblStatus.Text = "Processing Scan " + LstScanNumber[CurrentScan].ToString();
			//if (_MultiNGlycan.TwoPassedID  )
			//{
			//    lblStatus.Text = " 2nd passed Processing Scan " + LstScanNumber[CurrentScan].ToString();
			//}
		}

		private void bgWorker_Process_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			try
			{
				//  _MultiNGlycan.ExportParametersToExcel();
				if (DoLog)
				{
					Logger.WriteLog("Start merge peaks");
				}
				lblStatus.Text = "Mergeing Peaks";
				if (_MultiNGlycan.LabelingMethod == enumGlycanLabelingMethod.MultiplexPermethylated)
				{
					SecondPassedForMultiplexPermethylated.Processing(ref _MultiNGlycan);
				}
				//if (_MultiNGlycan.TwoPassedID)
				//{
				//   // _MultiNGlycan.Merge2PassedCluster();
				//    _MultiNGlycan.MergeSingleScanResultToPeak();
				//}
				//else
				//{
				// _MultiNGlycan.MergeCluster();
				_MultiNGlycan.MergeSingleScanResultToPeak();
				_MultiNGlycan.SolveDuplicateAssignment();
				_MultiNGlycan.MergeSingleScanResultToPeak();
				//}

				if (DoLog)
				{
					Logger.WriteLog("End merge peaks");
				}
				if (_MultiNGlycan.GlycanLCorderExist)
				{
					_MultiNGlycan.ApplyLCordrer();
				}

				if (!Directory.Exists(_MultiNGlycan.ExportFilePath + "\\Pic") && (_MultiNGlycan.IndividualImgs || _MultiNGlycan.QuantificationImgs))
				{
					Directory.CreateDirectory(_MultiNGlycan.ExportFilePath + "\\Pic");
				}

				if (_MultiNGlycan.LabelingMethod == GlycoLib.enumGlycanLabelingMethod.MultiplexPermethylated)
				{
					_MultiNGlycan.EstimatePurity();
					foreach (GlycoLib.enumLabelingTag tag in _MultiNGlycan.LabelingRatio.Keys)
					{
						if (tag == enumLabelingTag.MP_CH3 || !_MultiNGlycan.HasEstimatePurity((tag)))
							continue;
						_MultiNGlycan.GetPurityEstimateImage(tag, _MultiNGlycan.ExportFilePath + "\\Pic\\EstimatePurity_" + tag.ToString() + ".png");
					}

					//Correct Intensity;
					//_MultiNGlycan.CorrectIntensityByIsotope();
				}
				if (DoLog)
				{
					;
					Logger.WriteLog("Start export");
				}
				lblStatus.Text = "Exporting";
				_MultiNGlycan.ExportToCSV();

				//Get individual image
				if (_MultiNGlycan.IndividualImgs &&
					File.Exists(_MultiNGlycan.ExportFilePath + "\\" + Path.GetFileName(_MultiNGlycan.ExportFilePath) +
								"_FullList.csv"))
				{
					//GenerateImages.GenGlycanLcImg(
					//    _MultiNGlycan.ExportFilePath + "\\" + Path.GetFileName(_MultiNGlycan.ExportFilePath) +"_FullList.csv",
					//    _MultiNGlycan.ExportFilePath);
					GenerateImages.GenGlycanLcImg(_MultiNGlycan);
				}
				//Get Quant Image
				if (_MultiNGlycan.LabelingMethod != enumGlycanLabelingMethod.None && _MultiNGlycan.QuantificationImgs
					&& File.Exists(_MultiNGlycan.ExportFilePath + "\\" + Path.GetFileName(_MultiNGlycan.ExportFilePath) +
								"_Quant.csv"))
				{
					GenerateImages.GenQuantImg(
						_MultiNGlycan.ExportFilePath + "\\" + Path.GetFileName(_MultiNGlycan.ExportFilePath) + "_Quant.csv",
						_MultiNGlycan.LabelingMethod,
						_MultiNGlycan.ExportFilePath);
				}

				//_MultiNGlycan.ExportToExcel();
				if (DoLog)
				{
					Logger.WriteLog("End export");
				}
				TimeSpan TDiff = DateTime.Now.Subtract(Start);
				lblStatus.Text = "Finish in " + TDiff.TotalMinutes.ToString("0.00") + " mins";
				lblNumberOfMerge.Text = _MultiNGlycan.MergedResultList.Count.ToString();
				progressBar1.Value = 100;
				lblPercentage.Text = "100%";
				FlashWindow.Flash(this);
				this.Text = "Done";
				if (DoLog)
				{
					Logger.WriteLog("End process each scan");
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		private void frmProcessing_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (bgWorker_Process.IsBusy)
			{
				if (MessageBox.Show("Still processing, do you want to quit?", "Exit process?", MessageBoxButtons.YesNo) == DialogResult.Yes)
				{
					bgWorker_Process.CancelAsync();
					if (DoLog)
					{
						Logger.WriteLog("User terminate process");
					}
				}
				else
				{
					e.Cancel = true;
				}
			}
		}
	}
}