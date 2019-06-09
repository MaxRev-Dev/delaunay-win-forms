using System;
using DelaunayMethod.Algorithm.Interfaces;

namespace DelaunayMethod.Algorithm.Models
{
    public struct Point : IPoint
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Point(float x, float y)
        {
            X = x;
            Y = y;
        }
        public override string ToString() => $"{X},{Y}";
        public override bool Equals(object obj)
        {
            if (obj is Point p)
            {
                return Math.Abs(p.X - X) < 0.00001 && Math.Abs(p.Y - Y) < 0.00001;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}