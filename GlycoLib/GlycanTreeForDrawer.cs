using System;
using System.Collections.Generic;
using System.Text;

namespace COL.GlycoLib
{
    [Serializable]    
    public class GlycanTreeForDrawer
    {
        Glycan.Type _root;
        List<GlycanTreeForDrawer> _child;
        GlycanTreeForDrawer _parent;
        float _posX;
        float _posY;
        int _distanceToRoot = 0;
        public int DistanceToRoot
        {
            set { _distanceToRoot = value; }
            get { return _distanceToRoot; }
        }
        public GlycanTreeForDrawer Parent
        {
            set { _parent = value; }
            get { return _parent; }
        }
        public GlycanTreeForDrawer()
        {
            _child = new List<GlycanTreeForDrawer>();
        }
        public GlycanTreeForDrawer(Glycan.Type argType)
        {
            _root = argType;
            _child = new List<GlycanTreeForDrawer>();
        }
        public void AddChild(GlycanTreeForDrawer argType)
        {
            if (argType.Root == Glycan.Type.DeHex)
            {
                _child.Insert(_child.Count, argType);
            }
            else
            {
                _child.Add(argType);
            }
            _child.Sort(delegate(GlycanTreeForDrawer p1, GlycanTreeForDrawer p2)
            {
                return p1.GetChild.Count.CompareTo(p2.GetChild.Count);
            });
        }
        public Glycan.Type Root
        {
            set { _root = value; }
            get { return _root; }
        }
        public List<GlycanTreeForDrawer> GetChild
        {
            get { return _child; }
        }
        public float PosX
        {
            get { return _posX; }
            set { _posX = value; }
        }
        public float PosY
        {
            get { return _posY; }
            set { _posY = value; }
        }
        //public int Level
        //{
        //    get { return _level; }
        //    set { _level = value; }
        //}
        public int NumberOfFucChild
        {
            get
            {
                int tmp = 0;
                foreach (GlycanTreeForDrawer g in _child)
                {
                    if (g.Root == Glycan.Type.DeHex)
                    {
                        tmp = tmp + 1;
                    }
                }
                return tmp;
            }
        }
        public void UpdateDistance(int argParentDistance)
        {
            this._distanceToRoot = argParentDistance + 1;
            if (_child.Count != 0)
            {
                foreach (GlycanTreeForDrawer CT in _child)
                {
                    CT.UpdateDistance(this._distanceToRoot);
                }
            }
        }
        public IEnumerable<GlycanTreeForDrawer> TravelGlycanTreeBFS()
        {
            Queue<GlycanTreeForDrawer> GlycanQue = new Queue<GlycanTreeForDrawer>();
            List<GlycanTreeForDrawer> glycanOrder = new List<GlycanTreeForDrawer>();
            glycanOrder.Add(this);

            if (_child.Count != 0)
            {
                foreach (GlycanTreeForDrawer g in _child)
                {
                    GlycanQue.Enqueue(g);
                }
            }
            while (GlycanQue.Count > 0)
            {
                GlycanTreeForDrawer g = (GlycanTreeForDrawer)GlycanQue.Dequeue();
                glycanOrder.Add(g);
                foreach (GlycanTreeForDrawer k in g._child)
                {
                    GlycanQue.Enqueue(k);
                }
            }
            foreach (GlycanTreeForDrawer g in glycanOrder)
            {
                yield return g;
            }
        }
        public IEnumerable<GlycanTreeForDrawer> TravelGlycanTreeDFS()
        {
            Stack<GlycanTreeForDrawer> GlycanStk = new Stack<GlycanTreeForDrawer>();
            List<GlycanTreeForDrawer> glycanOrder = new List<GlycanTreeForDrawer>();
            GlycanStk.Push(this);


            while (GlycanStk.Count != 0)
            {
                GlycanTreeForDrawer g = (GlycanTreeForDrawer)GlycanStk.Peek();
                if (g._child.Count == 0)
                {
                    glycanOrder.Add((GlycanTreeForDrawer)GlycanStk.Pop());
                }
                else
                {
                    int NoneTravedChild = 0;
                    foreach (GlycanTreeForDrawer k in g._child)
                    {
                        if (!glycanOrder.Contains(k))
                        {
                            GlycanStk.Push(k);
                            NoneTravedChild++;
                        }

                    }
                    if (NoneTravedChild == 0)
                    {
                        glycanOrder.Add((GlycanTreeForDrawer)GlycanStk.Pop());
                    }
                }
            }

            foreach (GlycanTreeForDrawer g in glycanOrder)
            {
                yield return g;
            }
        }
    }
}
