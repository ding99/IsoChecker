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

		private const int MaxSize = 1_048_576;
		private readonly HashSet<AtomType> containers;
		private readonly Dictionary<AtomType, int> references;

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

            this.references = new Dictionary<AtomType, int> {
				[AtomType.DREF] = 8,
				[AtomType.STSD] = 8
			};
        }

		public void End () {
			if (this.file != null)
				this.file.End ();
        }

		public async Task<Atom> GetTree (string path) {
			this.file = new (path);
			this.fileSize = this.file.FileSize ();

			Atom atom = new ();

			// Root atom id is always 1
			await Task.Run (() => atom = this.GetAtom (1, this.file.FileSize (), 0L, 0, new Track()) );

			this.file.End ();
			return atom;
        }

		public async Task<byte[]> GetData (string path, long offset, int size) {
			if (size > MaxSize)
				size = MaxSize;

			//byte[] a = new byte[0];
			return await Task.Run (() => new byte[size]);
			//return a;
		}
		#endregion public

		#region atom utilities
		private Atom GetAtom (int id, long size, long offset, int head, Track track) {
			Console.WriteLine ($"id {id:x}, size {size:x}, offset {offset:x}, head {head:x}");
			Atom atom = new (id, size, offset);

			if (atom.Type.HasValue && !this.isContainer ((AtomType)atom.Type))
			{
				atom.Items = this.Parse (atom, track);
			}

			List<Atom> atoms = new ();
			bool valid = true;

			long si;
			for (long ip = offset + head; ip < offset + size; ip += si) {
				byte[] buffer = this.file.Read (8, ip);

				if (buffer.Length < 1)
					break;

				int atomId = DataType.ByteInt (buffer, 4);
				int atomHead = 8;

				if (DataType.ValidId (atomId)) {
					switch (si = DataType.ByteInt (buffer, 0)) {
					case 0:
						si = this.fileSize - ip;
						break;
					case 1:
						buffer = this.file.Read (8);
						if (buffer.Length < 1)
							valid = false;
						else {
							si = DataType.ByteLong (buffer, 0);
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

				//Console.WriteLine ($"  atomId {atomId:x}, si {si:x}, ip {ip:x}, atomHead {atomHead:x}");

				if (!valid)
					break;

				if (Enum.IsDefined (typeof (AtomType), atomId)) {
					if (this.references.ContainsKey ((AtomType)atomId))
						atomHead += this.references[(AtomType)atomId];
					Atom newAtom = GetAtom (atomId, si, ip, atomHead, track);
					atoms.Add (newAtom);
				}
			}

			if (atoms.Count > 0)
				atom.Atoms = atoms;

			return atom;
		}

		private List<Item> Parse (Atom atom, Track track) {
            switch (atom.Type) {
			case AtomType.FTYP:
				return this.ParseFtyp (atom);
			case AtomType.MVHD:
				return this.ParseMvhd (atom);
			case AtomType.TKHD:
				return this.ParseTkhd (atom);
			case AtomType.MDHD:
				return this.ParseMdhd (atom);
			case AtomType.VMHD:
				return this.ParseVmhd (atom);
			case AtomType.SMHD:
				return this.ParseSmhd (atom);
			case AtomType.ELST:
				return this.ParseElst (atom);
			case AtomType.HDLR:
				return this.ParseHdlr (atom, track);
			case AtomType.DREF:
				return this.ParseDref (atom);
			case AtomType.ALIS:
				return this.ParseAlis (atom);
			case AtomType.RSRC:
				return this.ParseRsrc (atom);
			case AtomType.URL:
				return this.ParseUrl (atom);
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
				return this.ParseStsc (atom, track);
			case AtomType.STCO:
				return this.ParseStco (atom, track);
			case AtomType.TMCD:
				return this.ParseTmcd (atom, track);
			case AtomType.TCMI:
				return this.ParseTcmi (atom);
			case AtomType.AVC1:
				return this.ParseAvc1 (atom);
			case AtomType.MP4A:
				return this.ParseMp4a (atom);
			case AtomType.C608:
				return this.ParseC608 (atom);
			case AtomType.C708:
				return this.ParseC708 (atom);
			}

			return Array.Empty <Item> ().ToList ();
        }

		private List<Item> ParseAtom (Func<byte[], List<Item>> add, Atom atom) {
			byte[] buffer = this.file.Read ((int)atom.Size, atom.Offset);
			return buffer.Length >= (int)atom.Size ? add (buffer) : Array.Empty<Item> ().ToList ();
		}

		private List<Item> ParseFtyp (Atom atom)
		{
			return this.ParseAtom (buffer => {
				return new[] {
					new Item { Name = "MajorBrand", Type = ItemType.String, Value = DataType.ByteString (buffer, 8) },
					new Item { Name = "MinorBrand", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 12) },
					new Item { Name = "CompatibleBrands", Type = ItemType.String, Value = DataType.ByteString(buffer, 16, (int) atom.Size - 16) }
				}.ToList ();
			}, atom);
		}

		private List<Item> ParseMvhd (Atom atom) {
			return this.ParseAtom (buffer => {
				this.timeScale = DataType.ByteInt (buffer, 20);
				return new[] {
					new Item { Name = "Version", Type = ItemType.Byte, Value = buffer[8] },
					new Item { Name = "Flags", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 8) & 0xffffff },
					new Item { Name = "CreationTime", Type = ItemType.String, Value = this.ParseTime (buffer, 12, atom.Offset) },
					new Item { Name = "ModificationTime", Type = ItemType.String, Value = this.ParseTime (buffer, 16, atom.Offset) },
					new Item { Name = "TimeScale", Type = ItemType.Int, Value = this.timeScale },
					new Item { Name = "Duration", Type = ItemType.String, Value = this.ParseDuration (buffer, 24, this.timeScale) },
					new Item { Name = "PerferredRate", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 28) },
					new Item { Name = "PreferredVolume", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 32) },
					new Item { Name = "MatrixStructure", Type = ItemType.Matrix, Value = this.ParseMatrix (buffer, 44) },
					new Item { Name = "PreviewTime", Type = ItemType.String, Value = this.ParseTime (buffer, 80, atom.Offset) },
					new Item { Name = "PreviewDuration", Type = ItemType.String, Value = this.ParseDuration (buffer, 84, this.timeScale) },
					new Item { Name = "PosterTime", Type = ItemType.String, Value = this.ParseTime (buffer, 88, atom.Offset) },
					new Item { Name = "SelectionTime", Type = ItemType.String, Value = this.ParseTime (buffer, 92, atom.Offset) },
					new Item { Name = "SelectionDuration", Type = ItemType.String, Value = this.ParseDuration (buffer, 96, this.timeScale) },
					new Item { Name = "CurrentTime", Type = ItemType.String, Value = this.ParseTime (buffer, 100, atom.Offset) },
					new Item { Name = "NextTrackID", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 104) },
				}.ToList ();
			}, atom);
        }

		private List<Item> ParseTkhd (Atom atom)
		{
			return this.ParseAtom (buffer => {
				return new[] {
					new Item { Name = "Version", Type = ItemType.Byte, Value = buffer[8] },
					new Item { Name = "Flags", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 8) & 0xffffff },
					new Item { Name = "FlagDetails", Type = ItemType.String, Value = this.ParseFlags ((uint)DataType.ByteInt (buffer, 8) & 0xffffff) },
					new Item { Name = "CreationTime", Type = ItemType.String, Value = this.ParseTime(buffer, 12, atom.Offset) },
					new Item { Name = "ModificationTime", Type = ItemType.String, Value = this.ParseTime (buffer, 16, atom.Offset) },
					new Item { Name = "TrackID", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 20) },
					new Item { Name = "Duration", Type = ItemType.String, Value = this.ParseDuration (buffer, 28, this.timeScale) },
					new Item { Name = "Layer", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 40) },
					new Item { Name = "AlternateGroup", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 42) },
					new Item { Name = "Volume", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 44) },
					new Item { Name = "MatrixStructure", Type = ItemType.Matrix, Value = this.ParseMatrix (buffer, 48) },
					new Item { Name = "TrackWidth", Type = ItemType.Double, Value = this.ParseDimension (buffer, 84) },
					new Item { Name = "TrackHeight", Type = ItemType.Double, Value = this.ParseDimension (buffer, 88) }
				}.ToList ();
			}, atom);
		}
		private string ParseFlags (uint flags)
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

		//TODO: language
		private List<Item> ParseMdhd (Atom atom)
		{
			return this.ParseAtom (buffer => {
				int timeScale = DataType.ByteInt (buffer, 20);
				return new[] {
					new Item { Name = "Version", Type = ItemType.Byte, Value = buffer[8] },
					new Item { Name = "Flags", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 8) & 0xffffff },
					new Item { Name = "CreationTime", Type = ItemType.String, Value = this.ParseTime (buffer, 12, atom.Offset) },
					new Item { Name = "ModificationTime", Type = ItemType.String, Value = this.ParseTime (buffer, 16, atom.Offset) },
					new Item { Name = "TimeScale", Type = ItemType.Int, Value = timeScale },
					new Item { Name = "Duration", Type = ItemType.String, Value = this.ParseDuration (buffer, 24, timeScale) },
					new Item { Name = "Language", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 28) },
					new Item { Name = "Quality", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 30) }
				}.ToList ();
			}, atom);
		}

		private List<Item> ParseVmhd (Atom atom)
		{
			return this.ParseAtom (buffer => {
				int value = DataType.ByteShort (buffer, 12);
				string graphics = Enum.IsDefined (typeof (GraphicsMode), value) ? ( (GraphicsMode) value).ToString () : "";

				return new[] {
					new Item { Name = "Version", Type = ItemType.Byte, Value = buffer[8] },
					new Item { Name = "Flags", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 8) & 0xffffff },
					new Item { Name = "GraphicsMode", Type = ItemType.String, Value = graphics },
					new Item { Name = "OpcolorRed", Type = ItemType.UShort, Value = DataType.ByteUShort (buffer, 14) },
					new Item { Name = "OpcolorGreen", Type = ItemType.UShort, Value = DataType.ByteUShort (buffer, 16) },
					new Item { Name = "OpcolorBlue", Type = ItemType.UShort, Value = DataType.ByteUShort (buffer, 18) }
				}.ToList ();
			}, atom);
		}

		private List<Item> ParseSmhd (Atom atom)
		{
			return this.ParseAtom (buffer => {
				return new[] {
					new Item { Name = "Version", Type = ItemType.Byte, Value = buffer[8] },
					new Item { Name = "Flags", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 8) & 0xffffff },
					new Item { Name = "Balance", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 12) }
				}.ToList ();
			}, atom);
		}

		private List<Item> ParseElst(Atom atom) {
            return this.ParseAtom (buffer => {
				List<Item> items = new ();

				int count = DataType.ByteInt (buffer, 12);
				items.Add (new Item { Name = "Entries", Type = ItemType.Int, Value = count });

				for (int i = 0; i < count; i++) {
					items.Add (new Item { Name = "TrackDuration", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 16 + 12 * i) });
					items.Add (new Item { Name = "MediaTime", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 20 + 12 * i) });
					items.Add (new Item { Name = "MediaRate", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 24 + 12 * i) });
					if (this.timeScale.HasValue && this.timeScale != 0)
						items.Add (new Item {
							Name = "DurationSec",
							Type = ItemType.Double,
							Value = (double)DataType.ByteInt (buffer, 16 + 12 * i) / (double)this.timeScale
						});
				}

				return items;
			}, atom);
        }

		private List<Item> ParseDref (Atom atom)
        {
			return this.ParseAtom (buffer => {
				return new[] {
					new Item { Name = "Version", Type = ItemType.Byte, Value = buffer[8] },
					new Item { Name = "Flags", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 8) & 0xffffff },
					new Item { Name = "Entries", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 12) }
				}.ToList ();
			}, atom);
		}

		private List<Item> ParseHdlr (Atom atom, Track track) {
			return this.ParseAtom (buffer => {
				int value = DataType.ByteInt (buffer, 12);
				if (Enum.IsDefined (typeof (ComponentType), value))
					track.Type = (ComponentType)value;

				value = DataType.ByteInt (buffer, 16);
				if (Enum.IsDefined (typeof (ComponentSubType), value))
					track.SubType = (ComponentSubType)value;

				return new[] {
					new Item { Name = "Version", Type = ItemType.Byte, Value = buffer[8] },
					new Item { Name = "Flags", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 8) & 0xffffff },
					new Item { Name = "ComponentType", Type = ItemType.String, Value = DataType.ByteString (buffer, 12) },
					new Item { Name = "ComponentSubType", Type = ItemType.String, Value = DataType.ByteString (buffer, 16) },
					new Item { Name = "ComponentManufacturer", Type = ItemType.String, Value = DataType.ByteString (buffer, 20) },
					new Item { Name = "ComponentFlags", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 24) },
					new Item { Name = "ComponentFlagsMask", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 28) },
					new Item { Name = "ComponentName", Type = ItemType.String, Value = DataType.ByteString (buffer, 33, buffer[32]) },
				}.ToList ();
			}, atom);
		}

		private List<Item> ParseGmin (Atom atom) {
			return this.ParseAtom (buffer => new[] {
				new Item { Name = "GraphicsMode", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 12) }
			}.ToList(), atom);
		}

		private List<Item> ParseAvc1 (Atom atom)
        {
			return this.ParseAtom (buffer => new[] {
				new Item { Name = "DataReferenceIndex", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 14) }
			}.ToList (), atom);
		}
		private List<Item> ParseMp4a (Atom atom)
		{
			return this.ParseAtom (buffer => new[] {
				new Item { Name = "DataReferenceIndex", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 14) }
			}.ToList (), atom);
		}
		private List<Item> ParseC608 (Atom atom)
		{
			return this.ParseAtom (buffer => new[] {
				new Item { Name = "DataReferenceIndex", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 14) }
			}.ToList (), atom);
		}
		private List<Item> ParseC708 (Atom atom)
		{
			return this.ParseAtom (buffer => new[] {
				new Item { Name = "DataReferenceIndex", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 14) }
			}.ToList (), atom);
		}

		private List<Item> ParseStsd (Atom atom)
		{
			return this.ParseAtom (buffer => new[] {
				new Item { Name = "Version", Type = ItemType.Byte, Value = buffer[8] },
				new Item { Name = "Flags", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 8) & 0xffffff },
				new Item { Name = "Entries", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 12) }
			}.ToList (), atom);
		}

		private List<Item> ParseStsd_o (Atom atom)
		{
			return this.ParseAtom (buffer => {
				int size = (int)atom.Size;

				List<Item> items = new ();
				items.Add (new Item { Name = "Version", Type = ItemType.Byte, Value = buffer[8] });
				items.Add (new Item { Name = "Flags", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 8) & 0xffffff });
				int count = DataType.ByteInt (buffer, 12);
				items.Add (new Item { Name = "Entries", Type = ItemType.Int, Value = count });

				if (count > 0)
				{
					List<Atom> entries = new ();
					int dataSize = 0;
					for (int i = 0, pos = 16; i < count && pos < size; i++, pos += dataSize)
					{
						dataSize = DataType.ByteInt (buffer, pos);
						entries.Add (new (DataType.ByteInt (buffer, pos + 4), dataSize, atom.Offset + pos)
						{
							Items = new[] {
								new Item { Name = "DataReferenceIndex", Type = ItemType.Short, Value = DataType.ByteShort (buffer, pos + 14) }
							}.ToList ()
						});
					}
					atom.Atoms = entries;
				}

				return items;
			}, atom);
		}

		private List<Item> ParseStts (Atom atom) {
			return this.ParseAtom (buffer => new[] {
				new Item { Name = "Entries", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 12) }
			}.ToList (), atom);
		}
		private List<Item> ParseStss (Atom atom) {
			return this.ParseAtom (buffer => new[] {
				new Item { Name = "Entries", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 12) }
			}.ToList (), atom);
		}
		private List<Item> ParseStsz (Atom atom) {
			return this.ParseAtom (buffer => new[] {
				new Item { Name = "SampleSize", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 12) },
				new Item { Name = "Entries", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 16) }
			}.ToList (), atom);
		}
		private List<Item> ParseStsc (Atom atom, Track track) {
			return this.ParseAtom (buffer => {
				List<Item> items = new ();
				int count = DataType.ByteInt (buffer, 12);
				items.Add (new Item { Name = "Entries", Type = ItemType.Int, Value = count });

				if (track.SubType == ComponentSubType.Caption)
				{
					for (int i = 0; i < count; i++)
					{
						int j = 16 + i * 12;
						items.Add (new Item { Name = "Chunk", Type = ItemType.String, Value = $"{DataType.ByteInt (buffer, j)}({DataType.ByteInt (buffer, j):x}h), {DataType.ByteInt (buffer, j + 4)}({DataType.ByteInt (buffer, j + 4):x}h), {DataType.ByteInt (buffer, j + 8)}({DataType.ByteInt (buffer, j+ 8):x}h)" });
					}
				}

				return items;
			}, atom);
		}
		private List<Item> ParseStco (Atom atom, Track track) {
			return this.ParseAtom (buffer => {
				List<Item> items = new ();
				int count = DataType.ByteInt (buffer, 12);
				items.Add (new Item { Name = "Entries", Type = ItemType.Int, Value = count });

				if(track.SubType == ComponentSubType.Caption) {
					for (int i = 0; i < count; i++)
						items.Add (new Item { Name = "Offset", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 16 + 4 * i) });
                }

				return items;
			}, atom);
		}


		//TODO: parse for multi cases
		private List<Item> ParseTmcd (Atom atom, Track track)
		{
			switch ((int) atom.Size)
			{
			case 0xc:  //tref
				return new List<Item> ();
			case 0x22:  //STSD
				return this.ParseAtom (buffer => new[] {
					new Item { Name = "DataReferenceIndex", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 14) }
				}.ToList (), atom);
			}

			return new List<Item> ();  //gmhd etc.
		}

		private List<Item> ParseTcmi (Atom atom)
        {
			return this.ParseAtom (buffer => new[] {
				new Item { Name = "Version", Type = ItemType.Byte, Value = buffer[8] },
				new Item { Name = "Flags", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 8) & 0xffffff },
				new Item { Name = "TextFont", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 12) },
				new Item { Name = "TextFace", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 14) },
				new Item { Name = "TextFaceDetails", Type = ItemType.String, Value = this.ParseFont (DataType.ByteShort (buffer, 14)) },
				new Item { Name = "TextSize", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 16) },
				new Item { Name = "TextColorRed", Type = ItemType.UShort, Value = DataType.ByteUShort (buffer, 20) },
				new Item { Name = "TextColorGreen", Type = ItemType.UShort, Value = DataType.ByteUShort (buffer, 22) },
				new Item { Name = "TextColorBlue", Type = ItemType.UShort, Value = DataType.ByteUShort (buffer, 24) },
				new Item { Name = "BackColorRed", Type = ItemType.UShort, Value = DataType.ByteUShort (buffer, 26) },
				new Item { Name = "BackColorGreen", Type = ItemType.UShort, Value = DataType.ByteUShort (buffer, 28) },
				new Item { Name = "BackColorBlue", Type = ItemType.UShort, Value = DataType.ByteUShort (buffer, 30) },
				new Item { Name = "FontName", Type = ItemType.String, Value = DataType.ByteString (buffer, 33, buffer[32]) }
			}.ToList (), atom);
		}
		private string ParseFont (short flags)
        {
			if (flags == 0)
				return "Normal";

			List<string> styles = new ();
			if ((flags & 1) > 0)
				styles.Add ("Bold");
			if ((flags & 2) > 0)
				styles.Add ("Italic");
			if ((flags & 4) > 0)
				styles.Add ("Underline");
			if ((flags & 8) > 0)
				styles.Add ("Outline");
			if ((flags & 10) > 0)
				styles.Add ("Shadow");
			if ((flags & 20) > 0)
				styles.Add ("Condense");
			if ((flags & 40) > 0)
				styles.Add ("Extend");

			return string.Join (", ", styles.ToArray ());
		}

		//TODO
		private List<Item> ParseAlis (Atom atom)
		{
			return this.ParseAtom (buffer => new[] {
				new Item { Name = "Version", Type = ItemType.Byte, Value = buffer[8] },
				new Item { Name = "Flags", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 8) & 0xffffff }
			}.ToList (), atom);
		}
		//TODO
		private List<Item> ParseRsrc (Atom atom)
		{
			return this.ParseAtom (buffer => new[] {
				new Item { Name = "Version", Type = ItemType.Byte, Value = buffer[8] },
				new Item { Name = "Flags", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 8) & 0xffffff }
			}.ToList (), atom);
		}
		//TODO
		private List<Item> ParseUrl (Atom atom)
		{
			return this.ParseAtom (buffer => new[] {
				new Item { Name = "Version", Type = ItemType.Byte, Value = buffer[8] },
				new Item { Name = "Flags", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 8) & 0xffffff }
			}.ToList (), atom);
		}

		#endregion atom utilities

		#region common utilities
		private bool isContainer(AtomType type) {
			return this.containers.Contains (type);
        }

		private string ParseTime (byte[] data, int offset, long position)
        {
			DateTime t = new DateTime (1904, 1, 1).AddSeconds (DataType.ByteUInt (data, offset));
			return $"{t.Year}-{t.Month:D2}-{t.Day:D2} {t.Hour:D2}:{t.Minute:D2}:{t.Second:D2} ({position + offset:x10}-{position + offset + 3:x10})";
        }

		private string ParseDuration (byte[] data, int offset, int? timeScale)
        {
			int value = DataType.ByteInt (data, offset);
			StringBuilder b = new ($"{value:x}h");
            if (timeScale.HasValue && timeScale > 0)
            {
				double s = value * 1.0 / (int)timeScale;
				b.Append ($" ({s} seconds, {(int)s / 3600:D2}:{(int)s / 60 % 60:D2}:{(int)s % 60:D2}.{((int)(s * 1000)) % 1000:D3})");
            }
			return b.ToString ();
        }

		private double[] ParseMatrix (byte[] data, int offset)
        {
			double[] values = new double[9];
			double fraction;
			int integerPortion;

			for(int i = 0; i < 9; i++) 

			{
				if (i == 2 || i == 5 || i == 8)
                {
					integerPortion = (int) (data[offset + i * 4] >> 6);
					fraction = ((uint)DataType.ByteInt (data, offset + i * 4) & 0x3fffffff) * 1.0 / 1_073_741_824;
				}
				else
                {
					integerPortion = DataType.ByteShort (data, offset + i * 4);
					fraction = DataType.ByteUShort (data, offset + i * 4 + 2) * 1.0 / 65536;
                }
				if (integerPortion >= 0)
					values[i] = (double)integerPortion + fraction;
				else
					values[i] = (double)integerPortion - fraction;
			}

			return values;
        }
		private double ParseDimension (byte[] data, int offset)
		{
			int integerPortion = DataType.ByteShort (data, offset);
			double fraction = DataType.ByteUShort (data, offset + 2) * 1.0 / 65_536;

			return integerPortion >= 0 ? (double)integerPortion + fraction : (double)integerPortion - fraction;
		}
		#endregion common utilities
	}
}
