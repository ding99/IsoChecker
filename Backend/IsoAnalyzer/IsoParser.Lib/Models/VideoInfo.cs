using System.Collections.Generic;

namespace IsoParser.Lib.Models
{
    public class VideoInfo
    {
        public List<Video> Videos { get; set; }

        public VideoInfo ()
        {
            this.Videos = new ();
        }
    }
}
