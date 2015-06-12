using UnityEngine;

public class LevelManager {

    public static void LoadNextLevel()
    {
        TransitionManager.Inst.NextLevel();
    }

    public static void LoadPrevLevel()
    {
        TransitionManager.Inst.PreviousLevel();
    }

    public static void LoadLevel(int i)
    {
        //TransitionManager.Inst.ChangeLevel(i);
        Application.LoadLevel(i);
    }

    // Must be called from each level's game manager - or we refactor to have a game manager that is never destroyed across levels.
    public static void InputHandler()
    {
        if (Input.GetKeyUp(KeyCode.Equals))
            LoadNextLevel();
        if (Input.GetKeyUp(KeyCode.Minus))
            LoadPrevLevel();

		/*
        if (Input.GetKeyUp(KeyCode.Alpha1))
            LoadLevel(0);
        if (Input.GetKeyUp(KeyCode.Alpha2))
            LoadLevel(1);
        if (Input.GetKeyUp(KeyCode.Alpha3))
            LoadLevel(2);
		*/
    }
}
