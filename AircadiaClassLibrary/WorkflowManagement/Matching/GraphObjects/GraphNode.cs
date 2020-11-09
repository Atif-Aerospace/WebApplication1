using System;
namespace Aircadia.WorkflowManagement

{//GraphNode inherites NOde class
	[Serializable]
    public class GraphNode : Node
    {
        public int index = -1;//YHB: added to take care of 'index' variable in TarjanCycleDetection code
        public int lowlink = -1;//YHB: added to take care of 'lowlink' variable in TarjanCycleDetection code

        public GraphNode() : base() { }
        public GraphNode(string value) : base(value) { }
        public GraphNode(string value, NodeList neighbors) : base(value, neighbors) { }
        public GraphNode(string value, Type type) : base(value) { NodeType = type; } //YHB: added to take care of 'variable' and 'model' type of nodes in a computational workflow

		public Type NodeType { get; set; }
		public enum Type { Type1, Type2 }
	}
}
