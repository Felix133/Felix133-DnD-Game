using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestingStats : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public string Name;
    public float skippedRelativeTime;       //portion of a day
    public float skippedTime;               //in seconds
    [Header("Instant Effect")]
    public int HP;              //Number of Hitpoints regenerated
    public float HP2;           //Percentage of Hitpoints regenerated
    public int Mana;            //Number of Mana regenerated
    public float Mana2;         //Percentage of Mana regenerated

    [Header("Lasting Effect")]
    public float duration = 0;
    public float damageMult = 1;
    public float attackSpeedMult = 1;    
    public int tempRK = 0;             
    public int tempETW0 = 0;
    public float speedMult = 1;
    public float HitpointsMult = 1;      
    public float LPRegen = 0;
    public float ManaMult = 1;           
    public float ManaRegen = 0;
    // Update is called once per frame
    void Update()
    {
        
    }
}
