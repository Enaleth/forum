﻿using System.Collections.Generic;

namespace Forum.Models.ViewModels.Messages {
	public class HistoryPage {
		public string Id { get; set; }
		public string DisplayName { get; set; }
		public string Email { get; set; }
		public int CurrentPage { get; set; }
		public bool ShowFavicons { get; set; }
		public bool MorePages { get; set; }

		public List<DisplayMessage> Messages { get; set; }
	}
}