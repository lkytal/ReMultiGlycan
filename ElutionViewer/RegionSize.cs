using System;
using System.Collections.Generic;
using System.Text;

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
            get { return _topBound; }
            set { _topBound = value; }
        }

        public float BottomBound
        {
            get { return _bottomBound; }
            set { _bottomBound = value; }
        }

        public float LeftBound
        {
            get { return _leftBound; }
            set { _leftBound = value; }
        }

        public float RightBound
        {
            get { return _rightBound; }
            set { _rightBound = value; }
        }

        public float Width
        {
            get { return Math.Max(0, RightBound - LeftBound); }
        }

        public float Height
        {
            get { return Math.Max(0, TopBound - BottomBound); }
        }
    }
}
