﻿using UnityEngine;

public class LevelManager {

    public static void LoadNextLevel()
    {
        Application.LoadLevel((Application.loadedLevel + 1) % Application.levelCount);
    }

    public static void LoadPrevLevel()
    {
        Application.LoadLevel((Application.loadedLevel + Application.levelCount - 1) % Application.levelCount);
    }

    public static void LoadLevel(int i)
    {
        Application.LoadLevel(i);
    }

    // Must be called from each level's game manager - or we refactor to have a game manager that is never destroyed across levels.
    public static void InputHandler()
    {
        if (Input.GetKeyUp(KeyCode.Equals))
            TransitionManager.Inst.NextLevel();
        if (Input.GetKeyUp(KeyCode.Minus))
            TransitionManager.Inst.PreviousLevel();
        if (Input.GetKeyUp(KeyCode.Alpha1))
            TransitionManager.Inst.ChangeLevel(0);
        if (Input.GetKeyUp(KeyCode.Alpha2))
            TransitionManager.Inst.ChangeLevel(1);

    }
}