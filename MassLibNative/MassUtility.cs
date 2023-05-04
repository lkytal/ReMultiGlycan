using System;
using System.Collections.Generic;
using System.Text;

namespace COL.MassLib
{
    public class MassUtility
    {
        public static double GetMassPPM(double argExactMass, double argMeasureMass)
        {
            return Math.Abs(Convert.ToDouble(((argMeasureMass - argExactMass) / argExactMass) * Math.Pow(10.0, 6.0)));
        }
        public static double GetMassPPM(float argExactMass, float argMeasureMass)
        {
            return Math.Abs(Convert.ToSingle(((argMeasureMass - argExactMass) / argExactMass) * Math.Pow(10.0, 6.0)));
        }
        public static int GetClosestMassIdx(List<MSPoint> argPoints, float argMZ)
        {
            if (argPoints.Count == 0)
            {
                return -1;
            }
            argPoints.Sort();
            List<float> lstCIDMz = new List<float>();
            for (int i = 0; i < argPoints.Count; i++)
            {
                lstCIDMz.Add(argPoints[i].Mass);
            }
            return GetClosestMassIdx(lstCIDMz, argMZ);
        }
        public static int GetClosestMassIdx(List<MSPeak> argPeaks, float argMZ)
        {
            if (argPeaks.Count == 0)
            {
                return -1;
            }
            //Convert MSPeaks mz into float[]
            argPeaks.Sort();
            List<float> lstCIDMz = new List<float>();
            for(int i =0; i<argPeaks.Count;i++)
            {
                  lstCIDMz.Add(argPeaks[i].MonoMass);
            }
            return GetClosestMassIdx(lstCIDMz, argMZ);
           
        }
        public static List<int> GetClosestMassIdxsWithinPPM(float[] argPeaks, float argMZ, float argPPM)
        {
            if (argPeaks.Length == 0)
            {
                return new List<int>();
            }
            List<float> lstCIDMz = new List<float>();
            for (int i = 0; i < argPeaks.Length; i++)
            {
                lstCIDMz.Add(argPeaks[i]);
            }

            return GetClosestMassIdxsWithinPPM(lstCIDMz, argMZ, argPPM);
        }
        public static List<int> GetClosestMassIdxsWithinPPM(List<MSPeak> argPeaks, float argMZ,float argPPM)
        {
            if (argPeaks.Count == 0)
            {
                return new List<int>();
            }
            argPeaks.Sort();
            List<float> lstCIDMz = new List<float>();
            for (int i = 0; i < argPeaks.Count; i++)
            {
                lstCIDMz.Add(argPeaks[i].MonoMass);
            }

            return GetClosestMassIdxsWithinPPM(lstCIDMz, argMZ, argPPM);
        }
        public static List<int> GetClosestMassIdxsWithinPPM(List<float> argPeaks, float argMZ, float argPPM)
        {
            if (argPeaks.Count == 0)
            {
                return new List<int>();
            }
            List<int> ClosedIdxs = new List<int>();
            //Convert MSPeaks mz into float[]
            argPeaks.Sort();
            List<float> lstCIDMz = argPeaks;
            int ClosedIdx = GetClosestMassIdx(lstCIDMz, argMZ);
            for (int i = ClosedIdx; i >= 0; i--)
            {
                if (GetMassPPM(lstCIDMz[i], argMZ) <= argPPM)
                {
                    ClosedIdxs.Add(i);
                }
                else  //No small peak within PPM
                {
                    break;
                }
            }
            for (int i = ClosedIdx + 1; i < lstCIDMz.Count; i++)
            {
                if (GetMassPPM(lstCIDMz[i], argMZ) <= argPPM)
                {
                    ClosedIdxs.Add(i);
                }
                else  //No small peak within PPM
                {
                    break;
                }
            }
            ClosedIdxs.Sort();
            return ClosedIdxs;
        }
        public static int GetClosestMassIdx(List<float> argPeaks, float argMZ)
        {
            if (argPeaks.Count == 0)
            {
                return -1;
            }
            int KeyIdx = argPeaks.BinarySearch(argMZ);
            if (KeyIdx < 0)
            {
                KeyIdx = ~KeyIdx;
            }

            int ClosetIdx = 0;
            double ClosestValue = 10000.0;
            for (int i = KeyIdx - 2; i <= KeyIdx + 2; i++)
            {
                if (i >= 0 && i < argPeaks.Count)
                {
                    if (Math.Abs(argPeaks[i] - argMZ) <= ClosestValue)
                    {
                        ClosestValue = Math.Abs(argPeaks[i] - argMZ);
                        ClosetIdx = i;
                    }
                }
            }
            return ClosetIdx;
        }
        public static int GetClosestMassIdx(float[] argPeaks, float argMZ)
        {
            if (argPeaks.Length == 0)
            {
                return -1;
            }
            int KeyIdx = Array.BinarySearch(argPeaks, argMZ);
            if (KeyIdx < 0)
            {
                KeyIdx = ~KeyIdx;
            }

            int ClosetIdx = 0;
            double ClosestValue = 10000.0;
            for (int i = KeyIdx - 2; i <= KeyIdx + 2; i++)
            {
                if (i >= 0 && i < argPeaks.Length)
                {
                    if (Math.Abs(argPeaks[i] - argMZ) <= ClosestValue)
                    {
                        ClosestValue = Math.Abs(argPeaks[i] - argMZ);
                        ClosetIdx = i;
                    }
                }
            }
            return ClosetIdx;
        }
    }
}
