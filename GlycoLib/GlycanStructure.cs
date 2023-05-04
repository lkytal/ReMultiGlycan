using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using COL.GlycoLib;
using COL.MassLib;
using COL.ProtLib;

namespace COL.GlycoLib
{
    /// <summary>
    /// This the the top class for glycan sequencing
    /// </summary>
    [Serializable]    
    public class GlycanStructure : ICloneable, IComparable<GlycanStructure>
    {
        private GlycanTreeNode _tree;
        private MSPoint _y1;
        private List<GlycanTreeNode> _fragment;
        private int _nextID =1;
        private string _IUPAC = "";
        private double _Score = 0.0;
        private float _IncompleteScore = 0;
        private bool _MatchWithPrecursorMW = false;
        /// <summary>
        /// Is the structure complete by substract precursor m/z? (add one extra glycan only)
        /// </summary>
        private bool _IsCompleteByPrecursorDifference = false;
        private List<Tuple<MSPoint, string>> _CoreIDPeaks = new List<Tuple<MSPoint, string>>();
        private List<Tuple<MSPoint, string>> _BranchIDPeaks = new List<Tuple<MSPoint, string>>();
        private string _restGlycanString = "";
        private string _peptideStr = "";
        private float _PrecursorMonoMass = 0;
        private TargetPeptide _targetPeptide;
        private List<double> _SVMMatrices = new List<double>();
        private int _SVMPredictedLabel;
        private List<double> _SVMPredictedProb = new List<double>();
        //Constrator
        public GlycanStructure(Glycan argGlycan)
        {
            new GlycanStructure(argGlycan, 0.0f);           
        }
        public GlycanStructure(Glycan argGlycan, float argY1)
        {
            _tree = new GlycanTreeNode(argGlycan.GlycanType, _nextID);
            _nextID++;
            _y1 = new MSPoint(argY1, 0.0f);
            _IUPAC = _tree.GetIUPACString();
        }
        public GlycanStructure(Glycan argGlycan, MSPoint argPeak)
        {
            _tree = new GlycanTreeNode(argGlycan.GlycanType, _nextID);
            _nextID++;
            _y1 = argPeak;
            _IUPAC = _tree.GetIUPACString();
        }
        public GlycanStructure(GlycanTreeNode argGlycan)
        {
            _tree = argGlycan;
            _nextID = argGlycan.NoOfTotalGlycan + 1;
            _IUPAC = _tree.GetIUPACString();
        }
        //Properties
        public bool MatchWithPrecursorMW
        {
            get { return _MatchWithPrecursorMW; }
            set { _MatchWithPrecursorMW = value; }
        }
        public bool IsCompleteByPrecursorDifference
        {
            get { return _IsCompleteByPrecursorDifference; }
            set { _IsCompleteByPrecursorDifference = value; }
        }
        /// <summary>
        /// Label 1: Completed glycan and peptide (within 10PPM) 2: Completed glycan only (Y1+glycan matched with precursor) No Peptide 3: Partial glycan no peptide
        /// </summary>
        public int SVMPredictedLabel
        {
            get {return _SVMPredictedLabel;}
            set { _SVMPredictedLabel = value; }
        }
        public List<double> SVMPrrdictedProbabilities
        {
            get {return _SVMPredictedProb;}
            set { _SVMPredictedProb = value; }
        }

        public List<double> SVMMatrices
        {
            get
            {
                if (_SVMMatrices.Count == 0)
                {
                    _SVMMatrices = CreateSVMMatrices(); 
                }
                return _SVMMatrices;
            }
        }
        public double PPM
        {
            get
            {
                float glycopeptide = 0;
                if (_targetPeptide!=null && _targetPeptide.PeptideMass != 0)
                {
                    glycopeptide = _targetPeptide.PeptideMass;
                }
                else if (_peptideStr != "")
                {
                    ProtLib.AminoAcidMass AAMS = new AminoAcidMass();
                    glycopeptide = AAMS.GetMonoMW(_peptideStr,true);
                }
                else
                {
                    glycopeptide = Y1.Mass*Charge - Atoms.ProtonMass*Charge -
                                  GlycanMass.GetGlycanMass(Glycan.Type.HexNAc);
                }
                if (_restGlycanString != "")
                {
                    string[] tmpAry = _restGlycanString.Split('-');
                    glycopeptide = glycopeptide + (Convert.ToInt32(tmpAry[0]) + Root.NoOfHexNac) * GlycanMass.GetGlycanMass(Glycan.Type.HexNAc);
                    glycopeptide = glycopeptide + (Convert.ToInt32(tmpAry[1]) + Root.NoOfHex) * GlycanMass.GetGlycanMass(Glycan.Type.Hex);
                    glycopeptide = glycopeptide + (Convert.ToInt32(tmpAry[2]) + Root.NoOfDeHex) * GlycanMass.GetGlycanMass(Glycan.Type.DeHex);
                    glycopeptide = glycopeptide + (Convert.ToInt32(tmpAry[3]) + Root.NoOfNeuAc) * GlycanMass.GetGlycanMass(Glycan.Type.NeuAc);
                    glycopeptide = glycopeptide + (Convert.ToInt32(tmpAry[4]) + Root.NoOfNeuGc) * GlycanMass.GetGlycanMass(Glycan.Type.NeuGc);
                }
                else
                {
                    glycopeptide = glycopeptide + (Root.NoOfHexNac) * GlycanMass.GetGlycanMass(Glycan.Type.HexNAc);
                    glycopeptide = glycopeptide + (Root.NoOfHex) * GlycanMass.GetGlycanMass(Glycan.Type.Hex);
                    glycopeptide = glycopeptide + (Root.NoOfDeHex) * GlycanMass.GetGlycanMass(Glycan.Type.DeHex);
                    glycopeptide = glycopeptide + (Root.NoOfNeuAc) * GlycanMass.GetGlycanMass(Glycan.Type.NeuAc);
                    glycopeptide = glycopeptide + (Root.NoOfNeuGc) * GlycanMass.GetGlycanMass(Glycan.Type.NeuGc);
                }
                return MassUtility.GetMassPPM(_PrecursorMonoMass, glycopeptide);
            }
        }

        public TargetPeptide TargetPeptide
        {
            get { return _targetPeptide; }
            set { _targetPeptide = value; }
        }

        public string PeptideModificationString
        {
            get
            {
                string tmpMod = "";
                if( _targetPeptide !=null)
                {
                    foreach (string key in _targetPeptide.Modifications.Keys)
                    {
                        tmpMod = tmpMod + key + "*" + _targetPeptide.Modifications[key].ToString() + ";";
                    }
                }
                return tmpMod;
            }
        }
        public float PrecursorMonoMass
        {
            get { return _PrecursorMonoMass; }
            set { _PrecursorMonoMass = value; }
        }
        public string PeptideSequence
        {
            get
            {
                if (_targetPeptide == null)
                {
                    return "";
                }
                return _targetPeptide.PeptideSequence;
            }
        }
        public string RestGlycanString
        {
            get { return _restGlycanString; }
            set { _restGlycanString = value; }
        }

        public string FullSequencedGlycanString
        {
            get
            {
                string _fullSeqGlycanString = "";
                if (_restGlycanString == "")
                {
                    _fullSeqGlycanString = _tree.NoOfHexNac + "-" + _tree.NoOfHex + "-" + _tree.NoOfDeHex + "-" +
                                           _tree.NoOfNeuAc + "-" + _tree.NoOfNeuGc;
                }
                else
                {
                    string[] appendGlycan = _restGlycanString.Split('-');
                    _fullSeqGlycanString = (_tree.NoOfHexNac + Convert.ToInt32(appendGlycan[0])).ToString() + "-";
                    _fullSeqGlycanString += (_tree.NoOfHex + Convert.ToInt32(appendGlycan[1])).ToString() + "-";
                    _fullSeqGlycanString += (_tree.NoOfDeHex + Convert.ToInt32(appendGlycan[2])).ToString() + "-";
                    _fullSeqGlycanString += (_tree.NoOfNeuAc + Convert.ToInt32(appendGlycan[3])).ToString() + "-";
                    _fullSeqGlycanString += (_tree.NoOfNeuGc + Convert.ToInt32(appendGlycan[4])).ToString();
                }
                return _fullSeqGlycanString;
            }
        }
        public GlycanTreeNode Root
        {
            get { return _tree; }
        }
        public MSPoint Y1
        {
            get { return _y1; }
            set { _y1 = value; }
        }

        public int Charge
        {
            set { _tree.Charge = value; }
            get { return _tree.Charge; }
        }
        public int NextID
        {
            get { return _nextID; }
            set { _nextID = value; }
        }        
        public List<GlycanTreeNode> TheoreticalFragment
        {
            get
            {
                if (_fragment == null)
                {
                    _fragment = FragementGlycanTree(_tree);
                }
                return _fragment;
            }
        }
        public List<GlycanTreeNode> GetNodeinLevel(int argDistance)
        {
            List<GlycanTreeNode> Level = new List<GlycanTreeNode>();
            foreach (GlycanTreeNode T in _tree.FetchAllGlycanNode())
            {
                if (T.DistanceRoot == argDistance)
                {
                    Level.Add(T);
                }
            }
            return Level;
        }
        public GlycanTreeNode GetGlycanTreeByID(int argID)
        {
            foreach (GlycanTreeNode t in _tree.TravelGlycanTreeBFS())
            {
                if (t.NodeID == argID)
                {
                    return t;
                }
            }
            return null;
        }
        public GlycanTreeNode GetGlycanTreeByID(string argNodeID)
        {
            string[] strIDs = argNodeID.Split('-');
            List<int> IDs = new List<int>();
            for (int i = 0; i < strIDs.Length; i++)
            {
                IDs.Add(Convert.ToInt32(strIDs[i]));
            }
            List<GlycanTreeNode> CloneTree = ((GlycanTreeNode)_tree.Clone()).GetListsofGlycanTree();
            for (int i = CloneTree.Count-1; i>= 0; i--)
            {
                if (!IDs.Contains(CloneTree[i].NodeID))
                {
                    CloneTree.RemoveAt(i);
                }
            }
            return CloneTree[0];
        }
        public void AddGlycanToStructure(GlycanTreeNode argAddTree, int argParentID)
        {
            GlycanTreeNode Parent = GetGlycanTreeByID(argParentID);
            if (Parent != null)
            {
                foreach (GlycanTreeNode GT in argAddTree.FetchAllGlycanNode())
                {
                    GT.NodeID = _nextID;
                    _nextID++;
                }
                argAddTree.Parent = Parent;
                Parent.AddGlycanSubTree(argAddTree);
               // Parent.UpdateGlycans();
                Parent.SortSubTree();
                _IUPAC = _tree.GetIUPACString();
            }
        }
        public string IUPACString
        {
            get {
                if (_IUPAC == "")
                {                    
                    _IUPAC = _tree.GetIUPACString(); 
                }
                return _IUPAC;
            }
        }
     
        public float GlycanMZ
        {
            get
            {
                return (GlycanMonoMass + MassLib.Atoms.ProtonMass * Charge) / Charge;
            }
        }
        public float GlycanMonoMass
        {
            get
            {
                float glycanMass = 0.0f;
                int[] appendGlycan = new int[5]{0,0,0,0,0};
                if (_restGlycanString != "")
                {
                    string[] tmpGlycan = _restGlycanString.Split('-');
                    for (int i = 0; i <= 4; i++)
                    {
                        appendGlycan[i] = Convert.ToInt32(tmpGlycan[i]);
                    }
                }

                glycanMass = glycanMass + (_tree.NoOfHexNac +appendGlycan[0])* GlycanMass.GetGlycanMass(Glycan.Type.HexNAc);
                glycanMass = glycanMass + (_tree.NoOfHex+appendGlycan[1] )* GlycanMass.GetGlycanMass(Glycan.Type.Hex);
                glycanMass = glycanMass + (_tree.NoOfDeHex+appendGlycan[2] )* GlycanMass.GetGlycanMass(Glycan.Type.DeHex);
                glycanMass = glycanMass + (_tree.NoOfNeuAc+ appendGlycan[3])* GlycanMass.GetGlycanMass(Glycan.Type.NeuAc);
                glycanMass = glycanMass + (_tree.NoOfNeuGc + appendGlycan[4]) * GlycanMass.GetGlycanMass(Glycan.Type.NeuGc);
                return glycanMass;
            }
        }
        public float GlycanAVGMZ
        {
            get
            {
                return (GlycanAVGMonoMass + MassLib.Atoms.ProtonMass * Charge) / Charge;
            }
        }
        public float GlycanAVGMonoMass
        {
            get
            {
                float glycanMass = 0.0f;
                int[] appendGlycan = new int[5] { 0, 0, 0, 0, 0 };
                if (_restGlycanString != "")
                {
                    string[] tmpGlycan = _restGlycanString.Split('-');
                    for (int i = 0; i <= 4; i++)
                    {
                        appendGlycan[i] = Convert.ToInt32(tmpGlycan[i]);
                    }
                }
                glycanMass = glycanMass + (_tree.NoOfHexNac + appendGlycan[0]) * GlycanMass.GetGlycanAVGMass(Glycan.Type.HexNAc);
                glycanMass = glycanMass + (_tree.NoOfHex + appendGlycan[1])* GlycanMass.GetGlycanAVGMass(Glycan.Type.Hex);
                glycanMass = glycanMass + (_tree.NoOfDeHex + appendGlycan[2])* GlycanMass.GetGlycanAVGMass(Glycan.Type.DeHex);
                glycanMass = glycanMass + (_tree.NoOfNeuAc + appendGlycan[3])* GlycanMass.GetGlycanAVGMass(Glycan.Type.NeuAc);
                glycanMass = glycanMass + (_tree.NoOfNeuGc + appendGlycan[4]) * GlycanMass.GetGlycanAVGMass(Glycan.Type.NeuGc);
                return glycanMass;
            }
        }
        public int NoOfGlycan(Glycan argGlycan)
        {
            switch (argGlycan.GlycanType)
            {
                case Glycan.Type.HexNAc:
                    return NoOfHex;
                case Glycan.Type.NeuAc:
                    return NoOfNeuAc;
                case Glycan.Type.NeuGc:
                    return NoOfNeuGc;
                case Glycan.Type.DeHex:
                    return NoOfDeHex;
                default:
                    return NoOfHex;
            }
        }
        private List<double> CreateSVMMatrices()
        {
            List<double> matrixes = new List<double>();
            int Peptide = 0;
            if (PeptideSequence != null && PeptideSequence != "")
            {
                Peptide = 1;
            }
            
            int fuc = 1;
            int CoreYPeaks = CoreIDPeak.Count;
            float CoreYScore = CoreScore;
            if (!IUPACString.EndsWith("(DeHex-)HexNAc"))
            {
                CoreYPeaks = CoreIDPeak.Count - CoreIDPeak.Where(x => x.Item2.Contains("deHex")).ToList().Count;
                foreach (var core in CoreIDPeak.Where(x => x.Item2.EndsWith("(DeHex-)HexNAc")))
                {
                    CoreYScore -= core.Item1.Intensity;
                }
                fuc = 0;
            }
            matrixes.Add(Peptide);
            matrixes.Add(fuc);
            matrixes.Add(NoOfTotalGlycan);
            matrixes.Add(Convert.ToDouble(PPM.ToString("0.000")));
            //matrixes.Add(CoreYPeaks);
            //matrixes.Add(Convert.ToDouble(CoreYScore.ToString("0.000")));
            matrixes.Add(BranchIDPeak.Count);
            matrixes.Add(Convert.ToDouble(BranchScore.ToString("0.000")));

            var CorePeaks = CoreIDPeak.Where(x => x.Item2 == "HexNAc").ToList();
            matrixes.Add(Convert.ToDouble(CorePeaks.Count != 0 ? CorePeaks[0].Item1.Intensity.ToString("0.0000") : "0"));
            CorePeaks = CoreIDPeak.Where(x => x.Item2 == "HexNAc-HexNAc").ToList();
            matrixes.Add(Convert.ToDouble(CorePeaks.Count != 0 ? CorePeaks[0].Item1.Intensity.ToString("0.0000") : "0"));
            CorePeaks = CoreIDPeak.Where(x => x.Item2 == "HexNAc-HexNAc-Hex").ToList();
            matrixes.Add(Convert.ToDouble(CorePeaks.Count != 0 ? CorePeaks[0].Item1.Intensity.ToString("0.0000") : "0"));
            CorePeaks = CoreIDPeak.Where(x => x.Item2 == "HexNAc-HexNAc-Hex-Hex").ToList();
            matrixes.Add(Convert.ToDouble(CorePeaks.Count != 0 ? CorePeaks[0].Item1.Intensity.ToString("0.0000") : "0"));
            CorePeaks = CoreIDPeak.Where(x => x.Item2 == "HexNAc-HexNAc-Hex-(Hex-)Hex").ToList();
            matrixes.Add(Convert.ToDouble(CorePeaks.Count != 0 ? CorePeaks[0].Item1.Intensity.ToString("0.0000") : "0"));

            //if (MatchWithPrecursorMW)
            //{
            //    matrixes.Add(1);
            //}
            //else
            //{
            //    matrixes.Add(0);
            //}
            return matrixes;
        }
        public int NoOfTotalGlycan
        {
            get
            {
                return _tree.NoOfTotalGlycan;
            }
        }        
        public int NoOfHexNac
        {
            get
            {
                return _tree.NoOfHexNac;
            }
        }
        public int NoOfHex
        {
            get
            {
                return _tree.NoOfHex;
            }
        }
        public int NoOfDeHex
        {
            get
            {
                return _tree.NoOfDeHex;
            }
        }
        public int NoOfNeuAc
        {
            get
            {
                return _tree.NoOfNeuAc;
            }
        }
        public int NoOfNeuGc
        {
            get
            {
                return _tree.NoOfNeuGc;
            }
        }
        public double Score
        {
            get
            {
                return CoreScore + BranchScore + InCompleteScore;
            }
        }
        public float CoreScore
        {
            get
            {
                float score = 0;
                foreach (Tuple<MSPoint, string> peak in _CoreIDPeaks)
                {
                    score += peak.Item1.Intensity;
                }
                return score;
            }
        }
        public float BranchScore
        {
            get
            {
                float score = 0;
                foreach (Tuple<MSPoint, string> peak in _BranchIDPeaks)
                {
                    score = score + peak.Item1.Intensity;
                }
                return score;
            }
        }

        public float InCompleteScore
        {
            get { return _IncompleteScore; }
            set { _IncompleteScore = value; }
        }
        public List<Tuple<MSPoint, string>> CoreIDPeak
        {
            get { return _CoreIDPeaks; }
            set { _CoreIDPeaks = value; }
        }
        public List<Tuple<MSPoint, string>> BranchIDPeak
        {
            get { return _BranchIDPeaks; }
            set { _BranchIDPeaks = value; }
        }
        //Functions
        public int CompareTo(GlycanStructure obj)
        {
            return -1 * this.Score.CompareTo(obj.Score);//Descending
        }
        public override bool Equals(System.Object obj)
        {
            if (obj == null)
            {
                return false;
            }
            GlycanStructure GT = obj as GlycanStructure;
            if ((System.Object)GT == null)
            {
                return false;
            }
            if (this.IUPACString == GT.IUPACString && this.Charge == GT.Charge)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return this.Root.GetHashCode();
        }
        public object Clone()
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, this);
            ms.Position = 0;
            object obj = bf.Deserialize(ms);
            ms.Close();
            return obj;
        }
        public string GetSequqncedIUPACwNodeID(int argNodeID)
        {
            string IUPAC = this.Root.GetIUPACStringWithNodeID();
            string[] a = IUPAC.Split('-');
            if (argNodeID == a.Length)
            {
                return this.Root.GetIUPACString();
            }
            for (int i = 0; i < a.Length; i++)
            {
                int ID = GetNodeNum(a[i]);
                if (ID > argNodeID)
                {
                    if (a[i].Contains(")("))
                    {
                        a[i] = ")(";
                    }
                    else if (a[i].Contains("("))
                    {
                        a[i] = "(";
                    }
                    else if (a[i].Contains(")"))
                    {
                        a[i]= ")";
                    }
                    else
                    {
                        a[i] = "";
                    }
                }
            }
            string IUPACtrim = "";
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i].Length != 0)
                {
                    if (a[i].Contains(",") && !a[i].Contains(")"))
                    {
                        if (a[i].Contains("("))
                        {
                            IUPACtrim = IUPACtrim + "("+a[i].Split(',')[1] + "-";
                        }
                        else
                        {
                            IUPACtrim = IUPACtrim + a[i].Split(',')[1] + "-";
                        }
                    }
                    else
                    {
                        if(a[i].StartsWith(")("))
                        {
                            if (a[i].Contains(","))
                            {
                                IUPACtrim = IUPACtrim + ")(" + a[i].Split(',')[1] + "-";
                            }
                            else
                            {
                                IUPACtrim = IUPACtrim + a[i] + "-";
                            }
                        }
                        else if (a[i].StartsWith(")"))
                        {
                            if (a[i].Contains(","))
                            {
                                IUPACtrim = IUPACtrim + ")" + a[i].Split(',')[1] + "-";
                            }
                            else
                            {
                                IUPACtrim = IUPACtrim  + a[i]+ "-";
                            }
                        }
                        else if (a[i].StartsWith("("))
                        {
                            if (i + 1 < a.Length)
                            {
                                if(a[i+1].Contains(")("))
                                {
                                    a[i + 1] = "(";
                                    continue;
                                }
                                if(a[i+1].StartsWith(")"))
                                {
                                    if (!a[i + 1].Contains(","))
                                    {
                                        i += 1;
                                    }
                                    else
                                    {
                                        a[i + 1] = a[i + 1].Substring(1);
                                    }
                                    continue;
                                }
                            }
                            IUPACtrim = IUPACtrim + a[i] ;
                        }
                        else
                        {
                            IUPACtrim = IUPACtrim + a[i] + "-";
                        }                       
                    }
                }
            }
            IUPACtrim = IUPACtrim.Replace("()-", "");
            IUPACtrim = IUPACtrim.Replace("()", "");
            IUPACtrim = IUPACtrim.Replace("(-)", "");
            IUPACtrim = IUPACtrim.Replace(")(-", "");
            //if (IUPACtrim.Split('(').Length != IUPACtrim.Split(')').Length)
            //{
            //    IUPACtrim = IUPACtrim.Replace(")", "");
            //    IUPACtrim = IUPACtrim.Replace("(", "");
            //}
            if (IUPACtrim.StartsWith("(")) 
            {
                IUPACtrim = IUPACtrim.Remove(IUPACtrim.IndexOf(')'), 1).Substring(1);
            }
            if (IUPACtrim.StartsWith(")"))
            {
                IUPACtrim = IUPACtrim.Substring(1);
            }
            if (IUPACtrim.StartsWith("-"))
            {
                IUPACtrim = IUPACtrim.Substring(1);
            }
            if (IUPACtrim.Contains("(("))
            {
                int Startidx = IUPACtrim.IndexOf("((");
                int Endidx = IUPACtrim.IndexOf(")", Startidx);
                IUPACtrim= IUPACtrim.Remove(Endidx,1).Remove(Startidx,1);
            }

            return IUPACtrim.Substring(0, IUPACtrim.Length - 1);
        }
        private int GetNodeNum(string argNode)
        {
            string node = argNode.Split(',')[0];
            node = node.Replace("(", "").Replace(")", "");
            return Convert.ToInt32(node);
        }
        public string GetIUPACfromParentToNodeID(int argNodeID)
        {
            string strTree = "";
            string strSub1 = "";
            string strSub2 = "";
            string strSub3 = "";
            string strSub4 = "";
            if (_tree.SubTree1 != null && _tree.SubTree1.NodeID < argNodeID)
            {
                strSub1 = _tree.SubTree1.GetIUPACString() + "-";
            }
            if (_tree.SubTree2 != null && _tree.SubTree2.NodeID < argNodeID)
            {
                strSub2 = "(" + _tree.SubTree2.GetIUPACString() + "-)";
            }

            if (_tree.SubTree3 != null && _tree.SubTree3.NodeID < argNodeID)
            {
                strSub3 = "(" + _tree.SubTree3.GetIUPACString() + "-)";
            }

            if (_tree.SubTree4 != null && _tree.SubTree4.NodeID < argNodeID)
            {
                strSub4 = "(" + _tree.SubTree4.GetIUPACString() + "-)";
            }
            strTree = strSub1 + strSub2 + strSub3 + strSub4 + _tree.Node.ToString();
            return strTree;
        }
        public bool HasNGlycanCore()
        {
            if (this.NoOfHexNac >= 2 && this.NoOfHex >= 3)
            {
                if (_tree.GlycanType == Glycan.Type.HexNAc && _tree.NoOfHexNacInChild == 1 &&
                    _tree.SubTree1.GlycanType == Glycan.Type.HexNAc &&
                    _tree.SubTree1.SubTree1.GlycanType == Glycan.Type.Hex &&
                    _tree.SubTree1.SubTree1.NoOfHexInChild == 2
                    )
                {
                    return true;
                }
                else
                {
                    return false;
                }
                //Old Segament
                //if (_tree.GlycanType == Glycan.Type.HexNAc &&
                //    _tree.SubTree1.GlycanType == Glycan.Type.HexNAc && _tree.Subtrees.Count==1  &&  _tree.SubTree1.SubTree1.GlycanType == Glycan.Type.Hex )
                //{
                //    if(_tree.Subtrees.Count>1)
                //    {
                //        foreach(GlycanTreeNode T in _tree.Subtrees)
                //        {
                //            if(!(T.GlycanType == Glycan.Type.DeHex || T.GlycanType == Glycan.Type.HexNAc))
                //            {
                //                return false;
                //            }
                //        }
                //    }
                //    if (!((_tree.SubTree1.SubTree1.Subtrees.Count == 2 &&
                //        _tree.SubTree1.SubTree1.SubTree1 != null && _tree.SubTree1.SubTree1.SubTree1.GlycanType == Glycan.Type.Hex &&
                //        _tree.SubTree1.SubTree1.SubTree2 != null && _tree.SubTree1.SubTree1.SubTree2.GlycanType == Glycan.Type.Hex)
                //        ||
                //        (_tree.SubTree1.SubTree1.Subtrees.Count == 3 && 
                //        _tree.SubTree1.SubTree1.NoOfHexInChild ==2 && _tree.SubTree1.SubTree1.NoOfHexNac==1))
                        
                //        )
                //    {
                //        return false;
                //    }



                //    if( _tree.SubTree1.SubTree1.Subtrees.Count>2)
                //    {
                //        int NoOfHexNAc = 0;
                //        int NoOfHex = 0;
                //        foreach(GlycanTreeNode T in this.GetNodeinLevel(3))
                //        {
                //            if (T.GlycanType == Glycan.Type.HexNAc)
                //            {
                //                NoOfHexNAc++;
                //            }
                //            if (T.GlycanType == Glycan.Type.Hex)
                //            {
                //                NoOfHex++;
                //            }
                //        }
                //        if (NoOfHex < 2)
                //        {
                //            return false;
                //        }
                //    }

                //    return true;
                //}            
            }
            return false;
        }
        
        public static List<GlycanTreeNode> FragementGlycanTree(GlycanTreeNode argTree)
        {
            List<GlycanTreeNode> _fragment = new List<GlycanTreeNode>();
            Queue ChildQueue = new Queue();
            GlycanTreeNode CurrentTree = argTree;
            do
            {
                GlycanTreeNode tmpTree = (GlycanTreeNode)argTree.Clone();
                if (CurrentTree.GetChildren() != null)
                {
                    if (CurrentTree.GetChildren().Count == 1)
                    {
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[0]);
                        _fragment.Add(tmpTree);
                        CurrentTree = CurrentTree.GetChildren()[0];
                    }
                    else if (CurrentTree.GetChildren().Count == 2)
                    {
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[0]);
                        _fragment.Add(tmpTree);

                        tmpTree = (GlycanTreeNode)argTree.Clone();
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[1]);
                        _fragment.Add(tmpTree);

                        tmpTree = (GlycanTreeNode)argTree.Clone();
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[0]);
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[1]);
                        _fragment.Add(tmpTree);

                        ChildQueue.Enqueue(CurrentTree.GetChildren()[1]);
                        CurrentTree = CurrentTree.GetChildren()[0];

                    }
                    else if (CurrentTree.GetChildren().Count == 3)
                    {
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[0]);
                        _fragment.Add(tmpTree);

                        tmpTree = (GlycanTreeNode)argTree.Clone();
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[1]);
                        _fragment.Add(tmpTree);

                        tmpTree = (GlycanTreeNode)argTree.Clone();
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[2]);
                        _fragment.Add(tmpTree);

                        tmpTree = (GlycanTreeNode)argTree.Clone();
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[0]);
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[1]);
                        _fragment.Add(tmpTree);

                        tmpTree = (GlycanTreeNode)argTree.Clone();
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[0]);
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[2]);
                        _fragment.Add(tmpTree);

                        tmpTree = (GlycanTreeNode)argTree.Clone();
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[1]);
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[2]);
                        _fragment.Add(tmpTree);

                        tmpTree = (GlycanTreeNode)argTree.Clone();
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[0]);
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[1]);
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[2]);
                        _fragment.Add(tmpTree);

                        ChildQueue.Enqueue(CurrentTree.GetChildren()[1]);
                        ChildQueue.Enqueue(CurrentTree.GetChildren()[2]);
                        CurrentTree = CurrentTree.GetChildren()[0];
                    }
                    else if (CurrentTree.GetChildren().Count == 4)
                    {
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[0]);
                        _fragment.Add(tmpTree);

                        tmpTree = (GlycanTreeNode)argTree.Clone();
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[1]);
                        _fragment.Add(tmpTree);

                        tmpTree = (GlycanTreeNode)argTree.Clone();
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[2]);
                        _fragment.Add(tmpTree);

                        tmpTree = (GlycanTreeNode)argTree.Clone();
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[3]);
                        _fragment.Add(tmpTree);

                        tmpTree = (GlycanTreeNode)argTree.Clone();
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[0]);
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[1]);
                        _fragment.Add(tmpTree);

                        tmpTree = (GlycanTreeNode)argTree.Clone();
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[0]);
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[2]);
                        _fragment.Add(tmpTree);

                        tmpTree = (GlycanTreeNode)argTree.Clone();
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[0]);
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[3]);
                        _fragment.Add(tmpTree);

                        tmpTree = (GlycanTreeNode)argTree.Clone();
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[1]);
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[2]);
                        _fragment.Add(tmpTree);

                        tmpTree = (GlycanTreeNode)argTree.Clone();
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[1]);
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[3]);
                        _fragment.Add(tmpTree);

                        tmpTree = (GlycanTreeNode)argTree.Clone();
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[2]);
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[3]);
                        _fragment.Add(tmpTree);


                        tmpTree = (GlycanTreeNode)argTree.Clone();
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[0]);
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[1]);
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[2]);
                        _fragment.Add(tmpTree);

                        tmpTree = (GlycanTreeNode)argTree.Clone();
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[0]);
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[1]);
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[3]);
                        _fragment.Add(tmpTree);

                        tmpTree = (GlycanTreeNode)argTree.Clone();
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[0]);
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[2]);
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[3]);
                        _fragment.Add(tmpTree);

                        tmpTree = (GlycanTreeNode)argTree.Clone();
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[1]);
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[2]);
                        tmpTree.RemoveGlycan(CurrentTree.GetChildren()[3]);
                        _fragment.Add(tmpTree);


                        ChildQueue.Enqueue(CurrentTree.GetChildren()[1]);
                        ChildQueue.Enqueue(CurrentTree.GetChildren()[2]);
                        ChildQueue.Enqueue(CurrentTree.GetChildren()[3]);
                        CurrentTree = CurrentTree.GetChildren()[0];
                    }

                }
                else
                {
                    if (ChildQueue.Count != 0)
                    {
                        CurrentTree = (GlycanTreeNode)ChildQueue.Dequeue();
                    }
                    else
                    {
                        break;
                    }
                }
            } while (true);

            //Filter out duplicate tree
            List<GlycanTreeNode> tmpGlycanTree = new List<GlycanTreeNode>();
            foreach (GlycanTreeNode t in _fragment)
            {
                if (!tmpGlycanTree.Contains(t))
                {
                    tmpGlycanTree.Add(t);
                }
            }
            tmpGlycanTree.Sort(delegate(GlycanTreeNode T1, GlycanTreeNode T2)
            {
                return GlycanMass.GetGlycanMasswithCharge(new GlycanCompound(T1.NoOfHexNac,T1.NoOfHex,T1.NoOfDeHex,T1.NoOfNeuAc), argTree.Charge).CompareTo(
                    GlycanMass.GetGlycanMasswithCharge(new GlycanCompound(T2.NoOfHexNac,T2.NoOfHex,T2.NoOfDeHex,T2.NoOfNeuAc), argTree.Charge));
            });

            return tmpGlycanTree;
        }
        public void RemoveSubtree(GlycanTreeNode argTree, GlycanTreeNode argRemoveTree)
        {
            int TargetLevel = argRemoveTree.DistanceRoot;
            GlycanTreeNode CurrentTree = argTree;
            Queue ChildQuene = new Queue();
            do
            {
                if (CurrentTree.DistanceRoot < TargetLevel)
                {
                    if (CurrentTree.GetChildren() == null) //Other Branch
                    {
                        CurrentTree = (GlycanTreeNode)ChildQuene.Dequeue();
                    }
                    else
                    {
                        if (CurrentTree.GetChildren().Count > 1)
                        {
                            for (int i = 1; i < CurrentTree.GetChildren().Count; i++)
                            {
                                ChildQuene.Enqueue(CurrentTree.GetChildren()[i]);
                            }
                        }
                        CurrentTree = CurrentTree.GetChildren()[0];
                    }
                }
                else
                {
                    if (CurrentTree.GetIUPACString() == argRemoveTree.GetIUPACString())
                    {
                        CurrentTree = CurrentTree.Parent;
                        CurrentTree.GetChildren().Remove(argRemoveTree);
                    }
                }
            } while (true);
        }
        public List<float> GetFragmentMassList()
        {
            if (_fragment == null)
            {
                _fragment =  FragementGlycanTree(_tree);
            }
            List<float> _fragmentMz = new List<float>();

            foreach (GlycanTreeNode t in _fragment)
            {
                GlycanCompound comp = new GlycanCompound(t.NoOfHexNac, t.NoOfHex, t.NoOfDeHex, t.NoOfNeuAc);
                if (!_fragmentMz.Contains(GlycanMass.GetGlycanMasswithCharge(comp, _tree.Charge)))
                {
                    _fragmentMz.Add(GlycanMass.GetGlycanMasswithCharge(t.GlycanType, _tree.Charge));
                }
            }
            return _fragmentMz;
        }
        /// <summary>
        /// Check if this tree obey N-Linked Core rules: 2 HexNex + 3 Hex, Branching is high man, complex. hybrid
        /// </summary>
        /// <returns></returns>
        public bool isObyeNLinkedCore()
        {
            //Node 1 
            GlycanTreeNode CheckNode = _tree;
            int NoOfHexNac = 0;
            int NoOfHex = 0;
            int NoOfDeHex = 0;
            if (CheckNode.GlycanType != Glycan.Type.HexNAc) //1st Node Only can be HexNac
            {
                return false;
            }
            if (CheckNode.Subtrees != null)  //1st Node can only link to one HexNac and multiple DeHex
            {
                foreach (GlycanTreeNode SubTree in CheckNode.Subtrees)
                {
                    if (SubTree.GlycanType == Glycan.Type.HexNAc)
                    {
                        NoOfHexNac++;
                    }
                    else if (SubTree.GlycanType == Glycan.Type.DeHex)
                    {
                        NoOfDeHex++;
                    }
                }
                if (NoOfHexNac > 1 || (NoOfHexNac + NoOfDeHex != CheckNode.Subtrees.Count))
                {
                    return false;
                }
            }
            if (CheckNode.Subtrees.Count == NoOfDeHex)
            {
                return true;
            }

            //Node 2
            CheckNode = CheckNode.Subtrees[0]; //DeHex will be sort to the end of list
            NoOfHexNac = 0;
            NoOfHex = 0;
            NoOfDeHex = 0;
            if (CheckNode.GlycanType != Glycan.Type.HexNAc)//2nd Node Only can be HexNac
            {
                return false;
            }
            if (CheckNode.Subtrees != null)  //2nd Node can only link to one Hex and multiple DeHex
            {
                foreach (GlycanTreeNode SubTree in CheckNode.Subtrees)
                {
                    if (SubTree.GlycanType == Glycan.Type.Hex)
                    {
                        NoOfHex++;
                    }
                    else if (SubTree.GlycanType == Glycan.Type.DeHex)
                    {
                        NoOfDeHex++;
                    }
                }
                if (NoOfDeHex == CheckNode.Subtrees.Count)
                {
                    return true;
                }
                if (NoOfHex != 1 || (NoOfHex + NoOfDeHex != CheckNode.Subtrees.Count))
                {
                    return false;
                }
            }
            else
            {
                return true;
            }

            //Node 3
            CheckNode = CheckNode.Subtrees[0]; //DeHex will be sort to the end of list
            NoOfHexNac = 0;
            NoOfHex = 0;
            NoOfDeHex = 0;
            if (CheckNode.GlycanType != Glycan.Type.Hex)//3rd Node Only can be Hex
            {
                return false;
            }
            if (CheckNode.Subtrees != null)  //3rd Node can only link to one HexNac and up to two Hex
            {
                foreach (GlycanTreeNode SubTree in CheckNode.Subtrees)
                {
                    if (SubTree.GlycanType == Glycan.Type.Hex)
                    {
                        NoOfHex++;
                    }
                    else if (SubTree.GlycanType == Glycan.Type.HexNAc)
                    {
                        NoOfHexNac++;
                    }
                }
                if (NoOfHex > 2 || NoOfHexNac > 1)
                {
                    return false;
                }
                if (NoOfHexNac + NoOfHex != CheckNode.Subtrees.Count)
                {
                    return false;
                }
                if (CheckNode.Subtrees.Count == 1)
                {
                    if (!(NoOfHex == 1 || NoOfHexNac == 1))
                    {
                        return false;
                    }
                    if (NoOfHexNac == 1 && CheckNode.Subtrees[0].Subtrees != null) //bisecting no child
                    {
                        return false;
                    }
                    foreach (GlycanTreeNode ChildTree in CheckNode.FetchAllGlycanNode())
                    {
                        if (ChildTree.Subtrees != null && ChildTree.Subtrees.Count > 1)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    if (CheckNode.Subtrees.Count != NoOfHex + NoOfHexNac)
                    {
                        return false;
                    }
                }
            }
            else
            {
                return true;
            }

            ////check branch can be either high man, hybrid or complex
            foreach (GlycanTreeNode SubTree in CheckNode.Subtrees)
            {
                if (SubTree.GlycanType == Glycan.Type.Hex)
                {
                    if (SubTree.NoOfTotalGlycan - SubTree.NoOfHex > 0 && SubTree.NoOfHexNac == 0)
                    {
                        return false;
                    }
                }
                else if (SubTree.GlycanType == Glycan.Type.HexNAc) //Bisecting
                {
                    if (SubTree.Subtrees != null)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
    }
}
