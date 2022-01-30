using System.Collections.Generic;

namespace IsoParser.Lib.Models
{
    public class AudioInfo
    {
        public List<Audio> Audios { get; set; }

        public AudioInfo ()
        {
            this.Audios = new ();
        }
    }
}
