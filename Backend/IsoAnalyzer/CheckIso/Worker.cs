using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using IsoParser.Lib.Concretes;
using IsoParser.Lib.Models;

namespace CheckIso {
	public class Worker {
		public string GetAtoms (string file) {
			Parser parser = new ();
			string message = Display (parser.GetTree (file).Result);
			parser.End ();
			return message;
		}

		private string Display (Atom atom) {
			StringBuilder b = new ();
			b.Append ("== Details ==").Append(Environment.NewLine);
			this.Layer (b, atom, 0);

			//b.Append (Environment.NewLine).Append ("== Content ==").Append (Environment.NewLine);
			//this.Content (b, atom, 0);
			return b.ToString ();
		}

		private void Layer (StringBuilder b, Atom atom, int layer) {
			try {
				b.Append ($"{Spaces (layer)}{atom.Offset:X10} [{(atom.Type.HasValue ? atom.Type.ToString () : "ROOT")}] size {atom.Size:X} id {atom.Id:x}").Append (Environment.NewLine);

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
				b.Append ($"{Spaces (layer)}{atom.Offset:X10} [{(atom.Type.HasValue ? atom.Type.ToString () : "ROOT")}] size {atom.Size:X} id {atom.Id:x}").Append (Environment.NewLine);
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
			case ItemType.Int:
			case ItemType.Long:
			case ItemType.Short: return $"{value} ({value:x}h)";
			case ItemType.Matrix: return this.ShowMatrix (value);
			case ItemType.String: return $"({value})";
			}
			return value.ToString ();
		}

		private string ShowMatrix(object value)
        {
			double[] values = value as double [];
			StringBuilder b = new ();
			for (int i = 0; i < values.Length; i++)
			{
				if (i != 0)
					b.Append (", ");
				b.Append (values[i]);
			}
			return b.ToString ();
        }

		private string Spaces (int layer) {
			StringBuilder b = new ();
			for (int i = 0; i < layer; i++)
				b.Append ("  ");
			return b.ToString ();
		}
	}
}
