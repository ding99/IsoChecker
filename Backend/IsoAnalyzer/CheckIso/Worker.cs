﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using IsoParser.Lib.Concretes;
using IsoParser.Lib.Models;
//using Parsers.MccParser;

using Utils.CEA608;
using Utils.CEA708;
using Utils.CEA708.Models;

namespace CheckIso {
	public class Worker {
		private const int maxRepeat = 5;
//		private readonly MccParser mcc;

		private readonly bool _detail;

		public Worker (bool detail = false)
        {
//			this.mcc = new (false);

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
			b.Append (Environment.NewLine);
			b.AppendLine ("== Tracks ==");
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
				b.AppendLine (note);
			#endregion

			#region subtitle
			if(iso.Subtitle != null)
            {
				b.Append (Environment.NewLine);
				b.AppendLine ($"== Subtitles({iso.Subtitle.Subtitles.Count}) ==");

				foreach (var s in iso.Subtitle.Subtitles)
				{
					b.Append ($"-- Type {s.Type}, Frames({s.Frames.Count})");
                    this.ShowTitles (b, s, "c608");
                }

//				b.Append (this.mcc.End ());
			}
			#endregion

			return b.ToString ();
		}

		private void ShowTitles (StringBuilder b, Subtitle sub, string type)
		{
			b.Append (Environment.NewLine);

			if (sub.Frames.Count < 1)
				return;

			int count = 0;
			if (type.Equals ("c708"))
			{
				//b.Append (this.DisplaySub (sub.Frames, "(fa0000-18)", "Frames", a => this.mcc.DisplayLine (a)));
				//b.Append (this.DisplaySub (sub.Frames, "Data:(00-36)", "Line Structure", a => this.mcc.LineC708 (a, "")));
			}
			if (type.Equals ("c608"))
			{
				//b.Append (this.DisplaySub (sub.Frames, "8080", "Line Structure", a => this.mcc.LineC608 (a, count++ % 2 == 0)));
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

		private string DisplaySub (List<byte[]> lines, string key, string name, Func<byte[], string> getValue)
		{
            Console.WriteLine ($"DisplaySub terms {lines.Count}");
			StringBuilder b = new StringBuilder ($"-- Display list of {name}").Append (Environment.NewLine);

			int count = 0;
			bool dsp;

			foreach (var line in lines)
			{
				string data = getValue (line);

				//if (key.Equals ("Data:(00-36)"))
				//{
				//    Console.WriteLine ($"DisplaySub [{data}] ({line.Length})");
				//}

				if (data.IndexOf (key) >= 0)
				{
					dsp = count++ < 5;
				}
				else
				{
					count = 0;
					dsp = true;
				}

				if (this._detail)
				{
					if (dsp)
						b.AppendLine (data);
					if (count == maxRepeat)
						b.AppendLine ("......");
				}
			}

			return b.ToString ();
		}

	}
}
