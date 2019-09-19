using System.Collections.Generic;
using System.Linq;

namespace EightPuzzleSolver.Search
{
    public class Node<TProblemState> where TProblemState : IProblemState<TProblemState>
    {
        public Node( TProblemState kState )
        {
            State = kState;
            PathCost = 0;
        }

        public Node( TProblemState kState, Node<TProblemState> parent ) : this( kState )
        {
            Parent = parent;

            if ( Parent != null )
            {
                PathCost = Parent.PathCost + kState.Cost;
            }
        }

        public TProblemState State { get; }

        public Node<TProblemState> Parent { get; }

        /// <summary>
        /// The cost of the path from the initial state to the node
        /// </summary>
        public int PathCost { get; }

        public bool IsRootNode => Parent == null;

        /// <summary>
        /// Returns the nodes available from this node
        /// </summary>
        public IList<Node<TProblemState>> ExpandNode()
        {
            var kChildren = new List<Node<TProblemState>>();

            foreach ( TProblemState kChildState in State.NextStates() )
            {
                kChildren.Add( new Node<TProblemState>( kChildState, this ) );
            }

            return kChildren;
        }

        public IEnumerable<Node<TProblemState>> PathFromRoot()
        {
            var path = new Stack<Node<TProblemState>>();

            var node = this;
            while ( !node.IsRootNode )
            {
                path.Push( node );
                node = node.Parent;
            }
            path.Push( node ); // root

            return path;
        }

        public IEnumerable<TProblemState> PathFromRootStates()
        {
            return PathFromRoot().Select( n => n.State );
        }

        public override string ToString()
        {
            return $"State: {State}, PathCost: {PathCost}";
        }
    }
}
