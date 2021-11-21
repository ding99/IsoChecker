using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IsoParser.Lib.Models {
	public class Atom {
		public string Id { get; set; }
		public long Size { get; set; }
		public long Offset { get; set; }

		public List<Item> Items { get; set; }
		public List<Atom> Atoms { get; set; }
	}

	public class Item {
		public string Name { get; set; }
		public ItemType Type { get; set; }
		public object Value { get; set; }
	}

	public enum ItemType {
		None = 0,
		Byte,
		Short,
		Int,
		Long,
		Double,
		Bool,
		String
	}
}
