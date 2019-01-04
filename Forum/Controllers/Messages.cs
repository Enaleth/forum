﻿using Forum.Annotations;
using Forum.Contexts;
using Forum.Errors;
using Forum.Interfaces.Services;
using Forum.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Forum.Controllers {
	using InputModels = Models.InputModels;
	using ViewModels = Models.ViewModels;

	public class Messages : Controller {
		ApplicationDbContext DbContext { get; }
		BoardRepository BoardRepository { get; }
		MessageRepository MessageRepository { get; }
		SmileyRepository SmileyRepository { get; }
		IForumViewResult ForumViewResult { get; }
		IUrlHelper UrlHelper { get; }

		public Messages(
			ApplicationDbContext dbContext,
			BoardRepository boardRepository,
			MessageRepository messageRepository,
			SmileyRepository smileyRepository,
			IActionContextAccessor actionContextAccessor,
			IForumViewResult forumViewResult,
			IUrlHelperFactory urlHelperFactory
		) {
			DbContext = dbContext;
			BoardRepository = boardRepository;
			MessageRepository = messageRepository;
			SmileyRepository = smileyRepository;
			ForumViewResult = forumViewResult;
			UrlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
		}

		[HttpGet]
		public async Task<IActionResult> Create(int id = 0) {
			ViewData["Smileys"] = await SmileyRepository.GetSelectorList();

			var board = (await BoardRepository.Records()).First(item => item.Id == id);

			if (Request.Query.TryGetValue("source", out var source)) {
				return await Create(new InputModels.MessageInput { BoardId = id, Body = source });
			}

			var viewModel = new ViewModels.Messages.CreateTopicPage {
				Id = "0",
				BoardId = id.ToString()
			};

			return await ForumViewResult.ViewResult(this, viewModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[PreventRapidRequests]
		public async Task<IActionResult> Create(InputModels.MessageInput input) {
			if (ModelState.IsValid) {
				if (Request.Method == "GET" && input.BoardId != null) {
					input.SelectedBoards.Add((int)input.BoardId);
				}
				else {
					foreach (var board in await BoardRepository.Records()) {
						if (Request.Form.TryGetValue("Selected_" + board.Id, out var boardSelected)) {
							if (boardSelected == "True") {
								input.SelectedBoards.Add(board.Id);
							}
						}
					}
				}

				var serviceResponse = await MessageRepository.CreateTopic(input);
				return await ForumViewResult.RedirectFromService(this, serviceResponse, FailureCallback);
			}

			return await FailureCallback();

			async Task<IActionResult> FailureCallback() {
				var viewModel = new ViewModels.Messages.CreateTopicPage() {
					Id = "0",
					BoardId = input.BoardId.ToString(),
					Body = input.Body
				};

				return await ForumViewResult.ViewResult(this, viewModel);
			}
		}

		[HttpGet]
		public async Task<IActionResult> Edit(int id) {
			ViewData["Smileys"] = await SmileyRepository.GetSelectorList();

			var record = await DbContext.Messages.SingleOrDefaultAsync(m => m.Id == id);

			if (record is null) {
				throw new HttpNotFoundError();
			}

			var viewModel = new ViewModels.Messages.EditMessagePage {
				Id = id.ToString(),
				Body = record.OriginalBody
			};

			return await ForumViewResult.ViewResult(this, viewModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[PreventRapidRequests]
		public async Task<IActionResult> Edit(InputModels.MessageInput input) {
			if (ModelState.IsValid) {
				var serviceResponse = await MessageRepository.EditMessage(input);
				return await ForumViewResult.RedirectFromService(this, serviceResponse, FailureCallback);
			}

			return await FailureCallback();

			async Task<IActionResult> FailureCallback() {
				var viewModel = new ViewModels.Messages.CreateTopicPage {
					Id = "0",
					Body = input.Body
				};

				return await ForumViewResult.ViewResult(this, viewModel);
			}
		}

		[HttpGet]
		public async Task<IActionResult> Delete(int id) {
			if (ModelState.IsValid) {
				var serviceResponse = await MessageRepository.DeleteMessage(id);
				return await ForumViewResult.RedirectFromService(this, serviceResponse);
			}

			return ForumViewResult.RedirectToReferrer(this);
		}

		[HttpGet]
		public async Task<IActionResult> AddThought(int id, int smiley) {
			if (ModelState.IsValid) {
				var serviceResponse = await MessageRepository.AddThought(id, smiley);
				return await ForumViewResult.RedirectFromService(this, serviceResponse);
			}

			return ForumViewResult.RedirectToReferrer(this);
		}

		[Authorize(Roles = Constants.InternalKeys.Admin)]
		[HttpGet]
		public async Task<IActionResult> Admin(InputModels.Continue input = null) => await ForumViewResult.ViewResult(this);

		[Authorize(Roles = Constants.InternalKeys.Admin)]
		[HttpGet]
		public async Task<IActionResult> ProcessMessages(InputModels.Continue input) {
			if (string.IsNullOrEmpty(input.Stage)) {
				var totalSteps = MessageRepository.ProcessMessages();

				input = new InputModels.Continue {
					Stage = nameof(MessageRepository.ProcessMessages),
					CurrentStep = -1,
					TotalSteps = totalSteps
				};
			}
			else {
				await MessageRepository.ProcessMessagesContinue(input);
			}

			var viewModel = new ViewModels.Delay {
				ActionName = "Processing Messages",
				ActionNote = "Processing message text, loading external sites, replacing smiley codes.",
				CurrentPage = input.CurrentStep,
				TotalPages = input.TotalSteps,
				NextAction = UrlHelper.Action(nameof(Messages.Admin), nameof(Messages))
			};

			if (input.CurrentStep < input.TotalSteps) {
				input.CurrentStep++;
				viewModel.NextAction = UrlHelper.Action(nameof(Messages.ProcessMessages), nameof(Messages), input);
			}

			return await ForumViewResult.ViewResult(this, "Delay", viewModel);
		}

		[Authorize(Roles = Constants.InternalKeys.Admin)]
		[HttpGet]
		public async Task<IActionResult> ReprocessMessages(InputModels.Continue input) {
			if (string.IsNullOrEmpty(input.Stage)) {
				var totalSteps = MessageRepository.ReprocessMessages();

				input = new InputModels.Continue {
					Stage = nameof(MessageRepository.ReprocessMessages),
					CurrentStep = -1,
					TotalSteps = totalSteps
				};
			}
			else {
				await MessageRepository.ReprocessMessagesContinue(input);
			}

			var viewModel = new ViewModels.Delay {
				ActionName = "Reprocessing Messages",
				ActionNote = "Processing message text, loading external sites, replacing smiley codes.",
				CurrentPage = input.CurrentStep,
				TotalPages = input.TotalSteps,
				NextAction = UrlHelper.Action(nameof(Messages.Admin), nameof(Messages))
			};

			if (input.CurrentStep < input.TotalSteps) {
				input.CurrentStep++;
				viewModel.NextAction = UrlHelper.Action(nameof(Messages.ReprocessMessages), nameof(Messages), input);
			}

			return await ForumViewResult.ViewResult(this, "Delay", viewModel);
		}

		[Authorize(Roles = Constants.InternalKeys.Admin)]
		[HttpGet]
		public async Task<IActionResult> RecountReplies(InputModels.Continue input) {
			if (string.IsNullOrEmpty(input.Stage)) {
				var totalSteps = MessageRepository.RecountReplies();

				input = new InputModels.Continue {
					Stage = nameof(MessageRepository.RecountReplies),
					CurrentStep = -1,
					TotalSteps = totalSteps
				};
			}
			else {
				MessageRepository.RecountRepliesContinue(input);
			}

			var viewModel = new ViewModels.Delay {
				ActionName = "Recounting Replies",
				CurrentPage = input.CurrentStep,
				TotalPages = input.TotalSteps,
				NextAction = UrlHelper.Action(nameof(Messages.Admin), nameof(Messages))
			};

			if (input.CurrentStep < input.TotalSteps) {
				input.CurrentStep++;
				viewModel.NextAction = UrlHelper.Action(nameof(Messages.RecountReplies), nameof(Messages), input);
			}

			return await ForumViewResult.ViewResult(this, "Delay", viewModel);
		}

		[Authorize(Roles = Constants.InternalKeys.Admin)]
		[HttpGet]
		public async Task<IActionResult> RebuildParticipants(InputModels.Continue input) {
			if (string.IsNullOrEmpty(input.Stage)) {
				var totalSteps = MessageRepository.RebuildParticipants();

				input = new InputModels.Continue {
					Stage = nameof(MessageRepository.RebuildParticipants),
					CurrentStep = -1,
					TotalSteps = totalSteps
				};
			}
			else {
				MessageRepository.RebuildParticipantsContinue(input);
			}

			var viewModel = new ViewModels.Delay {
				ActionName = "Rebuilding participants",
				CurrentPage = input.CurrentStep,
				TotalPages = input.TotalSteps,
				NextAction = UrlHelper.Action(nameof(Messages.Admin), nameof(Messages))
			};

			if (input.CurrentStep < input.TotalSteps) {
				input.CurrentStep++;
				viewModel.NextAction = UrlHelper.Action(nameof(Messages.RebuildParticipants), nameof(Messages), input);
			}

			return await ForumViewResult.ViewResult(this, "Delay", viewModel);
		}
	}
}
