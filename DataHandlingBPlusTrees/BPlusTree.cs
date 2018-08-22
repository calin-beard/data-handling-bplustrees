using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlingBPlusTrees
{
    class BPlusTree
    {
        /// <summary>
        /// Root node of the tree
        /// </summary>
        public Node Root { get; private set; }

        /// <summary>
        /// Basic constructor; sets the degree of the nodes (static property from the Node class)
        /// </summary>
        /// <param name="degree">Degree of the tree</param>
        public BPlusTree(int degree)
        {
            Node.Degree = degree;
        }

        public BPlusTree(Node root, int degree)
        {
            this.Root = root;
            Node.Degree = degree;
        }

        /// <summary>
        /// Searches the tree for a value
        /// </summary>
        /// <param name="value">Value to search</param>
        /// <returns>Tuple with containing the node and index where the value can be found; if it can't find it, the index where the value should be added is returned</returns>
        public Tuple<Node, int> Search(string value)
        {
            int index = -1;
            Node currentNode = this.Root;
            Tuple<Node, int> results = new Tuple<Node, int>(null, -1);

            while (!currentNode.IsLeaf())
            {
                for (int i = 0; i < currentNode.Keys.Length; i++)
                {
                    if (value.CompareTo(currentNode.Keys[i]) <= 0)
                    {
                        index = i;
                        break;
                    }
                }

                if (index < 0)
                {
                    currentNode = currentNode.Pointers[ArrayHandler.GetIndexOfLastElement(currentNode.Pointers)];
                }
                else if (value.CompareTo(currentNode.Keys[index]) == 0)
                {
                    currentNode = currentNode.Pointers[index + 1];
                }
                else
                {
                    currentNode = currentNode.Pointers[index];
                }
            }

            if (currentNode.IsLeaf())
            {
                for (int i = 0; i < currentNode.Keys.Length; i++)
                {
                    if (value.CompareTo(currentNode.Keys[i]) == 0)
                    {
                        results = new Tuple<Node, int>(currentNode, i);
                        break;
                    }
                    else if (value.CompareTo(currentNode.Keys[i]) < 0 || currentNode.Keys[i] == null)
                    {
                        results = new Tuple<Node, int>(currentNode, i);
                        break;
                    }
                    if (i == currentNode.Keys.Length - 1) //CHECK should check if it's the last index that has a value in the array
                    {
                        results = new Tuple<Node, int>(currentNode, i+1);
                    }
                }
            }
            return results;
        }

        // not used anymore. will delete
        /// <summary>
        /// Splits the target node and returns its new brother
        /// </summary>
        /// <param name="target">Node to split</param>
        /// <returns>The new node that was `reated (brother of the target)</returns>
        //private Node SplitNode(Node target)
        //{
        //    Node brother = new Node(target.Parent, target.IsLeaf());
        //    //add keys and RecordPointers to newTarget, starting with the key Ceiling(degree/2)
        //    //the RecordPointers and keys in the documentation start at 1
        //    //here, 0 based array index is used
        //    for (int i = target.SecondHalfFirstIndex, j = 0; i < target.Keys.Length; i++, j++)
        //    {
        //        //add key to brother
        //        brother.Keys[j] = target.Keys[i];

        //        if (brother.IsLeaf())
        //        {
        //            //add RecordPointer to brother
        //            brother.RecordPointers[j] = target.RecordPointers[i];
        //        }
        //        else
        //        {
        //            //add Pointer to brother
        //            brother.Pointers[j] = target.Pointers[i+1];
        //        }
        //    }

        //    if (!brother.IsLeaf())
        //    {
        //        for (int i = 0; i <= ArrayHandler.GetIndexOfLastElement(brother.Pointers); i++)
        //        {
        //            brother.Pointers[i].Parent = brother;
        //        }
        //    }

        //    ArrayHandler.RemoveFrom(target.SecondHalfFirstIndex, target.Keys);
        //    if (brother.IsLeaf())
        //    {
        //        ArrayHandler.RemoveFrom(target.SecondHalfFirstIndex, target.RecordPointers);
        //    }
        //    else
        //    {
        //        ArrayHandler.RemoveFrom(target.SecondHalfFirstIndex + 1, target.Pointers);
        //    }

        //    //if the target is a leaf
        //    //it is needed to set the correct linked-list links
        //    if (brother.IsLeaf())
        //    {
        //        brother.NextNode = target.NextNode;
        //        target.NextNode = brother;
        //    }

        //    return brother;
        //}

        /// <summary>
        /// Inserts multiple elements in the tree
        /// </summary>
        /// <param name="element">A dictionary containing key value pairs with the keys and recordpointers to be inserted</param>
        public void InsertMultiple(Dictionary<string, RecordPointer> elements)
        {
            foreach (KeyValuePair<string, RecordPointer> element in elements)
            {
                this.Insert(element.Key, element.Value);
            }
        }

        /// <summary>
        /// Inserts a new element in the tree
        /// </summary>
        /// <param name="value">Key to be inserted</param>
        /// <param name="recordpointer">Recordpointer to be inserted along with the value</param>
        public void Insert(string value, RecordPointer recordpointer)
        {
            if (this.Root == null)
            {
                this.Root = new Node(value, recordpointer);
            }
            else
            {
                Tuple<Node, int> searchResult = Search(value);
                Node target = searchResult.Item1;
                int index = searchResult.Item2;

                if ((ArrayHandler.GetIndexOfLastElement(target.Keys) + 1) < target.MaxKeys())
                {
                    InsertInLeaf(target, index, value, recordpointer);
                }
                else
                {
                    //Node newTarget = SplitNode(target);
                    Node brother = new Node(target.Parent, target.IsLeaf());
                    Node temp = new Node(target, 1);
                    InsertInLeaf(temp, index, value, recordpointer);
                    brother.NextNode = target.NextNode;
                    target.NextNode = brother;
                    ArrayHandler.RemoveFrom(0, target.Keys);
                    ArrayHandler.RemoveFrom(0, target.RecordPointers);
                    for (int i = 0; i < temp.SecondHalfFirstIndex(); i++)
                    {
                        target.Keys[i] = temp.Keys[i];
                        target.RecordPointers[i] = temp.RecordPointers[i];
                    }
                    for (int i = 0, j = temp.SecondHalfFirstIndex(); j < temp.Keys.Length; i++, j++)
                    {
                        brother.Keys[i] = temp.Keys[j];
                        brother.RecordPointers[i] = temp.RecordPointers[j];
                    }
                    //InsertInLeaf(newTarget, newTarget.FirstInsertionIndex, value, recordpointer);
                    InsertInParent(target, brother.Keys[0], brother);
                }
            }
        }

        //private void AdjustMinThresholdsAllLeafs()
        //{
        //    Node node = this.Root;
        //    while (!node.IsLeaf())
        //    {
        //        node = node.Pointers[0];
        //    }

        //    while (node.NextNode != null)
        //    {
        //        node.AdjustMinThresholds();
        //        node = node.NextNode;
        //    }
        //    node.AdjustMinThresholds();
        //}

        /// <summary>
        /// Helper method for the insertion algorithm
        /// </summary>
        /// <param name="target">Leaf node to insert value</param>
        /// <param name="index">Index where the value will be inserted in the leaf</param>
        /// <param name="value">Value to insert</param>
        /// <param name="recordpointer">Recordpointer of the value</param>
        private void InsertInLeaf(Node target, int index, string value, RecordPointer recordpointer)
        {
            ArrayHandler.InsertAt(index, target.Keys, value);
            ArrayHandler.InsertAt(index, target.RecordPointers, recordpointer);
        }

        /// <summary>
        /// Helper function for the insertion algorithm
        /// </summary>
        /// <param name="which">Internal node where the value will be inserted</param>
        /// <param name="value">Value to insert</param>
        /// <param name="brother">Brother to insert in root if the root needs to split</param>
        private void InsertInParent(Node which, string value, Node brother)
        {
            if (which.IsRoot())
            {
                this.Root = new Node(null, value, which, brother);
                which.Parent = this.Root;
                //which.AdjustMinThresholds();
                brother.Parent = this.Root;
                //brother.AdjustMinThresholds();
            }
            else
            {
                Node parent = which.Parent;
                int whichIndex = Array.IndexOf(parent.Pointers, which);
                // if (ArrayHandler.GetIndexOfLastElement(parent.Pointers) < parent.MaxPointers - 1)
                if (ArrayHandler.GetIndexOfLastElement(parent.Pointers) + 1 < parent.MaxPointers())
                {
                    //CHECK this after finishing switch                        
                    ArrayHandler.InsertAt(whichIndex, parent.Keys, value);
                    ArrayHandler.InsertAt(whichIndex + 1, parent.Pointers, brother);
                }
                else
                {
                    //Node uncle = SplitNode(parent);
                    Node temp = new Node(parent, 1);
                    ArrayHandler.InsertAt(whichIndex, temp.Keys, value);
                    ArrayHandler.InsertAt(whichIndex + 1, temp.Pointers, brother);
                    ArrayHandler.RemoveFrom(0, parent.Keys);
                    ArrayHandler.RemoveFrom(0, parent.Pointers);
                    Node uncle = new Node(parent, parent.IsLeaf());
                    for (int i = 0; i < temp.SecondHalfFirstIndex(); i++)
                    {
                        parent.Keys[i] = temp.Keys[i];
                        parent.Pointers[i] = temp.Pointers[i];
                    }
                    for (int i = 0, j = temp.SecondHalfFirstIndex(); j < temp.Keys.Length; i++, j++)
                    {
                        uncle.Keys[i] = temp.Keys[j];
                        uncle.Pointers[i] = temp.Pointers[j+1];
                    }
                    //ArrayHandler.InsertAt(uncle.FirstInsertionIndex, uncle.Keys, value);
                    //ArrayHandler.InsertAt(uncle.FirstInsertionIndex, uncle.Pointers, brother);
                    brother.Parent = uncle;
                    //brother.AdjustMinThresholds();
                    for (int i = 0; i <= ArrayHandler.GetIndexOfLastElement(uncle.Pointers); i++)
                    {
                        uncle.Pointers[i].Parent = uncle;
                    }
                    //uncle.AdjustMinThresholds();
                    InsertInParent(parent, uncle.Keys[0], uncle);
                }
            }
        }

        /// <summary>
        /// Updates an element from the tree
        /// </summary>
        /// <param name="value">Value to update</param>
        /// <param name="newvalue">New value for the element</param>
        public void Update(string value, string newvalue)
        {
            //TO DO
            //should use Remove then Add
        }

        /// <summary>
        /// Deletes an element from the tree
        /// </summary>
        /// <param name="value">Value to delete</param>
        public void Delete(string value)
        {
            //TO DO
            Tuple<Node, int> searchResult = Search(value);
            Node target = searchResult.Item1;
            int index = searchResult.Item2;
            RecordPointer targetRecordPointer = target.RecordPointers[index];
            // temporary (there is a bug with the some of the leafs, where the min threshold is that of a root node
            //this.AdjustMinThresholdsAllLeafs();
            // end temporary
            DeleteEntry(target, index, value, null, targetRecordPointer);
        }

        /// <summary>
        /// Helper function for delete algorithm
        /// </summary>
        /// <param name="target">Node where the value will be removed from</param>
        /// <param name="value">Value to remove</param>
        /// <param name="targetRecordPointer">Recordpointer to remove along with the value</param>
        // need to implement recursive bottom up deletion
        private void DeleteEntry(Node target, int index, string value, Node targetPointer = null , RecordPointer targetRecordPointer = null)
        {
            ArrayHandler.RemoveAt(index, target.Keys);
            ArrayHandler.RemoveAt(index, target.RecordPointers);
            if (target.IsRoot() && target.Pointers.Length == 1)
            {
                this.Root = target.Pointers[0];
                target.Pointers[0].Parent = null;
            }
            else if (ArrayHandler.GetIndexOfLastElement(target.Keys) < target.MinKeys())
            {
                int indexOfTargetInParent = Array.IndexOf(target.Parent.Pointers, target);
                int indexOfBrotherInParent;
                string valueBetween = "";
                Node brother = new Node();
                if (indexOfTargetInParent > 0)
                {
                    brother = target.Parent.Pointers[indexOfTargetInParent - 1];
                    valueBetween = target.Parent.Keys[indexOfTargetInParent - 1];
                }
                else if (indexOfTargetInParent < target.Parent.MaxPointers())
                {
                    brother = target.Parent.Pointers[indexOfTargetInParent + 1];
                    valueBetween = target.Parent.Keys[indexOfTargetInParent];
                }
                indexOfBrotherInParent = Array.IndexOf(brother.Parent.Pointers, brother);
                if (ArrayHandler.GetIndexOfLastElement(target.Keys) +1 + ArrayHandler.GetIndexOfLastElement(brother.Keys) + 1 <= target.MaxKeys())
                {
                    if (indexOfTargetInParent < indexOfBrotherInParent)
                    {
                        Node temp = new Node(target, 0);
                        target = new Node(brother, 0);
                        brother = new Node(temp, 0);
                        if (!target.IsLeaf())
                        {
                            Array.Copy(target.Keys, 0, brother.Keys, ArrayHandler.GetIndexOfLastElement(brother.Keys) + 1, target.Keys.Length);
                            //brother.Keys.AddRange(target.Keys);
                            Array.Copy(target.Pointers, 0, brother.Pointers, ArrayHandler.GetIndexOfLastElement(brother.Pointers) + 1, target.Pointers.Length);
                            //brother.Pointers.AddRange(target.Pointers);
                        }
                        else
                        {
                            Array.Copy(target.Keys, 0, brother.Keys, ArrayHandler.GetIndexOfLastElement(brother.Keys) + 1, target.Keys.Length);
                            //brother.Keys.AddRange(target.Keys);
                            Array.Copy(target.Pointers, 0, brother.Pointers, ArrayHandler.GetIndexOfLastElement(brother.Pointers) + 1, target.Pointers.Length);
                            //brother.RecordPointers.AddRange(target.RecordPointers);
                            brother.NextNode = target.NextNode;
                            //TO ADD after updating the function arguments
                            //DeleteEntry(target.Parent, iiiiindex, valueBetween, target, null);
                        }
                    }
                }
                else
                {
                    if (indexOfBrotherInParent < indexOfTargetInParent)
                    {
                        if (!target.IsLeaf())
                        {
                            Node lastChildOfBrother = brother.Pointers.Last();
                            ArrayHandler.InsertAt(0, target.Keys, valueBetween);
                            ArrayHandler.InsertAt(0, target.Pointers, lastChildOfBrother);
                            target.Parent.Keys[Array.IndexOf(target.Parent.Keys, valueBetween)] = brother.Keys.Last();
                            ArrayHandler.RemoveAt(brother.Keys.Length - 1, brother.Keys);
                            ArrayHandler.RemoveAt(brother.Pointers.Length - 1, brother.Pointers);
                        }
                        else
                        {
                            RecordPointer lastRecordPointerOfBrother = brother.RecordPointers.Last();
                            ArrayHandler.InsertAt(0, target.Keys, valueBetween);
                            ArrayHandler.InsertAt(0, target.RecordPointers, lastRecordPointerOfBrother);
                            target.Parent.Keys[Array.IndexOf(target.Parent.Keys, valueBetween)] = brother.Keys.Last();
                            ArrayHandler.RemoveAt(brother.Keys.Length - 1, brother.Keys);
                            ArrayHandler.RemoveAt(brother.RecordPointers.Length - 1, brother.RecordPointers);
                        }
                    }
                    else
                    {
                        if (!target.IsLeaf())
                        {
                            Node firstChildOfBrother = brother.Pointers.First();
                            ArrayHandler.InsertAt(ArrayHandler.GetIndexOfLastElement(target.Keys) + 1, target.Keys, valueBetween);
                            ArrayHandler.InsertAt(ArrayHandler.GetIndexOfLastElement(target.Pointers) + 1, target.Pointers, firstChildOfBrother);
                            target.Parent.Keys[Array.IndexOf(target.Parent.Keys, valueBetween)] = brother.Keys.First();
                            ArrayHandler.RemoveAt(0, brother.Keys);
                            ArrayHandler.RemoveAt(0, brother.Pointers);
                        }
                        else
                        {
                            RecordPointer firstPointerOfBrother = brother.RecordPointers.First();
                            ArrayHandler.InsertAt(ArrayHandler.GetIndexOfLastElement(target.Keys) + 1, target.Keys, valueBetween);
                            ArrayHandler.InsertAt(ArrayHandler.GetIndexOfLastElement(target.RecordPointers) + 1, target.RecordPointers, firstPointerOfBrother);
                            target.Parent.Keys[Array.IndexOf(target.Parent.Keys, valueBetween)] = brother.Keys.First();
                            ArrayHandler.RemoveAt(0, brother.Keys);
                            ArrayHandler.RemoveAt(0, brother.Keys);
                        }
                    }
                }

            }
        }
    }
}