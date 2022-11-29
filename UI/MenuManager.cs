using UnityEngine;
using UnityEngine.UIElements;

// For any instances where the UI needs to interact with itself.
public class MenuManager : VisualElement
{
    public new class UxmlFactory: UxmlFactory<MenuManager, UxmlTraits> { }
    public new class UxmlTraits: VisualElement.UxmlTraits{ }

    // The below is called when we start the game
    public MenuManager(){
        // Debug.Log("Hello from MenuManager()");
        this.RegisterCallback<GeometryChangedEvent>(OnGeometryChange);
    }

    private void OnGeometryChange(GeometryChangedEvent e) {
        // Any callback setups, element assignments ect.
    }

}
