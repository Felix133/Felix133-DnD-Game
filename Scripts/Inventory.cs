using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Inventory : MonoBehaviour
{
    [Header("Parents")]
    public GameObject InvParent;

    public GameObject RightParent;
    public GameObject LeftParent;
    public GameObject MiddleParent;

    public GameObject HeadParent;
    public GameObject ChestParent;
    public GameObject LegsParent;
    public GameObject ShoesParent;

    public GameObject MLeftParent;
    public GameObject MRightParent;
    public GameObject MMiddleParent;
    public GameObject PickUpItemsPf;      //Prefab

    [Header("Inventory")]

    public List<GameObject> inventory;
    public List<GameObject> PickUpList;
    public GameObject[] EquipList = new GameObject[10];
    private List<GameObject> tempList;
    public GameObject Coins;

    [Header("equipped Items")]

    //public GameObject EquipList[0];            //Right (Weapons)
    //public GameObject EquipList[1];            //Left (Shields)
    //public GameObject SlotM;            //Middle (Weapons)

    //public GameObject SlotA1;           //Armor Slot 1 (Head)
    //public GameObject SlotA2;           //Chest
    //public GameObject SlotA3;           //Legs
    //public GameObject EquipList[6];           //Shoes

    //public GameObject EquipList[7];           //left Ring
    //public GameObject EquipList[8];           //right Ring
    //public GameObject EquipList[9];           //Amulett

    [Header("GO for shared Meshes")]
    public GameObject[] SharedMeshes = new GameObject[5];
    
    //public GameObject SlotA1V;           //Armor Slot 1 (Head)
    //public GameObject SlotA2V;           //Chest
    //public GameObject SlotA3V;           //Legs
    //public GameObject SlotA4V;           //Shoes

    //public GameObject SlotM3V;           //Amulett
    
    [Header("Runtime")]
    private int tempType;               //1:MeleeWeapon; 2:RangeWeapon; 3:Shield; 4:Armor
    private bool tempTwoHanded;         //twoHanded Weapons or Shields block other Slots
    private int tempArmorSlot;          //1:Head; 2:Chest; 3:Legs; 4:Shoes
    private int tempMagicType;          //1:Ring; 2:Amulett
    private bool tempEquipable;
    
    // Start is called before the first frame update
    void Start()
    {
        GetEquipment();

        if(InvParent.transform.childCount != 0)
        {
            for(int i = 0; i < InvParent.transform.childCount; i++)
            {
                if(!inventory.Contains(InvParent.transform.GetChild(i).gameObject))
                {
                    inventory.Add(InvParent.transform.GetChild(i).gameObject);
                }
                InvParent.transform.GetChild(i).gameObject.SetActive(false);
            }
            inventory = inventory.OrderBy(go => go.name).ToList();
        }

        if(gameObject.layer == 6)
        {
            for(int i = 0; i < InvParent.transform.childCount; i++)
            {
                InvParent.transform.GetChild(i).GetComponent<InvItem>().ReloadName();
            }
            StartCoroutine(RestartInv());
        }
        else
        {
            for(int i = 0; i < InvParent.transform.childCount; i++)
            {
                InvParent.transform.GetChild(i).GetComponent<InvItem>().ReloadName();
                InvParent.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        FindCoins();

        UpdateRK();
    }

    IEnumerator RestartInv()
    {
        yield return new WaitForSeconds(0.01f);
        PlayerController.playerContr.unloadInv();
        //yield return new WaitForSeconds(1f);
        for(int i = 0; i < inventory.Count; i ++)
        {
            inventory[i].SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateRK()
    {
        int tempBaseRK = 10;
        if(EquipList[3] != null)
        {
            tempBaseRK = EquipList[3].GetComponent<ArmorStats>().ArmorRK - EquipList[3].GetComponent<ArmorStats>().Enchantment;
        }
        if(EquipList[4] != null)
        {
            tempBaseRK += EquipList[4].GetComponent<ArmorStats>().ArmorRK - EquipList[4].GetComponent<ArmorStats>().Enchantment;
        }
        if(EquipList[5] != null)
        {
            tempBaseRK += EquipList[5].GetComponent<ArmorStats>().ArmorRK - EquipList[5].GetComponent<ArmorStats>().Enchantment;
        }
        if(EquipList[6] != null)
        {
            tempBaseRK += EquipList[6].GetComponent<ArmorStats>().ArmorRK - EquipList[6].GetComponent<ArmorStats>().Enchantment;
        }

        int tempShieldRK = 0;
        if(EquipList[1] != null)
        {
            tempShieldRK += EquipList[1].GetComponent<ArmorStats>().ShieldRK - EquipList[1].GetComponent<ArmorStats>().Enchantment;
        }
        
        
        this.gameObject.GetComponent<Stats>().BaseRK = tempBaseRK;
        this.gameObject.GetComponent<Stats>().ShieldRK = tempShieldRK;
    }
    
    public void FindCoins()
    {
        foreach(int i in GetItems( 1, 6))
        {
            if(inventory[i].GetComponent<MoneyStats>())
            {
                if(!Coins)
                {
                    Coins = inventory[i];
                }
                else
                {
                    Coins.GetComponent<InvItem>().Count += inventory[i].GetComponent<InvItem>().Count;
                    inventory[i].GetComponent<InvItem>().Count = 0;
                }
            }
        }
    }
    
    public void GetEquipment()
    {
        if(RightParent.transform.childCount != 0)
        {
            EquipList[0] = RightParent.transform.GetChild(0).gameObject;
        }
        if(LeftParent.transform.childCount != 0)
        {
            EquipList[1] = LeftParent.transform.GetChild(0).gameObject;
        }
        if(MiddleParent.transform.childCount != 0)
        {
            EquipList[2] = MiddleParent.transform.GetChild(0).gameObject;
        }
        
        if(HeadParent.transform.childCount != 0)
        {
            EquipList[3] = HeadParent.transform.GetChild(0).gameObject;
        }
        if(ChestParent.transform.childCount != 0)
        {
            EquipList[4] = ChestParent.transform.GetChild(0).gameObject;
        }
        if(LegsParent.transform.childCount != 0)
        {
            EquipList[5] = LegsParent.transform.GetChild(0).gameObject;
        }
        if(ShoesParent.transform.childCount != 0)
        {
            EquipList[6] = ShoesParent.transform.GetChild(0).gameObject;
        }

        if(MLeftParent.transform.childCount != 0)
        {
            EquipList[7] = MLeftParent.transform.GetChild(0).gameObject;
        }
        if(MRightParent.transform.childCount != 0)
        {
            EquipList[8] = MRightParent.transform.GetChild(0).gameObject;
        }
        if(MMiddleParent.transform.childCount != 0)
        {
            EquipList[9] = MMiddleParent.transform.GetChild(0).gameObject;
        }
    }
    
    public void PopSlot(int Slot, int ListNumber)
    {
        if(ListNumber == 1)         //1:Inventory; 3:PickUpList
        {
            tempList = inventory;
        }
        else if(ListNumber == 3)
        {
            tempList = PickUpList;
        }
        else
        {
            return;
        }

        if(Slot <= 0)
        {
            return;
        }
        
        if(EquipList[Slot-1] == null)
        {
            Debug.Log("Slot is already empty!");
            return;
        }
        
        if(4 <= Slot && Slot <= 7)
        {
            SharedMeshes[Slot-4].GetComponent<SkinnedMeshRenderer>().sharedMesh = null;      //war eig. .mesh aber hatte Fehler
        }
        else if(Slot == 10)
        {
            SharedMeshes[4].GetComponent<SkinnedMeshRenderer>().sharedMesh = null;
        }
        EquipList[Slot-1].transform.SetParent(InvParent.transform, false);
        EquipList[Slot-1].SetActive(false);
        tempList.Add(EquipList[Slot-1]);
        EquipList[Slot-1] = null;
        UpdateRK();
        
        foreach (Transform child in InvParent.transform)
        {
            child.gameObject.SetActive(false);
        }
        inventory = inventory.OrderBy(go => go.name).ToList();
        PickUpList = PickUpList.OrderBy(go => go.name).ToList();
    }

    public void AddToSlot(int itemNumber, int Slot, int ListNumber)
    {
        if(ListNumber == 1)         //1:Inventory; 3:PickUpList
        {
            tempList = inventory;
        }
        else if(ListNumber == 3)
        {
            tempList = PickUpList;
        }
        else
        {
            return;
        }
        
        if(itemNumber <= (tempList.Count -1))          //itemNumber startet bei 0, also wird bei 0 das erste Objekt ausgegeben
        {
            if(tempList[itemNumber] != null)
            {
                Check(tempList[itemNumber]);
                Change(itemNumber, Slot, ListNumber);
            }
        }
    }

    private void Change(int itemNumber, int Slot, int ListNumber)       //tries to equip the selected item at the selected Equipment Slot
    {
        if(!tempEquipable)
        {
            return;
        }

        if(ListNumber == 1)         //1:Inventory; 3:PickUpList
        {
            tempList = inventory;
        }
        else if(ListNumber == 3)
        {
            tempList = PickUpList;
        }
        else
        {
            return;
        }
        
        if(RightParent.transform.childCount > 0)        //destroy temporary weapons
        {
            if(RightParent.transform.GetChild(0).TryGetComponent<InvItem>(out InvItem invItem))
            {
                if(invItem.Temporary)
                {
                    Destroy(RightParent.transform.GetChild(0).gameObject);
                }
                else
                {
                    EquipList[0] = RightParent.transform.GetChild(0).gameObject;
                }
            }
            else
            {
                Destroy(RightParent.transform.GetChild(0).gameObject);
            }
        }
        
        if(tempType == 1)       //Meleeweapons
        {
            if(tempTwoHanded)
            {
                if(Slot != 3)       //zweihändige Waffe kann nur in den EquipList[2]
                {
                    return;
                }
                if(EquipList[2] == null)
                {
                    tempList[itemNumber].transform.SetParent(MiddleParent.transform, false);
                    tempList[itemNumber].SetActive(true);
                    EquipList[2] = tempList[itemNumber];                     
                    tempList.RemoveAt(itemNumber);
                }
                else            //wenn schon ein zweihändiger Gegenstand ausgerüstet ist wird nur getauscht
                {
                    tempList[itemNumber].transform.SetParent(MiddleParent.transform, false);
                    tempList[itemNumber].SetActive(true);
                    EquipList[2] = tempList[itemNumber];                     
                    tempList.RemoveAt(itemNumber);
                    PopSlot(Slot, ListNumber);
                }
                
                if(EquipList[0] != null)       //zweihändige Waffe muss das einzige Objekt in den Händen sein
                {
                    PopSlot(1, 1);       //andere Waffen werden ins Inventar gelegt
                }
                if(EquipList[1] != null)
                {
                    PopSlot(2, 1);       //andere Waffen werden ins Inventar gelegt
                }
            }
            else
            {
                if(Slot == 1)       //Waffe muss in die rechte Hand
                {
                    if(EquipList[0] == null)
                    {
                        tempList[itemNumber].transform.SetParent(RightParent.transform, false);
                        tempList[itemNumber].SetActive(true);
                        EquipList[0] = tempList[itemNumber];
                        tempList.Remove(tempList[itemNumber]);
                        inventory.Remove(tempList[itemNumber]);
                    }
                    else            //wenn schon ein Gegenstand ausgerüstet ist wird nur getauscht
                    {
                        tempList[itemNumber].transform.SetParent(RightParent.transform, false);
                        tempList[itemNumber].SetActive(true);
                        EquipList[0] = tempList[itemNumber];
                        tempList.RemoveAt(itemNumber);
                        PopSlot(Slot, ListNumber);
                    }
                    if(EquipList[2] != null)
                    {
                        PopSlot(3, 1);       //beidhändige Waffe wird ins Inventar gelegt
                    }
                }
            }
        }
        else if(tempType == 2)       //Rangeweapons
        {
            if(!tempTwoHanded)          //Rangeweapons are always twoHanded
            {
                return;
            }

            if(Slot != 3)
            {
                return;
            }

            if(EquipList[2] == null)
            {
                tempList[itemNumber].transform.SetParent(MiddleParent.transform, false);
                tempList[itemNumber].SetActive(true);
                EquipList[2] = tempList[itemNumber];                     
                tempList.RemoveAt(itemNumber);
            }
            else            //wenn schon ein Gegenstand ausgerüstet ist wird nur getauscht
            {
                tempList[itemNumber].transform.SetParent(MiddleParent.transform, false);
                tempList[itemNumber].SetActive(true);
                EquipList[2] = tempList[itemNumber];                     
                tempList.RemoveAt(itemNumber);
                PopSlot(Slot, ListNumber);
            }

            if(EquipList[0] != null)
            {
                PopSlot(1, 1);       //andere Waffen werden ins Inventar gelegt
            }
            if(EquipList[1] != null)
            {
                PopSlot(2, 1);       //andere Waffen werden ins Inventar gelegt
            }
        }
        else if(tempType == 3)       //Shields
        {
            if(EquipList[2] != null)
            {
                PopSlot(3, 1);       //andere Waffen werden ins Inventar gelegt
            }

            if(Slot != 2)       //Schilder sind immer in der linken Hand
            {
                return;
            }

            if(EquipList[1] == null)
            {
                tempList[itemNumber].transform.SetParent(LeftParent.transform, false);
                tempList[itemNumber].SetActive(true);
                EquipList[1] = tempList[itemNumber];                     
                tempList.RemoveAt(itemNumber);
            }
            else            //wenn schon ein Gegenstand ausgerüstet ist wird nur getauscht
            {
                tempList[itemNumber].transform.SetParent(LeftParent.transform, false);
                tempList[itemNumber].SetActive(true);
                EquipList[1] = tempList[itemNumber];                     
                tempList.RemoveAt(itemNumber);
                PopSlot(Slot, ListNumber);
            }
            UpdateRK();
        }
        else if(tempType == 4)       //Armor
        {
            if(tempArmorSlot == 1 && Slot == 4)
            {
                SharedMeshes[0].GetComponent<SkinnedMeshRenderer>().sharedMesh = tempList[itemNumber].transform.GetChild(0).GetChild(0).GetComponent<MeshFilter>().mesh;
                if(EquipList[3] == null)
                {
                    tempList[itemNumber].transform.SetParent(HeadParent.transform, false);
                    tempList[itemNumber].SetActive(true);
                    EquipList[3] = tempList[itemNumber];                     
                    tempList.RemoveAt(itemNumber);
                }
                else            //wenn schon ein Gegenstand ausgerüstet ist wird nur getauscht
                {
                    tempList[itemNumber].transform.SetParent(HeadParent.transform, false);
                    tempList[itemNumber].SetActive(true);
                    EquipList[3] = tempList[itemNumber];                     
                    tempList.RemoveAt(itemNumber);
                    PopSlot(Slot, ListNumber);
                }
            }
            else if(tempArmorSlot == 2 && Slot == 5)
            {
                if(EquipList[4] == null)
                {
                    tempList[itemNumber].transform.SetParent(ChestParent.transform, false);
                    tempList[itemNumber].SetActive(true);
                    EquipList[4] = tempList[itemNumber];                     
                    tempList.RemoveAt(itemNumber);
                }
                else            //wenn schon ein Gegenstand ausgerüstet ist wird nur getauscht
                {
                    tempList[itemNumber].transform.SetParent(ChestParent.transform, false);
                    tempList[itemNumber].SetActive(true);
                    EquipList[4] = tempList[itemNumber];                     
                    tempList.RemoveAt(itemNumber);
                    PopSlot(Slot, ListNumber);
                }
            }
            else if(tempArmorSlot == 3 && Slot == 6)
            {
                if(EquipList[5] == null)
                {
                    tempList[itemNumber].transform.SetParent(LegsParent.transform, false);
                    tempList[itemNumber].SetActive(true);
                    EquipList[5] = tempList[itemNumber];                     
                    tempList.RemoveAt(itemNumber);
                }
                else            //wenn schon ein Gegenstand ausgerüstet ist wird nur getauscht
                {
                    tempList[itemNumber].transform.SetParent(LegsParent.transform, false);
                    tempList[itemNumber].SetActive(true);
                    EquipList[5] = tempList[itemNumber];                     
                    tempList.RemoveAt(itemNumber);
                    PopSlot(Slot, ListNumber);
                }
            }
            else if(tempArmorSlot == 4 && Slot == 7)
            {
                if(EquipList[6] == null)
                {
                    tempList[itemNumber].transform.SetParent(ShoesParent.transform, false);
                    tempList[itemNumber].SetActive(true);
                    EquipList[6] = tempList[itemNumber];                     
                    tempList.RemoveAt(itemNumber);
                }
                else            //wenn schon ein Gegenstand ausgerüstet ist wird nur getauscht
                {
                    tempList[itemNumber].transform.SetParent(ShoesParent.transform, false);
                    tempList[itemNumber].SetActive(true);
                    EquipList[6] = tempList[itemNumber];                     
                    tempList.RemoveAt(itemNumber);
                    PopSlot(Slot, ListNumber);
                }
            }
        }
        else if(tempType == 5)   //Ring
        {
            if(Slot == 8)
            {
                if(EquipList[7] == null)
                {
                    tempList[itemNumber].transform.SetParent(MLeftParent.transform, false);
                    tempList[itemNumber].SetActive(true);
                    EquipList[7] = tempList[itemNumber];                     
                    tempList.RemoveAt(itemNumber);
                }
                else            //wenn schon ein Gegenstand ausgerüstet ist wird nur getauscht
                {
                    tempList[itemNumber].transform.SetParent(MLeftParent.transform, false);
                    tempList[itemNumber].SetActive(true);
                    EquipList[7] = tempList[itemNumber];                     
                    tempList.RemoveAt(itemNumber);
                    PopSlot(Slot, ListNumber);
                }
            }
            else if(Slot == 9)
            {
                if(EquipList[8] == null)
                {
                    tempList[itemNumber].transform.SetParent(MRightParent.transform, false);
                    tempList[itemNumber].SetActive(true);
                    EquipList[8] = tempList[itemNumber];                     
                    tempList.RemoveAt(itemNumber);
                }
                else            //wenn schon ein Gegenstand ausgerüstet ist wird nur getauscht
                {
                    tempList[itemNumber].transform.SetParent(MRightParent.transform, false);
                    tempList[itemNumber].SetActive(true);
                    EquipList[8] = tempList[itemNumber];                     
                    tempList.RemoveAt(itemNumber);
                    PopSlot(Slot, ListNumber);
                }
            }
        }
        else if(tempType == 6)   //Amulett
        {
            if(Slot == 10)
            {
                if(EquipList[9] == null)
                {
                    tempList[itemNumber].transform.SetParent(MMiddleParent.transform, false);
                    tempList[itemNumber].SetActive(true);
                    EquipList[9] = tempList[itemNumber];                     
                    tempList.RemoveAt(itemNumber);
                }
                else            //wenn schon ein Gegenstand ausgerüstet ist wird nur getauscht
                {
                    tempList[itemNumber].transform.SetParent(MMiddleParent.transform, false);
                    tempList[itemNumber].SetActive(true);
                    EquipList[9] = tempList[itemNumber];                     
                    tempList.RemoveAt(itemNumber);
                    PopSlot(Slot, ListNumber);
                }
            }
        }
        UpdateRK();
        
        foreach (Transform child in InvParent.transform)
        {
            child.gameObject.SetActive(false);
        }
        inventory = inventory.OrderBy(go => go.name).ToList();
        PickUpList = PickUpList.OrderBy(go => go.name).ToList();
    }

    private void Check(GameObject temp)         //gets the data to check where the item is equipable
    {
        if(temp.GetComponent<WeaponStats>() != null)
        {
            tempTwoHanded = temp.GetComponent<WeaponStats>().TwoHanded;
            if(temp.GetComponent<WeaponStats>().Melee)
            {
                tempType = 1;       //Meleeweapon
            }
            else
            {
                tempType = 2;       //Rangeweapon
            }
            tempEquipable = true;
        }
        else if(temp.GetComponent<ArmorStats>() != null)
        {
            if(temp.GetComponent<ArmorStats>().Shield)
            {
                tempType = 3;       //Shield
                tempArmorSlot = temp.GetComponent<ArmorStats>().ArmorSlot;
            }
            else
            {
                tempType = 4;       //Armor
                tempArmorSlot = temp.GetComponent<ArmorStats>().ArmorSlot;
            }
            tempEquipable = true;
        }
        else if(temp.GetComponent<MagicStats>() != null)
        {
            if(temp.GetComponent<MagicStats>().Ring)
            {
                tempType = 5;       //Ring
            }
            else
            {
                tempType = 6;       //Amulett
            }
            tempEquipable = true;
        }
        else
        {
            tempEquipable = false;
        }
    }

    public void Move(int itemNumber, int ListNumber1, int ListNumber2)      //Item wird von ListeNr.1 nach ListeNr.2 bewegt
    {
        if(ListNumber1 == ListNumber2)
        {
            return;
        }
        if(ListNumber1 == 2 || ListNumber2 == 2)
        {
            return;
        }

        if(ListNumber1 == 1)        //Inventar nach PickUp
        {
            if(itemNumber <= (inventory.Count -1))
            {
                if(inventory[itemNumber] != null)       //muss etwas beinhalten
                {
                    if(PickUpList == null)      //currently no PickUpList exists
                    {
                        PickUpList = new List<GameObject>();
                    }
                    PickUpList.Add(inventory[itemNumber]);
                    inventory.RemoveAt(itemNumber);
                }
            }
        }
        else                        //PickUp nach Inventar
        {
            if(itemNumber <= (PickUpList.Count -1))
            {
                if(PickUpList[itemNumber] != null)
                {
                    PickUpList[itemNumber].transform.SetParent(InvParent.transform, false);
                    inventory.Add(PickUpList[itemNumber]);
                    PickUpList.RemoveAt(itemNumber);
                }
            }
        }

        foreach (Transform child in InvParent.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void DropItems()
    {
        GameObject tempPickUp = Instantiate(PickUpItemsPf, gameObject.transform.position + new Vector3(0, -1, 0), gameObject.transform.rotation);
        tempPickUp.transform.SetParent(null);

        List<GameObject> tempList = new();
        if(inventory.Count > 0)
        {
            foreach(GameObject gO in inventory)
            {
                if(gO != null)
                {
                    if(gO.TryGetComponent<InvItem>(out InvItem x))
                    {
                        if(x.Temporary || x.DestroyOnDeath)
                        {
                            Destroy(gO);
                            continue;
                        }
                        
                        tempList.Add(gO);
                    }
                    else
                    {
                        Destroy(gO);
                        continue;
                    }
                }
            }
        }
        
        for(int i = 0; i < EquipList.Length; i++)
        {
            if(EquipList[i])
            {
                if(EquipList[i].TryGetComponent<InvItem>(out InvItem x))
                {
                    if(x.Temporary || x.DestroyOnDeath)
                    {
                        Destroy(EquipList[i]);
                        continue;
                    }
                    
                    tempList.Add(EquipList[i]);
                    EquipList[i] = null;
                }
                else
                {
                    Destroy(EquipList[i]);
                    continue;
                }
            }
        }
        inventory = new();

        if(GetComponent<GenerateItems>())
        {
            foreach(GameObject item in GetComponent<GenerateItems>().GetDrops())
            {
                if(item)
                {
                    item.transform.SetParent(tempPickUp.GetComponent<PickUpScript>().ItemsParent.transform, false);
                    item.GetComponent<InvItem>().Collectable = true;
                }
            }
        }
        
        if(tempList != null)
        {
            foreach(GameObject item in tempList)
            {
                if(item)
                {
                    item.transform.SetParent(tempPickUp.GetComponent<PickUpScript>().ItemsParent.transform, false);
                    item.GetComponent<InvItem>().Collectable = true;
                }
            }
        }

        tempPickUp.GetComponent<PickUpScript>().Items = tempList;
        if(gameObject.GetComponent<PlayerController>() != null)
        {
            tempPickUp.GetComponent<PickUpScript>().LifeTime = 900;        //Extrazeit für den Spieler seine Items aufzusammeln
            tempPickUp.GetComponent<PickUpScript>().Name = "ItemPile from Player";
        }
        else
        {
            tempPickUp.GetComponent<PickUpScript>().LifeTime = 300;
            if(gameObject.GetComponent<Stats>())
            {
                tempPickUp.GetComponent<PickUpScript>().Name = "ItemPile from " + gameObject.GetComponent<Stats>().Name;
            }
            else
            {
                tempPickUp.GetComponent<PickUpScript>().Name = "ItemPile";
            }
        }
        Destroy(gameObject, 5);
    }

    public GameObject GetItem(int listNumber, int itemNumber)
    {
        if(listNumber == 1)
        {
            Debug.Log("Found item: " + inventory[itemNumber].name);
            return inventory[itemNumber];
    
        }
        else if(listNumber == 3)
        {
            return PickUpList[itemNumber];
        }
        else
        {
            if(itemNumber <= 10)
            {
                return EquipList[itemNumber - 1];
            }
            else
            {
                return null;
            }
        }
    }

    public List<int> GetItems(int list, int itemComponent = 0, int type = 0)
    {
        List<int> result = new();
        List<GameObject> tempList;
        
        if(list == 1)
        {
            tempList = inventory;
        }
        else if(list == 3)
        {
            tempList = PickUpList;
        }
        else
        {
            return result;
        }

        if(itemComponent == 0)                      //all
        {
            for(int i = 0; i < tempList.Count; i++)
            {
                result.Add(i);
            }
        }
        else if(itemComponent == 1)                 //Weapon
        {
            if(type == 0)       //all
            {
                for(int i = 0; i < tempList.Count; i++)
                {
                    if(tempList[i].GetComponent<WeaponStats>())
                    {
                        result.Add(i);
                    }
                }
            }
            else if(type == 1)      //one handed melee weapon
            {
                for(int i = 0; i < tempList.Count; i++)
                {
                    if(tempList[i].GetComponent<WeaponStats>())
                    {
                        if(tempList[i].GetComponent<WeaponStats>().Melee && !tempList[i].GetComponent<WeaponStats>().TwoHanded)
                        {
                            result.Add(i);
                        }
                    }
                }
            }
            else if(type == 2)      //two handed melee weapon
            {
                for(int i = 0; i < tempList.Count; i++)
                {
                    if(tempList[i].GetComponent<WeaponStats>())
                    {
                        if(tempList[i].GetComponent<WeaponStats>().Melee && tempList[i].GetComponent<WeaponStats>().TwoHanded)
                        {
                            result.Add(i);
                        }
                    }
                }
            }
            else if(type == 3)      //one handed range weapon
            {
                for(int i = 0; i < tempList.Count; i++)
                {
                    if(tempList[i].GetComponent<WeaponStats>())
                    {
                        if(!tempList[i].GetComponent<WeaponStats>().Melee && !tempList[i].GetComponent<WeaponStats>().TwoHanded)
                        {
                            result.Add(i);
                        }
                    }
                }
            }
            else if(type == 4)      //two handed range weapon
            {
                for(int i = 0; i < tempList.Count; i++)
                {
                    if(tempList[i].GetComponent<WeaponStats>())
                    {
                        if(!tempList[i].GetComponent<WeaponStats>().Melee && tempList[i].GetComponent<WeaponStats>().TwoHanded)
                        {
                            result.Add(i);
                        }
                    }
                }
            }
        }
        else if(itemComponent == 2)         //Armour
        {
            if(type == 0)
            {
                for(int i = 0; i < tempList.Count; i++)
                {
                    if(tempList[i].GetComponent<ArmorStats>())
                    {
                        result.Add(i);
                    }
                }
            }
            else if(type == 1)              //shield
            {
                for(int i = 0; i < tempList.Count; i++)
                {
                    if(tempList[i].GetComponent<ArmorStats>())
                    {
                        if(tempList[i].GetComponent<ArmorStats>().Shield)
                        {
                            result.Add(i);
                        }
                    }
                }
            }
            else if(type == 2)
            {
                for(int i = 0; i < tempList.Count; i++)
                {
                    if(tempList[i].GetComponent<ArmorStats>())
                    {
                        if(!tempList[i].GetComponent<ArmorStats>().Shield && tempList[i].GetComponent<ArmorStats>().ArmorSlot == 1)
                        {
                            result.Add(i);
                        }
                    }
                }
            }
            else if(type == 3)
            {
                for(int i = 0; i < tempList.Count; i++)
                {
                    if(tempList[i].GetComponent<ArmorStats>())
                    {
                        if(!tempList[i].GetComponent<ArmorStats>().Shield && tempList[i].GetComponent<ArmorStats>().ArmorSlot == 2)
                        {
                            result.Add(i);
                        }
                    }
                }
            }
            else if(type == 4)
            {
                for(int i = 0; i < tempList.Count; i++)
                {
                    if(tempList[i].GetComponent<ArmorStats>())
                    {
                        if(!tempList[i].GetComponent<ArmorStats>().Shield && tempList[i].GetComponent<ArmorStats>().ArmorSlot == 3)
                        {
                            result.Add(i);
                        }
                    }
                }
            }
            else if(type == 5)
            {
                for(int i = 0; i < tempList.Count; i++)
                {
                    if(tempList[i].GetComponent<ArmorStats>())
                    {
                        if(!tempList[i].GetComponent<ArmorStats>().Shield && tempList[i].GetComponent<ArmorStats>().ArmorSlot == 4)
                        {
                            result.Add(i);
                        }
                    }
                }
            }
        }
        else if(itemComponent == 3)         //magical Accesories
        {
            if(type == 0)
            {
                for(int i = 0; i < tempList.Count; i++)
                {
                    if(tempList[i].GetComponent<MagicStats>())
                    {
                        result.Add(i);
                    }
                }
            }
            else if(type == 1)              //ring
            {
                for(int i = 0; i < tempList.Count; i++)
                {
                    if(tempList[i].GetComponent<MagicStats>())
                    {
                        if(tempList[i].GetComponent<MagicStats>().Ring)
                        {
                            result.Add(i);
                        }
                    }
                }
            }
            else if(type == 2)              //amulett
            {
                for(int i = 0; i < tempList.Count; i++)
                {
                    if(tempList[i].GetComponent<MagicStats>())
                    {
                        if(!tempList[i].GetComponent<MagicStats>().Ring)
                        {
                            result.Add(i);
                        }
                    }
                }
            }
        }
        else if(itemComponent == 4)         //Projectiles
        {
            if(type == 0)
            {
                for(int i = 0; i < tempList.Count; i++)
                {
                    if(tempList[i].GetComponent<Arrow>())
                    {
                        result.Add(i);
                    }
                }
            }
            else
            {
                for(int i = 0; i < tempList.Count; i++)
                {
                    if(tempList[i].GetComponent<Arrow>())
                    {
                        if(tempList[i].GetComponent<Arrow>().ArrowType == type - 1)
                        {
                            result.Add(i);
                        }
                    }
                }
            }
        }
        else if(itemComponent == 5)     //Consumables
        {
            if(type == 0)
            {
                for(int i = 0; i < tempList.Count; i++)
                {
                    if(tempList[i].GetComponent<FoodStats>())
                    {
                        result.Add(i);
                    }
                }
            }
            else if(type == 1)          //Food
            {
                for(int i = 0; i < tempList.Count; i++)
                {
                    if(tempList[i].GetComponent<FoodStats>())
                    {
                        if(!tempList[i].GetComponent<FoodStats>().potion)
                        {
                            result.Add(i);
                        }
                    }
                }
            }
            else if(type == 2)          //potions
            {
                for(int i = 0; i < tempList.Count; i++)
                {
                    if(tempList[i].GetComponent<FoodStats>())
                    {
                        if(tempList[i].GetComponent<FoodStats>().potion)
                        {
                            result.Add(i);
                        }
                    }
                }
            }
        }
        else if(itemComponent == 6)     //other Items
        {
            for(int i = 0; i < tempList.Count; i++)
            {
                GameObject j = tempList[i];
                if(!j)
                {
                    continue;
                }
                
                if(j.GetComponent<MoneyStats>())
                {
                    result.Add(i);
                }
            }
        }
        return result;
    }

    public void SetEquipParents()
    {
        if(EquipList[0] != null)
        {
            EquipList[0].transform.SetParent(RightParent.transform, false);
            EquipList[0].transform.localPosition = new Vector3(0, 0, 0);
            EquipList[0].transform.localEulerAngles = new Vector3(0, 0, 0);
            Debug.Log("Set Parent of " + EquipList[0].name + " to " + RightParent.name);
        }
        if(EquipList[1] != null)
        {
            EquipList[1].transform.SetParent(LeftParent.transform, false);
            EquipList[1].transform.localPosition = new Vector3(0, 0, 0);
            EquipList[1].transform.localEulerAngles = new Vector3(0, 0, 0);
            Debug.Log("Set Parent of " + EquipList[1].name + " to " + LeftParent.name);
        }
        if(EquipList[2] != null)
        {
            EquipList[2].transform.SetParent(MiddleParent.transform, false);
            EquipList[2].transform.localPosition = new Vector3(0, 0, 0);
            EquipList[2].transform.localEulerAngles = new Vector3(0, 0, 0);
            Debug.Log("Set Parent of " + EquipList[2].name + " to " + MiddleParent.name);
        }
        
        if(EquipList[3] != null)
        {
            EquipList[3].transform.SetParent(HeadParent.transform, false);
            EquipList[3].transform.localPosition = new Vector3(0, 0, 0);
            EquipList[3].transform.localEulerAngles = new Vector3(0, 0, 0);
        }
        if(EquipList[4] != null)
        {
            EquipList[4].transform.SetParent(ChestParent.transform, false);
            EquipList[4].transform.localPosition = new Vector3(0, 0, 0);
            EquipList[4].transform.localEulerAngles = new Vector3(0, 0, 0);
        }
        if(EquipList[5] != null)
        {
            EquipList[5].transform.SetParent(LegsParent.transform, false);
            EquipList[5].transform.localPosition = new Vector3(0, 0, 0);
            EquipList[5].transform.localEulerAngles = new Vector3(0, 0, 0);
        }
        if(EquipList[6] != null)
        {
            EquipList[6].transform.SetParent(ShoesParent.transform, false);
            EquipList[6].transform.localPosition = new Vector3(0, 0, 0);
            EquipList[6].transform.localEulerAngles = new Vector3(0, 0, 0);
        }

        if(EquipList[7] != null)
        {
            EquipList[7].transform.SetParent(MLeftParent.transform, false);
            EquipList[7].transform.localPosition = new Vector3(0, 0, 0);
            EquipList[7].transform.localEulerAngles = new Vector3(0, 0, 0);
        }
        if(EquipList[8] != null)
        {
            EquipList[8].transform.SetParent(MRightParent.transform, false);
            EquipList[8].transform.localPosition = new Vector3(0, 0, 0);
            EquipList[8].transform.localEulerAngles = new Vector3(0, 0, 0);
        }
        if(EquipList[9] != null)
        {
            EquipList[9].transform.SetParent(MMiddleParent.transform, false);
            EquipList[9].transform.localPosition = new Vector3(0, 0, 0);
            EquipList[9].transform.localEulerAngles = new Vector3(0, 0, 0);
        }
    }

    public void RemoveItem(GameObject item)
    {
        for(int i = 0; i < inventory.Count; i++)
        {
            if(inventory[i] == item)
            {
                inventory.Remove(item);
                return;
            }
        }
        for(int i = 0; i < PickUpList.Count; i++)
        {
            if(PickUpList[i] == item)
            {
                PickUpList.Remove(item);
                return;
            }
        }
        for(int i = 0; i < EquipList.Length; i++)
        {
            if(EquipList[i] == item)
            {
                EquipList[i] = null;
                return;
            }
        }
    }
}