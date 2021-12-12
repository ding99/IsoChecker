using System.Collections.Generic;

namespace IsoParser.Api.Models
{
    public class DisplayAtom
    {
		public int Id { get; set; }
		public string Type { get; set; }
		public long Offset { get; set; }
		public long Size { get; set; }

		public List<DisplayItem> Items { get; set; }
		public List<DisplayAtom> Atoms { get; set; }

		public DisplayAtom () { }

		public DisplayAtom (int id, string type, long size, long offset)
		{
			this.Id = id;
			this.Type = type;
			this.Size = size;
			this.Offset = offset;
		}
	}
}
