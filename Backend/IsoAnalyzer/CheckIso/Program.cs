using System;
using System.IO;

namespace CheckIso {
	class Program {
		static void Main (string[] args) {
			Console.WriteLine ("CheckIso");

			if (args.Length < 1)
			{
				Console.WriteLine ("Usage:  CheckIso <file_path> [d] [s]");
                Console.WriteLine ("  file_path: file to be analyzed. Mandatory");
				Console.WriteLine ("  d: show detail information. Optional");
				Console.WriteLine ("  s: save result to a local file. Optional");
			}
			else
			{
				try
				{
					Console.WriteLine ($"Arguments ({args.Length})");
					foreach (var arg in args)
						Console.WriteLine ($"  [{arg}]");

					bool detail = false;
					bool record = false;

					if(args.Length > 1)
						for (int i = 1; i < args.Length; i++)
							switch(args[i].Trim ().ToLower ())
                            {
							case "d":
								detail = true;
								break;
							case "s":
								record = true;
								break;
							}

					string result = new Worker (detail).GetAtoms (args[0]);

					Console.WriteLine (result);

					if (record)
					{
						string file = $"{args[0]}_parsed.txt";
						var a = new StreamWriter (file);
						a.WriteLine (result);
						a.Flush ();
						a.Close ();
                        Console.WriteLine ($"Save reuslt to {file}");
					}
				}
				catch (Exception e)
				{
					Console.WriteLine (e.Message);
					Console.WriteLine (e.StackTrace);
				}
			}
		}
	}
}
