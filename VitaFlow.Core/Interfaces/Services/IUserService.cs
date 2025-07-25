using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VitaFlow.Core.Entities;
using VitaFlow.Core.Enums;

namespace VitaFlow.Core.Interfaces.Services
{
    /// <summary>
    /// Service interface for managing users.
    /// </summary>
    public interface IUserService
    {
        Task<User> GetUserByIdAsync(Guid id);
        Task<User> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role);
        Task<User> CreateUserAsync(User user);
        Task<User> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(Guid id);
        Task<bool> ChangeUserRoleAsync(Guid userId, UserRole newRole);
        Task<bool> DeactivateUserAsync(Guid userId);
        Task<bool> ActivateUserAsync(Guid userId);
        Task<(IEnumerable<User> users, int totalCount)> GetUsersWithPaginationAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string searchTerm = null,
            UserRole? roleFilter = null,
            bool? isActiveFilter = null);
        Task<int> GetUserCountByRoleAsync(UserRole role);
        Task<Dictionary<UserRole, int>> GetUserCountSummaryAsync();
    }
}
