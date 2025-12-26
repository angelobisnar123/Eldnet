using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1_Test1.Data;
using WebApplication1_Test1.Models;

namespace WebApplication1_Test1.Controllers
{
    public class UserInfoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public UserInfoController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: UserInfo/GetCurrentUserInfo
        [HttpGet]
        public async Task<IActionResult> GetCurrentUserInfo()
        {
            try
            {
                // Get current logged-in user
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "User not authenticated"
                    });
                }

                // Try to get user info from UserInfo table
                var userInfo = await _context.UserInfos
                    .FirstOrDefaultAsync(u => u.UserId == currentUser.Id);

                if (userInfo != null)
                {
                    // User exists in UserInfo table
                    return Json(new
                    {
                        success = true,
                        userInfo = new
                        {
                            UserId = currentUser.Id,
                            FullName = $"{userInfo.FirstName} {userInfo.LastName}",
                            FirstName = userInfo.FirstName,
                            LastName = userInfo.LastName,
                            Email = currentUser.Email,
                            ContactNumber = userInfo.ContactNumber,
                            Semester = userInfo.Semester,
                            IsRegistered = true
                        }
                    });
                }
                else
                {
                    // User doesn't exist in UserInfo table, return basic info
                    return Json(new
                    {
                        success = true,
                        userInfo = new
                        {
                            UserId = currentUser.Id,
                            FullName = currentUser.UserName,
                            FirstName = "",
                            LastName = "",
                            Email = currentUser.Email,
                            ContactNumber = "",
                            Semester = "",
                            IsRegistered = false
                        }
                    });
                }
            }
            catch (System.Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error retrieving user info: {ex.Message}"
                });
            }
        }

        // POST: UserInfo/SaveOrUpdate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveOrUpdate([FromBody] UserInfoModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return Json(new { success = false, errors });
                }

                // Get current logged-in user
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                // Verify the UserId matches the current user
                if (model.UserId != currentUser.Id)
                {
                    return Json(new { success = false, message = "User ID mismatch" });
                }

                // Check if user info already exists
                var existingInfo = await _context.UserInfos
                    .FirstOrDefaultAsync(u => u.UserId == model.UserId);

                if (existingInfo != null)
                {
                    // Update existing record
                    existingInfo.FirstName = model.FirstName;
                    existingInfo.LastName = model.LastName;
                    existingInfo.ContactNumber = model.ContactNumber;
                    existingInfo.Semester = model.Semester;
                    existingInfo.Email = model.Email;

                    _context.UserInfos.Update(existingInfo);
                }
                else
                {
                    // Create new record
                    _context.UserInfos.Add(model);
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "User information saved successfully"
                });
            }
            catch (System.Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error saving user info: {ex.Message}"
                });
            }
        }

        // GET: UserInfo/GetById/{userId}
        [HttpGet]
        public async Task<IActionResult> GetById(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User ID is required" });
                }

                var userInfo = await _context.UserInfos
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (userInfo == null)
                {
                    return Json(new { success = false, message = "User information not found" });
                }

                return Json(new
                {
                    success = true,
                    userInfo = new
                    {
                        userInfo.UserId,
                        userInfo.FirstName,
                        userInfo.LastName,
                        userInfo.Email,
                        userInfo.ContactNumber,
                        userInfo.Semester,
                        FullName = $"{userInfo.FirstName} {userInfo.LastName}"
                    }
                });
            }
            catch (System.Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error retrieving user info: {ex.Message}"
                });
            }
        }
    }
}