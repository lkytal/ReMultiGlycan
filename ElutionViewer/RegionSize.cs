using System;

namespace COL.ElutionViewer
{
	public class RegionSize : EventArgs
	{
		private float _leftBound;
		private float _rightBound;
		private float _topBound;
		private float _bottomBound;

		public RegionSize()
		{
		}

		public RegionSize(float leftBound, float rightBound, float topBound, float bottomBound)
		{
			_leftBound = leftBound;
			_rightBound = rightBound;
			_topBound = topBound;
			_bottomBound = bottomBound;
		}

		public float TopBound
		{
			get => _topBound;
			set => _topBound = value;
		}

		public float BottomBound
		{
			get => _bottomBound;
			set => _bottomBound = value;
		}

		public float LeftBound
		{
			get => _leftBound;
			set => _leftBound = value;
		}

		public float RightBound
		{
			get => _rightBound;
			set => _rightBound = value;
		}

		public float Width => Math.Max(0, RightBound - LeftBound);

		public float Height => Math.Max(0, TopBound - BottomBound);
	}
}