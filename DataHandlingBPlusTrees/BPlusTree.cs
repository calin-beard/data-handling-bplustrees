﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlingBPlusTrees
{
    class BPlusTree
    {
        public Node Root { get; private set; }

        public BPlusTree(int degree)
        {
            Node.Degree = degree;
        }

        public BPlusTree(Node root, int degree)
        {
            this.Root = root;
            Node.Degree = degree;
        }

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
                    }
                    else if (value.CompareTo(currentNode.Keys[i]) < 0 || currentNode.Keys[i] == null)
                    {
                        results = new Tuple<Node, int>(currentNode, i);
                        break;
                    }
                    if (i == currentNode.Keys.Length - 1) //CHECK should check if it's the last index that has a value in the array
                    {
                        results = new Tuple<Node, int>(currentNode, i);
                    }
                }
            }
            return results;
        }

        private Node SplitNode(Node target)
        {
            Node brother = new Node(target.Parent, target.IsLeaf());
            //add keys and RecordPointers to newTarget, starting with the key Ceiling(degree/2)
            //the RecordPointers and keys in the documentation start at 1
            //here, 0 based array index is used
            for (int i = target.SecondHalfFirstIndex; i < target.Keys.Length; i++)
            {
                //add key to brother
                brother.Keys[ArrayHandler.GetIndexOfLastElement(brother.Keys) + 1] = target.Keys[i];
                //remove key from target
                ArrayHandler.RemoveAt(i, target.Keys);

                if (brother.IsLeaf())
                {
                    //add Pointer to brother
                    brother.RecordPointers[ArrayHandler.GetIndexOfLastElement(brother.RecordPointers) + 1] = target.RecordPointers[i];
                    //remove Pointer from target
                    ArrayHandler.RemoveAt(i, target.RecordPointers);
                }
                else
                {
                    //add Child to brother
                    brother.Pointers[ArrayHandler.GetIndexOfLastElement(brother.Pointers) + 1] = target.Pointers[i];
                    //remove Pointer from target
                    ArrayHandler.RemoveAt(i, target.Pointers);
                }
                
            }
            //if the target is a leaf
            //it is needed to set the correct linked-list links
            if (brother.IsLeaf())
            {
                brother.NextNode = target.NextNode;
                target.NextNode = brother;
            }

            return brother;
        }

        //private void CoalesceNodes(Node target, Node brother)
        //{
        //    if (target.Parent.Pointers.IndexOf(target) < target.Parent.Pointers.IndexOf(brother))
        //    {
        //        //TO DO
        //    }
        //}

        public void InsertMultiple(Dictionary<string, Pointer> searchKeys)
        {
            foreach (KeyValuePair<string, Pointer> searchKey in searchKeys)
            {
                this.Insert(searchKey.Key, searchKey.Value);
            }
        }

        public void Insert(string value, Pointer pointer)
        {
            if (this.Root == null)
            {
                this.Root = new Node(value, pointer);
            }
            else
            {
                Tuple<Node, int> searchResult = Search(value);
                Node target = searchResult.Item1;
                int index = searchResult.Item2;

                if ((ArrayHandler.GetIndexOfLastElement(target.Keys) + 1) < target.MaxKeys) //CHECK try checking if last target.Keys[target.Keys.Length - 1] != null
                {
                    InsertInLeaf(target, index, value, pointer);
                }
                else
                {
                    Node newTarget = SplitNode(target);
                    InsertInLeaf(newTarget, newTarget.FirstInsertionIndex, value, pointer);
                    InsertInParent(target, newTarget.Keys[0], newTarget);
                }
            }
        }

        private void InsertInLeaf(Node target, int index, string value, Pointer pointer)
        {
            ArrayHandler.InsertAt(index, target.Keys, value);
            ArrayHandler.InsertAt(index, target.RecordPointers, pointer);
        }

        private void InsertInParent(Node which, string value, Node brother)
        {
            if (which.IsRoot())
            {
                this.Root = new Node(null, value, which, brother);
                //added this to constructor
                //which.Parent = this.Root;
                //brother.Parent = this.Root;
            }
            else
            {
                Node parent = which.Parent;
                // if (ArrayHandler.GetIndexOfLastElement(parent.Pointers) < parent.MaxPointers - 1)
                if (ArrayHandler.GetIndexOfLastElement(parent.Pointers) < parent.MaxPointers)
                {
                    int whichIndex = Array.IndexOf(parent.Pointers, which);
                    //CHECK this after finishing switch
                    ArrayHandler.InsertAt(whichIndex, parent.Keys, value);
                    ArrayHandler.InsertAt(whichIndex + 1, parent.Pointers, brother);
                }
                else
                {
                    Node uncle = SplitNode(parent);
                    InsertInParent(parent, uncle.Keys[0], uncle);
                }
            }
        }

        public void Update(string value)
        {
            //TO DO
            //should use Remove then Add
        }

        public void Remove(string value)
        {
            //TO DO
            Tuple<Node, int> searchResult = Search(value);
            Node target = searchResult.Item1;
            int index = searchResult.Item2;
            Pointer targetPointer = target.RecordPointers[index];
            //RemoveEntry(target, value, targetPointer);
        }

        //needs to be recursive and also take a Node as the 3rd argument
        //private void RemoveEntry(Node target, string value, Pointer targetPointer)
        //{
        //    target.Keys.Remove(value);
        //    target.RecordPointers.Remove(targetPointer);
        //    if (target.IsRoot() && target.Pointers.Count == 1)
        //    {
        //        this.Root = target.Pointers[0];
        //        target.Pointers[0].Parent = null;
        //    }
        //    else if (target.Keys.Count < target.MinKeys)
        //    {
        //        int indexOfTargetInParent = target.Parent.Pointers.IndexOf(target);
        //        int indexOfBrotherInParent;
        //        string valueBetween = "";
        //        Node brother = new Node();
        //        if (indexOfTargetInParent > 0)
        //        {
        //            brother = target.Parent.Pointers[indexOfTargetInParent - 1];
        //            valueBetween = target.Parent.Keys[indexOfTargetInParent - 1];
        //        }
        //        else if (indexOfTargetInParent < target.Parent.MaxPointers)
        //        {
        //            brother = target.Parent.Pointers[indexOfTargetInParent + 1];
        //            valueBetween = target.Parent.Keys[indexOfTargetInParent + 1];
        //        }
        //        indexOfBrotherInParent = brother.Parent.Pointers.IndexOf(brother);
        //        if (target.Keys.Count + brother.Keys.Count <= target.MaxKeys)
        //        {
        //            //if the keys from both nodes fit into 1
        //            //coalesce the nodes
        //            //TO DO 
        //            if (indexOfTargetInParent < indexOfBrotherInParent)
        //            {
        //                Node temp = new Node(target);
        //                target = new Node(brother);
        //                brother = new Node(temp);
        //                if (!target.IsLeaf())
        //                {
        //                    brother.Keys.AddRange(target.Keys);
        //                    brother.Pointers.AddRange(target.Pointers);
        //                }
        //                else
        //                {
        //                    brother.Keys.AddRange(target.Keys);
        //                    brother.RecordPointers.AddRange(target.RecordPointers);
        //                    brother.NextNode = target.NextNode;
        //                    //TO ADD after updating the function arguments
        //                    //RemoveEntry(target.Parent, valueBetween, target);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (indexOfBrotherInParent < indexOfTargetInParent)
        //            {
        //                if (!target.IsLeaf())
        //                {
        //                    Node lastChildOfBrother = brother.Pointers.Last();
        //                    target.Keys.Insert(0, valueBetween);
        //                    target.Pointers.Insert(0, lastChildOfBrother);
        //                    target.Parent.Keys[target.Parent.Keys.IndexOf(valueBetween)] = brother.Keys.Last();
        //                    brother.Keys.RemoveAt(brother.Keys.Count - 1);
        //                    brother.Pointers.RemoveAt(brother.Pointers.Count - 1);
        //                }
        //                else
        //                {
        //                    Pointer lastPointerOfBrother = brother.RecordPointers.Last();
        //                    target.Keys.Insert(0, valueBetween);
        //                    target.RecordPointers.Insert(0, lastPointerOfBrother);
        //                    target.Parent.Keys[target.Parent.Keys.IndexOf(valueBetween)] = brother.Keys.Last();
        //                    brother.Keys.RemoveAt(brother.Keys.Count - 1);
        //                    brother.RecordPointers.RemoveAt(brother.RecordPointers.Count - 1);
        //                }
        //            }
        //            else
        //            {
        //                if (!target.IsLeaf())
        //                {
        //                    Node firstChildOfBrother = brother.Pointers.First();
        //                    target.Keys.Add(valueBetween);
        //                    target.Pointers.Add(firstChildOfBrother);
        //                    target.Parent.Keys[target.Parent.Keys.IndexOf(valueBetween)] = brother.Keys.First();
        //                    brother.Keys.RemoveAt(0);
        //                    brother.Pointers.RemoveAt(0);
        //                }
        //                else
        //                {
        //                    Pointer firstPointerOfBrother = brother.RecordPointers.First();
        //                    target.Keys.Add(valueBetween);
        //                    target.RecordPointers.Add(firstPointerOfBrother);
        //                    target.Parent.Keys[target.Parent.Keys.IndexOf(valueBetween)] = brother.Keys.First();
        //                    brother.Keys.RemoveAt(0);
        //                    brother.Keys.RemoveAt(0);
        //                }
        //            }
        //        }

        //    }
        //}
    }
}