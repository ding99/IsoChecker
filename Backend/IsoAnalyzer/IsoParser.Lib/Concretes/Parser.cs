﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IsoParser.Lib.Services;
using IsoParser.Lib.Models;

namespace IsoParser.Lib.Concretes {
	public class Parser : IParser {
		public async Task<List<Atom>> GetTree(string path) {
			return await Task.Run(() => new Atom[] {
					new Atom {
						Id = "head",
						Size = 16,
						Offset = 0,
						Items = new Item[] {
							new Item { Name = "Company", Type = ItemType.String, Value = "MyCompanly" },
							new Item { Name = "Employees", Type = ItemType.Int, Value = 567 }
						}.ToList(),
						Atoms = new Atom[] {
							new Atom {
								Id = "vide",
								Size = 100,
								Offset = 32,
								Items = new Item[] {
									new Item { Name = "Codec", Type = ItemType.String, Value = "AVC" },
									new Item { Name = "Bitrate", Type = ItemType.Double, Value = 2048000.5 },
								}.ToList()
							},
							new Atom {
								Id = "audo",
								Size = 200,
								Offset = 9120,
								Items = new Item[] {
									new Item { Name = "Codec", Type = ItemType.String, Value = "MPL" },
									new Item { Name = "Channels", Type = ItemType.Int, Value = 6 },
								}.ToList()
							}
						}.ToList()
					}
				}.ToList()
			);
		}
	}
}
