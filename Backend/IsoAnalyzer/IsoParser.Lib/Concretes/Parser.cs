using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IsoParser.Lib.Services;
using IsoParser.Lib.Models;

namespace IsoParser.Lib.Concretes {
	public class Parser : IParser {
		public async Task<List<Atom>> GetTree(string path) {
			return await Task.Run(() => new Atom[] {
					new Atom {
						Id = (int) AtomType.FTYP,
						Type = AtomType.FTYP,
						Size = 16,
						Offset = 0,
						Items = new Item[] {
							new Item { Name = "Company", Type = ItemType.String, Value = "MyCompanly" },
							new Item { Name = "Employees", Type = ItemType.Int, Value = 567 }
						}.ToList(),
						Atoms = new Atom[] {
							new Atom {
								Id = (int) AtomType.VMHD,
								Type = AtomType.VMHD,
								Size = 100,
								Offset = 32,
								Items = new Item[] {
									new Item { Name = "Codec", Type = ItemType.String, Value = "AVC" },
									new Item { Name = "Bitrate", Type = ItemType.Double, Value = 2048000.5 },
								}.ToList()
							},
							new Atom {
								Id = (int) AtomType.SMHD,
								Type = AtomType.SMHD,
								Size = 200,
								Offset = 9120,
								Items = new Item[] {
									new Item { Name = "Codec", Type = ItemType.String, Value = "MPL" },
									new Item { Name = "Channels", Type = ItemType.Int, Value = 6 },
								}.ToList()
							}
						}.ToList()
					}
				}.ToList()
			);
		}

		public int StringInt(string type) {
			return type.Select(c => (int)c).Aggregate(0, (x, y) => (x << 8) + y);
		}
		public int ByteInt(byte [] data, int offset) {
			return data.Skip(offset).Take(4).ToArray().Aggregate(0, (x, y) => (x << 8) + y);
		}
	}
}
