﻿using Forum3.Services.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Forum3.Controllers {
	using InputModels = Models.InputModels;
	using PageViewModels = Models.ViewModels.Roles.Pages;

	[Authorize(Roles="Admin")]
	public class Roles : ForumController {
		RoleService RoleService { get; }

		public Roles(
			RoleService roleService
		) {
			RoleService = roleService;
		}

		[HttpGet]
		public async Task<IActionResult> Index() {
			var viewModel = await RoleService.IndexPage();
			return View(viewModel);
		}

		[HttpGet]
		public IActionResult Create() {
			var viewModel = new PageViewModels.CreatePage();
			return View(viewModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(InputModels.CreateRoleInput input) {
			if (ModelState.IsValid) {
				var serviceResponse = await RoleService.Create(input);
				ProcessServiceResponse(serviceResponse);

				if (serviceResponse.Success)
					return RedirectFromService();
			}

			var viewModel = new PageViewModels.CreatePage() {
				Name = input.Name,
				Description = input.Description
			};

			return View(viewModel);
		}

		[HttpGet]
		public async Task<IActionResult> Edit(string id) {
			var viewModel = await RoleService.EditPage(id);
			return View(viewModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(InputModels.EditRoleInput input) {
			if (ModelState.IsValid) {
				var serviceResponse = await RoleService.Edit(input);
				ProcessServiceResponse(serviceResponse);

				if (serviceResponse.Success)
					return RedirectFromService();
			}

			var viewModel = await RoleService.EditPage(input.Id);

			viewModel.Name = input.Name;
			viewModel.Description = input.Description;

			return View(viewModel);
		}

		[HttpGet]
		public async Task<IActionResult> Delete(string id) {
			if (ModelState.IsValid)
				await RoleService.Delete(id);

			return RedirectFromService();
		}

		[HttpGet]
		public async Task<IActionResult> UserList(string id) {
			var viewModel = await RoleService.UserList(id);
			return View(viewModel);
		}

		[HttpGet]
		public async Task<IActionResult> AddUser(string id, string user) {
			var serviceResponse = await RoleService.AddUser(id, user);
			ProcessServiceResponse(serviceResponse);

			if (serviceResponse.Success)
				return RedirectFromService();

			var viewModel = await RoleService.EditPage(id);
			return View(nameof(Edit), viewModel);
		}

		[HttpGet]
		public async Task<IActionResult> RemoveUser(string id, string user) {
			var serviceResponse = await RoleService.RemoveUser(id, user);
			ProcessServiceResponse(serviceResponse);

			if (serviceResponse.Success)
				return RedirectFromService();

			var viewModel = await RoleService.EditPage(id);
			return View(nameof(Edit), viewModel);
		}
	}
}