using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RetroRDP.Shared.Models;

namespace RetroRDPClient.Services
{
    /// <summary>
    /// Interface for managing session profiles
    /// </summary>
    public interface ISessionProfileService
    {
        /// <summary>
        /// Save a session profile
        /// </summary>
        Task<bool> SaveProfileAsync(SessionProfile profile, CancellationToken cancellationToken = default);

        /// <summary>
        /// Load a session profile by name
        /// </summary>
        Task<SessionProfile?> LoadProfileAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all available profiles
        /// </summary>
        Task<List<SessionProfile>> GetAllProfilesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete a profile
        /// </summary>
        Task<bool> DeleteProfileAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update profile usage statistics
        /// </summary>
        Task UpdateProfileUsageAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Search profiles by tags or description
        /// </summary>
        Task<List<SessionProfile>> SearchProfilesAsync(string query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get profiles sorted by usage frequency
        /// </summary>
        Task<List<SessionProfile>> GetFrequentProfilesAsync(int count = 5, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Service for managing RDP session profiles
    /// </summary>
    public class SessionProfileService : ISessionProfileService
    {
        private readonly ILogger<SessionProfileService>? _logger;
        private readonly string _profilesDirectory;
        private readonly Dictionary<string, SessionProfile> _profileCache = new();

        public SessionProfileService(ILogger<SessionProfileService>? logger = null)
        {
            _logger = logger;
            
            // Create profiles directory in user's application data
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _profilesDirectory = Path.Combine(appDataPath, "RetroRDP", "Profiles");
            
            // Ensure directory exists
            Directory.CreateDirectory(_profilesDirectory);
            
            _logger?.LogInformation("Session profile service initialized with directory: {ProfilesDirectory}", _profilesDirectory);
        }

        public async Task<bool> SaveProfileAsync(SessionProfile profile, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(profile.Name))
                {
                    _logger?.LogWarning("Cannot save profile with empty name");
                    return false;
                }

                var fileName = SanitizeFileName(profile.Name) + ".json";
                var filePath = Path.Combine(_profilesDirectory, fileName);

                var json = JsonSerializer.Serialize(profile, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await File.WriteAllTextAsync(filePath, json, cancellationToken);
                
                // Update cache
                _profileCache[profile.Name] = profile;
                
                _logger?.LogInformation("Profile '{ProfileName}' saved successfully", profile.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to save profile '{ProfileName}'", profile.Name);
                return false;
            }
        }

        public async Task<SessionProfile?> LoadProfileAsync(string name, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check cache first
                if (_profileCache.TryGetValue(name, out var cachedProfile))
                {
                    return cachedProfile;
                }

                var fileName = SanitizeFileName(name) + ".json";
                var filePath = Path.Combine(_profilesDirectory, fileName);

                if (!File.Exists(filePath))
                {
                    _logger?.LogWarning("Profile file not found: {FilePath}", filePath);
                    return null;
                }

                var json = await File.ReadAllTextAsync(filePath, cancellationToken);
                var profile = JsonSerializer.Deserialize<SessionProfile>(json);

                if (profile != null)
                {
                    _profileCache[name] = profile;
                    _logger?.LogInformation("Profile '{ProfileName}' loaded successfully", name);
                }

                return profile;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to load profile '{ProfileName}'", name);
                return null;
            }
        }

        public async Task<List<SessionProfile>> GetAllProfilesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var profiles = new List<SessionProfile>();
                var profileFiles = Directory.GetFiles(_profilesDirectory, "*.json");

                foreach (var filePath in profileFiles)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    try
                    {
                        var json = await File.ReadAllTextAsync(filePath, cancellationToken);
                        var profile = JsonSerializer.Deserialize<SessionProfile>(json);
                        
                        if (profile != null)
                        {
                            profiles.Add(profile);
                            _profileCache[profile.Name] = profile;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Failed to load profile from file: {FilePath}", filePath);
                    }
                }

                _logger?.LogInformation("Loaded {Count} profiles", profiles.Count);
                return profiles;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to get all profiles");
                return new List<SessionProfile>();
            }
        }

        public Task<bool> DeleteProfileAsync(string name, CancellationToken cancellationToken = default)
        {
            try
            {
                var fileName = SanitizeFileName(name) + ".json";
                var filePath = Path.Combine(_profilesDirectory, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                // Remove from cache
                _profileCache.Remove(name);

                _logger?.LogInformation("Profile '{ProfileName}' deleted successfully", name);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to delete profile '{ProfileName}'", name);
                return Task.FromResult(false);
            }
        }

        public async Task UpdateProfileUsageAsync(string name, CancellationToken cancellationToken = default)
        {
            try
            {
                var profile = await LoadProfileAsync(name, cancellationToken);
                if (profile != null)
                {
                    profile.UseCount++;
                    profile.LastUsed = DateTime.UtcNow;
                    await SaveProfileAsync(profile, cancellationToken);
                    
                    _logger?.LogDebug("Updated usage for profile '{ProfileName}' - Use count: {UseCount}", name, profile.UseCount);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to update usage for profile '{ProfileName}'", name);
            }
        }

        public async Task<List<SessionProfile>> SearchProfilesAsync(string query, CancellationToken cancellationToken = default)
        {
            try
            {
                var allProfiles = await GetAllProfilesAsync(cancellationToken);
                var lowerQuery = query.ToLowerInvariant();

                var matchingProfiles = allProfiles.Where(p =>
                    p.Name.ToLowerInvariant().Contains(lowerQuery) ||
                    p.Host.ToLowerInvariant().Contains(lowerQuery) ||
                    (p.Description?.ToLowerInvariant().Contains(lowerQuery) ?? false) ||
                    p.Tags.Any(tag => tag.ToLowerInvariant().Contains(lowerQuery))
                ).ToList();

                _logger?.LogInformation("Found {Count} profiles matching query '{Query}'", matchingProfiles.Count, query);
                return matchingProfiles;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to search profiles with query '{Query}'", query);
                return new List<SessionProfile>();
            }
        }

        public async Task<List<SessionProfile>> GetFrequentProfilesAsync(int count = 5, CancellationToken cancellationToken = default)
        {
            try
            {
                var allProfiles = await GetAllProfilesAsync(cancellationToken);
                
                var frequentProfiles = allProfiles
                    .Where(p => p.UseCount > 0)
                    .OrderByDescending(p => p.UseCount)
                    .ThenByDescending(p => p.LastUsed)
                    .Take(count)
                    .ToList();

                _logger?.LogInformation("Retrieved {Count} most frequent profiles", frequentProfiles.Count);
                return frequentProfiles;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to get frequent profiles");
                return new List<SessionProfile>();
            }
        }

        private static string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        }
    }
}