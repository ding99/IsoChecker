using System.Collections.Generic;

namespace IsoParser.Lib.Models
{
    public class SubtitleInfo
    {
        public List<Subtitle> Subtitles { get; set; }

        public SubtitleInfo ()
        {
            this.Subtitles = new ();
        }
    }
}
