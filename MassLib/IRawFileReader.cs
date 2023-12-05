using System;
using System.Collections.Generic;
using System.Text;

namespace COL.MassLib
{
    public interface IRawFileReader
    {
                
        MSScan ReadScan(int argScan);
        List<MSScan> ReadScans(int argStart, int argEnd);
        List<MSScan> ReadAllScans();
        List<MSScan> ReadScanWMSLevel(int argStart, int argEnd, int argMSLevel);
        //void SetPeakProcessorParameter(GlypID.Peaks.clsPeakProcessorParameters argPeakProcessorParameter);
        //void SetTransformParameter(GlypID.HornTransform.clsHornTransformParameters argTransformParameter);
        int NumberOfScans { get;}
        int GetMsLevel(int argScan);        
    }
}
