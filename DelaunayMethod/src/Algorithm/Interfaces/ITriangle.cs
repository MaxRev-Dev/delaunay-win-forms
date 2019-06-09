using System.Collections.Generic;

namespace DelaunayMethod.Algorithm.Interfaces
{
    public interface ITriangle
    {
        IEnumerable<IPoint> Points { get; }
        int Index { get; }
    }
}