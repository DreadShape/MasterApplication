using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace MasterApplication;

/// <summary>
/// All the necessary configuration for the application.
/// </summary>
public class Configuration
{
    #region StaticProperties

    private static readonly string _appDirectory = AppDomain.CurrentDomain.BaseDirectory;
    private static readonly string _appName = Process.GetCurrentProcess().ProcessName;
    private static readonly string _defaultConfigurationFilePath = Path.Combine(_appDirectory, $"{_appName}.json");

    private static readonly object _lock = new();
    private static Configuration? _instance;

    #endregion

    #region PublicProperties

    public IList<string> MouseClicker_AutoClickerSequence { get; set; } = new List<string>();

    #endregion

    #region PrivateConstructor

    private Configuration() { }

    #endregion

    #region PublicMethods

    /// <summary>
    /// Gets the singleton instance of <see cref="Configuration"/>.
    /// </summary>
    /// <param name="configurationPath">Optional path to the configuration file. If not provided, the default path is used.</param>
    /// <returns>The current <see cref="Configuration"/> instance.</returns>
    public static Configuration GetInstance(string? configurationPath = null)
    {
        lock (_lock)
        {
            if (_instance == null)
                _instance = LoadConfiguration(configurationPath);

            return _instance;
        }
    }

    /// <summary>
    /// Saves the singleton <see cref="Configuration"/> instance to the file.
    /// </summary>
    public static void Save()
    {
        lock (_lock)
        {
            if (_instance == null)
                return;

            SaveConfiguration(_instance);
        }
    }

    #endregion

    #region PrivateMethods

    /// <summary>
    /// Loads the <see cref="Configuration"/> from a file.
    /// </summary>
    /// <param name="configurationPath">Optional path to the file. If not provided, the default path is used.</param>
    /// <returns>The loaded <see cref="Configuration"/> instance or a new default one if deserialization fails.</returns>
    private static Configuration LoadConfiguration(string? configurationPath = null)
    {
        string configPath = !string.IsNullOrEmpty(configurationPath) ? configurationPath : _defaultConfigurationFilePath;

        if (!File.Exists(configPath))
            return new Configuration();

        try
        {
            string jsonString = File.ReadAllText(configPath);
            var configuration = JsonSerializer.Deserialize<Configuration>(jsonString);
            return configuration ?? new Configuration();  // Return a default config if deserialization fails
        }
        catch (Exception)
        {
            return new Configuration();
        }
    }

    /// <summary>
    /// Saves the <see cref="Configuration"/> to a file.
    /// </summary>
    /// <param name="configuration">The configuration instance to save.</param>
    private static void SaveConfiguration(Configuration configuration)
    {
        JsonSerializerOptions options = new() { WriteIndented = true };
        string jsonString = JsonSerializer.Serialize(configuration, options);
        File.WriteAllText(_defaultConfigurationFilePath, jsonString);
    }

    #endregion
}
