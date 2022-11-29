using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

// For any instances where the game needs to interact with the UI
/* 
    Usually seperate parts of the UI are handled seperately. But this is the place where
    any key interactions are sent to and routed from. I.E pause key goes to here, gets sent to PauseMenuUi.

*/
public class MenuController : MonoBehaviour
{
    #region Script Variables

    private VisualElement root;
    private VisualElement m_constructionMenu; // Used for OnPauseButton to get rid of the construction menu if its open.
    private VisualElement ve_playerInfo;
    private VisualElement m_pauseMenu;

    // Offloading tabbed build UI to another controller.
    [SerializeField] private ConstructionMenuUI constructionMenuUI;

    // Offloading pause menu to another script.
    [SerializeField] private PauseMenuUI pauseMenuUI;

    #endregion

    void Awake() {
        GameStateManager.OnGameStateChanged += OnGameStateChanged;

        //Assign Elements
        root = GetComponent<UIDocument>().rootVisualElement;

        m_constructionMenu = root.Q("Construction");
        ve_playerInfo = root.Q("PlayerInfo");
        m_pauseMenu = root.Q("PauseMenu");
        // when the mouse is hovered over any button element in the Construction UI.
        m_constructionMenu.Query<Button>().ForEach((Button button) => {
            button.RegisterCallback<MouseEnterEvent>(e => HandleUIMouseEnter());
            button.RegisterCallback<MouseLeaveEvent>(e => HandleUIMouseLeave());
        });
        // when the mouse is hovered over the players hands or health
        ve_playerInfo.RegisterCallback<MouseEnterEvent>(e => HandleUIMouseEnter());
        ve_playerInfo.RegisterCallback<MouseEnterEvent>(e => HandleUIMouseLeave());

    }
    void OnDestroy() {
        GameStateManager.OnGameStateChanged -= OnGameStateChanged;
    }

    public void OnPauseButton(InputAction.CallbackContext context) {
        if (context.started && m_constructionMenu.style.display == DisplayStyle.Flex) {
            constructionMenuUI.ToggleConstruction();
        } else if (context.started) {
            pauseMenuUI.TogglePaused();
        } else { }
    }

    public void OnToggleConstruction(InputAction.CallbackContext context) {
        if (context.started && GameStateManager.instance.gameState == GameState.Playing) {
            constructionMenuUI.ToggleConstruction();
        }
    }

    private void ToggleElementVisibility(VisualElement element) {
        // Toggle element visibility between None and Flex
        element.style.display = element.style.display == DisplayStyle.Flex ? DisplayStyle.None : DisplayStyle.Flex;
    }

    private void HandleUIMouseEnter() {
        Debug.Log("HandleUIMouseEnter");
        constructionMenuUI.SetPlayerConstruction(true);
    }
    private void HandleUIMouseLeave() {
        Debug.Log("HandleUIMouseLeave");
        constructionMenuUI.SetPlayerConstruction(false);
    }

    private void OnGameStateChanged(GameState newState) {
        if (newState == GameState.Paused) {
            // If the game is paused
            m_pauseMenu.style.display = DisplayStyle.Flex; // Show Pause Menu
            ve_playerInfo.style.display = DisplayStyle.None; // Hide Player Info
        } else if (newState == GameState.Playing) {
            // If the game is in play
            m_pauseMenu.style.display = DisplayStyle.None; // Hide Pause Menu
            ve_playerInfo.style.display = DisplayStyle.Flex; //Show Player Info
        } else { }
    }
    
}
