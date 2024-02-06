using System;

namespace COL.GlycoLib
{
    [Serializable]
    public class Glycan
    {
        public enum Type
        {
            HexNAc = 1,
            Hex,
            DeHex,
            NeuAc,
            NeuGc,
            Man,
            Gal
        }

        public Glycan(Type argType, int argCharge)
        {
            GlycanType = argType;
            Charge = argCharge;
        }

        public Type GlycanType { get; }

        public int Charge { get; }

        public float Mz => GlycanMass.GetGlycanMasswithCharge(GlycanType, Charge);

        public float AVGMz => GlycanMass.GetGlycanAVGMasswithCharge(GlycanType, Charge);

        public float Mass => GlycanMass.GetGlycanMass(GlycanType);

        public float AVGMass => GlycanMass.GetGlycanAVGMass(GlycanType);

        public static Type String2GlycanType(string argType)
        {
            if (argType.ToLower().Contains("glcnac") || argType.ToLower().Contains("hexnac"))
                return Type.HexNAc;
            if (argType.ToLower().Contains("fuc") || argType.ToLower().Contains("dehex"))
                return Type.DeHex;
            if (argType.ToLower().Contains("gal"))
                return Type.Hex;
            if (argType.ToLower().Contains("neuac"))
                return Type.NeuAc;
            if (argType.ToLower().Contains("neugc"))
                return Type.NeuGc;
            if (argType.ToLower().Contains("man") || argType.ToLower().Contains("hex"))
                return Type.Hex;
            throw new Exception("IUPAC contain unrecognized glycan or string");
        }
    }
}