using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
namespace COL.ProtLib
{
    public class ProteinInfo
    {
        //http://web.expasy.org/peptide_mass/peptide-mass-doc.html
        private string _title;
        private string _sequence;
        public ProteinInfo(string argTitle, string argSequence)
        {
            _title = argTitle;
            _sequence = argSequence.ToUpper();
        }
        public string Title
        {
            get { return _title; }
        }
        public string Sequence
        {
            get { return _sequence; }
        }
        public List<string> NGlycopeptide(int argAllowMissCleavage, List<Protease.Type> argProteaseType, enumPeptideMutation argMutation)
        {
            List<string> _glycopep = new List<string>();

            //Native Part
            List<string> _cleavages = CreateCleavage(_sequence, argAllowMissCleavage, argProteaseType);
            foreach (string pep in _cleavages)
            {
                if (ContainSequon(pep) && !_glycopep.Contains(pep))
                {
                    _glycopep.Add(pep);
                }
            }
            //Mutation Part
            string MutatedPeptide = MutateSequence(_sequence,argMutation);
           _cleavages = CreateCleavage(MutatedPeptide,argAllowMissCleavage, argProteaseType);
          

            foreach (string pep in _cleavages)
            {
                if (ContainSequon(pep) && !_glycopep.Contains(pep))
                {
                    _glycopep.Add(pep);
                }
            }
            
            return _glycopep;
        }

        public bool ContainSequon(string argDigustPeptide)
        {
            Regex sequon = new Regex("N[ARNDCEQGHILKMFSTWYV][S|T]", RegexOptions.IgnoreCase);  //NXS NXT  X!=P
            
            Match Sequon = sequon.Match(argDigustPeptide);
            if (Sequon.Length != 0)
            {
                return true;
            }
            Regex sequonEnd = new Regex("N[ARNDCEQGHILKMFSTWYV]$", RegexOptions.IgnoreCase);  //NX NX  X!=P in the end
            Match SequonEnd = sequonEnd.Match(argDigustPeptide); //Go to full sequence to check if the sequence contain S/T
            if (SequonEnd.Length != 0)
            {
                int idx = _sequence.IndexOf(argDigustPeptide) + argDigustPeptide.Length;
                if (idx < _sequence.Length)
                {
                    if (_sequence[idx] == 'S' || _sequence[idx] == 'T')
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private string MutateSequence(string argSequence, enumPeptideMutation argMutation)
        {
            if (argMutation == enumPeptideMutation.NoMutation)
            {
                return argSequence;
            }
            else if (argMutation == enumPeptideMutation.DtoN)
            {
                StringBuilder sb = new StringBuilder(argSequence);
                foreach (
                    Match match in Regex.Matches(argSequence, "D[ARNDCEQGHILKMFSTWYV][S|T]", RegexOptions.IgnoreCase))
                    //DXS DXT  X!=P
                {
                    sb.Remove(match.Index, 1);
                    sb.Insert(match.Index, "N");
                }
                return sb.ToString();
            }
            else //(argMutation == enumPeptideMutation.AnyToN)
            {
                StringBuilder sb = new StringBuilder(argSequence);
                foreach (
                    Match match in Regex.Matches(argSequence, "[ARNDCEQGHILKMFSTWYV][S|T]", RegexOptions.IgnoreCase))
                //XS XT  X!=P
                {
                    if (match.Index == 0)
                    {
                        continue;
                    }
                    sb.Remove(match.Index-1, 1);
                    sb.Insert(match.Index-1, "N");
                }
                return sb.ToString();
            }
        }
        /// <summary>
        /// Old function, no use anymore
        /// </summary>
        /// <param name="argSequence"></param>
        /// <param name="argMutation"></param>
        /// <param name="lstGlycopeptideSeq"></param>
        /// <returns></returns>
        public bool IsGlycopeptide(string argSequence, enumPeptideMutation argMutation, out List<string> lstGlycopeptideSeq)
        {
            List<string> _glycopep = new List<string>();
            Regex sequon = new Regex("N[ARNDCEQGHILKMFSTWYV][S|T]", RegexOptions.IgnoreCase);  //NXS NXT  X!=P
            Regex sequonEnd = new Regex("N[ARNDCEQGHILKMFSTWYV]$", RegexOptions.IgnoreCase);  //NX NX  X!=P in the end
            Match Sequon = sequon.Match(argSequence);
            if (Sequon.Length != 0)
            {
                _glycopep.Add(argSequence);
            }

            Match SequonEnd = sequonEnd.Match(argSequence); //Go to full sequence to check if the sequence contain S/T
            if (SequonEnd.Length != 0)
            {
                int idx = _sequence.IndexOf(argSequence) + argSequence.Length;
                if (idx < _sequence.Length)
                {
                    if (_sequence[idx] == 'S' || _sequence[idx] == 'T')
                    {
                        _glycopep.Add(argSequence);
                    }
                }
            }

            if (argMutation == enumPeptideMutation.DtoN)
            {
                Regex sequonMutaDN = new Regex("D[ARNDCEQGHILKMFSTWYV][S|T]", RegexOptions.IgnoreCase);  //DXS DXT  X!=P
                Regex sequonMutaDNEnd = new Regex("D[ARNDCEQGHILKMFSTWYV]$", RegexOptions.IgnoreCase);  //DX DX  X!=P in the end

                foreach (Match SequonMutaDN in sequonMutaDN.Matches(argSequence))
                {
                    if (SequonMutaDN.Length != 0)
                    {
                        StringBuilder sb = new StringBuilder(argSequence);
                        sb.Remove(SequonMutaDN.Index, 1);
                        sb.Insert(SequonMutaDN.Index, "N");
                        _glycopep.Add(sb.ToString());
                    }
                }


                Match SequonMutaDNEnd = sequonMutaDNEnd.Match(argSequence); //Go to full sequence to check if the sequence contain S/T
                if (SequonMutaDNEnd.Length != 0)
                {
                    int idx = _sequence.IndexOf(argSequence) + argSequence.Length;
                    if (idx < _sequence.Length)
                    {
                        if ((_sequence[idx] == 'S' || _sequence[idx] == 'T'))
                        {
                            StringBuilder sb = new StringBuilder(argSequence);
                            sb.Remove(SequonMutaDNEnd.Index, 1);
                            sb.Insert(SequonMutaDNEnd.Index, "N");
                            _glycopep.Add(sb.ToString());
                        }
                    }
                }

            }
            else if (argMutation == enumPeptideMutation.AnyToN)
            {
                foreach (Match match in Regex.Matches(argSequence, "[ARNDCEQGHILKMFSTWYV][S|T]", RegexOptions.IgnoreCase))
                {
                    int IndexInOriginalSequence = _sequence.IndexOf(match.Value);
                    if (IndexInOriginalSequence == 0)
                    {
                        continue;
                    }
                    StringBuilder sb = new StringBuilder(argSequence);
                    sb.Remove(match.Index - 1, 1);
                    sb.Insert(match.Index - 1, "N");
                    if (!_glycopep.Contains(sb.ToString()))
                    {
                        _glycopep.Add(sb.ToString());
                    }
                }
                if (argSequence[argSequence.Length - 1] != 'P')
                {
                    int idx = _sequence.IndexOf(argSequence) + argSequence.Length;
                    if (idx < _sequence.Length)
                    {
                        if ((_sequence[idx] == 'S' || _sequence[idx] == 'T'))
                        {
                            StringBuilder sb = new StringBuilder(argSequence);
                            sb.Remove(argSequence.Length-2, 1);
                            sb.Insert(argSequence.Length-2, "N");
                            _glycopep.Add(sb.ToString());
                        }
                    }
                }
                
            }
            lstGlycopeptideSeq = _glycopep;
            if (lstGlycopeptideSeq.Count == 0)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Old function, no use anymore
        /// </summary>
        /// <param name="argAllowMissCleavage"></param>
        /// <param name="argProteaseType"></param>
        /// <returns></returns>
        public List<string> OGlycopeptide(int argAllowMissCleavage, List<Protease.Type> argProteaseType)
        {
            List<string> _cleavages = CreateCleavage(_sequence,argAllowMissCleavage, argProteaseType);

            List<string> _glycopep = new List<string>();
             foreach (string pep in _cleavages)
            {
                if (pep.ToUpper().Contains("S") || pep.ToUpper().Contains("T"))
                {
                    _glycopep.Add(pep);
                }
             }
            return _glycopep;
        }

        private List<string> CreateCleavage(string argSeq, int argAllowMissCleavage, List<Protease.Type> argProteaseType)
        {
            List<string> _cleavedPeptides = new List<string>();
            if (argAllowMissCleavage == -1 || argProteaseType.Contains(Protease.Type.None))
            {
                _cleavedPeptides.Add(argSeq);
                return _cleavedPeptides;
            }
            //foreach(Protease.Type enzy in argProteaseType)
            //{
            //    switch (enzy)
            //    {
            //        case Protease.Type.None:
            //            _cleavedPeptides.Add(_sequence);
            //            break;
            //        case Protease.Type.Trypsin:
            //            _cleavedPeptides.AddRange(TrypsinDigest());
            //            break;
            //        case Protease.Type.GlucED:
            //            _cleavedPeptides.AddRange(GluCDigest(true));
            //            break;
            //        case Protease.Type.GlucE:
            //            _cleavedPeptides.AddRange(GluCDigest(false));
            //            break;
            //    }
            //}
            string Seq = argSeq;
            int p = 0;
            for (int i = 0; i < _sequence.Length - 1; i++)
            {
                if (argProteaseType.Contains(Protease.Type.Trypsin) && Seq[i] == 'R')//&& Seq[i + 1] != 'P')
                {
                    _cleavedPeptides.Add(Seq.Substring(p, Seq.IndexOf("R", p) + 1 - p));
                    p = i + 1;
                }
                else if (argProteaseType.Contains(Protease.Type.Trypsin) && Seq[i] == 'K')//&& Seq[i + 1] != 'P')
                {
                    _cleavedPeptides.Add(Seq.Substring(p, Seq.IndexOf("K", p) + 1 - p));
                    p = i + 1;
                }
                else if ((argProteaseType.Contains(Protease.Type.GlucE) || argProteaseType.Contains(Protease.Type.GlucED)) && Seq[i] == 'E')
                {
                    _cleavedPeptides.Add(Seq.Substring(p, Seq.IndexOf("E", p) + 1 - p));
                    p = i + 1;
                }
                else if (argProteaseType.Contains(Protease.Type.GlucED) && Seq[i] == 'D')
                {
                    _cleavedPeptides.Add(Seq.Substring(p, Seq.IndexOf("D", p) + 1 - p));
                    p = i + 1;
                }
            }
            _cleavedPeptides.Add(Seq.Substring(p));

            string Combind = "";
            List<string> MissCleavage = new List<string>();
            for (int i = 1; i <= argAllowMissCleavage; i++)
            {
                for (int j = 0; j < _cleavedPeptides.Count; j++)
                {
                    Combind = _cleavedPeptides[j];
                    for (int k = 1; k <= i; k++)
                    {
                        if (j + i < _cleavedPeptides.Count)
                        {
                            Combind = Combind + _cleavedPeptides[j + k];
                        }
                    }
                    if (Combind != _cleavedPeptides[j])
                    {
                        MissCleavage.Add(Combind);
                    }
                }
            }
            _cleavedPeptides.AddRange(MissCleavage);
            return _cleavedPeptides;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="argAllowMissCleavage">-1: return whole peptide,</param>
        /// <param name="argProteaseType"></param>
        /// <returns></returns>
        public List<string> CreateCleavage(int argAllowMissCleavage, List<Protease.Type> argProteaseType)
        {
            return CreateCleavage(_sequence, argAllowMissCleavage, argProteaseType);
        }
        public List<string> CreateCleavage(int argAllowMissCleavage, Protease.Type argProteaseType)
        {

            return CreateCleavage(_sequence,argAllowMissCleavage, new List<Protease.Type>() { argProteaseType });
        }
        /// <summary>
        /// http://web.mit.edu/toxms/www/filters/python/Biotin_peptide_ID_program/trypsin.py
        /// </summary>
        private List<string> TrypsinDigest()
        {
            string Seq = _sequence;
            List<string> _cleavages = new List<string>();
            int p = 0;
            for (int i = 0; i < _sequence.Length - 1; i++)
            {
                if (Seq[i] == 'R' && Seq[i + 1] != 'P')
                {
                    _cleavages.Add(Seq.Substring(p, Seq.IndexOf("R", p) + 1 - p));
                    p = i + 1;
                }
                if (Seq[i] == 'K' && Seq[i + 1] != 'P')
                {
                    _cleavages.Add(Seq.Substring(p, Seq.IndexOf("K", p) + 1 - p));
                    p = i + 1;
                }
            }
            _cleavages.Add(Seq.Substring(p));
            return _cleavages;
        }
        /// <summary>
        /// non-phosphate buffers, Glu-C cleaves on the C-terminal side of glutamic acid (E)
        /// in phosphate buffer  Glu-C cleaves on the C-terminal side of both glutamic acid(E) and aspartic acid (D)
        /// </summary>
        private List<string> GluCDigest(bool argIsPhosphate)
        {
            string Seq = _sequence;
            List<string> _cleavages = new List<string>();
            int p = 0;
            for (int i = 0; i < _sequence.Length - 1; i++)
            {
                if (Seq[i] == 'E' )
                {
                    _cleavages.Add(Seq.Substring(p, Seq.IndexOf("E", p) + 1 - p));
                    p = i + 1;
                }
                if (argIsPhosphate)
                {
                    if (Seq[i] == 'D')
                    {
                        _cleavages.Add(Seq.Substring(p, Seq.IndexOf("D", p) + 1 - p));
                        p = i + 1;
                    }
                }
            }
            _cleavages.Add(Seq.Substring(p));
            return _cleavages;
        }
    }
}
