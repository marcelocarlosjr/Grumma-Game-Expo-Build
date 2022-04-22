using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSystemAnimated
{
    public LevelSystem levelSystem;
    public bool isAnimating;
    private float updateTimer;
    private float updateTimerMax;

    public int level;
    public int experience;

    public event EventHandler OnExperienceChanged;
    public event EventHandler OnLevelChanged;

    public LevelSystemAnimated(LevelSystem levelSystem)
    {
        SetLevelSystem(levelSystem);
        updateTimerMax = .016f;

    }
    public void SetLevelSystem(LevelSystem levelSystem)
    {
        this.levelSystem = levelSystem;

        level = levelSystem.GetLevelNumber();
        experience = levelSystem.GetExperience();

        levelSystem.OnExperienceChanged += LevelSystem_OnExperienceChanged;
        levelSystem.OnLevelChanged += LevelSystem_OnLevelChanged;
    }

    private void LevelSystem_OnLevelChanged(object sender, EventArgs e)
    {
        isAnimating = true;
    }

    private void LevelSystem_OnExperienceChanged(object sender, EventArgs e)
    {
        isAnimating = true;
    }

    private void Update()
    {
        if (isAnimating)
        {
            updateTimer += Time.deltaTime;
            while(updateTimer > updateTimerMax)
            {
                updateTimer -= updateTimerMax;
                UpdateTypeAddExperience();
            }
        }
    }

    public void UpdateTypeAddExperience()
    {
        if (level < levelSystem.GetLevelNumber())
        {
            AddExperience();
        }
        else
        {
            if (experience < levelSystem.GetExperience())
            {
                AddExperience();
            }
            else
            {
                isAnimating = false;
            }
        }
    }

    private void AddExperience()
    {
        experience++;
        if(experience >= levelSystem.GetExperienceToNextLevel(level))
        {
            level++;
            experience = 0;
            if (OnLevelChanged != null) OnLevelChanged(this, EventArgs.Empty);
        }
        if (OnExperienceChanged != null) OnExperienceChanged(this, EventArgs.Empty);
    }
}
