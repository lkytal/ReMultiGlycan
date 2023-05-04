using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using COL.GlycoLib;
namespace COL.GlycoLib
{
    public class StructureRule
    {
        public enum FiltereTypes { Denied=0,Required,Option }

        GlycanTreeNode _structure;
        string _distanceOperator;
        int _distanceToRoot;
        FiltereTypes _type;

        public StructureRule(string argIUPAC, int argDistance, string argOperator,FiltereTypes argType)
        {
            _structure = GlycanTreeNode.IUPACToGlycanTree(argIUPAC);            
            _type = argType;
            _distanceOperator = argOperator;
            _distanceToRoot = argDistance;
            //if(argDistance.Contains("=") || argDistance.Contains("<") || argDistance.Contains(">"))
            //{
            //    _distanceOperator = argDistance.Substring(0,1);
            //    _distanceToRoot = Convert.ToInt32(argDistance.Substring(1));
            //}
            //else if (argDistance.Contains("*"))
            //{
            //    _distanceOperator = "";
            //    _distanceToRoot = -999;
            //}
            //else
            //{
            //    _distanceOperator = "";
            //    _distanceToRoot = Convert.ToInt32(argDistance);
            //}
        }
        public string DistanceOperator
        {
            get { return _distanceOperator; }
        }
        public GlycanTreeNode Structure
        {
            get { return _structure; }
        }
        public int DistanceToRoot
        {
            get { return _distanceToRoot; }
        }
        public FiltereTypes TypeOfRule
        {
            get { return _type; }
        }        
        
    }
}
