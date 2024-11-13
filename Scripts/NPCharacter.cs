using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SocialPlatforms.GameCenter;

public class NPCharacter : MonoBehaviour
{
    public Inventory inventory;
    public bool InvSelling;
    public Stats stats;
    public GameObject BareHand;
    

    [Header("Moving")]
    public NavMeshAgent agent;
    public Animator animator;
    public LayerMask jumpLayerMask;
    private float jumpRayLength = 1.05f;
    public bool grounded = false;

    [Header("Detect Enemy")]
    public int Affiliation;
    public LayerMask hitLayerMask;
    public float hitDistance = 20;      //the furthest a target can be to detect it
    public float MinDistance;           //can detects anything that close
    public Transform Viewfield;         //Pos and Rot of the Head
    public float maxAngle;              //determines the size of the Viewfield

    [Header("Generating Stats")]
    public int MinLvl;
    public int MaxLvl;
    public bool GenerateAttributes;
    public int AttrDice;
    public int AttrTries;
    public int AttrBoni;

    [Header("Runtime")]
    public WeaponStats currentWeapon;
    public float waitingTime;
    GameObject Consumable;
    public List<GameObject> personalEnemies = new();
    public List<GameObject> NPCinArea = new List<GameObject>();
    public Transform target;
    public GameObject tempLastHittetBy;
    public float targetTime;
    public Vector3 PointOfInterest;

    [Header("Saving")]
    public List<int> EnemyIDs;
    public int PrefabNumber;

    // Start is called before the first frame update
    void Start()
    {
        if(MainMenu.mainMenu.IsInList(gameObject))
        {

        }
        if(MainMenu.mainMenu.finishedLoadingNPCs)
        {
            if(EnemyIDs != null)
            {
                if(EnemyIDs.Count > 0)
                {
                    personalEnemies = MainMenu.mainMenu.GetEnemies(EnemyIDs);
                }
            }
        }

        if(GenerateAttributes)
        {
            GenerateAttributes = false;
            stats.GenerateAttributes(AttrDice, AttrTries, AttrBoni);
            
            if(MinLvl != 0)
            {
                stats.UpdateLevels(Random.Range(MinLvl, MaxLvl + 1));
            }
        }
        
        if(!agent)
        {
            agent = GetComponent<NavMeshAgent>();
            if(!agent)
            {
                Debug.LogWarning("Couldnt find NavMeshAgent!");
            }
        }
        if(!inventory)
        {
            inventory = gameObject.GetComponent<Inventory>();
            if(!stats)
            {
                Debug.LogWarning("Couldnt find Inventory!");
            }
        }
        if(!stats)
        {
            stats = gameObject.GetComponent<Stats>();
            if(!stats)
            {
                Debug.LogWarning("Couldnt find Stats!");
            }
        }

        if(!animator)
        {
            animator = GetComponent<Animator>();
            if(!animator)
            {
                //animated = false;
            }
            else
            {
                //animated = true;
            }
        }
        else
        {
            //animated = true;
        }

        if(Viewfield != null)
        {
            Viewfield.GetChild(0).localScale = Vector3.one * 2*(hitDistance + 5);
        }
        else
        {
            Debug.LogWarning("No Viewfield detected!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(MainMenu.mainMenu.finishedLoadingNPCs)
        {
            if(EnemyIDs != null)
            {
                if(EnemyIDs.Count > 0)
                {
                    personalEnemies = MainMenu.mainMenu.GetEnemies(EnemyIDs);
                }
            }
        }
        
        if(stats.Hitpoints <= 0)
        {
            if(!animator.GetBool("Dead"))
            {
                animator.SetBool("Dead", true);
                animator.SetTrigger("Death");
                foreach(Collider col in GetComponentsInChildren<Collider>())
                {
                    col.enabled = false;
                }
            }
            return;
        }
        
        RaycastHit hit;
        grounded = Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, jumpRayLength, jumpLayerMask);
        animator.SetBool("isGrounded", grounded);

        if(waitingTime > 0)
        {
            waitingTime -= Time.deltaTime;
            return;
        }
        
        if(target)
        {
            if(maxAngle < Vector3.Angle(Viewfield.forward, target.position - Viewfield.position))       //target is not in the Viewfield
            {
                //target = null;
            }
            else if(checkRays(target.transform.position, Viewfield.position, null, true, target.transform) == Vector3.zero)     //something is blocking direct sight
            {
                //target = null;
            }
            else
            {
                targetTime = 2;     //for this time the target will be remembered
            }

            if(targetTime > 0)
            {
                targetTime -= Time.deltaTime;
            }
            else    //Target is forgotten
            {
                target = null;
            }
        }
        else if(PointOfInterest != Vector3.zero)
        {
            if(targetTime > 0)
            {
                targetTime -= Time.deltaTime;
                PointOfInterest.y = 0;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(PointOfInterest - transform.position, Vector3.up), agent.angularSpeed * Time.deltaTime);
            }
            else    //Target is forgotten
            {
                PointOfInterest = Vector3.zero;
            }
            
        }
            
        for (int i = NPCinArea.Count - 1; i >= 0; i--)
        {
            if (!NPCinArea[i])
            {
                NPCinArea.RemoveAt(i);
            }
        }
        
        if(personalEnemies.Count > 0)
        {
            NPCharacter tempPB;
            foreach(GameObject npc in NPCinArea)
            {
                tempPB = npc.GetComponent<NPCharacter>();
                if(tempPB)
                {
                    if(tempPB.Affiliation == Affiliation)
                    {
                        foreach(GameObject i in personalEnemies)        //the personal Enemies will be shared
                        {
                            if(!tempPB.personalEnemies.Contains(i))
                            {
                                tempPB.personalEnemies.Add(i);
                            }
                        }
                        if(!tempPB.target && tempPB.targetTime <= 0)    //the target will be shared
                        {
                            tempPB.target = target;
                            tempPB.targetTime = targetTime / 2;     //to prevent a feedback loop
                        }
                    }
                }
            }
        }
        else if(target != null)
        {
            NPCharacter tempPB;
            foreach(GameObject npc in NPCinArea)
            {
                tempPB = npc.GetComponent<NPCharacter>();
                if(tempPB)
                {
                    if(tempPB.Affiliation == Affiliation)
                    {
                        if(!tempPB.target && tempPB.targetTime <= 0)    //the target will be shared
                        {
                            tempPB.target = target;
                            tempPB.targetTime = targetTime / 2;     //to prevent a feedback loop
                        }
                    }
                }
            }
        }

        if(tempLastHittetBy != stats.LastHittedByGO)        //was hitted by somebody else
        {
            tempLastHittetBy = stats.LastHittedByGO;
            stats.LastHittedByGO = null;
            NPCharacter temp;
            if(tempLastHittetBy != null)
            {
                if(tempLastHittetBy.TryGetComponent<NPCharacter>(out temp))
                {
                    if(temp.Affiliation != Affiliation)     //Hitted by something not of its group
                    {
                        if(!MainMenu.mainMenu.Relations[Affiliation].Contains(temp.Affiliation))
                        {
                            personalEnemies.Add(tempLastHittetBy);
                            if(!target)
                            {
                                if((tempLastHittetBy.transform.position - transform.position).magnitude <= MinDistance)
                                {
                                    target = tempLastHittetBy.transform;
                                }
                                else
                                {
                                    PointOfInterest = tempLastHittetBy.transform.position;
                                }
                                targetTime = 2;
                            }
                        }
                    }
                }
                else if(tempLastHittetBy.GetComponent<PlayerController>())
                {
                    if(0 != Affiliation)        //Hitted by something not of its group
                    {
                        if(!MainMenu.mainMenu.Relations[Affiliation].Contains(0))
                        {
                            personalEnemies.Add(tempLastHittetBy);
                            if(!target)
                            {
                                if((tempLastHittetBy.transform.position - transform.position).magnitude <= MinDistance)
                                {
                                    target = tempLastHittetBy.transform;
                                }
                                else
                                {
                                    PointOfInterest = tempLastHittetBy.transform.position;
                                }
                                targetTime = 2;
                            }
                        }
                    }
                }
            }
        }
        
        foreach(GameObject NPC in NPCinArea)
        {
            if(!NPC)
            {
                continue;
            }
            
            if(personalEnemies.Contains(NPC))       //is a personal Enemy
            {
                if(target)
                {
                    if((target.position - transform.position).magnitude > (NPC.transform.position - transform.position).magnitude + 1.5)
                    {
                        SetTarget(NPC.transform);
                    }
                }
                else
                {
                    SetTarget(NPC.transform);
                }
            }
                
            if(NPC.GetComponent<PlayerController>())
            {
                if(MainMenu.mainMenu.Relations[Affiliation].Contains(0))
                {
                    if(target)
                    {
                        if((target.position - transform.position).magnitude > (NPC.transform.position - transform.position).magnitude + 1.5)
                        {
                            SetTarget(NPC.transform);
                        }
                    }
                    else
                    {
                        SetTarget(NPC.transform);
                    }
                }
            }
            else if(NPC.GetComponent<NPCharacter>())
            {
                if(MainMenu.mainMenu.Relations[Affiliation].Contains(NPC.GetComponent<NPCharacter>().Affiliation))
                {
                    if(target)
                    {
                        if((target.position - transform.position).magnitude > (NPC.transform.position - transform.position).magnitude + 1.5)
                        {
                            SetTarget(NPC.transform);
                        }
                    }
                    else
                    {
                        SetTarget(NPC.transform);
                    }
                }
            }
        }

        if(animator.GetCurrentAnimatorStateInfo(1).IsName("Wait"))
        {
            if(stats.Hitpoints <= 0.3f * stats.MaxHitpoints)
            {
                if(Random.Range(0, 21) >= 20)
                {
                    Regenerate(1);
                    agent.destination = transform.position;
                    return;
                }
            }
            else if(stats.Hitpoints <= 0.2f * stats.MaxHitpoints)
            {
                if(Random.Range(0, 21) >= 20)
                {
                    Regenerate(2);
                    agent.destination = transform.position;
                    return;
                }
            }
        }
        else if(animator.GetCurrentAnimatorStateInfo(1).IsName("Drinking 2"))
        {
            if(Consumable)
            {
                stats.Consume(Consumable);
                Consumable.GetComponent<InvItem>().Count -= 1;
                if(Consumable.GetComponent<InvItem>().Count < 1)
                {
                    inventory.RemoveItem(Consumable);
                    Destroy(Consumable, 0);
                }
                Consumable = null;
                animator.ResetTrigger("Drink");
                agent.destination = transform.position;
            }
            return;
        }
        else if(animator.GetCurrentAnimatorStateInfo(1).IsName("Drinking 1"))
        {
            agent.destination = transform.position;
            return;
        }
        
        if(target)
        {
            if((target.position - transform.position).magnitude <= hitDistance - 3)
            {
                if(inventory.RightParent.transform.childCount > 0)
                {
                    currentWeapon = inventory.RightParent.transform.GetChild(0).gameObject.GetComponent<WeaponStats>();
                    if(currentWeapon.Melee)
                    {
                        animator.SetBool("isDrawing", false);
                        if(checkWeapon(currentWeapon) == Vector3.zero)
                        {
                            return;
                        }
                        else
                        {
                            if(inventory.RightParent.transform.childCount > 0)
                            {
                                if(currentWeapon.transform != inventory.RightParent.transform.GetChild(0))
                                {
                                    return;
                                }
                            }
                            else
                            {
                                return;
                            }
                        }

                        if((target.position - transform.position).magnitude > 0.71 + Mathf.Cos(0.488f) * currentWeapon.Range - 0.25)
                        {
                            agent.destination = target.position;
                            animator.SetBool("isFRunning", true);
                        }
                        else        //already close enough to the target
                        {
                            agent.destination = transform.position;
                            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(new Vector3(target.position.x - transform.position.x, 0, target.position.z - transform.position.z), Vector3.up), agent.angularSpeed * Time.deltaTime);
                            animator.SetBool("isFRunning", false);
                        }
                        
                        if((target.position - transform.position).magnitude <= 0.71 + Mathf.Cos(0.488f) * currentWeapon.Range)
                        {
                            int BaseETW0 = stats.BaseETW0;
                            int MeleeBoniETW0 = stats.MeleeBoniETW0;
                            int RangeBoniETW0 = stats.RangeBoniETW0;
                            int MeleeBoniSchaden = stats.MeleeBoniSchaden;
                            int tempETW0 = stats.tempETW0;
                            float damageMult = stats.damageMult;
                            
                            currentWeapon.Attack(BaseETW0, MeleeBoniETW0, RangeBoniETW0, MeleeBoniSchaden, tempETW0, damageMult);
                        }
                    }
                    else                                                //Fernkampfwaffe
                    {
                        animator.SetFloat("Attacking Speed", currentWeapon.AttackCooldown);
                        if(animator.GetCurrentAnimatorStateInfo(1).IsName("BowHold") && currentWeapon.Cooldown <= 0)      //can shoot
                        {
                            int BaseETW0 = stats.BaseETW0;
                            int MeleeBoniETW0 = stats.MeleeBoniETW0;
                            int RangeBoniETW0 = stats.RangeBoniETW0;
                            int MeleeBoniSchaden = stats.MeleeBoniSchaden;
                            int tempETW0 = stats.tempETW0;
                            float damageMult = stats.damageMult;
                            
                            Vector3 tempTargetPos = checkWeapon(currentWeapon);
                            if(inventory.RightParent.transform.childCount > 0)
                            {
                                if(currentWeapon.transform != inventory.RightParent.transform.GetChild(0))
                                {
                                    return;
                                }
                            }
                            else
                            {
                                return;
                            }
                            if(tempTargetPos != Vector3.zero)       //has an unobstructed way to the Target
                            {
                                if((target.position - transform.position).magnitude <= hitDistance - 3)
                                {
                                    agent.destination = transform.position;
                                    animator.SetBool("isFRunning", false);
                                    currentWeapon.Attack(BaseETW0, MeleeBoniETW0, RangeBoniETW0, MeleeBoniSchaden, tempETW0, damageMult, tempTargetPos);
                                    animator.SetBool("isDrawing", false);
                                }
                            }
                            else        //way is obstructed
                            {
                                float tempV = currentWeapon.ArrowPf.GetComponent<Rigidbody>().mass / currentWeapon.Force;
                                tempTargetPos = betterPosition(target.transform.position + (target.transform.position - transform.position).magnitude * tempV * target.GetComponent<Rigidbody>().velocity);
                                if(tempTargetPos != Vector3.zero)
                                {
                                    agent.destination = tempTargetPos;
                                    animator.SetBool("isFRunning", true);
                                }
                                else
                                {
                                    agent.destination = target.position;
                                    animator.SetBool("isFRunning", true);
                                    Debug.Log("Nothing Found!");
                                }
                            }
                        }
                        else        //isnt ready to shoot yet
                        {
                            animator.SetBool("isDrawing", true);
                        }
                    }
                }
                else if(inventory.MiddleParent.transform.childCount > 0)
                {
                    currentWeapon = inventory.MiddleParent.transform.GetChild(0).gameObject.GetComponent<WeaponStats>();
                    if(currentWeapon.Melee)
                    {
                        animator.SetBool("isDrawing", false);
                        if(checkWeapon(currentWeapon) == Vector3.zero)
                        {
                            return;
                        }
                        else
                        {
                            if(inventory.MiddleParent.transform.childCount > 0)
                            {
                                if(currentWeapon.transform != inventory.MiddleParent.transform.GetChild(0))
                                {
                                    return;
                                }
                            }
                            else
                            {
                                return;
                            }
                        }
                        
                        if((target.position - transform.position).magnitude > 0.71 + Mathf.Cos(0.488f) * currentWeapon.Range - 0.25)
                        {
                            agent.destination = target.position;
                            animator.SetBool("isFRunning", true);
                        }
                        else
                        {
                            agent.destination = transform.position;
                            animator.SetBool("isFRunning", false);
                        }
                        
                        if((target.position - transform.position).magnitude <= 0.71 + Mathf.Cos(0.488f) * currentWeapon.Range - 0.05)
                        {
                            int BaseETW0 = stats.BaseETW0;
                            int MeleeBoniETW0 = stats.MeleeBoniETW0;
                            int RangeBoniETW0 = stats.RangeBoniETW0;
                            int MeleeBoniSchaden = stats.MeleeBoniSchaden;
                            int tempETW0 = stats.tempETW0;
                            float damageMult = stats.damageMult;
                            
                            currentWeapon.Attack(BaseETW0, MeleeBoniETW0, RangeBoniETW0, MeleeBoniSchaden, tempETW0, damageMult);
                        }
                    }
                    else                                                //Fernkampfwaffe
                    {
                        animator.SetFloat("Attacking Speed", currentWeapon.AttackCooldown);
                        if(animator.GetCurrentAnimatorStateInfo(1).IsName("BowHold") && currentWeapon.Cooldown <= 0)      //can shoot
                        {
                            int BaseETW0 = stats.BaseETW0;
                            int MeleeBoniETW0 = stats.MeleeBoniETW0;
                            int RangeBoniETW0 = stats.RangeBoniETW0;
                            int MeleeBoniSchaden = stats.MeleeBoniSchaden;
                            int tempETW0 = stats.tempETW0;
                            float damageMult = stats.damageMult;
                            
                            Vector3 tempTargetPos = checkWeapon(currentWeapon);
                            if(inventory.MiddleParent.transform.childCount > 0)
                            {
                                if(currentWeapon.transform != inventory.MiddleParent.transform.GetChild(0))
                                {
                                    return;
                                }
                            }
                            else
                            {
                                return;
                            }

                            if(tempTargetPos != Vector3.zero)       //has an unobstructed way to the Target
                            {
                                if((target.position - transform.position).magnitude <= hitDistance - 3)
                                {
                                    agent.destination = transform.position;
                                    animator.SetBool("isFRunning", false);
                                    currentWeapon.Attack(BaseETW0, MeleeBoniETW0, RangeBoniETW0, MeleeBoniSchaden, tempETW0, damageMult, tempTargetPos);
                                    animator.SetBool("isDrawing", false);
                                }
                            }
                            else        //way is obstructed
                            {
                                if(target.GetComponent<Rigidbody>())
                                {
                                    float tempV = currentWeapon.ArrowPf.GetComponent<Rigidbody>().mass / currentWeapon.Force;
                                    tempTargetPos = betterPosition(target.transform.position + (target.transform.position - transform.position).magnitude * tempV * target.GetComponent<Rigidbody>().velocity);
                                }
                                else
                                {
                                    tempTargetPos = target.position;
                                    Debug.Log("No Rigidbody detected!");
                                }
                                
                                if(tempTargetPos != Vector3.zero)
                                {
                                    agent.destination = tempTargetPos;
                                    animator.SetBool("isFRunning", true);
                                    Debug.Log("Set alternative Position! " + (tempTargetPos - transform.position));
                                }
                                else
                                {
                                    agent.destination = target.position;
                                    animator.SetBool("isFRunning", true);
                                    Debug.Log("Nothing Found!");
                                }
                            }
                        }
                        else        //isnt ready to shoot yet
                        {
                            animator.SetBool("isDrawing", true);
                        }
                    }
                }
                else    //there is no weapon equipped (so use bare Hands)
                {
                    Transform t = inventory.RightParent.transform;
                    currentWeapon = Instantiate(BareHand, t.position, t.rotation, t).GetComponent<WeaponStats>();

                    animator.SetBool("isDrawing", false);
                    if(inventory.RightParent.transform.childCount > 0)
                    {
                        if(currentWeapon.transform != inventory.RightParent.transform.GetChild(0))
                        {
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }

                    if((target.position - transform.position).magnitude > 0.71 + Mathf.Cos(0.488f) * currentWeapon.Range - 0.25)
                    {
                        agent.destination = target.position;
                        animator.SetBool("isFRunning", true);
                    }
                    else        //already close enough to the target
                    {
                        agent.destination = transform.position;
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(new Vector3(target.position.x - transform.position.x, 0, target.position.z - transform.position.z), Vector3.up), agent.angularSpeed * Time.deltaTime);
                        animator.SetBool("isFRunning", false);
                    }
                    
                    if((target.position - transform.position).magnitude <= 0.71 + Mathf.Cos(0.488f) * currentWeapon.Range)
                    {
                        int BaseETW0 = stats.BaseETW0;
                        int MeleeBoniETW0 = stats.MeleeBoniETW0;
                        int RangeBoniETW0 = stats.RangeBoniETW0;
                        int MeleeBoniSchaden = stats.MeleeBoniSchaden;
                        int tempETW0 = stats.tempETW0;
                        float damageMult = stats.damageMult;
                        
                        currentWeapon.Attack(BaseETW0, MeleeBoniETW0, RangeBoniETW0, MeleeBoniSchaden, tempETW0, damageMult);
                    }
                }
            }
            else
            {
                agent.destination = target.position;
                animator.SetBool("isFRunning", true);
            }
        }
        else
        {
            animator.SetBool("isDrawing", false);
            agent.destination = transform.position;
            animator.SetBool("isFRunning", false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.isTrigger)
        {
            return;
        }

        if(!other.CompareTag("targetable"))
        {
            return;
        }
        
        GameObject HitObject = other.gameObject;
        while(!HitObject.GetComponent<Stats>())
        {
            if(HitObject.transform.parent)
            {
                HitObject = HitObject.transform.parent.gameObject;
            }
            else
            {
                return;
            }
        }

        if(NPCinArea.Contains(HitObject))
        {
            return;
        }
        else
        {
            NPCinArea.Add(HitObject);
            Debug.Log(gameObject.name + " Added: " + HitObject.name);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(!other.CompareTag("targetable"))
        {
            return;
        }
        
        GameObject HitObject = other.gameObject;
        while(!HitObject.GetComponent<Stats>())
        {
            if(HitObject.transform.parent)
            {
                HitObject = HitObject.transform.parent.gameObject;
            }
            else
            {
                return;
            }
        }
        
        if(HitObject.GetComponent<PlayerController>() || HitObject.GetComponent<NPCharacter>())
        {
            if(NPCinArea.Contains(HitObject))
            {
                if(target == HitObject.transform)
                {
                    target = null;
                    targetTime = 0;
                }
                NPCinArea.Remove(HitObject);
                Debug.Log(gameObject.name + " Removed: " + HitObject.name);
                return;
            }
        }
        else
        {
            return;
        }
    }

    Vector3 checkWeapon( WeaponStats weaponStats)
    {
        if(weaponStats.ManaRate != 0)           //weapon uses Mana
        {
            int temp;
            if(weaponStats.ArrowPf)
            {
                temp = Mathf.CeilToInt((weaponStats.Manacost + weaponStats.ArrowPf.GetComponent<Arrow>().Manacost) / weaponStats.ManaRate);
            }
            else
            {
                temp = Mathf.CeilToInt(weaponStats.Manacost / weaponStats.ManaRate);
            }

            if(stats.Mana < temp)
            {
                if(!ChangeWeapon(true))
                {
                    return Vector3.zero;
                }
            }
        }

        if(!weaponStats.Melee)      //is a ranged weapon
        {
            if(!weaponStats.ArrowPf)        //Bow doesnt have Arrow equipped
            {
                List<int> Arrows = inventory.GetItems(1, 4, weaponStats.NeededArrowType);
                if(Arrows.Count == 0)       //there are no Arrow substitutes
                {
                    if(!ChangeWeapon())
                    {
                        return Vector3.zero;
                    }
                }
                else
                {
                    weaponStats.ChangeArrow( null, inventory.GetItem(1, Arrows[0]));
                }
            }

            float tempV = weaponStats.ArrowPf.GetComponent<Rigidbody>().mass / weaponStats.Force;
            Vector3 targetPos = target.transform.position + (target.transform.position - transform.position).magnitude * tempV * target.GetComponent<Rigidbody>().velocity;
            return checkRays(targetPos, Vector3.zero, weaponStats);     //returning predicted TargetPos which can be hitted
        }
        else
        {
            return new Vector3(1, 1, 1);
        }
    }

    bool checkRay(Vector3 targetPos, Vector3 RayPos, WeaponStats weaponStats = null, bool IgnoreSelf = false, Transform tempTarget = null)
    {
        RaycastHit temp;
        GameObject hitGO;
        if(RayPos == Vector3.zero)
        {
            if(!weaponStats)
            {
                return false;
            }
            RayPos = weaponStats.ArrowSpawn.position;
        }
        if(!tempTarget)
        {
            tempTarget = target;
        }

        if(IgnoreSelf)
        {
            RaycastHit[] hits = Physics.RaycastAll(RayPos, targetPos - RayPos, hitDistance, hitLayerMask);
            foreach(RaycastHit hit in hits)
            {
                hitGO = hit.collider.gameObject;
                while(hitGO != null)                    //search for hittable Target
                {
                    if(hitGO.transform == tempTarget)       //found hittable Target
                    {
                        return true;
                    }
                    else if(hitGO == gameObject)        //hit self, so skip
                    {
                        break;
                    }
                    else if(hitGO.GetComponent<NPCharacter>())
                    {
                        if(hitGO.GetComponent<NPCharacter>().Affiliation == Affiliation)
                        {
                            break;
                        }
                        break;
                    }
                    else if(hitGO.transform.parent)     //can still search further in the hierarchy
                    {
                        hitGO = hitGO.transform.parent.gameObject;
                    }
                    else        //could not find hittable Target
                    {
                        return false;
                    }
                }
            }
        }
        else
        {
            if(Physics.Raycast(RayPos, targetPos - RayPos, out temp, hitDistance, hitLayerMask))
            {
                hitGO = temp.collider.gameObject;
                while(hitGO != null)                //search for hittable Target
                {
                    if(hitGO.transform == tempTarget)     //found hittable Target
                    {
                        return true;
                    }
                    else if(hitGO.transform.parent)     //can still search further in the hierarchy
                    {
                        hitGO = hitGO.transform.parent.gameObject;
                    }
                    else        //could not find hittable Target
                    {
                        return false;
                    }
                }
            }
        }
        
        return false;
    }

    public bool ChangeWeapon(bool mana = false)
    {
        int tempType;
        int tempSlot;
        
        if(inventory.RightParent.transform.childCount > 0)
        {
            if(inventory.RightParent.transform.GetChild(0).GetComponent<InvItem>() == null)
            {
                tempType = 0;
                tempSlot = 0;
                Destroy(inventory.RightParent.transform.GetChild(0).gameObject);
            }
            else if(inventory.RightParent.transform.GetChild(0).GetComponent<InvItem>().Temporary)
            {
                tempType = 0;
                tempSlot = 0;
                Destroy(inventory.RightParent.transform.GetChild(0).gameObject);
            }
            else
            {
                tempSlot = 1;
            
                if(inventory.RightParent.transform.GetChild(0).gameObject.GetComponent<WeaponStats>().TwoHanded)
                {
                    return false;
                }
                
                if(inventory.RightParent.transform.GetChild(0).gameObject.GetComponent<WeaponStats>().Melee)
                {
                    tempType = 1;
                }
                else
                {
                    tempType = 3;
                }
            }
        }
        else if(inventory.MiddleParent.transform.childCount > 0)
        {
            if(inventory.MiddleParent.transform.GetChild(0).GetComponent<InvItem>() == null)
            {
                tempType = 0;
                tempSlot = 0;
                Destroy(inventory.MiddleParent.transform.GetChild(0).gameObject);
            }
            else if(inventory.MiddleParent.transform.GetChild(0).GetComponent<InvItem>().Temporary)
            {
                tempType = 0;
                tempSlot = 0;
                Destroy(inventory.MiddleParent.transform.GetChild(0).gameObject);
            }
            else
            {
                tempSlot = 3;
            
                if(!inventory.MiddleParent.transform.GetChild(0).gameObject.GetComponent<WeaponStats>().TwoHanded)
                {
                    return false;
                }
                
                if(inventory.MiddleParent.transform.GetChild(0).gameObject.GetComponent<WeaponStats>().Melee)
                {
                    tempType = 2;
                }
                else
                {
                    tempType = 4;
                }
            }
        }
        else
        {
            tempType = 0;
            tempSlot = 0;
        }
        inventory.PopSlot(tempSlot, 1);
        
        List<int> Weapons = inventory.GetItems(1, 1, tempType);
        if(Weapons.Count == 0)      //there are no preferred Weapon substitutes
        {
            Weapons = inventory.GetItems( 1, 1, 0);
            if(Weapons.Count == 0)      //there are no other Weapons available
            {
                return false;
            }
        }

        WeaponStats tempStats;
        List<GameObject> tempWeapons = new();
        for(int i = 0; i < Weapons.Count; i++)
        {
            tempStats = inventory.GetItem(1, Weapons[i]).GetComponent<WeaponStats>();

            if(!tempStats.GetComponent<InvItem>().NPCUsable)     //skip this item
            {
                continue;
            }

            if(tempStats.ArrowPf)
            {
                if(tempStats.ManaRate != 0)           //weapon uses Mana
                {
                    if(tempType > 2)        //is a ranged Weapon
                    {
                        if(stats.Mana >= Mathf.CeilToInt((tempStats.Manacost + tempStats.ArrowPf.GetComponent<Arrow>().Manacost) / tempStats.ManaRate))
                        {
                            tempWeapons.Add(tempStats.gameObject);
                        }
                    }
                    else
                    {
                        if(stats.Mana >= Mathf.CeilToInt(tempStats.Manacost / tempStats.ManaRate))
                        {
                            tempWeapons.Add(tempStats.gameObject);
                        }
                    }
                }
                else
                {
                    tempWeapons.Add(tempStats.gameObject);
                }
            }
        }

        if(tempWeapons.Count > 0)
        {
            GameObject result = bestWeapon(tempWeapons);
            if(result.GetComponent<WeaponStats>().TwoHanded)
            {
                inventory.AddToSlot(inventory.inventory.IndexOf(result), 3, 1);
            }
            else
            {
                inventory.AddToSlot(inventory.inventory.IndexOf(result), 1, 1);
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    private void Regenerate(int type)
    {
        List<int> itemNumbers = inventory.GetItems(1,5);
        FoodStats temp;
        FoodStats result = null;
        for(int i = 0; i < itemNumbers.Count; i++)
        {
            temp = inventory.GetItem(1,itemNumbers[i]).GetComponent<FoodStats>();
            if(!temp.GetComponent<InvItem>().NPCUsable)     //skip this item
            {
                continue;
            }
            
            if(type == 1)       //regenerate Health
            {
                if(temp.HP > 0)
                {
                    if(!result)
                    {
                        result = temp;
                    }
                    if(temp.HP <= 2*stats.MaxHitpoints)
                    {
                        if(Mathf.Abs(temp.HP + stats.Hitpoints - stats.MaxHitpoints) > Mathf.Abs(result.HP + stats.Hitpoints - stats.MaxHitpoints))
                        {
                            result = temp;
                        }
                    }
                }
            }
            else if(type == 2)       //regenerate Mana
            {
                if(temp.Mana > 0)
                {
                    if(!result)
                    {
                        result = temp;
                    }
                    if(temp.Mana <= 2*stats.MaxMana)
                    {
                        if(Mathf.Abs(temp.Mana + stats.Mana - stats.MaxMana) > Mathf.Abs(result.Mana + stats.Mana - stats.MaxMana))
                        {
                            result = temp;
                        }
                    }
                }
            }
        }

        if(result != null)
        {
            animator.SetTrigger("Drink");
            waitingTime = 1.4f;
            Consumable = result.gameObject;
        }
    }

    Vector3 betterPosition( Vector3 targetPos)
    {
        float distance = (targetPos - transform.position).magnitude;

        if(distance > hitDistance - 3)      //target is too close to the edge of the viewfield
        {
            Debug.Log("Distance to long: " + distance);
            return Vector3.zero;
        }

        float angle;
        Vector3 newPos;

        for(int i = 0; i < 16; i ++)
        {
            angle = Mathf.Atan2(transform.position.z - targetPos.z, transform.position.x - targetPos.x) + (1 - 2 * i%2) * Mathf.PI * Mathf.CeilToInt(i/2) / 10;
            newPos = targetPos + new Vector3( distance * Mathf.Cos(angle), 0, distance * Mathf.Sin(angle));
            if(checkRays(targetPos, newPos) != Vector3.zero)
            {
                return newPos;
            }
        }
        return Vector3.zero;
    }

    Vector3 checkRays(Vector3 targetPos, Vector3 RayPos, WeaponStats weaponStats = null, bool IgnoreSelf = false, Transform tempTarget = null)
    {
        if(checkRay(targetPos, RayPos, weaponStats, IgnoreSelf, tempTarget))
        {
            return targetPos;
        }
        else if(checkRay(targetPos + new Vector3(0, 0.75f, 0), RayPos, weaponStats, IgnoreSelf, tempTarget))
        {
            return targetPos + new Vector3(0, 0.75f, 0);
        }
        else if(checkRay(targetPos + new Vector3(0, -0.75f, 0), RayPos, weaponStats, IgnoreSelf, tempTarget))
        {
            return targetPos + new Vector3(0, -0.75f, 0);
        }
        else
        {
            Vector3 temp = Vector3.Cross(new Vector3(0,1,0), targetPos - transform.position).normalized;
            
            if(checkRay(targetPos + new Vector3(0, 0.5f, 0) + temp * 0.15f, RayPos, weaponStats, IgnoreSelf, tempTarget))
            {
                return targetPos + new Vector3(0, 0.5f, 0) + temp * 0.15f;
            }
            else if(checkRay(targetPos + new Vector3(0, 0.5f, 0) - temp * 0.15f, RayPos, weaponStats, IgnoreSelf, tempTarget))
            {
                return targetPos + new Vector3(0, 0.5f, 0) - temp * 0.15f;
            }
            else if(checkRay(targetPos + new Vector3(0, -0.5f, 0) + temp * 0.15f, RayPos, weaponStats, IgnoreSelf, tempTarget))
            {
                return targetPos + new Vector3(0, -0.5f, 0) + temp * 0.15f;
            }
            else if(checkRay(targetPos + new Vector3(0, -0.5f, 0) - temp * 0.15f, RayPos, weaponStats, IgnoreSelf, tempTarget))
            {
                return targetPos + new Vector3(0, -0.5f, 0) - temp * 0.15f;
            }
            return Vector3.zero;
        }
    }

    GameObject bestWeapon(List<GameObject> Weapons)
    {
        if(Weapons.Count <= 0)
        {
            return null;
        }

        GameObject best = null;
        foreach(GameObject weapon in Weapons)
        {
            if(!best)
            {
                best = weapon;
            }
            else if(weapon.GetComponent<Damage>().GetDescriptionDamage(stats, weapon.GetComponent<WeaponStats>())[1] > best.GetComponent<Damage>().GetDescriptionDamage(stats, best.GetComponent<WeaponStats>())[1])
            {
                best = weapon;
            }
        }
        return best;
    }

    void SetTarget(Transform newTarget)
    {
        if(!newTarget)
        {
            Debug.Log("No Target found!");
            return;
        }
        if(newTarget.transform.position == Vector3.zero)
        {
            Debug.Log("Target has bad Pos!");
            return;
        }

        if(maxAngle < Vector3.Angle(Viewfield.forward, newTarget.transform.position - Viewfield.position))      //not inside the Viewfield
        {
            if((newTarget.transform.position - Viewfield.position).magnitude < 1.5f)    //close enough that the Target will still be detected
            {

            }
            else
            {
                return;
            }
        }

        if(checkRays(newTarget.transform.position, Viewfield.position, null, true, newTarget.transform) != Vector3.zero)
        {
            target = newTarget;
        }
    }
}
