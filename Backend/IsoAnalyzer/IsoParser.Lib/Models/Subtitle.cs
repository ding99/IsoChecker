using System.Collections.Generic;

namespace IsoParser.Lib.Models
{
    public class Subtitle
    {
        public string Format { get; set; }
        public string Type { get; set; }
        public List<string> Notes { get; set; }
        public List<byte[]> Frames { get; set; }
        public List<string> Boxes { get; set; }

        public List<Segment> Segments { get; set; }

        public Subtitle ()
        {
            this.Format =  string.Empty;
            this.Type = string.Empty;

            this.Notes = new ();
            this.Frames = new ();
            this.Boxes = new ();

            this.Segments = new ();
        }
    }

    public class Segment
    {
        public string Start { get; set; }
        public List<int> Words { get; set; }

        public Segment ()
        {
            this.Start = string.Empty;
            this.Words = new ();
        }
    }
}
