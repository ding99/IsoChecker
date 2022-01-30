using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using IsoParser.Lib.Concretes;
using IsoParser.Lib.Models;
using Parsers.MccParser;

namespace CheckIso {
	public class Worker {
		private readonly MccParser mcc;

		public Worker ()
        {
			this.mcc = new (false);
        }

		public string GetAtoms (string file) {
			Parser parser = new ();
			string message = Display (parser.GetTree (file).Result);
			parser.End ();
			return message;
		}

		private string Display (IsoInfo iso) {
			StringBuilder b = new ();
			b.Append ("== Atoms ==").Append(Environment.NewLine);
			this.Layer (b, iso.Atom, 0);

			#region display tracks
			b.Append (Environment.NewLine);
			b.Append ("== Tracks ==").Append (Environment.NewLine);
			b.Append ($"Tracks({iso.Tracks.Count}) :");
			foreach (var tr in iso.Tracks)
				b.Append ($" {tr.Type}-{tr.SubType}");
			b.Append (Environment.NewLine);

			foreach (var tr in iso.Tracks)
				if (tr.Type == ComponentType.Media && tr.SubType == ComponentSubType.Caption)
				{
					b.Append ($"Subtile({tr.DataFormats.Count}) :");
					foreach (var f in tr.DataFormats)
						b.Append ($" {f}");
					b.Append (Environment.NewLine);

					b.Append ($"TimeToSamples({tr.TimeToSamples.Count}) :");
					foreach (var s in tr.TimeToSamples)
						b.Append ($" {s.SampleCount}({s.SampleCount:x})/{s.SampleDuration}({s.SampleDuration:x})");
					b.Append (Environment.NewLine);

					b.Append ($"SampleToChunks({tr.SampleToChunks.Count}) :");
					foreach (var s in tr.SampleToChunks)
						b.Append ($" {s.FirstChunk}({s.FirstChunk:x})/{s.SamplesPerChunk}({s.SamplesPerChunk:x})/{s.DescriptionId}({s.DescriptionId:x})");
					b.Append (Environment.NewLine);

					b.Append ($"ChunkOffsets({tr.ChunkOffsets.Count}) :");
					foreach (var s in tr.ChunkOffsets)
						b.Append ($" {s:x}");
					b.Append (Environment.NewLine);
				}
			#endregion

			#region display notes
			if(iso.Notes != null)
			foreach (var note in iso.Notes)
				b.Append (note).Append (Environment.NewLine);
			#endregion

			#region subtitle
			if(iso.Subtitle != null)
            {
				b.Append (Environment.NewLine);
				b.Append ($"== Subtitles({iso.Subtitle.Subtitles.Count}) ==").Append(Environment.NewLine);
				foreach (var s in iso.Subtitle.Subtitles)
				{
					b.Append ($"-- Type {s.Type}, Frames({s.Frames.Count})").Append(Environment.NewLine);
					b.Append (this.DisplaySub (s.Frames, "(fa0000-18)", "Frames", a => this.mcc.DspLine (a)));
					b.Append (this.DisplaySub (s.Frames, "Data:(00-36)", "Line Structure", a => this.mcc.LineStructure (a, "")));
				}
				b.Append (this.mcc.End ());
            }
			#endregion

			return b.ToString ();
		}

		private void Layer (StringBuilder b, Atom atom, int layer) {
			try {
				b.Append ($"{Spaces (layer)}{atom.Offset:X10} [{(atom.Type.HasValue ? atom.Type.ToString () : "NONE")}] size {atom.Size:X} id {atom.Id:x}").Append (Environment.NewLine);

				if(atom.Items != null)
				foreach (var item in atom.Items)
					b.Append ($"{Spaces (layer + 1)}{item.Name}: {ShowValue (item.Value, item.Type)}")
						.Append (Environment.NewLine);
			}
			catch(Exception e) {
                Console.WriteLine (e.Message);
            }

			if(atom.Atoms != null)
			foreach (var a in atom.Atoms)
				Layer (b, a, layer + 1);
		}

		private void Content (StringBuilder b, Atom atom, int layer) {
			try {
				b.Append ($"{Spaces (layer)}{atom.Offset:X10} [{(atom.Type.HasValue ? atom.Type.ToString () : "NONE")}] size {atom.Size:X} id {atom.Id:x}").Append (Environment.NewLine);
			}
			catch (Exception e) {
				Console.WriteLine (e.Message);
			}

			if (atom.Atoms != null)
				foreach (var a in atom.Atoms)
					Content (b, a, layer + 1);
		}

		// TODO
		private string ShowValue (object value, ItemType type) {
			switch (type) {
			case ItemType.Byte:
			case ItemType.Int:
			case ItemType.Long:
			case ItemType.UShort:
			case ItemType.Short: return $"{value} ({value:x}h)";
			case ItemType.Matrix: return string.Join (", ", (value as double[]).ToArray ());
			case ItemType.String: return $"[{value}]";
			}
			return value.ToString ();
		}

		private string Spaces (int layer) {
			StringBuilder b = new ();
			for (int i = 0; i < layer; i++)
				b.Append ("  ");
			return b.ToString ();
		}

		private string DisplaySub (List<byte[]> lines, string key, string name, Func<byte[], string> getValue)
		{
			StringBuilder b = new StringBuilder ($"-- Display list of {name}").Append (Environment.NewLine);

			int count = 0;
			bool dsp;

			foreach (var line in lines)
			{
				string data = getValue (line);

                if (data.IndexOf (key) > 0)
                {
                    dsp = count++ < 5;
                }
                else
                {
                    count = 0;
                    dsp = true;
                }

                if (dsp)
                    b.Append (data).Append (Environment.NewLine);
                if (count == 6)
                    b.Append ("......").Append (Environment.NewLine);
			}

			return b.ToString ();
		}

	}
}
