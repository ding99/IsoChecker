using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IsoParser.Lib.Services;
using IsoParser.Lib.Models;
using IsoParser.Lib.Tools;

namespace IsoParser.Lib.Concretes {
	public class Parser : IParser {
		private BinFile file;

		public void End () {
			if (this.file != null)
				this.file.End ();
        }

		public async Task<Atom> GetTree (string path) {
			this.file = new BinFile (path);
			Atom atom = await this.GetAtom ();

			this.file.End ();

			return atom;
        }

		public async Task<byte []> GetData (string path, long offset, int size) {
			//byte [] a = new byte [0];
			return await Task.Run (() => new byte [size]);
			//return a;
        }

		public async Task<Atom> GetAtom () {

			//await Task.Run (() => {
			//	Atom atom = new (0x31303230, this.file.FileSize (), 0L);
			//	return atom;
			//});

			return await Task.Run ( () => new Atom (0x31303230, 0x800, 0) {
				Atoms = new Atom [] {
					new Atom ((int) AtomType.FTYP, 16, 0) {
						Type = AtomType.FTYP,
						Items = new Item [] {
							new Item { Name = "Company", Type = ItemType.String, Value = "MyCompanly" },
							new Item { Name = "Employees", Type = ItemType.Int, Value = 567 }
						}.ToList(),
						Atoms = new Atom [] {
							new Atom ((int) AtomType.VMHD, 100, 32) {
								Type = AtomType.VMHD,
								Items = new Item [] {
									new Item { Name = "Codec", Type = ItemType.String, Value = "AVC" },
									new Item { Name = "Bitrate", Type = ItemType.Double, Value = 2048000.5 },
								}.ToList ()
							},
							new Atom ((int) AtomType.SMHD, 200,	9120) {
								Type = AtomType.SMHD,
								Items = new Item [] {
									new Item { Name = "Codec", Type = ItemType.String, Value = "MPL" },
									new Item { Name = "Channels", Type = ItemType.Int, Value = 6 },
								}.ToList ()
							}
						}.ToList()
					}
				}.ToList ()
			}
			);
		}

		public int StringInt (string type) {
			return type.Select (c => (int)c).Aggregate (0, (x, y) => (x << 8) + y);
		}
		public int ByteInt (byte [] data, int offset) {
			return data.Skip (offset).Take (4).ToArray ().Aggregate (0, (x, y) => (x << 8) + y);
		}
	}
}
