using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using COL.ElutionViewer;
using COL.GlycoLib;
using COL.MassLib;
using MSPoint = COL.MassLib.MSPoint;

namespace COL.MultiGlycan
{
    public static class SecondPassedForMultiplexPermethylated
    {
        public static void Processing(ref MultiGlycanESI argMultiGlycan)
        {            
            try
            {

                List<string> identifiedGlycan =argMultiGlycan.IdentifiedGlycanCompounds.Select(x => x.GlycanKey).Distinct().ToList();
                Dictionary<string,List<int>> dictGlycanKeySearchRange = new Dictionary<string, List<int>>();


                int LastScan = argMultiGlycan.RawReader.NumberOfScans;
                foreach (string glycanKey in identifiedGlycan)
                {
                    List<int> identifedScans =
                        argMultiGlycan.MatchedPeakInScan.Where(x => x.GlycanComposition.GlycanKey == glycanKey)
                            .Select(y => y.ScanNum)
                            .ToList();
                    identifedScans.Sort();
                    
                    int frontEdge = identifedScans[0];
                    double FirstIdentifiedTime = argMultiGlycan.RawReader.ReadScan(frontEdge).Time;
                    while (frontEdge>1)
                    {
                        frontEdge -= 1;
                        if (argMultiGlycan.RawReader.GetMsLevel(frontEdge) != 1)
                        {
                            continue;
                        }
                        MSScan s = argMultiGlycan.RawReader.ReadScan(frontEdge);
                        if (FirstIdentifiedTime - s.Time > 5)
                        {
                            break;
                        }
                    }
                    int backEdge = identifedScans[identifedScans.Count - 1];
                    double LastIdentifedTime = argMultiGlycan.RawReader.ReadScan(backEdge).Time;
                 
                    while (backEdge < LastScan)
                    {
                        backEdge += 1;
                        if (argMultiGlycan.RawReader.GetMsLevel(backEdge) != 1)
                        {
                            continue;
                        }
                        MSScan s = argMultiGlycan.RawReader.ReadScan(backEdge);
                        if (s.Time - LastIdentifedTime > 5)
                        {
                            break;
                        }
                    }
                    dictGlycanKeySearchRange.Add(glycanKey, new List<int>(){frontEdge,backEdge});
                }

                List<MSScan> msScans = new List<MSScan>();
                string previousGlycanKey = "";
                foreach (GlycanCompound g in argMultiGlycan.GlycanList.OrderBy(x=> x.GlycanKey))
                {
                    if (!identifiedGlycan.Contains(g.GlycanKey))
                    {
                        continue;
                    }
                    if (previousGlycanKey != g.GlycanKey)
                    {
                        msScans.Clear();
                        //Read Scans
                        int startScan = dictGlycanKeySearchRange[g.GlycanKey][0] >= 1 ? dictGlycanKeySearchRange[g.GlycanKey][0] : 1;
                        int endScan = dictGlycanKeySearchRange[g.GlycanKey][1] <= LastScan ? dictGlycanKeySearchRange[g.GlycanKey][1] : LastScan;
                        msScans.AddRange(argMultiGlycan.RawReader.ReadScanWMSLevel(startScan, endScan,1));
                        previousGlycanKey = g.GlycanKey;
                    }
                    
                    //FindPeaks
                    int charge = g.Charge;
                    List<List<MSScan>> scanSectionsContainPeaks = new List<List<MSScan>>();
                    for (int i = 0; i < msScans.Count; i++)
                    {
                        MSScan scan = msScans[i];
                        int closedIdx = MassUtility.GetClosestMassIdx(scan.MZs, (float) g.MZ);
                        if (MassUtility.GetMassPPM(scan.MZs[closedIdx], g.MZ) <=argMultiGlycan.MassPPM)
                        {
                            List<int> peaks = FindPeakIdx(scan.MZs, closedIdx, charge, argMultiGlycan.IsotopePPM);
                            if (peaks.Count - peaks.IndexOf(closedIdx) < argMultiGlycan.MininumIsotopePeakCount)
                            {
                                continue;
                            }
                            //Found peaks
                            if (scanSectionsContainPeaks.Count > 0)
                            {
                                MSScan lastSectionLastScan = scanSectionsContainPeaks.Last().Last();
                                if (scan.Time - lastSectionLastScan.Time > 2.5)
                                {
                                    scanSectionsContainPeaks.Add(new List<MSScan>(){scan});
                                }
                                else
                                {
                                    scanSectionsContainPeaks.Last().Add(scan);
                                }
                            }
                            else
                            {
                                scanSectionsContainPeaks.Add(new List<MSScan>(){scan});
                            }
                        }
                    }

                    //If scans can form an elution profile add to identifed result;

                    foreach (List<MSScan> sectionScans in scanSectionsContainPeaks)
                    {
                        if (sectionScans.Count < 3)
                        {
                            continue;
                        }
                        else
                        {
                            //Add matched peak 
                            foreach (MSScan s in sectionScans)
                            {
                                int PeakIdx = MassUtility.GetClosestMassIdx(s.MZs, (float)g.MZ);
                                MatchedGlycanPeak mPeak = new MatchedGlycanPeak(s.ScanNo, s.Time, s.MSPeaks[PeakIdx], g);

                                List<int> peaks = FindPeakIdx(s.MZs, PeakIdx, charge, argMultiGlycan.IsotopePPM);
                                int startIdx = peaks.IndexOf(PeakIdx);
                                List<MSPoint> lstIsotopes= new List<MSPoint>();
                                for (int i = startIdx; i < peaks.Count; i++)
                                {
                                    lstIsotopes.Add(new MSPoint(s.MZs[peaks[i]],s.Intensities[peaks[i]]));
                                }
                                mPeak.MSPoints = lstIsotopes;
                                if (!argMultiGlycan.MatchedPeakInScan.Contains(mPeak))
                                {
                                    argMultiGlycan.MatchedPeakInScan.Add(mPeak);
                                }
                            }
                        }
                    }
                }





                //foreach (GlycanCompound g in argMultiGlycan.IdentifiedGlycanCompounds)
                //{
                //    processed = g;
                //    //Search Peak in front of the peak 
                //    ThermoRawReader raw = (ThermoRawReader)argMultiGlycan.RawReader;
                //    List<int> identifedScans = argMultiGlycan.MatchedPeakInScan.Where(x => x.GlycanComposition == g).Select(y => y.ScanNum).ToList();
                //    for (int i = 0; i < identifedScans.Count; i++)
                //    {
                //        int identifiedScanNum = identifedScans[i];
                //        while (true)
                //        {
                //            identifiedScanNum -= 1;
                //            if (identifiedScanNum < 1 || raw.GetRetentionTime(identifedScans[i]) - raw.GetRetentionTime(identifiedScanNum) > argMultiGlycan.MaxLCFrontMin)
                //            {
                //                break;
                //            }
                //            if (raw.GetMsLevel((identifiedScanNum)) != 1)
                //            {
                //                continue;
                //            }
                //            MSScan scan = raw.ReadScan(identifiedScanNum);
                //            int PeakIdx = MassUtility.GetClosestMassIdx(scan.MZs, (float)g.MZ);
                //            if (MassUtility.GetMassPPM(scan.MZs[PeakIdx], g.MZ) > argMultiGlycan.MassPPM)
                //            {
                //                continue;
                //            }
                //            //Find isotope cluster 
                //            List<MSPoint> lstIsotopes = new List<MSPoint>();
                //            lstIsotopes.Add(new MSPoint(scan.MZs[PeakIdx],scan.Intensities[PeakIdx]));
                //            float targetMZ = (float)g.MZ;
                //            do
                //            {
                //                targetMZ += 1.0f/g.Charge;
                //                PeakIdx = MassUtility.GetClosestMassIdx(scan.MZs, targetMZ);
                //                if (MassUtility.GetMassPPM(scan.MZs[PeakIdx], targetMZ) <= argMultiGlycan.MassPPM)
                //                {
                //                    lstIsotopes.Add(new MSPoint(scan.MZs[PeakIdx],scan.Intensities[PeakIdx]));
                //                }
                //                else
                //                {
                //                    break;
                //                }
                //            } while (true);
                //            if (lstIsotopes.Count < argMultiGlycan.MininumIsotopePeakCount)
                //            {
                //                continue;
                //            }
                //            MatchedGlycanPeak mPeak = new MatchedGlycanPeak(scan.ScanNo, scan.Time, scan.MSPeaks[PeakIdx], g);
                //            mPeak.MSPoints = lstIsotopes;
                //            if (!argMultiGlycan.MatchedPeakInScan.Contains(mPeak))
                //            {
                //                argMultiGlycan.MatchedPeakInScan.Add(mPeak);
                //            }
                //        }
                //        identifiedScanNum = identifedScans[i];
                //        while (true)
                //        {
                //            identifiedScanNum += 1;
                //            if (identifiedScanNum > raw.NumberOfScans || raw.GetRetentionTime(identifiedScanNum) - raw.GetRetentionTime(identifedScans[i]) > argMultiGlycan.MaxLCBackMin)
                //            {
                //                break;
                //            }
                //            if (raw.GetMsLevel((identifiedScanNum)) != 1)
                //            {
                //                continue;
                //            }
                //            MSScan scan = raw.ReadScan(identifiedScanNum);
                //            int PeakIdx = MassUtility.GetClosestMassIdx(scan.MZs, (float)g.MZ);
                //            if (MassUtility.GetMassPPM(scan.MZs[PeakIdx], g.MZ) > argMultiGlycan.MassPPM)
                //            {
                //                continue;
                //            }
                //            //Find isotope cluster 
                //            List<MSPoint> lstIsotopes = new List<MSPoint>();
                //            lstIsotopes.Add(new MSPoint(scan.MZs[PeakIdx], scan.Intensities[PeakIdx]));
                //            float targetMZ = (float)g.MZ;
                //            do
                //            {
                //                targetMZ += 1.0f / g.Charge;
                //                PeakIdx = MassUtility.GetClosestMassIdx(scan.MZs, targetMZ);
                //                if (MassUtility.GetMassPPM(scan.MZs[PeakIdx], targetMZ) <= argMultiGlycan.MassPPM)
                //                {
                //                    lstIsotopes.Add(new MSPoint(scan.MZs[PeakIdx], scan.Intensities[PeakIdx]));
                //                }
                //                else
                //                {
                //                    break;
                //                }
                //            } while (true);
                //            if (lstIsotopes.Count < argMultiGlycan.MininumIsotopePeakCount)
                //            {
                //                continue;
                //            }
                //            MatchedGlycanPeak mPeak = new MatchedGlycanPeak(scan.ScanNo, scan.Time, scan.MSPeaks[PeakIdx], g);
                //            mPeak.MSPoints = lstIsotopes;
                //            if (!argMultiGlycan.MatchedPeakInScan.Contains(mPeak))
                //            {
                //                argMultiGlycan.MatchedPeakInScan.Add(mPeak);
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                
                throw ex;
            }
         
        }
        private static List<int> FindPeakIdx(float[] argMZAry, int argTargetIdx, int argCharge, float argIsotopePPM)
        {
            List<int> Peak = new List<int>();
            float Interval = 1 / (float)argCharge;
            float FirstMZ = argMZAry[argTargetIdx];
            int CurrentIdx = argTargetIdx;
            Peak.Add(argTargetIdx);
            //Forward  Peak
            for (int i = argTargetIdx - 1; i >= 0; i--)
            {
                if (argMZAry[argTargetIdx] - argMZAry[i] >= Interval * 10)
                {
                    break;
                }
                List<int> ClosedPeaks = MassLib.MassUtility.GetClosestMassIdxsWithinPPM(argMZAry, argMZAry[CurrentIdx] - Interval, argIsotopePPM);
                if (ClosedPeaks.Count == 1)
                {
                    CurrentIdx = ClosedPeaks[0];
                    Peak.Insert(0, ClosedPeaks[0]);
                }
                else if (ClosedPeaks.Count > 1)
                {
                    double minPPM = 100;
                    int minPPMIdx = 0;
                    for (int j = 0; j < ClosedPeaks.Count; j++)
                    {
                        if (MassLib.MassUtility.GetMassPPM(argMZAry[ClosedPeaks[j]], argMZAry[CurrentIdx] - Interval) < minPPM)
                        {
                            minPPMIdx = ClosedPeaks[j];
                            minPPM = MassLib.MassUtility.GetMassPPM(argMZAry[ClosedPeaks[j]], argMZAry[CurrentIdx] + Interval);
                        }
                    }
                    CurrentIdx = minPPMIdx;
                    Peak.Insert(0, CurrentIdx);
                }
            }
            //Backward  Peak
            CurrentIdx = argTargetIdx;
            for (int i = argTargetIdx + 1; i < argMZAry.Length; i++)
            {
                if (argMZAry[i] - argMZAry[argTargetIdx] >= Interval * 10)
                {
                    break;
                }
                List<int> ClosedPeaks = MassLib.MassUtility.GetClosestMassIdxsWithinPPM(argMZAry, argMZAry[CurrentIdx] + Interval, argIsotopePPM);
                if (ClosedPeaks.Count == 1)
                {
                    CurrentIdx = ClosedPeaks[0];
                    Peak.Add(ClosedPeaks[0]);
                }
                else if (ClosedPeaks.Count > 1)
                {
                    double minPPM = 100;
                    int minPPMIdx = 0;
                    for (int j = 0; j < ClosedPeaks.Count; j++)
                    {
                        if (MassLib.MassUtility.GetMassPPM(argMZAry[ClosedPeaks[j]], argMZAry[CurrentIdx] + Interval) < minPPM)
                        {
                            minPPMIdx = ClosedPeaks[j];
                            minPPM = MassLib.MassUtility.GetMassPPM(argMZAry[ClosedPeaks[j]], argMZAry[CurrentIdx] + Interval);
                        }
                    }
                    CurrentIdx = minPPMIdx;
                    Peak.Add(CurrentIdx);
                }
            }

            return Peak;
        }
    }
}
