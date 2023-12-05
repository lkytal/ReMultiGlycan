using System;
using System.Collections.Generic;
using System.Text;

namespace COL.MassLib.MascotParser
{
    public class Summary
    {
        int _QueryNo;
        float _Mass;
        float _MZ;
        int _Charge;
        int _Match;
        public Summary(int argQueryNo, float argMass,float argMZ, int argCharge, int argMatch)
        {
            _QueryNo = argQueryNo;
            _Mass = argMass;
            _MZ = argMZ;
            _Charge = argCharge;
            _Match = argMatch;
        }
        public int QueryNo
        {
            get { return _QueryNo; }
        }
        public float Mass
        {
            get { return _Match; }
        }
        public float MZ
        {
            get { return _MZ; }
        }
        public int Charge
        {
            get { return _Charge; }
        }
        public int Match
        {
            get { return _Match; }
        }
    }
}
