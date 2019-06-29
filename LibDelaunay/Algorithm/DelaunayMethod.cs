using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using LibDelaunay.Algorithm.Interfaces;
using LibDelaunay.Algorithm.Models;

namespace LibDelaunay.Algorithm
{
    public class DelaunayEngine : IDisposable
    {
        //source algorithm https://github.com/mapbox/delaunator

        private static readonly ArrayPool<int> _intPool =
            ArrayPool<int>.Create(85000, 10);
        private static readonly ArrayPool<double> _doublePool =
            ArrayPool<double>.Create(85000, 10);

        private int[] _edgeHeap = new int[512];
        private readonly double _minDelta = Math.Pow(2, -52);

        public IEnumerable<IPoint> Points => _rawInternal;
        private int[] _trianglesHeap;
        private int[] _halfedgesHeap;
        private readonly IPoint[] _rawInternal;

        private int _hashSize;
        private double[] coordArr;
        private int[] _hullTri;
        private int[] _hull;
        private int[] _hullNext;
        private int[] _hullPrev;
        private int[] _hullHash;
        private int[] triaID;
        private int _triaLen;
        private int hullStart;
        private int hullSize;
        private readonly int _i0, _i1, _i2;
        private readonly double _cx;
        private readonly double _cy;
        private int n;

        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="InvalidDataException"></exception>
        public DelaunayEngine(IEnumerable<IPoint> points)
        {
            var list = points as IPoint[] ?? points.ToArray();
            if (list.Length < 2)
            {
                throw new ArgumentOutOfRangeException("Need at least 3 points");
            }

            _rawInternal = list;
            n = _rawInternal.Length;

            InitializeCoordinateHeap();
            InitializeHeap();
            var (cx, cy) = InitializeTriaID();

            _i0 = _i1 = _i2 = 0;

            CalculateI0(cx, cy, out _i0);

            var i0x = coordArr[2 * _i0];
            var i0y = coordArr[2 * _i0 + 1];

            CalculateI1(i0x, i0y, out _i1);

            var i1x = coordArr[2 * _i1];
            var i1y = coordArr[2 * _i1 + 1];

            var minRadius = CalculateI2(i0x, i0y, i1x, i1y, out _i2);

            var i2x = coordArr[2 * _i2];
            var i2y = coordArr[2 * _i2 + 1];

            if (double.IsPositiveInfinity(minRadius))
            {
                throw new InvalidDataException("No Delaunay triangulation exists for this input.");
            }

            if (Orient(i0x, i0y, i1x, i1y, i2x, i2y))
            {
                var i = _i1;
                var x = i1x;
                var y = i1y;
                _i1 = _i2;
                i1x = i2x;
                i1y = i2y;
                _i2 = i;
                i2x = x;
                i2y = y;
            }

            var center = CircumCenter(i0x, i0y, i1x, i1y, i2x, i2y);
            _cx = center.X;
            _cy = center.Y;

            var dists = _doublePool.Rent(n);
            for (var i = 0; i < n; i++)
            {
                dists[i] = GetDistance(coordArr[2 * i], coordArr[2 * i + 1], center.X, center.Y);
            }

            Quicksort(triaID, dists, 0, n - 1);
            _doublePool.Return(dists);

            _hullHash[HashKey(i0x, i0y)] = _i0;
            _hullHash[HashKey(i1x, i1y)] = _i1;
            _hullHash[HashKey(i2x, i2y)] = _i2;
            AddFirstTriangle();

            InitializeLookupTables(n);

            _hull = _intPool.Rent(hullSize);
            var s = hullStart;
            for (var i = 0; i < hullSize; i++)
            {
                _hull[i] = s;
                s = _hullNext[s];
            }

            var __trianglesHeap = _trianglesHeap.Take(_triaLen).ToArray();
            _intPool.Return(_trianglesHeap, true);
            var __halfedgesHeap = _halfedgesHeap.Take(_triaLen).ToArray();
            _intPool.Return(_halfedgesHeap, true);
            _trianglesHeap = __trianglesHeap;
            _halfedgesHeap = __halfedgesHeap;
        }

        private double CalculateI2(double i0x, double i0y, double i1x, double i1y, out int i2)
        {
            var minRadius = double.PositiveInfinity;
            i2 = 0;
            for (int i = 0; i < n; i++)
            {
                if (i == _i0 || i == _i1) continue;
                var r = CircumRadius(i0x, i0y, i1x, i1y, coordArr[2 * i], coordArr[2 * i + 1]);
                if (r < minRadius)
                {
                    i2 = i;
                    minRadius = r;
                }
            }

            return minRadius;
        }

        private void CalculateI1(double i0x, double i0y, out int i1)
        {
            var minDist = double.PositiveInfinity;
            i1 = 0;
            for (int i = 0; i < n; i++)
            {
                if (i == _i0) continue;
                var d = GetDistance(i0x, i0y, coordArr[2 * i], coordArr[2 * i + 1]);
                if (d < minDist && d > 0)
                {
                    i1 = i;
                    minDist = d;
                }
            }
        }

        private void CalculateI0(double cx, double cy, out int i0)
        {
            i0 = 0;
            var minDist = double.PositiveInfinity;
            for (int i = 0; i < n; i++)
            {
                var d = GetDistance(cx, cy, coordArr[2 * i], coordArr[2 * i + 1]);
                if (d < minDist)
                {
                    i0 = i;
                    minDist = d;
                }
            }
        }

        private (double cx, double cy) InitializeTriaID()
        {
            var minX = double.PositiveInfinity;
            var minY = double.PositiveInfinity;
            var maxX = double.NegativeInfinity;
            var maxY = double.NegativeInfinity;

            for (var i = 0; i < n; i++)
            {
                var x = coordArr[2 * i];
                var y = coordArr[2 * i + 1];
                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
                triaID[i] = i;
            }

            var cx = (minX + maxX) / 2;
            var cy = (minY + maxY) / 2;
            return (cx, cy);
        }

        private void InitializeCoordinateHeap()
        {
            coordArr = _doublePool.Rent(n * 2);

            for (var i = 0; i < n; i++)
            {
                var p = _rawInternal.ElementAt(i);
                coordArr[2 * i] = p.X;
                coordArr[2 * i + 1] = p.Y;
            }
        }

        private void AddFirstTriangle()
        {
            hullStart = _i0;
            hullSize = 3;

            _hullNext[_i0] = _hullPrev[_i2] = _i1;
            _hullNext[_i1] = _hullPrev[_i0] = _i2;
            _hullNext[_i2] = _hullPrev[_i1] = _i0;

            _hullTri[_i0] = 0;
            _hullTri[_i1] = 1;
            _hullTri[_i2] = 2;


            _triaLen = 0;
            AddTriangle(_i0, _i1, _i2, -1, -1, -1);
        }

        private void InitializeHeap()
        {
            var maxTriangles = 2 * n - 5;
            _hashSize = (int)Math.Ceiling(Math.Sqrt(n));

            _trianglesHeap = _intPool.Rent(maxTriangles * 3);
            _halfedgesHeap = _intPool.Rent(maxTriangles * 3);
            _hullPrev = _intPool.Rent(n);
            _hullNext = _intPool.Rent(n);
            _hullTri = _intPool.Rent(n);
            _hullHash = _intPool.Rent(_hashSize);
            triaID = _intPool.Rent(n);
        }

        private void InitializeLookupTables(int n)
        {
            double xp = 0;
            double yp = 0;
            for (var k = 0; k < n; k++)
            {
                var i = triaID[k];
                var x = coordArr[2 * i];
                var y = coordArr[2 * i + 1];

                // skip near-duplicate points
                if (k > 0 && Math.Abs(x - xp) <= _minDelta && Math.Abs(y - yp) <= _minDelta) continue;
                xp = x;
                yp = y;

                // skip seed triangle points
                if (i == _i0 || i == _i1 || i == _i2) continue;

                // find a visible edge on the convex hull using edge hash
                var start = 0;
                for (var j = 0; j < _hashSize; j++)
                {
                    var key = HashKey(x, y);
                    start = _hullHash[(key + j) % _hashSize];
                    if (start != -1 && start != _hullNext[start]) break;
                }


                start = _hullPrev[start];
                var e = start;
                var q = _hullNext[e];

                while (!Orient(x, y, coordArr[2 * e], coordArr[2 * e + 1], coordArr[2 * q], coordArr[2 * q + 1]))
                {
                    e = q;
                    if (e == start)
                    {
                        e = int.MaxValue;
                        break;
                    }

                    q = _hullNext[e];
                }

                if (e == int.MaxValue) continue; // likely a near-duplicate point; skip it

                // add the first triangle from the point
                var t = AddTriangle(e, i, _hullNext[e], -1, -1, _hullTri[e]);

                // recursively flip triangles from the point until they satisfy the Delaunay condition
                _hullTri[i] = Legalize(t + 2);
                _hullTri[e] = t; // keep track of boundary triangles on the hull
                hullSize++;

                // walk forward through the hull, adding more triangles and flipping recursively
                var next = _hullNext[e];
                q = _hullNext[next];

                while (Orient(x, y, coordArr[2 * next], coordArr[2 * next + 1], coordArr[2 * q], coordArr[2 * q + 1]))
                {
                    t = AddTriangle(next, i, q, _hullTri[i], -1, _hullTri[next]);
                    _hullTri[i] = Legalize(t + 2);
                    _hullNext[next] = next; // mark as removed
                    hullSize--;
                    next = q;

                    q = _hullNext[next];
                }

                // walk backward from the other side, adding more triangles and flipping
                if (e == start)
                {
                    q = _hullPrev[e];

                    while (Orient(x, y, coordArr[2 * q], coordArr[2 * q + 1], coordArr[2 * e], coordArr[2 * e + 1]))
                    {
                        t = AddTriangle(q, i, e, -1, _hullTri[e], _hullTri[q]);
                        Legalize(t + 2);
                        _hullTri[q] = t;
                        _hullNext[e] = e; // mark as removed
                        hullSize--;
                        e = q;

                        q = _hullPrev[e];
                    }
                }

                // update the hull indices
                hullStart = _hullPrev[i] = e;
                _hullNext[e] = _hullPrev[next] = i;
                _hullNext[i] = next;

                // save the two new edges in the hash table
                _hullHash[HashKey(x, y)] = i;
                _hullHash[HashKey(coordArr[2 * e], coordArr[2 * e + 1])] = e;
            }
        }

        #region Internals
        private int Legalize(int a)
        {
            var i = 0;
            int ar;

            // recursion eliminated with a fixed-size stack
            while (true)
            {
                var b = _halfedgesHeap[a];

                int a0 = a - a % 3;
                ar = a0 + (a + 2) % 3;

                if (b == -1)
                { // convex hull edge
                    if (i == 0) break;
                    a = _edgeHeap[--i];
                    continue;
                }

                var b0 = b - b % 3;
                var al = a0 + (a + 1) % 3;
                var bl = b0 + (b + 2) % 3;

                var p0 = _trianglesHeap[ar];
                var pr = _trianglesHeap[a];
                var pl = _trianglesHeap[al];
                var p1 = _trianglesHeap[bl];

                var illegal = InCircle(
                    coordArr[2 * p0], coordArr[2 * p0 + 1],
                    coordArr[2 * pr], coordArr[2 * pr + 1],
                    coordArr[2 * pl], coordArr[2 * pl + 1],
                    coordArr[2 * p1], coordArr[2 * p1 + 1]);

                if (illegal)
                {
                    _trianglesHeap[a] = p1;
                    _trianglesHeap[b] = p0;

                    var hbl = _halfedgesHeap[bl];

                    // edge swapped on the other side of the hull (rare); fix the halfedge reference
                    if (hbl == -1)
                    {
                        var e = hullStart;
                        int barrier = n;
                        do
                        {
                            if (_hullTri[e] == bl)
                            {
                                _hullTri[e] = a;
                                break;
                            }
                            e = _hullNext[e];
                            barrier--;
                            if (barrier < 0) break;
                        } while (e != hullStart);
                    }
                    Link(a, hbl);
                    Link(b, _halfedgesHeap[ar]);
                    Link(ar, bl);

                    var br = b0 + (b + 1) % 3;

                    // don't worry about hitting the cap: it can only happen on extremely degenerate input
                    if (i < _edgeHeap.Length)
                    {
                        _edgeHeap[i++] = br;
                    }
                }
                else
                {
                    if (i == 0) break;
                    a = _edgeHeap[--i];
                }
            }

            return ar;
        }
        private bool InCircle(double ax, double ay, double bx, double by, double cx, double cy, double px, double py)
        {
            var dx = ax - px;
            var dy = ay - py;
            var ex = bx - px;
            var ey = by - py;
            var fx = cx - px;
            var fy = cy - py;

            var ap = dx * dx + dy * dy;
            var bp = ex * ex + ey * ey;
            var cp = fx * fx + fy * fy;

            return dx * (ey * cp - bp * fy) -
                   dy * (ex * cp - bp * fx) +
                   ap * (ex * fy - ey * fx) < 0;
        }
        private int AddTriangle(int i0, int i1, int i2, int a, int b, int c)
        {
            var t = _triaLen;

            _trianglesHeap[t] = i0;
            _trianglesHeap[t + 1] = i1;
            _trianglesHeap[t + 2] = i2;

            Link(t, a);
            Link(t + 1, b);
            Link(t + 2, c);

            _triaLen += 3;
            return t;
        }
        private void Link(int a, int b)
        {
            _halfedgesHeap[a] = b;
            if (b != -1) _halfedgesHeap[b] = a;
        }
        private int HashKey(double x, double y) => (int)(Math.Floor(PseudoAngle(x - _cx, y - _cy) * _hashSize) % _hashSize);
        private double PseudoAngle(double dx, double dy)
        {
            // monotonically increases with real angle, but doesn't need expensive trigonometry
            var p = dx / (Math.Abs(dx) + Math.Abs(dy));
            return (dy > 0 ? 3 - p : 1 + p) / 4; // in [0..1]
        }

        private bool Orient(double px, double py, double qx, double qy, double rx, double ry) => (qy - py) * (rx - qx) - (qx - px) * (ry - qy) < 0;
        private double CircumRadius(double ax, double ay, double bx, double by, double cx, double cy)
        {
            var dx = bx - ax;
            var dy = by - ay;
            var ex = cx - ax;
            var ey = cy - ay;
            var bl = dx * dx + dy * dy;
            var cl = ex * ex + ey * ey;
            var d = 0.5 / (dx * ey - dy * ex);
            var x = (ey * bl - dy * cl) * d;
            var y = (dx * cl - ex * bl) * d;
            return x * x + y * y;
        }
        private Point CircumCenter(double ax, double ay, double bx, double by, double cx, double cy)
        {
            var dx = bx - ax;
            var dy = by - ay;
            var ex = cx - ax;
            var ey = cy - ay;
            var bl = dx * dx + dy * dy;
            var cl = ex * ex + ey * ey;
            var d = 0.5 / (dx * ey - dy * ex);
            var x = ax + (ey * bl - dy * cl) * d;
            var y = ay + (dx * cl - ex * bl) * d;

            return new Point((float)x, (float)y);
        }
        private double GetDistance(double ax, double ay, double bx, double by)
        {
            var dx = ax - bx;
            var dy = ay - by;
            return dx * dx + dy * dy;
        }
        private static void Quicksort(int[] ids, double[] dists, int left, int right)
        {
            if (right - left <= 20)
            {
                for (var i = left + 1; i <= right; i++)
                {
                    var temp = ids[i];
                    var tempDist = dists[temp];
                    var j = i - 1;
                    while (j >= left && dists[ids[j]] > tempDist) ids[j + 1] = ids[j--];
                    ids[j + 1] = temp;
                }
            }
            else
            {
                var median = (left + right) >> 1;
                var i = left + 1;
                var j = right;
                Swap(ids, median, i);
                if (dists[ids[left]] > dists[ids[right]]) Swap(ids, left, right);
                if (dists[ids[i]] > dists[ids[right]]) Swap(ids, i, right);
                if (dists[ids[left]] > dists[ids[i]]) Swap(ids, left, i);

                var temp = ids[i];
                var tempDist = dists[temp];
                while (true)
                {
                    do i++; while (dists[ids[i]] < tempDist);
                    do j--; while (dists[ids[j]] > tempDist);
                    if (j < i) break;
                    Swap(ids, i, j);
                }
                ids[left + 1] = ids[j];
                ids[j] = temp;

                if (right - i + 1 >= j - left)
                {
                    Quicksort(ids, dists, i, right);
                    Quicksort(ids, dists, left, j - 1);
                }
                else
                {
                    Quicksort(ids, dists, left, j - 1);
                    Quicksort(ids, dists, i, right);
                }
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Swap(int[] arr, int i, int j)
        {
            var tmp = arr[i];
            arr[i] = arr[j];
            arr[j] = tmp;
        }
        #endregion  

        #region API
        public IEnumerable<ITriangle> GetTriangles()
        {
            for (var t = 0; t < _trianglesHeap.Length / 3; t++)
            {
                yield return new Triangle(t, GetTrianglePoints(t));
            }
        }
        public IEnumerable<IEdge> GetEdges()
        {
            for (var e = 0; e < _trianglesHeap.Length; e++)
            {
                if (e > _halfedgesHeap[e])
                {
                    var p = _rawInternal.ElementAt(_trianglesHeap[e]);
                    var q = _rawInternal.ElementAt(_trianglesHeap[NextHalfedge(e)]);
                    yield return new Edge(e, p, q);
                }
            }
        }
        public IEnumerable<IEdge> GetVoronoiEdges()
        {
            for (var e = 0; e < _trianglesHeap.Length; e++)
            {
                if (e < _halfedgesHeap[e])
                {
                    var p = GetTriangleCenter(TriangleOfEdge(e));
                    var q = GetTriangleCenter(TriangleOfEdge(_halfedgesHeap[e]));
                    yield return new Edge(e, p, q);
                }
            }
        }
        public IEnumerable<IVoronoiCell> GetVoronoiCells()
        {
            var seen = new HashSet<int>();  // of point ids
            for (var triangleId = 0; triangleId < _trianglesHeap.Length; triangleId++)
            {
                var id = _trianglesHeap[NextHalfedge(triangleId)];
                if (seen.All(x => x != id))
                {
                    seen.Add(id);
                    var edges = EdgesAroundPoint(triangleId);
                    var triangles = edges.Select(TriangleOfEdge);
                    var vertices = triangles.Select(GetTriangleCenter);
                    yield return new VoronoiCell(id, vertices);
                }
            }
        }
        public IEnumerable<IEdge> GetHullEdges() =>
            CreateHull(GetHullPoints());
        public IEnumerable<IPoint> GetHullPoints() =>
            _hull.Select(_rawInternal.ElementAt);
        public IEnumerable<IPoint> GetTrianglePoints(int t) =>
            PointsOfTriangle(t).Select(_rawInternal.ElementAt);
        public IEnumerable<IPoint> GetRellaxedPoints() =>
            GetVoronoiCells().Select(x => GetCentroid(x.Points));
        public IEnumerable<IEdge> GetEdgesOfTriangle(int t) =>
            CreateHull(EdgesOfTriangle(t).Select(_rawInternal.ElementAt));
        public IEnumerable<int> EdgesAroundPoint(int start)
        {
            var incoming = start;
            do
            {
                yield return incoming;
                var outgoing = NextHalfedge(incoming);
                incoming = _halfedgesHeap[outgoing];
            } while (incoming != -1 && incoming != start);
        }
        public IEnumerable<int> PointsOfTriangle(int t) => EdgesOfTriangle(t).Select(e => _trianglesHeap[e]);
        public IEnumerable<int> TrianglesAdjacentToTriangle(int t)
        {
            var adjacentTriangles = new List<int>();
            var triangleEdges = EdgesOfTriangle(t);
            foreach (var e in triangleEdges)
            {
                var opposite = _halfedgesHeap[e];
                if (opposite >= 0)
                {
                    adjacentTriangles.Add(TriangleOfEdge(opposite));
                }
            }
            return adjacentTriangles;
        }
        public int NextHalfedge(int e) => (e % 3 == 2) ? e - 2 : e + 1;
        public int PreviousHalfedge(int e) => (e % 3 == 0) ? e + 2 : e - 1;
        public int[] EdgesOfTriangle(int t) => new[] { 3 * t, 3 * t + 1, 3 * t + 2 };
        public int TriangleOfEdge(int e) => (int)Math.Floor(e * 1.0 / 3);
        public IEnumerable<IEdge> CreateHull(IEnumerable<IPoint> points)
        {
            var list = points as IPoint[] ?? points.ToArray();
            return list.Zip(
                    list.Skip(1).Append(list.First()),
                    (a, b) => new Edge(0, a, b))
                .OfType<IEdge>();
        }
        public IPoint GetTriangleCenter(int t)
        {
            var vertices = GetTrianglePoints(t).ToArray();
            return GetCircumcenter(vertices.ElementAt(0), vertices.ElementAt(1), vertices.ElementAt(2));
        }
        public IPoint GetCircumcenter(IPoint a, IPoint b, IPoint c)
        {
            var ad = a.X * a.X + a.Y * a.Y;
            var bd = b.X * b.X + b.Y * b.Y;
            var cd = c.X * c.X + c.Y * c.Y;
            var D = 2 * (a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y));
            return new Point(
                1 / D * (ad * (b.Y - c.Y) + bd * (c.Y - a.Y) + cd * (a.Y - b.Y)),
                1 / D * (ad * (c.X - b.X) + bd * (a.X - c.X) + cd * (b.X - a.X)));
        }
        public IPoint GetCentroid(IEnumerable<IPoint> points)
        {
            double accumulatedArea = 0.0f;
            double centerX = 0.0f;
            double centerY = 0.0f;

            var list = points as IPoint[] ?? points.ToArray();
            for (int i = 0, j = list.Length - 1; i < list.Length; j = i++)
            {
                var temp = list.ElementAt(i).X * list.ElementAt(j).Y - list.ElementAt(j).X * list.ElementAt(i).Y;
                accumulatedArea += temp;
                centerX += (list.ElementAt(i).X + list.ElementAt(j).X) * temp;
                centerY += (list.ElementAt(i).Y + list.ElementAt(j).Y) * temp;
            }

            if (Math.Abs(accumulatedArea) < 1E-7f)
                return new Point();

            accumulatedArea *= 3f;
            return new Point((float)(centerX / accumulatedArea), (float)(centerY / accumulatedArea));
        }

        #endregion 

        public void Dispose()
        {
            _trianglesHeap = default;
            _halfedgesHeap = default;
            _edgeHeap = default;
            if (_hull != default)
                _intPool.Return(_hull, true);
            if (coordArr != default)
                _doublePool.Return(coordArr, true);
            if (_hullPrev != default)
                _intPool.Return(_hullPrev, true);
            if (_hullNext != default)
                _intPool.Return(_hullNext, true);
            if (_hullHash != default)
                _intPool.Return(_hullHash, true);
            if (triaID != default)
                _intPool.Return(triaID, true);
        }
    }
}