using System;
using System.Collections.Generic;

namespace Aircadia.WorkflowManagement
{
	[Serializable]
    class NArrayTreeNode : Node
    {
        // Private member-variables
        public string data;
        public int value;
        public string status = "white";
        public List<NArrayTreeNode> childrens = null;

        public List<int[]> variableDimension = new List<int[]>();

        public NArrayTreeNode() { }
        public NArrayTreeNode(string data, int value)
        {
            this.data = data;
            this.value = value;
        }
        public NArrayTreeNode(string data) : this(data, null) { }
        public NArrayTreeNode(string data, List<NArrayTreeNode> childrens)
        {
            this.data = data;
            this.childrens = childrens;
        }
        

     
    }
}
