using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using IsoParser.Lib.Concretes;
using IsoParser.Lib.Models;

using Utils.CEA708;
using Utils.CEA708.Models;

namespace CheckIso {
	public class Worker {
		private const int maxRepeat = 4;

		private readonly bool _detail;

		public Worker (bool detail = false)
        {
			this._detail = detail;
		}

		public string GetAtoms (string file) {
			Parser parser = new ();
			IsoInfo iso = parser.GetTree (file).Result;
			string message = this.Display (iso);
			parser.End ();
			return message;
		}

		private string Display (IsoInfo iso) {
			StringBuilder b = new ();
			b.AppendLine ("== Atoms ==");
			this.Layer (b, iso.Atom, 0);

			#region display tracks
			b.AppendLine ();
			b.AppendLine ("== Tracks ==");
			b.Append ($"Tracks({iso.Tracks.Count}) :");
			foreach (var tr in iso.Tracks)
				b.Append ($" {tr.Type}-{tr.SubType}");
			b.AppendLine ();

			foreach (var tr in iso.Tracks)
				if (tr.Type == ComponentType.Media && tr.SubType == ComponentSubType.Caption)
				{
					b.Append ($"Subtile({tr.DataFormats.Count}) :");
					foreach (var f in tr.DataFormats)
						b.Append ($" {f}");
					b.AppendLine ();

					b.Append ($"TimeToSamples({tr.TimeToSamples.Count}) :");
					foreach (var s in tr.TimeToSamples)
						b.Append ($" {s.SampleCount}({s.SampleCount:x})/{s.SampleDuration}({s.SampleDuration:x})");
					b.AppendLine ();

					b.Append ($"SampleToChunks({tr.SampleToChunks.Count}) :");
					foreach (var s in tr.SampleToChunks)
						b.Append ($" {s.FirstChunk}({s.FirstChunk:x})/{s.SamplesPerChunk}({s.SamplesPerChunk:x})/{s.DescriptionId}({s.DescriptionId:x})");
					b.AppendLine ();

					b.Append ($"ChunkOffsets({tr.ChunkOffsets.Count}) :");
					foreach (var s in tr.ChunkOffsets)
						b.Append ($" {s:x}");
					b.AppendLine ();
				}

			#endregion

			#region display notes
			if(iso.Notes != null)
				foreach (var note in iso.Notes)
					b.AppendLine (note);
			#endregion

			#region subtitle
			if(iso.Subtitle != null)
            {
				b.AppendLine ();
				b.AppendLine ($"== Subtitles({iso.Subtitle.Subtitles.Count}) ==");

				foreach (var s in iso.Subtitle.Subtitles)
				{
					b.Append ($"-- Type {s.Type}, Frames({s.Frames.Count})");
                    this.ShowTitles (b, s);
                }
			}
			#endregion

			return b.ToString ();
		}

		private void ShowTitles (StringBuilder b, Subtitle sub)
		{
			b.AppendLine ();
			if (sub.Frames.Count < 1)
				return;

            if (sub.Type.Equals ("c708"))
            {
				List<C708Line> c708lines = new ();

				int number = 0;
				foreach(var frame in sub.Frames)
					c708lines.Add (new C708Line (number++.ToString(), frame));

                Console.WriteLine ($"detail {this._detail}");
				C708Framework framework = new C708Parser ().Decode (c708lines);
				if(this._detail)
					b.AppendLine (framework.ShowPockets (maxRepeat));
				b.AppendLine (framework.ShowBlocks ());
				b.AppendLine (framework.ShowFields ());
            }
		}

		private void Layer (StringBuilder b, Atom atom, int layer) {
			try {
				b.AppendLine ($"{Spaces (layer)}{atom.Offset:X10} [{(atom.Type.HasValue ? atom.Type.ToString () : "NONE")}] size {atom.Size:X} id {atom.Id:x}");

				if(atom.Items != null)
				foreach (var item in atom.Items)
					b.AppendLine ($"{Spaces (layer + 1)}{item.Name}: {ShowValue (item.Value, item.Type)}");
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
				b.AppendLine ($"{Spaces (layer)}{atom.Offset:X10} [{(atom.Type.HasValue ? atom.Type.ToString () : "NONE")}] size {atom.Size:X} id {atom.Id:x}");
			}
			catch (Exception e) {
				Console.WriteLine (e.Message);
			}

			if (atom.Atoms != null)
				foreach (var a in atom.Atoms)
					Content (b, a, layer + 1);
		}

		private string ShowValue (object value, ItemType type) {
            return type switch
            {
                ItemType.Byte or ItemType.Int or ItemType.Long or ItemType.UShort or ItemType.Short => $"{value} ({value:x}h)",
                ItemType.Matrix => string.Join (", ", (value as double[]).ToArray ()),
                ItemType.String => $"[{value}]",
                _ => value.ToString (),
            };
        }

		private string Spaces (int layer) {
			return string.Join("", Enumerable.Repeat(' ', layer * 2 ));
		}
	}
}
