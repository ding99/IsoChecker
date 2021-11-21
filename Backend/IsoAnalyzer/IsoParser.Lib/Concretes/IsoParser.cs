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
			return await Task.Run(() => new Atom[] {
					new Atom {
						Id = "head",
						Size = 16,
						Atoms = new Atom[] {
							new Atom { Id = "sub1", Size = 100 },
							new Atom { Id = "sub2", Size = 200 }
						}.ToList()
					}
				}.ToList()
			);
		}
	}
}
