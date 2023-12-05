using System;
using System.Collections.Generic;
using System.Text;
namespace COL.MassLib.MascotParser
{
    class Unimod
    {
        string _ModifiedSiteAA;
        int _ModifiedSiteIdx;
        string _ModifiedType;
        public Unimod(string argModType, string argModAA, int argModIdx)
        {
            _ModifiedType = argModType;
            _ModifiedSiteAA = argModAA;
            _ModifiedSiteIdx = argModIdx;            
        }
        public string ModifiedSiteAminiAcid
        {
            get { return _ModifiedSiteAA; }
        }
        public string ModifiedType
        {
            get { return _ModifiedType; }
        }
        public int ModifiedSiteIndex
        {
            get { return _ModifiedSiteIdx; }
        }
    }
}
