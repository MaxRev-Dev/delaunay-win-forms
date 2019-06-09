using System.Collections.Generic;

namespace DelaunayMethod.Algorithm.Interfaces
{
    public interface IVoronoiCell
    {
        IEnumerable<IPoint> Points { get; }
        int Index { get; }
    }
}