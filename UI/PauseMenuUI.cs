using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PauseMenuUI : MonoBehaviour
{
    private VisualElement root;
    private Button b_resumeGame;
    private Button b_quitGame;
    private Button b_saveGame;
    private Button b_loadGame;

    void Awake() {
        //Assign Elements
        root = GetComponent<UIDocument>().rootVisualElement;

        // Assign the pause menu and pause/quit/save/load functions
        b_resumeGame = root.Q<Button>("button-resume-game");
        b_resumeGame?.RegisterCallback<ClickEvent>(e => TogglePaused());
        b_quitGame = root.Q<Button>("button-quit-game");
        b_quitGame?.RegisterCallback<ClickEvent>(e => QuitGame());
        b_saveGame = root.Q<Button>("button-save-game");
        b_saveGame?.RegisterCallback<ClickEvent>(e => SaveGame());
        b_loadGame = root.Q<Button>("button-load-game");
        b_loadGame?.RegisterCallback<ClickEvent>(e => LoadGame());
    }

    public void TogglePaused() {
        GameStateManager.instance.UpdateGameState(GameStateManager.instance.gameState == GameState.Paused ? GameState.Playing : GameState.Paused);
    }

    private void QuitGame() {
        GameStateManager.instance.UpdateGameState(GameState.Quit);
    }

    private void SaveGame() {
        Debug.Log("PauseMenuUI - Saving!");
        DataPersistenceManager.instance.SaveGame();
    }

    private void LoadGame() {
        Debug.Log("PauseMenuUI - Loading!");
        DataPersistenceManager.instance.LoadGame();
    }
}
