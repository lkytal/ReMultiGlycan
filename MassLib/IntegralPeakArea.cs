using System;
using System.Collections.Generic;
using System.Text;

namespace COL.MassLib
{
    public static class IntegralPeakArea
    {
        public static double IntegralArea(List<MSPoint> argMSP)
        {
            double AreaOfCurve = 0.0;

            for (int i = 0; i < argMSP.Count - 1; i++)
            {
                AreaOfCurve = AreaOfCurve + ((argMSP[i + 1].Mass - argMSP[i].Mass) * ((argMSP[i + 1].Intensity + argMSP[i].Intensity) / 2));

            }
            return AreaOfCurve;
        }
        public static double IntegralArea(LCPeak argLCPeak)
        {
            double AreaOfCurve = 0.0;
            if (argLCPeak.RawPoint.Count > 0)
            {
                AreaOfCurve = IntegralArea(argLCPeak.RawPoint);
            }
            return AreaOfCurve;
        }
    }
}
