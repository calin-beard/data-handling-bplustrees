using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlingBPlusTrees
{
    class BPlusTree<K> where K : IComparable
    {
        private const int DEGREE = 6;

        private static int Degree { get; set; }

        private Node Root { get; set; }

        public BPlusTree() : this(DEGREE) { }

        public BPlusTree(int _degree)
        {
            if (_degree <= 2)
            {
                throw new ArgumentException("The degree of the B+ tree is " + _degree + ".It should be >= 3");
            }
            BPlusTree<K>.Degree = _degree;
            this.Root = new LeafNode(this);
        }

        private InternalNode NewInternalNode()
        {
            return new InternalNode(this);
        }

        private LeafNode NewLeafNode()
        {
            return new LeafNode(this);
        }

        private N NewNode<N>() where N : Node, new()
        {
            N node = new N();
            node.SetTree(this);
            return node;
        }

        public static BPlusTree<K> BuildGroundUp(int degree, SortedDictionary<K, RecordPointer<K>> elements)
        {
            BPlusTree<K> result = new BPlusTree<K>(degree);
            List<LeafNode> leafs = BuildLevel<LeafNode, RecordPointer<K>>(result, elements);

            for (int i = 0; i < leafs.Count - 1; i++)
            {
                leafs.ElementAt(i).Next = leafs.ElementAt(i + 1);
            }

            // build next levels
            Dictionary<K, LeafNode> leafNodes = leafs.ToDictionary(e => e.GetFirstLeafKey(), e => e);
            SortedDictionary<K, LeafNode> sortedLeafNodes = new SortedDictionary<K, LeafNode>(leafNodes);
            SortedDictionary<K, Node> sortedInputNodes = new SortedDictionary<K, Node>(sortedLeafNodes.ToDictionary(e => e.Key, e => (Node)e.Value));
            result.Root = BuildUpperLevel<LeafNode>(result, sortedLeafNodes).ElementAtOrDefault(0);

            return result;
        }

        private static List<InternalNode> BuildUpperLevel<N>(BPlusTree<K> tree, SortedDictionary<K, N> nodes) where N : Node
        {
            if (nodes.Count < Degree)
            {
                List<InternalNode> result = new List<InternalNode>(1);
                InternalNode root = tree.NewInternalNode();
                root.QuickFill(new Dictionary<K, N>(nodes));
                result.Add(root);
                return result;
            }

            SortedDictionary<K, Node> castNodes = new SortedDictionary<K, Node>(nodes.ToDictionary(e => e.Key, e => (Node)e.Value));
            List<InternalNode> builtNodes = BuildLevel<InternalNode, Node>(tree, castNodes);
            Dictionary<K, InternalNode> internalNodes = builtNodes.ToDictionary(e => e.GetFirstLeafKey(), e => e);
            SortedDictionary<K, InternalNode> sortedInternalNodes = new SortedDictionary<K, InternalNode>(internalNodes);

            List<InternalNode> nextNodes = BuildUpperLevel<InternalNode>(tree, sortedInternalNodes);
            foreach (InternalNode node in builtNodes)
            {
                foreach (InternalNode parent in nextNodes)
                {
                    if (parent.HasChild(node))
                    {
                        node.Parent = parent;
                    }
                }
            }

            return nextNodes;
        }

        private static List<N> BuildLevel<N, P>(BPlusTree<K> tree, SortedDictionary<K, P> elements)
            where N : Node, new()
            where P : class //should always be Node for N=InternalNode or RecordPointer<K> for N=LeafNode
        {
            List<N> results;
            int totalKeyCount = elements.Count;
            int tryKeyCountLeaf = (2 * Degree) / 3;
            Tuple<int, int> keyCountNodeCountLeafs = GetKeyCountNodeCountLevel(tryKeyCountLeaf, totalKeyCount);
            int keyCountLeaf = keyCountNodeCountLeafs.Item1;
            int nodeCountLeafLevel = keyCountNodeCountLeafs.Item2;

            int counter = 0;
            results = new List<N>(nodeCountLeafLevel);
            foreach (Dictionary<K, P> shard in elements.GroupBy(x => counter++ / keyCountLeaf).Select(g => g.ToDictionary(h => h.Key, h => h.Value)))
            {
                if (typeof(N) == typeof(InternalNode))
                {
                    shard.Remove(shard.Keys.First());
                }
                N node = tree.NewNode<N>();
                node.QuickFill(shard);
                results.Add(node);
            }

            return results;
        }

        public RecordPointer<K> Find(K value)
        {
            return Root.GetValue(value);
        }

        public List<RecordPointer<K>> FindRange(K value1, K value2)
        {
            return this.Root.GetRange(value1, value2);
        }

        public void Insert(K value, RecordPointer<K> rp)
        {
            Root.InsertValue(value, rp);
        }
        public void InsertMultiple(Dictionary<K, RecordPointer<K>> elements)
        {
            foreach (KeyValuePair<K, RecordPointer<K>> element in elements)
            {
                this.Insert(element.Key, element.Value);
            }
        }

        public void Delete(K value)
        {
            Root.DeleteValue(value);
        }

        public static Tuple<int, int> GetKeyCountNodeCountLevel(int keyCountNode, int totalKeyCount)
        {
            if (totalKeyCount % keyCountNode >= Node.MinKeys())
            {
                return new Tuple<int, int>(keyCountNode, totalKeyCount / keyCountNode);
            }
            return GetKeyCountNodeCountLevel(keyCountNode + 5, totalKeyCount);
        }

        private abstract class Node : IComparable
        {
            public static BPlusTree<K> BPTree { get; set; }
            public Node Parent { get; set; }
            public List<K> Keys { get; set; }

            public void SetTree(BPlusTree<K> tree)
            {
                Node.BPTree = tree;
            }

            public BPlusTree<K> GetTree()
            {
                return Node.BPTree;
            }

            public abstract void QuickFill<P>(Dictionary<K, P> elements) where P : class;

            public abstract RecordPointer<K> GetValue(K value);

            public abstract List<RecordPointer<K>> GetRange(K value1, K value2);

            public abstract void InsertValue(K value, RecordPointer<K> rp);

            public abstract void DeleteValue(K value);

            public void CreateNewRoot(Node brother)
            {
                InternalNode newRoot = new InternalNode(BPTree);
                newRoot.Keys.Add(brother.GetFirstLeafKey());
                newRoot.Pointers.Add(this);
                newRoot.Pointers.Add(brother);
                this.Parent = newRoot;
                brother.Parent = newRoot;
                BPTree.Root = newRoot;
            }

            public static int BinarySeachFirstIndex<T>(List<T> list, T x) where T : IComparable
            {
                int low = 0;
                int high = list.Count - 1;
                int index = -1;

                while (low <= high)
                {
                    int mid = (low + high) / 2;

                    if (x.CompareTo(list.ElementAt(mid)) < 0)
                    {
                        high = mid - 1;
                    }
                    else if (x.CompareTo(list.ElementAt(mid)) > 0)
                    {
                        low = mid + 1;
                    }
                    else
                    {
                        index = mid;
                        high = mid;
                    }
                }
                return index >= 0 ? index : -low;
            }

            public abstract K GetFirstLeafKey();

            public abstract void Merge(Node brother);

            public abstract Node Split();

            public static int MinKeys()
            {
                return (int)Math.Ceiling((decimal)(BPlusTree<K>.Degree - 1) / 2);
            }
            protected abstract int MaxKeys();

            protected abstract int MinPointers();
            protected abstract int MaxPointers();

            public abstract bool IsUnderflow();

            public abstract bool IsOverflow();

            public bool IsRoot()
            {
                return BPTree.Root == this;
            }

            int IComparable.CompareTo(object obj)
            {
                Node n = (Node)obj;
                return this.Keys[0].CompareTo(n.Keys[0]);
            }
        }

        private class InternalNode : Node
        {
            public List<Node> Pointers { get; set; }

            public InternalNode()
            {
                this.Keys = new List<K>(this.MaxKeys());
                this.Pointers = new List<Node>(this.MaxPointers());
            }

            public InternalNode(BPlusTree<K> _bptree)
            {
                BPTree = _bptree;
                this.Keys = new List<K>(this.MaxKeys());
                this.Pointers = new List<Node>(this.MaxPointers());
            }

            public override void QuickFill<P>(Dictionary<K, P> elements)
            {
                this.Pointers.Add(elements.First().Value as Node);
                if (!elements.Remove(elements.Keys.First()))
                {
                    throw new Exception("--- Cannot remove first key from nodes dictionary");
                }
                foreach (KeyValuePair<K, P> element in elements)
                {
                    this.Keys.Add(element.Key);
                    this.Pointers.Add(element.Value as Node);
                }
            }

            public override RecordPointer<K> GetValue(K value)
            {
                return this.GetChild(value).GetValue(value);
            }

            public override List<RecordPointer<K>> GetRange(K value1, K value2)
            {
                return this.GetChild(value1).GetRange(value1, value2);
            }

            public override void InsertValue(K value, RecordPointer<K> rp)
            {
                Node child = this.GetChild(value);
                child.InsertValue(value, rp);
                if (child.IsOverflow())
                {
                    Node brother = child.Split();
                    brother.Parent = child.Parent;
                    this.InsertChild(brother.GetFirstLeafKey(), brother);
                }
                if (BPTree.Root.IsOverflow())
                {
                    Node brother = this.Split();
                    this.CreateNewRoot(brother);
                }
            }

            public override void DeleteValue(K value)
            {
                Node child = this.GetChild(value);
                child.DeleteValue(value);
                if (child.IsUnderflow())
                {
                    Node childLeftBrother = this.GetChildLeftBrother(value);
                    Node childRightBrother = this.GetChildRightBrother(value);
                    Node left = childLeftBrother != null ? childLeftBrother : child;
                    Node right = childLeftBrother != null ? child : childRightBrother;
                    left.Merge(right);
                    this.DeleteChild(right.GetFirstLeafKey());
                    if (left.IsOverflow())
                    {
                        Node brother = left.Split();
                        this.InsertChild(brother.GetFirstLeafKey(), brother);
                    }
                    if (BPTree.Root.Keys.Count == 0)
                    {
                        BPTree.Root = left;
                        BPTree.Root.Parent = null;
                    }
                }
            }

            public override K GetFirstLeafKey()
            {
                return Pointers.ElementAt(0).GetFirstLeafKey();
            }

            public override void Merge(Node brother)
            {
                InternalNode node = (InternalNode)brother;
                this.Keys.Add(node.GetFirstLeafKey());
                this.Keys.AddRange(node.Keys);
                this.Pointers.AddRange(node.Pointers);
            }

            public override Node Split()
            {
                InternalNode brother = new InternalNode(BPTree);
                int from = this.Keys.Count / 2 + 1;
                int count = this.Keys.Count - from;
                brother.Keys.AddRange(this.Keys.GetRange(from, count));
                brother.Pointers.AddRange(this.Pointers.GetRange(from, count + 1));
                foreach (BPlusTree<K>.Node child in brother.Pointers)
                {
                    child.Parent = brother;
                }

                this.Keys.RemoveRange(from, count);
                this.Pointers.RemoveRange(from, count + 1);
                return brother;
            }

            protected override int MaxKeys()
            {
                return BPlusTree<K>.Degree - 1;
            }

            protected override int MinPointers()
            {
                return (int)Math.Ceiling((decimal)BPlusTree<K>.Degree / 2);
            }

            protected override int MaxPointers()
            {
                return BPlusTree<K>.Degree;
            }

            public override bool IsUnderflow()
            {
                return this.Pointers.Count < this.MinPointers();
            }

            public override bool IsOverflow()
            {
                return this.Pointers.Count > this.MaxPointers();
            }

            protected Node GetChild(K value)
            {
                int where = this.Keys.BinarySearch(value);
                int childIndex = where >= 0 ? where + 1 : -where - 1;
                return this.Pointers.ElementAt(childIndex);
            }

            public bool HasChild(Node n)
            {
                int where = this.Pointers.BinarySearch(n);
                return where >= 0;
            }

            protected void InsertChild(K value, Node child)
            {
                int where = this.Keys.BinarySearch(value);
                int childIndex = where >= 0 ? where + 1 : -where - 1;
                if (where >= 0)
                {
                    this.Pointers.Insert(childIndex, child);
                }
                else
                {
                    this.Keys.Insert(childIndex, value);
                    this.Pointers.Insert(childIndex + 1, child);
                }
            }

            protected void DeleteChild(K value)
            {
                int where = this.Keys.BinarySearch(value);
                if (where >= 0)
                {
                    this.Keys.RemoveAt(where);
                    this.Pointers.RemoveAt(where + 1);
                }
                else
                {
                    this.Pointers.Remove(this.GetChild(value));
                }
            }

            public Node GetChildLeftBrother(K value)
            {
                int where = this.Keys.BinarySearch(value);
                int childIndex = where >= 0 ? where + 1 : -where - 1;
                if (childIndex > 0)
                {
                    return this.Pointers.ElementAtOrDefault(childIndex - 1);
                }
                return null;
            }

            public Node GetChildRightBrother(K value)
            {
                int where = this.Keys.BinarySearch(value);
                int childIndex = where >= 0 ? where + 1 : -where - 1;
                if (childIndex < this.Keys.Capacity)
                {
                    return this.Pointers.ElementAtOrDefault(childIndex + 1);
                }
                return null;
            }

            public override string ToString()
            {
                return (this.IsRoot() ? "Root " : "") + "InternalNode with first key " + this.Keys.ElementAt(0).ToString(); ;
            }
        }

        private class LeafNode : Node
        {
            List<RecordPointer<K>> RecordPointers { get; set; }

            public LeafNode Next { get; set; }

            public LeafNode()
            {
                this.Keys = new List<K>(this.MaxKeys());
                this.RecordPointers = new List<RecordPointer<K>>(this.MaxPointers());
            }

            public LeafNode(BPlusTree<K> _bptree)
            {
                BPTree = _bptree;
                this.Keys = new List<K>(this.MaxKeys());
                this.RecordPointers = new List<RecordPointer<K>>(this.MaxPointers());
            }

            public override void QuickFill<P>(Dictionary<K, P> elements)
            {
                foreach (KeyValuePair<K, P> element in elements)
                {
                    this.Keys.Add(element.Key);
                    this.RecordPointers.Add(element.Value as RecordPointer<K>);
                }
            }

            public override RecordPointer<K> GetValue(K value)
            {
                int where = this.Keys.BinarySearch(value);
                if (where >= 0)
                {
                    return this.RecordPointers.ElementAt(where);
                }
                return null;
            }

            public override List<RecordPointer<K>> GetRange(K value1, K value2)
            {
                List<RecordPointer<K>> results = new List<RecordPointer<K>>();
                LeafNode node = this;
                while (node != null)
                {
                    IEnumerator<K> valueEnumerator = node.Keys.GetEnumerator();
                    IEnumerator<RecordPointer<K>> rpEnumerator = node.RecordPointers.GetEnumerator();
                    while (valueEnumerator.MoveNext() && rpEnumerator.MoveNext())
                    {
                        K value = valueEnumerator.Current;
                        RecordPointer<K> rp = rpEnumerator.Current;
                        int c1 = value.CompareTo(value1);
                        int c2 = value.CompareTo(value2);
                        if (c1 >= 0 && c2 <= 0)
                        {
                            results.Add(rp);
                        }
                        else if (c2 > 0)
                        {
                            return results;
                        }
                    }
                    node = node.Next;
                }
                return results;
            }

            public override void InsertValue(K value, RecordPointer<K> rp)
            {
                int where = this.Keys.BinarySearch(value);
                int valueIndex = where >= 0 ? where : -where - 1;
                if (where >= 0)
                {
                    this.RecordPointers[valueIndex] = rp;
                }
                else
                {
                    this.Keys.Insert(valueIndex, value);
                    this.RecordPointers.Insert(valueIndex, rp);
                }
                if (BPTree.Root.IsOverflow())
                {
                    Node brother = this.Split();
                    this.CreateNewRoot(brother);
                }
            }

            public override void DeleteValue(K value)
            {
                int where = this.Keys.BinarySearch(value);
                if (where >= 0)
                {
                    this.Keys.RemoveAt(where);
                    this.RecordPointers.RemoveAt(where);
                }
            }

            public override K GetFirstLeafKey()
            {
                return this.Keys.ElementAt(0);
            }

            public override void Merge(Node brother)
            {
                LeafNode node = (LeafNode)brother;
                this.Keys.AddRange(node.Keys);
                this.RecordPointers.AddRange(node.RecordPointers);
                this.Next = node.Next;
            }

            public override Node Split()
            {
                LeafNode brother = new LeafNode(BPTree);
                brother.Parent = this.Parent;
                int from = (this.Keys.Count + 1) / 2;
                int count = this.Keys.Count - from;
                brother.Keys.AddRange(this.Keys.GetRange(from, count));
                brother.RecordPointers.AddRange(this.RecordPointers.GetRange(from, count));

                this.Keys.RemoveRange(from, count);
                this.RecordPointers.RemoveRange(from, count);

                brother.Next = this.Next;
                this.Next = brother;

                return brother;
            }

            protected override int MaxKeys()
            {
                return BPlusTree<K>.Degree - 1;
            }

            protected override int MinPointers()
            {
                return (int)Math.Ceiling((decimal)(BPlusTree<K>.Degree - 1) / 2);
            }

            protected override int MaxPointers()
            {
                return BPlusTree<K>.Degree - 1;
            }

            public override bool IsUnderflow()
            {
                return this.RecordPointers.Count < this.MinPointers();
            }

            public override bool IsOverflow()
            {
                return this.RecordPointers.Count > this.MaxPointers();
            }

            public override string ToString()
            {
                return (this.IsRoot() ? "Root " : "") + "LeafNode with first key " + this.Keys.ElementAt(0).ToString(); ;
            }
        }
    }
}