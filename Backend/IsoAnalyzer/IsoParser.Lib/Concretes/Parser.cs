﻿using System;
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
				atom = this.GetAtom (1, this.file.FileSize (), 0L, 0, new Track());

				#region test data
				//atom = new Atom (0x31303230, 0x800, 0) {
				//	Atoms = new Atom[] {
				//	new Atom ((int) AtomType.FTYP, 16, 0) {
				//		Type = AtomType.FTYP,
				//		Items = new Item[] {
				//			new Item { Name = "Company", Type = ItemType.String, Value = "MyCompanly" },
				//			new Item { Name = "Employees", Type = ItemType.Int, Value = 567 }
				//		}.ToList(),
				//		Atoms = new Atom[] {
				//			new Atom ((int) AtomType.VMHD, 100, 32) {
				//				Type = AtomType.VMHD,
				//				Items = new Item[] {
				//					new Item { Name = "Codec", Type = ItemType.String, Value = "AVC" },
				//					new Item { Name = "Bitrate", Type = ItemType.Double, Value = 2048000.5 },
				//				}.ToList ()
				//			},
				//			new Atom ((int) AtomType.SMHD, 200, 9120) {
				//				Type = AtomType.SMHD,
				//				Items = new Item[] {
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


		public async Task<byte[]> GetData (string path, long offset, int size) {
			//byte[] a = new byte[0];
			return await Task.Run (() => new byte[size]);
			//return a;
		}
		#endregion public

		#region atom utilities
		private Atom GetAtom (int id, long size, long offset, int head, Track track) {
			Console.WriteLine ($"id {id:x}, size {size:x}, offset {offset:x}, head {head:x}");
			Atom atom = new (id, size, offset, head);

			if (atom.Type.HasValue && !this.isContainer ((AtomType)atom.Type))
				atom.Items = this.Parse (atom, track);

			List<Atom> atoms = new ();
			bool valid = true;

			long si;
			for (long ip = offset + head; ip < offset + size - head; ip += si) {
				byte[] buffer = this.file.Read (8, ip);
				if (buffer.Length < 1)
					break;

				int atomId = this.ByteInt (buffer, 4);
				int atomHead = 8;

				if (this.ValidId (atomId)) {
					switch (si = this.ByteInt (buffer, 0)) {
					case 0:
						si = this.fileSize - ip;
						break;
					case 1:
						buffer = this.file.Read (8);
						if (buffer.Length < 1)
							valid = false;
						else {
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
					Console.WriteLine ($"-- Found Atom [{(AtomType)atomId}], subtype [{track.SubType}]");
					Atom newAtom = GetAtom (atomId, si, ip, atomHead, track);
					atoms.Add (newAtom);
				}

				if (!valid)
					break;
			}

			if (atoms.Count > 0)
				atom.Atoms = atoms;

			return atom;
		}

		private List<Item> Parse (Atom atom, Track track) {
            switch (atom.Type) {
			case AtomType.MVHD:
				return this.ParseMvhd (atom);
			case AtomType.TKHD:
				return this.ParseTkhd (atom);
				case AtomType.MDHD:
					return this.ParseMdhd (atom);
				case AtomType.ELST:
				return this.ParseElst (atom);
			case AtomType.HDLR:
				return this.ParseHdlr (atom, track);
			case AtomType.GMIN:
				return this.ParseGmin (atom);
			case AtomType.STSD:
				return this.ParseStsd (atom);
			case AtomType.STSS:
				return this.ParseStss (atom);
			case AtomType.STTS:
				return this.ParseStts (atom);
			case AtomType.STSZ:
				return this.ParseStsz (atom);
			case AtomType.STSC:
				return this.ParseStsc (atom);
			case AtomType.STCO:
				return this.ParseStco (atom, track);
			}

			return Array.Empty <Item> ().ToList ();
        }

		private List<Item> ParseAtom (Func<byte[], List<Item>> add, Atom atom) {
			byte[] buffer = this.file.Read ((int)atom.Size, atom.Offset);
			return buffer.Length >= (int)atom.Size ? add (buffer) : Array.Empty<Item> ().ToList ();
		}

		// TODO
		private List<Item> ParseMvhd(Atom atom) {
			return this.ParseAtom (buffer => {
				this.timeScale = this.ByteInt (buffer, 20);
				return new[] {
					new Item { Name = "Verison", Type = ItemType.Int, Value = buffer[8] },
					new Item { Name = "Flags", Type = ItemType.Int, Value = ByteInt (buffer, 8) & 0xfff },
					new Item { Name = "CreationTime", Type = ItemType.String, Value = this.ParseTime(buffer, 12, atom.Offset) },
					new Item { Name = "ModificationTime", Type = ItemType.String, Value = this.ParseTime(buffer, 16, atom.Offset) },
					new Item { Name = "TimeScale", Type = ItemType.Int, Value = this.timeScale },
					new Item { Name = "Duration", Type = ItemType.String, Value = this.ParseDuration (buffer, 24, this.timeScale) },
					new Item { Name = "PerferredRate", Type = ItemType.Int, Value = this.ByteInt (buffer, 28) },
					new Item { Name = "PreferredVolume", Type = ItemType.Short, Value = this.ByteShort (buffer, 32) }
				}.ToList ();
			}, atom);
        }

		// TODO
		private List<Item> ParseTkhd (Atom atom)
		{
			return this.ParseAtom (buffer => {
				return new[] {
					new Item { Name = "Verison", Type = ItemType.Int, Value = buffer[8] },
					new Item { Name = "Flags", Type = ItemType.Int, Value = this.ByteInt (buffer, 8) & 0xfff },
					new Item { Name = "FlagDetails", Type = ItemType.String, Value = this.ParseFlags ((uint)this.ByteInt (buffer, 8) & 0xfff) },
					new Item { Name = "CreationTime", Type = ItemType.String, Value = this.ParseTime(buffer, 12, atom.Offset) },
					new Item { Name = "ModificationTime", Type = ItemType.String, Value = this.ParseTime(buffer, 16, atom.Offset) },
					new Item { Name = "TrackID", Type = ItemType.Int, Value = this.ByteInt (buffer, 20) },
					new Item { Name = "Duration", Type = ItemType.String, Value = this.ParseDuration (buffer, 28, this.timeScale) },
					new Item { Name = "Layer", Type = ItemType.Short, Value = this.ByteShort (buffer, 40) },
					new Item { Name = "AlternateGroup", Type = ItemType.Short, Value = this.ByteShort (buffer, 42) },
					new Item { Name = "Volume", Type = ItemType.Short, Value = this.ByteShort (buffer, 44) },
					new Item { Name = "TrackWidth", Type = ItemType.Int, Value = this.ByteInt (buffer, 84) },
					new Item { Name = "TrackHeight", Type = ItemType.Int, Value = this.ByteInt (buffer, 88) }
				}.ToList ();
			}, atom);
		}
		private string ParseFlags(uint flags)
        {
			StringBuilder b = new ();
			bool start = true;

			if ( (flags & 8) > 0)
            {
				b.Append ("track is enabled");
				start = false;
            }
			if ((flags & 4) > 0)
			{
				if (start)
					start = false;
				else
					b.Append (", ");
				b.Append ("movie");
			}
			if ((flags & 2) > 0)
			{
				if (start)
					start = false;
				else
					b.Append (", ");
				b.Append ("movie's preview");
			}
			if ((flags & 1) > 0)
				b.Append ($"{(start ? "" : ", ")}movie's poster");

			return b.ToString ();
        }

		private List<Item> ParseMdhd (Atom atom)
		{
			return this.ParseAtom (buffer => {
				int timeScale = this.ByteInt (buffer, 20);
				return new[] {
					new Item { Name = "Verison", Type = ItemType.Int, Value = buffer[8] },
					new Item { Name = "Flags", Type = ItemType.Int, Value = this.ByteInt (buffer, 8) & 0xfff },
					new Item { Name = "CreationTime", Type = ItemType.String, Value = this.ParseTime(buffer, 12, atom.Offset) },
					new Item { Name = "ModificationTime", Type = ItemType.String, Value = this.ParseTime(buffer, 16, atom.Offset) },
					new Item { Name = "TimeScale", Type = ItemType.Int, Value = timeScale },
					new Item { Name = "Duration", Type = ItemType.String, Value = this.ParseDuration (buffer, 24, timeScale) },
					new Item { Name = "Language", Type = ItemType.Short, Value = this.ByteShort (buffer, 28) },
					new Item { Name = "Quality", Type = ItemType.Short, Value = this.ByteShort (buffer, 30) }
				}.ToList ();
			}, atom);
		}

		private List<Item> ParseElst(Atom atom) {
            return this.ParseAtom (buffer => {
				List<Item> items = new ();

				int count = this.ByteInt (buffer, 12);
				items.Add (new Item { Name = "Entries", Type = ItemType.Int, Value = count });

				for (int i = 0; i < count; i++) {
					items.Add (new Item { Name = "TrackDuration", Type = ItemType.Int, Value = this.ByteInt (buffer, 16 + 12 * i) });
					items.Add (new Item { Name = "MediaTime", Type = ItemType.Int, Value = this.ByteInt (buffer, 20 + 12 * i) });
					items.Add (new Item { Name = "MediaRate", Type = ItemType.Int, Value = this.ByteInt (buffer, 24 + 12 * i) });
					if (this.timeScale.HasValue && this.timeScale != 0)
						items.Add (new Item {
							Name = "DurationSec",
							Type = ItemType.Double,
							Value = (double) this.ByteInt (buffer, 16 + 12 * i) / (double)this.timeScale
						});
				}

				return items;
			}, atom);
        }

		private List<Item> ParseHdlr (Atom atom, Track track) {
			return this.ParseAtom (buffer => {
				int value = this.ByteInt(buffer, 12);
				if (Enum.IsDefined (typeof (ComponentType), value))
					track.Type = (ComponentType)value;

				Console.WriteLine ($"************ type {value:x}");
				value = this.ByteInt (buffer, 16);
                Console.WriteLine ($"************ type {track.Type} subtype {value:x}");
				if (Enum.IsDefined (typeof (ComponentSubType), value))
					track.SubType = (ComponentSubType)value;

				return new[] {
					new Item { Name = "ComponentType", Type = ItemType.String, Value = this.IntString (buffer, 12) },
					new Item { Name = "ComponentSubType", Type = ItemType.String, Value = this.IntString (buffer, 16) }
				}.ToList ();
			}, atom);
		}

		private List<Item> ParseGmin (Atom atom) {
			return this.ParseAtom (buffer => new[] {
				new Item { Name = "GraphicsMode", Type = ItemType.Short, Value = this.ByteShort (buffer, 12) }
			}.ToList(), atom);
		}

		private List<Item> ParseStsd (Atom atom) {
			return this.ParseAtom (buffer => {
				int size = (int)atom.Size;

				List<Item> items = new ();
				int count = this.ByteInt (buffer, 12);
				items.Add (new Item { Name = "Entries", Type = ItemType.Int, Value = count });

				int pos = 16;
				for (int i = 0; i < count && pos < size; i++) {
					int descriptionSize = this.ByteInt (buffer, pos);
					items.Add (new Item { Name = "DescriptionSize", Type = ItemType.Int, Value = descriptionSize });
					items.Add (new Item { Name = "DataFormat", Type = ItemType.String, Value = this.IntString (buffer, pos + 4) });
					items.Add (new Item { Name = "Index", Type = ItemType.Short, Value = this.ByteShort (buffer, pos + 14) });
					pos += descriptionSize;
				}

				return items;
			}, atom);
		}

		private List<Item> ParseStts (Atom atom) {
			return this.ParseAtom (buffer => new[] {
				new Item { Name = "Entries", Type = ItemType.Int, Value = this.ByteInt (buffer, 12) }
			}.ToList (), atom);
		}
		private List<Item> ParseStss (Atom atom) {
			return this.ParseAtom (buffer => new[] {
				new Item { Name = "Entries", Type = ItemType.Int, Value = this.ByteInt (buffer, 12) }
			}.ToList (), atom);
		}
		private List<Item> ParseStsz (Atom atom) {
			return this.ParseAtom (buffer => new[] {
				new Item { Name = "SampleSize", Type = ItemType.Int, Value = this.ByteInt (buffer, 12) },
				new Item { Name = "Entries", Type = ItemType.Int, Value = this.ByteInt (buffer, 16) }
			}.ToList (), atom);
		}
		private List<Item> ParseStsc (Atom atom) {
			return this.ParseAtom (buffer => new[] {
				new Item { Name = "Entries", Type = ItemType.Int, Value = this.ByteInt (buffer, 12) }
			}.ToList (), atom);
		}
		private List<Item> ParseStco (Atom atom, Track track) {
            Console.WriteLine ($"STCO type {track.Type}, subtype {track.SubType}");
			return this.ParseAtom (buffer => {
				List<Item> items = new ();
				int count = this.ByteInt (buffer, 12);
				items.Add (new Item { Name = "Entries", Type = ItemType.Int, Value = count });

				if(track.SubType == ComponentSubType.Caption) {
					for (int i = 0; i < count; i++)
						items.Add (new Item { Name = "Offset", Type = ItemType.Int, Value = this.ByteInt (buffer, 16 + 4 * i) });
                }

				return items;
			}, atom);
		}
		#endregion atom utilities

		#region common utilities
		private bool isContainer(AtomType type) {
			return this.containers.Contains (type);
        }

		public int StringInt (string type) {
			return type.Select (c => (int)c).Aggregate (0, (x, y) => (x << 8) + y);
		}
		public int ByteInt (byte[] data, int offset) {
			return data.Skip (offset).Take (4).ToArray ().Aggregate (0, (x, y) => (x << 8) + y);
		}

		public uint ByteUInt (byte[] data, int offset)
		{
			return data.Skip (offset).Take (4).ToArray ().Aggregate (0U, (x, y) => (x << 8) + y);
		}

		public short ByteShort (byte[] data, int offset) {
			return (short) (data[offset] << 8 | data[offset + 1]);
		}

		private long ByteLong (byte[] data, int offset) {
			return (long)ByteInt (data, offset) << 32 + ByteInt (data, offset + 4);
        }

		private string IntString (byte[] data, int offset) {
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

		private string ParseTime (byte[] data, int offset, long position)
        {
			DateTime t = new DateTime (1904, 1, 1).AddSeconds (this.ByteUInt (data, offset));
			return $"{t.Year}-{t.Month:D2}-{t.Day:D2} {t.Hour:D2}:{t.Minute:D2}:{t.Second:D2} ({position + offset:x10}-{position + offset + 3:x10})";
        }

		private string ParseDuration (byte[] data, int offset, int? timeScale)
        {
			int value = this.ByteInt (data, offset);
			StringBuilder b = new ($"{value:x}h");
            if (timeScale.HasValue && timeScale > 0)
            {
				double s = value * 1.0 / (int)timeScale;
				b.Append ($" ({s} seconds, {(int)s / 3600:D2}:{(int)s / 60 % 60:D2}:{(int)s % 60:D2}.{((int)(s * 1000)) % 1000:D3})");
            }
			return b.ToString ();
        }
		#endregion common utilities
	}
}
