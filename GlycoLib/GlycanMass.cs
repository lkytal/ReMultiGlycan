using System;
using System.Collections.Generic;
using System.Text;

namespace COL.GlycoLib
{
    public class GlycanMass
    {   
        //          z=1      z=2         z=3         z=4
        //Hex       162.0528	81.0264	    54.0176	    40.5132
        //DeHex     146.0579 	73.029	    48.686	    36.5145
        //HexNac    203.0794	101.5397	67.6931	    50.7699
        //NeuAc     291.0954	145.5477	97.0318	    72.7739


        private const float _HexNAc= 203.0794f;
        private const float _HexNAcAVG = 203.1950f;
        private const float _Hex= 162.0528f;
        private const float _HexAVG = 162.1424f;
        private const float _DeHex= 146.0579f;
        private const float _DeHexAVG = 146.1430f;
        private const float _NeuAc= 291.0954f;
        private const float _NeuAcAVG = 291.2579f;
        private const float _NeuGc = 307.0903f;
        private const float _NeuGcAVG = 307.2573f;      

        
        public static float GetGlycanMass(Glycan.Type argType)
        {
            float Mass = 0.0f;
            if (argType == Glycan.Type.HexNAc)
            {
                Mass = _HexNAc;
            }
            else if (argType == Glycan.Type.Hex)
            {
                Mass = _Hex;
            }
            else if (argType == Glycan.Type.DeHex)
            {
                Mass = _DeHex;
            }
            else if (argType == Glycan.Type.NeuAc)
            {
                Mass = _NeuAc;
            }
            else
            {
                Mass = _NeuGc;
            }
            return Mass ;
        }
        public static float GetGlycanAVGMass(Glycan.Type argType)
        {
            float Mass = 0.0f;
            if (argType == Glycan.Type.HexNAc)
            {
                Mass = _HexNAcAVG;
            }
            else if (argType == Glycan.Type.Hex)
            {
                Mass = _HexAVG;
            }
            else if (argType == Glycan.Type.DeHex)
            {
                Mass = _DeHexAVG;
            }
            else if (argType == Glycan.Type.NeuAc)
            {
                Mass = _NeuAcAVG;
            }
            else
            {
                Mass = _NeuGcAVG;
            }
            return Mass;
        }
        public static float GetGlycanMasswithCharge(Glycan.Type argType, int argCharge)
        {
            //
            // m/z = (mass  + Proton * charge) / charge  
            //     => mass/charge + Proton
            //
            return (float)((GetGlycanMass(argType) + MassLib.Atoms.ProtonMass * argCharge) / argCharge);
        }
        public static float GetGlycanAVGMasswithCharge(Glycan.Type argType, int argCharge)
        {
            //
            // m/z = (mass  + Proton * charge) / charge  
            //     => mass/charge + Proton
            //
 
             return (float)((GetGlycanAVGMass(argType) + MassLib.Atoms.ProtonMass* argCharge) / argCharge);
            
        }

        public static float GetGlycanMass(GlycanCompound argGlycanComp)
        {
            float Mass = 0.0f;
            Mass = argGlycanComp.NoOfHex * _Hex +
                          argGlycanComp.NoOfHexNAc * _HexNAc +
                          argGlycanComp.NoOfDeHex * _DeHex;
            if (argGlycanComp.isHuman)
            {
                Mass = Mass + argGlycanComp.NoOfSia * _NeuAc;
            }
            else
            {
                Mass = Mass + argGlycanComp.NoOfSia * _NeuGc;
            }
            return Mass;
        }
        public static float GetGlycanAVGMass(GlycanCompound argGlycanComp)
        {
            float Mass = 0.0f;
            Mass = argGlycanComp.NoOfHex * _HexAVG +
                          argGlycanComp.NoOfHexNAc * _HexNAcAVG +
                          argGlycanComp.NoOfDeHex * _DeHexAVG;
            if (argGlycanComp.isHuman)
            {
                Mass = Mass + argGlycanComp.NoOfSia * _NeuAcAVG;
            }
            else
            {
                Mass = Mass + argGlycanComp.NoOfSia * _NeuGcAVG;
            }
            return Mass;
        }
        public static float GetGlycanMasswithCharge(GlycanCompound argGlycanComp, int argCharge)
        {
            return (float)((GetGlycanMass(argGlycanComp) + MassLib.Atoms.ProtonMass* argCharge) / argCharge);
        }
        public static float GetGlycanAVGMasswithCharge(GlycanCompound argGlycanComp, int argCharge)
        {
            return (float)((GetGlycanAVGMass(argGlycanComp) + MassLib.Atoms.ProtonMass * argCharge) / argCharge);
        }
    }
}
