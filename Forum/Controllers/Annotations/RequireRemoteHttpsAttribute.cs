﻿using Forum.Extensions;
using Forum.Services.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Forum.Controllers.Annotations {
	/// <summary>
	/// Allows local requests to not use HTTPS (i.e. development)
	/// </summary>
	public class RequireRemoteHttpsAttribute : RequireHttpsAttribute {
		public override void OnAuthorization(AuthorizationFilterContext filterContext) {
			filterContext.ThrowIfNull(nameof(filterContext));

			if (filterContext.HttpContext.Request.IsLocal()) {
				return;
			}

			base.OnAuthorization(filterContext);
		}
	}
}