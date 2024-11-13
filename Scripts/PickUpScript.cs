using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpScript : MonoBehaviour
{
    public int PrefabNumber;
    public bool Chest;      //wird nicht gelöscht, wenn es leer ist
    public List<GameObject> Items;
    public GameObject ItemsParent;
    
    private bool hitted;
    private GameObject player;
    public float LifeTime = 10;
    public string Name;
    
    public static PickUpScript CreatePickUp(Transform PickUpPrefab, Vector3 position, Quaternion rotation, List<GameObject> ItemList, string name, float lifeTime)
    {
        PickUpScript pickUp = Instantiate(PickUpPrefab, position, rotation).GetComponent<PickUpScript>();
        pickUp.Items = ItemList;
        foreach(GameObject item in pickUp.Items)
        {
            item.transform.SetParent(pickUp.ItemsParent.transform, false);
        }
        pickUp.Name = name;
        pickUp.LifeTime = lifeTime;

        return pickUp;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!Chest)
        {
            if(LifeTime <= 0)
            {
                Destroy(gameObject);
            }
            
            if(Items.Count == 0)        //no Items, so delete
            {
                if(Name == "")
                {
                    return;
                }
                
                if(player != null && hitted == true)        //von den PickUplists löschen
                {
                    PlayerController.playerContr.PickUpLists.Remove(gameObject);
                    Destroy(gameObject);
                    Debug.Log("Dissolving " + Name + " beacuase it has no Items in it!");
                }
                else
                {
                    Destroy(gameObject);
                    Debug.Log("Dissolving " + Name + " beacuase it has no Items in it!");
                }
            }
            else
            {
                LifeTime -= Time.deltaTime;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        GameObject HitObject = other.gameObject;
        
        while(HitObject != null)
        {
            if(HitObject.GetComponent<PickUpScript>() != null)      //summary of multiple PickUpLists
            {
                if(HitObject.GetComponent<PickUpScript>().Chest)        //chests arent summarized
                {
                    return;
                }
                if(LifeTime > HitObject.GetComponent<PickUpScript>().LifeTime)                  //old in young PickUpList
                {
                    foreach(GameObject GO in HitObject.GetComponent<PickUpScript>().Items)
                    {
                        Items.Add(GO);
                        GO.transform.SetParent(ItemsParent.transform, false);
                    }
                    Destroy(HitObject);
                    return;
                }
            }
            else if(HitObject.name == "Player" && hitted == false)
            {
                PlayerController.playerContr.PickUpLists.Add(this.gameObject);
                player = HitObject;
                hitted = true;
                return;
            }
            if(HitObject.transform.parent)
            {
                HitObject = HitObject.transform.parent.gameObject;
            }
            else
            {
                break;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        GameObject HitObject = other.gameObject;
        
        while(HitObject != null)
        {
            if(HitObject.name == "Player" && hitted == true)
            {
                PlayerController.playerContr.PickUpLists.Remove(this.gameObject);
                player = null;
                hitted = false;
                return;
            }
            else if(HitObject.transform.parent)
            {
                HitObject = HitObject.transform.parent.gameObject;
            }
            else
            {
                return;
            }
        }
    }
}
