﻿using Forum.Annotations;
using Forum.Contexts;
using Forum.Interfaces.Services;
using Forum.Models.InputModels;
using Forum.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Forum.Controllers {
	using ViewModels = Models.ViewModels.Smileys;

	[Authorize(Roles="Admin")]
	public class Smileys : Controller {
		ApplicationDbContext DbContext { get; }
		SmileyRepository SmileyRepository { get; }
		IForumViewResult ForumViewResult { get; }

		public Smileys(
			ApplicationDbContext dbContext,
			SmileyRepository smileyRepository,
			IForumViewResult forumViewResult
		) {
			DbContext = dbContext;
			SmileyRepository = smileyRepository;
			ForumViewResult = forumViewResult;
		}

		[HttpGet]
		public IActionResult Index() {
			var viewModel = new ViewModels.IndexPage();

			foreach (var smiley in SmileyRepository) {
				var sortColumn = smiley.SortOrder / 1000;
				var sortRow = smiley.SortOrder % 1000;

				viewModel.Smileys.Add(new ViewModels.IndexItem {
					Id = smiley.Id,
					Code = smiley.Code,
					Path = smiley.Path,
					Thought = smiley.Thought,
					Column = sortColumn,
					Row = sortRow
				});
			}

			return ForumViewResult.ViewResult(this, viewModel);
		}

		[HttpGet]
		public IActionResult Create() {
			var viewModel = new ViewModels.CreatePage();
			return ForumViewResult.ViewResult(this, viewModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[PreventRapidRequests]
		public async Task<IActionResult> Create(CreateSmileyInput input) {
			if (ModelState.IsValid) {
				var serviceResponse = await SmileyRepository.Create(input);
				return await ForumViewResult.RedirectFromService(this, serviceResponse, FailureCallback);
			}

			return await FailureCallback();

			async Task<IActionResult> FailureCallback() {
				var viewModel = new ViewModels.CreatePage {
					Code = input.Code,
					Thought = input.Thought
				};

				return await Task.Run(() => { return ForumViewResult.ViewResult(this, viewModel); });
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[PreventRapidRequests]
		public async Task<IActionResult> Edit(EditSmileysInput input) {
			if (ModelState.IsValid) {
				var serviceResponse = SmileyRepository.Update(input);
				return await ForumViewResult.RedirectFromService(this, serviceResponse, FailureCallback);
			}

			return await FailureCallback();

			async Task<IActionResult> FailureCallback() {
				return await Task.Run(() => { return RedirectToAction(nameof(Index)); });
			}
		}

		[HttpGet]
		public async Task<IActionResult> Delete(int id) {
			if (ModelState.IsValid) {
				var serviceResponse = await SmileyRepository.Delete(id);
				return await ForumViewResult.RedirectFromService(this, serviceResponse, FailureCallback);
			}

			return await FailureCallback();

			async Task<IActionResult> FailureCallback() {
				return await Task.Run(() => { return ForumViewResult.RedirectToReferrer(this); });
			}
		}
	}
}