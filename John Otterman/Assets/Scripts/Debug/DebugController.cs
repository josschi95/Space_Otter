using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugController : MonoBehaviour
{
    private static DebugController instance;

    private bool showConsole = false;
    bool showHelp;

    private string input;
    Vector2 scroll;

    public static DebugCommand HELP;
    public static DebugCommand KILL_ALL;
    public static DebugCommand TOGGLE_GOD_MODE;
    public static DebugCommand<int> GRANT_CLAMS;
    public static DebugCommand<int> UNLOCK_STAGE;
    public static DebugCommand UNLOCK_ALL;
    public static DebugCommand LOCK_ALL;
    public static DebugCommand RESET;

    public List<object> commandList;

    private void Awake()
    {
        instance = this;

        HELP = new DebugCommand("help", "Shows list of commands.", "help", () =>
        {
            showHelp = true;
        });

        KILL_ALL = new DebugCommand("kill_all", "Kills all enemies in scene.", "kill_all", () =>
        {
            if (DungeonManager.instance != null)
            {
                DungeonManager.instance.KillAll();
            }
        });

        TOGGLE_GOD_MODE = new DebugCommand("tgm", "Enables God Mode on player.", "tgm", () =>
        {
            PlayerController.instance.ToggleGodMode();
        });

        GRANT_CLAMS = new DebugCommand<int>("clams", "Grants the player a given amount of clams.", "clams <amount>", (x) =>
        {
            GameManager.OnClamsGained(x);
        });

        UNLOCK_STAGE = new DebugCommand<int>("unlock_stage", "Sets the given stage as complete.", "stage <num>", (x) =>
        {
            GameManager.instance.OnStageClear(x);
        });

        UNLOCK_ALL = new DebugCommand("unlock_all", "Unlocks all stages.", "unlock_all", () =>
        {
            GameManager.instance.UnlockAll();
        });

        LOCK_ALL = new DebugCommand("lock_all", "Locks all stages.", "lock_all", () =>
        {
            GameManager.instance.LockAll();
        });

        RESET = new DebugCommand("reset", "Resets all player stats.", "reset", () =>
        {
            PlayerController.instance.OnResetValues();
        });

        commandList = new List<object>
        {
            HELP,
            KILL_ALL,
            TOGGLE_GOD_MODE,
            GRANT_CLAMS,
            UNLOCK_STAGE,
            UNLOCK_ALL,
            LOCK_ALL,
            RESET
        };
    }

    public static void OnToggleDebug()
    {
        instance.OnToggleDebugMenu();
    }

    private void OnToggleDebugMenu()
    {
        showConsole = !showConsole;
        showHelp = false;

        if (showConsole)
        {
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
        }

    }

    public static void OnReturn()
    {
        instance.OnReturnInput();
    }

    private void OnReturnInput()
    {
        if (showConsole)
        {
            HandleInput();
            input = "";
        }
    }

    private void OnGUI()
    {
        if (!showConsole) return;

        float y = 0f;

        if (showHelp)
        {
            GUI.Box(new Rect(0, y, Screen.width, 100), "");

            Rect viewport = new Rect(0, 0, Screen.width - 30, 20 * commandList.Count);

            scroll = GUI.BeginScrollView(new Rect(0, y + 5f, Screen.width, 90), scroll, viewport);

            for (int i = 0; i < commandList.Count; i++)
            {
                DebugCommandBase command = commandList[i] as DebugCommandBase;

                string label = $"{command.CommandFormat} - {command.CommandDescription}";

                Rect labelRect = new Rect(5, 20 * i, viewport.width - 100, 20);

                GUI.Label(labelRect, label);
            }

            GUI.EndScrollView();
            
            y += 100;
        }

        GUI.Box(new Rect(0, y, Screen.width, 30), "");
        GUI.backgroundColor = new Color(0, 0, 0, 0);
        input = GUI.TextField(new Rect(10f, y + 5f, Screen.width - 20f, 20f), input);


    }

    private void HandleInput()
    {
        string[] properties = input.Split(' ');

        for (int i = 0; i < commandList.Count; i++)
        {
            DebugCommandBase commandBase = commandList[i] as DebugCommandBase;

            if (input.Contains(commandBase.CommandID))
            {
                if (commandList[i] as DebugCommand != null)
                {
                    (commandList[i] as DebugCommand).Invoke();
                }
                else if (commandList[i] as DebugCommand<int> != null)
                {
                    (commandList[i] as DebugCommand<int>).Invoke(int.Parse(properties[1]));
                }
            }

        }
    }
}
