using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Graphs;


/* Adapted from https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp

The MIT License (MIT)

Copyright (c) 2013 Daniel "BlueRaja" Pflughoeft

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.*/

public interface IPriorityQueue<TItem, in TPriority> : IEnumerable<TItem>
    where TPriority : IComparable<TPriority>
{
    void Enqueue(TItem node, TPriority priority);
    TItem Dequeue();
    void Clear();
    bool Contains(TItem node);
    void Remove(TItem node);
    void UpdatePriority(TItem node, TPriority priority);
    TItem First { get; }
    int Count { get; }
}

internal interface IFixedSizePriorityQueue<TItem, in TPriority> : IPriorityQueue<TItem, TPriority>
    where TPriority : IComparable<TPriority>
{
    void Resize(int maxNodes);
    int MaxSize { get; }
    void ResetNode(TItem node);
}

public class GenericPriorityQueueNode<TPriority>
{
    public TPriority Priority { get; protected internal set; }
    public int QueueIndex { get; internal set; }
    public long InsertionIndex { get; internal set; }
}

public sealed class GenericPriorityQueue<TItem, TPriority> : IFixedSizePriorityQueue<TItem, TPriority>
    where TItem : GenericPriorityQueueNode<TPriority>
    where TPriority : IComparable<TPriority>
{
    private int _numNodes;
    private TItem[] _nodes;
    private long _numNodesEverEnqueued;
    private readonly Comparison<TPriority> _comparer;
    
    public GenericPriorityQueue(int maxNodes) : this(maxNodes, Comparer<TPriority>.Default) { }
    
    public GenericPriorityQueue(int maxNodes, IComparer<TPriority> comparer) : this(maxNodes, comparer.Compare) { }
    
    public GenericPriorityQueue(int maxNodes, Comparison<TPriority> comparer)
    {
        _numNodes = 0;
        _nodes = new TItem[maxNodes + 1];
        _numNodesEverEnqueued = 0;
        _comparer = comparer;
    }
    
    public int Count
    {
        get
        {
            return _numNodes;
        }
    }
    
    public int MaxSize
    {
        get
        {
            return _nodes.Length - 1;
        }
    }

    public void Clear()
    {
        Array.Clear(_nodes, 1, _numNodes);
        _numNodes = 0;
    }
    
    public bool Contains(TItem node)
    {
        return (_nodes[node.QueueIndex] == node);
    }
    
    public void Enqueue(TItem node, TPriority priority)
    {
        node.Priority = priority;
        _numNodes++;
        _nodes[_numNodes] = node;
        node.QueueIndex = _numNodes;
        node.InsertionIndex = _numNodesEverEnqueued++;
        CascadeUp(node);
    }
    
    private void CascadeUp(TItem node)
    {
        //aka Heapify-up
        int parent;
        if (node.QueueIndex > 1)
        {
            parent = node.QueueIndex >> 1;
            TItem parentNode = _nodes[parent];
            if(HasHigherPriority(parentNode, node))
                return;

            //Node has lower priority value, so move parent down the heap to make room
            _nodes[node.QueueIndex] = parentNode;
            parentNode.QueueIndex = node.QueueIndex;

            node.QueueIndex = parent;
        }
        else
        {
            return;
        }
        while(parent > 1)
        {
            parent >>= 1;
            TItem parentNode = _nodes[parent];
            if(HasHigherPriority(parentNode, node))
                break;

            //Node has lower priority value, so move parent down the heap to make room
            _nodes[node.QueueIndex] = parentNode;
            parentNode.QueueIndex = node.QueueIndex;

            node.QueueIndex = parent;
        }
        _nodes[node.QueueIndex] = node;
    }
    
    private void CascadeDown(TItem node)
    {
        //aka Heapify-down
        int finalQueueIndex = node.QueueIndex;
        int childLeftIndex = 2 * finalQueueIndex;

        // If leaf node, we're done
        if(childLeftIndex > _numNodes)
        {
            return;
        }

        // Check if the left-child is higher-priority than the current node
        int childRightIndex = childLeftIndex + 1;
        TItem childLeft = _nodes[childLeftIndex];
        if(HasHigherPriority(childLeft, node))
        {
            // Check if there is a right child. If not, swap and finish.
            if(childRightIndex > _numNodes)
            {
                node.QueueIndex = childLeftIndex;
                childLeft.QueueIndex = finalQueueIndex;
                _nodes[finalQueueIndex] = childLeft;
                _nodes[childLeftIndex] = node;
                return;
            }
            // Check if the left-child is higher-priority than the right-child
            TItem childRight = _nodes[childRightIndex];
            if(HasHigherPriority(childLeft, childRight))
            {
                // left is highest, move it up and continue
                childLeft.QueueIndex = finalQueueIndex;
                _nodes[finalQueueIndex] = childLeft;
                finalQueueIndex = childLeftIndex;
            }
            else
            {
                // right is even higher, move it up and continue
                childRight.QueueIndex = finalQueueIndex;
                _nodes[finalQueueIndex] = childRight;
                finalQueueIndex = childRightIndex;
            }
        }
        // Not swapping with left-child, does right-child exist?
        else if(childRightIndex > _numNodes)
        {
            return;
        }
        else
        {
            // Check if the right-child is higher-priority than the current node
            TItem childRight = _nodes[childRightIndex];
            if(HasHigherPriority(childRight, node))
            {
                childRight.QueueIndex = finalQueueIndex;
                _nodes[finalQueueIndex] = childRight;
                finalQueueIndex = childRightIndex;
            }
            // Neither child is higher-priority than current, so finish and stop.
            else
            {
                return;
            }
        }

        while(true)
        {
            childLeftIndex = 2 * finalQueueIndex;

            // If leaf node, we're done
            if(childLeftIndex > _numNodes)
            {
                node.QueueIndex = finalQueueIndex;
                _nodes[finalQueueIndex] = node;
                break;
            }

            // Check if the left-child is higher-priority than the current node
            childRightIndex = childLeftIndex + 1;
            childLeft = _nodes[childLeftIndex];
            if(HasHigherPriority(childLeft, node))
            {
                // Check if there is a right child. If not, swap and finish.
                if(childRightIndex > _numNodes)
                {
                    node.QueueIndex = childLeftIndex;
                    childLeft.QueueIndex = finalQueueIndex;
                    _nodes[finalQueueIndex] = childLeft;
                    _nodes[childLeftIndex] = node;
                    break;
                }
                // Check if the left-child is higher-priority than the right-child
                TItem childRight = _nodes[childRightIndex];
                if(HasHigherPriority(childLeft, childRight))
                {
                    // left is highest, move it up and continue
                    childLeft.QueueIndex = finalQueueIndex;
                    _nodes[finalQueueIndex] = childLeft;
                    finalQueueIndex = childLeftIndex;
                }
                else
                {
                    // right is even higher, move it up and continue
                    childRight.QueueIndex = finalQueueIndex;
                    _nodes[finalQueueIndex] = childRight;
                    finalQueueIndex = childRightIndex;
                }
            }
            // Not swapping with left-child, does right-child exist?
            else if(childRightIndex > _numNodes)
            {
                node.QueueIndex = finalQueueIndex;
                _nodes[finalQueueIndex] = node;
                break;
            }
            else
            {
                // Check if the right-child is higher-priority than the current node
                TItem childRight = _nodes[childRightIndex];
                if(HasHigherPriority(childRight, node))
                {
                    childRight.QueueIndex = finalQueueIndex;
                    _nodes[finalQueueIndex] = childRight;
                    finalQueueIndex = childRightIndex;
                }
                // Neither child is higher-priority than current, so finish and stop.
                else
                {
                    node.QueueIndex = finalQueueIndex;
                    _nodes[finalQueueIndex] = node;
                    break;
                }
            }
        }
    }
    
    private bool HasHigherPriority(TItem higher, TItem lower)
    {
        var cmp = _comparer(higher.Priority, lower.Priority);
        return (cmp < 0 || (cmp == 0 && higher.InsertionIndex < lower.InsertionIndex));
    }
    
    public TItem Dequeue()
    {
        TItem returnMe = _nodes[1];
        //If the node is already the last node, we can remove it immediately
        if(_numNodes == 1)
        {
            _nodes[1] = null;
            _numNodes = 0;
            return returnMe;
        }

        //Swap the node with the last node
        TItem formerLastNode = _nodes[_numNodes];
        _nodes[1] = formerLastNode;
        formerLastNode.QueueIndex = 1;
        _nodes[_numNodes] = null;
        _numNodes--;

        //Now bubble formerLastNode (which is no longer the last node) down
        CascadeDown(formerLastNode);
        return returnMe;
    }
    
    public void Resize(int maxNodes)
    {
        TItem[] newArray = new TItem[maxNodes + 1];
        int highestIndexToCopy = Math.Min(maxNodes, _numNodes);
        Array.Copy(_nodes, newArray, highestIndexToCopy + 1);
        _nodes = newArray;
    }

    /// <summary>
    /// Returns the head of the queue, without removing it (use Dequeue() for that).
    /// If the queue is empty, behavior is undefined.
    /// O(1)
    /// </summary>
    public TItem First
    {
        get
        {
            return _nodes[1];
        }
    }

    public void UpdatePriority(TItem node, TPriority priority)
    {
        node.Priority = priority;
        OnNodeUpdated(node);
    }
    
    private void OnNodeUpdated(TItem node)
    {
        //Bubble the updated node up or down as appropriate
        int parentIndex = node.QueueIndex >> 1;

        if(parentIndex > 0 && HasHigherPriority(node, _nodes[parentIndex]))
        {
            CascadeUp(node);
        }
        else
        {
            //Note that CascadeDown will be called if parentNode == node (that is, node is the root)
            CascadeDown(node);
        }
    }
    
    public void Remove(TItem node)
    {
        //If the node is already the last node, we can remove it immediately
        if(node.QueueIndex == _numNodes)
        {
            _nodes[_numNodes] = null;
            _numNodes--;
            return;
        }

        //Swap the node with the last node
        TItem formerLastNode = _nodes[_numNodes];
        _nodes[node.QueueIndex] = formerLastNode;
        formerLastNode.QueueIndex = node.QueueIndex;
        _nodes[_numNodes] = null;
        _numNodes--;

        //Now bubble formerLastNode (which is no longer the last node) up or down as appropriate
        OnNodeUpdated(formerLastNode);
    }
    
    public void ResetNode(TItem node)
    {
        node.QueueIndex = 0;
    }


    public IEnumerator<TItem> GetEnumerator()
    {
        for(int i = 1; i <= _numNodes; i++)
            yield return _nodes[i];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    public bool IsValidQueue()
    {
        for(int i = 1; i < _nodes.Length; i++)
        {
            if(_nodes[i] != null)
            {
                int childLeftIndex = 2 * i;
                if(childLeftIndex < _nodes.Length && _nodes[childLeftIndex] != null && HasHigherPriority(_nodes[childLeftIndex], _nodes[i]))
                    return false;

                int childRightIndex = childLeftIndex + 1;
                if(childRightIndex < _nodes.Length && _nodes[childRightIndex] != null && HasHigherPriority(_nodes[childRightIndex], _nodes[i]))
                    return false;
            }
        }
        return true;
    }
}

public class SimplePriorityQueue<TItem, TPriority> : IPriorityQueue<TItem, TPriority>
    where TPriority : IComparable<TPriority>
{
    private class SimpleNode : GenericPriorityQueueNode<TPriority>
    {
        public TItem Data { get; private set; }

        public SimpleNode(TItem data)
        {
            Data = data;
        }
    }

    private const int INITIAL_QUEUE_SIZE = 10;
    private readonly GenericPriorityQueue<SimpleNode, TPriority> _queue;
    private readonly Dictionary<TItem, IList<SimpleNode>> _itemToNodesCache;
    private readonly IList<SimpleNode> _nullNodesCache;

    #region Constructors
    
    public SimplePriorityQueue() : this(Comparer<TPriority>.Default, EqualityComparer<TItem>.Default) { }
    
    public SimplePriorityQueue(IComparer<TPriority> priorityComparer) : this(priorityComparer.Compare, EqualityComparer<TItem>.Default) { }
    
    public SimplePriorityQueue(Comparison<TPriority> priorityComparer) : this(priorityComparer, EqualityComparer<TItem>.Default) { }
    
    public SimplePriorityQueue(IEqualityComparer<TItem> itemEquality) : this(Comparer<TPriority>.Default, itemEquality) { }
    
    public SimplePriorityQueue(IComparer<TPriority> priorityComparer, IEqualityComparer<TItem> itemEquality) : this(priorityComparer.Compare, itemEquality) { }
    
    public SimplePriorityQueue(Comparison<TPriority> priorityComparer, IEqualityComparer<TItem> itemEquality)
    {
        _queue = new GenericPriorityQueue<SimpleNode, TPriority>(INITIAL_QUEUE_SIZE, priorityComparer);
        _itemToNodesCache = new Dictionary<TItem, IList<SimpleNode>>(itemEquality);
        _nullNodesCache = new List<SimpleNode>();
    }
    #endregion
    
    private SimpleNode GetExistingNode(TItem item)
    {
        if (item == null)
        {
            return _nullNodesCache.Count > 0 ? _nullNodesCache[0] : null;
        }

        IList<SimpleNode> nodes;
        if (!_itemToNodesCache.TryGetValue(item, out nodes))
        {
            return null;
        }
        return nodes[0];
    }
    
    private void AddToNodeCache(SimpleNode node)
    {
        if (node.Data == null)
        {
            _nullNodesCache.Add(node);
            return;
        }

        IList<SimpleNode> nodes;
        if (!_itemToNodesCache.TryGetValue(node.Data, out nodes))
        {
            nodes = new List<SimpleNode>();
            _itemToNodesCache[node.Data] = nodes;
        }
        nodes.Add(node);
    }
    
    private void RemoveFromNodeCache(SimpleNode node)
    {
        if (node.Data == null)
        {
            _nullNodesCache.Remove(node);
            return;
        }

        IList<SimpleNode> nodes;
        if (!_itemToNodesCache.TryGetValue(node.Data, out nodes))
        {
            return;
        }
        nodes.Remove(node);
        if (nodes.Count == 0)
        {
            _itemToNodesCache.Remove(node.Data);
        }
    }
    
    public int Count
    {
        get
        {
            lock(_queue)
            {
                return _queue.Count;
            }
        }
    }
    
    public TItem First
    {
        get
        {
            lock(_queue)
            {
                if(_queue.Count <= 0)
                {
                    throw new InvalidOperationException("Cannot call .First on an empty queue");
                }

                return _queue.First.Data;
            }
        }
    }
    
    public void Clear()
    {
        lock(_queue)
        {
            _queue.Clear();
            _itemToNodesCache.Clear();
            _nullNodesCache.Clear();
        }
    }
    
    public bool Contains(TItem item)
    {
        lock(_queue)
        {
            return item == null ? _nullNodesCache.Count > 0 : _itemToNodesCache.ContainsKey(item);
        }
    }
    
    public TItem Dequeue()
    {
        lock(_queue)
        {
            if(_queue.Count <= 0)
            {
                throw new InvalidOperationException("Cannot call Dequeue() on an empty queue");
            }

            SimpleNode node =_queue.Dequeue();
            RemoveFromNodeCache(node);
            return node.Data;
        }
    }
    
    private SimpleNode EnqueueNoLockOrCache(TItem item, TPriority priority)
    {
        SimpleNode node = new SimpleNode(item);
        if (_queue.Count == _queue.MaxSize)
        {
            _queue.Resize(_queue.MaxSize * 2 + 1);
        }
        _queue.Enqueue(node, priority);
        return node;
    }
    
    public void Enqueue(TItem item, TPriority priority)
    {
        lock(_queue)
        {
            IList<SimpleNode> nodes;
            if (item == null)
            {
                nodes = _nullNodesCache;
            }
            else if (!_itemToNodesCache.TryGetValue(item, out nodes))
            {
                nodes = new List<SimpleNode>();
                _itemToNodesCache[item] = nodes;
            }
            SimpleNode node = EnqueueNoLockOrCache(item, priority);
            nodes.Add(node);
        }
    }
    
    public bool EnqueueWithoutDuplicates(TItem item, TPriority priority)
    {
        lock(_queue)
        {
            IList<SimpleNode> nodes;
            if (item == null)
            {
                if (_nullNodesCache.Count > 0)
                {
                    return false;
                }
                nodes = _nullNodesCache;
            }
            else if (_itemToNodesCache.ContainsKey(item))
            {
                return false;
            }
            else
            {
                nodes = new List<SimpleNode>();
                _itemToNodesCache[item] = nodes;
            }
            SimpleNode node = EnqueueNoLockOrCache(item, priority);
            nodes.Add(node);
            return true;
        }
    }
    
    public void Remove(TItem item)
    {
        lock(_queue)
        {
            SimpleNode removeMe;
            IList<SimpleNode> nodes;
            if (item == null)
            {
                if (_nullNodesCache.Count == 0)
                {
                    throw new InvalidOperationException("Cannot call Remove() on a node which is not enqueued: " + item);
                }
                removeMe = _nullNodesCache[0];
                nodes = _nullNodesCache;
            }
            else
            {
                if (!_itemToNodesCache.TryGetValue(item, out nodes))
                {
                    throw new InvalidOperationException("Cannot call Remove() on a node which is not enqueued: " + item);
                }
                removeMe = nodes[0];
                if (nodes.Count == 1)
                {
                    _itemToNodesCache.Remove(item);
                }
            }
            _queue.Remove(removeMe);
            nodes.Remove(removeMe);
        }
    }
    
    public void UpdatePriority(TItem item, TPriority priority)
    {
        lock (_queue)
        {
            SimpleNode updateMe = GetExistingNode(item);
            if (updateMe == null)
            {
                throw new InvalidOperationException("Cannot call UpdatePriority() on a node which is not enqueued: " + item);
            }
            _queue.UpdatePriority(updateMe, priority);
        }
    }
    
    public TPriority GetPriority(TItem item)
    {
        lock (_queue)
        {
            SimpleNode findMe = GetExistingNode(item);
            if(findMe == null)
            {
                throw new InvalidOperationException("Cannot call GetPriority() on a node which is not enqueued: " + item);
            }
            return findMe.Priority;
        }
    }

    #region Try* methods for multithreading
    
    public bool TryFirst(out TItem first)
    {
        if (_queue.Count > 0)
        {
            lock (_queue)
            {
                if (_queue.Count > 0)
                {
                    first = _queue.First.Data;
                    return true;
                }
            }
        }

        first = default(TItem);
        return false;
    }
    
    public bool TryDequeue(out TItem first)
    {
        if (_queue.Count > 0)
        {
            lock (_queue)
            {
                if (_queue.Count > 0)
                {
                    SimpleNode node = _queue.Dequeue();
                    first = node.Data;
                    RemoveFromNodeCache(node);
                    return true;
                }
            }
        }
        
        first = default(TItem);
        return false;
    }
    
    public bool TryRemove(TItem item)
    {
        lock(_queue)
        {
            SimpleNode removeMe;
            IList<SimpleNode> nodes;
            if (item == null)
            {
                if (_nullNodesCache.Count == 0)
                {
                    return false;
                }
                removeMe = _nullNodesCache[0];
                nodes = _nullNodesCache;
            }
            else
            {
                if (!_itemToNodesCache.TryGetValue(item, out nodes))
                {
                    return false;
                }
                removeMe = nodes[0];
                if (nodes.Count == 1)
                {
                    _itemToNodesCache.Remove(item);
                }
            }
            _queue.Remove(removeMe);
            nodes.Remove(removeMe);
            return true;
        }
    }
    
    public bool TryUpdatePriority(TItem item, TPriority priority)
    {
        lock(_queue)
        {
            SimpleNode updateMe = GetExistingNode(item);
            if(updateMe == null)
            {
                return false;
            }
            _queue.UpdatePriority(updateMe, priority);
            return true;
        }
    }
    
    public bool TryGetPriority(TItem item, out TPriority priority)
    {
        lock(_queue)
        {
            SimpleNode findMe = GetExistingNode(item);
            if(findMe == null)
            {
                priority = default(TPriority);
                return false;
            }
            priority = findMe.Priority;
            return true;
        }
    }
    #endregion

    public IEnumerator<TItem> GetEnumerator()
    {
        List<TItem> queueData = new List<TItem>();
        lock (_queue)
        {
            //Copy to a separate list because we don't want to 'yield return' inside a lock
            foreach(var node in _queue)
            {
                queueData.Add(node.Data);
            }
        }

        return queueData.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool IsValidQueue()
    {
        lock(_queue)
        {
            // Check all items in cache are in the queue
            foreach (IList<SimpleNode> nodes in _itemToNodesCache.Values)
            {
                foreach (SimpleNode node in nodes)
                {
                    if (!_queue.Contains(node))
                    {
                        return false;
                    }
                }
            }

            // Check all items in queue are in cache
            foreach (SimpleNode node in _queue)
            {
                if (GetExistingNode(node.Data) == null)
                {
                    return false;
                }
            }

            // Check queue structure itself
            return _queue.IsValidQueue();
        }
    }
}


/* Adapted from https://github.com/Bl4ckb0ne/delaunay-triangulation

Copyright (c) 2015-2019 Simon Zeni (simonzeni@gmail.com)


Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:


The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.


THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.*/

namespace Graphs {
    public class Vertex : IEquatable<Vertex> {
        public Vector3 Position { get; private set; }

        public Vertex() {

        }

        public Vertex(Vector3 position) {
            Position = position;
        }

        public override bool Equals(object obj) {
            if (obj is Vertex v) {
                return Position == v.Position;
            }

            return false;
        }

        public bool Equals(Vertex other) {
            return Position == other.Position;
        }

        public override int GetHashCode() {
            return Position.GetHashCode();
        }
    }

    public class Vertex<T> : Vertex {
        public T Item { get; private set; }

        public Vertex(T item) {
            Item = item;
        }

        public Vertex(Vector3 position, T item) : base(position) {
            Item = item;
        }
    }

    public class Edge : IEquatable<Edge> {
        public Vertex U { get; set; }
        public Vertex V { get; set; }

        public Edge() {

        }

        public Edge(Vertex u, Vertex v) {
            U = u;
            V = v;
        }

        public static bool operator ==(Edge left, Edge right) {
            return (left.U == right.U || left.U == right.V)
                && (left.V == right.U || left.V == right.V);
        }

        public static bool operator !=(Edge left, Edge right) {
            return !(left == right);
        }

        public override bool Equals(object obj) {
            if (obj is Edge e) {
                return this == e;
            }

            return false;
        }

        public bool Equals(Edge e) {
            return this == e;
        }

        public override int GetHashCode() {
            return U.GetHashCode() ^ V.GetHashCode();
        }
    }
}

public class Node {
    public Vector3Int Position { get; private set; }
    public Node Previous { get; set; }
    public HashSet<Vector3Int> PreviousSet { get; private set; }
    public float Cost { get; set; }

    public Node(Vector3Int position) {
        Position = position;
        PreviousSet = new HashSet<Vector3Int>();
    }
}

public static class Prim {
    public class Edge : Graphs.Edge {
        public float Distance { get; private set; }

        public Edge(Vertex u, Vertex v) : base(u, v) {
            Distance = Vector3.Distance(u.Position, v.Position);
        }

        public static bool operator ==(Edge left, Edge right) {
            return (left.U == right.U && left.V == right.V)
                || (left.U == right.V && left.V == right.U);
        }

        public static bool operator !=(Edge left, Edge right) {
            return !(left == right);
        }

        public override bool Equals(object obj) {
            if (obj is Edge e) {
                return this == e;
            }

            return false;
        }

        public bool Equals(Edge e) {
            return this == e;
        }

        public override int GetHashCode() {
            return U.GetHashCode() ^ V.GetHashCode();
        }
    }

    public static List<Edge> MinimumSpanningTree(List<Edge> edges, Vertex start) {
        HashSet<Vertex> openSet = new HashSet<Vertex>();
        HashSet<Vertex> closedSet = new HashSet<Vertex>();

        foreach (var edge in edges) {
            openSet.Add(edge.U);
            openSet.Add(edge.V);
        }

        closedSet.Add(start);

        List<Edge> results = new List<Edge>();

        while (openSet.Count > 0) {
            bool chosen = false;
            Edge chosenEdge = null;
            float minWeight = float.PositiveInfinity;

            foreach (var edge in edges) {
                int closedVertices = 0;
                if (!closedSet.Contains(edge.U)) closedVertices++;
                if (!closedSet.Contains(edge.V)) closedVertices++;
                if (closedVertices != 1) continue;

                if (edge.Distance < minWeight) {
                    chosenEdge = edge;
                    chosen = true;
                    minWeight = edge.Distance;
                }
            }

            if (!chosen) break;
            results.Add(chosenEdge);
            openSet.Remove(chosenEdge.U);
            openSet.Remove(chosenEdge.V);
            closedSet.Add(chosenEdge.U);
            closedSet.Add(chosenEdge.V);
        }

        return results;
    }
}

public class Delaunay3D {
    public class Tetrahedron : IEquatable<Tetrahedron> {
        public Vertex A { get; set; }
        public Vertex B { get; set; }
        public Vertex C { get; set; }
        public Vertex D { get; set; }

        public bool IsBad { get; set; }

        Vector3 Circumcenter { get; set; }
        float CircumradiusSquared { get; set; }

        public Tetrahedron(Vertex a, Vertex b, Vertex c, Vertex d) {
            A = a;
            B = b;
            C = c;
            D = d;
            CalculateCircumsphere();
        }

        void CalculateCircumsphere() {
            //calculate the circumsphere of a tetrahedron
            //http://mathworld.wolfram.com/Circumsphere.html

            float a = new Matrix4x4(
                new Vector4(A.Position.x, B.Position.x, C.Position.x, D.Position.x),
                new Vector4(A.Position.y, B.Position.y, C.Position.y, D.Position.y),
                new Vector4(A.Position.z, B.Position.z, C.Position.z, D.Position.z),
                new Vector4(1, 1, 1, 1)
            ).determinant;

            float aPosSqr = A.Position.sqrMagnitude;
            float bPosSqr = B.Position.sqrMagnitude;
            float cPosSqr = C.Position.sqrMagnitude;
            float dPosSqr = D.Position.sqrMagnitude;

            float Dx = new Matrix4x4(
                new Vector4(aPosSqr, bPosSqr, cPosSqr, dPosSqr),
                new Vector4(A.Position.y, B.Position.y, C.Position.y, D.Position.y),
                new Vector4(A.Position.z, B.Position.z, C.Position.z, D.Position.z),
                new Vector4(1, 1, 1, 1)
            ).determinant;

            float Dy = -(new Matrix4x4(
                new Vector4(aPosSqr, bPosSqr, cPosSqr, dPosSqr),
                new Vector4(A.Position.x, B.Position.x, C.Position.x, D.Position.x),
                new Vector4(A.Position.z, B.Position.z, C.Position.z, D.Position.z),
                new Vector4(1, 1, 1, 1)
            ).determinant);

            float Dz = new Matrix4x4(
                new Vector4(aPosSqr, bPosSqr, cPosSqr, dPosSqr),
                new Vector4(A.Position.x, B.Position.x, C.Position.x, D.Position.x),
                new Vector4(A.Position.y, B.Position.y, C.Position.y, D.Position.y),
                new Vector4(1, 1, 1, 1)
            ).determinant;

            float c = new Matrix4x4(
                new Vector4(aPosSqr, bPosSqr, cPosSqr, dPosSqr),
                new Vector4(A.Position.x, B.Position.x, C.Position.x, D.Position.x),
                new Vector4(A.Position.y, B.Position.y, C.Position.y, D.Position.y),
                new Vector4(A.Position.z, B.Position.z, C.Position.z, D.Position.z)
            ).determinant;

            Circumcenter = new Vector3(
                Dx / (2 * a),
                Dy / (2 * a),
                Dz / (2 * a)
            );

            CircumradiusSquared = ((Dx * Dx) + (Dy * Dy) + (Dz * Dz) - (4 * a * c)) / (4 * a * a);
        }

        public bool ContainsVertex(Vertex v) {
            return AlmostEqual(v, A)
                || AlmostEqual(v, B)
                || AlmostEqual(v, C)
                || AlmostEqual(v, D);
        }

        public bool CircumCircleContains(Vector3 v) {
            Vector3 dist = v - Circumcenter;
            return dist.sqrMagnitude <= CircumradiusSquared;
        }

        public static bool operator ==(Tetrahedron left, Tetrahedron right) {
            return (left.A == right.A || left.A == right.B || left.A == right.C || left.A == right.D)
                && (left.B == right.A || left.B == right.B || left.B == right.C || left.B == right.D)
                && (left.C == right.A || left.C == right.B || left.C == right.C || left.C == right.D)
                && (left.D == right.A || left.D == right.B || left.D == right.C || left.D == right.D);
        }

        public static bool operator !=(Tetrahedron left, Tetrahedron right) {
            return !(left == right);
        }

        public override bool Equals(object obj) {
            if (obj is Tetrahedron t) {
                return this == t;
            }

            return false;
        }

        public bool Equals(Tetrahedron t) {
            return this == t;
        }

        public override int GetHashCode() {
            return A.GetHashCode() ^ B.GetHashCode() ^ C.GetHashCode() ^ D.GetHashCode();
        }
    }

    public class Triangle {
        public Vertex U { get; set; }
        public Vertex V { get; set; }
        public Vertex W { get; set; }

        public bool IsBad { get; set; }

        public Triangle() {

        }

        public Triangle(Vertex u, Vertex v, Vertex w) {
            U = u;
            V = v;
            W = w;
        }

        public static bool operator ==(Triangle left, Triangle right) {
            return (left.U == right.U || left.U == right.V || left.U == right.W)
                && (left.V == right.U || left.V == right.V || left.V == right.W)
                && (left.W == right.U || left.W == right.V || left.W == right.W);
        }

        public static bool operator !=(Triangle left, Triangle right) {
            return !(left == right);
        }

        public override bool Equals(object obj) {
            if (obj is Triangle e) {
                return this == e;
            }

            return false;
        }

        public bool Equals(Triangle e) {
            return this == e;
        }

        public override int GetHashCode() {
            return U.GetHashCode() ^ V.GetHashCode() ^ W.GetHashCode();
        }

        public static bool AlmostEqual(Triangle left, Triangle right) {
            return (Delaunay3D.AlmostEqual(left.U, right.U) || Delaunay3D.AlmostEqual(left.U, right.V) || Delaunay3D.AlmostEqual(left.U, right.W))
                && (Delaunay3D.AlmostEqual(left.V, right.U) || Delaunay3D.AlmostEqual(left.V, right.V) || Delaunay3D.AlmostEqual(left.V, right.W))
                && (Delaunay3D.AlmostEqual(left.W, right.U) || Delaunay3D.AlmostEqual(left.W, right.V) || Delaunay3D.AlmostEqual(left.W, right.W));
        }
    }

    public class Edge {
        public Vertex U { get; set; }
        public Vertex V { get; set; }
        public float Distance { get; private set; }

        public bool IsBad { get; set; }

        public Edge() {

        }

        public Edge(Vertex u, Vertex v) {
            U = u;
            V = v;
            Distance = Vector3.Distance(u.Position, v.Position);
        }

        public static bool operator ==(Edge left, Edge right) {
            return (left.U == right.U || left.U == right.V)
                && (left.V == right.U || left.V == right.V);
        }

        public static bool operator !=(Edge left, Edge right) {
            return !(left == right);
        }

        public override bool Equals(object obj) {
            if (obj is Edge e) {
                return this == e;
            }

            return false;
        }

        public bool Equals(Edge e) {
            return this == e;
        }

        public override int GetHashCode() {
            return U.GetHashCode() ^ V.GetHashCode();
        }

        public static bool AlmostEqual(Edge left, Edge right) {
            return (Delaunay3D.AlmostEqual(left.U, right.U) || Delaunay3D.AlmostEqual(left.V, right.U))
                && (Delaunay3D.AlmostEqual(left.U, right.V) || Delaunay3D.AlmostEqual(left.V, right.U));
        }
    }

    static bool AlmostEqual(Vertex left, Vertex right) {
        return (left.Position - right.Position).sqrMagnitude < 0.01f;
    }

    public List<Vertex> Vertices { get; private set; }
    public List<Edge> Edges { get; private set; }
    public List<Triangle> Triangles { get; private set; }
    public List<Tetrahedron> Tetrahedra { get; private set; }

    Delaunay3D() {
        Edges = new List<Edge>();
        Triangles = new List<Triangle>();
        Tetrahedra = new List<Tetrahedron>();
    }

    public static Delaunay3D Triangulate(List<Vertex> vertices) {
        Delaunay3D delaunay = new Delaunay3D();
        delaunay.Vertices = new List<Vertex>(vertices);
        delaunay.Triangulate();

        return delaunay;
    }

    void Triangulate() {
        float minX = Vertices[0].Position.x;
        float minY = Vertices[0].Position.y;
        float minZ = Vertices[0].Position.z;
        float maxX = minX;
        float maxY = minY;
        float maxZ = minZ;

        foreach (var vertex in Vertices) {
            if (vertex.Position.x < minX) minX = vertex.Position.x;
            if (vertex.Position.x > maxX) maxX = vertex.Position.x;
            if (vertex.Position.y < minY) minY = vertex.Position.y;
            if (vertex.Position.y > maxY) maxY = vertex.Position.y;
            if (vertex.Position.z < minZ) minZ = vertex.Position.z;
            if (vertex.Position.z > maxZ) maxZ = vertex.Position.z;
        }

        float dx = maxX - minX;
        float dy = maxY - minY;
        float dz = maxZ - minZ;
        float deltaMax = Mathf.Max(dx, dy, dz) * 2;

        Vertex p1 = new Vertex(new Vector3(minX - 1         , minY - 1          , minZ - 1          ));
        Vertex p2 = new Vertex(new Vector3(maxX + deltaMax  , minY - 1          , minZ - 1          ));
        Vertex p3 = new Vertex(new Vector3(minX - 1         , maxY + deltaMax   , minZ - 1          ));
        Vertex p4 = new Vertex(new Vector3(minX - 1         , minY - 1          , maxZ + deltaMax   ));

        Tetrahedra.Add(new Tetrahedron(p1, p2, p3, p4));

        foreach (var vertex in Vertices) {
            List<Triangle> triangles = new List<Triangle>();

            foreach (var t in Tetrahedra) {
                if (t.CircumCircleContains(vertex.Position)) {
                    t.IsBad = true;
                    triangles.Add(new Triangle(t.A, t.B, t.C));
                    triangles.Add(new Triangle(t.A, t.B, t.D));
                    triangles.Add(new Triangle(t.A, t.C, t.D));
                    triangles.Add(new Triangle(t.B, t.C, t.D));
                }
            }

            for (int i = 0; i < triangles.Count; i++) {
                for (int j = i + 1; j < triangles.Count; j++) {
                    if (Triangle.AlmostEqual(triangles[i], triangles[j])) {
                        triangles[i].IsBad = true;
                        triangles[j].IsBad = true;
                    }
                }
            }

            Tetrahedra.RemoveAll((Tetrahedron t) => t.IsBad);
            triangles.RemoveAll((Triangle t) => t.IsBad);

            foreach (var triangle in triangles) {
                Tetrahedra.Add(new Tetrahedron(triangle.U, triangle.V, triangle.W, vertex));
            }
        }

        Tetrahedra.RemoveAll((Tetrahedron t) => t.ContainsVertex(p1) || t.ContainsVertex(p2) || t.ContainsVertex(p3) || t.ContainsVertex(p4));

        HashSet<Triangle> triangleSet = new HashSet<Triangle>();
        HashSet<Edge> edgeSet = new HashSet<Edge>();

        foreach (var t in Tetrahedra) {
            var abc = new Triangle(t.A, t.B, t.C);
            var abd = new Triangle(t.A, t.B, t.D);
            var acd = new Triangle(t.A, t.C, t.D);
            var bcd = new Triangle(t.B, t.C, t.D);

            if (triangleSet.Add(abc)) {
                Triangles.Add(abc);
            }

            if (triangleSet.Add(abd)) {
                Triangles.Add(abd);
            }

            if (triangleSet.Add(acd)) {
                Triangles.Add(acd);
            }

            if (triangleSet.Add(bcd)) {
                Triangles.Add(bcd);
            }

            var ab = new Edge(t.A, t.B);
            var bc = new Edge(t.B, t.C);
            var ca = new Edge(t.C, t.A);
            var da = new Edge(t.D, t.A);
            var db = new Edge(t.D, t.B);
            var dc = new Edge(t.D, t.C);

            if (edgeSet.Add(ab)) {
                Edges.Add(ab);
            }

            if (edgeSet.Add(bc)) {
                Edges.Add(bc);
            }

            if (edgeSet.Add(ca)) {
                Edges.Add(ca);
            }

            if (edgeSet.Add(da)) {
                Edges.Add(da);
            }

            if (edgeSet.Add(db)) {
                Edges.Add(db);
            }

            if (edgeSet.Add(dc)) {
                Edges.Add(dc);
            }
        }
    }
}

public class AStar {
    public class Node {
        public Vector3Int Position { get; private set; }
        public Node Previous { get; set; }
        public HashSet<Vector3Int> PreviousSet { get; private set; }
        public float Cost { get; set; }

        public Node(Vector3Int position) {
            Position = position;
            PreviousSet = new HashSet<Vector3Int>();
        }
    }

    public struct PathCost {
        public bool traversable;
        public float cost;
        public bool isStairs;
        public ProcGenner.BlockType prevBlock;
    }

    static readonly Vector3Int[] neighbors = {
        new Vector3Int(1, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 0, -1),

        new Vector3Int(3, 1, 0),
        new Vector3Int(-3, 1, 0),
        new Vector3Int(0, 1, 3),
        new Vector3Int(0, 1, -3),

        new Vector3Int(3, -1, 0),
        new Vector3Int(-3, -1, 0),
        new Vector3Int(0, -1, 3),
        new Vector3Int(0, -1, -3),
    };

    Node[,,] grid;
    Vector3Int gridsize;
    SimplePriorityQueue<Node, float> queue;
    HashSet<Node> closed;
    Stack<Vector3Int> stack;

    public AStar(Vector3Int size) {
        grid = new Node[size.x, size.y, size.z];
        gridsize = size;
        
        queue = new SimplePriorityQueue<Node, float>();
        closed = new HashSet<Node>();
        stack = new Stack<Vector3Int>();

        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                for (int z = 0; z < size.z; z++) {
                    grid[x, y, z] = new Node(new Vector3Int(x, y, z));
                }
            }
        }
    }

    void ResetNodes() {
        var size = gridsize;

        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                for (int z = 0; z < size.z; z++) {
                    var node = grid[x, y, z];
                    node.Previous = null;
                    node.Cost = float.PositiveInfinity;
                    node.PreviousSet.Clear();
                }
            }
        }
    }

    public List<Vector3Int> FindPath(Vector3Int start, Vector3Int end, Func<Node, Node, PathCost> costFunction) {
        ResetNodes();
        queue.Clear();
        closed.Clear();

        queue = new SimplePriorityQueue<Node, float>();
        closed = new HashSet<Node>();

        grid[start.x, start.y, start.z].Cost = 0;
        queue.Enqueue(grid[start.x, start.y, start.z], 0);

        while (queue.Count > 0) {
            Node node = queue.Dequeue();
            closed.Add(node);

            if (node.Position == end) {
                return ReconstructPath(node);
            }

            foreach (var offset in neighbors) {
                if (!(new BoundsInt(Vector3Int.zero, gridsize).Contains(node.Position + offset))) continue;
                var neighbor = grid[(node.Position + offset).x, (node.Position + offset).y, (node.Position + offset).z];
                if (closed.Contains(neighbor)) continue;

                if (node.PreviousSet.Contains(neighbor.Position)) {
                    continue;
                }

                var pathCost = costFunction(node, neighbor);
                if (!pathCost.traversable) continue;

                if (pathCost.isStairs) {
                    int xDir = Mathf.Clamp(offset.x, -1, 1);
                    int zDir = Mathf.Clamp(offset.z, -1, 1);
                    Vector3Int verticalOffset = new Vector3Int(0, offset.y, 0);
                    Vector3Int horizontalOffset = new Vector3Int(xDir, 0, zDir);

                    if (node.PreviousSet.Contains(node.Position + horizontalOffset)
                        || node.PreviousSet.Contains(node.Position + horizontalOffset * 2)
                        || node.PreviousSet.Contains(node.Position + verticalOffset + horizontalOffset)
                        || node.PreviousSet.Contains(node.Position + verticalOffset + horizontalOffset * 2)) {
                        continue;
                    }
                }

                float newCost = node.Cost + pathCost.cost;

                if (newCost < neighbor.Cost) {
                    neighbor.Previous = node;
                    neighbor.Cost = newCost;

                    if (queue.TryGetPriority(node, out float existingPriority)) {
                        queue.UpdatePriority(node, newCost);
                    } else {
                        queue.Enqueue(neighbor, neighbor.Cost);
                    }

                    neighbor.PreviousSet.Clear();
                    neighbor.PreviousSet.UnionWith(node.PreviousSet);
                    neighbor.PreviousSet.Add(node.Position);

                    if (pathCost.isStairs){
                        int xDir = Mathf.Clamp(offset.x, -1, 1);
                        int zDir = Mathf.Clamp(offset.z, -1, 1);
                        Vector3Int verticalOffset = new Vector3Int(0, offset.y, 0);
                        Vector3Int horizontalOffset = new Vector3Int(xDir, 0, zDir);

                        neighbor.PreviousSet.Add(node.Position + horizontalOffset);
                        neighbor.PreviousSet.Add(node.Position + horizontalOffset * 2);
                        neighbor.PreviousSet.Add(node.Position + verticalOffset + horizontalOffset);
                        neighbor.PreviousSet.Add(node.Position + verticalOffset + horizontalOffset * 2);
                    }
                }
            }
        }

        return null;
    }

    List<Vector3Int> ReconstructPath(Node node) {
        List<Vector3Int> result = new List<Vector3Int>();

        while (node != null) {
            stack.Push(node.Position);
            node = node.Previous;
        }

        while (stack.Count > 0) {
            result.Add(stack.Pop());
        }

        return result;
    }
}