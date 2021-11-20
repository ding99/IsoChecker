using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IsoParser.Lib.Services;
using IsoParser.Lib.Models;

namespace IsoParser.Lib.Concretes {
	class IsoParser : IIsoParser {
		public async Task<List<Atom>> GetTree(string path) {
			List<Atom> atoms = new List<Atom>();
			return atoms;
		}
	}
}
