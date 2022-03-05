using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wordle.Client.Services;

namespace Wordle.Client.Components
{
	public partial class Modal : ComponentBase
	{
		[Inject]
		private ReplayService replayService { get; set; }

		private bool isHidden { get; set; } = true;
		private string Message { get; set; }

		public void Show(string message)
		{
			Message = message;
			isHidden = false;
			StateHasChanged();
		}

		public void Hide()
		{
			isHidden = true;
			StateHasChanged();
		}

		public async Task ReplayAsync()
		{
			Hide();
			await replayService.StartReplayingAsync();
		}
	}
}
