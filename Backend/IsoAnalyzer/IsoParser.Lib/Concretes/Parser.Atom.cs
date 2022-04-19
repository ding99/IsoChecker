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

		private Atom GetAtom (int id, long size, long offset, int head, int index = 0)
		{
			Console.WriteLine ($"id {id:x}, size {size:x}, offset {offset:x}, head {head:x}");
			Atom atom = new (id, size, offset);
			int headPlus = 0;

			if (atom.Type.HasValue && !this.isContainer ((AtomType)atom.Type))
			{
				(atom.Items, headPlus) = this.Parse (atom, index);
			}

			List<Atom> atoms = new ();
			bool valid = true;

			long si;
			for (long ip = offset + head + headPlus; ip < offset + size; ip += si)
			{
				byte[] buffer = this.file.Read (8, ip);

				if (buffer.Length < 1)
					break;

				int atomId = DataType.ByteInt (buffer, 4), subIndex = atomId;
				int atomHead = 8;
				if (id.Equals ((int)AtomType.ilst)) {
					atomId = (int)AtomType.ITEM;
				}

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
					Atom newAtom = GetAtom (atomId, si, ip, atomHead, subIndex);
					atoms.Add (newAtom);
				}
			}

			if (atoms.Count > 0)
				atom.Atoms = atoms;

			return atom;
		}

		private (List<Item>, int) Parse (Atom atom, int index)
		{
			switch (atom.Type)
			{
			case AtomType.ftyp:
				return this.ParseFtyp (atom);
			case AtomType.mvhd:
				return this.ParseMvhd (atom);
			case AtomType.tkhd:
				return this.ParseTkhd (atom);
			case AtomType.mdhd:
				return this.ParseMdhd (atom);
			case AtomType.vmhd:
				return this.ParseVmhd (atom);
			case AtomType.smhd:
				return this.ParseSmhd (atom);
			case AtomType.load:
				return this.ParseLoad (atom);
			case AtomType.elst:
				return this.ParseElst (atom);
			case AtomType.hdlr:
				return this.ParseHdlr (atom);
			case AtomType.mhdr:
				return this.ParseMhdr (atom);
			case AtomType.keys:
				return this.ParseKeys (atom);
			case AtomType.mdta:
			case AtomType.udta:
				return this.ParseMdta (atom);
			case AtomType.ITEM:
				return this.ParseItem (atom, index);
			case AtomType.data:
				return this.ParseData (atom);
			case AtomType.itif:
				return this.ParseItif (atom);
			case AtomType.name:
				return this.ParseName (atom);
			case AtomType.ctry:
				return this.ParseCtry (atom);
			case AtomType.lang:
				return this.ParseLang (atom);
			case AtomType.dref:
				return this.ParseDref (atom);
			case AtomType.alis:
				return this.ParseAlis (atom);
			case AtomType.rsrc:
				return this.ParseRsrc (atom);
			case AtomType.URL:
				return this.ParseUrl (atom);
			case AtomType.gmin:
				return this.ParseGmin (atom);
			case AtomType.stsd:
				return this.ParseStsd (atom);
			case AtomType.stss:
				return this.ParseStss (atom);
			case AtomType.stts:
				return this.ParseStts (atom);
			case AtomType.stsz:
				return this.ParseStsz (atom);
			case AtomType.stsc:
				return this.ParseStsc (atom);
			case AtomType.stco:
				return this.ParseStco (atom);
			case AtomType.tmcd:
				return this.ParseTmcd (atom);
			case AtomType.tcmi:
				return this.ParseTcmi (atom);
			case AtomType.avc1:
				return this.ParseAvc1 (atom);
			case AtomType.avcC:
				return this.ParseAvcC (atom);
			case AtomType.btrt:
				return this.ParseBtrt (atom);
			case AtomType.colr:
				return this.ParseColr (atom);
			case AtomType.pasp:
				return this.ParsePasp (atom);
			case AtomType.fiel:
				return this.ParseFiel (atom);
			case AtomType.clap:
				return this.ParseClap (atom);
			case AtomType.mp4a:
				return this.ParseMp4a (atom);
			case AtomType.chan:
				return this.ParseChan (atom);
			case AtomType.frma:
				return this.ParseFrma (atom);
			case AtomType.esds:
				return this.ParseEsds (atom);
			case AtomType.c608:
				return this.ParseC608 (atom);
			case AtomType.c708:
				return this.ParseC708 (atom);
			}

			return (Array.Empty<Item> ().ToList (), 0);
		}

		private (List<Item>, int) ParseAtom (Func<byte[], (List<Item>, int)> add, Atom atom)
		{
			byte[] buffer = this.file.Read ((int)atom.Size, atom.Offset);
			return buffer.Length >= (int)atom.Size ? add (buffer) : (Array.Empty<Item> ().ToList (), 0);
		}

		#region atom types
		private (List<Item>, int) ParseFtyp (Atom atom)
		{
			return this.ParseAtom (buffer => (new[] {
				new Item { Name = "MajorBrand", Type = ItemType.String, Value = DataType.ByteString (buffer, 8) },
				new Item { Name = "MinorBrand", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 12) },
				new Item { Name = "CompatibleBrands", Type = ItemType.String, Value = DataType.ByteString(buffer, 16, (int) atom.Size - 16) }
			}.ToList (), 0), atom);
		}

		private (List<Item>, int) ParseMvhd (Atom atom)
		{
			return this.ParseAtom (buffer => {
				this.timeScale = DataType.ByteInt (buffer, 20);
				return (new[] {
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
				}.ToList (), 0);
			}, atom);
		}

		private (List<Item>, int) ParseTkhd (Atom atom)
		{
			return this.ParseAtom (buffer => (new[] {
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
			}.ToList (), 0), atom);
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
		private (List<Item>, int) ParseMdhd (Atom atom)
		{
			return this.ParseAtom (buffer => {
				int timeScale = DataType.ByteInt (buffer, 20);
				return (new[] {
					new Item { Name = "Version", Type = ItemType.Byte, Value = buffer[8] },
					new Item { Name = "Flags", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 8) & 0xffffff },
					new Item { Name = "CreationTime", Type = ItemType.String, Value = this.ParseTime (buffer, 12, atom.Offset) },
					new Item { Name = "ModificationTime", Type = ItemType.String, Value = this.ParseTime (buffer, 16, atom.Offset) },
					new Item { Name = "TimeScale", Type = ItemType.Int, Value = timeScale },
					new Item { Name = "Duration", Type = ItemType.String, Value = this.ParseDuration (buffer, 24, timeScale) },
					new Item { Name = "Language", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 28) },
					new Item { Name = "Quality", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 30) }
				}.ToList (), 0);
			}, atom);
		}

		private (List<Item>, int) ParseVmhd (Atom atom)
		{
			return this.ParseAtom (buffer => {
				int value = DataType.ByteShort (buffer, 12);
				string graphics = Enum.IsDefined (typeof (GraphicsMode), value) ? ((GraphicsMode)value).ToString () : "";

				return (new[] {
					new Item { Name = "Version", Type = ItemType.Byte, Value = buffer[8] },
					new Item { Name = "Flags", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 8) & 0xffffff },
					new Item { Name = "GraphicsMode", Type = ItemType.String, Value = graphics },
					new Item { Name = "OpcolorRed", Type = ItemType.UShort, Value = DataType.ByteUShort (buffer, 14) },
					new Item { Name = "OpcolorGreen", Type = ItemType.UShort, Value = DataType.ByteUShort (buffer, 16) },
					new Item { Name = "OpcolorBlue", Type = ItemType.UShort, Value = DataType.ByteUShort (buffer, 18) }
				}.ToList (), 0);
			}, atom);
		}

		private (List<Item>, int) ParseSmhd (Atom atom)
		{
			return this.ParseAtom (buffer => (new[] {
				new Item { Name = "Version", Type = ItemType.Byte, Value = buffer[8] },
				new Item { Name = "Flags", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 8) & 0xffffff },
				new Item { Name = "Balance", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 12) }
			}.ToList (), 0), atom);
		}

		private (List<Item>, int) ParseLoad (Atom atom)
		{
			return this.ParseAtom (buffer => {
				List<Item> items = new ();

				items.Add (new Item { Name = "PreloadStartTime", Type = ItemType.String, Value = this.ParseTime (buffer, 8, atom.Offset) });
				items.Add (new Item { Name = "PreloadDuration", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 12) });
				int flags = DataType.ByteInt (buffer, 16);
				items.Add (new Item { Name = "PreloadFlags", Type = ItemType.Int, Value = flags });
				if (flags == 1 & flags == 2)
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
					items.Add (new Item { Name = "DefaultHintsDetails", Type = ItemType.String, Value = s.ToString () });
				}

				return (items, 0);
			}, atom);
		}

		private (List<Item>, int) ParseElst (Atom atom)
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
					{
						items.Add (new Item
						{
							Name = "TrackDurationSec",
							Type = ItemType.Double,
							Value = (double)DataType.ByteInt (buffer, 16 + 12 * i) / (double)this.timeScale
						});
					}
				}

				return (items, 0);
			}, atom);
		}

		private (List<Item>, int) ParseDref (Atom atom)
		{
			return this.ParseAtom (buffer => (new[] {
				new Item { Name = "Version", Type = ItemType.Byte, Value = buffer[8] },
				new Item { Name = "Flags", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 8) & 0xffffff },
				new Item { Name = "Entries", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 12) }
			}.ToList (), 8), atom);
		}

		private (List<Item>, int) ParseHdlr (Atom atom)
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

				return (new[] {
					new Item { Name = "Version", Type = ItemType.Byte, Value = buffer[8] },
					new Item { Name = "Flags", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 8) & 0xffffff },
					new Item { Name = "ComponentType", Type = ItemType.String, Value = DataType.ByteString (buffer, 12) },
					new Item { Name = "ComponentSubType", Type = ItemType.String, Value = DataType.ByteString (buffer, 16) },
					new Item { Name = "ComponentManufacturer", Type = ItemType.String, Value = DataType.ByteString (buffer, 20) },
					new Item { Name = "ComponentFlags", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 24) },
					new Item { Name = "ComponentFlagsMask", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 28) },
					new Item { Name = "ComponentName", Type = ItemType.String, Value = DataType.ByteString (buffer, 33, buffer[32]) },
				}.ToList (), 0);
			}, atom);
		}

		private (List<Item>, int) ParseMhdr (Atom atom)
		{
			return this.ParseAtom (buffer => (new[] {
				new Item { Name = "NextItemID", Type = ItemType.Int, Value = DataType.ByteUInt (buffer, 12) }
			}.ToList (), 0), atom);
		}

		private (List<Item>, int) ParseKeys (Atom atom)
		{
			return this.ParseAtom (buffer => (new[] {
				new Item { Name = "Entries", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 12) }
			}.ToList (), 8), atom);
		}

		private (List<Item>, int) ParseMdta (Atom atom)
		{
			return this.ParseAtom (buffer => (new[] {
				new Item { Name = "KeyValue", Type = ItemType.String, Value = DataType.ByteString (buffer, 8, DataType.ByteInt (buffer, 0) - 8) }
			}.ToList (), 0), atom);
		}

		private (List<Item>, int) ParseItem (Atom atom, int index)
		{
			return this.ParseAtom (buffer => (new[] {
				new Item { Name = "KeyIndex", Type = ItemType.Int, Value = index }
			}.ToList (), 0), atom);
		}

		private (List<Item>, int) ParseData (Atom atom)
		{
			return this.ParseAtom (buffer => (new[] {
				new Item { Name = "TypeSet", Type = ItemType.Byte, Value = buffer[8] },
				new Item { Name = "Type", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 8) & 0xffffff},
				new Item { Name = "TypeDesc", Type = ItemType.String, Value = $"{(WellKnownType)(DataType.ByteInt (buffer, 8) & 0xffffff)}" },
				new Item { Name = "CountryIndicator", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 12) },
				new Item { Name = "LanguageIndicator", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 14) },
				new Item { Name = "Value", Type = ItemType.String, Value = DataType.ByteString (buffer, 16, DataType.ByteInt(buffer, 0) - 16) }
			}.ToList (), 0), atom);
		}

		private (List<Item>, int) ParseItif (Atom atom)
		{
			return this.ParseAtom (buffer => (new[] {
				new Item { Name = "ItemID", Type = ItemType.Int, Value = Encoding.UTF8.GetString (buffer.Skip (12).Take (DataType.ByteInt (buffer, 0) - 12).ToArray ()) }
			}.ToList (), 0), atom);
		}

		// TODO: compare with the 'name' atom in metadata (ilst)
		private (List<Item>, int) ParseName (Atom atom)
		{
			return this.ParseAtom (buffer => (new[] {
				new Item { Name = "Name", Type = ItemType.String, Value = Encoding.UTF8.GetString (buffer.Skip (8).Take (DataType.ByteInt (buffer, 0) - 8).ToArray ()) }
			}.ToList (), 0), atom);
		}

		private (List<Item>, int) ParseCtry (Atom atom)
		{
			return this.ParseAtom (buffer => {
				List<Item> items = new ();

				int count = DataType.ByteInt (buffer, 8);
				items.Add (new Item { Name = "Entries", Type = ItemType.Int, Value = count });

				int offset = 12;
				for (int i = 0; i < count; i++)
				{
					short ctries = DataType.ByteShort (buffer, offset);
					items.Add (new Item { Name = "Countries", Type = ItemType.Short, Value = ctries });
					for (short j = 0; j < ctries; j++)
					{
						items.Add (new Item { Name = "Country", Type = ItemType.String, Value = DataType.ByteString (buffer, offset + 2 + j * 2, 2) });
					}

					offset += 2 + ctries * 2;
				}

				return (items, 0);
			}, atom);
		}

		private (List<Item>, int) ParseLang (Atom atom)
		{
			return this.ParseAtom (buffer => {
				List<Item> items = new ();

				int count = DataType.ByteInt (buffer, 8);
				items.Add (new Item { Name = "Entries", Type = ItemType.Int, Value = count });

				int offset = 12;
				for (int i = 0; i < count; i++)
				{
					short ctries = DataType.ByteShort (buffer, offset);
					items.Add (new Item { Name = "Countries", Type = ItemType.Short, Value = ctries });
					for (short j = 0; j < ctries; j++)
					{
						items.Add (new Item { Name = "Country", Type = ItemType.Short, Value = DataType.ByteShort (buffer, offset + 2 + j * 2) });
					}

					offset += 2 + ctries * 2;
				}

				return (items, 0);
			}, atom);
		}

		private (List<Item>, int) ParseGmin (Atom atom)
		{
			return this.ParseAtom (buffer => (new[] {
				new Item { Name = "GraphicsMode", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 12) }
			}.ToList (), 0), atom);
		}

		/*
		 * qtff-2015
		 * p100 General Structure of a Sample Description
		 * p156 Video Media
		 */
		private (List<Item>, int) ParseAvc1 (Atom atom)
		{
			return this.ParseAtom (buffer => (new[] {
				new Item { Name = "FormatDescription", Type = ItemType.String, Value = "H.264 video" },
				/* General Structure of a Sample Description */
				new Item { Name = "DataReferenceIndex", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 14) },
				/* Video Media */
				new Item { Name = "Version", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 16) },
				new Item { Name = "RevisionLevel", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 18) },
				new Item { Name = "Vendor", Type = ItemType.String, Value = DataType.ByteString (buffer, 20) },
				new Item { Name = "TemporalQuality", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 24) },
				new Item { Name = "SpatialQuality", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 28) },
				new Item { Name = "Width", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 32) },
				new Item { Name = "Height", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 34) },
				new Item { Name = "HorizontalResolution", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 36) },
				new Item { Name = "VerticalResolution", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 40) },
				new Item { Name = "DataSize", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 44) },
				new Item { Name = "FrameCount", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 48) },
				new Item { Name = "CompressorName", Type = ItemType.String, Value = DataType.PascalString (buffer, 50) },
				new Item { Name = "Depth", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 82) },
				new Item { Name = "ColorTableID", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 84) }
			}.ToList (), 78), atom);
		}

		/* 14496-15 5.2.4.1.1 */
		private (List<Item>, int) ParseAvcC (Atom atom)
		{
			return this.ParseAtom (buffer => {
				List<Item> items = new ();

				items.Add (new Item { Name = "ConfigurationVersion", Type = ItemType.Byte, Value = buffer[8] });
				items.Add (new Item { Name = "AVCProfileIndication", Type = ItemType.Byte, Value = buffer[9] });
				items.Add (new Item { Name = "ProfileCompatibility", Type = ItemType.Byte, Value = buffer[10] });
				items.Add (new Item { Name = "AVCLevelIndication", Type = ItemType.Byte, Value = buffer[11] });
				items.Add (new Item { Name = "LengthSizeMinusOne", Type = ItemType.Byte, Value = buffer[12] & 3 });

				int offset = 13;

				int seqs = buffer[offset++] & 31;
				items.Add (new Item { Name = "NumOfSequenceParameterSets", Type = ItemType.Byte, Value = seqs });
				for (int i = 0; i < seqs; i++)
				{
					int length = DataType.ByteShort (buffer, offset);
					items.Add (new Item { Name = $"SequenceParameterSet{i + 1}Length", Type = ItemType.Short, Value = length });
					offset += 2 + length;
				}

				int pics = buffer[offset++];
				items.Add (new Item { Name = "NumOfPictureParameterSets", Type = ItemType.Byte, Value = pics });
				for (int i = 0; i < pics; i++)
				{
					int length = DataType.ByteShort (buffer, offset);
					items.Add (new Item { Name = $"PictureParameterSet{i + 1}Length", Type = ItemType.Short, Value = length });
					offset += 2 + length;
				}

				return (items, 0);
			}, atom);
		}

		private (List<Item>, int) ParseBtrt (Atom atom)
		{
			return this.ParseAtom (buffer => (new[] {
				new Item { Name = "BufferSize", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 8) },
				new Item { Name = "MaxBitRate", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 12) },
				new Item { Name = "AverageBitRate", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 16) }
			}.ToList (), 0), atom);
		}

		private (List<Item>, int) ParseColr (Atom atom)
		{
			return this.ParseAtom (buffer => (new[] {
				new Item { Name = "ColorParameterType", Type = ItemType.String, Value = DataType.ByteString (buffer, 8) },
				new Item { Name = "PrimariesIndex", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 12) },
				new Item { Name = "TransferFunctionIndex", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 14) },
				new Item { Name = "MatrixIndex", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 16) }
			}.ToList (), 0), atom);
		}

		private (List<Item>, int) ParsePasp (Atom atom)
		{
			return this.ParseAtom (buffer => (new[] {
				new Item { Name = "hSpacing", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 8) },
				new Item { Name = "vSpacing", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 8) },
			}.ToList (), 0), atom);
		}

		private (List<Item>, int) ParseFiel (Atom atom)
		{
			return this.ParseAtom (buffer => (new[] {
				new Item { Name = "FieldCount", Type = ItemType.Byte, Value = buffer[8] },
				new Item { Name = "ScanType", Type = ItemType.String, Value = buffer[8] == 1 ? "progressive" : buffer[8] == 2 ? "interlaced" : "" },
				new Item { Name = "FieldOrdering", Type = ItemType.Byte, Value = buffer[9] }
			}.ToList (), 0), atom);
		}

		private (List<Item>, int) ParseClap (Atom atom)
		{
			return this.ParseAtom (buffer => (new[] {
				new Item { Name = "ApertureWidth_N", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 8) },
				new Item { Name = "ApertureWidth_D", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 12) },
				new Item { Name = "ApertureHeight_N", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 16) },
				new Item { Name = "ApertureHeight_D", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 20) },
				new Item { Name = "HorizOff_N", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 24) },
				new Item { Name = "HorizOff_D", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 28) },
				new Item { Name = "VertOff_N", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 32) },
				new Item { Name = "VertOff_D", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 36) }
			}.ToList (), 0), atom);
		}

		/*
		 * qtff-2015
		 * p100 General Structure of a Sample Description
		 * p176 Sound Media
		 */
		private (List<Item>, int) ParseMp4a (Atom atom)
		{
			return this.ParseAtom (buffer => {
				List<Item> items = new ();
				int headSize = 28;

				items.Add (new Item { Name = "FormatDescription", Type = ItemType.String, Value = "MPEG-4, AAC" });
				/* General Structure of a Sample Description */
				items.Add (new Item { Name = "DataReferenceIndex", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 14) });

				/* Sound Media */
				int version = DataType.ByteShort (buffer, 16);
				items.Add (new Item { Name = "Version", Type = ItemType.Short, Value = version });
				items.Add (new Item { Name = "RevisionLevel", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 18) });
				items.Add (new Item { Name = "Vendor", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 20) });

				/* Version 0 assumes uncompressed audio in 'raw ' or 'twos' format, 1 or 2 channels, 8 or 16 bits per sample, and a compression ID of 0. */
				items.Add (new Item { Name = "NumberOfChannels", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 24) });
				items.Add (new Item { Name = "SampleSize", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 26) });
				items.Add (new Item { Name = "CompressionID", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 28) });
				items.Add (new Item { Name = "PacketSize", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 30) });
				items.Add (new Item { Name = "SampleRate", Type = ItemType.String, Value = $"{DataType.ByteUShort (buffer, 32)}.{DataType.ByteUShort (buffer, 34)}" });

				/* Version 1 (QuickTime 3) extended 4 fields, each 4 bytes long, p180 qtff 2015 */
				if (version > 0) {
					headSize = 44;

					items.Add (new Item { Name = "SamplesPerPacket", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 36) });
					items.Add (new Item { Name = "BytesPerPacket", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 40) });
					items.Add (new Item { Name = "BytesPerFrame", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 44) });
					items.Add (new Item { Name = "BytesPerSample", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 48) });

					/* Verison 2 (QuickTime 7) extends QuickTime capabilities to include high resolution audio with another expansion of the sound sample description structure */
					if (version > 1)
					{
						headSize = 88;

						items.Add (new Item { Name = "always3", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 52) });
						items.Add (new Item { Name = "always16", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 54) });
						items.Add (new Item { Name = "alwaysMinus2", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 56) });
						items.Add (new Item { Name = "always0", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 58) });
						items.Add (new Item { Name = "always65536", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 60) });
						items.Add (new Item { Name = "sizeOfStructOnly", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 64) });
						items.Add (new Item { Name = "audioSampleRate", Type = ItemType.Long, Value = DataType.ByteLong (buffer, 68) });
						items.Add (new Item { Name = "numAudioChannels", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 72) });
						items.Add (new Item { Name = "always7F000000", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 76) });
						items.Add (new Item { Name = "constBitsPerChannel", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 80) });
						items.Add (new Item { Name = "formatSpecificFlags", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 84) });
						items.Add (new Item { Name = "constBytesPerAudioPacket", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 88) });
						items.Add (new Item { Name = "constPCMFramesPerAudioPacket", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 92) });
					}
				}

				return (items, headSize);
			}, atom);
		}

		/* 
		MPEG-4 Elementary Stream Descriptor Atom
		ISO/IEC 14496-1, 8.6.5 ES_Descriptor
		1. video: ISO/IEC 14496-1, restrictions for storage in ISO/IEC 14496-14 (mp4v)
		2. Audio: ISO/IEC 14496-3, subclause 1.6 (mp4a)
		*/
		private (List<Item>, int) ParseEsds (Atom atom)
		{
			return this.ParseAtom (buffer => (new[] {
				new Item { Name = "ESDescrTag", Type = ItemType.Byte, Value = buffer[12]},
				new Item { Name = "ESID", Type = ItemType.UShort, Value = DataType.ByteUShort(buffer, 13) },
				new Item { Name = "ESFlags", Type = ItemType.Byte, Value = (buffer[15] >> 5) },
				new Item { Name = "StreamPriority", Type = ItemType.Byte, Value = buffer[15] & 31 }
			}.ToList(), 0), atom);
		}

		/* Core Audio Format Specification */
		private (List<Item>, int) ParseChan (Atom atom)
		{
			return this.ParseAtom (buffer => {
				List<Item> items = new ();

				items.Add (new Item { Name = "ChannelLayoutTag", Type = ItemType.Int, Value = DataType.ByteUInt (buffer, 12) });
				items.Add (new Item { Name = "ChannelBitmap", Type = ItemType.Int, Value = DataType.ByteUInt (buffer, 16) });
				uint channels = DataType.ByteUInt (buffer, 20);
				items.Add (new Item { Name = "NumberChannelDescriptions", Type = ItemType.Int, Value = channels });

				if (channels > 0)
				{
					for(int i = 0; i < channels; i++)
                    {
						uint label = DataType.ByteUInt (buffer, 24 + i * 20);
						items.Add (new Item { Name = "ChannelLabel", Type = ItemType.Int, Value = label });
						items.Add (new Item { Name = "ChannelLabelDesc", Type = ItemType.String, Value = (ChannelLabel)label });

						items.Add (new Item { Name = "ChannelFlags", Type = ItemType.Int, Value = DataType.ByteUInt (buffer, 28 + i * 20) });

						items.Add (new Item { Name = "Coordinates", Type = ItemType.String, Value = $"{DataType.ByteUInt (buffer, 32 + i * 20)} {DataType.ByteUInt (buffer, 36 + i * 20)} {DataType.ByteUInt (buffer, 40 + i * 20)}" });
					}
				}

				return (items, 0);
			}, atom);
		}

		private (List<Item>, int) ParseFrma (Atom atom)
		{
			return this.ParseAtom (buffer => (new[] {
				new Item { Name = "DataFormat", Type = ItemType.String, Value = DataType.ByteString(buffer, 8, (int)atom.Size - 8) }
			}.ToList (), 0), atom);
		}

		//TODO: details
		private (List<Item>, int) ParseC608 (Atom atom)
		{
			this.track.DataFormats.Add ("c608");
			return this.ParseAtom (buffer => (new[] {
				new Item { Name = "DataReferenceIndex", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 14) }
			}.ToList (), 0), atom);
		}
		private (List<Item>, int) ParseC708 (Atom atom)
		{
			this.track.DataFormats.Add ("c708");

			return this.ParseAtom (buffer => (new[] {
				new Item { Name = "DataReferenceIndex", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 14) }
			}.ToList (), 0), atom);
		}

		private (List<Item>, int) ParseStsd (Atom atom)
		{
			return this.ParseAtom (buffer => (new[] {
				new Item { Name = "Version", Type = ItemType.Byte, Value = buffer[8] },
				new Item { Name = "Flags", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 8) & 0xffffff },
				new Item { Name = "Entries", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 12) }
			}.ToList (), 8), atom);
		}

		private (List<Item>, int) ParseStts (Atom atom)
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

				return (items, 0);
			}, atom);
		}
		private (List<Item>, int) ParseStss (Atom atom)
		{
			return this.ParseAtom (buffer => (new[] {
				new Item { Name = "Entries", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 12) }
			}.ToList (), 0), atom);
		}
		private (List<Item>, int) ParseStsz (Atom atom)
		{
			return this.ParseAtom (buffer => (new[] {
				new Item { Name = "SampleSize", Type = ItemType.Int, Value = this.track.SampleSize = DataType.ByteInt (buffer, 12) },
				new Item { Name = "Entries", Type = ItemType.Int, Value = this.track.SampleSizeCount = DataType.ByteInt (buffer, 16) }
			}.ToList (), 0), atom);
		}
		private (List<Item>, int) ParseStsc (Atom atom)
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

				return (items, 0);
			}, atom);
		}
		private (List<Item>, int) ParseStco (Atom atom)
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

				return (items, 0);
			}, atom);
		}

		//TODO: parse for multiple cases
		private (List<Item>, int) ParseTmcd (Atom atom)
		{
			switch ((int)atom.Size)
			{
			case 0xc:  //tref
				return (new List<Item> (), 0);
			case 0x22:  //STSD
				return this.ParseAtom (buffer => (new[] {
					new Item { Name = "DataReferenceIndex", Type = ItemType.Short, Value = DataType.ByteShort (buffer, 14) }
				}.ToList (), 0), atom);
			}

			return (new List<Item> (), 0);  //gmhd etc.
		}

		private (List<Item>, int) ParseTcmi (Atom atom)
		{
			return this.ParseAtom (buffer => (new[] {
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
			}.ToList (), 0), atom);
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
		private (List<Item>, int) ParseAlis (Atom atom)
		{
			return this.ParseAtom (buffer => (new[] {
				new Item { Name = "Version", Type = ItemType.Byte, Value = buffer[8] },
				new Item { Name = "Flags", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 8) & 0xffffff }
			}.ToList (), 0), atom);
		}
		//TODO
		private (List<Item>, int) ParseRsrc (Atom atom)
		{
			return this.ParseAtom (buffer => (new[] {
				new Item { Name = "Version", Type = ItemType.Byte, Value = buffer[8] },
				new Item { Name = "Flags", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 8) & 0xffffff }
			}.ToList (), 0), atom);
		}
		//TODO
		private (List<Item>, int) ParseUrl (Atom atom)
		{
			return this.ParseAtom (buffer => (new[] {
				new Item { Name = "Version", Type = ItemType.Byte, Value = buffer[8] },
				new Item { Name = "Flags", Type = ItemType.Int, Value = DataType.ByteInt (buffer, 8) & 0xffffff }
			}.ToList (), 0), atom);
		}

		#endregion atom utilities
	}
}
