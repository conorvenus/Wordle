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
using Wordle.Client.Services;
using Wordle.Shared;

namespace Wordle.Client.Pages
{
	public partial class Index : ComponentBase
	{
		[Inject]
		private HttpClient httpClient { get; set; }

		[Inject]
		private ReplayService replayService { get; set; }

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
				// Tell the replay service that the user typed a letter.
				replayService.Replay.AddEvent(EventType.TypeLetter, e.Key[0]);

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
				// Last letter in row.
				WordleLetter lastLetter = wordleGrid.Guesses[wordleGrid.RowIndex].Last();

				// Tell the replay service that the user deleted a letter.
				replayService.Replay.AddEvent(EventType.DeleteLetter, lastLetter.Letter);

				// Remove the respective letter from the current guess row.
				wordleGrid.Guesses[wordleGrid.RowIndex].Remove(lastLetter);
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
					// Tell the replay service that the user attempted to guess a word.
					replayService.Replay.AddEvent(EventType.Submit);
					// If all letters match, game over, they won!
					if (guessState.Count(letterState => letterState.State == LetterState.CorrectPosition) == 5)
					{
						replayService.Replay.Word = string.Join("", guessState.Select(letterState => letterState.Letter));
						Modal.Show($"{guess} was correct!");
						StateHasChanged();
						replayService.GridUpdated += () => StateHasChanged();
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
