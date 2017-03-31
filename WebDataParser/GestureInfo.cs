using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace WebDataParser {
    public class GestureInfo {
        public string HitData { get; set; }
        public string TimeData { get; set; }
        public MemoryStream Img { get; set; }
        public int PracticeTime { get; set; }
        public int TotalTime { get; set; }
        public float HitPercentage { get; set; }
    }
}