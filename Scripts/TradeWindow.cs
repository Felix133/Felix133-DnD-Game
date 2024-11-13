using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TradeWindow : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Items.Count == 0)
        {
            return;
        }
        else if(Items.Count == 1)
        {
            if(Items[0].GetComponent<InvItem>().Stackable)
            {
                Price = (int) Amount.value * Items[0].GetComponent<InvItem>().Worth;
            }
            else
            {
                Price = Items[0].GetComponent<InvItem>().Worth;
            }
        }
        else
        {
            Price = 0;
            foreach(GameObject item in Items)
            {
                if(item.GetComponent<InvItem>().Stackable)
                {
                    Price += (int) Amount.value * item.GetComponent<InvItem>().Worth;
                }
                else
                {
                    Price += item.GetComponent<InvItem>().Worth;
                }
            }
        }

        PriceText.text = Amount.value + "/" + Amount.maxValue  + " * " + Items[0].GetComponent<InvItem>().Worth + "$ --> " +   + Price + "$";
    }

    public List<GameObject> Items;
    public Slider Amount;
    public TextMeshProUGUI PriceText;
    public int Price;
    public int selectedList;
    public int selectedPos;

    public void Set(List<GameObject> items, int selList, int selPos)
    {
        selectedList = selList;
        selectedPos = selPos;
        Items = items;
        
        if(Items.Count == 0)
        {
            return;
        }
        else if(Items.Count == 1)
        {
            if(Items[0].GetComponent<InvItem>().Stackable)
            {
                Amount.minValue = 1;
                Amount.maxValue = Items[0].GetComponent<InvItem>().Count;
                Amount.value = Items[0].GetComponent<InvItem>().Count;
            }
            else
            {
                Amount.minValue = 1;
                Amount.maxValue = 1;
                Amount.value = 1;
            }
        }
    }
}
