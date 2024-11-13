using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, 1f);
        weaponStats = Origin.GetComponent<WeaponStats>();
        int parentLevel = weaponStats.parentStats.Level;
        transform.localScale = new Vector3(Mathf.Pow(SizeLvlMult, parentLevel - 1), Mathf.Pow(SizeLvlMult, parentLevel - 1), Mathf.Pow(SizeLvlMult, parentLevel - 1));
    }

    public Damage DamageScript;
    public float SizeLvlMult = 1f;
    [Header("Runtime")]
    public GameObject Origin;
    public List<GameObject> Hits;

    WeaponStats weaponStats;

    void OnTriggerEnter(Collider other)
    {
        GameObject HitObject = other.gameObject;
        while(HitObject != null)
        {
            if(HitObject.GetComponent<Stats>())         //wenn das getroffene Objekt Schaden erhalten kann (nicht die Map)
            {
                for(int i = 0; i < Hits.Count; i++)
                {
                    if(Hits[i] == HitObject)
                    {
                        return;
                    }
                }
                
                Hits.Add(HitObject);
                DamageScript.DoDamage(HitObject.GetComponent<Stats>(), weaponStats.parentStats, weaponStats, weaponStats.SchadensMod, weaponStats.ETW0 + weaponStats.ArrowPf.GetComponent<Damage>().Enchantment);
                return;
            }
            else if(HitObject.layer == 3)       //wenn die Umgebung getroffen wird
            {
                return;
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