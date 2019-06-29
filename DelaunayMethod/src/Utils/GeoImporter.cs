using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibDelaunay.Algorithm.Interfaces;
using LibDelaunay.Algorithm.Models;

namespace DelaunayMethod.Utils
{
    public class GeoPoint : IPoint
    {
        public readonly float Lon;
        public readonly float Lat;

        public GeoPoint(float lat, float lon)
        {
            Lat = lat;
            Lon = lon; 
        }

        public float GetX(float xMax) => (float)((180 + Lon) * (xMax * 1.0 / 360));
        public float GetY(float yMax) => (float)((90 - Lat) * (yMax * 1.0 / 180));
        public float X { get; set; }
        public float Y { get; set; }
    }
    public class GeoImporter
    {
        /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
        /// <exception cref="T:System.OutOfMemoryException">There is insufficient memory to allocate a buffer for the returned string.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid, (for example, it is on an unmapped drive).</exception>
        /// <exception cref="T:System.IO.FileNotFoundException">The file specified in <paramref name="fileNameCsv" /> was not found.</exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="T:System.OverflowException"></exception>
        public static IEnumerable<GeoPoint> ImportCsv(string fileNameCsv)
        {
            if (!File.Exists(fileNameCsv))
                yield break;
            using (var f = File.OpenText(fileNameCsv))
            {
                string line;
                bool headerCheck = false;
                while ((line = f.ReadLine()) != default)
                {
                    var u = line.Split(',').Select(x => x.Trim()).ToArray();
                    if (!headerCheck && !char.IsDigit(u[0].First()))
                    {
                        headerCheck = true;
                        continue;
                    }
                    if (u[0].Length == 0 || u[1].Length == 0) continue;
                    yield return new GeoPoint(float.Parse(u[0]), float.Parse(u[1]));
                }
            }
        }

        public static IPoint ProjectLatLonToXY(GeoPoint x, float xmax, float ymax)
        {
            return new Point(
                 (float)((180 + x.Lon) * (xmax * 1.0 / 360)),
                 (float)((90 - x.Lat) * (ymax * 1.0 / 180)));
        }
    }
}