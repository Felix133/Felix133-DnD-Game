using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Stats : MonoBehaviour
{
    public string Name;
    public bool Character;          //true:Spieler oder NPC; false:Monster
    public int CharacterClass;      //1:Kämpfer; 2:Paladin; 3:Waldläufer; 4:Zauberkundiger; 5:Spezialist; 6:Kleriker; 7:Druide; 8:Dieb; 9:Barde
    public int Experience;
    public int Level;
    int tempExp = -1;
    public Anzeige Display;         //Displays Name and Hitpoints for the Player

    [Header("Attribute")]
    public int Strength;            //Stärke
    public int Constitution;        //Konstitution
    public int Dexterity;           //Geschicklichkeit
    public int Intelligence;        //Intelligenz
    public int Wisdom;              //Weisheit
    public int Charisma;            //Charisma

    
    [Header("Stats")]
    public int Hitpoints;
    public int MaxHitpoints;
    public GameObject HitpointsTextField2;
    public GameObject HitpointsBar;
    public int Mana;
    public int MaxMana;
    public GameObject ManaBar;
    public GameObject ManaTextField;

    public int RK;                      //Rüstungsklasse (klein ist gut)
    public int BaseRK;                  //RK nur von Rüstung
    public int ShieldRK;                //RK vom Schild (nur wenn getragen)
    public int MagicalBoniRK;           //BonusRK von Ringen etc.
    public int BaseETW0;                //Erforderlicher Trefferwurf gegen RK = 0 um diesen zu treffen (ohne Bonis)

    [Header("Status effects")]
    public float damageMult = 1;            //multipliziert den Schaden
    public float attackSpeedMult = 1;       //multipliziert die vergangene Zeit
    public int tempRK = 0;                  //wird zur RK addiert -> negativ ist gut
    public int tempETW0 = 0;                //wird vom ETW0 subtrahiert -> positiv ist gut
    public float speedMult = 1;             //wird mit Geschwindigkeit multipliziert
    public float HitpointsMult = 1;         //multipliziert Hitpoints und MaxHitpoints
    float LPRegen = 0;                      //regeneriert diesen Wert pro Sekunde an HP
    public float ManaMult = 1;              //multipliziert Mana und MaxHitpoints
    float ManaRegen = 0;                    //regeneriert diesen Wert pro Sekunde an Mana
    private float tempHPReg = 0;            //already regenerated HP (only whole numbers get added to Hitpoints)
    private float tempManaReg = 0;          //already regenerated Mana (only whole numbers get added to Mana)

    public List<float[]> StatusEffects;
    
    [Header("Rettungswürfe")]
    public int RWLähmungGiftTodesmagie;             //erforderlicher Rettungswurf gegen Lähmung, Gift und Todesmagie
    public int RWZauberstäbeSteckenRuten;           //erforderlicher Rettungswurf gegen Zauberstäbe, -stecken und -ruten
    public int RWVersteinerungVerwandlung;          //erforderlicher Rettungswurf gegen Versteinerung und Verwandlung
    public int RWOdemwaffen;                        //erforderlicher Rettungswurf gegen Odemwaffen
    public int RWZauber;                            //erforderlicher Rettungswurf gegen Zauber

    [Header("Stärke")]
    public int StärkeMult;              //Prozentwurf für extra Stärke
    public int MeleeBoniETW0;           //ETW0 Boni beim Nahkampf
    public int MeleeBoniSchaden;        //SchadensBoni beim Nahkampf
    public int Traglast;                //max. Gewicht dass ein Charakter tragen und dabei normal bewegen kann 
    public int Stemmvermögen;           //max. Gewicht dass ein Charakter für kurze Zeit anheben kann
    public int TürenÖffnen;             //Chance eine Tür gewaltsam aufzubrechen (darunter würfeln mit W20)
    public int VerbiegenAnheben;        //Prozentchance eine Stange zu verbiegen oder ein Gatter anzuheben (kann nur einmal versucht werden)

    [Header("Geschicklichkeit")]
    public int Reaktionsmod;            //geringere Chance überrascht zu werden
    public int RangeBoniETW0;           //ETW0 Bonus beim Fernkampf
    public int GeschRK;                 //BonusRK

    [Header("Konstitution")]
    public int BoniTrefferpunkte;       //Extra Trefferpunkte beim Levelaufstieg
    public int KörperlSchock;           //Chance eine Verwandlung o.ä. zu überleben oder nicht das Bewusstsein zu verlieren
    public int Wiedererwecken;          //Chance wiedererweckt werden zu können oder unwiderruflich tot zu sein (bei 0 > TP > 2*MaxTP)
    public int BoniRWGift;              //Bonus auf RWLähmungGiftTodesmagie
    public int Regeneration;            //pro Runde regenerierte Trefferpunkte

    [Header("Intelligenz")]
    public int Sprachen;                //Anzahl von erlernbaren Sprachen
    public int ZauberHöchstgrad;        //höchster erlernbarer Zaubergrad (Magier)
    public int ZauberVerstehen;         //Chance neue Zauber zu verstehen und zu lernen (Magier)
    public int ZauberHöchstzahl;        //höchste Anzahl an Zaubern pro Grad (Magier)
    public int ImmunitätIllusion;       //Immunität gegen Illusionen dieses Grades
    public int BoniMana;

    [Header("Weisheit")]
    public int BoniRWZauber;            //Bonus für den RWZauber gegen Zauber die den Verstand angreifen
    public ulong ExtraZauber;           //Erhöht die Anzahl an erlernbaren Zaubern pro Grad (Priester)
    public int ZauberChance;            //Prozentchance dass ein Zauber versagt (Priester)
    public int ImmunitätZauber;         //Immunität gegen bestimmte Zauber die den Verstand angreifen

    [Header("Charisma")]
    public int MaxGefolgsleute;         //maximale Anzahl an Personen die sich dem Spieler ohne Bezahlung dauerhaft anschließen können
    public int Loyalität;               //Bonus auf Loyalitätswerte von allen Gefolgsleuten (wichtig z.B. bei Moral)
    public int BegegnungsMod;           //positiver einfluss auf Begegnungen (z.B. beim Handel)

    [Header("Runtime")]
    public GameObject LastHittedByGO;   //which parentGO last hitted this GO
    public bool dead = false;
    
    // Start is called before the first frame update
    void Start()
    {
        CheckLvL();
        StatusEffects = new List<float[]>();
    }

    // Update is called once per frame
    void Update()
    {
        if(dead)
        {
            return;
        }
        
        if(Hitpoints <= 0)
        {
            
            dead = true;
            
            GetComponent<Inventory>().DropItems();
            GetComponent<Rigidbody>().isKinematic = true;
            LastHittedByGO.GetComponent<Stats>().Experience += Experience;

            if(gameObject.layer == 6)       //is Player
            {
                GetComponent<Animator>().SetBool("Dead", true);
                Cursor.lockState = CursorLockMode.None;

                for(int i = 0; i < this.transform.childCount; i++)
                {
                    if(this.transform.GetChild(i).gameObject.name != "Camera Rig" || i == 1)
                    {
                        this.transform.GetChild(i).gameObject.SetActive(false);
                    }
                    else if(i != 1)
                    {
                        this.transform.GetChild(i).GetChild(0).gameObject.GetComponent<CameraManager>().PlayerDeath();
                    }
                }

                foreach (MonoBehaviour script in this.gameObject.GetComponents<MonoBehaviour>())
                {
                    if(script != this)
                    {
                        script.enabled = false;
                    }
                }
                this.enabled = false;
                return;
            }
            else
            {
                
            }
        }
        
        float tempHitMult = HitpointsMult - 1;
        float tempManaMult = ManaMult - 1;
        List<float[]> remove = new();
        
        damageMult = 1;         
        attackSpeedMult = 1;    
        tempRK = 0;             
        tempETW0 = 0;
        speedMult = 1;
        HitpointsMult = 1;      
        LPRegen = 0;
        ManaMult = 1;           
        ManaRegen = 0;

        if(StatusEffects != null)
        {
            List<float[]> temp = StatusEffects;
            foreach(float[] effect in temp)
            {
                effect[0] -= Time.deltaTime;
                if(effect[0] <= 0f)
                {
                    damageMult /= effect[1];
                    attackSpeedMult /= effect[2];
                    tempRK -= (int) effect[3];
                    tempETW0 -= (int) effect[4];
                    speedMult /= effect[5];
                    HitpointsMult /= effect[6];
                    LPRegen -= effect[7];
                    ManaMult /= effect[8];
                    ManaRegen -= effect[9];
                    remove.Add(effect);
                }
                else
                {
                    damageMult *= effect[1];
                    attackSpeedMult *= effect[2];
                    tempRK += (int) effect[3];
                    tempETW0 += (int) effect[4];
                    speedMult *= effect[5];
                    HitpointsMult *= effect[6];
                    LPRegen += effect[7];
                    ManaMult *= effect[8];
                    ManaRegen += effect[9];
                }
                
                if(ManaMult != tempManaMult)
                {
                    MaxMana += Mathf.RoundToInt(MaxMana * (1 - ManaMult - tempManaMult));
                    if(Mathf.RoundToInt(MaxMana * (1 - ManaMult - tempManaMult)) < 0)                //wenn Mana abgezogen werden sollen
                    {
                        if(Mana > MaxMana)        //man muss nicht mehr abziehen als nötig
                        {
                            Mana = MaxMana;
                        }
                    }
                    else
                    {
                        Mana += Mathf.RoundToInt(MaxMana * (1 - ManaMult - tempManaMult));
                    }
                }
                if(HitpointsMult != tempHitMult)
                {
                    MaxHitpoints += Mathf.RoundToInt(MaxHitpoints * (1 - HitpointsMult - tempHitMult));
                    if(Mathf.RoundToInt(MaxHitpoints * (1 - HitpointsMult - tempHitMult)) < 0)                //wenn Leben abgezogen werden sollen
                    {
                        if(Hitpoints > MaxHitpoints)        //man muss nicht mehr abziehen als nötig
                        {
                            Hitpoints = MaxHitpoints;
                        }
                    }
                    else
                    {
                        Hitpoints += Mathf.RoundToInt(MaxHitpoints * (1 - HitpointsMult - tempHitMult));
                    }
                }
            }

            foreach(float[] x in remove)
            {
                StatusEffects.Remove(x);
            }
        }
        
        tempHPReg += LPRegen * Time.deltaTime;
        tempManaReg += ManaRegen * Time.deltaTime;
        if(Mathf.FloorToInt(tempHPReg) != 0)
        {
            if(Hitpoints + Mathf.FloorToInt(tempHPReg) <= MaxHitpoints)
            {
                Hitpoints += Mathf.FloorToInt(tempHPReg);
            }
            else
            {
                Hitpoints = MaxHitpoints;
            }
            tempHPReg -= Mathf.FloorToInt(tempHPReg);
        }
        
        if(Mathf.FloorToInt(tempManaReg) != 0)
        {
            if(Mana + Mathf.FloorToInt(tempManaReg) <= MaxMana)
            {
                Mana += Mathf.FloorToInt(tempManaReg);
            }
            else
            {
                Mana = MaxMana;
            }
            tempManaReg -= Mathf.FloorToInt(tempManaReg);
        }
        RK = BaseRK + ShieldRK + MagicalBoniRK + GeschRK + tempRK;

        if(Character)
        {
            if(tempExp != Experience)
            {
                tempExp = Experience;
                CheckLvL();
            }
            HitpointsTextField2.GetComponent<TextMeshProUGUI>().text = Hitpoints +"/"+ MaxHitpoints;
            HitpointsBar.GetComponent<Image>().fillAmount = (float) Hitpoints / MaxHitpoints;
            ManaTextField.GetComponent<TextMeshProUGUI>().text = Mana +"/"+ MaxMana;
            ManaBar.GetComponent<Image>().fillAmount = (float) Mana / MaxMana;
        }
    }

    public void ApplyStatusEffect( float time, float tdamageMult, float tattackSpeedMult, int ttempRK, int ttempETW0, float tspeedMult, float tHitpointsMult, float tLPRegen, float tManaMult, float tManaRegen)
    {
        float[] tempEffects = new float[10];
        tempEffects[0] = time;
        tempEffects[1] = tdamageMult;           
        tempEffects[2] = tattackSpeedMult;   
        tempEffects[3] = ttempRK;                      
        tempEffects[4] = ttempETW0;               
        tempEffects[5] = tspeedMult;            
        tempEffects[6] = tHitpointsMult;             
        tempEffects[7] = tLPRegen;              
        tempEffects[8] = tManaMult;
        tempEffects[9] = tManaRegen;
        StatusEffects.Add(tempEffects);
    }

    public void UpdateLevels( int temp)
    {
        int x = temp - Level;
        for(int i = 0; i < x; i++)
        {
            Level ++;
            UpdateLevel();
        }
    }

    void CheckLvL()
    {
        int temp = Experience;
        
        if(CharacterClass == 1)
        {
            if(Experience >= 250000)
            {
                temp = 9 + Mathf.FloorToInt((temp - 250000) / 250000);
                if(temp > Level)
                {
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 125000)
            {
                if(Level < 8)
                {
                    temp = 8;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 64000)
            {
                if(Level < 7)
                {
                    temp = 7;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 32000)
            {
                if(Level < 6)
                {
                    temp = 6;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 16000)
            {
                if(Level < 5)
                {
                    temp = 5;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 8000)
            {
                if(Level < 4)
                {
                    temp = 4;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 4000)
            {
                if(Level < 3)
                {
                    temp = 3;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 2000)
            {
                if(Level < 2)
                {
                    temp = 2;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience == 0)
            {
                if(Level < 1)
                {
                    temp = 1;
                    UpdateLevels(temp);
                    return;
                }
            }
        }
        else if(CharacterClass == 2 || CharacterClass == 3)
        {
            if(Experience >= 300000)
            {
                temp = 9 + Mathf.FloorToInt((temp - 300000) / 300000);
                if(temp > Level)
                {
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 150000)
            {
                if(Level < 8)
                {
                    temp = 8;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 75000)
            {
                if(Level < 7)
                {
                    temp = 7;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 36000)
            {
                if(Level < 6)
                {
                    temp = 6;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 18000)
            {
                if(Level < 5)
                {
                    temp = 5;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 9000)
            {
                if(Level < 4)
                {
                    temp = 4;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 4500)
            {
                if(Level < 3)
                {
                    temp = 3;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 2250)
            {
                if(Level < 2)
                {
                    temp = 2;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience == 0)
            {
                if(Level < 1)
                {
                    temp = 1;
                    UpdateLevels(temp);
                    return;
                }
            }
        }
        else if(CharacterClass == 4 || CharacterClass == 5)
        {
            if(Experience >= 375000)
            {
                temp = 11 + Mathf.FloorToInt((temp - 375000) / 375000);
                if(temp > Level)
                {
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 250000)
            {
                if(Level < 10)
                {
                    temp = 10;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 135000)
            {
                if(Level < 9)
                {
                    temp = 9;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 90000)
            {
                if(Level < 8)
                {
                    temp = 8;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 60000)
            {
                if(Level < 7)
                {
                    temp = 7;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 40000)
            {
                if(Level < 6)
                {
                    temp = 6;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 20000)
            {
                if(Level < 5)
                {
                    temp = 5;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 10000)
            {
                if(Level < 4)
                {
                    temp = 4;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 5000)
            {
                if(Level < 3)
                {
                    temp = 3;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 2500)
            {
                if(Level < 2)
                {
                    temp = 2;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience == 0)
            {
                if(Level < 1)
                {
                    temp = 1;
                    UpdateLevels(temp);
                    return;
                }
            }
        }
        else if(CharacterClass == 6)
        {
            if(Experience >= 225000)
            {
                temp = 9 + Mathf.FloorToInt((temp - 225000) / 225000);
                if(temp > Level)
                {
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 110000)
            {
                if(Level < 8)
                {
                    temp = 8;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 55000)
            {
                if(Level < 7)
                {
                    temp = 7;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 27500)
            {
                if(Level < 6)
                {
                    temp = 6;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 13000)
            {
                if(Level < 5)
                {
                    temp = 5;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 6000)
            {
                if(Level < 4)
                {
                    temp = 4;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 3000)
            {
                if(Level < 3)
                {
                    temp = 3;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 1500)
            {
                if(Level < 2)
                {
                    temp = 2;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience == 0)
            {
                if(Level < 1)
                {
                    temp = 1;
                    UpdateLevels(temp);
                    return;
                }
            }
        }
        else if(CharacterClass == 7)
        {
            if((Experience >= 500000 && Level >= 16) || (Experience < 500000 && Level == 16))
            {
                temp = 16 + Mathf.FloorToInt((temp - 500000) / 500000);
                if(temp > Level)
                {
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 3500000)
            {
                if(Level < 16)
                {
                    temp = 16;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 3000000)
            {
                if(Level < 15)
                {
                    temp = 15;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 1500000)
            {
                if(Level < 14)
                {
                    temp = 14;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 750000)
            {
                if(Level < 13)
                {
                    temp = 13;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 300000)
            {
                if(Level < 12)
                {
                    temp = 12;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 200000)
            {
                if(Level < 11)
                {
                    temp = 11;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 125000)
            {
                if(Level < 10)
                {
                    temp = 10;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 90000)
            {
                if(Level < 9)
                {
                    temp = 9;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 60000)
            {
                if(Level < 8)
                {
                    temp = 8;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 35000)
            {
                if(Level < 7)
                {
                    temp = 7;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 20000)
            {
                if(Level < 6)
                {
                    temp = 6;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 12500)
            {
                if(Level < 5)
                {
                    temp = 5;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 7500)
            {
                if(Level < 4)
                {
                    temp = 4;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 4000)
            {
                if(Level < 3)
                {
                    temp = 3;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 2000)
            {
                if(Level < 2)
                {
                    temp = 2;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience == 0)
            {
                if(Level < 1)
                {
                    temp = 1;
                    UpdateLevels(temp);
                    return;
                }
            }
        }
        else if(CharacterClass == 8 || CharacterClass == 9)
        {
            if(Experience >= 220000)
            {
                temp = 11 + Mathf.FloorToInt((temp - 220000) / 220000);
                if(temp > Level)
                {
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 160000)
            {
                if(Level < 10)
                {
                    temp = 10;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 110000)
            {
                if(Level < 9)
                {
                    temp = 9;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 70000)
            {
                if(Level < 8)
                {
                    temp = 8;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 40000)
            {
                if(Level < 7)
                {
                    temp = 7;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 20000)
            {
                if(Level < 6)
                {
                    temp = 6;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 10000)
            {
                if(Level < 5)
                {
                    temp = 5;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 5000)
            {
                if(Level < 4)
                {
                    temp = 4;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 2500)
            {
                if(Level < 3)
                {
                    temp = 3;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience >= 1250)
            {
                if(Level < 2)
                {
                    temp = 2;
                    UpdateLevels(temp);
                    return;
                }
            }
            else if(Experience == 0)
            {
                if(Level < 1)
                {
                    temp = 1;
                    UpdateLevels(temp);
                    return;
                }
            }
        }
    }

    public void UpdateLevel()
    {
        UpdateAttributes();
        
        int temp = 0;

        if(CharacterClass == 1 || CharacterClass == 2 || CharacterClass == 3)       //1:Kämpfer; 2:Paladin; 3:Waldläufer
        {
            temp = Random.Range(1, 11) + BoniTrefferpunkte;
            if(temp < 1)
            {
                temp = 1;
            }
            Hitpoints += temp;
            MaxHitpoints += temp;

            temp = Random.Range(1, 6) + Random.Range(1, 6) + Random.Range(1, 6) + Random.Range(1, 6) + BoniMana;
            if(temp < 1)
            {
                temp = 1;
            }
            Mana += temp;
            MaxMana += temp;
            BaseETW0 = 20 - (Level - 1);

            if(Level > 16)
            {
                RWLähmungGiftTodesmagie = 3;
                RWZauberstäbeSteckenRuten = 5;
                RWVersteinerungVerwandlung = 4;
                RWOdemwaffen = 4;
                RWZauber = 6;
            }
            else if(Level > 14)
            {
                RWLähmungGiftTodesmagie = 4;
                RWZauberstäbeSteckenRuten = 6;
                RWVersteinerungVerwandlung = 5;
                RWOdemwaffen = 4;
                RWZauber = 7;
            }
            else if(Level > 12)
            {
                RWLähmungGiftTodesmagie = 5;
                RWZauberstäbeSteckenRuten = 7;
                RWVersteinerungVerwandlung = 6;
                RWOdemwaffen = 5;
                RWZauber = 8;
            }
            else if(Level > 10)
            {
                RWLähmungGiftTodesmagie = 7;
                RWZauberstäbeSteckenRuten = 9;
                RWVersteinerungVerwandlung = 8;
                RWOdemwaffen = 8;
                RWZauber = 10;
            }
            else if(Level > 8)
            {
                RWLähmungGiftTodesmagie = 8;
                RWZauberstäbeSteckenRuten = 10;
                RWVersteinerungVerwandlung = 9;
                RWOdemwaffen = 9;
                RWZauber = 11;
            }
            else if(Level > 6)
            {
                RWLähmungGiftTodesmagie = 10;
                RWZauberstäbeSteckenRuten = 12;
                RWVersteinerungVerwandlung = 11;
                RWOdemwaffen = 12;
                RWZauber = 13;
            }
            else if(Level > 4)
            {
                RWLähmungGiftTodesmagie = 11;
                RWZauberstäbeSteckenRuten = 13;
                RWVersteinerungVerwandlung = 12;
                RWOdemwaffen = 13;
                RWZauber = 14;
            }
            else if(Level > 2)
            {
                RWLähmungGiftTodesmagie = 13;
                RWZauberstäbeSteckenRuten = 15;
                RWVersteinerungVerwandlung = 14;
                RWOdemwaffen = 16;
                RWZauber = 16;
            }
            else if(Level > 0)
            {
                RWLähmungGiftTodesmagie = 14;
                RWZauberstäbeSteckenRuten = 16;
                RWVersteinerungVerwandlung = 15;
                RWOdemwaffen = 17;
                RWZauber = 17;
            }
            else
            {
                RWLähmungGiftTodesmagie = 16;
                RWZauberstäbeSteckenRuten = 18;
                RWVersteinerungVerwandlung = 17;
                RWOdemwaffen = 20;
                RWZauber = 19;
            }
        }
        else if(CharacterClass == 4 || CharacterClass == 5)      //4:Zauberkundiger; 5:Spezialist
        {
            temp = Random.Range(1, 5) + BoniTrefferpunkte;
            if(temp < 1)
            {
                temp = 1;
            }
            Hitpoints += temp;
            MaxHitpoints += temp;

            temp = Random.Range(1, 6) + Random.Range(1, 6) + Random.Range(1, 6) + Random.Range(1, 6) + BoniMana;
            if(temp < 1)
            {
                temp = 1;
            }
            Mana += temp;
            MaxMana += temp;

            BaseETW0 = 20 - Mathf.FloorToInt((Level - 1) / 3);

            if(Level > 20)
            {
                RWLähmungGiftTodesmagie = 8;
                RWZauberstäbeSteckenRuten = 3;
                RWVersteinerungVerwandlung = 5;
                RWOdemwaffen = 7;
                RWZauber = 4;
            }
            else if(Level > 15)
            {
                RWLähmungGiftTodesmagie = 10;
                RWZauberstäbeSteckenRuten = 5;
                RWVersteinerungVerwandlung = 7;
                RWOdemwaffen = 9;
                RWZauber = 6;
            }
            else if(Level > 10)
            {
                RWLähmungGiftTodesmagie = 11;
                RWZauberstäbeSteckenRuten = 7;
                RWVersteinerungVerwandlung = 9;
                RWOdemwaffen = 11;
                RWZauber = 8;
            }
            else if(Level > 5)
            {
                RWLähmungGiftTodesmagie = 13;
                RWZauberstäbeSteckenRuten = 9;
                RWVersteinerungVerwandlung = 11;
                RWOdemwaffen = 13;
                RWZauber = 10;
            }
            else
            {
                RWLähmungGiftTodesmagie = 14;
                RWZauberstäbeSteckenRuten = 11;
                RWVersteinerungVerwandlung = 13;
                RWOdemwaffen = 15;
                RWZauber = 12;
            }
        }
        else if(CharacterClass == 6 || CharacterClass == 7)      //6:Kleriker; 7:Druide
        {
            temp = Random.Range(1, 9) + BoniTrefferpunkte;
            if(temp < 1)
            {
                temp = 1;
            }
            Hitpoints += temp;
            MaxHitpoints += temp;

            temp = Random.Range(1, 6) + Random.Range(1, 6) + Random.Range(1, 6) + Random.Range(1, 6) + BoniMana;
            if(temp < 1)
            {
                temp = 1;
            }
            Mana += temp;
            MaxMana += temp;

            BaseETW0 = 20 - 2 * Mathf.FloorToInt((Level - 1) / 3);

            if(Level > 18)
            {
                RWLähmungGiftTodesmagie = 2;
                RWZauberstäbeSteckenRuten = 6;
                RWVersteinerungVerwandlung = 5;
                RWOdemwaffen = 8;
                RWZauber = 7;
            }
            else if(Level > 15)
            {
                RWLähmungGiftTodesmagie = 4;
                RWZauberstäbeSteckenRuten = 8;
                RWVersteinerungVerwandlung = 7;
                RWOdemwaffen = 10;
                RWZauber = 9;
            }
            else if(Level > 12)
            {
                RWLähmungGiftTodesmagie = 5;
                RWZauberstäbeSteckenRuten = 9;
                RWVersteinerungVerwandlung = 8;
                RWOdemwaffen = 11;
                RWZauber = 10;
            }
            else if(Level > 9)
            {
                RWLähmungGiftTodesmagie = 6;
                RWZauberstäbeSteckenRuten = 10;
                RWVersteinerungVerwandlung = 9;
                RWOdemwaffen = 12;
                RWZauber = 11;
            }
            else if(Level > 6)
            {
                RWLähmungGiftTodesmagie = 7;
                RWZauberstäbeSteckenRuten = 11;
                RWVersteinerungVerwandlung = 10;
                RWOdemwaffen = 13;
                RWZauber = 12;
            }
            else if(Level > 3)
            {
                RWLähmungGiftTodesmagie = 9;
                RWZauberstäbeSteckenRuten = 13;
                RWVersteinerungVerwandlung = 12;
                RWOdemwaffen = 15;
                RWZauber = 14;
            }
            else
            {
                RWLähmungGiftTodesmagie = 10;
                RWZauberstäbeSteckenRuten = 14;
                RWVersteinerungVerwandlung = 13;
                RWOdemwaffen = 16;
                RWZauber = 15;
            }
        }
        else if(CharacterClass == 8 || CharacterClass == 9)      //8:Dieb; 9:Barde
        {
            temp = Random.Range(1, 7) + BoniTrefferpunkte;
            if(temp < 1)
            {
                temp = 1;
            }
            Hitpoints += temp;
            MaxHitpoints += temp;

            temp = Random.Range(1, 6) + Random.Range(1, 6) + Random.Range(1, 6) + Random.Range(1, 6) + BoniMana;
            if(temp < 1)
            {
                temp = 1;
            }
            Mana += temp;
            MaxMana += temp;

            BaseETW0 = 20 - Mathf.FloorToInt((Level - 1) / 2);

            if(Level > 20)
            {
                RWLähmungGiftTodesmagie = 8;
                RWZauberstäbeSteckenRuten = 4;
                RWVersteinerungVerwandlung = 7;
                RWOdemwaffen = 11;
                RWZauber = 5;
            }
            else if(Level > 16)
            {
                RWLähmungGiftTodesmagie = 9;
                RWZauberstäbeSteckenRuten = 6;
                RWVersteinerungVerwandlung = 8;
                RWOdemwaffen = 12;
                RWZauber = 7;
            }
            else if(Level > 12)
            {
                RWLähmungGiftTodesmagie = 10;
                RWZauberstäbeSteckenRuten = 8;
                RWVersteinerungVerwandlung = 9;
                RWOdemwaffen = 13;
                RWZauber = 9;
            }
            else if(Level > 8)
            {
                RWLähmungGiftTodesmagie = 11;
                RWZauberstäbeSteckenRuten = 10;
                RWVersteinerungVerwandlung = 10;
                RWOdemwaffen = 14;
                RWZauber = 11;
            }
            else if(Level > 4)
            {
                RWLähmungGiftTodesmagie = 12;
                RWZauberstäbeSteckenRuten = 12;
                RWVersteinerungVerwandlung = 11;
                RWOdemwaffen = 15;
                RWZauber = 13;
            }
            else
            {
                RWLähmungGiftTodesmagie = 13;
                RWZauberstäbeSteckenRuten = 14;
                RWVersteinerungVerwandlung = 12;
                RWOdemwaffen = 16;
                RWZauber = 15;
            }
        }

        if(Hitpoints > MaxHitpoints)
        {
            Hitpoints = MaxHitpoints;
        }
        if(Mana > MaxMana)
        {
            Mana = MaxMana;
        }
    }

    public void UpdateAttributes( int St = 0, int Ges = 0, int Kon = 0, int Int = 0, int Wei = 0, int Cha = 0)
    {
        Strength += St;
        Dexterity += Ges;
        Constitution += Kon;
        Intelligence += Int;
        Wisdom += Wei;
        Charisma += Cha;

        if(Kon != 0)
        {
            MaxHitpoints += Level * Kon;
            Hitpoints += Level * Kon;
        }

        if(Strength == 1)
        {
            MeleeBoniETW0 = -5;
            MeleeBoniSchaden = -4;
            Traglast = 1;
            Stemmvermögen = 3;
            TürenÖffnen = 1;
            VerbiegenAnheben = 0;
        }
        else if(Strength == 2)
        {
            MeleeBoniETW0 = -3;
            MeleeBoniSchaden = -2;
            Traglast = 1;
            Stemmvermögen = 5;
            TürenÖffnen = 1;
            VerbiegenAnheben = 0;
        }
        else if(Strength == 3)
        {
            MeleeBoniETW0 = -3;
            MeleeBoniSchaden = -1;
            Traglast = 5;
            Stemmvermögen = 10;
            TürenÖffnen = 2;
            VerbiegenAnheben = 0;
        }
        else if(Strength == 4 || Strength == 5)
        {
            MeleeBoniETW0 = -2;
            MeleeBoniSchaden = -1;
            Traglast = 10;
            Stemmvermögen = 25;
            TürenÖffnen = 3;
            VerbiegenAnheben = 0;
        }
        else if(Strength == 6 || Strength == 7)
        {
            MeleeBoniETW0 = -1;
            MeleeBoniSchaden = 0;
            Traglast = 20;
            Stemmvermögen = 55;
            TürenÖffnen = 4;
            VerbiegenAnheben = 0;
        }
        else if(Strength == 8 || Strength == 9)
        {
            MeleeBoniETW0 = 0;
            MeleeBoniSchaden = 0;
            Traglast = 35;
            Stemmvermögen = 90;
            TürenÖffnen = 5;
            VerbiegenAnheben = 1;
        }
        else if(Strength == 10 || Strength == 11)
        {
            MeleeBoniETW0 = 0;
            MeleeBoniSchaden = 0;
            Traglast = 40;
            Stemmvermögen = 115;
            TürenÖffnen = 6;
            VerbiegenAnheben = 2;
        }
        else if(Strength == 12 || Strength == 13)
        {
            MeleeBoniETW0 = 0;
            MeleeBoniSchaden = 0;
            Traglast = 45;
            Stemmvermögen = 140;
            TürenÖffnen = 7;
            VerbiegenAnheben = 4;
        }
        else if(Strength == 14 || Strength == 15)
        {
            MeleeBoniETW0 = 0;
            MeleeBoniSchaden = 0;
            Traglast = 55;
            Stemmvermögen = 170;
            TürenÖffnen = 8;
            VerbiegenAnheben = 7;
        }
        else if(Strength == 16)
        {
            MeleeBoniETW0 = 0;
            MeleeBoniSchaden = 1;
            Traglast = 70;
            Stemmvermögen = 195;
            TürenÖffnen = 9;
            VerbiegenAnheben = 10;
        }
        else if(Strength == 17)
        {
            MeleeBoniETW0 = 1;
            MeleeBoniSchaden = 1;
            Traglast = 85;
            Stemmvermögen = 220;
            TürenÖffnen = 10;
            VerbiegenAnheben = 13;
        }
        else if(Strength == 18)
        {
            if(StärkeMult == 0)
            {
                MeleeBoniETW0 = 1;
                MeleeBoniSchaden = 2;
                Traglast = 110;
                Stemmvermögen = 255;
                TürenÖffnen = 11;
                VerbiegenAnheben = 16;
            }
            else if(1 < StärkeMult && StärkeMult < 50)
            {
                MeleeBoniETW0 = 1;
                MeleeBoniSchaden = 3;
                Traglast = 135;
                Stemmvermögen = 280;
                TürenÖffnen = 12;
                VerbiegenAnheben = 20;
            }
            else if(50 < StärkeMult && StärkeMult < 76)
            {
                MeleeBoniETW0 = 2;
                MeleeBoniSchaden = 3;
                Traglast = 160;
                Stemmvermögen = 305;
                TürenÖffnen = 13;
                VerbiegenAnheben = 25;
            }
            else if(75 < StärkeMult && StärkeMult < 91)
            {
                MeleeBoniETW0 = 2;
                MeleeBoniSchaden = 4;
                Traglast = 185;
                Stemmvermögen = 330;
                TürenÖffnen = 14;
                VerbiegenAnheben = 30;
            }
            else if(90 < StärkeMult && StärkeMult < 100)
            {
                MeleeBoniETW0 = 2;
                MeleeBoniSchaden = 5;
                Traglast = 235;
                Stemmvermögen = 380;
                TürenÖffnen = 15;
                VerbiegenAnheben = 35;
            }
            else
            {
                MeleeBoniETW0 = 3;
                MeleeBoniSchaden = 6;
                Traglast = 335;
                Stemmvermögen = 480;
                TürenÖffnen = 16;
                VerbiegenAnheben = 40;
            }
        }
        else if(Strength == 19)
        {
            MeleeBoniETW0 = 3;
            MeleeBoniSchaden = 7;
            Traglast = 485;
            Stemmvermögen = 640;
            TürenÖffnen = 16;
            VerbiegenAnheben = 50;
        }
        else if(Strength == 20)
        {
            MeleeBoniETW0 = 3;
            MeleeBoniSchaden = 8;
            Traglast = 535;
            Stemmvermögen = 700;
            TürenÖffnen = 17;
            VerbiegenAnheben = 60;
        }
        else if(Strength == 21)
        {
            MeleeBoniETW0 = 4;
            MeleeBoniSchaden = 9;
            Traglast = 635;
            Stemmvermögen = 810;
            TürenÖffnen = 17;
            VerbiegenAnheben = 70;
        }
        else if(Strength == 22)
        {
            MeleeBoniETW0 = 4;
            MeleeBoniSchaden = 10;
            Traglast = 785;
            Stemmvermögen = 970;
            TürenÖffnen = 18;
            VerbiegenAnheben = 80;
        }
        else if(Strength == 23)
        {
            MeleeBoniETW0 = 5;
            MeleeBoniSchaden = 11;
            Traglast = 935;
            Stemmvermögen = 1130;
            TürenÖffnen = 18;
            VerbiegenAnheben = 90;
        }
        else if(Strength == 24)
        {
            MeleeBoniETW0 = 6;
            MeleeBoniSchaden = 12;
            Traglast = 1235;
            Stemmvermögen = 1440;
            TürenÖffnen = 19;
            VerbiegenAnheben = 95;
        }
        else if(Strength == 25)
        {
            MeleeBoniETW0 = 7;
            MeleeBoniSchaden = 14;
            Traglast = 1535;
            Stemmvermögen = 1750;
            TürenÖffnen = 19;
            VerbiegenAnheben = 99;
        }


        if(Dexterity == 1)
        {
            Reaktionsmod = -6;
            RangeBoniETW0 = -6;
            GeschRK = 5;
        }
        else if(Dexterity == 2)
        {
            Reaktionsmod = -4;
            RangeBoniETW0 = -4;
            GeschRK = 5;
        }
        else if(Dexterity == 3)
        {
            Reaktionsmod = -3;
            RangeBoniETW0 = -3;
            GeschRK = 4;
        }
        else if(Dexterity == 4)
        {
            Reaktionsmod = -2;
            RangeBoniETW0 = -2;
            GeschRK = 3;
        }
        else if(Dexterity == 5)
        {
            Reaktionsmod = -1;
            RangeBoniETW0 = -1;
            GeschRK = 2;
        }
        else if(Dexterity == 6)
        {
            Reaktionsmod = 0;
            RangeBoniETW0 = 0;
            GeschRK = 1;
        }
        else if(6 < Dexterity && Dexterity < 15)
        {
            Reaktionsmod = 0;
            RangeBoniETW0 = 0;
            GeschRK = 0;
        }
        else if(Dexterity == 15)
        {
            Reaktionsmod = 0;
            RangeBoniETW0 = 0;
            GeschRK = -1;
        }
        else if(Dexterity == 16)
        {
            Reaktionsmod = 1;
            RangeBoniETW0 = 1;
            GeschRK = -2;
        }
        else if(Dexterity == 17)
        {
            Reaktionsmod = 2;
            RangeBoniETW0 = 2;
            GeschRK = -3;
        }
        else if(Dexterity == 18)
        {
            Reaktionsmod = 2;
            RangeBoniETW0 = 2;
            GeschRK = -4;
        }
        else if(18 < Dexterity && Dexterity < 21)
        {
            Reaktionsmod = 3;
            RangeBoniETW0 = 3;
            GeschRK = -4;
        }
        else if(20 < Dexterity && Dexterity < 24)
        {
            Reaktionsmod = 4;
            RangeBoniETW0 = 4;
            GeschRK = -5;
        }
        else if(23 < Dexterity && Dexterity < 25)
        {
            Reaktionsmod = 5;
            RangeBoniETW0 = 5;
            GeschRK = -6;
        }


        if(Constitution == 1)
        {
            BoniTrefferpunkte = -3;
            KörperlSchock = 25;
            Wiedererwecken = 30;
            BoniRWGift = -2;
            Regeneration = 0;
        }
        else if(Constitution == 2)
        {
            BoniTrefferpunkte = -2;
            KörperlSchock = 30;
            Wiedererwecken = 35;
            BoniRWGift = -1;
            Regeneration = 0;
        }
        else if(Constitution == 3)
        {
            BoniTrefferpunkte = -2;
            KörperlSchock = 35;
            Wiedererwecken = 40;
            BoniRWGift = 0;
            Regeneration = 0;
        }
        else if(Constitution == 4)
        {
            BoniTrefferpunkte = -1;
            KörperlSchock = 40;
            Wiedererwecken = 45;
            BoniRWGift = 0;
            Regeneration = 0;
        }
        else if(Constitution == 5)
        {
            BoniTrefferpunkte = -1;
            KörperlSchock = 45;
            Wiedererwecken = 50;
            BoniRWGift = 0;
            Regeneration = 0;
        }
        else if(Constitution == 6)
        {
            BoniTrefferpunkte = -1;
            KörperlSchock = 50;
            Wiedererwecken = 55;
            BoniRWGift = 0;
            Regeneration = 0;
        }
        else if(Constitution == 7)
        {
            BoniTrefferpunkte = 0;
            KörperlSchock = 55;
            Wiedererwecken = 60;
            BoniRWGift = 0;
            Regeneration = 0;
        }
        else if(Constitution == 8)
        {
            BoniTrefferpunkte = 0;
            KörperlSchock = 60;
            Wiedererwecken = 65;
            BoniRWGift = 0;
            Regeneration = 0;
        }
        else if(Constitution == 9)
        {
            BoniTrefferpunkte = 0;
            KörperlSchock = 65;
            Wiedererwecken = 70;
            BoniRWGift = 0;
            Regeneration = 0;
        }
        else if(Constitution == 10)
        {
            BoniTrefferpunkte = 0;
            KörperlSchock = 70;
            Wiedererwecken = 75;
            BoniRWGift = 0;
            Regeneration = 0;
        }
        else if(Constitution == 11)
        {
            BoniTrefferpunkte = 0;
            KörperlSchock = 75;
            Wiedererwecken = 80;
            BoniRWGift = 0;
            Regeneration = 0;
        }
        else if(Constitution == 12)
        {
            BoniTrefferpunkte = 0;
            KörperlSchock = 80;
            Wiedererwecken = 85;
            BoniRWGift = 0;
            Regeneration = 0;
        }
        else if(Constitution == 13)
        {
            BoniTrefferpunkte = 0;
            KörperlSchock = 85;
            Wiedererwecken = 90;
            BoniRWGift = 0;
            Regeneration = 0;
        }
        else if(Constitution == 14)
        {
            BoniTrefferpunkte = 0;
            KörperlSchock = 88;
            Wiedererwecken = 92;
            BoniRWGift = 0;
            Regeneration = 0;
        }
        else if(Constitution == 15)
        {
            BoniTrefferpunkte = 1;
            KörperlSchock = 90;
            Wiedererwecken = 94;
            BoniRWGift = 0;
            Regeneration = 0;
        }
        else if(Constitution == 16)
        {
            BoniTrefferpunkte = 2;
            KörperlSchock = 95;
            Wiedererwecken = 96;
            BoniRWGift = 0;
            Regeneration = 0;
        }
        else if(Constitution == 17)
        {
            BoniTrefferpunkte = 3;
            KörperlSchock = 97;
            Wiedererwecken = 98;
            BoniRWGift = 0;
            Regeneration = 0;
        }
        else if(Constitution == 18)
        {
            BoniTrefferpunkte = 4;
            KörperlSchock = 99;
            Wiedererwecken = 100;
            BoniRWGift = 0;
            Regeneration = 0;
        }
        else if(Constitution == 19)
        {
            BoniTrefferpunkte = 5;
            KörperlSchock = 99;
            Wiedererwecken = 100;
            BoniRWGift = 1;
            Regeneration = 0;
        }
        else if(Constitution == 20)
        {
            BoniTrefferpunkte = 5;
            KörperlSchock = 99;
            Wiedererwecken = 100;
            BoniRWGift = 1;
            Regeneration = 1/6;
        }
        else if(Constitution == 21)
        {
            BoniTrefferpunkte = 6;
            KörperlSchock = 99;
            Wiedererwecken = 100;
            BoniRWGift = 2;
            Regeneration = 1/5;
        }
        else if(Constitution == 22)
        {
            BoniTrefferpunkte = 6;
            KörperlSchock = 99;
            Wiedererwecken = 100;
            BoniRWGift = 2;
            Regeneration = 1/4;
        }
        else if(Constitution == 23)
        {
            BoniTrefferpunkte = 6;
            KörperlSchock = 99;
            Wiedererwecken = 100;
            BoniRWGift = 3;
            Regeneration = 1/3;
        }
        else if(Constitution == 24)
        {
            BoniTrefferpunkte = 7;
            KörperlSchock = 99;
            Wiedererwecken = 100;
            BoniRWGift = 3;
            Regeneration = 1/2;
        }
        else if(Constitution == 25)
        {
            BoniTrefferpunkte = 7;
            KörperlSchock = 100;
            Wiedererwecken = 100;
            BoniRWGift = 4;
            Regeneration = 1;
        }


        if(Intelligence == 1)
        {
            Sprachen = 0;
            ZauberHöchstgrad = 0;
            ZauberVerstehen = 0;
            ZauberHöchstzahl = 0;
            ImmunitätIllusion = 0;
            BoniMana = -5;
        }
        else if(1 < Intelligence && Intelligence < 9)
        {
            Sprachen = 1;
            ZauberHöchstgrad = 0;
            ZauberVerstehen = 0;
            ZauberHöchstzahl = 0;
            ImmunitätIllusion = 0;
            BoniMana = - Mathf.CeilToInt((10 - Intelligence) / 2);
        }
        else if(Intelligence == 9)
        {
            Sprachen = 2;
            ZauberHöchstgrad = 4;
            ZauberVerstehen = 35;
            ZauberHöchstzahl = 6;
            ImmunitätIllusion = 0;
            BoniMana = -1;
        }
        else if(Intelligence == 10)
        {
            Sprachen = 2;
            ZauberHöchstgrad = 5;
            ZauberVerstehen = 40;
            ZauberHöchstzahl = 7;
            ImmunitätIllusion = 0;
            BoniMana = 0;
        }
        else if(Intelligence == 11)
        {
            Sprachen = 2;
            ZauberHöchstgrad = 5;
            ZauberVerstehen = 45;
            ZauberHöchstzahl = 7;
            ImmunitätIllusion = 0;
            BoniMana = 0;
        }
        else if(Intelligence == 12)
        {
            Sprachen = 3;
            ZauberHöchstgrad = 6;
            ZauberVerstehen = 50;
            ZauberHöchstzahl = 7;
            ImmunitätIllusion = 0;
            BoniMana = 0;
        }
        else if(Intelligence == 13)
        {
            Sprachen = 3;
            ZauberHöchstgrad = 6;
            ZauberVerstehen = 55;
            ZauberHöchstzahl = 9;
            ImmunitätIllusion = 0;
            BoniMana = 0;
        }
        else if(Intelligence == 14)
        {
            Sprachen = 4;
            ZauberHöchstgrad = 7;
            ZauberVerstehen = 60;
            ZauberHöchstzahl = 9;
            ImmunitätIllusion = 0;
            BoniMana = 0;
        }
        else if(Intelligence == 15)
        {
            Sprachen = 4;
            ZauberHöchstgrad = 7;
            ZauberVerstehen = 65;
            ZauberHöchstzahl = 11;
            ImmunitätIllusion = 0;
            BoniMana = 0;
        }
        else if(Intelligence == 16)
        {
            Sprachen = 5;
            ZauberHöchstgrad = 8;
            ZauberVerstehen = 70;
            ZauberHöchstzahl = 11;
            ImmunitätIllusion = 0;
            BoniMana = 1;
        }
        else if(Intelligence == 17)
        {
            Sprachen = 6;
            ZauberHöchstgrad = 8;
            ZauberVerstehen = 75;
            ZauberHöchstzahl = 14;
            ImmunitätIllusion = 0;
            BoniMana = 2;
        }
        else if(Intelligence == 18)
        {
            Sprachen = 7;
            ZauberHöchstgrad = 9;
            ZauberVerstehen = 85;
            ZauberHöchstzahl = 18;
            ImmunitätIllusion = 0;
            BoniMana = 3;
        }
        else if(Intelligence == 19)
        {
            Sprachen = 8;
            ZauberHöchstgrad = 9;
            ZauberVerstehen = 95;
            ZauberHöchstzahl = 100;
            ImmunitätIllusion = 1;
            BoniMana = 4;
        }
        else if(Intelligence == 20)
        {
            Sprachen = 9;
            ZauberHöchstgrad = 9;
            ZauberVerstehen = 96;
            ZauberHöchstzahl = 100;
            ImmunitätIllusion = 2;
            BoniMana = 5;
        }
        else if(Intelligence == 21)
        {
            Sprachen = 10;
            ZauberHöchstgrad = 9;
            ZauberVerstehen = 97;
            ZauberHöchstzahl = 100;
            ImmunitätIllusion = 3;
            BoniMana = 6;
        }
        else if(Intelligence == 22)
        {
            Sprachen = 11;
            ZauberHöchstgrad = 9;
            ZauberVerstehen = 98;
            ZauberHöchstzahl = 100;
            ImmunitätIllusion = 4;
            BoniMana = 7;
        }
        else if(Intelligence == 23)
        {
            Sprachen = 12;
            ZauberHöchstgrad = 9;
            ZauberVerstehen = 99;
            ZauberHöchstzahl = 100;
            ImmunitätIllusion = 5;
            BoniMana = 8;
        }
        else if(Intelligence == 24)
        {
            Sprachen = 13;
            ZauberHöchstgrad = 9;
            ZauberVerstehen = 100;
            ZauberHöchstzahl = 100;
            ImmunitätIllusion = 6;
            BoniMana = 9;
        }
        else if(Intelligence == 25)
        {
            Sprachen = 20;
            ZauberHöchstgrad = 9;
            ZauberVerstehen = 100;
            ZauberHöchstzahl = 100;
            ImmunitätIllusion = 7;
            BoniMana = 10;
        }


        if(Wisdom == 1)
        {
            BoniRWZauber = -6;
            ExtraZauber = 0;
            ZauberChance = 80;
            ImmunitätZauber = 0;
        }
        else if(Wisdom == 2)
        {
            BoniRWZauber = -4;
            ExtraZauber = 0;
            ZauberChance = 60;
            ImmunitätZauber = 0;
        }
        else if(Wisdom == 3)
        {
            BoniRWZauber = -3;
            ExtraZauber = 0;
            ZauberChance = 50;
            ImmunitätZauber = 0;
        }
        else if(Wisdom == 4)
        {
            BoniRWZauber = -2;
            ExtraZauber = 0;
            ZauberChance = 45;
            ImmunitätZauber = 0;
        }
        else if(Wisdom == 5)
        {
            BoniRWZauber = -1;
            ExtraZauber = 0;
            ZauberChance = 40;
            ImmunitätZauber = 0;
        }
        else if(Wisdom == 6)
        {
            BoniRWZauber = -1;
            ExtraZauber = 0;
            ZauberChance = 35;
            ImmunitätZauber = 0;
        }
        else if(Wisdom == 7)
        {
            BoniRWZauber = -1;
            ExtraZauber = 0;
            ZauberChance = 30;
            ImmunitätZauber = 0;
        }
        else if(Wisdom == 8)
        {
            BoniRWZauber = 0;
            ExtraZauber = 0;
            ZauberChance = 25;
            ImmunitätZauber = 0;
        }
        else if(Wisdom == 9)
        {
            BoniRWZauber = 0;
            ExtraZauber = 0;
            ZauberChance = 20;
            ImmunitätZauber = 0;
        }
        else if(Wisdom == 10)
        {
            BoniRWZauber = 0;
            ExtraZauber = 0;
            ZauberChance = 15;
            ImmunitätZauber = 0;
        }
        else if(Wisdom == 11)
        {
            BoniRWZauber = 0;
            ExtraZauber = 0;
            ZauberChance = 10;
            ImmunitätZauber = 0;
        }
        else if(Wisdom == 12)
        {
            BoniRWZauber = 0;
            ExtraZauber = 0;
            ZauberChance = 5;
            ImmunitätZauber = 0;
        }
        else if(Wisdom == 13)
        {
            BoniRWZauber = 0;
            ExtraZauber = 1;
            ZauberChance = 0;
            ImmunitätZauber = 0;
        }
        else if(Wisdom == 14)
        {
            BoniRWZauber = 0;
            ExtraZauber = 11;
            ZauberChance = 0;
            ImmunitätZauber = 0;
        }
        else if(Wisdom == 15)
        {
            BoniRWZauber = 1;
            ExtraZauber = 112;
            ZauberChance = 0;
            ImmunitätZauber = 0;
        }
        else if(Wisdom == 16)
        {
            BoniRWZauber = 2;
            ExtraZauber = 1122;
            ZauberChance = 0;
            ImmunitätZauber = 0;
        }
        else if(Wisdom == 17)
        {
            BoniRWZauber = 3;
            ExtraZauber = 11223;
            ZauberChance = 0;
            ImmunitätZauber = 0;
        }
        else if(Wisdom == 18)
        {
            BoniRWZauber = 4;
            ExtraZauber = 112234;
            ZauberChance = 0;
            ImmunitätZauber = 0;
        }
        else if(Wisdom == 19)
        {
            BoniRWZauber = 4;
            ExtraZauber = 11122344;
            ZauberChance = 0;
            ImmunitätZauber = 1;
        }
        else if(Wisdom == 20)
        {
            BoniRWZauber = 4;
            ExtraZauber = 1112223444;
            ZauberChance = 0;
            ImmunitätZauber = 2;
        }
        else if(Wisdom == 21)
        {
            BoniRWZauber = 4;
            ExtraZauber = 111222334445;
            ZauberChance = 0;
            ImmunitätZauber = 2;
        }
        else if(Wisdom == 22)
        {
            BoniRWZauber = 4;
            ExtraZauber = 11122233444455;
            ZauberChance = 0;
            ImmunitätZauber = 3;
        }
        else if(Wisdom == 23)
        {
            BoniRWZauber = 4;
            ExtraZauber = 1112223344445555;
            ZauberChance = 0;
            ImmunitätZauber = 4;
        }
        else if(Wisdom == 24)
        {
            BoniRWZauber = 4;
            ExtraZauber = 111222334444555566;
            ZauberChance = 0;
            ImmunitätZauber = 4;
        }
        else if(Wisdom == 25)
        {
            BoniRWZauber = 4;
            ExtraZauber = 11122233444455556667;
            ZauberChance = 0;
            ImmunitätZauber = 5;
        }


        if(Charisma == 1)
        {
            MaxGefolgsleute = 0;
            Loyalität = -8;
            BegegnungsMod = -7;
        }
        else if(Charisma == 2)
        {
            MaxGefolgsleute = 1;
            Loyalität = -7;
            BegegnungsMod = -6;
        }
        else if(Charisma == 3)
        {
            MaxGefolgsleute = 1;
            Loyalität = -6;
            BegegnungsMod = -5;
        }
        else if(Charisma == 4)
        {
            MaxGefolgsleute = 1;
            Loyalität = -5;
            BegegnungsMod = -4;
        }
        else if(Charisma == 5)
        {
            MaxGefolgsleute = 2;
            Loyalität = -4;
            BegegnungsMod = -3;
        }
        else if(Charisma == 6)
        {
            MaxGefolgsleute = 2;
            Loyalität = -3;
            BegegnungsMod = -2;
        }
        else if(Charisma == 7)
        {
            MaxGefolgsleute = 3;
            Loyalität = -2;
            BegegnungsMod = -1;
        }
        else if(Charisma == 8)
        {
            MaxGefolgsleute = 3;
            Loyalität = -1;
            BegegnungsMod = 0;
        }
        else if(8 < Charisma && Charisma < 12)
        {
            MaxGefolgsleute = 4;
            Loyalität = 0;
            BegegnungsMod = 0;
        }
        else if(Charisma == 12)
        {
            MaxGefolgsleute = 5;
            Loyalität = 0;
            BegegnungsMod = 0;
        }
        else if(Charisma == 13)
        {
            MaxGefolgsleute = 5;
            Loyalität = 0;
            BegegnungsMod = 1;
        }
        else if(Charisma == 14)
        {
            MaxGefolgsleute = 6;
            Loyalität = 1;
            BegegnungsMod = 2;
        }
        else if(Charisma == 15)
        {
            MaxGefolgsleute = 7;
            Loyalität = 3;
            BegegnungsMod = 3;
        }
        else if(Charisma == 16)
        {
            MaxGefolgsleute = 8;
            Loyalität = 4;
            BegegnungsMod = 5;
        }
        else if(Charisma == 17)
        {
            MaxGefolgsleute = 10;
            Loyalität = 6;
            BegegnungsMod = 6;
        }
        else if(Charisma == 18)
        {
            MaxGefolgsleute = 15;
            Loyalität = 8;
            BegegnungsMod = 7;
        }
        else if(Charisma == 19)
        {
            MaxGefolgsleute = 20;
            Loyalität = 10;
            BegegnungsMod = 8;
        }
        else if(Charisma == 20)
        {
            MaxGefolgsleute = 25;
            Loyalität = 12;
            BegegnungsMod = 9;
        }
        else if(Charisma == 21)
        {
            MaxGefolgsleute = 30;
            Loyalität = 14;
            BegegnungsMod = 10;
        }
        else if(Charisma == 22)
        {
            MaxGefolgsleute = 35;
            Loyalität = 16;
            BegegnungsMod = 11;
        }
        else if(Charisma == 23)
        {
            MaxGefolgsleute = 40;
            Loyalität = 18;
            BegegnungsMod = 12;
        }
        else if(Charisma == 24)
        {
            MaxGefolgsleute = 45;
            Loyalität = 20;
            BegegnungsMod = 13;
        }
        else if(Charisma == 25)
        {
            MaxGefolgsleute = 50;
            Loyalität = 20;
            BegegnungsMod = 14;
        }
    }

    public void Consume(GameObject Item)
    {
        FoodStats temp = Item.GetComponent<FoodStats>();
        if(temp == null)
        {
            return;
        }
        
        if(Hitpoints + temp.HP <= MaxHitpoints)
        {
            Hitpoints += temp.HP;
        }
        else
        {
            Hitpoints = MaxHitpoints;
        }
        if(Mana + temp.Mana <= MaxMana)
        {
            Mana += temp.Mana;
        }
        else
        {
            Mana = MaxMana;
        }

        if(temp.duration > 0)      //lasting effects are not doing anything without a duration
        {
            ApplyStatusEffect( temp.duration, temp.damageMult, temp.attackSpeedMult, temp.tempRK, temp.tempETW0, temp.speedMult, temp.HitpointsMult, temp.LPRegen, temp.ManaMult, temp.ManaRegen);
        }
    }

    public void Use(GameObject item)
    {
        RestingStats temp = item.GetComponent<RestingStats>();
        if(temp == null)
        {
            return;
        }
        
        if(Hitpoints + temp.HP + temp.HP2 * MaxHitpoints <= MaxHitpoints)
        {
            Hitpoints += temp.HP + Mathf.FloorToInt(temp.HP2 * MaxHitpoints);
        }
        else
        {
            Hitpoints = MaxHitpoints;
        }
        
        if(Mana + temp.Mana + temp.Mana2 * MaxMana <= MaxMana)
        {
            Mana += temp.Mana + Mathf.FloorToInt(temp.Mana2 * MaxMana);
        }
        else
        {
            Mana = MaxMana;
        }

        if(temp.duration > 0)      //lasting effects are not doing anything without a duration
        {
            ApplyStatusEffect( temp.duration, temp.damageMult, temp.attackSpeedMult, temp.tempRK, temp.tempETW0, temp.speedMult, temp.HitpointsMult, temp.LPRegen, temp.ManaMult, temp.ManaRegen);
        }
    }
    public void GenerateAttributes(int dice, int tries, int boni = 0)
    {
        Strength = generateAttr(dice, tries, boni);
        Constitution = generateAttr(dice, tries, boni);
        Dexterity = generateAttr(dice, tries, boni);
        Intelligence = generateAttr(dice, tries, boni);
        Wisdom = generateAttr(dice, tries, boni);
        Charisma = generateAttr(dice, tries, boni);

        UpdateAttributes();
    }

    private int generateAttr(int dice, int tries, int boni)
    {
        int resultAttr = 1;
        for(int i = 0; i < tries; i++)
        {
            int temp = boni;
            for(int j = 0; j < dice; j++)
            {
                temp += Random.Range(1, 7);
            }
            if(temp > resultAttr)
            {
                resultAttr = temp;
            }
        }
        return resultAttr;
    }
}