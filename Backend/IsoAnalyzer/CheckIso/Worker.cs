﻿using System;
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
			this.Layer (b, atom, 0);
			return b.ToString ();
		}

		private void Layer (StringBuilder b, Atom atom, int layer) {
			try {
				b.Append ($"{Spaces (layer)}{atom.Offset:X10} [{(atom.Type.HasValue ? atom.Type.ToString () : "NONE")}] size {atom.Size:X}").Append (Environment.NewLine);

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

		// TODO
		private string ShowValue (object value, ItemType type) {
			return value.ToString ();
		}

		private string Spaces (int layer) {
			StringBuilder b = new ();
			for (int i = 0; i < layer; i++)
				b.Append ("  ");
			return b.ToString ();
		}
	}
}
