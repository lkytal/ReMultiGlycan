using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using COL.ProtLib;
using COL.GlycoLib;
namespace COL.GlycoLib
{
    public static class GlycanTools
    {
        public static float GetGlycopeptideMZ(string argPeptide, GlycanStructure argGlycan, int argCharge)
        {
            float TotalMZ = 0.0f;
            AminoAcidMass AAMS = new AminoAcidMass();
            float PeptideMonoMass = AAMS.GetAVGMonoMW(argPeptide, true);
            float GlycanMass = 0.0f;
            GlycanMass = GlycanMass + argGlycan.NoOfHex*GlycoLib.GlycanMass.GetGlycanAVGMass(Glycan.Type.Hex);
            GlycanMass = GlycanMass + argGlycan.NoOfHexNac * GlycoLib.GlycanMass.GetGlycanAVGMass(Glycan.Type.HexNAc);
            GlycanMass = GlycanMass + argGlycan.NoOfDeHex * GlycoLib.GlycanMass.GetGlycanAVGMass(Glycan.Type.DeHex);
            GlycanMass = GlycanMass + argGlycan.NoOfNeuAc * GlycoLib.GlycanMass.GetGlycanAVGMass(Glycan.Type.NeuAc);
            GlycanMass = GlycanMass + argGlycan.NoOfNeuGc * GlycoLib.GlycanMass.GetGlycanAVGMass(Glycan.Type.NeuGc);
            TotalMZ = (PeptideMonoMass + GlycanMass + MassLib.Atoms.ProtonMass*argCharge)/argCharge;
            return TotalMZ;
        }
    }
}
