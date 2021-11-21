using System;

namespace CheckIso {
	class Program {
		static void Main(string[] args) {
			Console.WriteLine("CheckIso");

			if (args.Length < 1)
				Console.WriteLine("Usage:  CheckIso <file_path>");
			else {
				try {
					Console.WriteLine($"Arguments ({args.Length})");
					foreach(var a in args)
						Console.WriteLine($"  [{a}]");

					Console.WriteLine(new Worker().GetAtoms(args[0]));
				}
				catch(Exception e) {
					Console.WriteLine(e.Message);
					Console.WriteLine(e.StackTrace);
				}
			}
		}
	}
}
