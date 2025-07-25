using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VitaFlow.Core.Interfaces.Services;
using VitaFlow.Core.Enums;
using VitaFlow.Web.ViewModels;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace VitaFlow.Web.Pages.Admin
{
    /// <summary>
    /// Page model for user management in admin panel.
    /// </summary>
    public class UsersModel : PageModel
    {
        private readonly IUserService _userService;

        public UserManagementViewModel UserManagement { get; set; } = new UserManagementViewModel();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public UserRole? RoleFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public UsersModel(IUserService userService)
        {
            _userService = userService;
        }

        public async Task OnGetAsync()
        {
            try
            {
                var pageSize = 20;
                var (users, totalCount) = await _userService.GetUsersWithPaginationAsync(
                    CurrentPage, pageSize, SearchTerm, RoleFilter);

                // Map to ViewModels
                var userViewModels = users.Select(u => new UserListItemViewModel
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                });

                UserManagement = new UserManagementViewModel
                {
                    Users = userViewModels,
                    CurrentPage = CurrentPage,
                    TotalUsers = totalCount,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    SearchTerm = SearchTerm,
                    RoleFilter = RoleFilter,
                    UserCountSummary = await _userService.GetUserCountSummaryAsync()
                };
            }
            catch (Exception)
            {
                ErrorMessage = "Có lỗi xảy ra khi tải danh sách người dùng.";
                UserManagement = new UserManagementViewModel();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var success = await _userService.DeleteUserAsync(id);
                if (success)
                {
                    SuccessMessage = "Xóa người dùng thành công.";
                }
                else
                {
                    ErrorMessage = "Không tìm thấy người dùng để xóa.";
                }
            }
            catch (Exception)
            {
                ErrorMessage = "Có lỗi xảy ra khi xóa người dùng.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostChangeRoleAsync(Guid userId, UserRole newRole)
        {
            try
            {
                var success = await _userService.ChangeUserRoleAsync(userId, newRole);
                if (success)
                {
                    SuccessMessage = $"Thay đổi vai trò người dùng thành công thành {GetRoleDisplayName(newRole)}.";
                }
                else
                {
                    ErrorMessage = "Không tìm thấy người dùng để thay đổi vai trò.";
                }
            }
            catch (Exception)
            {
                ErrorMessage = "Có lỗi xảy ra khi thay đổi vai trò người dùng.";
            }

            return RedirectToPage();
        }

        private string GetRoleDisplayName(UserRole role)
        {
            return role switch
            {
                UserRole.Admin => "Quản trị viên",
                UserRole.Staff => "Nhân viên",
                UserRole.Donor => "Người hiến máu",
                UserRole.Recipient => "Người nhận máu",
                _ => role.ToString()
            };
        }
    }
}
