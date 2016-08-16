using System.Collections.Generic;
using UnityEngine;
using KSP.UI;
using KSP.UI.Screens;
using Icon = RUI.Icons.Selectable.Icon;


namespace KerBalloons
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    class KerBalloonsCatagory : MonoBehaviour
    {
        private static readonly List<AvailablePart> availableParts = new List<AvailablePart>();

        private void Awake()
        {

            availableParts.Clear();
            foreach (var part in PartLoader.LoadedPartsList)
            {
                if (!part.partPrefab) continue;
                if (part.manufacturer == "KerBalloons")
                {
                    availableParts.Add(part);
                }
            }

            GameEvents.onGUIEditorToolbarReady.Add(SubCategory);
          
        }

        
        private void SubCategory()
        { 
            Debug.Log("Adding KerBalloons Catagory");
            const string FILTER_CATEGORY = "Filter by Function";
            const string CUSTOM_CATEGORY_NAME = "KerBalloons";

            

            Texture2D iconTexNormal = GameDatabase.Instance.GetTexture("KerBalloons/Textures/KBIconNormal", false);
            Texture2D iconTexSelected = GameDatabase.Instance.GetTexture("KerBalloons/Textures/KBIconSelected", false);

            Icon icon = new Icon("KerBalloons", iconTexNormal, iconTexSelected, false);

            PartCategorizer.Category filter = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == FILTER_CATEGORY);
            PartCategorizer.AddCustomSubcategoryFilter(filter, CUSTOM_CATEGORY_NAME, icon, p => availableParts.Contains(p));

            UIRadioButton button = filter.button.activeButton;
        }
    }
}

