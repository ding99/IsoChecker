using System;
using System.Collections.Generic;
using System.Text;

using IsoParser.Lib.Concretes;
using IsoParser.Lib.Models;

namespace CheckIso {
	public class Worker {
		public string GetAtoms(string file) {
			return Display(new Parser().GetTree(file).Result);
		}

		private string Display(List<Atom> atoms) {
			StringBuilder b = new ();

			if(atoms != null)
				this.Layer(b, atoms, 0);

			return b.ToString();
		}

		private void Layer(StringBuilder b, List<Atom> atoms, int layer) {
			foreach (var a in atoms) {
				b.Append(Spaces(layer));

				b.Append($"{a.Offset:X10} [{(a.Type.HasValue ? a.Type.ToString() : "NONE")}] size {a.Size:X}").Append(Environment.NewLine);

				foreach (var item in a.Items)
					b.Append($"{Spaces(layer + 1)}{item.Name}: {ShowValue(item.Value, item.Type)}")
						.Append(Environment.NewLine);

				if (a.Atoms != null && a.Atoms.Count > 0)
					Layer(b, a.Atoms, layer + 1);
			}
		}

		// TODO
		private string ShowValue(object value, ItemType type) {
			return value.ToString();
		}

		private string Spaces(int layer) {
			StringBuilder b = new ();
			for (int i = 0; i < layer; i++)
				b.Append("  ");
			return b.ToString();
		}
	}
}
