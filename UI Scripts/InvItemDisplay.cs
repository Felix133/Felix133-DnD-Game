using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InvItemDisplay : MonoBehaviour
{
    public GameObject DisplayObject;
    public int List;
    public int Position;

    public TextMeshProUGUI DisplayTextName;         //displays the name of the item
    public TextMeshProUGUI DisplayTextCount;        //displays the amount of this item in the inventory
    public Image DisplayImage;                      //displays a representative image of the item
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Set(GameObject Object, int list, int Pos)
    {
        List = list;
        Position = Pos;
        
        if(Object != null)
        {
            DisplayObject = Object;
            if(DisplayObject.GetComponent<InvItem>())
            {
                InvItem temp = DisplayObject.GetComponent<InvItem>();
                DisplayTextName.text = temp.Name;
                DisplayImage.sprite = temp.Image;
                if(temp.Stackable)
                {
                    DisplayTextCount.text = "" + temp.Count;
                }
                else            //isnt stackable so shouldnt have a count
                {
                    DisplayTextCount.text = "";
                }
            }
            else
            {
                DisplayTextName.text = "Error! No InvItem detected";
            }
        }
        else            //Item Placeholder
        {
            DisplayTextName.text = "   - - - - -";
            DisplayTextCount.gameObject.SetActive(false);
            DisplayImage.gameObject.SetActive(false);
        }
    }

    public void Clicked()
    {
        PlayerController.playerContr.Select(List, Position);
    }
}
