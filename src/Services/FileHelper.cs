using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rhino;

namespace RhinoCncSuite.Services
{
    /// <summary>
    /// Shared utility class for file operations to reduce boilerplate code
    /// and centralize error handling across services.
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// Asynchronously reads and deserializes JSON from a file.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="filePath">Path to the JSON file</param>
        /// <param name="defaultValue">Default value to return if file doesn't exist or deserialization fails</param>
        /// <returns>Deserialized object or default value</returns>
        public static async Task<T> ReadJsonAsync<T>(string filePath, T defaultValue = default(T))
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    RhinoApp.WriteLine($"RhinoCNC: File not found: {filePath}, returning default value.");
                    return defaultValue;
                }

                using var fileStream = File.OpenRead(filePath);
                using var reader = new StreamReader(fileStream);
                var json = await reader.ReadToEndAsync();
                
                if (string.IsNullOrWhiteSpace(json))
                {
                    RhinoApp.WriteLine($"RhinoCNC: Empty file: {filePath}, returning default value.");
                    return defaultValue;
                }

                var result = JsonConvert.DeserializeObject<T>(json);
                RhinoApp.WriteLine($"RhinoCNC: Successfully loaded {typeof(T).Name} from {Path.GetFileName(filePath)}");
                return result ?? defaultValue;
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error reading JSON from {filePath}: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Asynchronously serializes and writes an object to a JSON file.
        /// </summary>
        /// <typeparam name="T">The type to serialize</typeparam>
        /// <param name="filePath">Path to save the JSON file</param>
        /// <param name="data">Object to serialize</param>
        /// <param name="createDirectory">Whether to create the directory if it doesn't exist</param>
        /// <returns>True if successful, false otherwise</returns>
        public static async Task<bool> WriteJsonAsync<T>(string filePath, T data, bool createDirectory = true)
        {
            try
            {
                if (createDirectory)
                {
                    var directory = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                }

                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                
                using var fileStream = File.Create(filePath);
                using var writer = new StreamWriter(fileStream);
                await writer.WriteAsync(json);
                
                RhinoApp.WriteLine($"RhinoCNC: Successfully saved {typeof(T).Name} to {Path.GetFileName(filePath)}");
                return true;
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error writing JSON to {filePath}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Synchronously reads and deserializes JSON from a file.
        /// Use only when async is not possible.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="filePath">Path to the JSON file</param>
        /// <param name="defaultValue">Default value to return if file doesn't exist or deserialization fails</param>
        /// <returns>Deserialized object or default value</returns>
        public static T ReadJson<T>(string filePath, T defaultValue = default(T))
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return defaultValue;
                }

                var json = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return defaultValue;
                }

                return JsonConvert.DeserializeObject<T>(json) ?? defaultValue;
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error reading JSON from {filePath}: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Synchronously serializes and writes an object to a JSON file.
        /// Use only when async is not possible.
        /// </summary>
        /// <typeparam name="T">The type to serialize</typeparam>
        /// <param name="filePath">Path to save the JSON file</param>
        /// <param name="data">Object to serialize</param>
        /// <param name="createDirectory">Whether to create the directory if it doesn't exist</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool WriteJson<T>(string filePath, T data, bool createDirectory = true)
        {
            try
            {
                if (createDirectory)
                {
                    var directory = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                }

                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(filePath, json);
                return true;
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"RhinoCNC: Error writing JSON to {filePath}: {ex.Message}");
                return false;
            }
        }
    }
} 