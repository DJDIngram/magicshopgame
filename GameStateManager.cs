using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is a singleton pattern Game State Manager.
// It is used as a global identifier/game flow controller for the state of play.

// Different Gamestates
public enum GameState {
    Playing,
    Paused,
    Quit,
}

public class GameStateManager : MonoBehaviour
{
    // Instance of the singleton (the one and only)
    public static GameStateManager instance;

    // The current gamestate.
    public GameState gameState;

    // An event that listeners can subscribe to, in order to react to the changing game state.
    public static event Action<GameState> OnGameStateChanged;

    public void UpdateGameState(GameState newState) {
        gameState = newState;

        switch (newState) {
            case GameState.Playing:
                break;
            case GameState.Paused:
                break;
            case GameState.Quit:
                // Going to fire off the event here, as when we quit afterwards, any subscribers wont be able to react to the event further down.
                // Just incase any subscribers need to do anything before quitting.
                OnGameStateChanged?.Invoke(newState);
                HandleQuitGame();
                break;
            default:
                throw new System.Exception(nameof(newState));
        }

        OnGameStateChanged?.Invoke(newState);

    }

    void Awake () {
        instance = this;
    }
    
    void Start() {
        UpdateGameState(GameState.Playing);
    }

    private void HandleQuitGame() {
        //  Quit the Application
        // Application.Quit();

        // As this doesn't work for the play mode / development environment, we use this instead:
        UnityEditor.EditorApplication.isPlaying = false;
    }
}
