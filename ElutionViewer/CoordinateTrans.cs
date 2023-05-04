using System;
using System.Collections.Generic;
using System.Text;

namespace COL.ElutionViewer
{
    public class CoordinateTrans
    {
        public static float TimeToX(RegionSize TimeRegion, RegionSize XRegion, float Time)
        {
            float X = XRegion.LeftBound + ((Time - TimeRegion.LeftBound) * XRegion.Width / TimeRegion.Width);
            return X;
        }
        public static float MassToY(RegionSize MassRegion, RegionSize YRegion, float Mass)
        {
            float Y = YRegion.BottomBound + ((Mass - MassRegion.BottomBound) * YRegion.Height / MassRegion.Height);
            return Y;
        }
        public static float XToTime(RegionSize XRegion, RegionSize TimeRegion, float X)
        {
            float Time = TimeRegion.LeftBound + ((X - XRegion.LeftBound) * TimeRegion.Width / XRegion.Width);
            return Time;
        }
        public static float YToMass(RegionSize YRegion, RegionSize MassRegion, float Y)
        {
            float Mass = MassRegion.BottomBound + ((Y - YRegion.BottomBound) * MassRegion.Height / YRegion.Height);
            return Mass;
        }
    }
}
