using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ConstructionMenuUI : MonoBehaviour
{
    // Attach PlayerConstruction to UI to disable building when hovered over UI.
    [SerializeField] PlayerConstruction playerConstruction;
    //Define member variables
    private const string tabClassName = "construction-category-box";
    private const string currentlySelectedTabClassName = "construction-category-box-selected";
    private const string unselectedContentClassName = "construction-content-unselected";
    // tab and tab content have the same prefix but different suffix.
    // define the suffix of the tab name
    private const string tabNameSuffix = "-buildcategory";
    // define the suffix of the tab content name
    private const string contentNameSuffix = "-buildcontent";

    private VisualElement root;
    private VisualElement m_constructionMenu; // The #Construction VE on the UXML doc.

    void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        // Assign the construction menu VisElement
        m_constructionMenu = root.Q("Construction"); 
        // Set up the construction tab callbacks.
        RegisterTabCallbacks();
        Debug.Log("TabbedMenuController Initialized");
    }

    // CONSTRUCTION BUILD MENU - Defines tab generation, navigation and onclick events
    //  for the construction menu.
    #region ConstructionBuildMenu

    public void RegisterTabCallbacks()
    {
        UQueryBuilder<Button> tabs = GetAllTabs();
        tabs.ForEach((Button tab) => {
            Debug.Log("Got Tab: " + tab.name);
            tab.RegisterCallback<ClickEvent>(TabOnClick);
            GenerateTabContent(tab);
        });
    }

    private void GenerateTabContent(Button tab) {
        // Could probably make this a little more concise.
        string tabType = tab.name.Replace(tabNameSuffix, "");
        if (tabType == "floors") {
            VisualElement tabContent = root.Q(tabType+"-buildcontent");
            foreach (ConstructionTile tile in playerConstruction.constructionTiles)
            {
                if (tile.tileType == ConstructionTile.TileType.Floor) {
                    Button b_Tile = new Button();
                    b_Tile.text = tile.tileName;
                    b_Tile.AddToClassList("construction-box");
                    b_Tile.RegisterCallback<ClickEvent, ConstructionTile>(SetConstructionTile, tile);
                    tabContent.Add(b_Tile);
                }
            }
        } else if (tabType == "walls") {
            VisualElement tabContent = root.Q(tabType+"-buildcontent");
            foreach (ConstructionTile tile in playerConstruction.constructionTiles)
            {
                if (tile.tileType == ConstructionTile.TileType.Wall) {
                    Button b_Tile = new Button();
                    b_Tile.text = tile.tileName;
                    b_Tile.AddToClassList("construction-box");
                    b_Tile.RegisterCallback<ClickEvent, ConstructionTile>(SetConstructionTile, tile);
                    tabContent.Add(b_Tile);
                }
            }
        } else if (tabType == "furniture") {
            VisualElement tabContent = root.Q(tabType+"-buildcontent");
            foreach (FurnitureObject furniture in playerConstruction.furnitureObjects)
            {
                Button b_Furniture = new Button();
                b_Furniture.text = furniture.furnitureName;
                b_Furniture.AddToClassList("construction-box");
                b_Furniture.RegisterCallback<ClickEvent, FurnitureObject>(SetFurnitureObject, furniture);
                tabContent.Add(b_Furniture);
            }
        }
    }

    /* Method for the tab on-click event: 

       - If it is not selected, find other tabs that are selected, unselect them 
       - Then select the tab that was clicked on
    */
    private void TabOnClick(ClickEvent evt)
    {
        Button clickedTab = evt.currentTarget as Button;
        if (!TabIsCurrentlySelected(clickedTab))
        {
            GetAllTabs().Where(
                (tab) => tab != clickedTab && TabIsCurrentlySelected(tab)
            ).ForEach(UnselectTab);
            SelectTab(clickedTab);
        }
    }
    //Method that returns a Boolean indicating whether a tab is currently selected
    private static bool TabIsCurrentlySelected(Button tab)
    {
        return tab.ClassListContains(currentlySelectedTabClassName);
    }

    private UQueryBuilder<Button> GetAllTabs()
    {
        return root.Query<Button>(className: tabClassName);
    }

    /* Method for the selected tab:
       -  Takes a tab as a parameter and adds the currentlySelectedTab class
       -  Then finds the tab content and removes the unselectedContent class */
    private void SelectTab(Button tab)
    {
        tab.AddToClassList(currentlySelectedTabClassName);
        VisualElement content = FindContent(tab);
        content.RemoveFromClassList(unselectedContentClassName);

        switch (tab.name)
        {
            case "floors-buildcategory":
            case "walls-buildcategory":
                playerConstruction.SetBuildMode(true);
                break;
            case "furniture-buildcategory":
                playerConstruction.SetPlaceMode(true);
                break;
            default:
                Debug.LogError("Tab Not Found? Please Check ConstructionUI");
                break;
        }
    }

    /* Method for the unselected tab: 
       -  Takes a tab as a parameter and removes the currentlySelectedTab class
       -  Then finds the tab content and adds the unselectedContent class */
    private void UnselectTab(Button tab)
    {
        tab.RemoveFromClassList(currentlySelectedTabClassName);
        VisualElement content = FindContent(tab);
        content.AddToClassList(unselectedContentClassName);
    }

    // Method to generate the associated tab content name by for the given tab name
    private static string GenerateContentName(Button tab) =>
        tab.name.Replace(tabNameSuffix, contentNameSuffix);

    // Method that takes a tab as a parameter and returns the associated content element
    private VisualElement FindContent(Button tab)
    {
        return root.Q(GenerateContentName(tab));
    }

    #endregion

    private void SetConstructionTile(ClickEvent evt, ConstructionTile tile) {
        playerConstruction.SetConstructionTile(tile);
    }

    private void SetFurnitureObject(ClickEvent evt, FurnitureObject furnitureObject) {
        playerConstruction.SetFurnitureObject(furnitureObject);
    }

    public void ToggleConstruction() {
        if (m_constructionMenu.style.display == DisplayStyle.Flex) {
            // If we're displaying the construction menu, we're in build/place mode.
            //      Toggle it OFF.
            m_constructionMenu.style.display = DisplayStyle.None;
            playerConstruction.SetBuildMode(false);
            playerConstruction.SetPlaceMode(false);
        } else {
            //      Toggle it ON.
            m_constructionMenu.style.display = DisplayStyle.Flex;
            playerConstruction.SetBuildMode(true);
        }
    }

    public void SetPlayerConstruction(bool isEnabled) {
        playerConstruction.isEnabled = isEnabled;
    }

}