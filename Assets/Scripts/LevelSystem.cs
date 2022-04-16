using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSystem
{
    public event EventHandler OnExperienceChanged;
    public event EventHandler OnLevelChanged;

    public static readonly int[] LevelEXP = {100, 150, 200, 300};

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
