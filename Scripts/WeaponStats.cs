using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponStats : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Load();
    }

    public bool Melee;                  //true: MeleeWeapon; false: RangeWeapon
    public bool TwoHanded;              //true: no Shield and belongs to the Middle GameObject
    public InvItem invItem;

    [Header("Damage")]
    public float DamageMult = 1;
    public Damage DamageScript;

    [Header("Stats")]
    public int Size;                    //Size of the Weapon (you can only wield a weapon if your Size <= Size of weapon)
    public int DamageType;              //Damage done by a sharp point, a sharp blade or the sheer force of the blow
    public float AttackCooldown;        //how many seconds between attacks
    public float Cooldown;
    public float LocalWeight;
    public float Range;

    [Header("Body")]
    public Collider Hitbox;
    public Animator mAnimator;

    [Header("Range Weapon")]
    public LayerMask layerMask;
    public Transform ArrowSpawn;        //Spawnpoint for Projectiles
    public float ManaRate;
    public int Manacost;                //how much Mana is consumed per shot

    [Header("Projectile")]
    public int ArrowCost ;              //how many Arrows are consumed per shot
    public int ArrowNumber;             //how many Arrows are fired each shot
    public GameObject ArrowPf;          //the Prefab of the Projectile
    public GameObject ArrowInvGO;       //the ArrowStack in the Inv
    public int NeededArrowType;         //Arrowtype the Weapon can use
    public bool UseItself;              //Throw itself (Throwing Knifes)
    public float Force;                 //Force applied to Projectile

    [Header("Runtime")]
    public Stats parentStats;
    public GameObject parentGO;
    public int ETW0;
    public int SchadensMod;             //extra Damage for high Strenght or Dexterity
    GameObject currentArrow;
    public GameObject Target;
    public List<GameObject> Hits;


    void OnDestroy() 
    {
        Debug.Log("This GameObject is being destroyed: " + gameObject.name);
    }
    
    // Update is called once per frame
    void Update()
    {
        if(invItem)
        {
            if(invItem.Collectable)
            {
                return;
            }
        }

        if(Cooldown > 0)
        {
            Cooldown -= Time.deltaTime * parentStats.attackSpeedMult;
        }

        if(Melee)   //MeleeWeapon
        {
            if(parentGO != null)
            {
                if(parentGO.GetComponent<Animator>())
                {
                if(TwoHanded)
                    {
                        if(parentGO.GetComponent<Animator>().GetCurrentAnimatorStateInfo(1).IsName("MeleeAttack_TwoHanded 2"))
                        {
                            Hitbox.enabled = true;
                        }
                        else
                        {
                            Hitbox.enabled = false;
                        }
                    }   
                    else
                    {
                        if(parentGO.GetComponent<Animator>().GetCurrentAnimatorStateInfo(1).IsName("MeleeAttack_OneHanded 2"))
                        {
                            Hitbox.enabled = true;
                        }
                        else
                        {
                            Hitbox.enabled = false;
                        }
                    }
                }
            }
            else
            {
                Debug.Log("ParentGO hasnt been assigned!");
            }
        }
    }

    public void Attack(int BaseETW0, int MeleeBoniETW0, int RangeBoniETW0, int MeleeBoniSchaden, int tempETW0, float damageMult, Vector3 tempTargetPos = new())
    {
        if(Cooldown > 0)
        {
            return;
        }
        

        if(ManaRate != 0)           //weapon needs manausing projectiles
        {
            int temp;
            if(ArrowPf)
            {
                if(ArrowPf.GetComponent<Arrow>().Manacost == 0)
                {
                    //return;
                }
                temp = Mathf.CeilToInt((Manacost + ArrowPf.GetComponent<Arrow>().Manacost) / ManaRate);
            }
            else
            {
                temp = Mathf.CeilToInt(Manacost / ManaRate);
            }

            if(parentStats.Mana >= temp)
            {
                parentStats.Mana -= temp;
            }
            else
            {
                if(parentGO.layer.Equals("Player"))
                {
                    scriptDamagePopup.Create(MainMenu.mainMenu.transform, "Not enough Mana!", 1, new Vector3(0, 4f, 0));
                }
                return;
            }
        }
        else
        {
            if(ArrowPf)
            {
                if(ArrowPf.GetComponent<Arrow>().Manacost != 0)
                {
                    Debug.LogWarning("The Weapon has an Arrow with a Manacost but no ManaRate!");
                    return;
                }
            }
            if(Manacost != 0)
            {
                Debug.LogWarning("The Weapon has a Manacost but no ManaRate!");
                return;
            }
        }

        Hits = new();
        
        DamageMult = damageMult;
        if(Melee)   //MeleeWeapon
        {
            ETW0 = BaseETW0 - MeleeBoniETW0 - tempETW0;
            SchadensMod = MeleeBoniSchaden;
            if(TwoHanded)
            {
                parentGO.GetComponent<Animator>().SetTrigger("AttackTwoHanded");
            }
            else
            {
                parentGO.GetComponent<Animator>().SetTrigger("AttackSingleHanded");
            }
        }
        else        //RangeWeapon
        {
            if(!ArrowPf)
            {
                if(ArrowInvGO)
                {
                    ArrowPf = ArrowInvGO.GetComponent<Arrow>().ArrowPf;
                }
                else
                {
                    if(parentGO.layer.Equals("Player"))
                    {
                        scriptDamagePopup.Create(MainMenu.mainMenu.transform, "No Projectiles selected!", 1, new Vector3(0, 4f, 0));
                    }
                    return;
                }
            }
            else if(NeededArrowType != ArrowPf.GetComponent<Arrow>().ArrowType)
            {
                if(parentGO.layer.Equals("Player"))
                {
                    scriptDamagePopup.Create(MainMenu.mainMenu.transform, "Not the right Projectiles selected!", 1, new Vector3(0, 4f, 0));
                }
                return;
            }

            if(ArrowCost != 0)          //the Weapon uses up the Projectiles it shoots
            {
                InvItem ArrowInvItem;
                if(!ArrowInvGO && !UseItself)         //Projectiles were deleted
                {
                    if(parentGO.layer.Equals("Player"))
                    {
                        scriptDamagePopup.Create(MainMenu.mainMenu.transform, "No Projectile selected!", 1, new Vector3(0, 4f, 0));
                    }
                    return;
                }
                else if(UseItself)
                {
                    ArrowInvItem = this.GetComponent<InvItem>();
                }
                else
                {
                    ArrowInvItem = ArrowInvGO.GetComponent<InvItem>();
                }
                
                if(ArrowInvItem.Count < ArrowCost)        //not enough Arrows to fire
                {
                    if(ArrowInvItem.Count <= 0)           //no Arrows left
                    {
                        if(parentGO.layer.Equals("Player"))
                        {
                            scriptDamagePopup.Create(MainMenu.mainMenu.transform, "No Projectiles!", 1, new Vector3(0, 4f, 0));
                        }
                        if(ArrowInvGO)
                        {
                            parentGO.GetComponent<Inventory>().RemoveItem(ArrowInvGO);
                            Destroy(ArrowInvGO, 0);
                        }
                        else
                        {
                            parentGO.GetComponent<Inventory>().RemoveItem(gameObject);
                            Destroy(gameObject, 0);
                        }
                    }
                    else
                    {
                        if(parentGO.layer.Equals("Player"))
                        {
                            scriptDamagePopup.Create(MainMenu.mainMenu.transform, "Not enough Projectiles!", 1, new Vector3(0, 4f, 0));
                        }
                    }
                    return;
                }
                else        //there are enough Arrows to shoot
                {
                    ArrowInvItem.Count -= ArrowCost;
                    if(ArrowInvItem.Count <= 0)           //used up remaining Arrows
                    {
                        if(parentGO.layer.Equals("Player"))
                        {
                            scriptDamagePopup.Create(MainMenu.mainMenu.transform, "Used up all Projectiles!", 1, new Vector3(0, 4f, 0));
                        }
                        if(ArrowInvGO)
                        {
                            parentGO.GetComponent<Inventory>().RemoveItem(ArrowInvGO);
                            Destroy(ArrowInvGO, 0);
                        }
                        else
                        {
                            parentGO.GetComponent<Inventory>().RemoveItem(gameObject);
                            Destroy(gameObject, 0);
                        }
                    }
                }
            }
            
            Vector3 TargetPos = new();
            Vector3 Pos = ArrowSpawn.position;
            
            if(tempTargetPos != Vector3.zero)
            {
                TargetPos = tempTargetPos;
                //if(Target.GetComponent<Rigidbody>())        //calculates the likely position of the target because of its velocity
                //{
                //    TargetPos = Target.transform.position + (Target.transform.position - Pos).magnitude * ArrowPf.GetComponent<Rigidbody>().mass / Force * Target.GetComponent<Rigidbody>().velocity;
                //}
                //else
                //{
                //    TargetPos = Target.transform.position;
                //}
            }
            else
            {
                Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));       //ray from the crosshair
                RaycastHit hit;
                if(Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))        //hit something
                {
                    TargetPos = hit.point;
                }
            }
            ETW0 = BaseETW0 - RangeBoniETW0 - tempETW0;
            SchadensMod = 0;
            float a = BowAngle(TargetPos.y - Pos.y, Mathf.Sqrt(Mathf.Pow(TargetPos.x - Pos.x, 2)+Mathf.Pow(TargetPos.z - Pos.z, 2)), Force/ArrowPf.GetComponent<Rigidbody>().mass);
            Quaternion Rot;
            if((TargetPos.z - Pos.z) >= 0f)
            {
                Rot = Quaternion.Euler(a, Mathf.Rad2Deg * Mathf.Atan((TargetPos.x - Pos.x) / (TargetPos.z - Pos.z)), 0);
            }
            else
            {
                Rot = Quaternion.Euler(a, 180 + Mathf.Rad2Deg * Mathf.Atan((TargetPos.x - Pos.x) / (TargetPos.z - Pos.z)), 0);
            }

            StartCoroutine(SpawnArrows( TargetPos, Pos, Rot));
            
            currentArrow = null;
        }
        Cooldown = AttackCooldown;
    }

    public void ChangeArrow(GameObject tempArrowPf, GameObject tempArrowInvGO)
    {
        if(tempArrowInvGO)
        {
            if(tempArrowInvGO.GetComponent<Arrow>().ArrowType == NeededArrowType)
            {
                ArrowInvGO = tempArrowInvGO;
                ArrowPf = tempArrowInvGO.GetComponent<Arrow>().ArrowPf;
            }
            else
            {
                return;
            }
        }
        else if(tempArrowPf != null)
        {
            if(ArrowPf.GetComponent<Arrow>().ArrowType == NeededArrowType)
            {
                ArrowInvGO = null;
                ArrowPf = tempArrowPf;
            }
            else
            {
                return;
            }
        }
        else
        {

        }
    }

    IEnumerator SpawnArrows( Vector3 hitPos, Vector3 tempPos, Quaternion tempRot)
    {
        for(int i = 0; i < ArrowNumber; i++)
        {
            for(int j = 0; j < ArrowPf.GetComponent<Arrow>().ArrowNumber; j++)
            {
                currentArrow = Instantiate(ArrowPf, tempPos, tempRot);
                Arrow temp = currentArrow.GetComponent<Arrow>();
                temp.Force = Force;
                temp.Origin = this.gameObject;
                temp.Target = Target;
                temp.TargetPos = hitPos;
                temp.InvItem = false;
                currentArrow.SetActive(true);

                yield return new WaitForSeconds(temp.TimeBetweenArrows);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.isTrigger)
        {
            return;
        }
        
        GameObject HitObject = other.gameObject;
        while(!HitObject.GetComponent<Stats>())
        {
            HitObject = HitObject.transform.parent.gameObject;
            if(HitObject == null)
            {
                return;
            }
        }

        for(int i = 0; i < Hits.Count; i++)
        {
            if(Hits[i] == HitObject)
            {
                return;
            }
        }
        Hits.Add(HitObject);

        DamageScript.DoDamage(HitObject.GetComponent<Stats>(), parentStats, this, SchadensMod, ETW0);
    }
    
    float BowAngle(float height, float distance, float speed) 
    {
        float x = Mathf.Rad2Deg * Mathf.Atan(height / distance);        //Anfang mit Winkel, der genau aufs Ziel zeigt (man muss immer drüber sein um es wirklich zu treffen)

        if(ArrowPf.GetComponent<Arrow>().IgnoreGravity || ArrowPf.GetComponent<Arrow>().Homing)
        {
            return -x;
        }

        float step = 0.01f;
        float dHeight = -10000;
        float dHeight2;
        while (true) 
        {
            x += step;

            dHeight2 = Mathf.Tan(Mathf.Deg2Rad * x) * distance - 0.5f * 9.81f * Mathf.Pow(distance / (speed * Mathf.Cos(Mathf.Deg2Rad * x)), 2) - height;

            if(dHeight2 < dHeight)      //wenn durch vergrößern von x die Nähe zum Ziel nur kleiner wird
            {
                Debug.Log("Target not in range!");
                return -x;
            }
            
            if(dHeight2 >= 0)       //wenn das Ziel getroffen wird
            {
                return -x;
            }

            dHeight = dHeight2;
        }
    }

    public void Load()
    {
        Transform t = transform;
        while(t.parent)
        {
            if(t.parent.GetComponent<Inventory>())      //if the parentGO has an inventory
            {
                t = t.parent;
                parentGO = t.gameObject;
                parentStats = t.GetComponent<Stats>();
                Debug.Log("Found Stats on " + t.name);
                break;
            }
            t = t.parent;
        }

        if(parentGO == null || parentStats == null)
        {
            if(parentGO == null && parentStats == null)
            {
                Debug.LogWarning("Couldnt find the Parent GameObject and the Stats of the Parent! " + t.name);
            }
            else if(parentGO == null)
            {
                Debug.LogWarning("Couldnt find the Parent GameObject! " + t.name);
            }
            else
            {
                Debug.LogWarning("Couldnt find the Stats of the Parent! " + t.name);
            }
            
        }

        if(Hitbox == null)
        {
            Hitbox = transform.GetChild(0).GetComponent<Collider>();
        }
        
        if(!ArrowPf)
        {
            if(ArrowInvGO)
            {
                ArrowPf = ArrowInvGO.GetComponent<Arrow>().ArrowPf;
            }
        }

        if(!invItem)
        {
            invItem = GetComponent<InvItem>();
            if(!invItem)
            {
                Debug.LogWarning("No InvItem attatched! Deleting self!");
                Destroy(gameObject);
            }
        }
    }
}
