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
				if (layer > 0)
					for (int i = 0; i < layer; i++)
						b.Append("  ");

				b.Append($"[{a.Id?? "none"}] (size {a.Size})").Append(Environment.NewLine);

				if (a.Atoms != null && a.Atoms.Count > 0)
					Layer(b, a.Atoms, layer + 1);
			}
		}
	}
}
