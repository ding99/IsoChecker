using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IsoParser.Lib.Models
{
    public class IsoInfo
    {
        public Atom Atom { get; set; }
        public VideoInfo Video { get; set; }
        public AudioInfo Audio { get; set; }
        public SubtitleInfo Subtitle { get; set; }
    }
}
