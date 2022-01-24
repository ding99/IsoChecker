namespace IsoParser.Lib.Models
{
    public class Track {
        public ComponentType Type { get; set; }
		public ComponentSubType SubType { get; set; }

		public Track ()
        {
			this.Type = ComponentType.Unknown;
			this.SubType = ComponentSubType.None;
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
}
