using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IsoParser.Lib.Models;

namespace IsoParser.Lib.Services {
	public interface IParser {
		Task<List<Atom>> GetTree(string path);
	}
}
