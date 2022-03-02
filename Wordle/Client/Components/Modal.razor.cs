using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wordle.Client.Components
{
	public partial class Modal : ComponentBase
	{
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
	}
}
