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

        public Subtitle ()
        {
            this.Notes = new ();
            this.Boxes = new ();
            this.Frames = new ();
        }
    }
}
