using LibDelaunay.Algorithm.Interfaces;

namespace LibDelaunay.Algorithm.Models
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