using System.Collections.Generic;

namespace LibDelaunay.Algorithm.Interfaces
{
    public interface IVoronoiCell
    {
        IEnumerable<IPoint> Points { get; }
        int Index { get; }
    }
}