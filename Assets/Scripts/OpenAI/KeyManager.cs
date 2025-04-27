using System.IO;
using UnityEngine;

public static class KeyManager
{
    private static string _apiKey;
    private static bool _isLoaded = false;

    public static string GetApiKey()
    {
        if (!_isLoaded)
        {
            string apiKeyPath = Path.Combine(Application.streamingAssetsPath, "openai_key.txt");
            if (File.Exists(apiKeyPath))
            {
                _apiKey = File.ReadAllText(apiKeyPath).Trim();
                _isLoaded = true;
            }
            else
            {
                Debug.LogError("API Key file not found at: " + apiKeyPath);
            }
        }
        return _apiKey;
    }
}
