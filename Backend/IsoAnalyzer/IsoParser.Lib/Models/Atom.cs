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

		public int GetInt(string type) {
			return type.Select(c => (int)c).Aggregate(0, (x, y) => (x << 8) + y);
		}
	}

	public enum AtomType {
	}
}
