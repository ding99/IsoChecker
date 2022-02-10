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

        //public ClosedCaption CC1 { get; set; }
        //public ClosedCaption CC2 { get; set; }
        //public ClosedCaption CC3 { get; set; }
        //public ClosedCaption CC4 { get; set; }

        public Subtitle ()
        {
            this.Notes = new ();
            this.Boxes = new ();
            this.Frames = new ();
            //this.CC1 = new ();
            //this.CC2 = new ();
            //this.CC3 = new ();
            //this.CC4 = new ();
        }
    }

    public class ClosedCaption
    {
        public List<byte[]> Frames { get; set; }
        public List<string> Boxes { get; set; }
        public ClosedCaption ()
        {
            this.Boxes = new ();
            this.Frames = new ();
        }
    }
}
