using UnityEngine;

public class InvItem : MonoBehaviour
{
    public Sprite Image;
    public string Name;
    public string DefaultName;
    public string Description;
    public int Count;
    public bool Stackable;              //if the Item allows Stacks, f.ex. Arrows but not Swords
    public bool Collectable;
    public int Worth;
    public bool NPCUsable = true;
    public int PrefabNumber;
    public bool DestroyOnDeath;     //doesnt drop when the Character holding it dies
    public bool Temporary;          //if the weapon is only temporary and can be deleted
    
    // Start is called before the first frame update
    void Start()
    {
        ReloadName();
    }

    public void ReloadName()
    {
        if(this.gameObject.GetComponent<WeaponStats>())         //item is a weapon
        {
            if(this.gameObject.GetComponent<Damage>().Enchantment == 0)
            {
                Name = DefaultName;
            }
            else                        //weapon is enchanted
            {
                Name = DefaultName + " [" + this.gameObject.GetComponent<Damage>().Enchantment.ToString() + "]";
            }
        }
        else if(this.gameObject.GetComponent<Arrow>())         //item is an arrow
        {
            if(this.gameObject.GetComponent<Damage>().Enchantment == 0)
            {
                Name = DefaultName;
            }
            else                        //arrow is enchanted
            {
                Name = DefaultName + " [" + this.gameObject.GetComponent<Damage>().Enchantment.ToString() + "]";
            }
        }
        else if(this.gameObject.GetComponent<ArmorStats>())         //item is an armorpiece
        {
            if(this.gameObject.GetComponent<ArmorStats>().Enchantment == 0)
            {
                Name = DefaultName;
            }
            else                        //armor is enchanted
            {
                Name = DefaultName + " [" + this.gameObject.GetComponent<ArmorStats>().Enchantment.ToString() + "]";
            }
        }
        else                    //Item is something else
        {
            Name = DefaultName;
        }
    }
}
