﻿using System;
using System.ComponentModel.DataAnnotations;
using Forum3.Enums;

namespace Forum3.Models.DataModels {
	public class Notification {
		public int Id { get; set; }

		[Required]
		public string UserId { get; set; }

		[Required]
		public string TargetUserId { get; set; }

		public int? MessageId { get; set; }

		public DateTime Time { get; set; }
		public bool Unread { get; set; }
		public ENotificationType Type { get; set; }
	}
}