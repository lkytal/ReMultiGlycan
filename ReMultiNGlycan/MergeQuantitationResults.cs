using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using OfficeOpenXml.ConditionalFormatting;

namespace COL.MultiGlycan
{
    public static class MergeQuantitationResults
    {
        public static void MergeFullList(List<string> argFiles, string argOutputFile, bool argProtonatedResult = false)
        {
            
            List<string> ResultFiles = argFiles;
            List<string> AllGlycans = new List<string>();
            Dictionary<string, List<QuantitationPeak>> AllResult = new Dictionary<string, List<QuantitationPeak>>();
            for (int i = 0; i < ResultFiles.Count; i++)
            {
                //Glycan, Adduct
                Dictionary<string, Dictionary<string, List<Tuple<float, float>>>> Result = ReadFullResultCSV(ResultFiles[i]);
                //Impute data 

                ImputationData(Result);
                foreach (string glycanKey in Result.Keys)
                {
                    if (!AllGlycans.Contains(glycanKey))
                    {
                        AllGlycans.Add(glycanKey);
                    }
                    if (!AllResult.ContainsKey(glycanKey))
                    {
                        AllResult.Add(glycanKey, new List<QuantitationPeak>());
                    }
                    QuantitationPeak qPeak = new QuantitationPeak(Path.GetFileNameWithoutExtension(ResultFiles[i]).Replace("_FullList", ""), glycanKey);
                    qPeak.AssignPeaks(Result[glycanKey]);

                    if (!AllResult.ContainsKey(glycanKey))
                    {
                        AllResult.Add(glycanKey, new List<QuantitationPeak>());
                    }
                    AllResult[glycanKey].Add(qPeak);
                }
            }



            //Export File
            using (StreamWriter sw = new StreamWriter(argOutputFile))
            {
                string tmpStr = "";
                tmpStr = "Glycan,";
                //Title
                foreach (string filename in ResultFiles)
                {
                    tmpStr = tmpStr + Path.GetFileNameWithoutExtension(filename) + ",";
                }
                sw.WriteLine(tmpStr);

                foreach (string GlycanKey in AllResult.Keys)
                {
                    GetBalanceData(AllResult[GlycanKey]);
                    //Protonated
                    tmpStr = GlycanKey + " Protonated,";
                    int ZeroAndNACount = 0;
                    double smallestIntensity = 9999999999999;
                    int fileIdx = 0;
                    for (int i = 0; i < AllResult[GlycanKey].Count; i++)
                    {
                        while (AllResult[GlycanKey][i].DataSetName != Path.GetFileNameWithoutExtension(ResultFiles[fileIdx]).Replace("_FullList", ""))
                        {
                            tmpStr = tmpStr + "N/A,";
                            fileIdx++;
                            ZeroAndNACount++;
                        }
                        tmpStr = tmpStr + AllResult[GlycanKey][i].TotalProtonatedIntensity.ToString("0.00") + ",";
                        if (AllResult[GlycanKey][i].TotalProtonatedIntensity == 0)
                        {
                            ZeroAndNACount++;
                        }
                        fileIdx++;

                        if (smallestIntensity >= AllResult[GlycanKey][i].TotalProtonatedIntensity && AllResult[GlycanKey][i].TotalProtonatedIntensity > 0)
                        {
                            smallestIntensity = AllResult[GlycanKey][i].TotalProtonatedIntensity;
                        }
                    }
                    while (tmpStr.Count(t => t == ',') < ResultFiles.Count + 1)
                    {
                        tmpStr += "N/A,";
                        ZeroAndNACount++;
                    }
                    if (ZeroAndNACount == ResultFiles.Count)
                    {
                        continue;
                    }
                    sw.WriteLine(tmpStr);
                    tmpStr = GlycanKey + " Protonated Ratio,";
                    fileIdx = 0;
                    for (int i = 0; i < AllResult[GlycanKey].Count; i++)
                    {
                        while (AllResult[GlycanKey][i].DataSetName != Path.GetFileNameWithoutExtension(ResultFiles[fileIdx]).Replace("_FullList", ""))
                        {
                            tmpStr = tmpStr + "N/A,";
                            fileIdx++;
                        }
                        tmpStr = tmpStr + (AllResult[GlycanKey][i].TotalProtonatedIntensity / smallestIntensity).ToString("0.00") + ",";
                        fileIdx++;
                    }
                    while (tmpStr.Count(t => t == ',') < ResultFiles.Count + 1)
                    {
                        tmpStr += "N/A,";
                    }
                    sw.WriteLine(tmpStr);
                    //All
                    tmpStr = GlycanKey + " All Adducts,";

                    smallestIntensity = 9999999999999;
                    fileIdx = 0;
                    for (int i = 0; i < AllResult[GlycanKey].Count; i++)
                    {
                        while (AllResult[GlycanKey][i].DataSetName != Path.GetFileNameWithoutExtension(ResultFiles[fileIdx]).Replace("_FullList", ""))
                        {
                            tmpStr = tmpStr + "N/A,";
                            fileIdx++;
                        }
                        tmpStr = tmpStr + AllResult[GlycanKey][i].TotalIntensity.ToString("0.00") + ",";
                        fileIdx++;

                        if (smallestIntensity >= AllResult[GlycanKey][i].TotalIntensity && AllResult[GlycanKey][i].TotalIntensity > 0)
                        {
                            smallestIntensity = AllResult[GlycanKey][i].TotalIntensity;
                        }
                    }
                    while (tmpStr.Count(t => t == ',') < ResultFiles.Count + 1)
                    {
                        tmpStr += "N/A,";
                    }
                    sw.WriteLine(tmpStr);
                    tmpStr = GlycanKey + " All Ratio,";
                    fileIdx = 0;
                    for (int i = 0; i < AllResult[GlycanKey].Count; i++)
                    {
                        while (AllResult[GlycanKey][i].DataSetName != Path.GetFileNameWithoutExtension(ResultFiles[fileIdx]).Replace("_FullList", ""))
                        {
                            tmpStr = tmpStr + "N/A,";
                            fileIdx++;
                        }
                        tmpStr = tmpStr + (AllResult[GlycanKey][i].TotalIntensity / smallestIntensity).ToString("0.00") + ",";
                        fileIdx++;
                    }
                    while (tmpStr.Count(t => t == ',') < ResultFiles.Count + 1)
                    {
                        tmpStr += "N/A,";
                    }
                    sw.WriteLine(tmpStr);
                }
            }
            //Debug
            if (argProtonatedResult)
            {
                StreamWriter swDebug = new StreamWriter(Path.GetDirectoryName(argOutputFile) + "\\MergeResult_Debug.csv");
                string tmpDebugStr = "";
                tmpDebugStr = "Glycan,";
                //Title
                foreach (string filename in ResultFiles)
                {
                    tmpDebugStr = tmpDebugStr + Path.GetFileNameWithoutExtension(filename) + "_Time," + Path.GetFileNameWithoutExtension(filename) + "_Intensity,";
                }
                swDebug.WriteLine(tmpDebugStr);
                foreach (string GlycanKey in AllResult.Keys)
                {
                    GetBalanceData(AllResult[GlycanKey]);
                    swDebug.WriteLine(GlycanKey + " Protonated");
                    int ProtonatedPeakCount = AllResult[GlycanKey][0].ProtonatedPeaks.Count;


                    for (int i = 0; i < ProtonatedPeakCount; i++)
                    {
                        tmpDebugStr = ",";
                        int fileIdx = 0;
                        for (int j = 0; j < ResultFiles.Count; j++)
                        {
                            if (AllResult[GlycanKey].Count <= fileIdx || AllResult[GlycanKey][fileIdx].DataSetName != Path.GetFileNameWithoutExtension(ResultFiles[fileIdx]).Replace("_FullList", "") || AllResult[GlycanKey][fileIdx].ProtonatedPeaks.Count == 0)
                            {
                                tmpDebugStr += "N/A,N/A,";
                            }
                            else
                            {
                                tmpDebugStr += AllResult[GlycanKey][fileIdx].ProtonatedPeaks[i].Item1.ToString("0.00") + "," + AllResult[GlycanKey][fileIdx].ProtonatedPeaks[i].Item2.ToString("0.00") + ",";
                                fileIdx++;
                            }
                        }
                        swDebug.WriteLine(tmpDebugStr);
                    }
                }
                swDebug.Close();
            }

        }
        private static Dictionary<string, Dictionary<string, List<Tuple<float, float>>>> ReadFullResultCSV(string argFile)
        {
            StreamReader sr = null;
            //Time	Scan Num	Abundance	m/z	HexNac-Hex-deHex-NeuAc-NeuGc	Adduct	Composition mono
            //34.21	3605	53598.02	1093.567	2-8-0-0-0	H * 2; 	2185.118885

            try
            {
                Dictionary<string, Dictionary<string, List<Tuple<float, float>>>> Result = new Dictionary<string, Dictionary<string, List<Tuple<float, float>>>>();
                //Key 1: Glycan, Key 2: Adduct
                sr = new StreamReader(argFile);
                sr.ReadLine(); // Title
                string tmp = "";
                do
                {
                    tmp = sr.ReadLine();
                    if (tmp == null)
                    {
                        break;
                    }
                    string[] tmpAry = tmp.Split(',');
                    string GlycanKey = tmpAry[4];
                    string[] Adducts = tmpAry[5].Trim().Substring(0, tmpAry[5].Trim().Length - 1).Split(';');
                    string AdductKey = "";

                    for (int i = 0; i < Adducts.Length; i++)
                    {
                        AdductKey = AdductKey + Adducts[i].Trim().Split('*')[0].Trim() + "+";
                    }
                    AdductKey = AdductKey.Substring(0, AdductKey.Length - 1);
                    if (!Result.ContainsKey(GlycanKey))
                    {
                        Result.Add(GlycanKey, new Dictionary<string, List<Tuple<float, float>>>());
                    }
                    if (!Result[GlycanKey].ContainsKey(AdductKey))
                    {
                        Result[GlycanKey].Add(AdductKey, new List<Tuple<float, float>>());
                    }

                    List<Tuple<float, float>> sameLCTime = Result[GlycanKey][AdductKey].Where(t => t.Item1 == Convert.ToSingle(tmpAry[0])).ToList();
                    if (sameLCTime.Count != 0)
                    {
                        float LCTime = sameLCTime[0].Item1;
                        float NewIntensity = sameLCTime[0].Item2 + Convert.ToSingle(tmpAry[2]);
                        int RemoveIdx = Result[GlycanKey][AdductKey].IndexOf(sameLCTime[0]);
                        Result[GlycanKey][AdductKey].RemoveAt(RemoveIdx);
                        Result[GlycanKey][AdductKey].Insert(RemoveIdx, new Tuple<float, float>(LCTime, NewIntensity));
                    }
                    else
                    {
                        Result[GlycanKey][AdductKey].Add(new Tuple<float, float>(Convert.ToSingle(tmpAry[0]), Convert.ToSingle(tmpAry[2])));
                    }
                } while (!sr.EndOfStream);
                return Result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (sr != null)
                {
                    sr.Close();
                }
            }
        }
        private static void ImputationData(Dictionary<string, Dictionary<string, List<Tuple<float, float>>>> argResult)
        {
            foreach (string GlycanKey in argResult.Keys)
            {
                if (!argResult[GlycanKey].ContainsKey("H"))
                {
                    continue;
                }
                List<Tuple<float, float>> ProtonatedPeak = argResult[GlycanKey]["H"];
                List<Tuple<float, float>> AddedPeak = new List<Tuple<float, float>>();

                for (int i = 0; i < ProtonatedPeak.Count - 1; i++)
                {
                    float TimeDifference = Convert.ToSingle(Math.Floor((ProtonatedPeak[i + 1].Item1 - ProtonatedPeak[i].Item1) * 100) / 100);
                    if (TimeDifference > 0.5 || TimeDifference <= 0.07f)
                    {
                        continue;
                    }
                    else //Imputation needed
                    {
                        int TimeIntervalCount = (int)Math.Ceiling(TimeDifference / 0.07) - 1;
                        float AvgIntensity = (ProtonatedPeak[i].Item2 + ProtonatedPeak[i + 1].Item2) / 2;
                        for (int j = 1; j <= TimeIntervalCount; j++)
                        {
                            AddedPeak.Add(new Tuple<float, float>(ProtonatedPeak[i].Item1 + j * 0.07f, AvgIntensity));
                        }
                    }
                }
                ProtonatedPeak.AddRange(AddedPeak);
                ProtonatedPeak = ProtonatedPeak.OrderBy(i => i.Item1).ToList();
            }
        }
        private static void GetBalanceData(List<QuantitationPeak> argQuantPeaks)
        {
            QuantitationPeak processingGlycan = null;
            try
            {
                int minScanCount = 0;
                List<QuantitationPeak> qPeaks = argQuantPeaks.Where(x => x.ProtonatedPeaks.Count > 0).ToList();
                if (qPeaks.Count > 0)
                {
                    minScanCount = qPeaks.Min(x => x.ProtonatedPeaks.Count);
                    foreach (QuantitationPeak qPeak in argQuantPeaks)
                    {
                        processingGlycan = qPeak;
                        while (qPeak.ProtonatedPeaks.Count > minScanCount)
                        {
                            if (qPeak.ProtonatedPeaks[0].Item2 > qPeak.ProtonatedPeaks[qPeak.ProtonatedPeaks.Count - 1].Item2)
                            {
                                qPeak.ProtonatedPeaks.RemoveAt(qPeak.ProtonatedPeaks.Count - 1);
                            }
                            else
                            {
                                qPeak.ProtonatedPeaks.RemoveAt(0);
                            }
                        }
                    }
                }
            }
            catch
            {
                throw new InvalidDataException("GetBalanceData exception " + processingGlycan.GlycanKey);
            }
        }

        public static void MergeConservedList(List<string> argFiles, string argOutputFile)
        {
            Dictionary<string, Dictionary<string, double>> dictAllResult = new Dictionary<string, Dictionary<string, double>>(); //Key:File Name,
            List<string> allGlycansList = new List<string>();
            foreach (string f in argFiles)
            {
                Dictionary<string,double> dictGlycanIntensity = new Dictionary<string, double>(); //Key:glycan Value:intensity
                using (StreamReader sr = new StreamReader(f))
                {
                    bool isInSection = false;
                    Dictionary<string,int> dictHeaderMapping = new Dictionary<string, int>(); //Key :Header title Value: index
                    while (!sr.EndOfStream)
                    {
                        string tmpLine = sr.ReadLine();
                        if (tmpLine.StartsWith("Start Time"))
                        {
                            isInSection = true;
                            string[] tmpTitle = tmpLine.Split(',');
                            for (int i = 0; i < tmpTitle.Length; i++)
                            {
                                dictHeaderMapping.Add(tmpTitle[i], i);
                            }
                            continue;
                        }
                        if (!isInSection)
                        {
                            continue;
                        }
                        string[] tmpAry = tmpLine.Split(',');

                        if (!allGlycansList.Contains(tmpAry[dictHeaderMapping["HexNac-Hex-deHex-NeuAc-NeuGc"]]))
                        {
                            allGlycansList.Add(tmpAry[dictHeaderMapping["HexNac-Hex-deHex-NeuAc-NeuGc"]]);
                        }
                        if (!dictGlycanIntensity.ContainsKey(tmpAry[dictHeaderMapping["HexNac-Hex-deHex-NeuAc-NeuGc"]]))
                        {
                            dictGlycanIntensity.Add(tmpAry[dictHeaderMapping["HexNac-Hex-deHex-NeuAc-NeuGc"]],0);
                        }
                        dictGlycanIntensity[tmpAry[dictHeaderMapping["HexNac-Hex-deHex-NeuAc-NeuGc"]]] +=Convert.ToDouble(tmpAry[dictHeaderMapping["Peak Intensity"]]);
                    }
                }
                dictAllResult.Add(Path.GetFileNameWithoutExtension(f),dictGlycanIntensity);
            }

            //Export
            using (StreamWriter sw = new StreamWriter(argOutputFile))
            {
                //Title
                string tmpOutput = "Glycan,";
                foreach (string fileName in argFiles)
                {
                    tmpOutput += Path.GetFileNameWithoutExtension(fileName) + ",";
                }
                sw.WriteLine(tmpOutput);

                List<double> lstIntensity = new List<double>();
                foreach (string glycanKey in allGlycansList)
                {
                    tmpOutput = glycanKey + ",";
                    foreach (string fi in argFiles)
                    {
                        string filename = Path.GetFileNameWithoutExtension(fi);
                        if (dictAllResult[filename].ContainsKey(glycanKey))
                        {
                            tmpOutput += dictAllResult[filename][glycanKey].ToString() + ",";
                            lstIntensity.Add(Convert.ToDouble(dictAllResult[filename][glycanKey]));
                        }
                        else
                        {
                            tmpOutput += "N/A,";
                        }
                    }
                    sw.WriteLine(tmpOutput);
                    tmpOutput = glycanKey + " ratio,";
                    double smallestInt = lstIntensity.Min();
                    foreach (string fi in argFiles)
                    {
                        string filename = Path.GetFileNameWithoutExtension(fi);
                        if (dictAllResult[filename].ContainsKey(glycanKey))
                        {
                            tmpOutput += (dictAllResult[filename][glycanKey]/smallestInt).ToString("0.00")+ ",";
                        }
                        else
                        {
                            tmpOutput += "N/A,";
                        }
                    }
                    sw.WriteLine(tmpOutput);
                }
            }
        }
    }
}
