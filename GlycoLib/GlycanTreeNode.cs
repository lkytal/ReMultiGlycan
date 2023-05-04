using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using COL.MassLib;
namespace COL.GlycoLib
{
    [Serializable]
    public class GlycanTreeNode : System.Object, ICloneable, IComparable<GlycanTreeNode>, IDisposable
    {
        bool disposed = false;
        private int _NodeID;
        private Glycan.Type _NodeType;
        private List<GlycanTreeNode> _subTrees;
        private float _IDMass = 0.0f;
        private float _IDIntensity = 0.0f;
        private int _charge = 0;
        private int _missedpeak = 0;
        private GlycanTreeNode _parent;
        private string _IUPAC;
        private string _IUPACFromRoot;
        public GlycanTreeNode(Glycan.Type argGlycanType, int argID)
        {
            _NodeType = argGlycanType;
            _NodeID = argID;
        }
        public GlycanTreeNode() { }

        public Glycan.Type GlycanType
        {
            get { return _NodeType; }
        }
        public List<GlycanTreeNode> Subtrees
        {
            get { return _subTrees; }
        }
        public GlycanTreeNode Parent
        {
            set { _parent = value; }
            get { return _parent; }
        }
        public int NodeID
        {
            get { return _NodeID; }
            set { _NodeID = value; }
        }
        public string IUPAC
        {
            get
            {
                if (_IUPAC == "" || _IUPAC == null)
                {
                    _IUPAC = this.GetIUPACString();
                }

                return _IUPAC;
            }
        }

        public string IUPACFromRoot
        {
            get
            {
                if (_IUPACFromRoot == "" || _IUPACFromRoot == null)
                {
                    _IUPACFromRoot = GetIUPACStringFromRootToThis();
                }
                return _IUPACFromRoot;
            }
        }
        /// <summary>
        /// How many missed peak in this glycan
        /// </summary>
        public int MissedPeak
        {
            get
            {

                if (_IDMass == 0.0f)
                {
                    _missedpeak = 1;
                }
                else
                {
                    _missedpeak = 0;
                }
                int tmpCount = _missedpeak;
                if (this.SubTree1 != null)
                {
                    tmpCount += this.SubTree1.MissedPeak;
                }
                if (this.SubTree2 != null)
                {
                    tmpCount += this.SubTree2.MissedPeak;
                }
                if (this.SubTree3 != null)
                {
                    tmpCount += this.SubTree3.MissedPeak;
                }
                if (this.SubTree4 != null)
                {
                    tmpCount += this.SubTree4.MissedPeak;
                }
                return tmpCount;
            }
        }

        public IEnumerable TravelGlycanTreeBFS()
        {
            Queue GlycanQue = new Queue();
            List<GlycanTreeNode> glycanOrder = new List<GlycanTreeNode>();
            glycanOrder.Add(this);

            if (GetChildren() != null && GetChildren().Count != 0)
            {
                foreach (GlycanTreeNode g in GetChildren())
                {
                    GlycanQue.Enqueue(g);
                }
            }
            while (GlycanQue.Count > 0)
            {
                GlycanTreeNode g = (GlycanTreeNode)GlycanQue.Dequeue();
                glycanOrder.Add(g);
                if (g.GetChildren() != null)
                {
                    foreach (GlycanTreeNode k in g.GetChildren())
                    {
                        GlycanQue.Enqueue(k);
                    }
                }
            }
            GlycanQue.Clear();
            GlycanQue = null;
            foreach (GlycanTreeNode g in glycanOrder)
            {
                yield return g;
            }
        }
        public IEnumerable TravelGlycanTreeDFS()
        {
            Stack GlycanStk = new Stack();
            List<GlycanTreeNode> glycanOrder = new List<GlycanTreeNode>();
            GlycanStk.Push(this);
            while (GlycanStk.Count != 0)
            {
                GlycanTreeNode g = (GlycanTreeNode)GlycanStk.Peek();
                if ((g.GetChildren() != null && g.GetChildren().Count == 0) ||
                    g.GetChildren() == null)
                {
                    glycanOrder.Add((GlycanTreeNode)GlycanStk.Pop());
                }
                else
                {
                    int NoneTravedChild = 0;
                    foreach (GlycanTreeNode k in g.GetChildren())
                    {
                        if (!glycanOrder.Contains(k))
                        {
                            GlycanStk.Push(k);
                            NoneTravedChild++;
                        }

                    }
                    if (NoneTravedChild == 0)
                    {
                        glycanOrder.Add((GlycanTreeNode)GlycanStk.Pop());

                    }
                }
            }

            foreach (GlycanTreeNode g in glycanOrder)
            {
                yield return g;
            }
        }
        public List<GlycanTreeNode> GetChildren()
        {
            /*List<GlycanTree> GlycanChild = new List<GlycanTree>();
            if (_subTree1 != null)
            {
                GlycanChild.Add(_subTree1);
            }
            if (_subTree2 != null)
            {
                GlycanChild.Add(_subTree2);
            }
            if (_subTree3 != null)
            {
                GlycanChild.Add(_subTree3);
            }
            if (_subTree4 != null)
            {
                GlycanChild.Add(_subTree4);
            }
            return GlycanChild;*/

            return _subTrees;

        }



        public int CompareTo(GlycanTreeNode obj)
        {
            return -1 * this.Score.CompareTo(obj.Score);//Descending
        }
        public override bool Equals(System.Object obj)
        {
            if (obj == null)
            {
                return false;
            }
            GlycanTreeNode GT = obj as GlycanTreeNode;
            if ((System.Object)GT == null)
            {
                return false;
            }
            if (this.GetIUPACString() == GT.GetIUPACString() && this.Charge == GT.Charge)
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
            return this.Node.GetHashCode();
        }
        public int DistanceRoot
        {
            get
            {
                //return _distanceRoot;
                if (this.Parent == null)
                {
                    return 0;
                }
                else
                {
                    int Level = 1;
                    GlycanTreeNode Par = this.Parent;
                    while (Par.Parent != null)
                    {
                        Level = Level + 1;
                        Par = Par.Parent;
                    }
                    return Level;
                }
            }
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
        public int Charge
        {
            set { _charge = value; }
            get { return _charge; }
        }

        public int NoOfTotalGlycan
        {
            get
            {
                return NoOfHexNac + NoOfHex + NoOfDeHex + NoOfNeuAc + NoOfNeuGc;
                //return GetListsofGlycanTree().Count;
            }
        }
        public int NoOfHexNac
        {
            get
            {
                return GetNoOfGlycan(Glycan.Type.HexNAc);
            }
        }
        public int NoOfHexNacInChild
        {
            get
            {
                if (_subTrees == null)
                {
                    return 0;
                }
                else
                {
                    int Count = 0;
                    foreach (GlycanTreeNode T in _subTrees)
                    {
                        if (T.GlycanType == Glycan.Type.HexNAc)
                        {
                            Count++;
                        }
                    }
                    return Count;
                }
            }
        }
        public int NoOfHex
        {
            get
            {
                return GetNoOfGlycan(Glycan.Type.Hex);
            }
        }
        public int NoOfHexInChild
        {
            get
            {
                if (_subTrees == null)
                {
                    return 0;
                }
                else
                {
                    int Count = 0;
                    foreach (GlycanTreeNode T in _subTrees)
                    {
                        if (T.GlycanType == Glycan.Type.Hex)
                        {
                            Count++;
                        }
                    }
                    return Count;
                }
            }
        }
        public int NoOfDeHex
        {
            get
            {
                return GetNoOfGlycan(Glycan.Type.DeHex);
            }
        }
        public int NoOfDeHexInChild
        {
            get
            {
                if (_subTrees == null)
                {
                    return 0;
                }
                else
                {
                    int Count = 0;
                    foreach (GlycanTreeNode T in _subTrees)
                    {
                        if (T.GlycanType == Glycan.Type.DeHex)
                        {
                            Count++;
                        }
                    }
                    return Count;
                }
            }
        }
        public int NoOfNeuAc
        {
            get
            {
                return GetNoOfGlycan(Glycan.Type.NeuAc);
            }
        }
        public int NoOfNeuACInChild
        {
            get
            {
                if (_subTrees == null)
                {
                    return 0;
                }
                else
                {
                    int Count = 0;
                    foreach (GlycanTreeNode T in _subTrees)
                    {
                        if (T.GlycanType == Glycan.Type.NeuAc)
                        {
                            Count++;
                        }
                    }
                    return Count;
                }
            }
        }
        public int NoOfNeuGc
        {
            get
            {
                return GetNoOfGlycan(Glycan.Type.NeuGc);
            }
        }
        public int NoOfNeuGCInChild
        {
            get
            {
                if (_subTrees == null)
                {
                    return 0;
                }
                else
                {
                    int Count = 0;
                    foreach (GlycanTreeNode T in _subTrees)
                    {
                        if (T.GlycanType == Glycan.Type.NeuGc)
                        {
                            Count++;
                        }
                    }
                    return Count;
                }
            }
        }
        public bool HasHexNAcAncestorOtherThanCore()
        {
            bool _foundHexNAc = false;
            GlycanTreeNode Parent = this.Parent;
            while (Parent.DistanceRoot >= 2)
            {
                if (Parent.GlycanType == Glycan.Type.HexNAc)
                {
                    _foundHexNAc = true;
                }
                Parent = Parent.Parent;
            }
            return _foundHexNAc;
        }



        public float IDMass
        {
            set { _IDMass = value; }
            get { return _IDMass; }
        }
        public float IDIntensity
        {
            get { return _IDIntensity; }
            set { _IDIntensity = value; }
        }
        public Glycan.Type Node
        {
            get { return _NodeType; }
        }
        public float Score
        {
            get
            {
                float score = _IDIntensity;
                if (this.SubTree1 != null)
                {
                    score += this.SubTree1.Score;
                }
                if (this.SubTree2 != null)
                {
                    score += this.SubTree2.Score;
                }
                if (this.SubTree3 != null)
                {
                    score += this.SubTree3.Score;
                }
                if (this.SubTree4 != null)
                {
                    score += this.SubTree4.Score;
                }
                return score;
            }
        }
        private int GetNoOfGlycan(Glycan.Type argGlycanType)
        {
            int count = 0;
            foreach (GlycanTreeNode t in TravelGlycanTreeBFS())
            {
                if (t.GlycanType == argGlycanType)
                {
                    count = count + 1;
                }
            }
            return count;
        }
        /*public void aaUpdateGlycans()
        {            
            _noOfHex = 0;
            _noOfDeHex = 0;
            _noNeuAc =0;
            _noNeuGc =0;
            _noOfHexNac = 0;
            foreach (GlycanTree ChildTree in FetchAllGlycanNode())
            {
                switch (ChildTree.GlycanType)
                {
                    case Glycan.Types.DeHex:
                        _noOfDeHex++;
                        break;
                    case Glycan.Types.Hex:
                        _noOfHex++;
                        break;
                    case Glycan.Types.HexNAc:
                        _noOfHexNac++;
                        break;
                    case Glycan.Types.NeuAc:
                        _noNeuAc++;
                        break;
                    case Glycan.Types.NeuGc:
                        _noNeuGc++;
                        break;                    
                }
            }
            if (_subTrees != null)
            {
                foreach (GlycanTree SubTree in _subTrees)
                {
                    SubTree.Parent = this;
                }
            }
        }*/
        public List<string> GetSequencingMapList()
        {
            List<string> List = new List<string>();
            foreach (GlycanTreeNode GT in this.TravelGlycanTreeBFS())
            {
                if (GT.GetChildren() != null)
                {
                    foreach (GlycanTreeNode subGT in GT.GetChildren())
                    {
                        List.Add(GT.IDMass.ToString() + "-" + subGT.Node.ToString() + "-" + subGT.IDMass);
                    }
                }
            }
            return List;
        }

        public void SortSubTree()
        {
            //if (this.Subtrees.Count > 1)
            //{
            //    int[] SortedIdx  = new int[this.Subtrees.Count];
            //    int[] NoOfGlycanWithoutFuc = new int[this.Subtrees.Count];
            //    float[] GlycanMassWithoutFuc = new float[this.Subtrees.Count];

            //    int tmpIdx;
            //    int tmpNoGLycan;
            //    float tmpMass;
            //    for(int i =0;i<this.Subtrees.Count;i++)
            //    {
            //        SortedIdx[i] = 0;
            //        NoOfGlycanWithoutFuc[i] = this.Subtrees[i].NoOfTotalGlycan - this.Subtrees[i].NoOfDeHexInChild;
            //        GlycanMassWithoutFuc[i] = this.Subtrees[i].NoOfHex * GlycanMass.GetGlycanMass(Glycan.Type.Hex) +
            //                                                         this.Subtrees[i].NoOfHexNac * GlycanMass.GetGlycanMass(Glycan.Type.HexNAc) +
            //                                                          this.Subtrees[i].NoOfNeuAc * GlycanMass.GetGlycanMass(Glycan.Type.NeuAc) +
            //                                                           this.Subtrees[i].NoOfNeuGc * GlycanMass.GetGlycanMass(Glycan.Type.NeuGc);
            //    }
            //    for (int i = 0; i < SortedIdx.Length-1; i++)
            //    {
            //        for (int j = i + 1; j < SortedIdx.Length; j++)
            //        {
            //            if (NoOfGlycanWithoutFuc[i] < NoOfGlycanWithoutFuc[j])
            //            {
            //                tmpIdx = SortedIdx[i];
            //                SortedIdx[i] = SortedIdx[j];
            //                SortedIdx[j] = tmpIdx;

            //                tmpNoGLycan = NoOfGlycanWithoutFuc[i];
            //                NoOfGlycanWithoutFuc[i] = NoOfGlycanWithoutFuc[j];
            //                NoOfGlycanWithoutFuc[j] = tmpNoGLycan;

            //                tmpMass = GlycanMassWithoutFuc[i];
            //                GlycanMassWithoutFuc[i] = GlycanMassWithoutFuc[j];
            //                GlycanMassWithoutFuc[j] = tmpMass;
            //            }
            //            else if (NoOfGlycanWithoutFuc[i]  == NoOfGlycanWithoutFuc[j] &&
            //                        GlycanMassWithoutFuc[i] < GlycanMassWithoutFuc[j])
            //            {
            //                tmpIdx = SortedIdx[i];
            //                SortedIdx[i] = SortedIdx[j];
            //                SortedIdx[j] = tmpIdx;

            //                tmpNoGLycan = NoOfGlycanWithoutFuc[i];
            //                NoOfGlycanWithoutFuc[i] = NoOfGlycanWithoutFuc[j];
            //                NoOfGlycanWithoutFuc[j] = tmpNoGLycan;

            //                tmpMass = GlycanMassWithoutFuc[i];
            //                GlycanMassWithoutFuc[i] = GlycanMassWithoutFuc[j];
            //                GlycanMassWithoutFuc[j] = tmpMass;
            //            }
            //        }
            //    }
            //    List<GlycanTreeNode> SortedSubTree = new List<GlycanTreeNode>();
            //    for (int i = 0; i < SortedIdx.Length; i++)
            //    {
            //        SortedSubTree.Add(this.Subtrees[SortedIdx[i]]);
            //    }
            //    _subTrees = SortedSubTree;
            //}
            foreach (GlycanTreeNode GT in this.FetchAllGlycanNode())
            {
                if (GT.GetChildren() != null && GT.GetChildren().Count > 1)
                {
                    GT.GetChildren().Sort(delegate(GlycanTreeNode p1, GlycanTreeNode p2)
                    {
                        return -1 * p1.NoOfTotalGlycan.CompareTo(p2.NoOfTotalGlycan);
                    });
                }
            }
            for (int i = _subTrees.Count - 2; i >= 0; i--)//Move Fucose to the end of subtree list
            {
                if (_subTrees[i].GlycanType == Glycan.Type.DeHex)
                {
                    GlycanTreeNode tmpGlycan = _subTrees[i];
                    _subTrees.Remove(tmpGlycan);
                    _subTrees.Add(tmpGlycan);
                }
            }
        }
        private bool InTheMiddleOfBranch(string[] argArray, int argIdx)
        {
            for (int i = argIdx; i >= 0; i--)
            {
                if (argArray[i].StartsWith("("))
                {
                    return true;
                }
                else if (argArray[i].StartsWith(")"))
                {
                    return false;
                }
            }
            return false;
        }
        private bool IsTwoGlycanConnect(string argParentNodeID, string argChildNodeID)
        {
            int ParentID = Convert.ToInt32(argParentNodeID.TrimStart(')').TrimStart('(').TrimStart('(').TrimStart(')'));
            int ChildID = Convert.ToInt32(argChildNodeID.TrimStart(')').TrimStart('(').TrimStart('(').TrimStart(')'));
            foreach (GlycanTreeNode t in FetchAllGlycanNode())
            {
                if (t.NodeID == ParentID && t.Subtrees != null)
                {
                    foreach (GlycanTreeNode child in t.Subtrees)
                    {
                        if (child.NodeID == ChildID)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private bool IsTwoGlycanHaveSameParent(string argChildNodeID1, string argChildNodeID2)
        {
            int CID1 = Convert.ToInt32(argChildNodeID1.TrimStart(')').TrimStart('(').TrimStart('(').TrimStart(')'));
            int CID2 = Convert.ToInt32(argChildNodeID2.TrimStart(')').TrimStart('(').TrimStart('(').TrimStart(')'));
            foreach (GlycanTreeNode t in FetchAllGlycanNode())
            {
                if (t.NodeID == CID1)
                {
                    foreach (GlycanTreeNode child in t.Parent.Subtrees)
                    {
                        if (child.NodeID == CID2)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public static GlycanTreeNode IUPACToGlycanTree(string argIUPAC)
        {
            int id = 1;
            GlycanTreeNode GTree = new GlycanTreeNode();
            string[] glycans = argIUPAC.Replace(" ", "").Replace("??", "-").Split('-');
            Stack TreeStake = new Stack();
            for (int i = 0; i < glycans.Length; i++)
            {
                if (glycans[i].Contains(")("))
                {
                    TreeStake.Push(")");
                    TreeStake.Push("(");
                    TreeStake.Push(new GlycanTreeNode(Glycan.String2GlycanType(glycans[i]), id));
                }
                else if (glycans[i].Contains("("))
                {
                    List<GlycanTreeNode> GI = new List<GlycanTreeNode>();
                    while (TreeStake.Count != 0 && TreeStake.Peek().ToString() != ")" && TreeStake.Peek().ToString() != "(")
                    {
                        GI.Add((GlycanTreeNode)TreeStake.Pop());
                    }

                    //Connect Tree
                    GlycanTreeNode ConnectedTree = GI[0];
                    if (GI.Count == 1)
                    {
                        ConnectedTree = GI[0];
                    }
                    else
                    {
                        for (int j = 1; j < GI.Count; j++)
                        {
                            if (ConnectedTree.Subtrees.Count == 0)
                            {
                                ConnectedTree.AddGlycanSubTree(GI[j]);
                            }
                            else
                            {
                                GlycanTreeNode GITree = ConnectedTree.SubTree1;
                                while (GITree.Subtrees.Count != 0)
                                {
                                    GITree = GITree.SubTree1;
                                }
                                GITree.AddGlycanSubTree(GI[j]);
                            }
                        }
                    }
                    TreeStake.Push(ConnectedTree);

                    TreeStake.Push("(");
                    TreeStake.Push(new GlycanTreeNode(Glycan.String2GlycanType(glycans[i]), id));
                }
                else if (glycans[i].Contains(")"))
                {
                    List<GlycanTreeNode> GILst = new List<GlycanTreeNode>();
                    while (TreeStake.Count != 0 && TreeStake.Peek().ToString() != ")" && TreeStake.Peek().ToString() != "(")
                    {
                        GILst.Add((GlycanTreeNode)TreeStake.Pop());
                    }


                    //Connect Tree
                    GlycanTreeNode ConnectedTree = GILst[0];
                    if (GILst.Count == 1)
                    {
                        ConnectedTree = GILst[0];
                    }
                    else
                    {
                        for (int j = 1; j < GILst.Count; j++)
                        {
                            if (ConnectedTree.Subtrees.Count == 0)
                            {
                                ConnectedTree.AddGlycanSubTree(GILst[j]);
                            }
                            else
                            {
                                GlycanTreeNode GITree = ConnectedTree.SubTree1;
                                while (GITree.Subtrees.Count != 0)
                                {
                                    GITree = GITree.SubTree1;
                                }
                                GITree.AddGlycanSubTree(GILst[j]);
                            }
                        }
                    }

                    TreeStake.Push(ConnectedTree);

                    TreeStake.Push(new GlycanTreeNode(Glycan.String2GlycanType(glycans[i]), id));
                    GlycanTreeNode GI = (GlycanTreeNode)TreeStake.Pop();
                    GlycanTreeNode child = (GlycanTreeNode)TreeStake.Pop();
                    child.Parent = GI;
                    GI.AddGlycanSubTree(child);
                    TreeStake.Pop(); //(
                    if (TreeStake.Peek().ToString() == ")") //One More Link
                    {
                        //Todo: Check 3~4 branches parse
                        TreeStake.Pop();//)
                        child = (GlycanTreeNode)TreeStake.Pop();
                        child.Parent = GI;
                        GI.AddGlycanSubTree(child);
                        TreeStake.Pop();//(
                        child = (GlycanTreeNode)TreeStake.Pop();
                        child.Parent = GI;
                        GI.AddGlycanSubTree(child);
                    }
                    else
                    {
                        child = (GlycanTreeNode)TreeStake.Pop();
                        child.Parent = GI;
                        GI.AddGlycanSubTree(child);
                    }
                    TreeStake.Push(GI);
                }
                else
                {
                    if (TreeStake.Count != 0 && TreeStake.Peek().ToString() != ")" && TreeStake.Peek().ToString() != "(")
                    {
                        GlycanTreeNode GI = new GlycanTreeNode(Glycan.String2GlycanType(glycans[i]), id);
                        GlycanTreeNode child = (GlycanTreeNode)TreeStake.Pop();
                        child.Parent = GI;
                        GI.AddGlycanSubTree(child);
                        TreeStake.Push(GI);
                    }
                    else
                    {
                        TreeStake.Push(new GlycanTreeNode(Glycan.String2GlycanType(glycans[i]), id));
                    }
                }
            }
            GTree = (GlycanTreeNode)TreeStake.Pop();
            if (TreeStake.Count != 0)
            {
                throw new Exception("Steak is not zero,Parsing Error");
            }


            return GTree;
        }
        public List<string> Contain(string argIUPAC)
        {
            GlycanTreeNode Pattern = IUPACToGlycanTree(argIUPAC);
            return Contain(Pattern);
        }
        public List<string> Contain(GlycanTreeNode argPattern)
        {
            List<string> MatchNodeId = new List<string>();
            if (argPattern.NoOfTotalGlycan > this.NoOfTotalGlycan ||
                argPattern.NoOfDeHex > this.NoOfDeHex ||
                argPattern.NoOfHex > this.NoOfHex ||
                argPattern.NoOfHexNac > this.NoOfHexNac ||
                argPattern.NoOfNeuAc > this.NoOfNeuAc ||
                argPattern.NoOfNeuGc > this.NoOfNeuGc)
            {
                return MatchNodeId; //none match
            }
            string strPattern = argPattern.GetIUPACString().Replace("-)", "(-").Replace("-(", ")-").Replace("HexNAc", "NAc"); //Change order of symbol for reverse
            //strPattern = "HexNAc-Hex-Hex";
            //strPattern = strPattern.Replace("-)", "(-").Replace("-(", ")-").Replace("HexNAc", "NAc");
            string[] PatternArray = strPattern.Split('-');
            Array.Reverse(PatternArray);
            for (int i = 0; i < PatternArray.Length; i++)
            {
                if (PatternArray[i].EndsWith("("))
                {
                    PatternArray[i] = PatternArray[i].Insert(0, "(").TrimEnd('(');
                }
                else if (PatternArray[i].EndsWith("()"))
                {
                    PatternArray[i] = PatternArray[i].Insert(0, ")(");
                    PatternArray[i] = PatternArray[i].Remove(PatternArray[i].Length - 2);
                }
                else if (PatternArray[i].EndsWith(")"))
                {
                    PatternArray[i] = PatternArray[i].Insert(0, ")").TrimEnd(')');
                }
            }
            string Target = GetIUPACString().Replace("-)", "(-").Replace("-(", ")-").Replace("HexNAc", "NAc");
            string[] TargetArray = Target.Split('-');
            Array.Reverse(TargetArray);
            for (int i = 0; i < TargetArray.Length; i++)
            {
                if (TargetArray[i].EndsWith("("))
                {
                    TargetArray[i] = TargetArray[i].Insert(0, "(").TrimEnd('(');
                }
                else if (TargetArray[i].EndsWith("()"))
                {
                    TargetArray[i] = TargetArray[i].Insert(0, ")(");
                    TargetArray[i] = TargetArray[i].Remove(TargetArray[i].Length - 2);
                }
                else if (TargetArray[i].EndsWith(")"))
                {
                    TargetArray[i] = TargetArray[i].Insert(0, ")").TrimEnd(')');
                }
            }
            string TargetID = GetNodeIDCorrespondIUPAC().Replace("-)", "(-").Replace("-(", ")-");
            string[] TargerIDArray = TargetID.Split('-');
            Array.Reverse(TargerIDArray);
            for (int i = 0; i < TargerIDArray.Length; i++)
            {
                if (TargerIDArray[i].EndsWith("("))
                {
                    TargerIDArray[i] = TargerIDArray[i].Insert(0, "(").TrimEnd('(');
                }
                else if (TargerIDArray[i].EndsWith("()"))
                {
                    TargerIDArray[i] = TargerIDArray[i].Insert(0, ")(");
                    TargerIDArray[i] = TargerIDArray[i].Remove(TargerIDArray[i].Length - 2);
                }
                else if (TargerIDArray[i].EndsWith(")"))
                {
                    TargerIDArray[i] = TargerIDArray[i].Insert(0, ")").TrimEnd(')');
                }
            }
            Queue MatchArrayIdxQueue = new Queue();

            //Start Point
            string Pattern = PatternArray[0];
            for (int i = 0; i < TargetArray.Length; i++)
            {
                if (TargetArray[i].Equals(Pattern) ||
                     TargetArray[i].Equals("(" + Pattern) ||
                     TargetArray[i].Equals(")" + Pattern))
                {
                    MatchArrayIdxQueue.Enqueue(i);
                }
            }
            while (MatchArrayIdxQueue.Count != 0)
            {
                if (MatchArrayIdxQueue.Peek().ToString().Split('-').Length == PatternArray.Length)
                {
                    break; //All done
                }
                string structure = MatchArrayIdxQueue.Dequeue().ToString();
                int lastTargetIdx = Convert.ToInt32(structure.Split('-')[structure.Split('-').Length - 1]);
                int CountOfPattern = structure.Split('-').Length;

                Pattern = PatternArray[CountOfPattern];
                if (Pattern.Contains("(") || Pattern.Contains(")"))
                {
                    for (int j = lastTargetIdx + 1; j < TargetArray.Length; j++)
                    {
                        if (TargetArray[j].Equals(Pattern))
                        {
                            if (IsTwoGlycanConnect(TargerIDArray[lastTargetIdx], TargerIDArray[j]) || IsTwoGlycanHaveSameParent(TargerIDArray[lastTargetIdx], TargerIDArray[j]))
                            {
                                MatchArrayIdxQueue.Enqueue(structure + "-" + j);
                            }
                        }
                    }
                }
                else
                {
                    for (int j = lastTargetIdx + 1; j < TargetArray.Length; j++)
                    {
                        if ((TargetArray[j].Equals(Pattern) && j == lastTargetIdx + 1) || //Must be next pattern
                            (TargetArray[j].Equals("(" + Pattern) && TargetArray[lastTargetIdx + 1].Contains("(")) ||
                            (TargetArray[lastTargetIdx + 1].Contains("(") && TargetArray[j].Equals(")" + Pattern) && !InTheMiddleOfBranch(TargetArray, lastTargetIdx)) //Not in the same Branch
                            )
                        {
                            if (IsTwoGlycanConnect(TargerIDArray[lastTargetIdx], TargerIDArray[j]))
                            {
                                MatchArrayIdxQueue.Enqueue(structure + "-" + j);
                            }
                        }
                    }
                }
            }

            while (MatchArrayIdxQueue.Count != 0)
            {
                MatchNodeId.Add(MatchArrayIdxQueue.Dequeue().ToString());
            }

            return MatchNodeId;
        }
        private bool IsChildGlycanPatternConatinInStructure(GlycanTreeNode argStructure, GlycanTreeNode argPattern)
        {
            int[] StructureCounter = new int[] { 0, 0, 0, 0, 0 };
            int[] PatternCounter = new int[] { 0, 0, 0, 0, 0 };
            foreach (GlycanTreeNode Child in argStructure.Subtrees)
            {
                StructureCounter[(int)Child.GlycanType]++;
            }
            foreach (GlycanTreeNode Child in argPattern.Subtrees)
            {
                PatternCounter[(int)Child.GlycanType]++;
            }
            for (int i = 0; i < 5; i++)
            {
                if (StructureCounter[i] < PatternCounter[i])
                {
                    return false;
                }
            }
            return true;
        }
        public void AddGlycanSubTree(GlycanTreeNode argAddTree)
        {
            if (_subTrees == null)
            {
                _subTrees = new List<GlycanTreeNode>();
                argAddTree.Parent = this;
                _subTrees.Add(argAddTree);
                return;
            }
            argAddTree.Parent = this;
            _subTrees.Add(argAddTree);
            SortSubTree();
        }

        public void AddGlycanToExistChild(GlycanTreeNode argAddTree, int argChildID)
        {
            if (_subTrees == null)
            {
                AddGlycanSubTree(argAddTree);
            }
            _subTrees[argChildID].AddGlycanSubTree(argAddTree);
        }
        public GlycanTreeNode SubTree1
        {
            get
            {
                if (_subTrees != null && _subTrees.Count >= 1)
                {
                    return _subTrees[0];
                }
                else
                {
                    return null;
                }
            }
        }
        public GlycanTreeNode SubTree2
        {
            get
            {
                if (_subTrees != null && _subTrees.Count >= 2)
                {
                    return _subTrees[1];
                }
                else
                {
                    return null;
                }
            }
        }
        public GlycanTreeNode SubTree3
        {
            get
            {
                if (_subTrees != null && _subTrees.Count >= 3)
                {
                    return _subTrees[2];
                }
                else
                {
                    return null;
                }
            }
        }
        public GlycanTreeNode SubTree4
        {
            get
            {
                if (_subTrees != null && _subTrees.Count >= 4)
                {
                    return _subTrees[3];
                }
                else
                {
                    return null;
                }
            }
        }
        public List<GlycanTreeNode> GetListsofGlycanTree()
        {
            //Iterative
            List<GlycanTreeNode> lstGlycansT = new List<GlycanTreeNode>();
            Stack<GlycanTreeNode> stackTreeNode = new Stack<GlycanTreeNode>();
            stackTreeNode.Push(this);
            do
            {
                GlycanTreeNode node = stackTreeNode.Pop();
                lstGlycansT.Add(node);
                if (node._subTrees != null)
                {
                    foreach (GlycanTreeNode subtree in node._subTrees)
                    {
                        if (subtree._subTrees == null)
                        {
                            continue;
                        }
                        if (subtree._subTrees.Count != 0)
                        {
                            stackTreeNode.Push(subtree);
                        }
                    }
                }
            } while (stackTreeNode.Count != 0);
            return lstGlycansT;
            //Recursive
            //List<GlycanTreeNode> lstGlycansT = new List<GlycanTreeNode>();            
            //lstGlycansT.Add(this);
            //if (this.SubTree1 != null)
            //{
            //    foreach (GlycanTreeNode t in this.SubTree1.GetListsofGlycanTree())
            //    {
            //        lstGlycansT.Add(t);
            //    }
            //}
            //if (this.SubTree2 != null)
            //{
            //    foreach (GlycanTreeNode t in this.SubTree2.GetListsofGlycanTree())
            //    {
            //        lstGlycansT.Add(t);
            //    }
            //}
            //if (this.SubTree3 != null)
            //{
            //    foreach (GlycanTreeNode t in this.SubTree3.GetListsofGlycanTree())
            //    {
            //        lstGlycansT.Add(t);
            //    }
            //}
            //if (this.SubTree4 != null)
            //{
            //    foreach (GlycanTreeNode t in this.SubTree4.GetListsofGlycanTree())
            //    {
            //        lstGlycansT.Add(t);
            //    }
            //}
            //return lstGlycansT;
        }
        public float GetMonoMassForGlycanTree(GlycanTreeNode argTree)
        {
            float SUM = 0.0f;
            SUM = argTree.NoOfDeHex * GlycanMass.GetGlycanMass(Glycan.Type.DeHex);
            SUM = SUM + argTree.NoOfHex * GlycanMass.GetGlycanMass(Glycan.Type.Hex);
            SUM = SUM + argTree.NoOfHexNac * GlycanMass.GetGlycanMass(Glycan.Type.HexNAc);
            SUM = SUM + argTree.NoOfNeuAc * GlycanMass.GetGlycanMass(Glycan.Type.NeuAc);
            SUM = SUM + argTree.NoOfNeuGc * GlycanMass.GetGlycanMass(Glycan.Type.NeuGc);
            return SUM;
        }
        public string GetIUPACStringWithNodeID()
        {
            return GenerateIUPACString(true);
        }
        public string GetIUPACString()
        {
            return GenerateIUPACString(false);
        }

        private string GetIUPACStringFromRootToThis()
        {
            return GenerateIUPACStringFromRoot(false);
        }

        private string GenerateIUPACStringFromRoot(bool argWithID)
        {
            List<string> PrintOutID = new List<string>();
            string tmpStr = "";
            GlycanTreeNode StartNode = this;

            while (StartNode.Parent != null)
            {
                StartNode = StartNode.Parent;
            }
            foreach (GlycanTreeNode GN in StartNode.TravelGlycanTreeBFS())
            {
                if (GN.NodeID > this.NodeID)
                {
                    continue;
                }
                if (PrintOutID.Contains(GN.NodeID.ToString("00")))
                {
                    if ((GN.Subtrees == null) || GN.Subtrees.Count == 0)
                    {
                        continue;
                    }
                    else
                    {
                        string ChildandParent;
                        string PID = GN.NodeID.ToString("00") + "," + GN.GlycanType.ToString();
                        ChildandParent = PID + "-";

                        if (GN.Subtrees.Count == 1)
                        {
                            if (GN.Subtrees[0].NodeID > this.NodeID)
                            {
                                continue;
                            }
                            ChildandParent = GN.Subtrees[0].NodeID.ToString("00") + "," + GN.Subtrees[0].GlycanType.ToString() + "-" + ChildandParent;
                            PrintOutID.Add(GN.Subtrees[0].NodeID.ToString("00"));
                        }
                        else
                        {
                            for (int i = GN.Subtrees.Count - 1; i > 0; i--)
                            {
                                if (GN.Subtrees[i].NodeID > this.NodeID)
                                {
                                    continue;
                                }
                                ChildandParent = "(" + GN.Subtrees[i].NodeID.ToString("00") + "," + GN.Subtrees[i].GlycanType.ToString() + "-)" + ChildandParent;
                                PrintOutID.Add(GN.Subtrees[i].NodeID.ToString("00"));
                            }
                            if (GN.Subtrees[0].NodeID > this.NodeID)
                            {
                                continue;
                            }
                            ChildandParent = GN.Subtrees[0].NodeID.ToString("00") + "," + GN.Subtrees[0].GlycanType.ToString() + "-" + ChildandParent;
                            PrintOutID.Add(GN.Subtrees[0].NodeID.ToString("00"));
                        }
                        //Replace 
                        tmpStr = tmpStr.Replace(PID, ChildandParent).Replace("--", "-");
                    }
                }
                else
                {
                    tmpStr = GN.NodeID.ToString("00") + "," + GN.GlycanType.ToString();
                    PrintOutID.Add(GN.NodeID.ToString("00"));
                    if (GN.Subtrees != null)
                    {
                        if (GN.Subtrees.Count == 0)
                        {
                            continue;
                        }
                        if (GN.Subtrees.Count == 1)
                        {
                            if (GN.Subtrees[0].NodeID > this.NodeID)
                            {
                                continue;
                            }
                            tmpStr = GN.Subtrees[0].NodeID.ToString("00") + "," + GN.Subtrees[0].GlycanType.ToString() + "-" + tmpStr;
                            PrintOutID.Add(GN.Subtrees[0].NodeID.ToString("00"));
                        }
                        else
                        {
                            for (int i = GN.Subtrees.Count - 1; i > 0; i--)
                            {
                                if (GN.Subtrees[i].NodeID > this.NodeID)
                                {
                                    continue;
                                }
                                tmpStr = "(" + GN.Subtrees[i].NodeID.ToString("00") + "," + GN.Subtrees[i].GlycanType.ToString() + "-)" + tmpStr;
                                PrintOutID.Add(GN.Subtrees[i].NodeID.ToString("00"));
                            }
                            if (GN.Subtrees[0].NodeID > this.NodeID)
                            {
                                continue;
                            }
                            tmpStr = GN.Subtrees[0].NodeID.ToString("00") + "," + GN.Subtrees[0].GlycanType.ToString() + "-" + tmpStr;
                            PrintOutID.Add(GN.Subtrees[0].NodeID.ToString("00"));
                        }
                    }
                }
            }
            //Clean up the string
            if (argWithID == false)
            {
                tmpStr = System.Text.RegularExpressions.Regex.Replace(tmpStr, @"[\d]", string.Empty).Replace(",", "");
            }
            return tmpStr;
        }
        private string GenerateIUPACString(bool argWithID)
        {
            List<string> PrintOutID = new List<string>();
            string tmpStr = "";
            GlycanTreeNode StartNode = this;

            foreach (GlycanTreeNode GN in StartNode.TravelGlycanTreeBFS())
            {
                if (PrintOutID.Contains(GN.NodeID.ToString("00")))
                {
                    if ((GN.Subtrees == null) || GN.Subtrees.Count == 0)
                    {
                        continue;
                    }
                    else
                    {
                        string ChildandParent;
                        string PID = GN.NodeID.ToString("00") + "," + GN.GlycanType.ToString();
                        ChildandParent = PID + "-";

                        if (GN.Subtrees.Count == 1)
                        {
                            ChildandParent = GN.Subtrees[0].NodeID.ToString("00") + "," + GN.Subtrees[0].GlycanType.ToString() + "-" + ChildandParent;
                            PrintOutID.Add(GN.Subtrees[0].NodeID.ToString("00"));
                        }
                        else
                        {
                            for (int i = GN.Subtrees.Count - 1; i > 0; i--)
                            {
                                ChildandParent = "(" + GN.Subtrees[i].NodeID.ToString("00") + "," + GN.Subtrees[i].GlycanType.ToString() + "-)" + ChildandParent;
                                PrintOutID.Add(GN.Subtrees[i].NodeID.ToString("00"));
                            }
                            ChildandParent = GN.Subtrees[0].NodeID.ToString("00") + "," + GN.Subtrees[0].GlycanType.ToString() + "-" + ChildandParent;
                            PrintOutID.Add(GN.Subtrees[0].NodeID.ToString("00"));
                        }
                        //Replace 
                        tmpStr = tmpStr.Replace(PID, ChildandParent).Replace("--", "-");
                    }
                }
                else
                {
                    tmpStr = GN.NodeID.ToString("00") + "," + GN.GlycanType.ToString();
                    PrintOutID.Add(GN.NodeID.ToString("00"));
                    if (GN.Subtrees != null)
                    {
                        if (GN.Subtrees.Count == 0)
                        {
                            continue;
                        }
                        if (GN.Subtrees.Count == 1)
                        {
                            tmpStr = GN.Subtrees[0].NodeID.ToString("00") + "," + GN.Subtrees[0].GlycanType.ToString() + "-" + tmpStr;
                            PrintOutID.Add(GN.Subtrees[0].NodeID.ToString("00"));
                        }
                        else
                        {
                            for (int i = GN.Subtrees.Count - 1; i > 0; i--)
                            {
                                tmpStr = "(" + GN.Subtrees[i].NodeID.ToString("00") + "," + GN.Subtrees[i].GlycanType.ToString() + "-)" + tmpStr;
                                PrintOutID.Add(GN.Subtrees[i].NodeID.ToString("00"));
                            }
                            tmpStr = GN.Subtrees[0].NodeID.ToString("00") + "," + GN.Subtrees[0].GlycanType.ToString() + "-" + tmpStr;
                            PrintOutID.Add(GN.Subtrees[0].NodeID.ToString("00"));
                        }
                    }
                }
            }
            //Clean up the string
            if (argWithID == false)
            {
                tmpStr = System.Text.RegularExpressions.Regex.Replace(tmpStr, @"[\d]", string.Empty).Replace(",", "");
            }
            return tmpStr;
        }


        /* OLD method Recurivse call TreeNode
        //
        public string GetIUPACString()
        {
            string strTree = "";
            string strSub1 = "";
            string strSub2 = "";
            string strSub3 = "";
            string strSub4 = "";

            if (this.DistanceRoot == 2 && this.Subtrees != null && this.Subtrees.Count > 1)
            {
                int[] GlycanCount = new int[this.Subtrees.Count];
                int BisectingHexNacIdx = -1; //Bisecting
                for (int i = 0; i < this.Subtrees.Count; i++)
                {
                    GlycanCount[i] = this.Subtrees[i].NoOfTotalGlycan - this.Subtrees[i].NoOfDeHex;
                    if (this.Subtrees[i].Node == Glycan.Type.HexNAc)
                    {
                        BisectingHexNacIdx = i; //Bisecting
                    }
                }
                if (GlycanCount.Length == 3) //Has  Bisecting structure
                {
                    if (BisectingHexNacIdx == -1)
                    {
                        List<KeyValuePair<int, int>> Soreted = FindOrder(GlycanCount);
                        //0>1>2
                        strSub1 = this.Subtrees[Soreted[0].Key].GetIUPACString() + "-";
                        strSub2 = this.Subtrees[Soreted[1].Key].GetIUPACString();
                        strSub3 = "(" + this.Subtrees[Soreted[2].Key].GetIUPACString() + "-)";
                    }
                    else if (BisectingHexNacIdx == 0)
                    {
                        strSub2 = "(" + this.Subtrees[BisectingHexNacIdx].GetIUPACString() + "-)";
                        if (GlycanCount[1] > GlycanCount[2])
                        {
                            strSub1 = this.Subtrees[1].GetIUPACString() + "-";
                            strSub3 = "(" + this.Subtrees[2].GetIUPACString() + "-)";
                        }
                        else if (GlycanCount[1] < GlycanCount[2])
                        {
                            strSub1 = this.Subtrees[2].GetIUPACString() + "-";
                            strSub3 = "(" + this.Subtrees[1].GetIUPACString() + "-)";
                        }
                        else
                        {
                            if (GetMonoMassForGlycanTree(this.Subtrees[1]) > GetMonoMassForGlycanTree(this.Subtrees[2]))
                            {
                                strSub1 = this.Subtrees[1].GetIUPACString() + "-";
                                strSub3 = "(" + this.Subtrees[2].GetIUPACString() + "-)";
                            }
                            else
                            {
                                strSub1 = this.Subtrees[2].GetIUPACString() + "-";
                                strSub3 = "(" + this.Subtrees[1].GetIUPACString() + "-)";
                            }
                        }
                    }
                    else if (BisectingHexNacIdx == 1)
                    {
                        strSub2 = "(" + this.Subtrees[BisectingHexNacIdx].GetIUPACString() + "-)";
                        if (GlycanCount[0] > GlycanCount[2])
                        {
                            strSub1 = this.Subtrees[0].GetIUPACString() + "-";
                            strSub3 = "(" + this.Subtrees[2].GetIUPACString() + "-)";
                        }
                        else if (GlycanCount[0] < GlycanCount[2])
                        {
                            strSub1 = this.Subtrees[2].GetIUPACString() + "-";
                            strSub3 = "(" + this.Subtrees[0].GetIUPACString() + "-)";
                        }
                        else
                        {
                            if (GetMonoMassForGlycanTree(this.Subtrees[0]) > GetMonoMassForGlycanTree(this.Subtrees[2]))
                            {
                                strSub1 = this.Subtrees[1].GetIUPACString() + "-";
                                strSub3 = "(" + this.Subtrees[2].GetIUPACString() + "-)";
                            }
                            else
                            {
                                strSub1 = this.Subtrees[2].GetIUPACString() + "-";
                                strSub3 = "(" + this.Subtrees[1].GetIUPACString() + "-)";
                            }
                        }
                    }
                    else
                    {
                        strSub2 = "(" + this.Subtrees[BisectingHexNacIdx].GetIUPACString() + "-)";
                        if (GlycanCount[0] > GlycanCount[1])
                        {
                            strSub1 = this.Subtrees[0].GetIUPACString() + "-";
                            strSub3 = "(" + this.Subtrees[1].GetIUPACString() + "-)";
                        }
                        else if (GlycanCount[0] < GlycanCount[1])
                        {
                            strSub1 = this.Subtrees[1].GetIUPACString() + "-";
                            strSub3 = "(" + this.Subtrees[0].GetIUPACString() + "-)";
                        }
                        else
                        {
                            if (GetMonoMassForGlycanTree(this.Subtrees[0]) > GetMonoMassForGlycanTree(this.Subtrees[1]))
                            {
                                strSub1 = this.Subtrees[1].GetIUPACString() + "-";
                                strSub3 = "(" + this.Subtrees[2].GetIUPACString() + "-)";
                            }
                            else
                            {
                                strSub1 = this.Subtrees[2].GetIUPACString() + "-";
                                strSub3 = "(" + this.Subtrees[1].GetIUPACString() + "-)";
                            }
                        }
                    }
                    strTree = strSub1 + strSub2 + strSub3 + _NodeType.ToString();
                }
                else //no Bisecting
                {
                    if (GlycanCount[0] > GlycanCount[1])
                    {
                        strSub1 = this.Subtrees[0].GetIUPACString() + "-";
                        strSub2 = "(" + this.Subtrees[1].GetIUPACString() + "-)";
                    }
                    else if (GlycanCount[0] < GlycanCount[1])
                    {
                        strSub1 = this.Subtrees[1].GetIUPACString() + "-";
                        strSub2 = "(" + this.Subtrees[0].GetIUPACString() + "-)";
                    }
                    else
                    {
                        if (GetMonoMassForGlycanTree(this.Subtrees[0]) > GetMonoMassForGlycanTree(this.Subtrees[1]))
                        {
                            strSub1 = this.Subtrees[0].GetIUPACString() + "-";
                            strSub2 = "(" + this.Subtrees[1].GetIUPACString() + "-)";
                        }
                        else
                        {
                            strSub1 = this.Subtrees[1].GetIUPACString() + "-";
                            strSub2 = "(" + this.Subtrees[0].GetIUPACString() + "-)";
                        }
                    }
                    strTree = strSub1 + strSub2 + _NodeType.ToString();
                }
            }
            else if (this.DistanceRoot == 3 && this.Subtrees != null && this.Subtrees.Count > 1)
            {
                int[] GlycanCount = new int[this.Subtrees.Count];
                for (int i = 0; i < this.Subtrees.Count; i++)
                {
                    GlycanCount[i] = this.Subtrees[i].NoOfTotalGlycan - this.Subtrees[i].NoOfDeHex;
                }
                if (GlycanCount[0] > GlycanCount[1])
                {
                    strSub1 = this.Subtrees[0].GetIUPACString() + "-";
                    strSub2 = "(" + this.Subtrees[1].GetIUPACString() + "-)";
                }
                else if (GlycanCount[0] < GlycanCount[1])
                {
                    strSub1 = this.Subtrees[1].GetIUPACString() + "-";
                    strSub2 = "(" + this.Subtrees[0].GetIUPACString() + "-)";
                }
                else
                {
                    if (GetMonoMassForGlycanTree(this.Subtrees[0]) > GetMonoMassForGlycanTree(this.Subtrees[1]))
                    {
                        strSub1 = this.Subtrees[0].GetIUPACString() + "-";
                        strSub2 = "(" + this.Subtrees[1].GetIUPACString() + "-)";
                    }
                    else
                    {
                        strSub1 = this.Subtrees[1].GetIUPACString() + "-";
                        strSub2 = "(" + this.Subtrees[0].GetIUPACString() + "-)";
                    }
                }
                strTree = strSub1 + strSub2 + _NodeType.ToString();
            }
            else
            {
                if (this.SubTree1 != null)
                {
                    strSub1 = this.SubTree1.GetIUPACString() + "-";
                }
                if (this.SubTree2 != null)
                {
                    strSub2 = "(" + this.SubTree2.GetIUPACString() + "-)";
                }

                if (this.SubTree3 != null)
                {
                    strSub3 = "(" + this.SubTree3.GetIUPACString() + "-)";
                }

                if (this.SubTree4 != null)
                {
                    strSub4 = "(" + this.SubTree4.GetIUPACString() + "-)";
                }
                strTree = strSub1 + strSub2 + strSub3 + strSub4 + _NodeType.ToString();  //strSub1 First row ; strSub4, 4th row 
            }
            _IUPAC = strTree;
            return strTree;
        }
          */
        /* public string GetIUPACStringWithNodeID()
         {
             string strTree = "";
             string strSub1 = "";
             string strSub2 = "";
             string strSub3 = "";
             string strSub4 = "";
             if (this.SubTree1 != null)
             {
                 strSub1 = this.SubTree1.GetIUPACStringWithNodeID() + "-";
             }
             if (this.SubTree2 != null)
             {
                 strSub2 = "(" + this.SubTree2.GetIUPACStringWithNodeID() + "-)";
             }

             if (this.SubTree3 != null)
             {
                 strSub3 = "(" + this.SubTree3.GetIUPACStringWithNodeID() + "-)";
             }

             if (this.SubTree4 != null)
             {
                 strSub4 = "(" + this.SubTree4.GetIUPACStringWithNodeID() + "-)";
             }
             strTree = strSub1 + strSub2 + strSub3 + strSub4 + _NodeID.ToString()+","+_NodeType.ToString();
             return strTree;
         }*/
        public string GetNodeIDCorrespondIUPAC()
        {
            string strTree = "";
            string strSub1 = "";
            string strSub2 = "";
            string strSub3 = "";
            string strSub4 = "";
            if (this.SubTree1 != null)
            {
                strSub1 = this.SubTree1.GetNodeIDCorrespondIUPAC() + "-";
            }
            if (this.SubTree2 != null)
            {
                strSub2 = "(" + this.SubTree2.GetNodeIDCorrespondIUPAC() + "-)";
            }

            if (this.SubTree3 != null)
            {
                strSub3 = "(" + this.SubTree3.GetNodeIDCorrespondIUPAC() + "-)";
            }

            if (this.SubTree4 != null)
            {
                strSub4 = "(" + this.SubTree4.GetNodeIDCorrespondIUPAC() + "-)";
            }
            strTree = strSub1 + strSub2 + strSub3 + strSub4 + _NodeID;
            return strTree;
        }

        private void AddPeaksToChild(GlycanTreeNode argChildGT)
        {
            argChildGT.IDPeak(_IDMass, _IDIntensity);
        }
        public List<GlycanTreeNode> FetchAllGlycanNode()
        {
            List<GlycanTreeNode> tmpList = new List<GlycanTreeNode>();
            tmpList.Add(this);

            if (this.SubTree1 != null)
            {
                tmpList.AddRange(this.SubTree1.FetchAllGlycanNode());
            }
            if (this.SubTree2 != null)
            {
                tmpList.AddRange(this.SubTree2.FetchAllGlycanNode());
            }
            if (this.SubTree3 != null)
            {
                tmpList.AddRange(this.SubTree3.FetchAllGlycanNode());
            }
            if (this.SubTree4 != null)
            {
                tmpList.AddRange(this.SubTree4.FetchAllGlycanNode());
            }
            return tmpList;
        }

        public bool RemoveGlycan(GlycanTreeNode argRemove)
        {
            bool _found = false;
            int TargetParentLevel = argRemove.DistanceRoot - 1;
            if (TargetParentLevel < 0)
            {
                return false;
            }
            GlycanTreeNode parent = this;
            Stack _ChildStack = new Stack();
            while (_found != true)
            {
                if (parent.DistanceRoot < TargetParentLevel)
                {
                    if (parent.GetChildren() != null && parent.GetChildren().Count == 1) //Only one branch
                    {
                        parent = parent.SubTree1;
                    }
                    else if (parent.GetChildren() != null && parent.GetChildren().Count > 1) //Two or more branch push the other into stack
                    {
                        for (int i = 1; i < parent.GetChildren().Count; i++)
                        {
                            _ChildStack.Push(parent.GetChildren()[i]);
                        }
                        parent = parent.SubTree1;
                    }
                    else
                    {
                        if (_ChildStack.Count != 0)
                        {
                            parent = (GlycanTreeNode)_ChildStack.Pop();
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    if (parent.GetChildren() != null && parent.GetChildren().Exists(delegate(GlycanTreeNode t1) { return t1.GetIUPACString() == argRemove.GetIUPACString(); }))
                    {
                        parent.GetChildren().Remove(argRemove);
                        return true;
                    }
                    else
                    {
                        if (_ChildStack.Count != 0)
                        {
                            parent = (GlycanTreeNode)_ChildStack.Pop();
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            return false;
        }
        public void IDPeak(float argMS, float argIntensity)
        {
            _IDMass = argMS;
            _IDIntensity = argIntensity;
        }
        private GlycanTreeNode ConnectTree(List<GlycanTreeNode> argList)
        {
            GlycanTreeNode Tree = argList[0];
            if (argList.Count == 1)
            {
                return argList[0];
            }
            else
            {
                for (int i = 1; i < argList.Count; i++)
                {
                    if (Tree.Subtrees.Count == 0)
                    {
                        Tree.AddGlycanSubTree(argList[i]);
                    }
                    else
                    {
                        GlycanTreeNode GI = Tree.SubTree1;
                        while (GI.Subtrees.Count != 0)
                        {
                            GI = GI.SubTree1;
                        }
                        GI.AddGlycanSubTree(argList[i]);
                    }
                }
                return Tree;
            }
        }
        /// <summary>
        /// Find the order of Input and return INDEX in DESC order
        /// </summary>
        /// <param name="argInput"></param>
        /// <returns></returns>
        private List<KeyValuePair<int, int>> FindOrder(int[] argInput)
        {
            List<KeyValuePair<int, int>> myList = new List<KeyValuePair<int, int>>();
            for (int i = 0; i < argInput.Length; i++)
            {
                myList.Add(new KeyValuePair<int, int>(i, argInput[i]));
            }
            myList.Sort(delegate(KeyValuePair<int, int> firstPair,
                  KeyValuePair<int, int> nextPair)
                  {
                      return firstPair.Value.CompareTo(nextPair.Value);
                  }
              );
            return myList;
        }

        /*public void AddScorePeak(List<float> argMS, List<float> argIntensity)
        {
            if (_scoreMass == null)
            {
                _scoreMass = new List<float>();
                _scoreIntensity = new List<float>();
            }

            //_scoreMass.AddRange(argMS);
            _scoreIntensity.AddRange(argIntensity);
        }
        public void RemoveScorePeak(float argMS)
        {
            int idx = 0;
            for (int i = 0; i < _scoreMass.Count; i++)
            {
                if (_scoreMass[i] == argMS)
                {
                    break;
                }
                idx++;
            }
            _scoreMass.RemoveAt(idx);
            _scoreIntensity.RemoveAt(idx);
        }*/
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern. 
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                // Free any other managed objects here. 
                _subTrees.Clear();
                _subTrees = null;
                _parent.Dispose();
            }
            // Free any unmanaged objects here. 
            disposed = true;
        }
    }

}
