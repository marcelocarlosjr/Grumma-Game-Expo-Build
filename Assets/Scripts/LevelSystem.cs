using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSystem
{
    public event EventHandler OnExperienceChanged;
    public event EventHandler OnLevelChanged;

    public static readonly int[] LevelEXP = { 100, 120, 160, 220, 300, 400, 520, 660, 820, 1000, 1200, 1420, 1660, 1920, 2200, 2500, 2820, 3160, 3520, 3900, 4300, 4720, 5160, 5620, 6100, 6600, 7120, 7660, 8220, 8800, 9400, 10020, 10660, 11320, 12000, 12700, 13420, 14160, 14920, 15700, 16500, 17320, 18160, 19020, 19900, 20800, 21720, 22660, 23620, 24600, 25600 };


    public int level;
    public int experience;

    public LevelSystem()
    {
        level = 0;
        experience = 0;
    }
    public void AddExperience(int _amount)
    {
        Debug.Log("ExperienceAdded " + _amount);
        if (!IsMaxLevel())
        {
            experience += _amount;
            while (!IsMaxLevel() && experience >= GetExperienceToNextLevel(level))
            {
                experience -= GetExperienceToNextLevel(level);
                level++;
                if (OnLevelChanged != null) OnLevelChanged(this, EventArgs.Empty);
            }
            if (OnExperienceChanged != null) OnExperienceChanged(this, EventArgs.Empty);
        }
    }

    public int GetLevelNumber()
    {
        return level;
    }

    public int GetExperience()
    {
        return experience;
    }

    public int GetExperienceToNextLevel(int level)
    {
        return LevelEXP[level];
    }

    public bool IsMaxLevel()
    {
        return IsMaxLevel(level);
    }

    public bool IsMaxLevel(int level)
    {
        return level == 50;
    }
}
