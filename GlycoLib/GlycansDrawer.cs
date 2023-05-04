using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace COL.GlycoLib
{
    public class GlycansDrawer
    {
        GlycanTreeForDrawer GTree;
        
        private string _iupac;
        int Interval_X = 40;
        int Interval_Y = 30;
        int MaxXAxis = 0;
        int MaxYAxis = 0;
        bool _isBW = false;
        float _ScaleFactor = 1.0f;

        public GlycansDrawer(){}
        public GlycansDrawer(string argIUPAC)
        {
            _iupac = argIUPAC;
            ConstructTree();            
            
            Placement(GTree);            
        }
        public GlycansDrawer(string argIUPAC, bool argIsBW)
        {
            _isBW = argIsBW;
            _iupac = argIUPAC;
            ConstructTree();
           
            Placement(GTree);      
        }
          public GlycansDrawer(GlycanTreeForDrawer argTree)
        {
            GTree = argTree;
            Placement(GTree);
        }
        
        public Image GetImage(float argScaleFactor)
        {
            _ScaleFactor = argScaleFactor;
            return GetImage();
        }
        public Image GetImage()
        {
            int SizeX = MaxXAxis + 15;
            int SizeY = MaxYAxis + 15 ;
            Bitmap bmp = new Bitmap(SizeX, SizeY );
            Graphics g = Graphics.FromImage(bmp);
            g.FillRectangle(new SolidBrush(Color.FromArgb(255, 255, 255)), 0, 0, SizeX, SizeY); //white background
            Pen Line = new Pen(Color.Black, 1.0f * _ScaleFactor); //Branch line
            AssignColor();
            foreach (GlycanTreeForDrawer T in GTree.TravelGlycanTreeBFS())
            {
                if (T.GetChild.Count != 0)
                {
                    foreach (GlycanTreeForDrawer GChild in T.GetChild) //Draw Line
                    {
                        g.DrawLine(Line, T.PosX + 5 * _ScaleFactor, T.PosY + 5 * _ScaleFactor, GChild.PosX + 5 * _ScaleFactor, GChild.PosY + 5 * _ScaleFactor);
                    }
                    g.DrawImage(GlycanCartoon(T.Root), T.PosX, T.PosY);
                }
                else
                {
                    g.DrawImage(GlycanCartoon(T.Root), T.PosX, T.PosY);
                }
            }

            return ResizeImage(bmp, _ScaleFactor);
        }
        private Image ResizeImage(Image image, float argPercentage)
        {
            int newWidth;
            int newHeight;

            newWidth = (int)(image.Width * argPercentage);
            newHeight = (int)(image.Height * argPercentage);

            Image newImage = new Bitmap(newWidth, newHeight);
            using (Graphics graphicsHandle = Graphics.FromImage(newImage))
            {
                graphicsHandle.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphicsHandle.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }
        private void AssignColor()
        {
            foreach (GlycanTreeForDrawer T in GTree.TravelGlycanTreeBFS())
            {
                if(T.DistanceToRoot <=3 && T.Root == Glycan.Type.Hex)
                {
                     T.Root = Glycan.Type.Man;
                }
                if (T.DistanceToRoot > 3 && T.Root == Glycan.Type.Hex)
                {
                    T.Root = Glycan.Type.Man;
                    GlycanTreeForDrawer Parent = T.Parent;
                    bool _HexNacFound = false;
                    for (int i = T.DistanceToRoot - 1; i >= 3; i--)
                    {
                        if (Parent.Root == Glycan.Type.HexNAc)
                        {
                            _HexNacFound = true;
                        }
                        Parent = Parent.Parent;
                    }
                    if (_HexNacFound)
                    {
                        T.Root = Glycan.Type.Gal;
                    }
                }
            }
        }       
        public Image GlycanCartoon(Glycan.Type argType)
        {

            Bitmap glycan = new Bitmap(11 ,11 );
            Graphics g = Graphics.FromImage(glycan);

            Brush SolidBrush;
            Pen Linear = new Pen(Color.Black, 1.0f * _ScaleFactor);
            Point[] loc;
            switch (argType)
            {
                case Glycan.Type.DeHex:
                    SolidBrush = new SolidBrush(Color.FromArgb(255,0,0) );
                    if (_isBW)
                    {
                        SolidBrush = new SolidBrush(Color.White);
                    }
                    loc = new Point[] { new Point(0, 10 ), new Point(5 , 0), new Point(10 , 10 ) };
                    g.FillPolygon(SolidBrush, loc);
                    g.DrawPolygon(Linear, loc);
                    break;
                case Glycan.Type.Gal:
                    SolidBrush = new SolidBrush(Color.FromArgb(255, 255, 0));
                    if (_isBW)
                    {
                        SolidBrush = new SolidBrush(Color.White);
                    }
                    g.FillEllipse(SolidBrush, 0, 0, 10 * _ScaleFactor, 10 * _ScaleFactor);
                    g.DrawEllipse(Linear, 0, 0, 10 * _ScaleFactor, 10 * _ScaleFactor);
                    break;
                case Glycan.Type.HexNAc:
                    SolidBrush = new SolidBrush(Color.FromArgb(0, 0, 250));
                    if (_isBW)
                    {
                        SolidBrush = new SolidBrush(Color.White);
                    }
                    g.FillRectangle(SolidBrush, 0, 0, 10 * _ScaleFactor, 10 * _ScaleFactor);
                    g.DrawRectangle(Linear, 0, 0, 10 * _ScaleFactor, 10 * _ScaleFactor);
                    break;
                case Glycan.Type.Man:
                    SolidBrush = new SolidBrush(Color.FromArgb(0, 200, 50));
                    if (_isBW)
                    {
                        SolidBrush = new SolidBrush(Color.White);
                    }
                    g.FillEllipse(SolidBrush, 0, 0, 10 * _ScaleFactor, 10 * _ScaleFactor);
                    g.DrawEllipse(Linear, 0, 0, 10 * _ScaleFactor, 10 * _ScaleFactor);
                    break;
                case Glycan.Type.NeuAc:
                    SolidBrush = new SolidBrush(Color.FromArgb(200, 0, 200));
                    if (_isBW)
                    {
                        SolidBrush = new SolidBrush(Color.White);
                    }
                    loc = new Point[] { new Point(0, 5 ), new Point(5 , 10 ), new Point(10 , 5 ), new Point(5 , 0) };
                    g.FillPolygon(SolidBrush, loc);
                    g.DrawPolygon(Linear, loc);
                    break;
                case Glycan.Type.NeuGc:
                    SolidBrush = new SolidBrush(Color.FromArgb(233, 255, 255));
                    if (_isBW)
                    {
                        SolidBrush = new SolidBrush(Color.White);
                    }
                    loc = new Point[] { new Point(0, 5 ), new Point(5 , 10 ), new Point(10 , 5 ), new Point(5 , 0) };
                    g.FillPolygon(SolidBrush, loc);
                    g.DrawPolygon(Linear, loc);
                    break;
            }
            return glycan;
        }
        
        private void ConstructTree()
        {
            string[] glycans = _iupac.Replace(" ", "").Replace("??", "-").Split('-');
            Stack TreeStake = new Stack();
            for (int i = 0; i < glycans.Length; i++)
            {
                if (glycans[i].Contains(")("))
                {
                    TreeStake.Push(")");
                    TreeStake.Push("(");
                    TreeStake.Push(new GlycanTreeForDrawer(String2GlycanType(glycans[i])));
                }
                else if (glycans[i].Contains("("))
                {
                    List<GlycanTreeForDrawer> GI = new List<GlycanTreeForDrawer>();
                    while (TreeStake.Count != 0 && TreeStake.Peek().ToString() != ")" && TreeStake.Peek().ToString() != "(")
                    {
                        GI.Add((GlycanTreeForDrawer)TreeStake.Pop());
                    }
                    TreeStake.Push(ConnectTree(GI));


                    TreeStake.Push("(");
                    TreeStake.Push(new GlycanTreeForDrawer(String2GlycanType(glycans[i])));
                }
                else if (glycans[i].Contains(")"))
                {
                    List<GlycanTreeForDrawer> GILst = new List<GlycanTreeForDrawer>();
                    while (TreeStake.Count != 0 && TreeStake.Peek().ToString() != ")" && TreeStake.Peek().ToString() != "(")
                    {
                        GILst.Add((GlycanTreeForDrawer)TreeStake.Pop());
                    }
                    TreeStake.Push(ConnectTree(GILst));

                    TreeStake.Push(new GlycanTreeForDrawer(String2GlycanType(glycans[i])));
                    GlycanTreeForDrawer GI = (GlycanTreeForDrawer)TreeStake.Pop();
                    GlycanTreeForDrawer child = (GlycanTreeForDrawer)TreeStake.Pop();
                    child.Parent = GI;
                    GI.AddChild(child);
                    TreeStake.Pop(); //(
                    if (TreeStake.Peek().ToString() == ")") //One More Link
                    {
                        TreeStake.Pop();//)
                        child = (GlycanTreeForDrawer)TreeStake.Pop();
                        child.Parent = GI;
                        GI.AddChild(child);
                        TreeStake.Pop();//(
                        if (TreeStake.Peek().ToString() == ")") //One More Link
                        {
                            TreeStake.Pop();//)
                            child = (GlycanTreeForDrawer)TreeStake.Pop();
                            child.Parent = GI;
                            GI.AddChild(child);
                            TreeStake.Pop();//(
                        }
                        child = (GlycanTreeForDrawer)TreeStake.Pop();
                        child.Parent = GI;
                        GI.AddChild(child);
                    }
                    else
                    {
                        child = (GlycanTreeForDrawer)TreeStake.Pop();
                        child.Parent = GI;
                        GI.AddChild(child);                        
                    }
                    TreeStake.Push(GI);
                }
                else
                {
                    if (TreeStake.Count != 0 && TreeStake.Peek().ToString() != ")" && TreeStake.Peek().ToString() != "(")
                    {
                        GlycanTreeForDrawer GI = new GlycanTreeForDrawer(String2GlycanType(glycans[i]));
                        GlycanTreeForDrawer child = (GlycanTreeForDrawer)TreeStake.Pop();
                        child.Parent = GI;
                        GI.AddChild(child);
                        TreeStake.Push(GI);
                    }
                    else
                    {
                        TreeStake.Push(new GlycanTreeForDrawer(String2GlycanType(glycans[i])));
                    }
                }
            }
            GTree = (GlycanTreeForDrawer)TreeStake.Pop();
            if (TreeStake.Count != 0)
            {
                throw new Exception("Steak is not zero,Parsing Error");
            }
            GTree.UpdateDistance(-1);
             if(!_isBW)
            {
                AssignColor();
            }
        }
        private void Placement(GlycanTreeForDrawer argTree)
        {
            //PostOrderTravel
            List<GlycanTreeForDrawer> DFSOrder = new List<GlycanTreeForDrawer>();

            foreach (GlycanTreeForDrawer t in argTree.TravelGlycanTreeDFS())
            {
                DFSOrder.Add(t);     
            }

            //Assign X Level
            for (int i = 0; i < DFSOrder.Count; i++)
            {
                GlycanTreeForDrawer t = DFSOrder[i];
                if (t.Root == Glycan.Type.DeHex)
                {
                    t.PosX = t.DistanceToRoot - 1;
                }
                else
                {
                    t.PosX = t.DistanceToRoot;
                }
            }

            //Assign Y Level
            for (int i = 0; i < DFSOrder.Count; i++)
            {
                GlycanTreeForDrawer t = DFSOrder[i];

                if (i == 0) //leaf
                {
                    t.PosY =0.0f;
                }
                else
                {
                    if (t.GetChild.Count == 1 && DFSOrder[i - 1].Parent == t) // Only one chlid
                    {
                        t.PosY = DFSOrder[i - 1].PosY;

                        if (t.GetChild[0].Root == Glycan.Type.DeHex)
                        {
                            t.GetChild[0].PosY = t.PosY + 0.5f;
                        }
                    }
                    else if(DFSOrder[i-1].Parent!= t) //branch
                    {
                        t.PosY = DFSOrder[i - 1].PosY + 1.0f;
                    }
                    else if(t.GetChild.Count==2)
                    {
                        if (t.NumberOfFucChild == 0)
                        {
                            t.PosY = (t.GetChild[0].PosY + t.GetChild[1].PosY) / 2;
                        }
                        else
                        {
                            if (t.GetChild[0].Root == Glycan.Type.DeHex && t.GetChild[1].Root == Glycan.Type.DeHex)
                            {
                                t.PosY = (t.GetChild[1].PosY);
                                t.GetChild[0].PosY = t.PosY + 0.5f;
                                t.GetChild[1].PosY = t.PosY + 0.5f;
                            }
                            else if (t.GetChild[0].Root == Glycan.Type.DeHex && t.GetChild[1].Root != Glycan.Type.DeHex)
                            {
                                t.PosY = (t.GetChild[1].PosY);
                                t.GetChild[0].PosY = t.PosY + 0.5f;
                            }
                            else
                            {
                                t.PosY = (t.GetChild[0].PosY);
                                t.GetChild[1].PosY = t.PosY + 0.5f;
                            }
                        }
                    }
                    else if (t.GetChild.Count == 3)
                    {
                        List<GlycanTreeForDrawer> Fuc = new List<GlycanTreeForDrawer>();
                        List<GlycanTreeForDrawer> NonFuc = new List<GlycanTreeForDrawer>();
                        foreach (GlycanTreeForDrawer Gt in t.GetChild)
                        {
                            if (Gt.Root == Glycan.Type.DeHex)
                            {
                                Fuc.Add(Gt);
                            }
                            else
                            {
                                NonFuc.Add(Gt);
                            }
                        }
                        List<float> matrix = new List<float>();
                        foreach (GlycanTreeForDrawer Ct in NonFuc)
                        {
                            matrix.Add(Ct.PosY);
                        }
                        matrix.Sort();

                        if (Fuc.Count == 0)
                        {
                            t.PosY = matrix[1];
                        }
                        else if (Fuc.Count == 1)
                        {
                            t.PosY = (matrix[0]+matrix[1])/2;
                            Fuc[0].PosY = t.PosY + 0.5f;
                        }
                        else if (Fuc.Count == 2)
                        {
                            t.PosY = NonFuc[0].PosY;
                            Fuc[0].PosY = t.PosY + 0.5f;
                            Fuc[1].PosY = t.PosY - 0.5f;
                        }
                        else
                        {
                            foreach (GlycanTreeForDrawer Ct in Fuc)
                            {
                                matrix.Add(Ct.PosY);
                            }
                            matrix.Sort();
                            t.PosY = matrix[1];
                            Fuc[0].PosY = t.PosY + 0.5f;
                            Fuc[1].PosY = t.PosY - 0.5f;
                            Fuc[2].PosY = t.PosY + 0.5f;
                            Fuc[2].PosX = t.PosX + 0.5f;
                        }     

                    }
                    else if (t.GetChild.Count == 4)
                    {
                        if (t.NumberOfFucChild == 0)
                        {
                            List<float> matrix = new List<float>();
                            matrix.Add(t.GetChild[0].PosY);
                            matrix.Add(t.GetChild[1].PosY);
                            matrix.Add(t.GetChild[2].PosY);
                            matrix.Add(t.GetChild[3].PosY);
                            matrix.Sort();
                            t.PosY =( matrix[1]+ matrix[2])/2;
                        }
                        else
                        {
                            List<GlycanTreeForDrawer> Fuc = new List<GlycanTreeForDrawer>();
                            List<GlycanTreeForDrawer> NonFuc = new List<GlycanTreeForDrawer>();
                            foreach (GlycanTreeForDrawer Gt in t.GetChild)
                            {
                                if(Gt.Root == Glycan.Type.DeHex)
                                {
                                    Fuc.Add(Gt);
                                }
                                else
                                {
                                    NonFuc.Add(Gt);
                                }
                            }
                            List<float> matrix = new List<float>();
                            foreach(GlycanTreeForDrawer Ct in NonFuc)
                            {
                                matrix.Add(Ct.PosY);
                            }
                            matrix.Sort();
                            if(Fuc.Count==1)
                            {
                                t.PosY=matrix[1];
                                Fuc[0].PosY = t.PosY + 0.5f;
                            }
                            else if (Fuc.Count==2)
                            {
                                t.PosY = (matrix[0]+matrix[1])/2;
                                Fuc[0].PosY = t.PosY + 0.5f;
                                Fuc[1].PosY = t.PosY - 0.5f;
                            }
                            else if (Fuc.Count==3) 
                            {
                                
                                t.PosY = matrix[0];
                                Fuc[0].PosY = t.PosY + 0.5f;
                                Fuc[1].PosY = t.PosY - 0.5f;
                                Fuc[2].PosY = t.PosY + 0.5f;
                                Fuc[2].PosX = Fuc[2].PosX + 0.5f;

                            }
                            else if (Fuc.Count==4)
                            {
                                foreach (GlycanTreeForDrawer Ct in Fuc)
                                {
                                    matrix.Add(Ct.PosY);
                                }
                                matrix.Sort();
                                t.PosY = (matrix[1]+matrix[2])/2;
                                Fuc[0].PosY = t.PosY + 0.5f;
                                Fuc[1].PosY = t.PosY - 0.5f;
                                Fuc[2].PosY = t.PosY + 0.5f;
                                Fuc[2].PosX = Fuc[2].PosX + 0.5f;
                                Fuc[3].PosY = t.PosY - 0.5f;
                                Fuc[3].PosX = Fuc[2].PosX - 0.5f;

                            }

                        }


                    }

                  
                }
            }
 
            //Convert Level to Real Position

            
            for (int i = 0; i < DFSOrder.Count; i++)
            {
                GlycanTreeForDrawer t = DFSOrder[i];
                t.PosX = t.PosX * Interval_X+2.0f;
                t.PosY = t.PosY * Interval_Y+2.0f;

                if (t.PosX > MaxXAxis)
                {
                    MaxXAxis = Convert.ToInt32(t.PosX);
                }
                if (t.PosY > MaxYAxis)
                {
                    MaxYAxis = Convert.ToInt32(t.PosY);
                }
            }
        }
       
        private GlycanTreeForDrawer ConnectTree(List<GlycanTreeForDrawer> argList)
        {
            GlycanTreeForDrawer Tree = argList[0];
            if (argList.Count == 1)
            {
                return argList[0];
            }
            else
            {                
                for (int i = 1; i < argList.Count; i++)
                {
                    if(Tree.GetChild.Count==0)
                    {
                        Tree.AddChild(argList[i]);
                    }
                    else
                    {
                        GlycanTreeForDrawer GI = Tree.GetChild[0];
                        while (GI.GetChild.Count != 0)
                        {
                            GI = GI.GetChild[0];
                        } 
                        GI.AddChild(argList[i]);
                    }
                }
                return Tree;
            }
        }    
        private Glycan.Type String2GlycanType(string argType)
        {
            if (argType.ToLower().Contains("glcnac") || argType.ToLower().Contains("hexnac"))
            {
                return Glycan.Type.HexNAc;
            }
            else if (argType.ToLower().Contains("fuc")|| argType.ToLower().Contains("dehex"))
            {
                return Glycan.Type.DeHex;
            }
            else if (argType.ToLower().Contains("gal"))
            {
                return Glycan.Type.Gal;
            }
            else if (argType.ToLower().Contains("neuac"))
            {
                return Glycan.Type.NeuAc;
            }
            else if (argType.ToLower().Contains("neugc"))
            {
                return Glycan.Type.NeuGc;
            }
            else if (argType.ToLower().Contains("man"))
            {
                return Glycan.Type.Man;
            }
            else if (argType.ToLower().Contains("hex"))
            {
                return Glycan.Type.Hex;
            }
            else
            {
                throw new Exception("IUPAC contain unrecognized glycan or string");
            }
        }
        
        private void AsignPosY2Nodes(GlycanTreeForDrawer GT)
        {
            GlycanTreeForDrawer childT1 = GT.GetChild[0];
            GlycanTreeForDrawer childT2 = GT.GetChild[1];
            if (GT.GetChild.Count == 3)
            {
                if (GT.GetChild[0].Root == Glycan.Type.DeHex)
                {
                    childT1 = GT.GetChild[1];
                    childT2 = GT.GetChild[2];
                }
                if (GT.GetChild[1].Root == Glycan.Type.DeHex)
                {
                    childT1 = GT.GetChild[0];
                    childT2 = GT.GetChild[2];
                }
                if (GT.GetChild[2].Root == Glycan.Type.DeHex)
                {
                    childT1 = GT.GetChild[0];
                    childT2 = GT.GetChild[1];
                }
            }
            
            int GradChildT1 = childT1.GetChild.Count - childT1.NumberOfFucChild;
            int GradChildT2 = childT2.GetChild.Count - childT2.NumberOfFucChild;
            
            childT1.PosX = GT.PosX - Interval_X;
            childT2.PosX = GT.PosX - Interval_X;
            if (GradChildT1 + GradChildT2 <= 2)
            {
                childT1.PosY = GT.PosY - Interval_Y;
                childT2.PosY = GT.PosY + Interval_Y;
            }
            else
            {
                switch (GradChildT1)
                {
                    case 1:
                        switch (GradChildT2)
                        {
                            case 2:
                                childT1.PosY = GT.PosY - Interval_Y * 1.5f;
                                childT2.PosY = GT.PosY + Interval_Y * 1.5f;
                                break;
                            case 3:
                                childT1.PosY = GT.PosY - Interval_Y * 2.0f;
                                childT2.PosY = GT.PosY + Interval_Y * 2.0f;
                                break;
                            case 4:
                                childT1.PosY = GT.PosY - Interval_Y * 2.5f;
                                childT2.PosY = GT.PosY + Interval_Y * 2.5f;
                                break;
                        }
                        break;
                    case 2:
                        switch (GradChildT2)
                        {
                            case 1:
                                childT1.PosY = GT.PosY - Interval_Y * 1.5f;
                                childT2.PosY = GT.PosY + Interval_Y * 1.5f;
                                break;
                            case 2:
                                childT1.PosY = GT.PosY - Interval_Y * 2.0f;
                                childT2.PosY = GT.PosY + Interval_Y * 2.0f;
                                break;
                            case 3:
                                childT1.PosY = GT.PosY - Interval_Y * 2.5f;
                                childT2.PosY = GT.PosY + Interval_Y * 2.5f;
                                break;
                            case 4:
                                childT1.PosY = GT.PosY - Interval_Y * 3.0f;
                                childT2.PosY = GT.PosY + Interval_Y * 3.0f;
                                break;
                        }
                        break;
                    case 3:
                        switch (GradChildT2)
                        {
                            case 1:
                                childT1.PosY = GT.PosY - Interval_Y * 2.0f;
                                childT2.PosY = GT.PosY + Interval_Y * 2.0f;
                                break;
                            case 2:
                                childT1.PosY = GT.PosY - Interval_Y * 2.5f;
                                childT2.PosY = GT.PosY + Interval_Y * 2.5f;
                                break;
                            case 3:
                                childT1.PosY = GT.PosY - Interval_Y * 3.0f;
                                childT2.PosY = GT.PosY + Interval_Y * 3.0f;
                                break;
                            case 4:
                                childT1.PosY = GT.PosY - Interval_Y * 3.5f;
                                childT2.PosY = GT.PosY + Interval_Y * 3.5f;
                                break;
                        }
                        break;
                    case 4:
                        switch (GradChildT2)
                        {
                            case 1:
                                childT1.PosY = GT.PosY - Interval_Y * 2.5f;
                                childT2.PosY = GT.PosY + Interval_Y * 2.5f;
                                break;
                            case 2:
                                childT1.PosY = GT.PosY - Interval_Y * 3.0f;
                                childT2.PosY = GT.PosY + Interval_Y * 3.0f;
                                break;
                            case 3:
                                childT1.PosY = GT.PosY - Interval_Y * 3.5f;
                                childT2.PosY = GT.PosY + Interval_Y * 3.5f;
                                break;
                            case 4:
                                childT1.PosY = GT.PosY - Interval_Y * 4.0f;
                                childT2.PosY = GT.PosY + Interval_Y * 4.0f;
                                break;
                        }
                        break;
                }
            }
        }
        private void AsignPosY3Nodes(GlycanTreeForDrawer GT)
        {
            GlycanTreeForDrawer childT1 = GT.GetChild[0];
            GlycanTreeForDrawer childT2 = GT.GetChild[1];
            GlycanTreeForDrawer childT3 = GT.GetChild[2];
            if (GT.GetChild.Count == 4)
            {
                if (GT.GetChild[0].Root == Glycan.Type.DeHex)
                {
                    childT1 = GT.GetChild[1];
                    childT2 = GT.GetChild[2];
                    childT3 = GT.GetChild[3];
                }
                else if (GT.GetChild[1].Root == Glycan.Type.DeHex)
                {
                    childT1 = GT.GetChild[0];
                    childT2 = GT.GetChild[2];
                    childT3 = GT.GetChild[3];
                }
                else if (GT.GetChild[2].Root == Glycan.Type.DeHex)
                {
                    childT1 = GT.GetChild[0];
                    childT2 = GT.GetChild[1];
                    childT3 = GT.GetChild[3];
                }
                else
                {
                    childT1 = GT.GetChild[0];
                    childT2 = GT.GetChild[1];
                    childT3 = GT.GetChild[2];
                }
            }
           
            

            int GradChildT1 = childT1.GetChild.Count - childT1.NumberOfFucChild;
            int GradChildT2 = childT2.GetChild.Count - childT2.NumberOfFucChild;
            int GradChildT3 = childT3.GetChild.Count - childT3.NumberOfFucChild;
            childT1.PosX = GT.PosX - Interval_X;
            childT2.PosX = GT.PosX - Interval_X;
            childT3.PosX = GT.PosX - Interval_X;
            childT2.PosY = GT.PosY;
            if (GradChildT1 + GradChildT2 <= 3)
            {
                childT1.PosY = GT.PosY - Interval_Y * 2.0f;
                childT2.PosY = GT.PosY;
                childT3.PosY = GT.PosY + Interval_Y * 2.0f;
            }
            else
            {
                switch (GradChildT1)
                {
                    case 1:
                        switch (GradChildT2)
                        {
                            case 2:                                
                                    childT1.PosY = GT.PosY - Interval_Y * 3.0f;
                                    childT3.PosY = GT.PosY + Interval_Y * (  2.5f + GradChildT3 * 0.5f );                 
                                break;
                            case 3:
                                    childT1.PosY = GT.PosY - Interval_Y * 4.0f;
                                    childT3.PosY = GT.PosY + Interval_Y * (  3.5f + GradChildT3 * 0.5f );
                                break;
                            case 4:                                
                                    childT1.PosY = GT.PosY - Interval_Y * 4.5f;
                                    childT3.PosY = GT.PosY + Interval_Y * (  4.0f + GradChildT3 * 0.5f );
                                break;
                        }
                        break;
                    case 2:
                        switch (GradChildT2)
                        {
                            case 1:
                                childT1.PosY = GT.PosY - Interval_Y * 3.0f;
                                childT3.PosY = GT.PosY + Interval_Y * (1.5f + GradChildT3 *0.5f);
                                break;
                            case 2:
                                childT1.PosY = GT.PosY - Interval_Y * 4.0f;
                                childT3.PosY = GT.PosY + Interval_Y * (2.5f + GradChildT3 * 0.5f);
                                break;
                            case 3:
                                childT1.PosY = GT.PosY - Interval_Y * 4.5f;
                                childT3.PosY = GT.PosY + Interval_Y * (3.5f + GradChildT3 * 0.5f);
                                break;
                            case 4:
                                childT1.PosY = GT.PosY - Interval_Y * 4.5f;
                                childT3.PosY = GT.PosY + Interval_Y * (4.0f + GradChildT3 * 0.5f);
                                break;
                        }
                        break;
                    case 3:
                        switch (GradChildT2)
                        {
                            case 1:
                                childT1.PosY = GT.PosY - Interval_Y * 4.0f;
                                childT3.PosY = GT.PosY + Interval_Y * (1.5f + GradChildT3 * 0.5f);
                                break;
                            case 2:
                                childT1.PosY = GT.PosY - Interval_Y * 4.5f;
                                childT3.PosY = GT.PosY + Interval_Y * (2.5f + GradChildT3 * 0.5f);
                                break;
                            case 3:
                                childT1.PosY = GT.PosY - Interval_Y * 5.0f;
                                childT3.PosY = GT.PosY + Interval_Y * (3.5f + GradChildT3 * 0.5f);
                                break;
                            case 4:
                                childT1.PosY = GT.PosY - Interval_Y * 5.5f;
                                childT3.PosY = GT.PosY + Interval_Y * (4.0f + GradChildT3 * 0.5f);
                                break;
                        }
                        break;
                    case 4:
                        switch (GradChildT2)
                        {
                            case 1:
                                childT1.PosY = GT.PosY - Interval_Y * 4.5f;
                                childT3.PosY = GT.PosY + Interval_Y * (1.5f + GradChildT3 * 0.5f);
                                break;
                            case 2:
                                childT1.PosY = GT.PosY - Interval_Y * 5.0f;
                                childT3.PosY = GT.PosY + Interval_Y * (2.5f + GradChildT3 * 0.5f);
                                break;
                            case 3:
                                childT1.PosY = GT.PosY - Interval_Y * 5.5f;
                                childT3.PosY = GT.PosY + Interval_Y * (3.5f + GradChildT3 * 0.5f);
                                break;
                            case 4:
                                childT1.PosY = GT.PosY - Interval_Y * 6.0f;
                                childT3.PosY = GT.PosY + Interval_Y * (4.0f + GradChildT3 * 0.5f);
                                break;
                        }
                        break;
                }
            }
        }
    }
    
}

