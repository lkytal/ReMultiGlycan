using System;
using System.Collections.Generic;
using System.Text;

namespace COL.ProtLib
{
    public class FastaFile
    {
        private string _fastaFullPath;
        private List<ProteinInfo> _proteinInfo;
        private int _misscleavaged;
        public FastaFile(string argFastaFullPath)
        {
            _fastaFullPath = argFastaFullPath;            
            _proteinInfo = FastaReader.ReadFasta(_fastaFullPath);
        }
        public List<string> CreateCleavedPeptides(int argAllowMissCleavage, Protease.Type argProteaseType)
        {
            _misscleavaged = argAllowMissCleavage;
             List<string> peptides  = new List<string>();
            foreach (ProteinInfo prot in _proteinInfo)
            {
                peptides.AddRange(prot.CreateCleavage(_misscleavaged, argProteaseType));
            }
            return peptides;
        }
        public List<string> CreateCleavedPeptides(int argAllowMissCleavage, List<Protease.Type> argProteaseType)
        {
            _misscleavaged = argAllowMissCleavage;
            List<string> peptides = new List<string>();
            foreach (ProteinInfo prot in _proteinInfo)
            {
                peptides.AddRange(prot.CreateCleavage(_misscleavaged, argProteaseType));
            }
            return peptides;
        }
        public List<string> CreateCleavedOGlycoPeptides(int argAllowMissCleavage, List<Protease.Type> argProteaseType)
        {
            _misscleavaged = argAllowMissCleavage;
              List<string> glycopeptides  = new List<string>();
            foreach (ProteinInfo prot in _proteinInfo)
            {
                glycopeptides.AddRange(prot.OGlycopeptide(_misscleavaged, argProteaseType));
            }
            return glycopeptides;
        }
        public List<string> CreateCleavedNGlycoPeptides(int argAllowMissCleavage, List<Protease.Type> argProteaseType)
        {
            return CreateCleavedNGlycoPeptides(argAllowMissCleavage,argProteaseType, enumPeptideMutation.NoMutation);
        }
        public List<string> CreateCleavedNGlycoPeptides(int argAllowMissCleavage, List<Protease.Type> argProteaseType, enumPeptideMutation argMutation)
        {
            _misscleavaged = argAllowMissCleavage;
            List<string> glycopeptides  = new List<string>();
            foreach (ProteinInfo prot in _proteinInfo)
            {
               glycopeptides.AddRange(prot.NGlycopeptide(_misscleavaged, argProteaseType, argMutation));
            }
            return glycopeptides;
        }
    }
}
