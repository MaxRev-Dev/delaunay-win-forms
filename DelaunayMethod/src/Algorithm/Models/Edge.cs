using DelaunayMethod.Algorithm.Interfaces;

namespace DelaunayMethod.Algorithm.Models
{
    public struct Edge : IEdge
    {
        public IPoint P { get; }
        public IPoint Q { get; }
        public int Index { get; }

        public Edge(int e, IPoint p, IPoint q)
        {
            Index = e;
            P = p;
            Q = q;
        }
    }
}