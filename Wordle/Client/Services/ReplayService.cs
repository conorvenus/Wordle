using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wordle.Shared;

namespace Wordle.Client.Services
{
	public enum EventType : byte
	{
		TypeLetter,
		DeleteLetter,
		Submit
	}

	public class Event
	{
		public DateTime Timestamp { get; set; }
		public EventType Type { get; set; }
		public char? Letter { get; set; }

		public Event(DateTime timestamp, EventType type, char? letter = null)
		{
			Timestamp = timestamp;
			Type = type;
			Letter = letter;
		}
	}

	public class Replay
	{
		public readonly DateTime CreatedAt = DateTime.Now;
		public readonly List<Event> Events = new List<Event>();
		public string Word { get; set; }
		public void AddEvent(EventType type, char? letter = null)
		{
			Events.Add(new Event(DateTime.Now, type, letter));
		}
	}

	public class ReplayService
	{
		public readonly Replay Replay = new Replay();
		public readonly WordleGrid Grid = new WordleGrid(6);

		public bool isReplaying = false;
		public Action GridUpdated { get; set; }

		public async Task StartReplayingAsync()
		{
			GridUpdated.Invoke();
			// Set the service to be replaying.
			isReplaying = true;
			// Set the previous event time to the start of the replay as the initial offset.
			DateTime previousEventTime = Replay.CreatedAt;
			// Order the events by timestamp since they are not reliably in order.
			foreach (Event @event in Replay.Events.OrderBy(e => e.Timestamp))
			{
				// Wait for the offset between two events in milliseconds, this creates the effect of it being replayed.
				await Task.Delay((int)(@event.Timestamp - previousEventTime).TotalMilliseconds);
				// Set the previous timestamp to the current timestamp for the next offset waiting.
				previousEventTime = @event.Timestamp;
				switch (@event.Type)
				{
					case EventType.TypeLetter:
						// If a new grid row does not exist, create it.
						if (Grid.Guesses.Count <= Grid.RowIndex)
							Grid.Guesses.Add(new List<WordleLetter>());
						// If amount of letter for that row is less than 5, you can add another letter.
						if (Grid.Guesses[Grid.RowIndex].Count < 5)
							Grid.Guesses[Grid.RowIndex].Add(new WordleLetter(@event.Letter.Value.ToString().ToUpper()[0], LetterState.Undefined));
						break;
					case EventType.DeleteLetter:
						// If there is at least 1 letter in the row, delete the letter in the last position.
						if (Grid.Guesses[Grid.RowIndex].Count > 0)
							Grid.Guesses[Grid.RowIndex].RemoveAt(Grid.Guesses[Grid.RowIndex].Count - 1);
						break;
					case EventType.Submit:
						// Calculates correct letters and their respective states.
						List<WordleLetter> guessState = new List<WordleLetter>();
						// Declare guess and word.
						string word = Replay.Word.ToUpper();
						string guess = string.Join("", Grid.Guesses[Grid.RowIndex].Select(letterState => letterState.Letter)).ToUpper();
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
						Grid.Guesses[Grid.RowIndex] = guessState;
						Grid.RowIndex += 1;
						break;
				}
				// Notify the frontend UI that the replay grid has been updated.
				GridUpdated.Invoke();
			}
		}
	}
}
