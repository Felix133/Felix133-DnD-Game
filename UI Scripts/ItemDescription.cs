using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class ItemDescription : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Classes[0] = "Kämpfer [";
        Classes[1] = "Paladin [";
        Classes[2] = "Waldläufer [";
        Classes[3] = "Zauberkundiger [";
        Classes[4] = "Spezialist [";
        Classes[5] = "Kleriker [";
        Classes[6] = "Druide [";
        Classes[7] = "Dieb [";
        Classes[8] = "Barde [";
        
        if(PlayerGO == null)
        {
            PlayerGO = GameObject.Find("/Player");
        }
        playerInv = PlayerGO.GetComponent<Inventory>();
    }
    public GameObject PlayerGO;
    private Inventory playerInv;
    public TMP_Dropdown ProjectileChange;
    public GameObject Item;

    public TextMeshProUGUI ItemNameTxt;
    public TextMeshProUGUI ItemDescriptTxt;
    public TextMeshProUGUI[] DisplayItemTexts = new TextMeshProUGUI[11];
    public string[] Classes = new string[9];
    public float[] ItemStats = new float[9];
    private List<int> projectiles;


    // Update is called once per frame
    void Update()
    {
        
    }

    public void DisplayItem(GameObject item, int List, int Pos)
    {
        Item = item;
        if(Item == null)              //no Item selected (display general Player Stats)
        {
            Stats playerStats = PlayerGO.GetComponent<Stats>();
            DisplayItemTexts[0].text = playerStats.Name;
            DisplayItemTexts[1].text = Classes[playerStats.CharacterClass - 1] + playerStats.Level + "]";
            DisplayItemTexts[2].text = "XP: " + playerStats.Experience;
            DisplayItemTexts[3].text = "Hitpoints: " + playerStats.Hitpoints + "/ " + playerStats.MaxHitpoints;
            DisplayItemTexts[4].text = "Mana: " + playerStats.Mana + "/ " + playerStats.MaxMana;
            DisplayItemTexts[5].text = "RK: " + playerStats.RK;
            DisplayItemTexts[6].text = "ETW0: " + playerStats.BaseETW0;
            DisplayItemTexts[7].text = null;
            DisplayItemTexts[8].text = null;
            DisplayItemTexts[9].text = null;
            DisplayItemTexts[10].text = null;
            ProjectileChange.gameObject.SetActive(false);
            return;
        }
        else if(!Item.GetComponent<InvItem>())
        {
            foreach(TextMeshProUGUI temp in DisplayItemTexts)
            {
                temp.text = "Error! No InvItem on Object";
            }
            return;
        }

        InvItem ItemInv = Item.GetComponent<InvItem>();
        DisplayItemTexts[0].text = ItemInv.Name;
        DisplayItemTexts[1].text = ItemInv.Description;
        DisplayItemTexts[2].text = "Price: " + ItemInv.Worth;
        for(int i = 3; i < DisplayItemTexts.Length; i++)
        {
            DisplayItemTexts[i].text = "";
        }

        if(Item.GetComponent<WeaponStats>())        //the Item is a Weapon
        {
            WeaponStats ItemWeaponStats = Item.GetComponent<WeaponStats>();
            Damage ItemDamage = Item.GetComponent<Damage>();
            
            float[] temp = ItemDamage.GetDescriptionDamage(PlayerGO.GetComponent<Stats>(), ItemWeaponStats);

            ItemStats[1] = temp[0];                                 //MinDamage
            ItemStats[2] = temp[1];                                 //MaxDamage
            ItemStats[3] = temp[2];                                 //AverageDamage
            ItemStats[4] = ItemWeaponStats.AttackCooldown;          //AttackCooldown
            ItemStats[5] = ItemStats[3] / ItemStats[4];             //DamagePerSecond
            ItemStats[8] = 0;                                       //Range
            if(ItemWeaponStats.NeededArrowType != 0)                //the weapon needs a Projectile (Bow or MagicWand)
            {
                if(!ItemWeaponStats.ArrowInvGO && ItemWeaponStats.ArrowPf && !ItemWeaponStats.UseItself)        //has a Prefab but no Item
                {
                    foreach(TextMeshProUGUI tempText in DisplayItemTexts)
                    {
                        tempText.text = "Error! No ArrowInvGO on Projectile";
                    }
                    return;
                }
                else if(ItemWeaponStats.UseItself)      //is using itself as a projectile
                {
                    ProjectileChange.gameObject.SetActive(false);
                }
                else
                {
                    ProjectileChange.gameObject.SetActive(true);
                    SetDropDown();
                    Debug.Log("Set DropDown!");
                }
                
                
                
                if(ItemWeaponStats.ManaRate == 0)
                {
                    ItemStats[6] = 0;
                    ItemStats[7] = 0;
                }
                else
                {
                    if(ItemWeaponStats.ArrowPf)
                    {
                        ItemStats[6] = Mathf.CeilToInt((ItemWeaponStats.Manacost + ItemWeaponStats.ArrowPf.GetComponent<Arrow>().Manacost) / ItemWeaponStats.ManaRate);
                        ItemStats[7] = ItemStats[3] / ItemStats[6];
                    }
                    else        //currently no Projectile selected
                    {
                        ItemStats[6] = Mathf.CeilToInt(ItemWeaponStats.Manacost / ItemWeaponStats.ManaRate);
                        ItemStats[7] = ItemStats[3] / ItemStats[6];
                    }
                }
                ItemStats[8] = Mathf.Pow(ItemWeaponStats.Force, 2)/(Mathf.Pow(ItemWeaponStats.ArrowPf.GetComponent<Rigidbody>().mass, 2) * 9.81f);           //Range
            }
            else            //the weapon is not using a ProjectilePrefab (is a Sword)
            {
                ProjectileChange.gameObject.SetActive(false);
                
                if(ItemWeaponStats.Manacost != 0)
                {
                    ItemStats[6] = Mathf.CeilToInt(ItemWeaponStats.Manacost/ ItemWeaponStats.ManaRate);
                    ItemStats[7] = ItemStats[3] / ItemStats[6];
                }
                else
                {
                    ItemStats[6] = 0;
                    ItemStats[7] = 0;
                }
            }
            
            string[] displayItemStats = Compared(Item, List, ItemStats);

            if(temp[1] != 0)            //MaxDamage != 0
            {
                ItemStats[0] = ItemWeaponStats.ETW0 + ItemDamage.Enchantment;
                if(ItemWeaponStats.ArrowPf)
                {
                    ItemStats[0] += ItemWeaponStats.ArrowPf.GetComponent<Damage>().Enchantment;
                    if(ItemWeaponStats.ArrowPf.GetComponent<Arrow>().Explosion)
                    {
                        DisplayItemTexts[3].text = "ETW0 Projectile: " + displayItemStats[0] + "   ETW0 Explosion: " + (ItemStats[0] + ItemWeaponStats.ArrowPf.GetComponent<Arrow>().Explosion.GetComponent<Damage>().Enchantment);
                    }
                    else
                    {
                        DisplayItemTexts[3].text = "ETW0: " + displayItemStats[0];
                    }
                }
                else
                {
                    DisplayItemTexts[3].text = "ETW0: " + displayItemStats[0];
                }
            }
            else
            {
                DisplayItemTexts[3].text = null;
            }

            
            
            
            DisplayItemTexts[4].text = "Minimal Damage: " + displayItemStats[1] + "   Maximum Damage: " + displayItemStats[2];
            DisplayItemTexts[4].text = "Average Damage: " + displayItemStats[3];
            DisplayItemTexts[5].text = "Cooldown: " + displayItemStats[4];
            DisplayItemTexts[7].gameObject.SetActive(true);
            DisplayItemTexts[7].text = "Damage per Second: " + displayItemStats[5];
            if(ItemWeaponStats.NeededArrowType != 0)
            {
                DisplayItemTexts[8].gameObject.SetActive(true);
                DisplayItemTexts[8].text = "Manacost: " + displayItemStats[6];
                if(ItemStats[7] == 0)
                {
                    DisplayItemTexts[9].gameObject.SetActive(false);
                }
                else
                {
                    DisplayItemTexts[9].gameObject.SetActive(true);
                    DisplayItemTexts[9].text = "Damage per Mana: " + displayItemStats[7];
                }
            }
            else
            {
                DisplayItemTexts[8].gameObject.SetActive(false);
                DisplayItemTexts[9].gameObject.SetActive(false);
            }
            DisplayItemTexts[10].text = "Range: " + displayItemStats[8];
        }
        else if(Item.GetComponent<Arrow>())
        {
            ProjectileChange.gameObject.SetActive(false);
            Damage ItemDamage = Item.GetComponent<Damage>();
            Arrow arrowStats = Item.GetComponent<Arrow>();
            float[] temp = ItemDamage.GetDescriptionDamage(PlayerGO.GetComponent<Stats>(), null);

            if(temp[1] != 0)
            {
                ItemStats[0] = ItemDamage.Enchantment;
                if(arrowStats.Explosion)
                {
                    DisplayItemTexts[3].text = "Enchantment Projectile: " + ItemStats[0] + "   Explosion: " + (ItemStats[0] + arrowStats.Explosion.GetComponent<Damage>().Enchantment);
                }
                else
                {
                    DisplayItemTexts[3].text = "Enchantment: " + ItemStats[0];
                }
            }
            else
            {
                DisplayItemTexts[3].text = null;
            }

            ItemStats[1] = temp[0];                                 //MinDamage
            ItemStats[2] = temp[1];                                 //MaxDamage
            ItemStats[3] = temp[2];                                 //AverageDamage
            ItemStats[4] = arrowStats.AttackCooldown;          //AttackCooldown
            ItemStats[5] = ItemStats[3] / ItemStats[4];             //DamagePerSecond
            if(arrowStats.Manacost == 0)
            {
                ItemStats[6] = 0;
                ItemStats[7] = 0;
            }
            else
            {
                ItemStats[6] = arrowStats.Manacost;
                ItemStats[7] = ItemStats[3] / ItemStats[6];
            }
            ItemStats[8] = 0;                                                               //Range
            
            DisplayItemTexts[4].text = "Minimal Damage: " + ItemStats[1] + "   Maximum Damage: " + ItemStats[2];
            DisplayItemTexts[5].text = "Average Damage: " + ItemStats[3];
            DisplayItemTexts[6].text = "Cooldown: " + ItemStats[4];
            DisplayItemTexts[7].text = null;
            DisplayItemTexts[7].gameObject.SetActive(false);
            DisplayItemTexts[8].gameObject.SetActive(true);
            DisplayItemTexts[8].text = "Manacost: " + ItemStats[6];
            if(ItemStats[6] == 0)
            {
                DisplayItemTexts[9].gameObject.SetActive(false);
            }
            else
            {
                DisplayItemTexts[9].gameObject.SetActive(true);
                DisplayItemTexts[9].text = "Damage per Mana: " + ItemStats[7];
            }
            DisplayItemTexts[10].text = "Range: " + ItemStats[8];
        }
        else if(Item.GetComponent<ArmorStats>())
        {
            ArmorStats ItemArmor = Item.GetComponent<ArmorStats>();

            if(ItemArmor.Shield)
            {
                DisplayItemTexts[3].text = "Armor Slot: Shield";
                DisplayItemTexts[4].text = "Shield RK: " + ItemArmor.ShieldRK;
            }
            else
            {
                DisplayItemTexts[3].text = "Armor Slot: " + ItemArmor.ArmorSlot;
                DisplayItemTexts[4].text = "Shield RK: " + ItemArmor.ArmorRK;
            }
            
            DisplayItemTexts[5].text = "Enchantment: " + ItemArmor.Enchantment;

            DisplayItemTexts[6].text = null;
            DisplayItemTexts[7].text = null;
            DisplayItemTexts[8].text = null;
            DisplayItemTexts[9].text = null;
            DisplayItemTexts[10].text = null;
            ProjectileChange.gameObject.SetActive(false);
        }
        else if(Item.GetComponent<FoodStats>())
        {
            List<string> temp = new();
            FoodStats tStats = Item.GetComponent<FoodStats>();
            if(tStats.HP != 0)
            {
                temp.Add("Adds " + tStats.HP + " Hitpoints");
            }
            if(tStats.Mana != 0)
            {
                temp.Add("Adds " + tStats.Mana + " Mana");
            }
            if(tStats.duration > 0)
            {
                if(tStats.duration != 1)
                {
                    temp.Add("Following effects last for " + tStats.duration + " seconds:");
                }
                else
                {
                    temp.Add("Following effects last for 1 second:");
                }
                int templength = temp.Count;
                if(tStats.damageMult != 1)
                {
                    if(tStats.damageMult > 1)
                    {
                        temp.Add("Improves your Damage by " + (tStats.damageMult - 1) * 100 + "%");
                    }
                    else
                    {
                        temp.Add("Worsens your Damage by " + (1 - tStats.damageMult) * 100 + "%");
                    }
                }
                if(tStats.attackSpeedMult != 1)
                {
                    if(tStats.attackSpeedMult > 1)
                    {
                        temp.Add("Improves your Attack Speed by " + (tStats.attackSpeedMult - 1) * 100 + "%");
                    }
                    else
                    {
                        temp.Add("Worsens your Attack Speed by " + (1 - tStats.attackSpeedMult) * 100 + "%");
                    }
                }
                if(tStats.tempRK != 0)
                {
                    if(tStats.tempRK < 0)
                    {
                        temp.Add("Improves your Armor Class by " + -tStats.tempRK + " RK");
                    }
                    else
                    {
                        temp.Add("Worsens your Armor Class by " + tStats.tempRK + " RK");
                    }
                }
                if(tStats.tempETW0 != 0)
                {
                    if(tStats.tempETW0 > 0)
                    {
                        temp.Add("Improves your Hit Throw by " + tStats.tempETW0);
                    }
                    else
                    {
                        temp.Add("Worsens your Hit Throw by " + -tStats.tempETW0);
                    }
                }
                if(tStats.speedMult != 1)
                {
                    if(tStats.speedMult > 1)
                    {
                        temp.Add("Improves your Movement Speed by " + (tStats.speedMult - 1) * 100 + "%");
                    }
                    else
                    {
                        temp.Add("Worsens your Movement Speed by " + (1 - tStats.speedMult) * 100 + "%");
                    }
                }
                if(tStats.HitpointsMult != 1)
                {
                    if(tStats.HitpointsMult > 1)
                    {
                        temp.Add("Improves your Hitpoints by " + (tStats.HitpointsMult - 1) * 100 + "%");
                    }
                    else
                    {
                        temp.Add("Worsens your Hitpoints by " + (1 - tStats.HitpointsMult) * 100 + "%");
                    }
                }
                if(tStats.LPRegen != 0)
                {
                    if(tStats.LPRegen > 0)
                    {
                        temp.Add("Regenerates " + tStats.LPRegen + " HP per second");
                    }
                    else
                    {
                        temp.Add("Degenerates " + -tStats.LPRegen + " HP per second");
                    }
                }
                if(tStats.ManaMult != 1)
                {
                    if(tStats.ManaMult > 1)
                    {
                        temp.Add("Improves your Mana by " + (tStats.ManaMult - 1) * 100 + "%");
                    }
                    else
                    {
                        temp.Add("Worsens your Mana by " + (1 - tStats.ManaMult) * 100 + "%");
                    }
                }
                if(tStats.ManaRegen != 0)
                {
                    if(tStats.ManaRegen > 0)
                    {
                        temp.Add("Regenerates " + tStats.ManaRegen + " Mana per second");
                    }
                    else
                    {
                        temp.Add("Degenerates " + -tStats.ManaRegen + " Mana per second");
                    }
                }

                if(templength == temp.Count)        //there are no Effects so duration is not needed
                {
                    temp.RemoveAt(templength - 1);
                }
                else if(templength == temp.Count - 1)
                {
                    if(tStats.duration != 1)
                    {
                        temp[templength - 1] = "Following effect lasts for " + tStats.duration + " seconds:";
                    }
                    else
                    {
                        temp[templength - 1] = "Following effect lasts for 1 second:";
                    }
                    
                }
            }
            
            for(int i = 0; i < DisplayItemTexts.Length - 3; i++)
            {
                DisplayItemTexts[i + 3].text = null;
            }
            for(int i = 0; i < temp.Count; i++)
            {
                if(i + 3 < DisplayItemTexts.Length)
                {
                    DisplayItemTexts[i + 3].text = temp[i];
                }
                else
                {
                    break;
                }
            }
        }
    }

    private string[] Compared(GameObject Item, int List, float[] tempValues2)
    {
        string[] tempResult = new string[9];
        float[] tempValues1 = new float[9];
        GameObject comparableGO;
        if(List == 2)           //Item is already equipped (no comparison required)
        {
            for(int i = 0; i < 9; i++)
            {
                tempResult[i] = "" + ItemStats[i];
            }
            return tempResult;
        }
        else
        {
            if(Item.GetComponent<WeaponStats>() != null)
            {
                if(playerInv.EquipList[0])
                {
                    comparableGO = playerInv.EquipList[0];
                }
                else if(playerInv.EquipList[2])
                {
                    comparableGO = playerInv.EquipList[2];
                }
                else
                {
                    for(int i = 0; i < 9; i++)
                    {
                        if(i == 4)
                        {
                            tempResult[i] = ItemStats[i] + "";
                        }
                        else if(i == 6)
                        {
                            tempResult[i] = ItemStats[i] + " ---";
                        }
                        else
                        {
                            tempResult[i] = ItemStats[i] + " +++";
                        }
                    }
                    return tempResult;
                }

                float[] descrDamage = comparableGO.GetComponent<Damage>().GetDescriptionDamage(PlayerGO.GetComponent<Stats>(), comparableGO.GetComponent<WeaponStats>());
                
                tempValues1[1] = descrDamage[0];                                 //MinDamage
                tempValues1[2] = descrDamage[1];                                 //MaxDamage
                tempValues1[3] = descrDamage[2];                                 //AverageDamage
                tempValues1[4] = comparableGO.GetComponent<WeaponStats>().AttackCooldown;          //AttackCooldown
                tempValues1[5] = tempValues1[3] / tempValues1[4];             //DamagePerSecond
                if(comparableGO.GetComponent<WeaponStats>().Manacost == 0)
                {
                    tempValues1[6] = 0;
                    tempValues1[7] = 0;
                }
                else
                {
                    tempValues1[6] = comparableGO.GetComponent<WeaponStats>().Manacost;
                    tempValues1[7] = tempValues1[3] / tempValues1[6];
                }
                if(comparableGO.GetComponent<WeaponStats>().ArrowSpawn)
                {
                    ItemStats[8] = Mathf.Pow(comparableGO.GetComponent<WeaponStats>().Force, 2)/(Mathf.Pow(comparableGO.GetComponent<WeaponStats>().ArrowPf.GetComponent<Rigidbody>().mass, 2) * 9.81f);           //Range
                }
                else
                {
                    tempValues1[8] = 0;
                }
                
                for(int i = 0; i < 9; i++)
                {
                    if(i == 4 || i == 6)        //smaller is better
                    {
                        tempResult[i] = tempValues2[i] + compare(tempValues2[i], tempValues1[i]);
                    }
                    else        //bigger is better
                    {
                        tempResult[i] = tempValues2[i] + compare(tempValues1[i], tempValues2[i]);
                    }
                }

                return tempResult;
            }
            else if(Item.GetComponent<ArmorStats>() != null)
            {
                if(Item.GetComponent<ArmorStats>().Shield)
                {
                    comparableGO = playerInv.EquipList[1];
                }
                else
                {
                    if(Item.GetComponent<ArmorStats>().ArmorSlot == 1)
                    {
                        comparableGO = playerInv.EquipList[3];
                    }
                    else if(Item.GetComponent<ArmorStats>().ArmorSlot == 2)
                    {
                        comparableGO = playerInv.EquipList[4];
                    }
                    else if(Item.GetComponent<ArmorStats>().ArmorSlot == 3)
                    {
                        comparableGO = playerInv.EquipList[5];
                    }
                    else if(Item.GetComponent<ArmorStats>().ArmorSlot == 4)
                    {
                        comparableGO = playerInv.EquipList[6];
                    }
                }
            }
            else if(Item.GetComponent<MagicStats>() != null)
            {
                if(Item.GetComponent<MagicStats>().Ring)
                {
                    if(playerInv.EquipList[7] && playerInv.EquipList[8])
                    {
                        comparableGO = playerInv.EquipList[7];
                    }
                }
                else
                {
                    comparableGO = playerInv.EquipList[9];
                }
            }
            else
            {
                for(int i = 0; i < 9; i++)
                {
                    tempResult[i] = "" + ItemStats[i];
                }
                return tempResult;
            }
        }
        return null;
    }

    private string compare(float value1, float value2)
    {
        float ratio = 1;
        if(value1 != 0)
        {
            ratio = value2/value1;
        }
        else if(value2 != 0)
        {
            ratio = 2;
        }
        
        if(ratio > 1)
        {
            if(ratio >= 1.5)            //over 50% increase
            {
                return " +++";
            }
            else if(ratio >= 1.2)       //20% to 50% increase
            {
                return " ++";
            }
            else                        //up to 20% increase
            {
                return " +";
            }
        }
        else if(ratio < 1)
        {
            if(ratio <= 0.5)            //over 50% decrease
            {
                return " ---";
            }
            else if(ratio <= 0.8)       //20% to 50% decrease
            {
                return " --";
            }
            else                        //up to 20% decrease
            {
                return " -";
            }
        }
        else
        {
            return "";
        }
    }

    public void SetDropDown()
    {
        projectiles = playerInv.GetItems(1, 4, Item.GetComponent<WeaponStats>().NeededArrowType + 1);
        List<string> options = new List<string>();
        int temp = -1;
        ProjectileChange.ClearOptions();
        options.Add("None");
        for(int i = 0; i < projectiles.Count; i++)
        {
            options.Add(playerInv.inventory[projectiles[i]].GetComponent<InvItem>().Name);
            if(Item.GetComponent<WeaponStats>().ArrowInvGO)
            {
                if(playerInv.inventory[projectiles[i]] == Item.GetComponent<WeaponStats>().ArrowInvGO)      //if this current object is the equipped Arrow
                {
                    temp = i;
                    Debug.Log("found equipped Arrow!");
                }
            }
        }
        ProjectileChange.AddOptions(options);
        if(temp != -1)          //if the already equipped Arrow was found in the options
        {
            ProjectileChange.value = temp + 1;      //change the selected Dropdown option to the right Arrow
            Debug.Log("Selected Option: " + (temp + 1));
        }
    }

    public void SetProjectile()
    {
        if(ProjectileChange.value == 0)
        {
            Item.GetComponent<WeaponStats>().ChangeArrow(null, null);
        }
        else
        {
            Item.GetComponent<WeaponStats>().ChangeArrow(null, playerInv.inventory[projectiles[ProjectileChange.value - 1]]);
        }
    }
}
