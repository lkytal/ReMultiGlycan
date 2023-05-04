using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COL.MassLib.Filters
{
    public class PeakPickingFilter : IFilter
    {
        private MSScan _msScan;
        public PeakPickingFilter(MSScan argScan)
        {
            _msScan = argScan;
        }
            
        public void ApplyFilter()
        {

        }
    }
}
