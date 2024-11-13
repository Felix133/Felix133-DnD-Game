using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEditor;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public static PlayerController playerContr;
    
    public GameObject Canvas;
    public Rigidbody rbPlayer;
    public Animator animator;
    public Stats playerStats;
    public GameObject BareHand;

    [Header("Movement Stats")]
    public LayerMask jumpLayerMask;         //which Layers the Player can jump on
    public float jumpHeight = 5.0f;
    private float jumpRayLength = 1.05f;
    public bool grounded = false;
    
    public float Speed = 3.0f;
    public float SprintSpeed = 7.0f;
    private Vector3 playerVelocity;
    private float xMove;
    private float zMove;

    [Header("Attack")]
    public GameObject Target;                   //targeted Object
    public LayerMask layerMaskSelect;           //Layers of the selectable Targets

    [Header("Inventory Display")]
    public GameObject StatusEffectDisplay;
    public GameObject ReloadBar;                //displays (f.ex. drawing progress of bow)
    public GameObject InvDisplayParent;
    public GameObject InventoryDisplay;         //Overall Display Containing the Inventory, PickUp, Equipment and Description Display
    public GameObject PickUpDisplay;
    public GameObject EquipmentDisplay;
    public GameObject DescriptionDisplay;
    public GameObject InvItemDisplay;           //Prefab
    public GameObject PickUpItemsPf;            //Prefab
    public bool InvActive = true;
    public bool PickUpActive;
    public bool DescriptionActive;
    public bool EquipmentActive = true;
    public GameObject InvButton;
    public GameObject EquipButton;
    public GameObject PickUpButton;
    public GameObject DescButton;
    public TextMeshProUGUI ActionDisplay;
    public TextMeshProUGUI LookInfoDisplay;
    public GameObject CoinPf;

    [Header("Inventory")]
    public List<GameObject> PickUpLists;
    public GameObject tempSelectedPickUpGO;
    public int selectedList;
    public int selectedPosition;
    public float reachDistance;
    public LayerMask reachLayerMask;
    public Inventory inventory;
    public TradeWindow tradeWindow;

    [Header("Items")]
    public GameObject MiddleParent;
    public GameObject RightHandParent;
    public GameObject LeftHandParent;
    public GameObject RingLParent;
    public GameObject RingRParent;
    public GameObject AmuletParent;

    [Header("Runtime")]
    public List<int> tempInventory;
    public List<int> tempPickUp;

    void Awake()
    {
        if(!playerContr)
        {
            playerContr = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        if(!animator)
        {
            animator = GetComponent<Animator>();
            if(!animator)
            {
                Debug.LogWarning("Couldnt find Animator!");
            }
        }
        if(!inventory)
        {
            inventory = gameObject.GetComponent<Inventory>();
        }
    }

    void Update()
    {
        if(playerStats.Hitpoints <= 0)
        {
            return;
        }

        if(MainMenu.mainMenu.Options.activeSelf)
        {
            return;
        }

        if(InvDisplayParent.activeSelf)
        {
            if(Input.GetKeyDown(MainMenu.mainMenu.Inv))       //Inventory will be closed
            {
                StatusEffectDisplay.SetActive(true);
                Cursor.lockState = CursorLockMode.Locked;
                tradeWindow.gameObject.SetActive(false);
                
                InventoryDisplay.GetComponent<InventoryDisplay>().Reset();
                PickUpDisplay.GetComponent<InventoryDisplay>().Reset();
                if(inventory.PickUpList.Count > 0)        //wenn eine PickUpListe geöffnet wurde
                {
                    if(tempSelectedPickUpGO != null)        //es gibt schon eine offene PickUpList
                    {
                        if(tempSelectedPickUpGO.GetComponent<NPCharacter>())
                        {
                            foreach(GameObject go in inventory.PickUpList)
                            {
                                go.transform.SetParent(tempSelectedPickUpGO.GetComponent<Inventory>().InvParent.transform, false);
                            }
                            tempSelectedPickUpGO.GetComponent<Inventory>().inventory = inventory.PickUpList;
                        }
                        else
                        {
                            foreach(GameObject go in inventory.PickUpList)
                            {
                                go.transform.SetParent(tempSelectedPickUpGO.GetComponent<PickUpScript>().ItemsParent.transform, false);
                            }
                            tempSelectedPickUpGO.GetComponent<PickUpScript>().Items = inventory.PickUpList;
                        }
                        tempSelectedPickUpGO = null;
                    }
                    else           //es gibt noch keine offene PickUpList
                    {
                        tempSelectedPickUpGO = Instantiate(PickUpItemsPf, gameObject.transform.position, gameObject.transform.rotation);
                        tempSelectedPickUpGO.transform.position = new Vector3(tempSelectedPickUpGO.transform.position.x, tempSelectedPickUpGO.transform.position.y -1, tempSelectedPickUpGO.transform.position.z);
                        foreach(GameObject go in inventory.PickUpList)
                        {
                            go.transform.SetParent(tempSelectedPickUpGO.GetComponent<PickUpScript>().ItemsParent.transform, false);
                        }
                        tempSelectedPickUpGO.GetComponent<PickUpScript>().Items = inventory.PickUpList;
                        tempSelectedPickUpGO.GetComponent<PickUpScript>().Name = "ItemPile from Player";
                        tempSelectedPickUpGO = null;
                    }
                }
                else            //die PickUpListe wurde geleert
                {
                    if(tempSelectedPickUpGO != null)
                    {
                        if(tempSelectedPickUpGO.GetComponent<NPCharacter>())
                        {
                            tempSelectedPickUpGO.GetComponent<NPCharacter>().inventory.inventory = new List<GameObject>();
                        }
                        else
                        {
                            tempSelectedPickUpGO.GetComponent<PickUpScript>().Items = new List<GameObject>();
                        }
                        tempSelectedPickUpGO = null;
                    }
                    else
                    {
                        tempSelectedPickUpGO = null;
                    }
                }
                inventory.PickUpList = new List<GameObject>();
                unloadInv();
            }
            else if(Input.GetKeyDown(MainMenu.mainMenu.InvMove))          //Item will be thrown out of the Inv or picked up from another Inv
            {
                if(tradeWindow.gameObject.activeSelf)
                {
                    return;
                }
                
                if(selectedList == 1)
                {
                    if(!PickUpActive)
                    {
                        PickUpActive = true;
                        PickUpDisplay.SetActive(true);
                        PickUpButton.SetActive(false);
                    }

                    InvItem tempItem = inventory.GetItem(selectedList, selectedPosition).GetComponent<InvItem>();
                    if(tempItem)
                    {
                        if(tempSelectedPickUpGO.GetComponent<NPCharacter>())
                        {
                            if(tempSelectedPickUpGO.GetComponent<NPCharacter>().InvSelling)
                            {
                                tradeWindow.gameObject.SetActive(true);
                                List<GameObject> items = new()
                                {
                                    tempItem.gameObject
                                };
                                tradeWindow.Set(items, selectedList, selectedPosition);
                            }
                            
                        }
                        else            //normal ItemTransfer (not trading)
                        {
                            inventory.Move(selectedPosition, selectedList, 3);
                            Select( selectedList, selectedPosition);
                        }
                    }
                }
                else if(selectedList == 2)
                {
                    if(!InvActive)
                    {
                        InvActive = true;
                        InventoryDisplay.SetActive(true);
                        InvButton.SetActive(false);
                    }
                    inventory.PopSlot(selectedPosition, 3);
                }
                else if (selectedList == 3)
                {
                    if(!InvActive)
                    {
                        InvActive = true;
                        InventoryDisplay.SetActive(true);
                        InvButton.SetActive(false);
                    }

                    InvItem tempItem = inventory.GetItem(selectedList, selectedPosition).GetComponent<InvItem>();
                    if(tempItem)
                    {
                        if(tempSelectedPickUpGO.GetComponent<NPCharacter>())
                        {
                            if(tempSelectedPickUpGO.GetComponent<NPCharacter>().InvSelling)
                            {
                                tradeWindow.gameObject.SetActive(true);
                                List<GameObject> items = new()
                                {
                                    tempItem.gameObject
                                };
                                tradeWindow.Set(items, selectedList, selectedPosition);
                            }
                        }
                        else            //normal ItemTransfer (not trading)
                        {
                            inventory.Move(selectedPosition, selectedList, 1);
                            Select( selectedList, selectedPosition);
                        }
                    }
                }
                else            //all Items will be taken out of a PickUpScript (Chests or Bodies)
                {
                    if(PickUpActive)
                    {
                        if(!InvActive)
                        {
                            InvActive = true;
                            InventoryDisplay.SetActive(true);
                            InvButton.SetActive(false);
                        }

                        Inventory tempInv = tempSelectedPickUpGO.GetComponent<Inventory>();
                        List<GameObject> tempList = tempInv.inventory;
                        
                        if(tempSelectedPickUpGO.GetComponent<NPCharacter>())
                        {
                            if(tempSelectedPickUpGO.GetComponent<NPCharacter>().InvSelling)
                            {
                                tradeWindow.gameObject.SetActive(true);
                                List<GameObject> items = new();

                                foreach(GameObject item in tempList)
                                {
                                    if(!item.GetComponent<InvItem>())
                                    {
                                        tempInv.RemoveItem(item);
                                        tempList.Remove(item);
                                        Destroy(item);
                                    }
                                    else
                                    {
                                        items.Add(item);
                                    }
                                }

                                tradeWindow.Set(items, selectedList, selectedPosition);
                            }
                        }
                        else            //normal ItemTransfer (not trading)
                        {
                            foreach(GameObject item in tempList)
                            {
                                if(!item.GetComponent<InvItem>())
                                {
                                    tempInv.RemoveItem(item);
                                    tempList.Remove(item);
                                    Destroy(item);
                                }
                                else
                                {
                                    inventory.Move(selectedPosition, selectedList, 1);
                                    Select( selectedList, selectedPosition);
                                }
                            }
                            
                        }
                    }
                }
                unloadInv();
                loadInv();
            }
            else if(Input.GetKeyDown(MainMenu.mainMenu.InvInteract))
            {
                if(tradeWindow.gameObject.activeSelf)
                {
                    return;
                }
                
                GameObject temp = inventory.GetItem(selectedList, selectedPosition);
                if(temp)
                {
                    if(temp.GetComponent<FoodStats>() && temp.GetComponent<InvItem>().Count > 0)        //food will be consumed
                    {
                        playerStats.Consume(temp);
                        temp.GetComponent<InvItem>().Count -= 1;
                        if(temp.GetComponent<InvItem>().Count < 1)
                        {
                            inventory.RemoveItem(temp);
                            Destroy(temp, 0);
                        }
                        unloadInv();
                        loadInv();
                    }
                }
            }
        }
        else if(Input.GetKeyDown(MainMenu.mainMenu.Inv) || Input.GetKeyDown(MainMenu.mainMenu.InvInteract))       //Inventar wird aktiviert
        {
            bool tempInv = false;
            if(Input.GetKeyDown(MainMenu.mainMenu.Inv))
            {
                EquipmentActive = true;
                PickUpActive = false;
                tempInv = true;
            }
            else            //open another inventory (from chest/ item pile) or interact with Object
            {
                EquipmentActive = false;
                PickUpActive = true;
                Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                RaycastHit temp;
                GameObject hitGO;
                if(Physics.Raycast(ray, out temp, Mathf.Infinity, reachLayerMask))
                {
                    hitGO = temp.collider.gameObject;
                    while(hitGO != null)                //search for hittable Target
                    {
                        if(hitGO.GetComponent<InvItem>())     //found hittable Target
                        {
                            if((hitGO.transform.position - gameObject.transform.position).magnitude <= reachDistance)
                            {
                                if(hitGO.transform.parent)
                                {
                                    if(hitGO.transform.parent.gameObject.GetComponent<PickUpScript>())
                                    {
                                        tempSelectedPickUpGO = hitGO;
                                        tempInv = true;
                                    }
                                    else if(hitGO.transform.parent.parent)
                                    {
                                        if(hitGO.transform.parent.gameObject.GetComponent<PickUpScript>())
                                        {
                                            tempSelectedPickUpGO = hitGO;
                                            tempInv = true;
                                        }
                                        else if(hitGO.GetComponent<InvItem>().Collectable)
                                        {
                                            hitGO.transform.SetParent(inventory.InvParent.transform, false);
                                            inventory.inventory.Add(hitGO);
                                            hitGO.SetActive(false);
                                            tempInv = false;
                                        }
                                    }
                                    else if(hitGO.GetComponent<InvItem>().Collectable)
                                    {
                                        hitGO.transform.SetParent(inventory.InvParent.transform, false);
                                        inventory.inventory.Add(hitGO);
                                        hitGO.SetActive(false);
                                        tempInv = false;
                                    }
                                }
                                else if(hitGO.GetComponent<InvItem>().Collectable)              //Object is collectable
                                {
                                    hitGO.transform.SetParent(inventory.InvParent.transform, false);
                                    inventory.inventory.Add(hitGO);
                                    hitGO.SetActive(false);
                                    tempInv = false;
                                }
                            }
                            
                            break;
                        }
                        else if(hitGO.GetComponent<PickUpScript>())
                        {
                            if((hitGO.transform.position - gameObject.transform.position).magnitude <= reachDistance)
                            {
                                tempSelectedPickUpGO = hitGO;
                                tempInv = true;
                            }

                            break;
                        }
                        else if(hitGO.GetComponent<RestingStats>())
                        {
                            if((hitGO.transform.position - gameObject.transform.position).magnitude <= reachDistance)
                            {
                                playerStats.Use(hitGO);
                                MainMenu.mainMenu.timePassed += hitGO.GetComponent<RestingStats>().skippedTime;
                                MainMenu.mainMenu.timePassed += hitGO.GetComponent<RestingStats>().skippedRelativeTime * MainMenu.mainMenu.TimePerDay;
                                tempInv = false;
                            }

                            break;
                        }
                        else if(hitGO.GetComponent<NPCharacter>())
                        {
                            if((hitGO.transform.position - gameObject.transform.position).magnitude <= reachDistance)
                            {
                                NPCharacter tempPB = hitGO.GetComponent<NPCharacter>();
                                if(MainMenu.mainMenu.Relations[tempPB.Affiliation].Contains(0))
                                {
                                    break;
                                }

                                if(tempPB.personalEnemies.Contains(gameObject))
                                {
                                    break;
                                }

                                if(tempPB.InvSelling)
                                {
                                    tempSelectedPickUpGO = hitGO;
                                    tempInv = true;
                                }
                            }

                            break;
                        }
                        else if(hitGO.TryGetComponent<DoorStats>(out DoorStats doorStats))
                        {
                            if(doorStats.DestinationScene != SceneManager.GetActiveScene().buildIndex)
                            {
                                MainMenu.mainMenu.DestinationPos = doorStats.DestinationPos;
                                MainMenu.mainMenu.DestinationRot = doorStats.DestinationRot;
                                MainMenu.mainMenu.ChangeScenes(doorStats.DestinationScene);
                            }
                            else
                            {
                                MainMenu.mainMenu.Teleport(doorStats.DestinationPos, doorStats.DestinationRot);
                            }
                        }
                        else if(hitGO.transform.parent)     //can still search further in the hierarchy
                        {
                            hitGO = hitGO.transform.parent.gameObject;
                        }
                        else        //could not find hittable Target
                        {
                            tempInv = false;
                            break;
                        }
                    }
                }
            }
            
            if(tempInv)
            {
                Cursor.lockState = CursorLockMode.None;
                InvActive = true;
                DescriptionActive = true;
                
                selectedList = 0;
                selectedPosition = 0;
                DescriptionDisplay.GetComponent<ItemDescription>().DisplayItem(null, 0, 0);

                //tempSelectedPickUpGO = null;

                StatusEffectDisplay.SetActive(false);
                
                //foreach(GameObject tempGO in PickUpLists)       //das nächste Objekt mit einer PickUpList wird ausgewählt, um dieses zu öffnen
                //{
                //    if(tempSelectedPickUpGO == null)        //wenn noch kein Objekt ausgewählt wurde
                //    {
                //        tempSelectedPickUpGO = tempGO;
                //    }
                //    else if(Vector3.Distance(tempGO.transform.position, gameObject.transform.position) < Vector3.Distance(tempSelectedPickUpGO.transform.position, gameObject.transform.position))
                //    {                                           //wenn das Objekt näher am Spieler ist, als das bereits ausgewählte, wird es statdessen ausgewählt
                //        tempSelectedPickUpGO = tempGO;
                //    }
                //}

                if(tempSelectedPickUpGO != null)
                {
                    if(tempSelectedPickUpGO.GetComponent<PickUpScript>())
                    {
                        inventory.PickUpList = tempSelectedPickUpGO.GetComponent<PickUpScript>().Items;
                    }
                    else
                    {
                        inventory.PickUpList = tempSelectedPickUpGO.GetComponent<Inventory>().inventory;
                    }
                }
                else
                {
                    inventory.PickUpList = new List<GameObject>();
                }

                tempInventory = inventory.GetItems(1, InventoryDisplay.GetComponent<InventoryDisplay>().currentItemType, InventoryDisplay.GetComponent<InventoryDisplay>().currentType);
                tempPickUp = inventory.GetItems(3, PickUpDisplay.GetComponent<InventoryDisplay>().currentItemType, PickUpDisplay.GetComponent<InventoryDisplay>().currentType);
                loadInv();
            }
            
        }

        if(InvDisplayParent.activeSelf)     //can not do anything else while in Inventory
        {
            ActionDisplay.text = null;
            LookInfoDisplay.text = null;
            return;
        }
        else
        {
            if(tradeWindow.gameObject.activeSelf)
            {
                tradeWindow.gameObject.SetActive(false);
            }
            
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit temp;
            GameObject hitGO;
            if(Physics.Raycast(ray, out temp, Mathf.Infinity, reachLayerMask))
            {
                hitGO = temp.collider.gameObject;
                while(hitGO != null)                //search for hittable Target
                {
                    if(hitGO.GetComponent<InvItem>())     //found hittable Target
                    {
                        if((hitGO.transform.position - gameObject.transform.position).magnitude <= reachDistance)
                        {
                            if(hitGO.GetComponent<InvItem>().Collectable)             //Object is collectable
                            {
                                ActionDisplay.text = "[E]Collect";
                                LookInfoDisplay.text = hitGO.GetComponent<InvItem>().Name;
                            }
                        }
                        else
                        {
                            ActionDisplay.text = null;
                            LookInfoDisplay.text = null;
                        }
                        
                        break;
                    }
                    else if(hitGO.GetComponent<PickUpScript>())
                    {
                        if((hitGO.transform.position - gameObject.transform.position).magnitude <= reachDistance)
                        {
                            if(hitGO.GetComponent<PickUpScript>().Chest)             //Object is a Chest
                            {
                                ActionDisplay.text = "[E]Open";
                            }
                            else
                            {
                                ActionDisplay.text = "[E]Collect";
                            }
                            LookInfoDisplay.text = hitGO.GetComponent<PickUpScript>().Name;
                        }
                        else
                        {
                            ActionDisplay.text = null;
                            LookInfoDisplay.text = null;
                        }

                        break;
                    }
                    else if(hitGO.GetComponent<RestingStats>())
                    {
                        if((hitGO.transform.position - gameObject.transform.position).magnitude <= reachDistance)
                        {
                            ActionDisplay.text = "[E]Use";
                            LookInfoDisplay.text = hitGO.GetComponent<RestingStats>().Name;
                        }
                        else
                        {
                            ActionDisplay.text = null;
                            LookInfoDisplay.text = null;
                        }

                        break;
                    }
                    else if(hitGO.GetComponent<NPCharacter>())
                    {
                        if((hitGO.transform.position - gameObject.transform.position).magnitude <= reachDistance)
                        {
                            NPCharacter tempPB = hitGO.GetComponent<NPCharacter>();
                            
                            if(MainMenu.mainMenu.Relations[tempPB.Affiliation].Contains(0))
                            {
                                break;
                            }

                            if(tempPB.personalEnemies.Contains(gameObject))
                            {
                                break;
                            }

                            if(tempPB.InvSelling)
                            {
                                ActionDisplay.text = "[E]Trade";
                                LookInfoDisplay.text = hitGO.GetComponent<Stats>().Name;
                            }
                        }
                        else
                        {
                            ActionDisplay.text = null;
                            LookInfoDisplay.text = null;
                        }

                        break;
                    }
                    else if(hitGO.GetComponent<DoorStats>())
                    {
                        if((hitGO.transform.position - gameObject.transform.position).magnitude <= reachDistance)
                        {
                            ActionDisplay.text = "[E]Open";
                            LookInfoDisplay.text = hitGO.GetComponent<DoorStats>().DestinationName;
                        }
                        else
                        {
                            ActionDisplay.text = null;
                            LookInfoDisplay.text = null;
                        }

                        break;
                    }
                    else if(hitGO.transform.parent)     //can still search further in the hierarchy
                    {
                        hitGO = hitGO.transform.parent.gameObject;
                    }
                    else        //could not find hittable Target
                    {
                        ActionDisplay.text = null;
                        LookInfoDisplay.text = null;
                        break;
                    }
                }
            }
            else
            {
                ActionDisplay.text = null;
                LookInfoDisplay.text = null;
            }
            
        }

        RaycastHit hit;
        grounded = Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, jumpRayLength, jumpLayerMask);
        animator.SetBool("isGrounded", grounded);
        if(!grounded)
        {
            animator.ResetTrigger("Jump");
        }

        if(Input.GetKey(MainMenu.mainMenu.SelectTarget))
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            GameObject hitGO = null;
            if(Physics.Raycast(ray, out hit, Mathf.Infinity, layerMaskSelect))
            {
                hitGO = hit.collider.gameObject;
                while(hitGO != null)                //search for hittable Target
                {
                    if(hitGO.GetComponent<Stats>())     //found hittable Target
                    {
                        if(Target != hitGO)             //Target is a new one
                        {
                            if(Target != null)          //already had a Target
                            {
                                Target.GetComponent<Stats>().Display.Lowlight();
                            }
                            hitGO.GetComponent<Stats>().Display.Highlight();
                        }
                        
                        break;
                    }
                    else if(hitGO.transform.parent)     //can still search further in the hierarchy
                    {
                        hitGO = hitGO.transform.parent.gameObject;
                    }
                    else        //could not find hittable Target
                    {
                        hitGO = null;
                        if(Target != null)
                        {
                            Target.GetComponent<Stats>().Display.Lowlight();
                        }
                        break;
                    }
                }
            }
            else if(Target != null)     //could not hit anything
            {
                Target.GetComponent<Stats>().Display.Lowlight();
            }

            Target = hitGO;
            if(RightHandParent.transform.childCount > 0)            //keine beidhändige Waffe, sondern einhändige Waffe (immer Rechts)
            {
                RightHandParent.transform.GetChild(0).gameObject.GetComponent<WeaponStats>().Target = Target;
            }
            else if (MiddleParent.transform.childCount > 0)         //beidhändige Waffe
            {
                if(MiddleParent.transform.GetChild(0).gameObject.GetComponent<WeaponStats>().Melee)
                {
                    MiddleParent.transform.GetChild(0).gameObject.GetComponent<WeaponStats>().Target = Target;
                }
                else                                                //Fernkampfwaffe
                {
                    MiddleParent.transform.GetChild(0).gameObject.GetComponent<WeaponStats>().Target = Target;
                }
            }
        }

        if(Input.GetKey(MainMenu.mainMenu.Attack))
        {
            int BaseETW0 = playerStats.BaseETW0;
            int MeleeBoniETW0 = playerStats.MeleeBoniETW0;
            int RangeBoniETW0 = playerStats.RangeBoniETW0;
            int MeleeBoniSchaden = playerStats.MeleeBoniSchaden;
            int tempETW0 = playerStats.tempETW0;
            float damageMult = playerStats.damageMult;
            if(RightHandParent.transform.childCount > 0)            //keine beidhändige Waffe, sondern einhändige Waffe (immer Rechts)
            {
                if(RightHandParent.transform.GetChild(0).gameObject.GetComponent<WeaponStats>().Melee)
                {
                    RightHandParent.transform.GetChild(0).gameObject.GetComponent<WeaponStats>().Attack(BaseETW0, MeleeBoniETW0, RangeBoniETW0, MeleeBoniSchaden, tempETW0, damageMult);
                }
                else                                                //Fernkampfwaffe
                {
                    animator.SetFloat("Attacking Speed", RightHandParent.transform.GetChild(0).gameObject.GetComponent<WeaponStats>().AttackCooldown);
                    
                    animator.SetBool("isDrawing", true);
                }
                //animator.SetTrigger("AttackSingleHanded");
            }
            else if (MiddleParent.transform.childCount > 0)         //beidhändige Waffe
            {
                if(MiddleParent.transform.GetChild(0).gameObject.GetComponent<WeaponStats>().Melee)
                {
                    MiddleParent.transform.GetChild(0).gameObject.GetComponent<WeaponStats>().Attack(BaseETW0, MeleeBoniETW0, RangeBoniETW0, MeleeBoniSchaden, tempETW0, damageMult);
                }
                else                                                //Fernkampfwaffe
                {
                    animator.SetFloat("Attacking Speed", MiddleParent.transform.GetChild(0).gameObject.GetComponent<WeaponStats>().AttackCooldown);
                    
                    animator.SetBool("isDrawing", true);
                }
                
                //animator.SetTrigger("AttackTwoHanded");
            }
            else        //currently no weapon equipped, so bare handed
            {
                Transform t = RightHandParent.transform;
                if(BareHand != null)
                {
                    Instantiate(BareHand, t.position, t.rotation, t).GetComponent<WeaponStats>().Attack(BaseETW0, MeleeBoniETW0, RangeBoniETW0, MeleeBoniSchaden, tempETW0, damageMult);
                }
            }

            //LeftHandParent.transform.GetChild(0).gameObject.GetComponent<WeaponStats>().Attack(BaseETW0, MeleeBoniETW0, RangeBoniETW0, MeleeBoniSchaden);  //Waffe immer Rechts
        }
        else if(Input.GetKeyUp(MainMenu.mainMenu.Attack))         //is only true when the key is released in that frame
        {
            animator.SetBool("isDrawing", false);
            
            if(animator.GetCurrentAnimatorStateInfo(1).IsName("BowHold"))
            {
                int BaseETW0 = playerStats.BaseETW0;
                int MeleeBoniETW0 = playerStats.MeleeBoniETW0;
                int RangeBoniETW0 = playerStats.RangeBoniETW0;
                int MeleeBoniSchaden = playerStats.MeleeBoniSchaden;
                int tempETW0 = playerStats.tempETW0;
                float damageMult = playerStats.damageMult;
                
                MiddleParent.transform.GetChild(0).gameObject.GetComponent<WeaponStats>().Attack(BaseETW0, MeleeBoniETW0, RangeBoniETW0, MeleeBoniSchaden, tempETW0, damageMult);
            }
            else        //released the bow too early
            {
                if(MiddleParent.transform.childCount > 0)
                {
                    if(!MiddleParent.transform.GetChild(0).gameObject.GetComponent<WeaponStats>().Melee)
                    {
                        scriptDamagePopup.Create(MainMenu.mainMenu.Notifications.transform, "Released too early!", 1, new Vector3(0, 4f, 0));
                    }
                }
            }
        }

        if(MiddleParent.transform.childCount > 0)
        {
            if(MiddleParent.transform.GetChild(0).gameObject.GetComponent<WeaponStats>().Melee)
            {
                ReloadBar.transform.parent.gameObject.SetActive(false);
            }
            else                                                //Fernkampfwaffe
            {
                ReloadBar.transform.parent.gameObject.SetActive(true);
                if(animator.GetCurrentAnimatorStateInfo(1).IsName("BowHold"))
                {
                    ReloadBar.GetComponent<Image>().fillAmount = 1;
                }
                else if(animator.GetCurrentAnimatorStateInfo(1).IsName("BowDraw"))
                {
                    ReloadBar.GetComponent<Image>().fillAmount = (float) animator.GetCurrentAnimatorStateInfo(1).normalizedTime;
                }
                else
                {
                    ReloadBar.GetComponent<Image>().fillAmount = 0;
                }
            }
        }
        else
        {
            ReloadBar.transform.parent.gameObject.SetActive(false);
        }
        
        if(Input.GetKey(MainMenu.mainMenu.Jump))
        {
            if(grounded)
            {
                rbPlayer.velocity = transform.TransformDirection(new Vector3(rbPlayer.velocity.x, jumpHeight, rbPlayer.velocity.z));
                animator.SetTrigger("Jump");
            }
        }

        if(Input.GetKey(MainMenu.mainMenu.Forward))
        {
            if(Input.GetKey(MainMenu.mainMenu.Left))
            {
                xMove = -0.6540737726f;      //0.925
                zMove = 0.6540737726f;
                animator.SetBool("isFLRunning", true);
                animator.SetBool("isFRunning", false);
                animator.SetBool("isFRRunning", false);
                animator.SetBool("isSprinting", false);
            }
            else if(Input.GetKey(MainMenu.mainMenu.Right))
            {
                xMove = 0.6540737726f;       //0.925
                zMove = 0.6540737726f;
                animator.SetBool("isFRRunning", true);
                animator.SetBool("isFRunning", false);
                animator.SetBool("isFLRunning", false);
                animator.SetBool("isSprinting", false);
            }
            else
            {
                xMove = 0;                  //1
                zMove = 1f;
                if(Input.GetKey(MainMenu.mainMenu.Sprint))
                {
                    //rbPlayer.velocity = transform.TransformDirection(new Vector3(xMove * SprintSpeed, rbPlayer.velocity.y, zMove * SprintSpeed));
                    animator.SetBool("isFRunning", false);
                    animator.SetBool("isSprinting", true);
                }
                else
                {
                    //rbPlayer.velocity = transform.TransformDirection(new Vector3(xMove * Speed, rbPlayer.velocity.y, zMove * Speed));
                    animator.SetBool("isFRunning", true);
                    animator.SetBool("isSprinting", false);
                }
                animator.SetBool("isFLRunning", false);
                animator.SetBool("isFRRunning", false);
            }

            animator.SetBool("isBRunning", false);
            animator.SetBool("isBLRunning", false);
            animator.SetBool("isBRRunning", false);

            animator.SetBool("isRRunning", false);
            animator.SetBool("isLRunning", false);
        }
        else if(Input.GetKey(MainMenu.mainMenu.Backwards))
        {
            if(Input.GetKey(MainMenu.mainMenu.Left))
            {
                xMove = -0.5480077554f;      //0.775
                zMove = -0.5480077554f;
                animator.SetBool("isBRunning", false);
                animator.SetBool("isBLRunning", true);
                animator.SetBool("isBRRunning", false);
            }
            else if(Input.GetKey(MainMenu.mainMenu.Right))
            {
                xMove = 0.5480077554f;       //0.775
                zMove = -0.5480077554f;
                animator.SetBool("isBRunning", false);
                animator.SetBool("isBLRunning", false);
                animator.SetBool("isBRRunning", true);
            }
            else
            {
                xMove = 0;                  //0.7
                zMove = -0.7f;
                animator.SetBool("isBRunning", true);
                animator.SetBool("isBLRunning", false);
                animator.SetBool("isBRRunning", false);
            }

            animator.SetBool("isFRunning", false);
            animator.SetBool("isFLRunning", false);
            animator.SetBool("isFRRunning", false);
            animator.SetBool("isSprinting", false);

            animator.SetBool("isRRunning", false);
            animator.SetBool("isLRunning", false);
        }
        else
        {
            if(Input.GetKey(MainMenu.mainMenu.Left))
            {
                xMove = -0.85f;              //0.85
                zMove = 0;
                animator.SetBool("isRRunning", false);
                animator.SetBool("isLRunning", true);
            }
            else if(Input.GetKey(MainMenu.mainMenu.Right))
            {
                xMove = 0.85f;               //0.85
                zMove = 0;
                animator.SetBool("isRRunning", true);
                animator.SetBool("isLRunning", false);
            }
            else
            {
                xMove = 0;
                zMove = 0;
                animator.SetBool("isRRunning", false);
                animator.SetBool("isLRunning", false);
            }

            animator.SetBool("isBRunning", false);
            animator.SetBool("isBLRunning", false);
            animator.SetBool("isBRRunning", false);
            animator.SetBool("isSprinting", false);

            animator.SetBool("isFRunning", false);
            animator.SetBool("isFLRunning", false);
            animator.SetBool("isFRRunning", false);
        }

        xMove *= playerStats.speedMult;
        zMove *= playerStats.speedMult;
        if(Input.GetKey(MainMenu.mainMenu.Sprint) && Input.GetKey(MainMenu.mainMenu.Forward))
        {
            rbPlayer.velocity = transform.TransformDirection(new Vector3(xMove * SprintSpeed, rbPlayer.velocity.y, zMove * SprintSpeed));
        }
        else
        {
            rbPlayer.velocity = transform.TransformDirection(new Vector3(xMove * Speed, rbPlayer.velocity.y, zMove * Speed));
        }
        

        

        //Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        //controller.Move(move * Time.deltaTime * speed);

        //if (move != Vector3.zero)
        //{
            //gameObject.transform.forward = move;
        //}

        // Changes the height position of the player..
        //if (Input.GetButtonDown("Jump") && groundedPlayer)
        //{
            //playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        //}

        //playerVelocity.y += gravityValue * Time.deltaTime;
        //controller.Move(playerVelocity * Time.deltaTime);
    }

    public void unloadInv()
    {
        for(int i = 0; i < InventoryDisplay.transform.GetChild(1).GetChild(0).GetChild(0).childCount; i++)
        {
            GameObject.Destroy(InventoryDisplay.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(i).gameObject);
        }

        for(int i = 1; i < EquipmentDisplay.transform.childCount; i++) 
        {
            for(int j = 1; j < EquipmentDisplay.transform.GetChild(i).childCount; j++)
            {
                GameObject.Destroy(EquipmentDisplay.transform.GetChild(i).GetChild(j).gameObject);
            }
        }

        for(int i = 0; i < PickUpDisplay.transform.GetChild(1).GetChild(0).GetChild(0).childCount; i++)
        {
            GameObject.Destroy(PickUpDisplay.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(i).gameObject);
        }

        InvDisplayParent.SetActive(false);
    }

    public void loadInv()
    {
        Inventory Invtemp = inventory;
        GameObject temp;
        InvDisplayParent.SetActive(true);

        if(DescriptionActive)
        {
            DescriptionDisplay.SetActive(true);
            DescButton.SetActive(false);
        }
        else
        {
            DescriptionDisplay.SetActive(false);
            DescButton.SetActive(true);
        }
        
        if(InvActive)
        {
            InventoryDisplay.SetActive(true);
            InvButton.SetActive(false);
            
            tempInventory = inventory.GetItems(1, InventoryDisplay.GetComponent<InventoryDisplay>().currentItemType, InventoryDisplay.GetComponent<InventoryDisplay>().currentType);

            if(tempInventory.Count > 0)
            {
                for(int i = 0; i < tempInventory.Count; i ++)               //Inventar anzeigen
                {
                    if(tempInventory[i] < Invtemp.inventory.Count)
                    {
                        temp = Instantiate( InvItemDisplay, InventoryDisplay.transform.GetChild(1).GetChild(0).GetChild(0), false);
                        temp.GetComponent<InvItemDisplay>().Set( Invtemp.inventory[tempInventory[i]], 1, tempInventory[i]);
                        Invtemp.inventory[tempInventory[i]].SetActive(false);
                    }
                }
            }
            else
            {
                temp = Instantiate( InvItemDisplay, InventoryDisplay.transform.GetChild(1).GetChild(0).GetChild(0), false);
                temp.GetComponent<InvItemDisplay>().Set( null, 1, 0);
            }
        }
        else
        {
            InventoryDisplay.SetActive(false);
            InvButton.SetActive(true);
        }
            
        if(PickUpActive)
        {
            PickUpDisplay.SetActive(true);
            PickUpButton.SetActive(false);

            tempPickUp = inventory.GetItems(3, PickUpDisplay.GetComponent<InventoryDisplay>().currentItemType, PickUpDisplay.GetComponent<InventoryDisplay>().currentType);

            if(tempPickUp.Count > 0)        //wenn die Liste etwas beinhaltet
            {
                for(int i = 0; i < tempPickUp.Count; i ++)              //PickUpListe anzeigen
                {
                    if(tempPickUp[i] < Invtemp.PickUpList.Count)
                    {
                        temp = Instantiate( InvItemDisplay, PickUpDisplay.transform.GetChild(1).GetChild(0).GetChild(0), false);
                        temp.GetComponent<InvItemDisplay>().Set( Invtemp.PickUpList[tempPickUp[i]], 3, tempPickUp[i]);
                        Invtemp.PickUpList[tempPickUp[i]].SetActive(false);
                    }
                }
            }
            else        //wenn die Liste noch nichts beinhaltet
            {
                temp = Instantiate( InvItemDisplay, PickUpDisplay.transform.GetChild(1).GetChild(0).GetChild(0), false);
                temp.GetComponent<InvItemDisplay>().Set( null, 3, 0);
            }
        }
        else
        {
            PickUpDisplay.SetActive(false);
            PickUpButton.SetActive(true);
        }
            
        if(EquipmentActive)
        {
            EquipmentDisplay.SetActive(true);
            EquipButton.SetActive(false);

            for (int i = 0; i < Invtemp.EquipList.Length; i++) 
            {
                temp = Instantiate(InvItemDisplay, EquipmentDisplay.transform.GetChild(i + 1), false);
                temp.GetComponent<InvItemDisplay>().Set(Invtemp.EquipList[i], 2, i + 1);
            }
        }
        else
        {
            EquipmentDisplay.SetActive(false);
            EquipButton.SetActive(true);
        }
    }
    
    public void Select( int selList, int selPos)        //selList:selected List; selPos:selectedPosition
    {
        if(selectedList == selList && selectedPosition == selPos)       //zweimal das gleiche angeklickt
        {
            selectedList = 0;
            selectedPosition = 0;
            DescriptionDisplay.GetComponent<ItemDescription>().DisplayItem(null, 0, 0);
        }
        else if (selectedList == 0 && selectedPosition == 0)        //das erste Mal etwas angeklickt
        {
            selectedList = selList;
            selectedPosition = selPos;
            DescriptionDisplay.GetComponent<ItemDescription>().DisplayItem(inventory.GetItem(selList, selPos), selList, selPos);
        }
        else                //das zweite Mal etwas ausgewählt
        {
            if(selectedList == 1)
            {
                if(selList == 2)
                {
                    inventory.AddToSlot(selectedPosition, selPos, selectedList);
                }
                else if(selList == 3)
                {
                    if(tempSelectedPickUpGO.GetComponent<NPCharacter>())
                    {
                        if(tempSelectedPickUpGO.GetComponent<NPCharacter>().InvSelling)
                        {
                            tradeWindow.gameObject.SetActive(true);
                            List<GameObject> items = new()
                            {
                                inventory.GetItem(selectedList, selectedPosition)
                            };
                            tradeWindow.Set(items, selectedList, selectedPosition);
                        }
                    }
                    else            //normal ItemTransfer (not trading)
                    {
                        inventory.Move(selectedPosition, selectedList, selList);
                    }
                    
                    DescriptionDisplay.GetComponent<ItemDescription>().DisplayItem(null, 0, 0);
                }
                else
                {
                    selectedList = selList;
                    selectedPosition = selPos;
                    DescriptionDisplay.GetComponent<ItemDescription>().DisplayItem(inventory.GetItem(selList, selPos), selList, selPos);
                }
            }
            else if(selectedList == 2)
            {
                if(selList == 3)
                {
                    if(tempSelectedPickUpGO.GetComponent<NPCharacter>())
                    {
                        if(tempSelectedPickUpGO.GetComponent<NPCharacter>().InvSelling)
                        {
                            tradeWindow.gameObject.SetActive(true);
                            List<GameObject> items = new()
                            {
                                inventory.GetItem(selectedList, selectedPosition)
                            };
                            tradeWindow.Set(items, selectedList, selectedPosition);
                        }
                    }
                    else            //normal ItemTransfer (not trading)
                    {
                        inventory.PopSlot(selectedPosition, selList);
                    }
                }
                else
                {
                    inventory.PopSlot(selectedPosition, selList);
                }
                
                selectedList = 0;
                selectedPosition = 0;
                DescriptionDisplay.GetComponent<ItemDescription>().DisplayItem(null, 0, 0);
            }
            else if(selectedList == 3)
            {
                if(selList == 2)
                {
                    if(tempSelectedPickUpGO.GetComponent<NPCharacter>())
                    {
                        if(tempSelectedPickUpGO.GetComponent<NPCharacter>().InvSelling)
                        {
                            tradeWindow.gameObject.SetActive(true);
                            List<GameObject> items = new()
                            {
                                inventory.GetItem(selectedList, selectedPosition)
                            };
                            tradeWindow.Set(items, selectedList, selectedPosition);
                        }
                    }
                    else            //normal ItemTransfer (not trading)
                    {
                        inventory.AddToSlot(selectedPosition, selPos, selectedList);
                    }
                }
                else if(selList == 1)
                {
                    if(tempSelectedPickUpGO.GetComponent<NPCharacter>())
                    {
                        if(tempSelectedPickUpGO.GetComponent<NPCharacter>().InvSelling)
                        {
                            tradeWindow.gameObject.SetActive(true);
                            List<GameObject> items = new()
                            {
                                inventory.GetItem(selectedList, selectedPosition)
                            };
                            tradeWindow.Set(items, selectedList, selectedPosition);
                        }
                    }
                    else            //normal ItemTransfer (not trading)
                    {
                        inventory.Move(selectedPosition, selectedList, selList);
                    }
                }
                else
                {
                    selectedList = selList;
                    selectedPosition = selPos;
                    DescriptionDisplay.GetComponent<ItemDescription>().DisplayItem(inventory.GetItem(selList, selPos), selList, selPos);
                }
            }
            unloadInv();
            loadInv();
        }
    }

    public bool GetCoins( int CoinAmount, Inventory otherInv)
    {
        Debug.Log("Coins: " + CoinAmount);
        
        if(CoinAmount > 0)      //you get the coins
        {
            if(otherInv.Coins)
            {
                if(otherInv.Coins.GetComponent<InvItem>().Count >= CoinAmount)
                {
                    otherInv.Coins.GetComponent<InvItem>().Count -= CoinAmount;
                }
                else
                {
                    foreach(int i in otherInv.GetItems(1, 6))
                    {
                        if(!otherInv.Coins)
                        {
                            otherInv.Coins = otherInv.inventory[i];
                        }
                        else
                        {
                            if(otherInv.Coins != otherInv.inventory[i])
                            {
                                otherInv.Coins.GetComponent<InvItem>().Count += otherInv.inventory[i].GetComponent<InvItem>().Count;
                                otherInv.inventory[i].GetComponent<InvItem>().Count = 0;
                                Destroy(otherInv.inventory[i]);
                                otherInv.inventory.RemoveAt(i);
                            }
                        }
                    }
                    
                    if(otherInv.Coins.GetComponent<InvItem>().Count >= CoinAmount)
                    {
                        otherInv.Coins.GetComponent<InvItem>().Count -= CoinAmount;
                    }
                    else
                    {
                        scriptDamagePopup.Create(MainMenu.mainMenu.Notifications.transform, "There arent enough Coins!", 1, new Vector3(0, 4f, 0));
                        return false;
                    }
                }
            }
            else
            {
                foreach(int i in otherInv.GetItems(1, 6))
                {
                    if(otherInv.inventory[i].name == "Coin")
                    {
                        if(!otherInv.Coins)
                        {
                            otherInv.Coins = otherInv.inventory[i];
                        }
                        else
                        {
                            otherInv.Coins.GetComponent<InvItem>().Count += otherInv.inventory[i].GetComponent<InvItem>().Count;
                            otherInv.inventory[i].GetComponent<InvItem>().Count = 0;
                            Destroy(otherInv.inventory[i]);
                            otherInv.inventory.RemoveAt(i);
                        }
                    }
                }
                if(otherInv.Coins)
                {
                    return GetCoins(CoinAmount, otherInv);
                }
                else
                {
                    scriptDamagePopup.Create(MainMenu.mainMenu.Notifications.transform, "There arent enough Coins!", 1, new Vector3(0, 4f, 0));
                    return false;
                }
            }

            if(inventory.Coins)
            {
                inventory.Coins.GetComponent<InvItem>().Count += CoinAmount;
            }
            else
            {
                inventory.Coins = Instantiate( CoinPf, inventory.InvParent.transform.position, inventory.InvParent.transform.rotation);
                inventory.inventory.Add(inventory.Coins);
                inventory.Coins.GetComponent<InvItem>().Count = CoinAmount;
            }
        }
        else        //you pay the coins
        {
            if(inventory.Coins)
            {
                if(inventory.Coins.GetComponent<InvItem>().Count >= -CoinAmount)
                {
                    inventory.Coins.GetComponent<InvItem>().Count += CoinAmount;
                }
                else
                {
                    foreach(int i in inventory.GetItems(1, 6))
                    {
                        if(!inventory.Coins)
                        {
                            inventory.Coins = inventory.inventory[i];
                        }
                        else
                        {
                            if(inventory.Coins != inventory.inventory[i])
                            {
                                inventory.Coins.GetComponent<InvItem>().Count += inventory.inventory[i].GetComponent<InvItem>().Count;
                                inventory.inventory[i].GetComponent<InvItem>().Count = 0;
                                Destroy(inventory.inventory[i]);
                                inventory.inventory.RemoveAt(i);
                            }
                        }
                    }
                    
                    if(inventory.Coins.GetComponent<InvItem>().Count >= -CoinAmount)
                    {
                        inventory.Coins.GetComponent<InvItem>().Count += CoinAmount;
                    }
                    else
                    {
                        scriptDamagePopup.Create(MainMenu.mainMenu.Notifications.transform, "You dont have enough Coins!", 1, new Vector3(0, 4f, 0));
                        return false;
                    }
                }
            }
            else
            {
                foreach(int i in inventory.GetItems(1, 6))
                {
                    if(!inventory.Coins)
                    {
                        inventory.Coins = inventory.inventory[i];
                    }
                    else
                    {
                        if(inventory.Coins != inventory.inventory[i])
                        {
                            inventory.Coins.GetComponent<InvItem>().Count += inventory.inventory[i].GetComponent<InvItem>().Count;
                            inventory.inventory[i].GetComponent<InvItem>().Count = 0;
                            Destroy(inventory.inventory[i]);
                            inventory.inventory.RemoveAt(i);
                        }
                    }
                }
                
                if(inventory.Coins.GetComponent<InvItem>().Count >= -CoinAmount)
                {
                    inventory.Coins.GetComponent<InvItem>().Count += CoinAmount;
                }
                else
                {
                    scriptDamagePopup.Create(MainMenu.mainMenu.Notifications.transform, "You dont have enough Coins!", 1, new Vector3(0, 4f, 0));
                    return false;
                }
            }
            
            if(otherInv.Coins)
            {
                otherInv.Coins.GetComponent<InvItem>().Count -= CoinAmount;
            }
            else
            {
                otherInv.Coins = Instantiate( CoinPf, otherInv.InvParent.transform.position, otherInv.InvParent.transform.rotation);
                otherInv.inventory.Add(otherInv.Coins);
                otherInv.Coins.GetComponent<InvItem>().Count = -CoinAmount;
            }
        }
        
        return true;
    }

    public void Trade()
    {
        selectedList = tradeWindow.selectedList;
        selectedPosition = tradeWindow.selectedPos;
        int price = tradeWindow.Price;
        List<GameObject> items = tradeWindow.Items;
        Inventory tempInv = tempSelectedPickUpGO.GetComponent<Inventory>();

        if(selectedList == 1)
        {
            if(!PickUpActive)
            {
                PickUpActive = true;
                PickUpDisplay.SetActive(true);
                PickUpButton.SetActive(false);
            }

            if(GetCoins( price, tempInv))
            {
                if(tradeWindow.Amount.value == tradeWindow.Amount.maxValue)
                {
                    inventory.Move(selectedPosition, selectedList, 3);
                }
                else
                {
                    inventory.GetItem(selectedList, selectedPosition).GetComponent<InvItem>().Count -= (int) tradeWindow.Amount.value;
                    GameObject item = Instantiate(items[0], tempInv.InvParent.transform.position, tempInv.InvParent.transform.rotation);
                    item.GetComponent<InvItem>().Count = (int) tradeWindow.Amount.value;
                    inventory.PickUpList.Add(item);
                }
                Select( selectedList, selectedPosition);
            }
        }
        else if(selectedList == 3)
        {
            if(!InvActive)
            {
                InvActive = true;
                InventoryDisplay.SetActive(true);
                InvButton.SetActive(false);
            }

            if(GetCoins( -price, tempInv))
            {
                if(tradeWindow.Amount.value == tradeWindow.Amount.maxValue)
                {
                    inventory.Move(selectedPosition, selectedList, 1);
                }
                else
                {
                    inventory.GetItem(selectedList, selectedPosition).GetComponent<InvItem>().Count -= (int) tradeWindow.Amount.value;
                    GameObject item = Instantiate(items[0], inventory.InvParent.transform.position, inventory.InvParent.transform.rotation);
                    item.GetComponent<InvItem>().Count = (int) tradeWindow.Amount.value;
                    inventory.inventory.Add(item);
                }
                Select( selectedList, selectedPosition);
            }
        }
        else if(selectedList == 0)
        {
            if(PickUpActive)
            {
                if(!InvActive)
                {
                    InvActive = true;
                    InventoryDisplay.SetActive(true);
                    InvButton.SetActive(false);
                }

                if(GetCoins( -price, tempInv))
                {
                    foreach(GameObject item in items)
                    {
                        inventory.Move(selectedPosition, selectedList, 1);
                        tempInv.RemoveItem(item);
                    }
                }
            }
        }

        unloadInv();
        loadInv();
    }
}