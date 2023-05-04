using System;
using System.Collections.Generic;
using System.Text;

namespace COL.GlycoLib
{
    class ScorePeak
    {
        private COL.MassLib.MSPoint _pt;
        private GlycoLib.GlycanCompound _comp;
        public ScorePeak(COL.MassLib.MSPoint argMSPoint, GlycoLib.GlycanCompound argComposition)
        {
            _pt = argMSPoint;
            _comp = argComposition;
        }
    }
}
