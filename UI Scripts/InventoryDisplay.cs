using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryDisplay : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ItemTypeDropDown = ItemTypeDropDownGO.GetComponent<TMP_Dropdown>();
        WeaponsDropDown = WeaponsDropDownGO.GetComponent<TMP_Dropdown>();
        ArmorDropDown = ArmorDropDownGO.GetComponent<TMP_Dropdown>();
        MagicsDropDown = MagicsDropDownGO.GetComponent<TMP_Dropdown>();
        ProjectilesDropDown = ProjectilesDropDownGO.GetComponent<TMP_Dropdown>();
        ConsumablesDropDown = ConsumablesDropDownGO.GetComponent<TMP_Dropdown>();
    }

    public int List;

    public GameObject ItemTypeDropDownGO;
    public GameObject WeaponsDropDownGO;
    public GameObject ArmorDropDownGO;
    public GameObject MagicsDropDownGO;
    public GameObject ProjectilesDropDownGO;
    public GameObject ConsumablesDropDownGO;

    private TMP_Dropdown ItemTypeDropDown;
    private TMP_Dropdown WeaponsDropDown;
    private TMP_Dropdown ArmorDropDown;
    private TMP_Dropdown MagicsDropDown;
    private TMP_Dropdown ProjectilesDropDown;
    private TMP_Dropdown ConsumablesDropDown;

    public int currentItemType;
    public int currentType;

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Selected(int dropDown)
    {
        if(dropDown == 0)       //all items
        {
            WeaponsDropDownGO.SetActive(false);
            ArmorDropDownGO.SetActive(false);
            MagicsDropDownGO.SetActive(false);
            ProjectilesDropDownGO.SetActive(false);
            ConsumablesDropDownGO.SetActive(false);
            if(ItemTypeDropDown.value == 0)
            {
                Set();
            }
            else if(ItemTypeDropDown.value == 1)
            {
                WeaponsDropDownGO.SetActive(true);
                Set(ItemTypeDropDown.value);
            }
            else if(ItemTypeDropDown.value == 2)
            {
                ArmorDropDownGO.SetActive(true);
                Set(ItemTypeDropDown.value);
            }
            else if(ItemTypeDropDown.value == 3)
            {
                MagicsDropDownGO.SetActive(true);
                Set(ItemTypeDropDown.value);
            }
            else if(ItemTypeDropDown.value == 4)
            {
                ProjectilesDropDownGO.SetActive(true);
                Set(ItemTypeDropDown.value);
            }
            else if(ItemTypeDropDown.value == 5)       //wahrscheinlich consumables
            {
                ConsumablesDropDownGO.SetActive(true);
                Set(ItemTypeDropDown.value);
            }
        }
        else if(dropDown == 1)
        {
            Set(ItemTypeDropDown.value, WeaponsDropDown.value);
        }
        else if(dropDown == 2)
        {
            Set(ItemTypeDropDown.value, ArmorDropDown.value);
        }
        else if(dropDown == 3)
        {
            Set(ItemTypeDropDown.value, MagicsDropDown.value);
        }
        else if(dropDown == 4)
        {
            Set(ItemTypeDropDown.value, ProjectilesDropDown.value);
        }
        else if(dropDown == 5)
        {
            Set(ItemTypeDropDown.value, ConsumablesDropDown.value);
        }
    }

    private void Set(int itemType = 0, int type = 0)
    {
        currentItemType = itemType;
        currentType = type;
        if(List == 1)
        {
            PlayerController.playerContr.tempInventory = PlayerController.playerContr.gameObject.GetComponent<Inventory>().GetItems(List, itemType, type);
        }
        else if(List == 3)
        {
            PlayerController.playerContr.tempPickUp = PlayerController.playerContr.gameObject.GetComponent<Inventory>().GetItems(List, itemType, type);
        }
        PlayerController.playerContr.unloadInv();
        PlayerController.playerContr.loadInv();
    }

    public void Reset()
    {
        ItemTypeDropDown.value = 0;

        WeaponsDropDown.value = 0;
        ArmorDropDown.value = 0;
        MagicsDropDown.value = 0;
        ProjectilesDropDown.value = 0;
        ConsumablesDropDown.value = 0;

        currentItemType = 0;
        currentType = 0;
    }
}
