using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafetyMapData.Entities;
using SafetyMapWeb.Models.Users;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SafetyMapWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<UserIdentity> _userManager;

        public UsersController(UserManager<UserIdentity> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserListViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                userViewModels.Add(new UserListViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles
                });
            }

            return View(userViewModels);
        }
    }
}
