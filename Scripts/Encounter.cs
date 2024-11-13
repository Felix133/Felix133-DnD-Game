using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Encounter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if(!SpawnPos)
        {
            SpawnPos = transform;
        }
        if(NPCs.Length == 0)
        {
            Debug.LogWarning("No NPCs specified, deleting self!");
            Destroy(gameObject);
        }
    }

    public int Affiliation;
    public GameObject[] NPCs;
    public Transform SpawnPos;
    public float Spawnradius;

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider coll)
    {
        GameObject HitObject = coll.gameObject;
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
        
        if(HitObject.tag == "Player")
        {
            GameObject tempGO;
            foreach(GameObject npc in NPCs)
            {
                tempGO = Instantiate(npc, SpawnPos.position + new Vector3(Spawnradius * Random.Range(-1.0f, 1.0f), 0, Spawnradius * Random.Range(-1.0f, 1.0f)), Quaternion.Euler(Vector3.zero));
                tempGO.GetComponent<NPCharacter>().Affiliation = Affiliation;
            }
            Destroy(gameObject);
        }
    }
}
