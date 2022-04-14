using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSystem
{
    public event EventHandler OnExperienceChanged;
    public event EventHandler OnLevelChanged;

    private int level;
    private int experience;

    public LevelSystem()
    {
        level = 0;
        experience = 0;
    }
    public void AddExperience(int _amount)
    {
        if (!IsMaxLevel())
        {
            experience += _amount;
            while (!IsMaxLevel() && experience >= GetExperienceToNextLevel(level))
            {
                level++;
                experience -= GetExperienceToNextLevel(level);
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
        return (int)Mathf.Floor(100 * level * Mathf.Pow(level, 0.5f));
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
