using System;
using System.Collections.Generic;
using System.Text;

namespace COL.MassLib
{
    public static class LCPeakDetection
    {
        //In this class all MSPoint.Mass store  Time not mass;
        static float LCApexMaxTolTime = 0.1f;
        public static List<LCPeak> PeakFinding(List<MSPoint> argMSPs, float argDelta, float argIntensityPercentageTol)
        {
            argMSPs.Sort(
                delegate(MSPoint a, MSPoint b)
                {
                    return a.Mass.CompareTo(b.Mass);
                });
            List<MSPoint> localMSPs = new List<MSPoint>();
            localMSPs.AddRange(argMSPs);
            List<LCPeak> DetectedPeaks = new List<LCPeak>();
            //if (localMSPs.Count == 1)
            //{
            //    DetectedPeaks.Add(new LCPeak(argMSPs[0].Mass, argMSPs[0].Mass, argMSPs));
            //}
            //else if (localMSPs.Count == 2)
            //{
            //    if (argMSPs[1].Mass - argMSPs[0].Mass < argDelta)
            //    {
            //        DetectedPeaks.Add(new LCPeak(argMSPs[0].Mass, argMSPs[1].Mass, argMSPs));
            //    }
            //    else
            //    {
            //        List<MSPoint> tmpMSP = new List<MSPoint>();
            //        tmpMSP.Add(argMSPs[0]);
            //        DetectedPeaks.Add(new LCPeak(argMSPs[0].Mass, argMSPs[0].Mass, tmpMSP));
            //        tmpMSP = new List<MSPoint>();
            //        tmpMSP.Add(argMSPs[1]);
            //        DetectedPeaks.Add(new LCPeak(argMSPs[1].Mass, argMSPs[1].Mass, tmpMSP));
            //    }                
            //}
            if (localMSPs.Count >= 3)
            {
                while (localMSPs.Count != 0)
                {
                    int MaxIdx = MaxIntensityIdx(localMSPs);
                    int Lidx = FindLeftBound(localMSPs, MaxIdx, argDelta);
                    int Ridx = FindRightBound(localMSPs, MaxIdx, argDelta);
                    List<MSPoint> PeakMSPs = new List<MSPoint>();
                    for (int i = Ridx; i >= Lidx; i--)
                    {
                        PeakMSPs.Add(localMSPs[i]);
                        localMSPs.RemoveAt(i);
                    }
                    PeakMSPs.Reverse();
                    DetectedPeaks.Add(new LCPeak(PeakMSPs[0].Mass, PeakMSPs[PeakMSPs.Count - 1].Mass, PeakMSPs));
                }
            }
            //DetectedPeaks.Sort(delegate(COL.MassLib.LCPeak PeakA, COL.MassLib.LCPeak PeakB)
            //{
            //    return PeakA.StartTime.CompareTo(PeakB.StartTime);
            //});

            //Remove Peak which has less 3 points and intensity is less than 5 percent
            if (DetectedPeaks.Count > 1)
            {
                float IntTol = DetectedPeaks[0].Apex.Intensity * argIntensityPercentageTol;
                for (int i = DetectedPeaks.Count - 1; i >= 0; i--)
                {
                    if (DetectedPeaks[i].RawPoint.Count <= 3 || DetectedPeaks[i].Apex.Intensity < IntTol)
                    {
                        DetectedPeaks.RemoveAt(i);
                    }
                }
            }
            return DetectedPeaks;
        }
        private static List<MSPeak> MergePeaks(List<MSPeak> argMSPeak)
        {

            return argMSPeak;
        }        
        private static int MaxIntensityIdx(List<MSPoint> argMSPs)
        {
            int MaxIdx = 0;

            for (int i = 0; i < argMSPs.Count; i++)
            {
                if (argMSPs[i].Intensity >= argMSPs[MaxIdx].Intensity)
                {
                    MaxIdx = i;
                }
            }
            return MaxIdx;
        }
        private static int FindLeftBound(List<MSPoint> argMSPs, int argApexIdx, float argDelta)
        {
            int LBoundIdx = argApexIdx - 1;

            if (argApexIdx <= 0)
            {
                return 0;
            }
            if (argMSPs[argApexIdx].Mass - argMSPs[LBoundIdx].Mass > argDelta)
            {
                return argApexIdx;
            }
            for(int i = argApexIdx-1; i>=0 ;i--)
            {
                if (argMSPs[LBoundIdx].Mass - argMSPs[i].Mass  > argDelta)
                {
                    break;
                }
                if (argMSPs[i].Intensity - argMSPs[LBoundIdx].Intensity <= 0)
                {
                    LBoundIdx = i;
                }
                else
                {
                    if (argMSPs[argApexIdx].Mass - argMSPs[i].Mass > LCApexMaxTolTime) //Mass filed store LC time.  If two apex is far away  the this is left bound
                    {
                        break;
                    }
                    else
                    {
                        LBoundIdx = i;
                    }
                }
            }
            return LBoundIdx;
        }
        private static int FindRightBound(List<MSPoint> argMSPs, int argApexIdx, float argDelta)
        {
            int RBoundIdx = argApexIdx + 1;

            if (argApexIdx == argMSPs.Count - 1) //ApexIdx is the Right most point
            {
                RBoundIdx = argApexIdx;
                return RBoundIdx;
            }

            if (argApexIdx > argMSPs.Count)
            {
                return argApexIdx;
            }
            if (argMSPs[RBoundIdx].Mass - argMSPs[argApexIdx].Mass > argDelta)
            {
                return argApexIdx;
            }


            for (int i = argApexIdx +1; i < argMSPs.Count; i++)
            {
                if (argMSPs[i].Mass - argMSPs[RBoundIdx].Mass > argDelta)
                {
                    break;
                }
                if (argMSPs[i].Intensity - argMSPs[RBoundIdx].Intensity <= 0)
                {
                    RBoundIdx = i;
                }
                else
                {
                    if (argMSPs[i].Mass - argMSPs[argApexIdx].Mass > LCApexMaxTolTime) //Mass filed store LC time.  If two apex is far away  the this is left bound
                    {
                        break;
                    }
                    else
                    {
                        RBoundIdx = i;
                    }
                }
            }
            return RBoundIdx;
        }
        public static List<LCPeak> PeakFindingOld(List<MSPoint> argMSPs, float argDelta)
        {
            List<LCPeak> DetectedPeaks = new List<LCPeak>();

            List<KeyValuePair<int, float>> MaxPoint = new List<KeyValuePair<int, float>>();  //MSPoint index, Intensity
            List<KeyValuePair<int, float>> MinPoint = new List<KeyValuePair<int, float>>();
            float delta = argDelta;
            //Peak Detection  http://www.billauer.co.il/peakdet.html
            float mn = float.PositiveInfinity;
            float mx = float.NegativeInfinity;
            int mnpos = -1;
            int mxpos = -1;
            bool LookForMax = true;
            for (int i = 0; i < argMSPs.Count; i++)
            {
                float Intensity = argMSPs[i].Intensity;
                if (Intensity > mx)
                {
                    mx = Intensity;
                    mxpos = i;
                }
                if (Intensity < mn)
                {
                    mn = Intensity;
                    mnpos = i;
                }

                if (LookForMax)
                {
                    if (Intensity < delta)
                    {
                        MaxPoint.Add(new KeyValuePair<int, float>(mxpos, mx));
                        mn = Intensity;
                        mnpos = i;
                        LookForMax = false;
                    }
                }
                else
                {
                    if (Intensity > mn + delta)
                    {
                        MinPoint.Add(new KeyValuePair<int, float>(mnpos, mn));
                        mx = Intensity;
                        mxpos = i;
                        LookForMax = true;
                    }
                }
            }
            return DetectedPeaks;
        }
    }
    
}


/*
      List<KeyValuePair<int, float>> MaxPoint = new List<KeyValuePair<int, float>>();  //MSPoint index, Intensity
            List<KeyValuePair<int, float>> MinPoint = new List<KeyValuePair<int, float>>();
            float delta = argDelta;
            //Peak Detection  http://www.billauer.co.il/peakdet.html
            float mn = float.PositiveInfinity;
            float mx = float.NegativeInfinity;
            int mnpos = -1;
            int mxpos = -1;
            bool LookForMax = true;
            for (int i = 0; i < argMSPs.Count; i++)
            {
                float Intensity = argMSPs[i].Intensity;
                if (Intensity > mx)
                {
                    mx = Intensity;
                    mxpos = i;
                }
                if (Intensity < mn)
                {
                    mn = Intensity;
                    mnpos = i;
                }

                if (LookForMax)
                {
                    if (Intensity < delta)
                    {
                        MaxPoint.Add(new KeyValuePair<int,float>( mxpos,mx));
                        mn = Intensity;
                        mnpos = i;
                        LookForMax = false;
                    }
                }
                else
                {
                    if (Intensity > mn + delta)
                    {
                        MinPoint.Add(new KeyValuePair<int, float>(mnpos, mn));
                        mx = Intensity;
                        mxpos = i;
                        LookForMax = true;
                    }
                }
            }in my off

*/