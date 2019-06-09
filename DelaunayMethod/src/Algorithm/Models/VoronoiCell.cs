using System.Collections.Generic;
using DelaunayMethod.Algorithm.Interfaces;

namespace DelaunayMethod.Algorithm.Models
{
    public struct VoronoiCell : IVoronoiCell
    {
        public IEnumerable<IPoint> Points { get; set; }
        public int Index { get; set; }
        public VoronoiCell(int triangleIndex, IEnumerable<IPoint> points)
        {
            Points = points;
            Index = triangleIndex;
        }
    }
}