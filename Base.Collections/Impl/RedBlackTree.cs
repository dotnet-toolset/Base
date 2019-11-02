using System;
using System.Collections.Generic;

namespace Base.Collections.Impl
{
    public class RedBlackTree<T> : IRedBlackTree<T>
    {
        protected enum Color
        {
            Red,
            Black
        };

        // The base Node class which is stored in the tree. Nodes are only
        // an internal concept; users of the tree deal only with the data
        // they store in it.
        protected class Node
        {
            internal Node LeftNode;
            internal Node RightNode;
            internal Node ParentNode;
            internal Color NodeColor;
            internal T NodeValue;

            public Node Left => LeftNode;
            public Node Right => RightNode;
            public Node Parent => ParentNode;

            public Color Color => NodeColor;

            public T Value => NodeValue;

            // Constructor. Newly-created nodes are colored red.
            public Node(T data)
            {
                NodeValue = data;
            }

            // Copies all user-level fields from the source node, but not
            // internal fields. For example, the base implementation of this
            // method copies the "m_data" field, but not the child or parent
            // fields. Any augmentation information also does not need to be
            // copied, as it will be recomputed. Subclasses must call the
            // superclass implementation.
            public virtual void CopyFrom(Node src) { NodeValue = src.NodeValue; }

        };

        private readonly Comparison<T> _comparison;
        private Node _root;

        public bool NeedsFullOrderingComparisons;

        // Returns the root of the tree, which is needed by some subclasses.
        protected Node Root => _root;

        public int Count
        {
            get
            {
                var counter = 0;
                VisitInorder(n => ++counter);
                return counter;
            }
        }

        private static int GenericCompare(T a, T b)
        {
            if (ReferenceEquals(a, b)) return 0;
            if (a != null) return ((IComparable<T>)a).CompareTo(b);
            if (b != null) return -((IComparable<T>)b).CompareTo(a);
            throw new NotSupportedException();
        }

        private static int ObjectCompare(T a, T b)
        {
            if (ReferenceEquals(a, b)) return 0;
            if (a != null) return ((IComparable)a).CompareTo(b);
            if (b != null) return -((IComparable)b).CompareTo(a);
            throw new NotSupportedException();
        }

        public RedBlackTree()
        {
            var t = typeof(T);
            if (typeof(IComparable<T>).IsAssignableFrom(t))
                _comparison = GenericCompare;
            else if (typeof(IComparable).IsAssignableFrom(t))
                _comparison = ObjectCompare;
            else throw new NotSupportedException("either use constructor with comparer or provide comparable type");
        }

        public RedBlackTree(IComparer<T> comparer)
        {
            _comparison = comparer.Compare;
        }

        public RedBlackTree(Comparison<T> comparison)
        {
            _comparison = comparison;
        }

        // Clearing will delete the contents of the tree. After this call
        // isInitialized will return false.
        public void Clear()
        {
            MarkFree(_root);
            _root = null;
        }

        public void Add(T data)
        {
            var node = new Node(data);
            InsertNode(node);
        }

        // Returns true if the datum was found in the tree.
        public bool Remove(T data)
        {
            var node = TreeSearch(data);
            if (node != null)
            {
                DeleteNode(node);
                return true;
            }
            return false;
        }

        public bool Contains(T data)
        {
            return TreeSearch(data) != null;
        }

        public void VisitInorder(Action<T> visitor)
        {
            if (_root == null)
                return;
            VisitInorderImpl(_root, visitor);
        }


        public virtual bool CheckInvariants()
        {
            var blackCount = 0;
            return CheckInvariantsFromNode(_root, ref blackCount);
        }



        // This virtual method is the hook that subclasses should use when
        // augmenting the red-black tree with additional per-node summary
        // information. For example, in the case of an interval tree, this
        // is used to compute the maximum endpoint of the subtree below the
        // given node based on the values in the left and right children. It
        // is guaranteed that this will be called in the correct order to
        // properly update such summary information based only on the values
        // in the left and right children. This method should return true if
        // the node's summary information changed.
        protected virtual bool UpdateNode(Node node) { return false; }

        // Searches the tree for the given datum.
        Node TreeSearch(T data)
        {
            if (NeedsFullOrderingComparisons)
                return TreeSearchFullComparisons(_root, data);

            return TreeSearchNormal(_root, data);
        }


        // Searches the tree using the normal comparison operations,
        // suitable for simple data types such as numbers.
        Node TreeSearchNormal(Node current, T data)
        {
            while (current != null)
            {
                var c = _comparison(current.NodeValue, data);
                if (c == 0/*current.data() == data*/)
                    return current;
                if (c > 0/*data < current.data()*/)
                    current = current.LeftNode;
                else
                    current = current.RightNode;
            }
            return null;
        }

        // Searches the tree using multiple comparison operations, required
        // for data types with more complex behavior such as intervals.
        Node TreeSearchFullComparisons(Node current, T data)
        {
            if (current == null)
                return null;
            var c = _comparison(current.NodeValue, data);
            if (c > 0/*data < current.data()*/)
                return TreeSearchFullComparisons(current.LeftNode, data);
            if (c < 0/*current.data() < data*/)
                return TreeSearchFullComparisons(current.RightNode, data);
            if (c == 0/*data == current.data()*/)
                return current;

            // We may need to traverse both the left and right subtrees.
            return TreeSearchFullComparisons(current.LeftNode, data) ?? TreeSearchFullComparisons(current.RightNode, data);
        }

        void TreeInsert(Node z)
        {
            Node y = null;
            Node x = _root;
            while (x != null)
            {
                y = x;
                x = _comparison(z.NodeValue, x.NodeValue) < 0 ? x.LeftNode : x.RightNode;
            }
            z.ParentNode = y;
            if (y == null)
                _root = z;
            else
            {
                if (_comparison(z.NodeValue, y.NodeValue) < 0)
                    y.LeftNode = z;
                else
                    y.RightNode = z;
            }
        }

        // Finds the node following the given one in sequential ordering of
        // their data, or null if none exists.
        Node TreeSuccessor(Node x)
        {
            if (x.RightNode != null)
                return TreeMinimum(x.RightNode);
            var y = x.ParentNode;
            while (y != null && x == y.RightNode)
            {
                x = y;
                y = y.ParentNode;
            }
            return y;
        }

        // Finds the minimum element in the sub-tree rooted at the given
        // node.
        Node TreeMinimum(Node x)
        {
            while (x.LeftNode != null)
                x = x.LeftNode;
            return x;
        }

        // Helper for maintaining the augmented red-black tree.
        void PropagateUpdates(Node start)
        {
            var  shouldContinue = true;
            while (start != null && shouldContinue)
            {
                shouldContinue = UpdateNode(start);
                start = start.ParentNode;
            }
        }

        //----------------------------------------------------------------------
        // Red-Black tree operations
        //

        // Left-rotates the subtree rooted at x.
        // Returns the new root of the subtree (x's right child).
        Node LeftRotate(Node x)
        {
            // Set y.
            var y = x.RightNode;

            // Turn y's left subtree into x's right subtree.
            x.RightNode = y.LeftNode;
            if (y.LeftNode != null)
                y.LeftNode.ParentNode = x;

            // Link x's parent to y.
            y.ParentNode = x.ParentNode;
            if (x.ParentNode == null)
                _root = y;
            else
            {
                if (x == x.ParentNode.LeftNode)
                    x.ParentNode.LeftNode = y;
                else
                    x.ParentNode.RightNode = y;
            }

            // Put x on y's left.
            y.LeftNode = x;
            x.ParentNode = y;

            // Update nodes lowest to highest.
            UpdateNode(x);
            UpdateNode(y);
            return y;
        }

        // Right-rotates the subtree rooted at y.
        // Returns the new root of the subtree (y's left child).
        Node RightRotate(Node y)
        {
            // Set x.
            var x = y.LeftNode;

            // Turn x's right subtree into y's left subtree.
            y.LeftNode = x.RightNode;
            if (x.RightNode != null)
                x.RightNode.ParentNode = y;

            // Link y's parent to x.
            x.ParentNode = y.ParentNode;
            if (y.ParentNode == null)
                _root = x;
            else
            {
                if (y == y.ParentNode.LeftNode)
                    y.ParentNode.LeftNode = x;
                else
                    y.ParentNode.RightNode = x;
            }

            // Put y on x's right.
            x.RightNode = y;
            y.ParentNode = x;

            // Update nodes lowest to highest.
            UpdateNode(y);
            UpdateNode(x);
            return x;
        }

        // Inserts the given node into the tree.
        void InsertNode(Node x)
        {
            TreeInsert(x);
            x.NodeColor = Color.Red;
            UpdateNode(x);

            // The node from which to start propagating updates upwards.
            var updateStart = x.ParentNode;

            while (x != _root && x.ParentNode.NodeColor == Color.Red)
            {
                if (x.ParentNode == x.ParentNode.ParentNode.LeftNode)
                {
                    var y = x.ParentNode.ParentNode.RightNode;
                    if (y != null && y.NodeColor == Color.Red)
                    {
                        // Case 1
                        //logIfVerbose("  Case 1/1");
                        x.ParentNode.NodeColor = Color.Black;
                        y.NodeColor = Color.Black;
                        x.ParentNode.ParentNode.NodeColor = Color.Red;
                        UpdateNode(x.ParentNode);
                        x = x.ParentNode.ParentNode;
                        UpdateNode(x);
                        updateStart = x.ParentNode;
                    }
                    else
                    {
                        if (x == x.ParentNode.RightNode)
                        {
                            //logIfVerbose("  Case 1/2");
                            // Case 2
                            x = x.ParentNode;
                            LeftRotate(x);
                        }
                        // Case 3
                        //logIfVerbose("  Case 1/3");
                        x.ParentNode.NodeColor = Color.Black;
                        x.ParentNode.ParentNode.NodeColor = Color.Red;
                        var newSubTreeRoot = RightRotate(x.ParentNode.ParentNode);
                        updateStart = newSubTreeRoot.ParentNode;
                    }
                }
                else
                {
                    // Same as "then" clause with "right" and "left" exchanged.
                    var y = x.ParentNode.ParentNode.LeftNode;
                    if (y != null && y.NodeColor == Color.Red)
                    {
                        // Case 1
                        //logIfVerbose("  Case 2/1");
                        x.ParentNode.NodeColor = Color.Black;
                        y.NodeColor = Color.Black;
                        x.ParentNode.ParentNode.NodeColor = Color.Red;
                        UpdateNode(x.ParentNode);
                        x = x.ParentNode.ParentNode;
                        UpdateNode(x);
                        updateStart = x.ParentNode;
                    }
                    else
                    {
                        if (x == x.ParentNode.LeftNode)
                        {
                            // Case 2
                            //logIfVerbose("  Case 2/2");
                            x = x.ParentNode;
                            RightRotate(x);
                        }
                        // Case 3
                        //logIfVerbose("  Case 2/3");
                        x.ParentNode.NodeColor = Color.Black;
                        x.ParentNode.ParentNode.NodeColor = Color.Red;
                        var newSubTreeRoot = LeftRotate(x.ParentNode.ParentNode);
                        updateStart = newSubTreeRoot.ParentNode;
                    }
                }
            }

            PropagateUpdates(updateStart);

            _root.NodeColor = Color.Black;
        }

        // Restores the red-black property to the tree after splicing out
        // a node. Note that x may be null, which is why xParent must be
        // supplied.
        void DeleteFixup(Node x, Node xParent)
        {
            while (x != _root && (x == null || x.NodeColor == Color.Black))
            {
                if (x == xParent.LeftNode)
                {
                    // Note: the text points out that w can not be null.
                    // The reason is not obvious from simply looking at
                    // the code; it comes about from the properties of the
                    // red-black tree.
                    var w = xParent.RightNode;
                    //ASSERT(w); // x's sibling should not be null.
                    if (w.NodeColor == Color.Red)
                    {
                        // Case 1
                        w.NodeColor = Color.Black;
                        xParent.NodeColor = Color.Red;
                        LeftRotate(xParent);
                        w = xParent.RightNode;
                    }
                    if ((w.LeftNode == null || w.LeftNode.NodeColor == Color.Black)
                        && (w.RightNode == null || w.RightNode.NodeColor == Color.Black))
                    {
                        // Case 2
                        w.NodeColor = Color.Red;
                        x = xParent;
                        xParent = x.ParentNode;
                    }
                    else
                    {
                        if (w.RightNode == null || w.RightNode.NodeColor == Color.Black)
                        {
                            // Case 3
                            w.LeftNode.NodeColor = Color.Black;
                            w.NodeColor = Color.Red;
                            RightRotate(w);
                            w = xParent.RightNode;
                        }
                        // Case 4
                        w.NodeColor = xParent.NodeColor;
                        xParent.NodeColor = Color.Black;
                        if (w.RightNode != null)
                        {
                            w.RightNode.NodeColor = Color.Black;
                        }
                        LeftRotate(xParent);
                        x = _root;
                        xParent = x.ParentNode;
                    }
                }
                else
                {
                    // Same as "then" clause with "right" and "left"
                    // exchanged.

                    // Note: the text points out that w can not be null.
                    // The reason is not obvious from simply looking at
                    // the code; it comes about from the properties of the
                    // red-black tree.
                    var w = xParent.LeftNode;
                    //ASSERT(w); // x's sibling should not be null.
                    if (w.NodeColor == Color.Red)
                    {
                        // Case 1
                        w.NodeColor = Color.Black;
                        xParent.NodeColor = Color.Red;
                        RightRotate(xParent);
                        w = xParent.LeftNode;
                    }
                    if ((w.RightNode == null || w.RightNode.NodeColor == Color.Black)
                        && (w.LeftNode == null || w.LeftNode.NodeColor == Color.Black))
                    {
                        // Case 2
                        w.NodeColor = Color.Red;
                        x = xParent;
                        xParent = x.ParentNode;
                    }
                    else
                    {
                        if (w.LeftNode == null || w.LeftNode.NodeColor == Color.Black)
                        {
                            // Case 3
                            w.RightNode.NodeColor = Color.Black;
                            w.NodeColor = Color.Red;
                            LeftRotate(w);
                            w = xParent.LeftNode;
                        }
                        // Case 4
                        w.NodeColor = xParent.NodeColor;
                        xParent.NodeColor = Color.Black;
                        if (w.LeftNode != null)
                        {
                            w.LeftNode.NodeColor = Color.Black;
                        }
                        RightRotate(xParent);
                        x = _root;
                        xParent = x.ParentNode;
                    }
                }
            }
            if (x != null)
            {
                x.NodeColor = Color.Black;
            }
        }

        // Deletes the given node from the tree. Note that this
        // particular node may not actually be removed from the tree;
        // instead, another node might be removed and its contents
        // copied into z.
        void DeleteNode(Node z)
        {
            // Y is the node to be unlinked from the tree.
            Node y;
            if (z.LeftNode == null || z.RightNode == null)
                y = z;
            else
                y = TreeSuccessor(z);

            // Y is guaranteed to be non-null at this point.
            Node x;
            if (y.LeftNode != null)
                x = y.LeftNode;
            else
                x = y.RightNode;

            // X is the child of y which might potentially replace y in
            // the tree. X might be null at this point.
            Node xParent;
            if (x != null)
            {
                x.ParentNode = y.ParentNode;
                xParent = x.ParentNode;
            }
            else
                xParent = y.ParentNode;
            if (y.ParentNode == null)
                _root = x;
            else
            {
                if (y == y.ParentNode.LeftNode)
                {
                    y.ParentNode.LeftNode = x;
                }
                else
                {
                    y.ParentNode.RightNode = x;
                }
            }
            if (y != z)
            {
                z.CopyFrom(y);
                // This node has changed location in the tree and must be updated.
                UpdateNode(z);
                // The parent and its parents may now be out of date.
                PropagateUpdates(z.ParentNode);
            }

            // If we haven't already updated starting from xParent, do so now.
            if (xParent != null && xParent != y && xParent != z)
                PropagateUpdates(xParent);
            if (y.NodeColor == Color.Black)
                DeleteFixup(x, xParent);

            //   m_arena.freeObject(y);
        }

        // Visits the subtree rooted at the given node in order.
        void VisitInorderImpl(Node node, Action<T> visitor)
        {
            if (node.LeftNode != null)
                VisitInorderImpl(node.LeftNode, visitor);
            visitor(node.NodeValue);
            if (node.RightNode != null)
                VisitInorderImpl(node.RightNode, visitor);
        }

        void MarkFree(Node node)
        {
            if (node == null)
                return;

            if (node.LeftNode != null)
                MarkFree(node.LeftNode);
            if (node.RightNode != null)
                MarkFree(node.RightNode);
        }

        // Returns in the "blackCount" parameter the number of black
        // children along all paths to all leaves of the given node.
        bool CheckInvariantsFromNode(Node node, ref int blackCount)
        {
            // Base case is a leaf node.
            if (node == null)
            {
                blackCount = 1;
                return true;
            }

            // Each node is either red or black.
            if (!(node.NodeColor == Color.Red || node.NodeColor == Color.Black))
                return false;

            // Every leaf (or null) is black.

            if (node.NodeColor == Color.Red)
            {
                // Both of its children are black.
                if (!((node.LeftNode == null || node.LeftNode.NodeColor == Color.Black)))
                    return false;
                if (!((node.RightNode == null || node.RightNode.NodeColor == Color.Black)))
                    return false;
            }

            // Every simple path to a leaf node contains the same number of
            // black nodes.
            int leftCount = 0, rightCount = 0;
            var leftValid = CheckInvariantsFromNode(node.LeftNode, ref leftCount);
            var rightValid = CheckInvariantsFromNode(node.RightNode, ref rightCount);
            if (!leftValid || !rightValid)
                return false;
            blackCount = leftCount + (node.NodeColor == Color.Black ? 1 : 0);
            return leftCount == rightCount;
        }

    }
}
