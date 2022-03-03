using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Wordle.Client.Components;
using Wordle.Shared;

namespace Wordle.Client.Pages
{
	public partial class Index : ComponentBase
	{
		[Inject]
		private HttpClient httpClient { get; set; }

		private ElementReference inputHandler { get; set; }

		private Modal Modal { get; set; }

		private WordleGrid wordleGrid { get; set; } = new WordleGrid(6);

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			await inputHandler.FocusAsync();
		}

		protected async Task KeyDownAsync(KeyboardEventArgs e)
		{
			// If the key is a letter.
			if (Regex.IsMatch(e.Key, "^[a-zA-Z]$") && wordleGrid.RowIndex < wordleGrid.MaximumGuesses)
			{
				// If the row does not exist, create it.
				if (wordleGrid.Guesses.Count <= wordleGrid.RowIndex)
					wordleGrid.Guesses.Add(new List<WordleLetter>());

				// If there are less than 5 letters in the row, add the new letter.
				if (wordleGrid.Guesses[wordleGrid.RowIndex].Count < 5)
					wordleGrid.Guesses[wordleGrid.RowIndex].Add(new WordleLetter(e.Key.ToUpper()[0], LetterState.Undefined));
			}
			// If they are trying to remove a letter.
			else if ((e.Key == "Backspace" || e.Key == "Delete") && wordleGrid.Guesses.Any() && wordleGrid.Guesses[wordleGrid.RowIndex].Any())
			{
				wordleGrid.Guesses[wordleGrid.RowIndex].RemoveAt(wordleGrid.Guesses[wordleGrid.RowIndex].Count - 1);
			}
			// If they are trying to submit their word.
			else if (e.Key == "Enter" && wordleGrid.Guesses.Count > wordleGrid.RowIndex && wordleGrid.Guesses[wordleGrid.RowIndex].Count == 5)
			{
				// Join their guess letters into a single word.
				string guess = string.Join("", wordleGrid.Guesses[wordleGrid.RowIndex].Select(letter => letter.Letter));
				// Send a request to the backend.
				try
				{
					List<WordleLetter> guessState = await httpClient.GetFromJsonAsync<List<WordleLetter>>($"api/daily/{guess}");
					wordleGrid.Guesses[wordleGrid.RowIndex] = guessState.Select(letter => new WordleLetter(letter.Letter.ToString().ToUpper()[0], letter.State)).ToList();
					wordleGrid.RowIndex += 1;
					// If all letters match, game over, they won!
					if (guessState.Count(letterState => letterState.State == LetterState.CorrectPosition) == 5)
					{
						Modal.Show($"{guess} was correct!");
						StateHasChanged();
					}
				}
				catch (HttpRequestException ex)
				{
					if (ex.StatusCode != System.Net.HttpStatusCode.BadRequest)
						Console.WriteLine(ex);
				}
			}
		}
	}
}
