using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wordle.Shared
{
	public enum LetterState
	{
		Undefined,
		CorrectPosition,
		InWord,
		NotInWord
	}

	public class WordleLetter
	{
		public char Letter { get; set; }
		public LetterState State { get; set; }

		public WordleLetter(char letter, LetterState state)
		{
			Letter = letter;
			State = state;
		}
	}

	public class WordleGrid
	{
		public int MaximumGuesses { get; set; }
		public List<List<WordleLetter>> Guesses { get; set; }
		public int RowIndex { get; set; } = 0;

		public WordleGrid(int maximumGuesses)
		{
			MaximumGuesses = maximumGuesses;
			Guesses = new List<List<WordleLetter>>();
		}
	}
}
