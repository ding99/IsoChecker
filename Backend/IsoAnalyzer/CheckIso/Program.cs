﻿using System;
using System.IO;

namespace CheckIso {
	class Program {
		static void Main (string[] args) {
			Console.WriteLine ("CheckIso");

			if (args.Length < 1)
				Console.WriteLine ("Usage:  CheckIso <file_path>");
			else {
				try {
					Console.WriteLine ($"Arguments ({args.Length})");
					foreach (var arg in args)
						Console.WriteLine($"  [{arg}]");

					string result = new Worker ().GetAtoms (args[0]);

					Console.WriteLine (result);

					// temp
					if (args.Length > 1 && args[1].Trim ().ToLower ().Equals ("s"))
					{
						var a = new StreamWriter ($"{args[0]}_parsed.txt");
						a.WriteLine (result);
						a.Flush ();
						a.Close ();
					}
				}
				catch (Exception e) {
					Console.WriteLine (e.Message);
					Console.WriteLine (e.StackTrace);
				}
			}
		}
	}
}
