using MauiApp1.Data;
using MauiApp1.Models;
using System.Diagnostics;

namespace MauiApp1.Services
{
    public class SupabaseAuthService
    {
        private Supabase.Client? _client;
        private readonly UserRepository _userRepository;
        private bool _isInitialized = false;

        public SupabaseAuthService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> InitializeAsync()
        {
            if (_isInitialized) return true;

            // Check for placeholder values
            if (Constants.SupabaseUrl.Contains("your-project") ||
                Constants.SupabaseKey.Contains("your-anon-key"))
            {
                Debug.WriteLine("Supabase configuration is using placeholder values. Please update Constants.cs with your actual Supabase credentials.");
                return false;
            }

            try
            {
                var options = new Supabase.SupabaseOptions
                {
                    AutoConnectRealtime = false
                };

                _client = new Supabase.Client(Constants.SupabaseUrl, Constants.SupabaseKey, options);
                await _client.InitializeAsync();
                _isInitialized = true;
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Supabase initialization error: {ex.Message}");
                // Don't throw - allow app to continue in offline mode
                return false;
            }
        }

        public async Task<Models.User?> SignUpAsync(string email, string password, UserRole role = UserRole.User)
        {
            if (!await InitializeAsync()) return null;

            try
            {
                var session = await _client!.Auth.SignUp(email, password);

                if (session?.User != null)
                {
                    var user = new Models.User
                    {
                        Id = session.User.Id,
                        Email = email,
                        Role = role,
                        CreatedAt = DateTime.UtcNow,
                        LastLoginAt = DateTime.UtcNow
                    };

                    await _userRepository.SaveAsync(user);
                    return user;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SignUp error: {ex.Message}");
                return null;
            }

            return null;
        }

        public async Task<Models.User?> SignInAsync(string email, string password)
        {
            if (!await InitializeAsync()) return null;

            try
            {
                var session = await _client!.Auth.SignIn(email, password);

                if (session?.User != null)
                {
                    var existingUser = await _userRepository.GetAsync(session.User.Id);

                    if (existingUser == null)
                    {
                        // First time login - create local user record
                        existingUser = new Models.User
                        {
                            Id = session.User.Id,
                            Email = email,
                            Role = UserRole.User, // Default role
                            CreatedAt = DateTime.UtcNow,
                            LastLoginAt = DateTime.UtcNow
                        };
                    }
                    else
                    {
                        existingUser.LastLoginAt = DateTime.UtcNow;
                    }

                    await _userRepository.SaveAsync(existingUser);
                    return existingUser;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SignIn error: {ex.Message}");
                return null;
            }

            return null;
        }

        public async Task SignOutAsync()
        {
            if (!await InitializeAsync()) return;

            try
            {
                await _client!.Auth.SignOut();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SignOut error: {ex.Message}");
            }
        }

        public async Task<Models.User?> GetCurrentUserAsync()
        {
            if (!await InitializeAsync()) return null;

            var supabaseUser = _client!.Auth.CurrentUser;

            if (supabaseUser != null)
            {
                var localUser = await _userRepository.GetAsync(supabaseUser.Id);
                return localUser;
            }

            return null;
        }

        public bool IsAuthenticated()
        {
            return _client?.Auth.CurrentUser != null;
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            if (!await InitializeAsync()) return null;
            return _client?.Auth.CurrentSession?.AccessToken;
        }

        public async Task<bool> UpdateUserRoleAsync(string userId, UserRole newRole)
        {
            var user = await _userRepository.GetAsync(userId);
            if (user != null)
            {
                user.Role = newRole;
                await _userRepository.SaveAsync(user);
                return true;
            }
            return false;
        }
    }
}
