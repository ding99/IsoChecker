using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IsoParser.Lib.Models {
	public class Atom {
		public String Id { get; set; }
		public long Size { get; set; }

		public List<Atom> Atoms { get; set; }
	}
}
