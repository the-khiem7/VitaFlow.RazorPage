using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VitaFlow.Core.Entities;

using VitaFlow.Core.Enums;
using VitaFlow.Core.Interfaces.Services;
using VitaFlow.Infrastructure.Repositories.Interfaces;

namespace VitaFlow.Services.Services
{
    /// <summary>
    /// Implementation of the IUserService interface.
    /// Provides user management functionality with caching, logging, and error handling.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly ILogger<UserService> _logger;
        private const string UserCacheKey = "User_{0}";
        private const string AllUsersCacheKey = "AllUsers";
        private const string UsersByRoleCacheKey = "UsersByRole_{0}";

        public UserService(IUnitOfWork unitOfWork, IMemoryCache cache, ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<User> GetUserByIdAsync(Guid id)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var cacheKey = string.Format(UserCacheKey, id);
                if (_cache.TryGetValue(cacheKey, out User cachedUser))
                {
                    _logger.LogInformation($"User {id} retrieved from cache");
                    return cachedUser;
                }

                _logger.LogInformation($"Fetching user with id {id}");
                var repo = _unitOfWork.GetRepository<User>();
                var user = await repo.GetByIdAsync(id);

                if (user == null)
                {
                    _logger.LogWarning($"User with id {id} not found");
                    throw new KeyNotFoundException($"User with id {id} not found");
                }

                _cache.Set(cacheKey, user, TimeSpan.FromMinutes(30));
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user with id {id}");
                throw;
            }
            finally
            {
                sw.Stop();
                _logger.LogInformation($"GetUserByIdAsync executed in {sw.ElapsedMilliseconds} ms");
            }
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation($"Fetching user with email {email}");
                var repo = _unitOfWork.GetRepository<User>();
                var users = await repo.GetListAsync(predicate: u => u.Email == email);
                var user = users.FirstOrDefault();

                if (user == null)
                {
                    _logger.LogWarning($"User with email {email} not found");
                    return null;
                }

                var cacheKey = string.Format(UserCacheKey, user.Id);
                _cache.Set(cacheKey, user, TimeSpan.FromMinutes(30));
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user with email {email}");
                throw;
            }
            finally
            {
                sw.Stop();
                _logger.LogInformation($"GetUserByEmailAsync executed in {sw.ElapsedMilliseconds} ms");
            }
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                if (_cache.TryGetValue(AllUsersCacheKey, out IEnumerable<User> cachedUsers))
                {
                    _logger.LogInformation("All users retrieved from cache");
                    return cachedUsers;
                }

                _logger.LogInformation("Fetching all users");
                var repo = _unitOfWork.GetRepository<User>();
                var users = await repo.GetListAsync(predicate: null);

                _cache.Set(AllUsersCacheKey, users, TimeSpan.FromMinutes(15));
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                throw;
            }
            finally
            {
                sw.Stop();
                _logger.LogInformation($"GetAllUsersAsync executed in {sw.ElapsedMilliseconds} ms");
            }
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var cacheKey = string.Format(UsersByRoleCacheKey, role);
                if (_cache.TryGetValue(cacheKey, out IEnumerable<User> cachedUsers))
                {
                    _logger.LogInformation($"Users with role {role} retrieved from cache");
                    return cachedUsers;
                }

                _logger.LogInformation($"Fetching users with role {role}");
                var repo = _unitOfWork.GetRepository<User>();
                var users = await repo.GetListAsync(predicate: u => u.Role == role);

                _cache.Set(cacheKey, users, TimeSpan.FromMinutes(15));
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting users with role {role}");
                throw;
            }
            finally
            {
                sw.Stop();
                _logger.LogInformation($"GetUsersByRoleAsync executed in {sw.ElapsedMilliseconds} ms");
            }
        }

        public async Task<User> CreateUserAsync(User user)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation($"Creating user with email {user.Email}");

                // Check if user with email already exists
                var existingUser = await GetUserByEmailAsync(user.Email);
                if (existingUser != null)
                {
                    throw new InvalidOperationException($"User with email {user.Email} already exists");
                }

                user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;

                User createdUser = null;
                await _unitOfWork.ProcessInTransactionAsync(async () =>
                {
                    var repo = _unitOfWork.GetRepository<User>();
                    await repo.InsertAsync(user);
                    createdUser = user;
                });

                // Clear cache
                _cache.Remove(AllUsersCacheKey);
                _cache.Remove(string.Format(UsersByRoleCacheKey, user.Role));

                _logger.LogInformation($"User created with id {createdUser.Id}");
                return createdUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                throw;
            }
            finally
            {
                sw.Stop();
                _logger.LogInformation($"CreateUserAsync executed in {sw.ElapsedMilliseconds} ms");
            }
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation($"Updating user with id {user.Id}");

                var existingUser = await GetUserByIdAsync(user.Id);
                if (existingUser == null)
                {
                    throw new KeyNotFoundException($"User with id {user.Id} not found");
                }

                user.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.ProcessInTransactionAsync(async () =>
                {
                    var repo = _unitOfWork.GetRepository<User>();
                    await repo.UpdateAsync(user);
                });

                // Clear cache
                _cache.Remove(string.Format(UserCacheKey, user.Id));
                _cache.Remove(AllUsersCacheKey);
                _cache.Remove(string.Format(UsersByRoleCacheKey, user.Role));
                _cache.Remove(string.Format(UsersByRoleCacheKey, existingUser.Role));

                _logger.LogInformation($"User updated with id {user.Id}");
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user with id {user.Id}");
                throw;
            }
            finally
            {
                sw.Stop();
                _logger.LogInformation($"UpdateUserAsync executed in {sw.ElapsedMilliseconds} ms");
            }
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation($"Deleting user with id {id}");

                var user = await GetUserByIdAsync(id);
                if (user == null)
                {
                    return false;
                }

                await _unitOfWork.ProcessInTransactionAsync(async () =>
                {
                    var repo = _unitOfWork.GetRepository<User>();
                    repo.DeleteAsync(user);
                });

                // Clear cache
                _cache.Remove(string.Format(UserCacheKey, id));
                _cache.Remove(AllUsersCacheKey);
                _cache.Remove(string.Format(UsersByRoleCacheKey, user.Role));

                _logger.LogInformation($"User deleted with id {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting user with id {id}");
                throw;
            }
            finally
            {
                sw.Stop();
                _logger.LogInformation($"DeleteUserAsync executed in {sw.ElapsedMilliseconds} ms");
            }
        }

        public async Task<bool> ChangeUserRoleAsync(Guid userId, UserRole newRole)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation($"Changing role for user {userId} to {newRole}");

                var user = await GetUserByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                var oldRole = user.Role;
                user.Role = newRole;
                user.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.ProcessInTransactionAsync(async () =>
                {
                    var repo = _unitOfWork.GetRepository<User>();
                    await repo.UpdateAsync(user);
                });

                // Clear cache
                _cache.Remove(string.Format(UserCacheKey, userId));
                _cache.Remove(AllUsersCacheKey);
                _cache.Remove(string.Format(UsersByRoleCacheKey, oldRole));
                _cache.Remove(string.Format(UsersByRoleCacheKey, newRole));

                _logger.LogInformation($"Role changed for user {userId} from {oldRole} to {newRole}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error changing role for user {userId}");
                throw;
            }
            finally
            {
                sw.Stop();
                _logger.LogInformation($"ChangeUserRoleAsync executed in {sw.ElapsedMilliseconds} ms");
            }
        }

        public async Task<bool> DeactivateUserAsync(Guid userId)
        {
            // Note: This would require adding an IsActive property to User entity
            // For now, returning true as placeholder
            _logger.LogInformation($"Deactivating user {userId}");
            return true;
        }

        public async Task<bool> ActivateUserAsync(Guid userId)
        {
            // Note: This would require adding an IsActive property to User entity
            // For now, returning true as placeholder
            _logger.LogInformation($"Activating user {userId}");
            return true;
        }

        public async Task<(IEnumerable<User> users, int totalCount)> GetUsersWithPaginationAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string searchTerm = null,
            UserRole? roleFilter = null,
            bool? isActiveFilter = null)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation($"Getting users with pagination - Page: {pageNumber}, Size: {pageSize}");

                var repo = _unitOfWork.GetRepository<User>();
                var allUsers = await repo.GetListAsync(predicate: null);

                // Apply filters
                var filteredUsers = allUsers.AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    filteredUsers = filteredUsers.Where(u =>
                        u.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        u.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        u.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                }

                if (roleFilter.HasValue)
                {
                    filteredUsers = filteredUsers.Where(u => u.Role == roleFilter.Value);
                }

                var totalCount = filteredUsers.Count();
                var users = filteredUsers
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return (users, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users with pagination");
                throw;
            }
            finally
            {
                sw.Stop();
                _logger.LogInformation($"GetUsersWithPaginationAsync executed in {sw.ElapsedMilliseconds} ms");
            }
        }

        public async Task<int> GetUserCountByRoleAsync(UserRole role)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation($"Getting user count for role {role}");
                var users = await GetUsersByRoleAsync(role);
                return users.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user count for role {role}");
                throw;
            }
            finally
            {
                sw.Stop();
                _logger.LogInformation($"GetUserCountByRoleAsync executed in {sw.ElapsedMilliseconds} ms");
            }
        }

        public async Task<Dictionary<UserRole, int>> GetUserCountSummaryAsync()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("Getting user count summary by role");
                var allUsers = await GetAllUsersAsync();
                return allUsers.GroupBy(u => u.Role)
                             .ToDictionary(g => g.Key, g => g.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user count summary");
                throw;
            }
            finally
            {
                sw.Stop();
                _logger.LogInformation($"GetUserCountSummaryAsync executed in {sw.ElapsedMilliseconds} ms");
            }
        }
    }
}
