using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wordle.Server.Services;
using Wordle.Shared;

namespace Wordle.Server.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class WordleController : ControllerBase
	{
		private readonly WordleService wordleService;
		public WordleController(WordleService _wordleService)
		{
			wordleService = _wordleService;
		}

		[HttpGet("/api/daily/{guess}")]
		public ActionResult<List<WordleLetter>> Get(string guess)
		{
			// Set the guess to lowercase as this is the format the words are in.
			guess = guess.ToLower();
			// If the guessed word is 5 letters.
			if (Regex.IsMatch(guess, "^[a-zA-Z]{5}$"))
			{
				// Gets a list of 5 letter words.
				List<string> words = wordleService.GetWords();
				// Generates a random 'daily' word seeded by the current date.
				string word = words[new Random((int)(DateTime.Now.Date - new DateTime(1970, 1, 1)).TotalSeconds).Next(words.Count)];
				// Calculates correct letters and their respective states.
				List<WordleLetter> guessState = new List<WordleLetter>();
				// Iterate through every letter in the wordle.
				for (int i = 0; i < 5; i++)
				{
					// If the characters at the same position match.
					if (guess[i] == word[i])
					{
						while (guessState.Count(letterState => letterState.Letter == guess[i] && letterState.State == LetterState.InWord) >= word.Count(c => c == guess[i]))
						{
							guessState.FirstOrDefault(letterState => letterState.Letter == guess[i]).State = LetterState.NotInWord;
						}
						guessState.Add(new WordleLetter(guess[i], LetterState.CorrectPosition));
					}
					// If the character exists in the wordle, but not at that position.
					else if (word.Contains(guess[i]) && guessState.Count(letterState => letterState.Letter == guess[i]) < word.Count(c => c == guess[i]))
						guessState.Add(new WordleLetter(guess[i], LetterState.InWord));
					// If character does not exist in the world.
					else
						guessState.Add(new WordleLetter(guess[i], LetterState.NotInWord));
				}
				// Return the final guess state.
				return guessState;
			}
			// Return bad request, invalid guess since the length is not 5.
			return StatusCode(400, "Invalid Guess");
		}
	}
}
