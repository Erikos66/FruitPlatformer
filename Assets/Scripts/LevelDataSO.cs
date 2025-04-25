using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
public class LevelDataSO : ScriptableObject
{
    [System.Serializable]
    public class LevelInfo
    {
        public string levelName;
        public int totalFruits;
    }

    [SerializeField] private List<LevelInfo> levelInfoList = new List<LevelInfo>();

    // Dictionary for faster lookups (populated at runtime)
    private Dictionary<string, int> fruitCountsDict = new Dictionary<string, int>();

    // Get the number of fruits in a specific level
    public int GetTotalFruitsInLevel(string levelName)
    {
        // Initialize dictionary if needed
        if (fruitCountsDict.Count == 0 && levelInfoList.Count > 0)
        {
            foreach (var levelInfo in levelInfoList)
            {
                fruitCountsDict[levelInfo.levelName] = levelInfo.totalFruits;
            }
        }

        if (fruitCountsDict.TryGetValue(levelName, out int fruitCount))
        {
            return fruitCount;
        }

        return 0;
    }
}