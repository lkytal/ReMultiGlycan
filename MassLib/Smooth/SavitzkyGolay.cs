using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using COL.MassLib;
namespace COL.MassLib.Smooth
{
    public class SavitzkyGolay
    {
        private static float[][] table;
        public enum FILTER_WIDTH { FILTER_WIDTH_5 = 0, FILTER_WIDTH_7 = 1, FILTER_WIDTH_9 = 2, FILTER_WIDTH_11 = 3 };

        public static List<MSPeak> Smooth(List<MSPeak> argPeaks, FILTER_WIDTH argFilterWidth)
        {
            table = new float[4][];
            table[0] = new float[] { -3, 12, 17, 12, -3 };
            table[1] = new float[] { -2, 3, 6, 7, 6, 3, -2 };
            table[2] = new float[] { -21, 14, 39, 54, 59, 54, 39, 14, -21 };
            table[3] = new float[] { -36, 9, 44, 69, 84, 89, 84, 69, 44, 9, -36 };
            if (argPeaks == null)
            {
                return null;
            }
            else if (argPeaks.Count <= 1)
            {
                List<MSPeak> smoothed = new List<MSPeak>();
                smoothed.AddRange(argPeaks);                
                return smoothed;
            }
            else
            {
                List<MSPeak> smoothed = new List<MSPeak>();
                List<MSPeak> temp = new List<MSPeak>();

                int extra = (table[(int)argFilterWidth].Length - 1) / 2;
                float sumOfFilter = 0;
                foreach (float value in table[(int)argFilterWidth])
                {
                    sumOfFilter += value;
                }

                //12345678
                //add pre node
                for (int i = 0; i < extra; i++)
                {
                    temp.Add(argPeaks[0]);
                }

                //add rawdata
                temp.AddRange(argPeaks);
 

                //add post node
                for (int i = 0; i < extra; i++)
                {
                    temp.Add(argPeaks[argPeaks.Count - 1]);
                }

                //111234567888

                for (int i = extra; i < temp.Count - extra; i++)
                {
                    float sumwidth = 0.0f;
                    for (int j = (-1 * extra); j <= extra; j++)
                    {
                        sumwidth += temp[i + j].MonoIntensity * table[(int)argFilterWidth][j + extra];
                    }
                    float newy = sumwidth / sumOfFilter;
                    if (newy < 0) //Savitzky Golay 可能會有小於0的y
                    {
                        newy = 0;
                    }
                    smoothed.Add(new MSPeak(temp[i].MonoisotopicMZ, newy));
                }
                return smoothed;
            }
        }
        public static List<MSPoint> Smooth(List<MSPoint> argPeaks, FILTER_WIDTH argFilterWidth)
        {
            table = new float[4][];
            table[0] = new float[] { -3, 12, 17, 12, -3 };
            table[1] = new float[] { -2, 3, 6, 7, 6, 3, -2 };
            table[2] = new float[] { -21, 14, 39, 54, 59, 54, 39, 14, -21 };
            table[3] = new float[] { -36, 9, 44, 69, 84, 89, 84, 69, 44, 9, -36 };
            if (argPeaks == null)
            {
                return null;
            }
            else if (argPeaks.Count <= 1)
            {
                List<MSPoint> smoothed = new List<MSPoint>();
                smoothed.AddRange(argPeaks);
                return smoothed;
            }
            else
            {
                List<MSPoint> smoothed = new List<MSPoint>();
                List<MSPoint> temp = new List<MSPoint>();

                int extra = (table[(int)argFilterWidth].Length - 1) / 2;
                float sumOfFilter = 0;
                foreach (float value in table[(int)argFilterWidth])
                {
                    sumOfFilter += value;
                }

                //12345678
                //add pre node
                for (int i = 0; i < extra; i++)
                {
                    temp.Add(argPeaks[0]);
                }

                //add rawdata
                temp.AddRange(argPeaks);


                //add post node
                for (int i = 0; i < extra; i++)
                {
                    temp.Add(argPeaks[argPeaks.Count - 1]);
                }

                //111234567888

                for (int i = extra; i < temp.Count - extra; i++)
                {
                    float sumwidth = 0.0f;
                    for (int j = (-1 * extra); j <= extra; j++)
                    {
                        sumwidth += temp[i + j].Intensity * table[(int)argFilterWidth][j + extra];
                    }
                    float newy = sumwidth / sumOfFilter;
                    if (newy < 0) //Savitzky Golay 可能會有小於0的y
                    {
                        newy = 0;
                    }
                    smoothed.Add(new MSPoint(temp[i].Mass, newy));
                }
                return smoothed;
            }
        }
    }
}