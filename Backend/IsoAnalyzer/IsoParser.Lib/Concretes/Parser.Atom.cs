using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IsoParser.Lib.Models;
using IsoParser.Lib.Tools;

namespace IsoParser.Lib.Concretes
{
    partial class Parser
    {
		private int? timeScale;

		private Atom GetAtom (int id, long size, long offset, int head)
		{
			Console.WriteLine ($"id {id:x}, size {size:x}, offset {offset:x}, head {head:x}");
			Atom atom = new (id, size, offset);

			if (atom.Type.HasValue && !this.isContainer ((AtomType)atom.Type))
			{
				atom.Items = this.Parse (atom);
			}

			List<Atom> atoms = new ();
			bool valid = true;

			long si;
			for (long ip = offset + head; ip < offset + size; ip += si)
			{
				byte[] buffer = this.file.Read (8, ip);

				if (buffer.Length < 1)
					break;

				int atomId = DataType.ByteInt (buffer, 4);
				int atomHead = 8;

				if (DataType.ValidId (atomId))
				{
					switch (si = DataType.ByteInt (buffer, 0))
					{
					case 0:
						si = this.fileSize - ip;
						break;
					case 1:
						buffer = this.file.Read (8);
						if (buffer.Length < 1)
							valid = false;
						else
						{
							si = DataType.ByteLong (buffer, 0);
							atomHead += 8;
						}
						break;
					}
				}
				else
				{
					atomId = 0;
					atomHead = 0;
					si = size - ip;
					valid = false;
				}

				//Console.WriteLine ($"  atomId {atomId:x}, si {si:x}, ip {ip:x}, atomHead {atomHead:x}");

				if (!valid)
					break;

				if (Enum.IsDefined (typeof (AtomType), atomId))
				{
					if (this.references.ContainsKey ((AtomType)atomId))
						atomHead += this.references[(AtomType)atomId];
					Atom newAtom = GetAtom (atomId, si, ip, atomHead);
					atoms.Add (newAtom);
				}
			}

			if (atoms.Count > 0)
				atom.Atoms = atoms;

			return atom;
		}

		private List<Item> Parse (Atom atom)
		{
			switch (atom.Type)
			{
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
			case AtomType.LOAD:
				return this.ParseLoad (atom);
			case AtomType.ELST:
				return this.ParseElst (atom);
			case AtomType.HDLR:
				return this.ParseHdlr (atom);
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
				return this.ParseStsc (atom);
			case AtomType.STCO:
				return this.ParseStco (atom);
			case AtomType.TMCD:
				return this.ParseTmcd (atom);
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

			return Array.Empty<Item> ().ToList ();
		}

		private List<Item> ParseAtom (Func<byte[], List<Item>> add, Atom atom)
		{
			byte[] buffer = this.file.Read ((int)atom.Size, atom.Offset);
			return buffer.Length >= (int)atom.Size ? add (buffer) : Array.Empty<Item> ().ToList ();
		}

		#region atom types
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

		private List<Item> ParseMvhd (Atom atom)
		{
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

			if ((flags & 8) > 0)
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
				string graphics = Enum.IsDefined (typeof (GraphicsMode), value) ? ((GraphicsMode)value).ToString () : "";

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

		private List<Item> ParseLoad (Atom atom)
		{
			return this.ParseAtom (buffer => {
				List<Item> items = new ();

				items.Add (new Item { Name = "PreloadStartTime", Type = ItemType.String, Value = this.ParseTime (buffer, 8, atom.Offset) });
				items.Add (new Item { Name = "PreloadDuration", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 12) });
				int flags = DataType.ByteInt (buffer, 16);
				items.Add (new Item { Name = "PreloadFlags", Type = ItemType.Int, Value = flags });
				if(flags == 1 & flags == 2)
                {
					items.Add (new Item { Name = "PreloadFlagsDetails", Type = ItemType.String, Value = $"preloaded {(flags == 1 ? "absolutely" : "only if enabled")}" });
				}
				int hints = DataType.ByteInt (buffer, 20);
				items.Add (new Item { Name = "DefaultHints", Type = ItemType.Int, Value = hints });
				if (hints != 0)
				{
					StringBuilder s = new ();
					if ((hints & 0x20) != 0)
					{
						s.Append ("double-buffer");
					}
					if ((hints & 0x100) != 0)
					{
						s.Append ($"{(s.Length > 0 ? " " : "")}high-quality");
					}
					items.Add (new Item {  Name = "DefaultHintsDetails", Type= ItemType.String, Value = s.ToString() });
				}

				return items;
			}, atom);
		}

		private List<Item> ParseElst (Atom atom)
		{
			return this.ParseAtom (buffer => {
				List<Item> items = new ();

				int count = DataType.ByteInt (buffer, 12);
				items.Add (new Item { Name = "Entries", Type = ItemType.Int, Value = count });

				for (int i = 0; i < count; i++)
				{
					items.Add (new Item { Name = "TrackDuration", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 16 + 12 * i) });
					items.Add (new Item { Name = "MediaTime", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 20 + 12 * i) });
					items.Add (new Item { Name = "MediaRate", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 24 + 12 * i) });
					if (this.timeScale.HasValue && this.timeScale != 0)
						items.Add (new Item
						{
							Name = "TrackDurationSec",
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

		private List<Item> ParseHdlr (Atom atom)
		{
			return this.ParseAtom (buffer => {
				int value = DataType.ByteInt (buffer, 12);
				if (Enum.IsDefined (typeof (ComponentType), value))
				{
					if (this.track.Type != ComponentType.Unknown)
						this.iso.Tracks.Add (this.track);

					this.track = new () { Type = (ComponentType)value };
				}

				value = DataType.ByteInt (buffer, 16);
				if (Enum.IsDefined (typeof (ComponentSubType), value))
					this.track.SubType = (ComponentSubType)value;

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

		private List<Item> ParseGmin (Atom atom)
		{
			return this.ParseAtom (buffer => new[] {
				new Item { Name = "GraphicsMode", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 12) }
			}.ToList (), atom);
		}

		//TODO: details
		private List<Item> ParseAvc1 (Atom atom)
		{
			return this.ParseAtom (buffer => new[] {
				new Item { Name = "DataReferenceIndex", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 14) }
			}.ToList (), atom);
		}
		//TODO: details
		private List<Item> ParseMp4a (Atom atom)
		{
			return this.ParseAtom (buffer => new[] {
				new Item { Name = "DataReferenceIndex", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 14) }
			}.ToList (), atom);
		}
		private List<Item> ParseC608 (Atom atom)
		{
			this.track.DataFormats.Add ("c608");
			return this.ParseAtom (buffer => new[] {
				new Item { Name = "DataReferenceIndex", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 14) }
			}.ToList (), atom);
		}
		private List<Item> ParseC708 (Atom atom)
		{
			this.track.DataFormats.Add ("c708");

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

		private List<Item> ParseStts (Atom atom)
		{
			return this.ParseAtom (buffer => {
				List<Item> items = new ();
				int count = DataType.ByteInt (buffer, 12);
				items.Add (new Item { Name = "Entries", Type = ItemType.Int, Value = count });

				items.Add (new Item { Name = "Table", Type = ItemType.String, Value = "SampleCount, Duration" });
				for (int i = 0; i < count; i++)
				{
					int SampleCount = DataType.ByteInt (buffer, 16 + i * 8);
					int Duration = DataType.ByteInt (buffer, 20 + i * 8);
					this.track.TimeToSamples.Add (new () { SampleCount = SampleCount, SampleDuration = Duration });
					items.Add (new Item { Name = "Entry", Type = ItemType.String, Value = $"{SampleCount}({SampleCount:x}h), {Duration}({Duration:x}h)" });
				}

				return items;
			}, atom);
		}
		private List<Item> ParseStss (Atom atom)
		{
			return this.ParseAtom (buffer => new[] {
				new Item { Name = "Entries", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 12) }
			}.ToList (), atom);
		}
		private List<Item> ParseStsz (Atom atom)
		{
			return this.ParseAtom (buffer => new[] {
				new Item { Name = "SampleSize", Type = ItemType.Int, Value = this.track.SampleSize = DataType.ByteInt (buffer, 12) },
				new Item { Name = "Entries", Type = ItemType.Int, Value = this.track.SampleSizeCount = DataType.ByteInt (buffer, 16) }
			}.ToList (), atom);
		}
		private List<Item> ParseStsc (Atom atom)
		{
			return this.ParseAtom (buffer => {
				List<Item> items = new ();
				int count = DataType.ByteInt (buffer, 12);
				items.Add (new Item { Name = "Entries", Type = ItemType.Int, Value = count });

					items.Add (new Item { Name = "Table", Type = ItemType.String, Value = "FirstChunk, Samples, Description ID" });
				for (int i = 0; i < count; i++)
				{
					int chunk = DataType.ByteInt (buffer, 16 + i * 12);
					int samples = DataType.ByteInt (buffer, 20 + i * 12);
					int id = DataType.ByteInt (buffer, 24 + i * 12);

					this.track.SampleToChunks.Add (new () { FirstChunk = chunk, SamplesPerChunk = samples, DescriptionId = id });

					items.Add (new Item { Name = "Chunk", Type = ItemType.String, Value = $"{chunk}({chunk:x}h), {samples}({samples:x}h), {id}({id:x}h)" });
				}

				return items;
			}, atom);
		}
		private List<Item> ParseStco (Atom atom)
		{
			return this.ParseAtom (buffer => {
				List<Item> items = new ();
				int count = DataType.ByteInt (buffer, 12);
				items.Add (new Item { Name = "Entries", Type = ItemType.Int, Value = count });

				for (int i = 0; i < count; i++)
				{
					int offset = DataType.ByteInt (buffer, 16 + 4 * i);
					this.track.ChunkOffsets.Add (offset);
					items.Add (new Item { Name = "Offset", Type = ItemType.Int, Value = offset });
				}

				return items;
			}, atom);
		}

		//TODO: parse for multi cases
		private List<Item> ParseTmcd (Atom atom)
		{
			switch ((int)atom.Size)
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
	}
}
