using UnityEngine;

public static class SaveManager
{
    public static void SaveUnlockedLevel(int unlockedLevel) {
        PlayerPrefs.SetInt("unlockedLevel", unlockedLevel);
    }

    public static int GetUnlockedLevel() {
        return PlayerPrefs.GetInt("unlockedLevel", 0);
    }

    public static void SaveLevelHighscore(int level, int highscore) {
        PlayerPrefs.SetInt("level" + level + "Highscore", highscore);
    }

    public static int GetLevelHighscore(int level) {
        return PlayerPrefs.GetInt("level" + level + "Highscore", 0);
    }
}
