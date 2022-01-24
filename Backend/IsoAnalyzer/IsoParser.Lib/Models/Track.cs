using System.Collections.Generic;

namespace IsoParser.Lib.Models
{
    public class Track {
        public ComponentType Type { get; set; }
		public ComponentSubType SubType { get; set; }
		public List<string> DataFormats { get; set; }  // stsd
		public List<TimeToSample> TimeToSamples { get; set; }  // stts
		public int SampleSize { get; set; }  // stsz
		public List<int> SampleSizes { get; set; }  // stsz
		public int SampleSizeCount { get; set; }  // stsz
		public List<SampleToChunk> SampleToChunks { get; set; }  // stsc
		public List<int> ChunkOffsets { get; set; }  // stco

		public Track ()
        {
			this.Type = ComponentType.Unknown;
			this.SubType = ComponentSubType.None;

			this.DataFormats = new ();
			this.TimeToSamples = new ();
			this.SampleSize = 0;
			this.SampleSizes = new ();
			this.SampleSizeCount = 0;
			this.SampleToChunks = new ();
			this.ChunkOffsets = new ();
        }
    }

	public enum ComponentType {
		Unknown = 0,
		Media = 0x6d686c72,  //mhlr, media handler
		Data = 0x64686c72,   //dhlr, data handler
	}

	public enum ComponentSubType {
		None = 0,
		Video = 0x76696465,  //vide, video data
		Sound = 0x736f756e,  //soun, sound data
		Subtitle = 0x73756274,  //subt, subtitles
		Caption = 0x636c6370,  //clcp, closed caption
		File = 0x616c6973,  //alis, data, file alias
		Meta = 0x6d647461,  //mdta, meta data, 
		Doc = 0x444f4320,  //'DOC ', meta data, 
	}

	public class TimeToSample
    {
		public int SampleCount { get; set; }
		public int SampleDuration { get; set; }
    }

	public class SampleToChunk
    {
		public int FirstChunk { get; set; }
		public int SamplesPerChunk { get; set; }
		public int DescriptionId { get; set; }
    }
}
