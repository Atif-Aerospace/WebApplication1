using System;
using System.Collections.Generic;
using System.Linq;

namespace Aircadia.WorkflowManagement
{
	class NArrayTree
    {
        List<string> postOrderTrav = new List<string>();
        public List<NArrayTreeNode> treeNodes;
        public NArrayTree()
        {
            treeNodes = new List<NArrayTreeNode>();
        }
        public NArrayTree(List<NArrayTreeNode> treeNodeslist)
        {
            treeNodes = treeNodeslist;
        }

        public NArrayTreeNode GetNode(String tt)  //YHB:this mehtod returns the GraphNode object with given value
        {
            NArrayTreeNode returnNode = null;
            foreach (NArrayTreeNode item in treeNodes)
            {
                if (item.data == tt)
                    returnNode = item;
            }

            return returnNode;
        }

        public bool compareDimensions(int[] d1, int[] d2)
        {
            bool returnFlag = true;
            for (int i = 0; i < d1.Count(); i++)
            {
                if (d1[i] != d2[i])
                {
                    returnFlag = false;
                    break;
                }
            }

            return returnFlag;
        }

        public int postOrderAggregation(NArrayTreeNode root)
        {//this method aggregates variables at any sub-tree of the N-arry tree given the root of the sub-tree or tree
            int returnValue = 0;


            if (root == null)
                return returnValue = 0;

            if (root.childrens == null)
            {//Here write code to search the variable with given dimension
                //if childrens are null, it means it is leap nodes in component hierarchy. so, variables associated with the component in computational domain are searched to get the
                // required dimension variables. Once the variable is found, its value is assigned to the root.value otherwise it is assigned as zero.
               /* int[] masDim = new int[] { 1, 0, 0, 0, 0, 0, 0 };
                foreach (var item in root.variableDimension)
                {
                    if (compareDimensions(item, masDim))
                    {
                        root.value = root.value+0; //need to write code to take the same dimension varialbe value 
                    }

                } */
                returnValue = root.value;
            }
            else
            {
                foreach (NArrayTreeNode item in root.childrens)
                {
                    root.value = root.value + postOrderAggregation(item);
                }
            }

            return returnValue = root.value;

        }


        public void postOrderTravMethod(NArrayTreeNode root)
        {//post-order traversal method for N-arry tree
            if (root != null)
            {
                if (root.childrens != null)
                {
                    foreach (NArrayTreeNode item in root.childrens)
                    {
                        postOrderTravMethod(item);
                    }
                }
                //Visit the node by Printing the node data  

            }

			postOrderTrav.Add(root.data);
        }



    }
}
