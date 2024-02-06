using System;
using System.Collections.Generic;

namespace COL.GlycoLib
{
	[Serializable]
	public class GlycanTreeForDrawer
	{
		private Glycan.Type _root;
		private List<GlycanTreeForDrawer> _child;
		private GlycanTreeForDrawer _parent;
		private float _posX;
		private float _posY;
		private int _distanceToRoot = 0;

		public int DistanceToRoot
		{
			set => _distanceToRoot = value;
			get => _distanceToRoot;
		}

		public GlycanTreeForDrawer Parent
		{
			set => _parent = value;
			get => _parent;
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
			_child.Sort(delegate (GlycanTreeForDrawer p1, GlycanTreeForDrawer p2)
			{
				return p1.GetChild.Count.CompareTo(p2.GetChild.Count);
			});
		}

		public Glycan.Type Root
		{
			set => _root = value;
			get => _root;
		}

		public List<GlycanTreeForDrawer> GetChild => _child;

		public float PosX
		{
			get => _posX;
			set => _posX = value;
		}

		public float PosY
		{
			get => _posY;
			set => _posY = value;
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
				var tmp = 0;
				foreach (var g in _child)
				{
					if (g.Root == Glycan.Type.DeHex)
					{
						tmp += 1;
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
				foreach (var CT in _child)
				{
					CT.UpdateDistance(this._distanceToRoot);
				}
			}
		}

		public IEnumerable<GlycanTreeForDrawer> TravelGlycanTreeBFS()
		{
			var GlycanQue = new Queue<GlycanTreeForDrawer>();
			var glycanOrder = new List<GlycanTreeForDrawer>();
			glycanOrder.Add(this);

			if (_child.Count != 0)
			{
				foreach (var g in _child)
				{
					GlycanQue.Enqueue(g);
				}
			}
			while (GlycanQue.Count > 0)
			{
				var g = (GlycanTreeForDrawer)GlycanQue.Dequeue();
				glycanOrder.Add(g);
				foreach (var k in g._child)
				{
					GlycanQue.Enqueue(k);
				}
			}
			foreach (var g in glycanOrder)
			{
				yield return g;
			}
		}

		public IEnumerable<GlycanTreeForDrawer> TravelGlycanTreeDFS()
		{
			var GlycanStk = new Stack<GlycanTreeForDrawer>();
			var glycanOrder = new List<GlycanTreeForDrawer>();
			GlycanStk.Push(this);

			while (GlycanStk.Count != 0)
			{
				var g = (GlycanTreeForDrawer)GlycanStk.Peek();
				if (g._child.Count == 0)
				{
					glycanOrder.Add((GlycanTreeForDrawer)GlycanStk.Pop());
				}
				else
				{
					var NoneTravedChild = 0;
					foreach (var k in g._child)
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

			foreach (var g in glycanOrder)
			{
				yield return g;
			}
		}
	}
}