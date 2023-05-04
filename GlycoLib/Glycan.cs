using System;

namespace COL.GlycoLib
{
	[Serializable]
	public class Glycan
	{
		public enum Type
		{ HexNAc = 1, Hex, DeHex, NeuAc, NeuGc, Man, Gal }

		private Type _type;
		private int _charge;

		public Glycan(Type argType, int argCharge)
		{
			_type = argType;
			_charge = argCharge;
		}

		public Type GlycanType => _type;

		public int Charge => _charge;

		public float Mz => GlycanMass.GetGlycanMasswithCharge(_type, _charge);

		public float AVGMz => GlycanMass.GetGlycanAVGMasswithCharge(_type, _charge);

		public float Mass => GlycanMass.GetGlycanMass(_type);

		public float AVGMass => GlycanMass.GetGlycanAVGMass(_type);

		public static Glycan.Type String2GlycanType(string argType)
		{
			if (argType.ToLower().Contains("glcnac") || argType.ToLower().Contains("hexnac"))
			{
				return Glycan.Type.HexNAc;
			}
			else if (argType.ToLower().Contains("fuc") || argType.ToLower().Contains("dehex"))
			{
				return Glycan.Type.DeHex;
			}
			else if (argType.ToLower().Contains("gal"))
			{
				return Glycan.Type.Hex;
			}
			else if (argType.ToLower().Contains("neuac"))
			{
				return Glycan.Type.NeuAc;
			}
			else if (argType.ToLower().Contains("neugc"))
			{
				return Glycan.Type.NeuGc;
			}
			else if (argType.ToLower().Contains("man") || argType.ToLower().Contains("hex"))
			{
				return Glycan.Type.Hex;
			}
			else
			{
				throw new Exception("IUPAC contain unrecognized glycan or string");
			}
		}
	}
}