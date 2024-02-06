namespace COL.GlycoLib
{
	public class GlycanMass
	{
		//          z=1      z=2         z=3         z=4
		//Hex       162.0528	81.0264	    54.0176	    40.5132
		//DeHex     146.0579 	73.029	    48.686	    36.5145
		//HexNac    203.0794	101.5397	67.6931	    50.7699
		//NeuAc     291.0954	145.5477	97.0318	    72.7739

		private const float _HexNAc = 203.0794f;
		private const float _HexNAcAVG = 203.1950f;
		private const float _Hex = 162.0528f;
		private const float _HexAVG = 162.1424f;
		private const float _DeHex = 146.0579f;
		private const float _DeHexAVG = 146.1430f;
		private const float _NeuAc = 291.0954f;
		private const float _NeuAcAVG = 291.2579f;
		private const float _NeuGc = 307.0903f;
		private const float _NeuGcAVG = 307.2573f;

		public static float GetGlycanMass(Glycan.Type argType)
		{
			float Mass = 0.0f;
            switch (argType)
            {
                case Glycan.Type.HexNAc:
                    Mass = _HexNAc;
                    break;
                case Glycan.Type.Hex:
                    Mass = _Hex;
                    break;
                case Glycan.Type.DeHex:
                    Mass = _DeHex;
                    break;
                case Glycan.Type.NeuAc:
                    Mass = _NeuAc;
                    break;
                default:
                    Mass = _NeuGc;
                    break;
            }
            return Mass;
		}

		public static float GetGlycanAVGMass(Glycan.Type argType)
		{
			float Mass = 0.0f;
            switch (argType)
            {
                case Glycan.Type.HexNAc:
                    Mass = _HexNAcAVG;
                    break;
                case Glycan.Type.Hex:
                    Mass = _HexAVG;
                    break;
                case Glycan.Type.DeHex:
                    Mass = _DeHexAVG;
                    break;
                case Glycan.Type.NeuAc:
                    Mass = _NeuAcAVG;
                    break;
                default:
                    Mass = _NeuGcAVG;
                    break;
            }
            return Mass;
		}

		public static float GetGlycanMasswithCharge(Glycan.Type argType, int argCharge)
		{
			//
			// m/z = (mass  + Proton * charge) / charge
			//     => mass/charge + Proton
			//
			return (GetGlycanMass(argType) + MassLib.Atoms.ProtonMass * argCharge) / argCharge;
		}

		public static float GetGlycanAVGMasswithCharge(Glycan.Type argType, int argCharge)
		{
			//
			// m/z = (mass  + Proton * charge) / charge
			//     => mass/charge + Proton
			//

			return (GetGlycanAVGMass(argType) + MassLib.Atoms.ProtonMass * argCharge) / argCharge;
		}

		public static float GetGlycanMass(GlycanCompound argGlycanComp)
		{
			float Mass = 0.0f;
			Mass = argGlycanComp.NoOfHex * _Hex +
						  argGlycanComp.NoOfHexNAc * _HexNAc +
						  argGlycanComp.NoOfDeHex * _DeHex;
			if (argGlycanComp.isHuman)
			{
				Mass += argGlycanComp.NoOfSia * _NeuAc;
			}
			else
			{
				Mass += argGlycanComp.NoOfSia * _NeuGc;
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
				Mass += argGlycanComp.NoOfSia * _NeuAcAVG;
			}
			else
			{
				Mass += argGlycanComp.NoOfSia * _NeuGcAVG;
			}
			return Mass;
		}

		public static float GetGlycanMasswithCharge(GlycanCompound argGlycanComp, int argCharge)
		{
			return (GetGlycanMass(argGlycanComp) + MassLib.Atoms.ProtonMass * argCharge) / argCharge;
		}

		public static float GetGlycanAVGMasswithCharge(GlycanCompound argGlycanComp, int argCharge)
		{
			return (GetGlycanAVGMass(argGlycanComp) + MassLib.Atoms.ProtonMass * argCharge) / argCharge;
		}
	}
}