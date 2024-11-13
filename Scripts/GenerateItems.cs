using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateItems : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if(loaded && !generated)
        {
            prepare();
        }
    }

    public bool loaded;
    private bool generated = false;
    Stats stats;
    public Inventory inventory;
    public PickUpScript pickUpScript;
    public GameObject[] Items;
    public float[] ItemRate;
    public int[] ItemMinAmount;
    public int[] ItemMaxAmount;
    public GameObject[] Weapon;
    public float[] WeaponRate;
    public GameObject[] Equipment;
    public float[] EquipmentRate;

    [Header("Drops")]
    public GameObject[] Drops;
    public float[] DropRate;
    public int[] DropMinAmount;
    public int[] DropMaxAmount;

    // Update is called once per frame
    void Update()
    {
        if(loaded && !generated)
        {
            prepare();
        }
    }

    public List<GameObject> GetDrops()
    {
        List<GameObject> result = new();
        GameObject tempItem;
        
        if(Drops.Length > 0)
        {
            for(int i = 0; i < Drops.Length; i++)
            {
                if(i < DropRate.Length)
                {
                    if(Random.Range(0.00f, 1.00f) <= ItemRate[i])
                    {
                        tempItem = Instantiate(Drops[i]);
                        result.Add(tempItem);
                        if(tempItem.GetComponent<InvItem>() && DropMinAmount.Length > i && DropMaxAmount.Length > i)
                        {
                            tempItem.GetComponent<InvItem>().Count = Random.Range(DropMinAmount[i], DropMaxAmount[i] + 1);
                        }
                    }
                }
                else
                {
                    tempItem = Instantiate(Drops[i]);
                    result.Add(tempItem);
                    if(tempItem.GetComponent<InvItem>() && DropMinAmount.Length > i && DropMaxAmount.Length > i)
                    {
                        tempItem.GetComponent<InvItem>().Count = Random.Range(DropMinAmount[i], DropMaxAmount[i] + 1);
                    }
                }
            }
        }

        return result;
    }

    void prepare()
    {
        if(!stats)
        {
            TryGetComponent<Stats>(out stats);
        }
        if(!inventory)
        {
            TryGetComponent<Inventory>(out inventory);
        }
        if(!pickUpScript)
        {
            TryGetComponent<PickUpScript>(out pickUpScript);
        }
        
        List<GameObject> tempItems = new();
        Transform InvParent;
        if(inventory)
        {
            InvParent = inventory.InvParent.transform;
        }
        else if(pickUpScript)
        {
            InvParent = pickUpScript.ItemsParent.transform;
        }
        else
        {
            return;
        }
        
        GameObject tempItem;
        
        if(Weapon.Length > 0)
        {
            for(int i = 0; i < Weapon.Length; i++)
            {
                if(i < WeaponRate.Length)
                {
                    if(Random.Range(0.00f, 1.00f) <= WeaponRate[i])
                    {
                        tempItem = Instantiate(Weapon[i]);
                        tempItem.transform.SetParent(InvParent, false);
                        tempItems.Add(tempItem);
                    }
                }
                else
                {
                    tempItem = Instantiate(Weapon[i]);
                    tempItem.transform.SetParent(InvParent, false);
                    tempItems.Add(tempItem);
                }
            }

            if(inventory)
            {
                foreach(GameObject item in tempItems)
                {
                    inventory.inventory.Add(item);
                }
                tempItems = new();
            }

            NPCharacter parent;
            if(TryGetComponent<NPCharacter>(out parent))
            {
                parent.ChangeWeapon();
            }
        }

        if(Items.Length > 0)
        {
            for(int i = 0; i < Items.Length; i++)
            {
                if(i < ItemRate.Length)
                {
                    if(Random.Range(0.00f, 1.00f) <= ItemRate[i])
                    {
                        tempItem = Instantiate(Items[i]);
                        tempItem.transform.SetParent(InvParent, false);
                        tempItems.Add(tempItem);
                        if(tempItem.GetComponent<InvItem>() && ItemMinAmount.Length > i && ItemMaxAmount.Length > i)
                        {
                            tempItem.GetComponent<InvItem>().Count = Random.Range(ItemMinAmount[i], ItemMaxAmount[i] + 1);
                        }
                    }
                }
                else
                {
                    tempItem = Instantiate(Items[i]);
                    tempItem.transform.SetParent(InvParent, false);
                    tempItems.Add(tempItem);
                    if(tempItem.GetComponent<InvItem>() && ItemMinAmount.Length > i && ItemMaxAmount.Length > i)
                    {
                        tempItem.GetComponent<InvItem>().Count = Random.Range(ItemMinAmount[i], ItemMaxAmount[i] + 1);
                    }
                }
            }

            if(inventory)
            {
                foreach(GameObject item in tempItems)
                {
                    inventory.inventory.Add(item);
                }
                tempItems = new();
            }
        }

        if(Equipment.Length > 0)
        {
            for(int i = 0; i < Equipment.Length; i++)
            {
                if(i < EquipmentRate.Length)
                {
                    if(Random.Range(0.00f, 1.00f) <= EquipmentRate[i])
                    {
                        tempItem = Instantiate(Equipment[i]);
                        tempItem.transform.SetParent(InvParent, false);
                        tempItems.Add(tempItem);
                    }
                }
                else
                {
                    tempItem = Instantiate(Equipment[i]);
                    tempItem.transform.SetParent(InvParent, false);
                    tempItems.Add(tempItem);
                }
            }

            if(inventory)
            {
                foreach(GameObject item in tempItems)
                {
                    inventory.inventory.Add(item);
                }
                tempItems = new();
            }
        }

        if(pickUpScript)
        {
            pickUpScript.Items = tempItems;
        }

        generated = true;
    }
}
