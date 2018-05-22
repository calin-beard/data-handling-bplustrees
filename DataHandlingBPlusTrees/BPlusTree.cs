using System;
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
            while (!currentNode.isLeaf())
            {
                for (int i = 0; i < currentNode.Keys.Count; i++)
                {
                    if (value.CompareTo(currentNode.Keys[i]) <= 0)
                    {
                        index = i;
                    }
                }

                if (index < 0)
                {
                    currentNode = currentNode.Children.Last();
                }
                else if (value.CompareTo(currentNode.Keys[index]) == 0)
                {
                    currentNode = currentNode.Children[index + 1];
                }
                else
                {
                    currentNode = currentNode.Children[index];
                }
            }

            if (currentNode.isLeaf())
            {
                for (int i = 0; i < currentNode.Keys.Count; i++)
                {
                    if (value.CompareTo(currentNode.Keys[i]) == 0)
                    {
                        return new Tuple<Node, int>(currentNode, i);
                    }
                    else if (value.CompareTo(currentNode.Keys[i]) < 0)
                    {
                        return new Tuple<Node, int>(currentNode, i);
                    }
                    if (i == currentNode.Keys.Count - 1)
                    {
                        return new Tuple<Node, int>(currentNode, i + 1);
                    }
                }
            }
            return null;
        }

        private Node SplitNode(Node target)
        {
            Node brother = new Node(target.Parent, target.isLeaf());
            //add keys and records to newTarget, starting with the key Ceiling(degree/2)
            //the pointers and keys in the documentation start at 1
            //here, 0 based list index is used
            for (int i = target.SecondHalfFirstIndex; i < target.Keys.Count; i++)
            {
                //add key to brother
                brother.Keys.Add(target.Keys[i]);
                //remove key from target
                target.Keys.RemoveAt(i);

                //add record to brother
                brother.Records.Add(target.Records[i]);
                //remove record from target
                target.Records.RemoveAt(i);
            }
            //if the target is a leaf
            //it is needed to set the correct linked-list links
            if (target.isLeaf())
            {
                brother.NextNode = target.NextNode;
                target.NextNode = brother;
            }

            return brother;
        }

        private void CoalesceNodes(Node target, Node brother)
        {
            if (target.Parent.Children.IndexOf(target) < target.Parent.Children.IndexOf(brother))
            {
                //TO DO
            }
        }

        public void AddMultiple(Dictionary<string, Record> searchKeys)
        {
            foreach(KeyValuePair<string, Record> searchKey in searchKeys)
            {
                this.Add(searchKey.Key, searchKey.Value);
            }
        }

        public void Add(string value, Record pointer)
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

                if (target.Keys.Count < target.MaxKeys)
                {
                    AddToLeaf(target, index, value, pointer);
                }
                else
                {
                    AddToLeaf(target, index, value, pointer);
                    Node newTarget = SplitNode(target);
                    AddToParent(target, newTarget.Keys[0], newTarget);
                }
            }
        }

        private void AddToLeaf(Node target, int index, string value, Record pointer)
        {
            target.Keys.Insert(index, value);
            target.Records.Insert(index, pointer);
        }

        private void AddToParent(Node which, string value, Node brother)
        {
            if (which.isRoot())
            {
                this.Root = new Node(null, value, which, brother);
                which.Parent = this.Root;
                brother.Parent = this.Root;
            }
            else
            {
                Node parent = which.Parent;
                if (parent.Children.Count < parent.MaxPointers - 1)
                {
                    int whichIndex = parent.Children.IndexOf(which);
                    parent.Keys.Insert(whichIndex, value);
                    parent.Children.Insert(whichIndex + 1, brother);
                }
                else
                {
                    Node uncle = SplitNode(parent);
                    AddToParent(parent, uncle.Keys[0], uncle);
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
            Record targetRecord = target.Records[index];
            RemoveEntry(target, value, targetRecord);
        }

        private void RemoveEntry(Node target, string value, Record targetRecord)
        {
            target.Keys.Remove(value);
            target.Records.Remove(targetRecord);
            if (target.isRoot() && target.Children.Count == 1)
            {
                this.Root = target.Children[0];
                target.Children[0].Parent = null;
            }
            else if (target.Keys.Count < target.MinKeys)
            {
                int indexOfTargetInParent = target.Parent.Children.IndexOf(target);
                Node brother = new Node();
                if (indexOfTargetInParent > 0)
                {
                    brother = target.Parent.Children[indexOfTargetInParent - 1];
                    string valueBetween = target.Parent.Keys[indexOfTargetInParent - 1];
                }
                else if (indexOfTargetInParent < target.Parent.MaxPointers)
                {
                    brother = target.Parent.Children[indexOfTargetInParent + 1];
                    string valueBetween = target.Parent.Keys[indexOfTargetInParent + 1];
                }
                if (target.Keys.Count + brother.Keys.Count <= target.MaxKeys)
                {
                    //if the keys from both nodes fit into 1
                    //coalesce the nodes
                    //TO DO 
                }
                else
                {

                }
            }

        }
    }
}
