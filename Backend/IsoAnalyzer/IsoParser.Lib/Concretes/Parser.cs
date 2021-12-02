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
		private long fileSize;

		private readonly HashSet<AtomType> containers;

		#region movie variable
		private int? timeScale;
		#endregion

		#region public
		public Parser () {
			this.file = null;
			this.containers = new HashSet<AtomType> {
				AtomType.CLIP,
				AtomType.DINF,
				AtomType.EDTS,
				AtomType.GMHD,
				AtomType.IMAP,
				AtomType.IN,
				AtomType.MATT,
				AtomType.META,
				AtomType.MDIA,
				AtomType.MINF,
				AtomType.MOOV,
				AtomType.STBL,
				AtomType.TRAK,
				AtomType.UDTA
			};
			this.containers = new HashSet<AtomType> {
				AtomType.CLIP
			};
		}

		public void End () {
			if (this.file != null)
				this.file.End ();
        }

		public async Task<Atom> GetTree (string path) {
			this.file = new BinFile (path);
			this.fileSize = this.file.FileSize ();

			Atom atom = new ();
			await Task.Run (() => {
				// Root atom id is always 1
				atom = this.GetAtom (1, this.file.FileSize (), 0L, 0);

				#region test data
				//atom = new Atom (0x31303230, 0x800, 0) {
				//	Atoms = new Atom [] {
				//	new Atom ((int) AtomType.FTYP, 16, 0) {
				//		Type = AtomType.FTYP,
				//		Items = new Item [] {
				//			new Item { Name = "Company", Type = ItemType.String, Value = "MyCompanly" },
				//			new Item { Name = "Employees", Type = ItemType.Int, Value = 567 }
				//		}.ToList(),
				//		Atoms = new Atom [] {
				//			new Atom ((int) AtomType.VMHD, 100, 32) {
				//				Type = AtomType.VMHD,
				//				Items = new Item [] {
				//					new Item { Name = "Codec", Type = ItemType.String, Value = "AVC" },
				//					new Item { Name = "Bitrate", Type = ItemType.Double, Value = 2048000.5 },
				//				}.ToList ()
				//			},
				//			new Atom ((int) AtomType.SMHD, 200, 9120) {
				//				Type = AtomType.SMHD,
				//				Items = new Item [] {
				//					new Item { Name = "Codec", Type = ItemType.String, Value = "MPL" },
				//					new Item { Name = "Channels", Type = ItemType.Int, Value = 6 },
				//				}.ToList ()
				//			}
				//		}.ToList()
				//	}
				//}.ToList ()
				//};
				#endregion test data
			});

			this.file.End ();

			return atom;
        }


		public async Task<byte []> GetData (string path, long offset, int size) {
			//byte [] a = new byte [0];
			return await Task.Run (() => new byte [size]);
			//return a;
		}
		#endregion public

		#region atom utilities
		private Atom GetAtom (int id, long size, long offset, int head) {
			Console.WriteLine ($"id {id:x}, size {size:x}, offset {offset:x}, head {head:x}");
			Atom atom = new (id, size, offset, head);

			if (atom.Type.HasValue && !this.isContainer ((AtomType)atom.Type))
				atom.Items = this.Parse (atom);

			List<Atom> atoms = new ();
			bool valid = true;

			long si;
			for (long ip = offset + head; ip < offset + size - head; ip += si) {
				byte [] buffer = this.file.Read (8, ip);
				if (buffer.Length < 1) {
					valid = false;
					break;
				}

				int atomId = ByteInt (buffer, 4);
				int atomHead = 8;

				if (this.ValidId (atomId)) {
					//				if (this.valid) {
					//					if (this.valid && this.ValidId (atomId)) {
					switch (si = this.ByteInt (buffer, 0)) {
					case 0:
						si = this.fileSize - ip;
						break;
					case 1:
						buffer = this.file.Read (8);
						if (buffer.Length < 1) {
							valid = false;
							break;
						} else {
							si = this.ByteLong (buffer, 0);
							atomHead += 8;
						}
						break;
					}

				} else {
					atomId = 0;
					atomHead = 0;
					si = size - ip;
					valid = false;
				}

				Console.WriteLine ($"  atomId {atomId:x}, si {si:x}, ip {ip:x}, atomHead {atomHead:x}");

				if (Enum.IsDefined (typeof (AtomType), atomId)) {
					Console.WriteLine ($"-- Found Atom [{(AtomType)atomId}]");
					Atom newAtom = GetAtom (atomId, si, ip, atomHead);
					atoms.Add (newAtom);
				}

				if (!valid)
					break;
			}

			if (atoms.Count > 0)
				atom.Atoms = atoms;

			return atom;
		}

		private List<Item> Parse(Atom atom) {
            switch (atom.Type) {
			case AtomType.MVHD:
				return ParseMvhd (atom);
			case AtomType.ELST:
				return ParseElst (atom);
			case AtomType.HDLR:
				return ParseHdlr (atom);
			}

			return Array.Empty <Item> ().ToList ();
        }

		private List<Item> ParseMvhd(Atom atom) {
			if (atom.Size < 108)
				return Array.Empty <Item> ().ToList ();

			byte [] buffer = this.file.Read ((int)atom.Size, atom.Offset);
			if (buffer.Length < 24)
				return Array.Empty <Item> ().ToList ();

			this.timeScale = ByteInt (buffer, 20);
			List<Item> items = new ();
			items.Add (new Item {
				Name = "TimeScale",
				Type = ItemType.Int,
				Value = this.timeScale
			});
			return items;
        }

		private List<Item> ParseElst(Atom atom) {
			byte [] buffer = this.file.Read ((int)atom.Size, atom.Offset);
			if(buffer.Length >= (int)atom.Size) {
				List<Item> items = new ();

				int count = ByteInt (buffer, 12);
				items.Add (new Item { Name = "Entries", Type = ItemType.Int, Value = count });

				for(int i = 0; i < count; i++) {
					items.Add (new Item { Name = "TrackDuration", Type = ItemType.Int, Value = ByteInt (buffer, 16 + 12 * i) });
					items.Add (new Item { Name = "MediaTime", Type = ItemType.Int, Value = ByteInt (buffer, 20 + 12 * i) });
					items.Add (new Item { Name = "MediaRate", Type = ItemType.Int, Value = ByteInt (buffer, 24 + 12 * i) });
					if (this.timeScale.HasValue && this.timeScale != 0)
						items.Add (new Item {
							Name = "DurationSec",
							Type = ItemType.Double,
							Value = (double)ByteInt (buffer, 16 + 12 * i) / (double)this.timeScale
						});
				}

				return items;
			}
			return Array.Empty <Item> ().ToList ();
        }

		private List<Item> ParseHdlr (Atom atom) {
			byte [] buffer = this.file.Read ((int)atom.Size, atom.Offset);
			if (buffer.Length >= (int)atom.Size) {
				List<Item> items = new ();

				items.Add (new Item { Name = "ComponentType", Type = ItemType.String, Value = this.IntString(buffer, 12) });
				items.Add (new Item { Name = "ComponentSubType", Type = ItemType.String, Value = this.IntString (buffer, 16) });

				return items;
			}
			return Array.Empty<Item> ().ToList ();
		}
		#endregion atom utilities

		#region common utilities
		private bool isContainer(AtomType type) {
			return this.containers.Contains (type);
        }

		public int StringInt (string type) {
			return type.Select (c => (int)c).Aggregate (0, (x, y) => (x << 8) + y);
		}
		public int ByteInt (byte [] data, int offset) {
			return data.Skip (offset).Take (4).ToArray ().Aggregate (0, (x, y) => (x << 8) + y);
		}

		private long ByteLong (byte [] data, int offset) {
			return (long)ByteInt (data, offset) << 32 + ByteInt (data, offset + 4);
        }

		private string IntString(byte [] data, int offset) {
			return data.Skip (offset).Take (4).ToArray ().Aggregate ("", (x, y) => x + Convert.ToChar (y));
        }

		private bool ValidId (int id) {
			return this.ValidByte (id >> 24)
				&& this.ValidByte (id >> 16)
				&& this.ValidByte (id >> 8)
				&& this.ValidByte (id);
        }

		private bool ValidByte (int id) {
			return (id & 255) > 0x1f;
        }
		#endregion common utilities
	}
}
