using System.Collections.Generic;

namespace LibDelaunay.Algorithm.Interfaces
{
    public interface ITriangle
    {
        IEnumerable<IPoint> Points { get; }
        int Index { get; }
    }
}