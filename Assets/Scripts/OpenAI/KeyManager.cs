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
            Debug.Log("API Key path: " + apiKeyPath); // Debugging line
            if (File.Exists(apiKeyPath))
            {
                _apiKey = File.ReadAllText(apiKeyPath).Trim();
                Debug.Log("Api key" + _apiKey);
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
