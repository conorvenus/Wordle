using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Wordle.Server.Services
{
	public class WordleService
	{
		private List<string> Words { get; set; }

		public List<string> GetWords()
		{
			if (Words is null)
				Words = File.ReadAllLines(Directory.GetCurrentDirectory() + "/words.txt").ToList();
			return Words;
		}
	}
}
