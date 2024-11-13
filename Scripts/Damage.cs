using UnityEngine;

public class Damage : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    [Header("Damage")]
    public int ExtraDamage;
    public int W2;
    public int W3;
    public int W4;
    public int W6;
    public int W8;
    public int W10;
    public int W12;
    float DamageMult = 1;               //Factor of the Damage because of Statuseffects
    public float DamageLvlMult = 1;     //Factor of the Damage because of Level

    [Header("Stats")]
    public int Enchantment;             //boost in Damage and Accuracy (only enchanted weapons can be wielded by mages and wizards)
    public int DamageType;              //Damage done by a sharp point, a sharp blade or the sheer force of the blow
    public float DmgKept;               //if you miss, what proportion of the damage you do

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DoDamage(Stats HitStats, Stats parentStats, WeaponStats weaponStats, int SchadensMod, int ETW0)
    {
        DamageMult = weaponStats.DamageMult;
                
        int Trefferwurf = Random.Range(1, 21);
        if(Trefferwurf == 1)        //schlägt immer fehl
        {
            float crit = Random.Range(1,101);
            Debug.Log("Crit Fumble: " + crit);
            
            if(1 <= crit && crit <= 28)
            {
                parentStats.ApplyStatusEffect( 10, 1, 1, 0, 0, 0.8f, 1, 0, 1, 0);     //drop weapon
            }
            else if(29 <= crit && crit <= 58)
            {
                parentStats.ApplyStatusEffect( 10, 1, 0.5f, 0, 0, 0.8f, 1, 0, 1, 0);     //Fall, lose next round
            }
            else if(59 <= crit && crit <= 78)
            {
                parentStats.ApplyStatusEffect( 10, 1, 1, 0, 0, 0.8f, 1, 0, 1, 0);     //Stumble, hit self; roll for crit
                DoDamage(parentStats, parentStats, weaponStats, SchadensMod, ETW0);
            }
            else if(79 <= crit && crit <= 88)
            {
                parentStats.ApplyStatusEffect( 10, 1, 1, 0, 0, 0.8f, 1, 0, 1, 0);     //Stumble, extra attack for opponent
                HitStats.ApplyStatusEffect( 10, 1, 2, 0, 0, 1, 1, 0, 1, 0);
            }
            else if(89 <= crit && crit <= 99)
            {
                parentStats.ApplyStatusEffect( 10, 1, 0.3f, 0, 0, 0.6f, 1, 0, 1, 0);     //Fall, lose next round and drop weapon
            }
            else if(crit == 100)
            {
                parentStats.ApplyStatusEffect( 10, 1, 1, 0, 0, 0.6f, 1, 0, 1, 0);     //hit closest ally within 10 feet (or hit self)
                DoDamage(parentStats, parentStats, weaponStats, SchadensMod, ETW0);
            }
            
            if(parentStats.gameObject.layer == 6)
            {
                scriptDamagePopup.Create(MainMenu.mainMenu.transform, "Miss", 1, Vector3.zero);
            }
            else if(HitStats.gameObject.layer == 6)
            {
                scriptDamagePopup.Create(MainMenu.mainMenu.transform, "Miss", 1, new Vector3(0, 4f, 0));
            }
        }
        else if(Trefferwurf == 20)       //verursacht Extraschaden oder tötet sofort
        {
            float crit = Random.Range(1,101);
            int Schaden = 0;
            
            if(1 <= crit && crit <= 20)
            {
                Schaden = (int) ((MaxDamage(parentStats, weaponStats) + Enchantment + SchadensMod) * Mathf.Pow(DamageLvlMult, parentStats.Level - 1));
            }
            else if(21 <= crit && crit <= 65)
            {
                Schaden = BaseDamage(parentStats, weaponStats) * 2;
            }
            else if(66 <= crit && crit <= 83)
            {
                Schaden = BaseDamage(parentStats, weaponStats) * 3;
            }
            else if(84 <= crit && crit <= 93)
            {
                Schaden = BaseDamage(parentStats, weaponStats) * 2;
                HitStats.ApplyStatusEffect( 200, 1, 1, 2, 0, 1, 1, 0, 1, 0);
            }
            else if(94 <= crit && crit <= 95)
            {
                Schaden = BaseDamage(parentStats, weaponStats) * 4;
            }
            else if(96 <= crit && crit <= 98)
            {
                Schaden = MaxDamage(parentStats, weaponStats) * 2;
            }
            else if(crit == 99)
            {
                Schaden = MaxDamage(parentStats, weaponStats) * 2;
                HitStats.ApplyStatusEffect( 10, 1, 1, 0, 0, 0.8f, 1, 0, 1, 0);
            }
            else if(crit == 100)
            {
                if(Random.Range(1,21) > HitStats.Constitution)
                {
                    Schaden = (int) (HitStats.Hitpoints * 2);
                    if(parentStats.gameObject.layer == 6)
                    {
                        scriptDamagePopup.Create(MainMenu.mainMenu.transform, "" + Schaden, 3, Vector3.zero);
                    }
                    else if(HitStats.gameObject.layer == 6)
                    {
                        scriptDamagePopup.Create(MainMenu.mainMenu.transform, "" + Schaden, 1, new Vector3(0, 4f, 0));
                    }
                }
                else
                {
                    Schaden = (int) HitStats.Hitpoints;
                    if(parentStats.gameObject.layer == 6)
                    {
                        scriptDamagePopup.Create(MainMenu.mainMenu.transform, "" + Schaden, 3, Vector3.zero);
                    }
                    else if(HitStats.gameObject.layer == 6)
                    {
                        scriptDamagePopup.Create(MainMenu.mainMenu.transform, "" + Schaden, 3, new Vector3(0, 4f, 0));
                    }
                }
            }

            if(crit != 100f)
            {
                Schaden = Mathf.RoundToInt(Schaden * DamageMult * Mathf.Pow(DamageLvlMult, parentStats.Level - 1));
                
                if(Schaden >= 50 && Random.Range(1f,20f) < HitStats.RWLähmungGiftTodesmagie || (96 <= crit && crit <= 99))       //ab 50 Schaden erfährt das Ziel einen Schock
                {
                    Debug.Log("TW= " + Trefferwurf + "; Crit= " + crit + "; Instant Death (Schock): " + HitStats.Hitpoints);
                    if(parentStats.gameObject.layer == 6)
                    {
                        scriptDamagePopup.Create(MainMenu.mainMenu.transform, "" + HitStats.Hitpoints, 3, Vector3.zero);
                    }
                    else if(HitStats.gameObject.layer == 6)
                    {
                        scriptDamagePopup.Create(MainMenu.mainMenu.transform, "" + HitStats.Hitpoints, 3, new Vector3(0, 4f, 0));
                    }
                    Schaden = HitStats.Hitpoints;
                }
                else                //nicht über 50 Schaden oder Rettungswurf geglückt
                {
                    Debug.Log("TW= " + Trefferwurf + "; Crit= " + crit + "; Schaden = " + Schaden);
                    if(parentStats.gameObject.layer == 6)
                    {
                        scriptDamagePopup.Create(MainMenu.mainMenu.transform, "" + Schaden, 2, Vector3.zero);
                    }
                    else if(HitStats.gameObject.layer == 6)
                    {
                        scriptDamagePopup.Create(MainMenu.mainMenu.transform, "" + Schaden, 2, new Vector3(0, 4f, 0));
                    }
                }  
            }
            HitStats.LastHittedByGO = parentStats.gameObject;
            HitStats.Hitpoints -= Schaden;
        }
        else if(Trefferwurf >= (ETW0 - HitStats.RK + Enchantment))         //normaler geglückter Angriff
        {
            int Schaden = BaseDamage(parentStats, weaponStats);
            if(Schaden >= 50 && Random.Range(1f,20f) < HitStats.RWLähmungGiftTodesmagie)       //ab 50 Schaden erfährt das Ziel einen Schock
            {
                Debug.Log("TW= " + Trefferwurf + "; InstantDeath (Shock): " + Schaden);
                if(parentStats.gameObject.layer == 6)
                {
                    scriptDamagePopup.Create(MainMenu.mainMenu.transform, "" + HitStats.Hitpoints, 3, Vector3.zero);
                }
                else if(HitStats.gameObject.layer == 6)
                {
                    scriptDamagePopup.Create(MainMenu.mainMenu.transform, "" + HitStats.Hitpoints, 3, new Vector3(0, 4f, 0));
                }
                HitStats.LastHittedByGO = parentStats.gameObject;
                HitStats.Hitpoints = 0;
            }
            else                //nicht über 50 Schaden oder Rettungswurf geglückt
            {
                Debug.Log("TW= " + Trefferwurf + "; Schaden = " + Schaden);
                HitStats.LastHittedByGO = parentStats.gameObject;
                HitStats.Hitpoints = HitStats.Hitpoints - Schaden;
                if(parentStats.gameObject.layer == 6)
                {
                    scriptDamagePopup.Create(MainMenu.mainMenu.transform, "" + Schaden, 1, Vector3.zero);
                }
                else if(HitStats.gameObject.layer == 6)
                {
                    scriptDamagePopup.Create(MainMenu.mainMenu.transform, "" + Schaden, 1, new Vector3(0, 4f, 0));
                }
            }
        }
        else        //zu geringer Trefferwurf
        {
            Debug.Log("TW= " + Trefferwurf + "; Missed!");
            if(parentStats.gameObject.layer == 6)
            {
                scriptDamagePopup.Create(MainMenu.mainMenu.transform, "Miss", 1, Vector3.zero);
            }
            else if(HitStats.gameObject.layer == 6)
            {
                scriptDamagePopup.Create(MainMenu.mainMenu.transform, "Miss", 1, new Vector3(0, 4f, 0));
            }
        }
    }

    int BaseDamage(Stats parentStats, WeaponStats weaponStats)        //used to determine the basedamage for weapons (without f.ex. critical hits)
    {
        int temp = ExtraDamage + Enchantment + weaponStats.SchadensMod;
        for(int i = W2; i > 0; i--)
        {
            temp += Random.Range(1, 3);     //Random.Range(1, 3) gibt entweder 1 oder 2 aus
        }
        for(int i = W3; i > 0; i--)
        {
            temp += Random.Range(1, 4);
        }
        for(int i = W4; i > 0; i--)
        {
            temp += Random.Range(1, 5);
        }
        for(int i = W6; i > 0; i--)
        {
            temp += Random.Range(1, 7);
        }
        for(int i = W8; i > 0; i--)
        {
            temp += Random.Range(1, 9);
        }
        for(int i = W10; i > 0; i--)
        {
            temp += Random.Range(1, 11);
        }
        for(int i = W12; i > 0; i--)
        {
            temp += Random.Range(1, 13);
        }

        temp = Mathf.FloorToInt(temp * Mathf.Pow(DamageLvlMult, parentStats.Level - 1));
        return temp;
    }

    public int MaxDamage(Stats parentStats, WeaponStats weaponStats)        //used to determine the maxdamage for weapons (without f.ex. critical hits)
    {
        int temp = ExtraDamage + W2*2 + W3*3 + W4*4 + W6*6 + W8*8 + W10*10 + W12*12 + Enchantment + weaponStats.SchadensMod;
        if(!gameObject.GetComponent<WeaponStats>())
        {
            temp += weaponStats.gameObject.GetComponent<Damage>().Enchantment;
        }
        temp = Mathf.FloorToInt(temp * weaponStats.DamageMult * Mathf.Pow(DamageLvlMult, parentStats.Level - 1));

        return temp;
    }

    public float[] GetDescriptionDamage(Stats parentStats, WeaponStats weaponStats)
    {
        float[] result = new float[3];
        float schadensMod = 0;
        int weaponEnchantment = 0;
        if(weaponStats)
        {
            DamageMult = weaponStats.DamageMult;
            schadensMod += weaponStats.SchadensMod;
            weaponEnchantment += weaponStats.gameObject.GetComponent<Damage>().Enchantment;
        }
        else if(DamageMult == 0)
        {
            DamageMult = 1;
        }

        if(gameObject.GetComponent<WeaponStats>())          //ist an einer Waffe, nicht an einem Pfeil
        {
            if(gameObject.GetComponent<WeaponStats>().Melee)
            {
                result[0] = ExtraDamage + W2*1 + W3*1 + W4*1 + W6*1 + W8*1 + W10*1 + W12*1 + Enchantment + schadensMod;
                result[0] = Mathf.FloorToInt(result[0] * DamageMult * Mathf.Pow(DamageLvlMult, parentStats.Level - 1));

                result[1] = ExtraDamage + W2*2 + W3*3 + W4*4 + W6*6 + W8*8 + W10*10 + W12*12 + Enchantment + schadensMod;
                result[1] = Mathf.FloorToInt(result[1] * DamageMult * Mathf.Pow(DamageLvlMult, parentStats.Level - 1));

                result[2] = ExtraDamage + W2*1.5f + W3*2 + W4*2.5f + W6*3.5f + W8*4.5f + W10*5.5f + W12*6.5f + Enchantment + schadensMod;
                result[2] = Mathf.FloorToInt(result[2] * DamageMult * Mathf.Pow(DamageLvlMult, parentStats.Level - 1));
            }
            
            Debug.Log("Weapon: " + result[0] + "; " + result[1] + "; " + result[2]);
            if(weaponStats.ArrowPf)
            {
                if(weaponStats.ArrowInvGO)
                {
                    float[] temp = weaponStats.ArrowPf.GetComponent<Damage>().GetDescriptionDamage(parentStats, weaponStats);
                    result[0] += temp[0];
                    result[1] += temp[1];
                    result[2] += temp[2];
                }
                else if(weaponStats.UseItself)
                {
                    float[] temp = weaponStats.ArrowPf.GetComponent<Damage>().GetDescriptionDamage(parentStats, weaponStats);
                    result[0] += temp[0];
                    result[1] += temp[1];
                    result[2] += temp[2];
                }
            }
            Debug.Log("Weapon: " + result[0] + "; " + result[1] + "; " + result[2]);
        }
        else if(gameObject.GetComponent<Arrow>())
        {
            Arrow arrowStats = gameObject.GetComponent<Arrow>();
            
            result[0] = ExtraDamage + W2*1 + W3*1 + W4*1 + W6*1 + W8*1 + W10*1 + W12*1 + Enchantment + schadensMod + weaponEnchantment;
            result[0] = arrowStats.ArrowNumber * Mathf.FloorToInt(result[0] * DamageMult * Mathf.Pow(DamageLvlMult, parentStats.Level - 1));
            
            result[1] = ExtraDamage + W2*2 + W3*3 + W4*4 + W6*6 + W8*8 + W10*10 + W12*12 + Enchantment + schadensMod + weaponEnchantment;
            result[1] = arrowStats.ArrowNumber * Mathf.FloorToInt(result[1] * DamageMult * Mathf.Pow(DamageLvlMult, parentStats.Level - 1));

            result[2] = ExtraDamage + W2*1.5f + W3*2 + W4*2.5f + W6*3.5f + W8*4.5f + W10*5.5f + W12*6.5f + Enchantment + schadensMod + weaponEnchantment;
            result[2] = arrowStats.ArrowNumber * Mathf.FloorToInt(result[2] * DamageMult * Mathf.Pow(DamageLvlMult, parentStats.Level - 1));

            Debug.Log("Projectile: " + result[0] + "; " + result[1] + "; " + result[2]);
            if(arrowStats.Explosion != null)
            {
                float[] temp = arrowStats.Explosion.GetComponent<Damage>().GetDescriptionDamage(parentStats, weaponStats);
                result[0] += arrowStats.ArrowNumber * temp[0];
                result[1] += arrowStats.ArrowNumber * temp[1];
                result[2] += arrowStats.ArrowNumber * temp[2];
            }
            Debug.Log("Projectile: " + result[0] + "; " + result[1] + "; " + result[2]);
        }
        else                    //Damage of the Explosion
        {
            Explosion explosionStats = gameObject.GetComponent<Explosion>();
            
            result[0] = ExtraDamage + W2*1 + W3*1 + W4*1 + W6*1 + W8*1 + W10*1 + W12*1 + Enchantment + schadensMod + weaponEnchantment;
            result[0] = Mathf.FloorToInt(result[0] * DamageMult * Mathf.Pow(DamageLvlMult, parentStats.Level - 1));

            result[1] = ExtraDamage + W2*2 + W3*3 + W4*4 + W6*6 + W8*8 + W10*10 + W12*12 + Enchantment + schadensMod + weaponEnchantment;
            result[1] = Mathf.FloorToInt(result[1] * DamageMult * Mathf.Pow(DamageLvlMult, parentStats.Level - 1));

            result[2] = ExtraDamage + W2*1 + W3*1.5f + W4*2 + W6*3 + W8*4 + W10*5 + W12*6 + Enchantment + schadensMod + weaponEnchantment;
            result[2] = Mathf.FloorToInt(result[2] * DamageMult * Mathf.Pow(DamageLvlMult, parentStats.Level - 1));
            Debug.Log("Explosion: " + result[0] + "; " + result[1] + "; " + result[2]);
        }
        return result;
    }
}
