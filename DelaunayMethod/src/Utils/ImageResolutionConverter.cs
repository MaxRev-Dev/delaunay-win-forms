using System;
using System.Collections.Generic;
using System.Linq;

namespace DelaunayMethod.Utils
{
    public enum AspectRatio
    {
        _4_3,
        _16_10,
        _16_9,
        _1_1,
    }
    public class ImageResolutionConverter
    {
        private readonly List<string> _aratioList;
        private readonly List<string> _resolutionRaw;

        public ImageResolutionConverter()
        {
            _aratioList = new List<string>
            {
                "4:3",
                "16:10",
                "16:9",
                "1:1",
            };
            _resolutionRaw = new List<string>
            {
                "640×480, 800×600, 960×720, 1024×768, 1280×960, 1400×1050, 1440×1080, 1600×1200, 1856×1392, 1920×1440, 2048×1536",
                "1280×800, 1440×900, 1680×1050, 1920×1200, 2560×1600",
                "1024×576, 1152×648, 1280×720, 1366×768, 1600×900, 1920×1080, 2560×1440, 3840×2160, 7680x4320",
            };
        }

        public IEnumerable<string> GetForRatio(AspectRatio ratio)
        {
            var r = (int)ratio;
            var rx = ParseRatio(_aratioList[r]);
            var expr = _resolutionRaw.Count > r ? _resolutionRaw[r] : string.Join(",", _resolutionRaw);
            return expr.Split(',')
                .Select(s =>
                    s.Trim().Split('x', '×').Select(int.Parse).ToArray())
                .Select(x => $"{x[0]}x{(int)Math.Round(x[0] * 1f / rx, MidpointRounding.ToEven)}");
        }
        public IEnumerable<float> GetRatios() =>
            _aratioList.Select(ParseRatio);

        private float ParseRatio(string arg)
        {
            var x = arg.Split(':').Select(g => g.Trim()).ToArray();
            return int.Parse(x[0]) * 1f / int.Parse(x[1]);
        }

        public IEnumerable<int> GetWidth(AspectRatio ratio)
        {
            var r = (int)ratio;
            var expr = _resolutionRaw.Count > r ? _resolutionRaw[r] : string.Join(",", _resolutionRaw);
            return expr.Split(',')
                .Select(s =>
                    s.Trim().Split('x').Select(int.Parse).First()).Distinct();
        }
    }
}