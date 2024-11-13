using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if(InvItem)
        {
            return;
        }
        Destroy(gameObject, Range);
        ArrowRigidbody.useGravity = !IgnoreGravity;
        weaponStats = Origin.GetComponent<WeaponStats>();
        transform.position += new Vector3(ArrowPosChange.x * Random.Range(-1.0f, 1.0f), ArrowPosChange.y * Random.Range(-1.0f, 1.0f), ArrowPosChange.z * Random.Range(-1.0f, 1.0f));
        transform.eulerAngles += new Vector3(ArrowRotChange.x * Random.Range(-1.0f, 1.0f), ArrowRotChange.y * Random.Range(-1.0f, 1.0f), ArrowRotChange.z * Random.Range(-1.0f, 1.0f));
    }

    [Header("Damage")]
    public GameObject Explosion;
    public Damage DamageScript;

    [Header("Stats")]
    public float LocalWeight;
    public Rigidbody ArrowRigidbody;
    public bool InvItem;
    public GameObject ArrowPf;          //Prefab
    public int ArrowType;

    [Header("Firing Stats")]
    public float Range;
    public float AttackCooldown;        //how many seconds between attacks
    public LayerMask layerMask;
    public int ArrowNumber;             //how many Arrows will be fired
    public Vector3 ArrowRotChange;      //randomise firing Rotation
    public Vector3 ArrowPosChange;      //randomise starting Position
    public float TimeBetweenArrows;     //if multiple Arrows are fired, the time between each
    public int Manacost;

    [Header("Flying Stats")]
    public bool IgnoreGravity;
    public bool Homing;
    public bool IgnoreObstacles;
    public bool Piercing;
    
    [Header("Runtime")]
    public GameObject Target;
    public GameObject Origin;
    public float Force;
    public WeaponStats weaponStats;
    public Vector3 TargetPos;
    bool shot = false;
    Vector3 lastPos;
    public bool hitted = false;
    public List<GameObject> Hits;
    private Collider lastCollider = null;
    GameObject HitObject;
    Vector3 targetDirection;

    void Update()
    {
        if(InvItem)
        {
            return;
        }
        
        if(hitted)
        {
            return;
        }
        
        if(!shot)       //es wurde noch nicht abgeschossen
        {
            if(Force != 0)
            {
                shot = true;
                Vector3 theForce = this.transform.forward * Force;
                Debug.Log("The Force: " + theForce);
                ArrowRigidbody.AddForceAtPosition( theForce, this.transform.position, ForceMode.Impulse);
            }
        }
        else        //es wurde schon abgeschossen
        {
            if(Homing)
            {
                if(Target != null)
                {
                    if(Target != HitObject)
                    {
                        ArrowRigidbody.velocity = (Target.transform.position - transform.position).normalized * Force / ArrowRigidbody.mass;       //rotate the direction of flight
                    }
                    else                    //if Target was hit, resume normal flight
                    {
                        Homing = false;
                    }
                }
                else
                {
                    ArrowRigidbody.velocity = (TargetPos - transform.position).normalized * Force / ArrowRigidbody.mass;       //rotate the direction of flight
                }
            }
            this.transform.rotation = Quaternion.LookRotation(ArrowRigidbody.velocity);       //rotate in flight direction

            RaycastHit hit;
            if(Physics.Raycast(lastPos, transform.position - lastPos, out hit, (transform.position - lastPos).magnitude, layerMask))    //wenn er etwas getroffen hat
		    {
                HitCollider(hit.collider, hit.point, (transform.position - lastPos).normalized);
            }
        }
        lastPos = transform.position;
    }

    public void HitCollider(Collider hittedColl, Vector3 hitPos, Vector3 tempDirection)
    {
        if(hittedColl.isTrigger)
        {
            return;
        }
        
        if(InvItem)
        {
            return;
        }
        
        if(hittedColl == lastCollider)
        {
            return;
        }

        HitObject = hittedColl.gameObject;
        while(HitObject != null)
        {
            if(HitObject.GetComponent<Stats>())         //wenn das getroffene Objekt Schaden erhalten kann (nicht die Map)
            {
                bool temp = true;
                for(int i = 0; i < Hits.Count; i++)
                {
                    if(Hits[i] == HitObject)
                    {
                        temp = false;
                    }
                }
                
                if(temp)                            //das Objekt wurde vorher noch nicht getroffen
                {
                    if(Piercing || IgnoreObstacles)
                    {
                        Hits.Add(HitObject);
                        if(Homing)
                        {
                            if(Target == HitObject)
                            {
                                Homing = false;
                                ArrowRigidbody.velocity = tempDirection * Force / ArrowRigidbody.mass;
                            }
                            if(Target == null)
                            {
                                Homing = false;
                                ArrowRigidbody.velocity = tempDirection * Force / ArrowRigidbody.mass;
                            }
                        }
                        
                        lastCollider = hittedColl;
                        DamageScript.DoDamage(HitObject.GetComponent<Stats>(), weaponStats.parentStats, weaponStats, weaponStats.SchadensMod, weaponStats.ETW0 + Origin.GetComponent<Damage>().Enchantment);
                        if(Explosion != null)
                        {
                            GameObject currentExplosion = Instantiate(Explosion, transform.position, transform.rotation);
                            currentExplosion.GetComponent<Explosion>().Origin = Origin;
                        }
                        
                        if(!Piercing)
                        {
                            hitted = true;
                            ArrowRigidbody.isKinematic = true;
                            transform.position = hitPos;
                            transform.SetParent(HitObject.transform, true);
                        }
                        return;
                    }
                    else
                    {
                        hitted = true;
                        ArrowRigidbody.isKinematic = true;
                        transform.position = hitPos;
                        transform.SetParent(HitObject.transform, true);
                        
                        DamageScript.DoDamage(HitObject.GetComponent<Stats>(), weaponStats.parentStats, weaponStats, weaponStats.SchadensMod, weaponStats.ETW0 + Origin.GetComponent<Damage>().Enchantment);
                        if(Explosion != null)
                        {
                            GameObject currentExplosion = Instantiate(Explosion, hitPos, transform.rotation);
                            currentExplosion.GetComponent<Explosion>().Origin = Origin;
                        }
                        return;
                    }
                }
            }
            else if(HitObject.layer == 3)       //wenn die Umgebung getroffen wird
            {
                if(!IgnoreObstacles)            //und diese nicht ignoriert wird
                {
                    hitted = true;
                    ArrowRigidbody.isKinematic = true;
                    transform.SetParent(HitObject.transform, true);
                    transform.position = hitPos;
                    if(Explosion != null)
                    {
                        GameObject currentExplosion = Instantiate(Explosion, transform.position, transform.rotation);
                        currentExplosion.GetComponent<Explosion>().Origin = Origin;
                    }
                    return;
                }
            }

            if(HitObject.transform.parent)
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