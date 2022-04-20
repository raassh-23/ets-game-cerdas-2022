using UnityEngine;

public static class SaveManager
{
    public const int MaxLevel = 5;

    public static int CurrentLevel { get; set; }

    public static void SetUnlockedLevel(int unlockedLevel) {
        string key = "unlockedLevel";

        if (unlockedLevel > PlayerPrefs.GetInt(key))
        {
            PlayerPrefs.SetInt(key, unlockedLevel);
        }
    }

    public static int GetUnlockedLevel() {
        return PlayerPrefs.GetInt("unlockedLevel", 0);
    }

    public static void SetLevelHighscore(int level, float highscore) {
        string key = "level" + level + "Highscore";

        if (highscore > GetLevelHighscore(level)) {
            PlayerPrefs.SetFloat(key, highscore);
        }
    }

    public static float GetLevelHighscore(int level) {
        return PlayerPrefs.GetFloat("level" + level + "Highscore", 0f);
    }
}
