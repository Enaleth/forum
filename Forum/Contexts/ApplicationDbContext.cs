﻿using Forum.Enums;
using Forum.Models.DataModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Forum.Contexts {
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string> {
		public DbSet<Board> Boards { get; set; }
		public DbSet<BoardRole> BoardRoles { get; set; }
		public DbSet<Category> Categories { get; set; }
		public DbSet<MessageBoard> MessageBoards { get; set; }
		public DbSet<Message> Messages { get; set; }
		public DbSet<MessageThought> MessageThoughts { get; set; }
		public DbSet<Notification> Notifications { get; set; }
		public DbSet<Participant> Participants { get; set; }
		public DbSet<Pin> Pins { get; set; }
		public DbSet<Quote> Quotes { get; set; }
		public DbSet<Smiley> Smileys { get; set; }
		public DbSet<StrippedUrl> StrippedUrls { get; set; }
		public DbSet<ViewLog> ViewLogs { get; set; }

		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

		/// <summary>
		/// Standard SaveChanges, with a blunt-force concurrency exception handler to retry 3 times before failing.
		/// </summary>
		public override int SaveChanges() {
			var attempts = 0;

			while (true) {
				try {
					attempts++;
					return base.SaveChanges();
				}
				catch (DbUpdateConcurrencyException ex) when (attempts <= 3) {
					foreach (var entry in ex.Entries) {
						ex.Entries.Single().Reload();
					}
				}
			}
		}

		/// <summary>
		/// Standard SaveChangesAsync, with a blunt-force concurrency exception handler to retry 3 times before failing.
		/// </summary>
		public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) {
			var attempts = 0;

			while (true) {
				try {
					attempts++;
					return await base.SaveChangesAsync(cancellationToken);
				}
				catch (DbUpdateConcurrencyException ex) when (attempts <= 3) {
					foreach (var entry in ex.Entries) {
						await ex.Entries.Single().ReloadAsync();
					}
				}
			}
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<ApplicationUser>()
				.HasIndex(r => r.DisplayName);

			modelBuilder.Entity<ApplicationUser>()
				.Property(r => r.FrontPage)
				.HasDefaultValue(EFrontPage.Boards);

			modelBuilder.Entity<ApplicationUser>()
				.Property(r => r.MessagesPerPage)
				.HasDefaultValue(25);

			modelBuilder.Entity<ApplicationUser>()
				.Property(r => r.PopularityLimit)
				.HasDefaultValue(30);

			modelBuilder.Entity<ApplicationUser>()
				.Property(r => r.ShowFavicons)
				.HasDefaultValue(true);

			modelBuilder.Entity<ApplicationUser>()
				.Property(r => r.TopicsPerPage)
				.HasDefaultValue(7);

			modelBuilder.Entity<BoardRole>()
				.HasIndex(r => r.BoardId);

			modelBuilder.Entity<MessageBoard>()
				.HasIndex(r => r.BoardId);

			modelBuilder.Entity<MessageBoard>()
				.HasIndex(r => r.MessageId);

			modelBuilder.Entity<Message>()
				.HasIndex(r => r.Processed);

			modelBuilder.Entity<Message>()
				.HasIndex(r => r.LastReplyPosted);

			modelBuilder.Entity<Message>()
				.HasIndex(r => r.PostedById);

			modelBuilder.Entity<Pin>()
				.HasIndex(r => r.MessageId);

			modelBuilder.Entity<Pin>()
				.HasIndex(r => r.UserId);

			modelBuilder.Entity<Quote>()
				.HasIndex(r => r.Approved);

			modelBuilder.Entity<Participant>()
				.HasIndex(r => r.MessageId );

			modelBuilder.Entity<Participant>()
				.HasIndex(r => r.UserId);

			modelBuilder.Entity<Participant>()
				.HasIndex(r => new { r.UserId, r.MessageId });

			modelBuilder.Entity<ViewLog>()
				.HasIndex(r => new { r.LogTime, r.UserId });

			modelBuilder.Entity<ViewLog>()
				.HasIndex(r => new { r.UserId, r.TargetType, r.TargetId });

			modelBuilder.Entity<ViewLog>()
				.HasIndex(r => r.LogTime);

			modelBuilder.Entity<Notification>()
				.HasIndex(r => r.UserId);
		}
	}
}