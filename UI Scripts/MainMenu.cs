using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using System.Linq;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public static MainMenu mainMenu;
    public GameObject Notifications;
    public Transform pfDamagePopup;

    public float timePassed;
    public float TimePerDay;
    public Gradient SunlightColor;
    public Light Sunlight;
    public Transform LightParent;

    [Header("Group Relations")]
    public List<int> playerEnemies = new();
    public List<int> monsterEnemies = new();
    public List<int> banditEnemies = new();
    public List<int> cityEnemies = new();
    
    public List<List<int>> Relations = new List<List<int>>();

    public List<Quest> Quests;

    [Header("Keys")]
    public KeyCode Forward;
    public KeyCode Backwards;
    public KeyCode Left;
    public KeyCode Right;
    public KeyCode Sprint;
    public KeyCode Jump;
    public KeyCode Attack;
    public KeyCode SelectTarget;
    public KeyCode Inv;             //Open/ Close Inventory Display
    public KeyCode InvInteract;     //interact with chests/ itemPiles
    public KeyCode InvMove;         //move items between inventorys (Inventory and Chest/ throwing items out)
    public KeyCode OpenOptions;

    int currentKey = -1;

    [Header("Prefabs")]
    public List<GameObject> ItemPrefabs;
    public List<GameObject> NPCPrefabs;
    public GameObject PlayerPrefab;
    public List<GameObject> PickUpPrefabs;

    [Header("Saving")]
    SceneData[] currentScenesData;
    List<ListItem> currentNPCs = new();
    public bool finishedLoadingNPCs = false;

    [Header("Options")]
    public List<TextMeshProUGUI> KeyLabels;
    public GameObject Options;
    public GameObject OptionsButton;
    public TMP_Dropdown LoadingSaveDropdown;
    public TMP_Dropdown DeletingSaveDropdown;
    public TMP_InputField SaveFilename;

    [Header("Runtime")]
    public Vector3 DestinationPos;
    public Quaternion DestinationRot;
    
    void Awake()
    {
        currentScenesData = new SceneData[SceneManager.sceneCountInBuildSettings];
        if(mainMenu == null)
        {
            mainMenu = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        Application.targetFrameRate = 60;
        LoadKeys();
    }
    
    void Start()
    {
        if(!Sunlight || !LightParent)
        {
            LightParent = GameObject.FindGameObjectWithTag("Light").transform;
            if(LightParent)
            {
                if(LightParent.GetComponent<Light>())
                {
                    Sunlight = LightParent.GetComponent<Light>();
                    LightParent = LightParent.parent;
                }
                else
                {
                    Sunlight = LightParent.GetChild(0).GetComponent<Light>();
                }
            }
            else
            {
                Debug.LogWarning("Sunlight-Source or Sunlight-Parent not assigned!");
            }
        }
        else if(!Sunlight || !LightParent)
        {
            if(!Sunlight)
            {
                Sunlight = LightParent.GetComponentInChildren<Light>();
                if(!Sunlight)
                {
                    Debug.LogWarning("couldnt find Sunlight-Source!");
                }
            }
            else
            {
                LightParent = Sunlight.transform.parent;
                if(!LightParent)
                {
                    Debug.LogWarning("couldnt find Sunlight-Parent!");
                }
            }
        }

        if(Relations.Count == 0)
        {
            Relations.Add(playerEnemies);
            Relations.Add(monsterEnemies);
            Relations.Add(banditEnemies);
            Relations.Add(cityEnemies);
        }
    }

    // Update is called once per frame
    void Update()
    {
        float tempTime = timePassed % TimePerDay / TimePerDay;
        if(tempTime >= 0.25 && tempTime <= 0.75)
        {
            if(tempTime < 0.27)
            {
                Sunlight.intensity = (tempTime - 0.25f) / 0.02f;
            }
            else if(tempTime > 0.73)
            {
                Sunlight.intensity = 1 - (tempTime - 0.73f) / 0.02f;
            }
            else
            {
                Sunlight.intensity = 1;
            }

            LightParent.rotation = Quaternion.Euler((2 * tempTime - 0.5f) * new Vector3(180, 0 , 0));
            Sunlight.color = SunlightColor.Evaluate(2 * tempTime - 0.5f);
        }
        
        timePassed += Time.deltaTime;

        if(currentKey != -1)
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                currentKey = -1;
            }
            else if(Input.anyKeyDown)
            {
                foreach(KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
                {
                    if(Input.GetKeyDown(keyCode))
                    {
                        ChangeKey(keyCode);
                        SaveKeys();
                        UpdateOptions();
                    }
                }
            }
        }
        else if(Input.GetKeyDown(OpenOptions))
        {
            if(Options.activeSelf)
            {
                Options.SetActive(false);
                OptionsButton.SetActive(true);
                PlayerController.playerContr.unloadInv();
            }
            else
            {
                Options.SetActive(true);
                OptionsButton.SetActive(false);
                PlayerController.playerContr.unloadInv();
                UpdateOptions();
            }
        }
    }

    public void UpdateOptions()
    {
        KeyLabels[0].text = "Forward: " + Forward;
        KeyLabels[1].text = "Backwards: " + Backwards;
        KeyLabels[2].text = "Left: " + Left;
        KeyLabels[3].text = "Right: " + Right;
        KeyLabels[4].text = "Sprint: " + Sprint;
        KeyLabels[5].text = "Jump: " + Jump;
        KeyLabels[6].text = "Attack: " + Attack;
        KeyLabels[7].text = "SelectTarget: " + SelectTarget;
        KeyLabels[8].text = "Inv: " + Inv;
        KeyLabels[9].text = "InvInteract: " + InvInteract;
        KeyLabels[10].text = "InvMove: " + InvMove;
        KeyLabels[11].text = "Options: " + OpenOptions;

        LoadingSaveDropdown.ClearOptions();
        LoadingSaveDropdown.AddOptions(GetSaves());
        LoadingSaveDropdown.value = 0;

        DeletingSaveDropdown.ClearOptions();
        DeletingSaveDropdown.AddOptions(GetSaves());
        DeletingSaveDropdown.value = 0;

        SaveFilename.text = null;
    }

    public void SetCurrKey(int i)
    {
        currentKey = i;
    }

    public void Teleport(Vector3 Pos, Quaternion Rot)
    {
        PlayerController.playerContr.transform.SetPositionAndRotation(DestinationPos, DestinationRot);
    }
    
    void ChangeKey(KeyCode key)
    {
        if(currentKey == -1)
        {
            return;
        }
        if(currentKey == 0)
        {
            Forward = key;
        }
        else if(currentKey == 1)
        {
            Backwards = key;
        }
        else if(currentKey == 2)
        {
            Left = key;
        }
        else if(currentKey == 3)
        {
            Right = key;
        }
        else if(currentKey == 4)
        {
            Sprint = key;
        }
        else if(currentKey == 5)
        {
            Jump = key;
        }
        else if(currentKey == 6)
        {
            Attack = key;
        }
        else if(currentKey == 7)
        {
            SelectTarget = key;
        }
        else if(currentKey == 8)
        {
            Inv = key;
        }
        else if(currentKey == 9)
        {
            InvInteract = key;
        }
        else if(currentKey == 10)
        {
            InvMove = key;
        }
        else if(currentKey == 11)
        {
            OpenOptions = key;
        }
        currentKey = -1;
    }

    public void LoadGame(int x)
    {
        SceneManager.LoadScene(x);      //0 = Main Menu, 1 = Game Scene
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void DisplayActive( int display)
    {
        if(display == 1)
        {
            PlayerController.playerContr.InvActive = !PlayerController.playerContr.InvActive;
        }
        else if(display == 2)
        {
            PlayerController.playerContr.EquipmentActive = !PlayerController.playerContr.EquipmentActive;
        }
        else if(display == 3)
        {
            PlayerController.playerContr.PickUpActive = !PlayerController.playerContr.PickUpActive;
        }
        else if(display == 4)
        {
            PlayerController.playerContr.DescriptionActive = !PlayerController.playerContr.DescriptionActive;
        }
        PlayerController.playerContr.unloadInv();
        PlayerController.playerContr.loadInv();
    }

    public void Save(string FileName = null)
    {
        SaveKeys();

        if(FileName == null || FileName == "")
        {
            if(SaveFilename.text == null || SaveFilename.text == "")
            {
                int days = (int)(timePassed / TimePerDay);
                float dayFraction = (timePassed % TimePerDay) / TimePerDay;
                int hours = (int)(dayFraction * 24);
                int minutes = (int)((dayFraction * 24 - hours) * 60);
                
                FileName = "Save_" + days + "-" + hours + "-" + minutes;
            }
            else
            {
                FileName = SaveFilename.text;
            }
        }

        string directoryPath = Path.GetDirectoryName(Application.persistentDataPath + "/" + FileName + ".dat");
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
        
        BinaryFormatter bf = new();
        FileStream file = File.Create(Application.persistentDataPath + "/" + FileName + ".dat");     //creates File with Name: playerInfo in Roaming


        Stats stats = PlayerController.playerContr.playerStats;
        Inventory inv = PlayerController.playerContr.inventory;
        SaveSceneData();
        SaveData data = new()
        {
            menuData = new()
            {
                TimePassed = timePassed,
                relations = Relations
            },
            
            playerData = new()
            {
                Pos = new float[] {PlayerController.playerContr.transform.position.x, PlayerController.playerContr.transform.position.y, PlayerController.playerContr.transform.position.z},
                Rot = new float[] {PlayerController.playerContr.transform.rotation.x, PlayerController.playerContr.transform.rotation.y, PlayerController.playerContr.transform.rotation.z, PlayerController.playerContr.transform.rotation.w},
                
                stats = GetStatsData(PlayerController.playerContr.playerStats),

                inventory = GetInventoryData(inv),
            },
            scenesData = currentScenesData,
            currentScene = SceneManager.GetActiveScene().buildIndex
        };

        bf.Serialize(file, data);       //writes the data to the file
        file.Close();

        Debug.Log("Saved succesfully! " + Application.persistentDataPath + "/" + FileName + ".dat");
    }

    public void Load(string FileName = null)
    {
        finishedLoadingNPCs = false;
        LoadKeys();

        if(FileName == null || FileName == "")
        {
            if(Options.activeSelf)
            {
                if(LoadingSaveDropdown.value < LoadingSaveDropdown.options.Count)
                {
                    FileName = LoadingSaveDropdown.options[LoadingSaveDropdown.value].text;
                    Debug.Log("Got Name from Dropdown: ''" + FileName + "''");
                }
                else
                {
                    Debug.LogWarning("There arent any Saves to load!");
                }
            }
            else
            {
                Debug.LogWarning("Couldnt find the FileName!");
                return;
            }
        }

        Debug.Log("Attempting to load ''" + FileName + "''!");
        
        if(File.Exists(Application.persistentDataPath + "/" + FileName + ".dat"))
        {
            BinaryFormatter bf = new();
            FileStream file = File.OpenRead(Application.persistentDataPath + "/" + FileName + ".dat");
            SaveData data = (SaveData) bf.Deserialize(file);
            file.Close();

            timePassed = data.menuData.TimePassed;
            Relations = data.menuData.relations;

            LoadSceneData(data.scenesData[data.currentScene]);
            if(LoadPlayerData(data.playerData) == null)
            {
                Debug.Log("Player not loaded correctly!");
            }

            Debug.Log("Finished loading!");
            finishedLoadingNPCs = true;
            Options.SetActive(false);
            OptionsButton.SetActive(true);
            PlayerController.playerContr.unloadInv();
        }
        else
        {
            Debug.Log("Save wasnt found!");
        }
    }

    public void Delete(string FileName = null)
    {
        if(FileName == null || FileName == "")
        {
            if(Options.activeSelf)
            {
                if(DeletingSaveDropdown.value < DeletingSaveDropdown.options.Count)
                {
                    FileName = DeletingSaveDropdown.options[DeletingSaveDropdown.value].text;
                }
                else
                {
                    Debug.LogWarning("There arent any Saves to delete!");
                }
            }
            else
            {
                Debug.LogWarning("Couldnt find the FileName!");
                return;
            }
        }
        
        if(File.Exists(Application.persistentDataPath + "/" + FileName + ".dat"))
        {
            File.Delete(Application.persistentDataPath + "/" + FileName + ".dat");

            Debug.Log("Deleted succesfully!");
        }
        else
        {
            Debug.Log("Save wasnt found!");
        }
    }

    public List<string> GetSaves()
    {
        var result = Directory.GetFiles(Application.persistentDataPath, "*.dat").Select(path => Path.GetFileNameWithoutExtension(path));
        
        return result.OrderByDescending(path => File.GetCreationTime(Path.Combine(Application.persistentDataPath, $"{path}.dat"))).ToList();
    }

    public void ChangeScenes(int Scene)
    {
        SaveSceneData();
        SceneManager.LoadScene(Scene);
        if(currentScenesData[Scene] != null)    //if the Scene was already saved before
        {
            LoadSceneData(currentScenesData[Scene]);
        }
        PlayerController.playerContr.transform.SetPositionAndRotation(DestinationPos, DestinationRot);
    }

    public bool IsInList(GameObject GO)
    {
        foreach(ListItem LI in currentNPCs)
        {
            if(LI.GO == GO)
            {
                return true;
            }
        }

        ListItem temp = new();
        temp.ID = GO.GetInstanceID();
        temp.GO = GO;
        currentNPCs.Add(temp);
        return false;
    }

    void LoadKeys()
    {
        if(PlayerPrefs.GetString("Forward") != null)
        {
            Forward = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Forward"));
            Backwards = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Backwards", Backwards.ToString()));
            Left = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Left", Left.ToString()));
            Right = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Right", Right.ToString()));
            Sprint = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Sprint", Sprint.ToString()));
            Jump = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Jump", Jump.ToString()));
            Attack = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Attack", Attack.ToString()));
            SelectTarget = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("SelectTarget", SelectTarget.ToString()));
            Inv = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Inv", Inv.ToString()));
            InvInteract = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("InvInteract", InvInteract.ToString()));
            InvMove = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("InvMove", InvMove.ToString()));
            OpenOptions = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("OpenOptions", OpenOptions.ToString()));
        }
        else
        {
            SaveKeys();
        }
    }

    void SaveKeys()
    {
        PlayerPrefs.SetString("Forward", Forward.ToString());
        PlayerPrefs.SetString("Backwards", Backwards.ToString());
        PlayerPrefs.SetString("Left", Left.ToString());
        PlayerPrefs.SetString("Right", Right.ToString());
        PlayerPrefs.SetString("Sprint", Sprint.ToString());
        PlayerPrefs.SetString("Jump", Jump.ToString());
        PlayerPrefs.SetString("Attack", Attack.ToString());
        PlayerPrefs.SetString("SelectTarget", SelectTarget.ToString());
        PlayerPrefs.SetString("Inv", Inv.ToString());
        PlayerPrefs.SetString("InvInteract", InvInteract.ToString());
        PlayerPrefs.SetString("InvMove", InvMove.ToString());
        PlayerPrefs.SetString("OpenOptions", OpenOptions.ToString());
    }

    List<ItemData> GetItemsData(List<GameObject> Items)
    {
        List<ItemData> result = new();

        if(Items.Count == 0)
        {
            return result;
        }

        ItemData itemData;
        foreach (GameObject Item in Items)
        {
            itemData = GetItemData(Item);
            if(itemData != null)
            {
                result.Add(itemData);
            }
        }
        return result;
    }

    ItemData GetItemData(GameObject Item)
    {
        if(Item == null)
        {
            return null;
        }
        
        if(Item.TryGetComponent<InvItem>(out InvItem invItem))
        {
            ItemData itemData = new()
            {
                PrefabNumber = invItem.PrefabNumber,
                Count = invItem.Count,
                Collectable = invItem.Collectable,
                Pos = new float[] {invItem.transform.position.x, invItem.transform.position.y, invItem.transform.position.z},
                Rot = new float[] {invItem.transform.rotation.x, invItem.transform.rotation.y, invItem.transform.rotation.z, invItem.transform.rotation.w},
                activeSelf = Item.activeSelf,
                Name = invItem.Name
            };

            if (Item.TryGetComponent<WeaponStats>(out WeaponStats weaponStats))
            {
                itemData.Enchantment = weaponStats.DamageScript.Enchantment;
                weaponStats.Load();
            }
            else if (Item.TryGetComponent<ArmorStats>(out ArmorStats armorStats))
            {
                itemData.Enchantment = armorStats.Enchantment;
            }

            return itemData;
        }
        else
        {
            return null;
        }
    }

    InventoryData GetInventoryData(Inventory inv)
    {
        return new()
        {
            Inventory = GetItemsData(inv.inventory),
            EquipList = GetEquipData(inv.EquipList),
        };
    }

    GameObject LoadPlayerData(PlayerData playerData)
    {
        GameObject Player;
        
        if(PlayerController.playerContr == null)
        {
            if(PlayerPrefab == null)
            {
                Debug.LogWarning("Couldnt instantiate a new Player: There is no PlayerPrefab assigned!");
                return null;
            }
            else
            {
                Player = Instantiate( PlayerPrefab, new Vector3(playerData.Pos[0], playerData.Pos[1], playerData.Pos[2]), new Quaternion(playerData.Rot[0], playerData.Rot[1], playerData.Rot[2], playerData.Rot[3]));
                Debug.Log("Successfully instantiated new Player GameObject!");
            }
        }
        else
        {
            Debug.Log("Player was already instantiated!");
            Player = PlayerController.playerContr.gameObject;
            Player.transform.position = new Vector3(playerData.Pos[0], playerData.Pos[1], playerData.Pos[2]);
            Player.transform.rotation = new Quaternion(playerData.Rot[0], playerData.Rot[1], playerData.Rot[2], playerData.Rot[3]);
        }
        
        foreach(InvItem x in Player.GetComponentsInChildren<InvItem>(true))
        {
            Destroy(x.gameObject);
        }

        if(Player.TryGetComponent<PlayerController>(out PlayerController tempPlayer))
        {
            LoadStatsData(playerData.stats, Player.GetComponent<Stats>());
            LoadInventoryData(playerData.inventory, Player.GetComponent<Inventory>());
        }
        else
        {
            Destroy(Player);
            Debug.LogWarning("Couldnt find PlayerController!");
            return null;
        }

        Debug.Log("Finished loading Player successfully!");
        return Player;
    }

    void LoadInventoryData(InventoryData invData, Inventory inv)
    {
        if(invData == null || inv == null)
        {
            Debug.LogWarning("No Inventory Script or InventoryData attached to Load!");
            return;
        }

        inv.inventory = LoadItemsData(invData.Inventory, inv.InvParent.transform);
        inv.EquipList = LoadEquipData(invData.EquipList);
        inv.SetEquipParents();
        inv.GetEquipment();
        inv.FindCoins();
        Debug.Log("Finished loading Inventory of " + inv.gameObject.name);
    }
    
    GameObject[] LoadEquipData(ItemData[] Items)
    {
        GameObject[] result = new GameObject[10];
        GameObject Item;

        for(int i = 0; i < Items.Length; i ++)
        {
            if(i < result.Length)
            {
                if(Items[i] != null)
                {
                    Item = LoadItemData(Items[i], null);
                    Debug.Log("New Item " + Item.name + "; " + Item.GetInstanceID());
                    if(Item != null)
                    {
                        result[i] = Item;
                    }
                }
            }
            else
            {
                Debug.LogWarning("The EquipDataItems Length is too long!");
            }
        }
        return result;
    }

    ItemData[] GetEquipData(GameObject[] Items)
    {
        ItemData[] result = new ItemData[10];
        ItemData Item;

        for(int i = 0; i < Items.Length; i ++)
        {
            if(i < result.Length)
            {
                if(Items[i] != null)
                {
                    Item = GetItemData(Items[i]);
                    if(Item != null)
                    {
                        result[i] = Item;
                    }
                }
            }
        }
        return result;
    }

    List<GameObject> LoadItemsData(List<ItemData> Items, Transform Parent = null)
    {
        List<GameObject> result = new();
        GameObject Item;

        foreach(ItemData itemData in Items)
        {
            Item = LoadItemData(itemData, Parent);
            if(Item != null)
            {
                result.Add(Item);
            }
        }
        return result;
    }

    GameObject LoadItemData(ItemData itemData, Transform parent = null)
    {
        if(itemData.PrefabNumber >= ItemPrefabs.Count)
        {
            return null;
        }
        GameObject Item;

        Item = Instantiate( ItemPrefabs[itemData.PrefabNumber], new Vector3(itemData.Pos[0], itemData.Pos[1], itemData.Pos[2]), new Quaternion(itemData.Rot[0], itemData.Rot[1], itemData.Rot[2], itemData.Rot[3]));
        if(parent)
        {
            Item.transform.SetParent(parent, false);
        }
        
        if(Item.TryGetComponent<InvItem>(out InvItem invItem))
        {
            invItem.Count = itemData.Count;
            invItem.Collectable = itemData.Collectable;
            invItem.Name = itemData.Name;

            if(itemData.Enchantment != 0)
            {
                if (Item.TryGetComponent<WeaponStats>(out WeaponStats weaponStats))
                {
                    weaponStats.DamageScript.Enchantment = itemData.Enchantment;
                }
                else if (Item.TryGetComponent<ArmorStats>(out ArmorStats armorStats))
                {
                    armorStats.Enchantment = itemData.Enchantment;
                }
            }
        }
        Item.SetActive(itemData.activeSelf);
        return Item;
    }

    StatsData GetStatsData(Stats stats)
    {
        return new()
        {
            Hitpoints = stats.Hitpoints,
            MaxHitpoints = stats.MaxHitpoints,
            Mana = stats.Mana,
            MaxMana = stats.MaxMana,
            Experience = stats.Experience,
            Attributes = new int[]
            {
                stats.Strength,
                stats.Constitution,
                stats.Dexterity,
                stats.Intelligence,
                stats.Wisdom,
                stats.Charisma
            },
            Level = stats.Level,
            Class = stats.CharacterClass,
            StatusEffects = stats.StatusEffects
        };
    }

    NPCData GetNPCData(NPCharacter npcScript)
    {
        NPCData result = new()
        {
            stats = GetStatsData(npcScript.stats),
            inventory = GetInventoryData(npcScript.inventory),
            Pos = new float[] {npcScript.transform.position.x, npcScript.transform.position.y, npcScript.transform.position.z},
            Rot = new float[] {npcScript.transform.rotation.x, npcScript.transform.rotation.y, npcScript.transform.rotation.z, npcScript.transform.rotation.w},
            InvSelling = npcScript.InvSelling,
            Affiliation = npcScript.Affiliation,
            ID = npcScript.gameObject.GetInstanceID(),
            PrefabNumber = npcScript.PrefabNumber,
            activeSelf = npcScript.gameObject.activeSelf
        };
        if(npcScript.personalEnemies.Count > 0)
        {
            foreach(GameObject Enemy in npcScript.personalEnemies)
            {
                if(Enemy.TryGetComponent<NPCharacter>(out NPCharacter x))
                {
                    result.personalEnemies.Add(Enemy.GetInstanceID());
                }
                else if(Enemy.TryGetComponent<PlayerController>(out PlayerController y))
                {
                    result.playerEnemy = true;
                }
            }
        }
        if(npcScript.gameObject.TryGetComponent<GenerateItems>(out GenerateItems GI))
        {
            result.generateItemsData = GetGenerateItemsData(GI);
        }
        
        return result;
    }

    GenerateItemsData GetGenerateItemsData(GenerateItems GenerateData)
    {
        return new()
        {
            ItemRate = GenerateData.ItemRate,
            ItemMinAmount = GenerateData.ItemMinAmount,
            ItemMaxAmount = GenerateData.ItemMaxAmount,
            WeaponPrefabs = GetItemsData(GenerateData.Weapon.ToList()).ToArray(),
            WeaponRate = GenerateData.WeaponRate,
            EquipmentPrefabs = GetItemsData(GenerateData.Equipment.ToList()).ToArray(),
            EquipmentRate = GenerateData.EquipmentRate,
            DropPrefabs = GetItemsData(GenerateData.Drops.ToList()).ToArray(),
            DropRate = GenerateData.DropRate,
            DropMinAmount = GenerateData.DropMinAmount,
            DropMaxAmount = GenerateData.DropMaxAmount
        };
    }

    GameObject LoadNPCData(NPCData npcData, Transform Parent)
    {
        if(npcData.PrefabNumber >= NPCPrefabs.Count)
        {
            Debug.LogWarning("PrefabNumber doesn't match any known ones!");
            return null;
        }

        GameObject NPC = Instantiate( NPCPrefabs[npcData.PrefabNumber], new Vector3(npcData.Pos[0], npcData.Pos[1], npcData.Pos[2]), new Quaternion(npcData.Rot[0], npcData.Rot[1], npcData.Rot[2], npcData.Rot[3]));
        if(Parent)
        {
            NPC.transform.SetParent(Parent, true);
        }
        Debug.Log("Instantiated new NPC: " + NPC.name);

        foreach(InvItem x in NPC.GetComponentsInChildren<InvItem>())
        {
            Debug.Log("Destroying " + x.name + " to avoid duplicates!");
            Destroy(x.gameObject);
        }
        
        if(NPC.TryGetComponent<NPCharacter>(out NPCharacter tempNPC))
        {
            tempNPC.Affiliation = npcData.Affiliation;
            tempNPC.EnemyIDs = npcData.personalEnemies;
            tempNPC.GenerateAttributes = false;
            if(npcData.playerEnemy)
            {
                tempNPC.personalEnemies.Add(PlayerController.playerContr.gameObject);
            }

            LoadStatsData(npcData.stats, NPC.GetComponent<Stats>());
            LoadInventoryData(npcData.inventory, NPC.GetComponent<Inventory>());
        }
        else
        {
            Debug.LogWarning("Couldnt find NPCharacter on " + NPC.name);
            Destroy(NPC);
            return null;
        }

        if(npcData.generateItemsData != null)
        {
            GenerateItems x = NPC.AddComponent<GenerateItems>();
            GenerateItemsData y = npcData.generateItemsData;
            
            x.ItemRate = y.ItemRate;
            x.ItemMinAmount = y.ItemMinAmount;
            x.ItemMaxAmount = y.ItemMaxAmount;
            x.Weapon = LoadItemsData(y.WeaponPrefabs.ToList(), null).ToArray();
            x.WeaponRate = y.WeaponRate;
            x.Equipment = LoadItemsData(y.EquipmentPrefabs.ToList()).ToArray();
            x.EquipmentRate = y.EquipmentRate;
            x.Drops = LoadItemsData(y.DropPrefabs.ToList()).ToArray();
            x.DropRate = y.DropRate;
            x.DropMinAmount = y.DropMinAmount;
            x.DropMaxAmount = y.DropMaxAmount;

            Debug.Log("Saved Generate Items for " + NPC.name);
        }

        NPC.SetActive(npcData.activeSelf);

        return NPC;
    }

    private void LoadStatsData(StatsData statsData, Stats stats)
    {
        stats.Hitpoints = statsData.Hitpoints;
        stats.MaxHitpoints = statsData.MaxHitpoints;
        stats.Mana = statsData.Mana;
        stats.MaxMana = statsData.MaxMana;
        stats.Strength = statsData.Attributes[0];
        stats.Constitution = statsData.Attributes[1];
        stats.Dexterity = statsData.Attributes[2];
        stats.Intelligence = statsData.Attributes[3];
        stats.Wisdom = statsData.Attributes[4];
        stats.Charisma = statsData.Attributes[5];
        stats.Experience = statsData.Experience;
        stats.Level = statsData.Level;
        stats.CharacterClass = statsData.Class;
        stats.StatusEffects = statsData.StatusEffects;
    }

    SceneData GetSceneData()
    {
        Debug.Log("Getting Scene Data!");
        SceneData result = new();

        result.NPCs = new();
        foreach(ListItem currNPC in currentNPCs)
        {
            if(currNPC != null )
            {
               if(currNPC.GO != null)
               {
                    if(currNPC.GO.TryGetComponent<NPCharacter>(out NPCharacter npc))
                    {
                        Debug.Log("Got NPCharacter: " + npc.name);
                        NPCData tempNPCData = GetNPCData(npc);
                        if(tempNPCData != null)
                        {
                            result.NPCs.Add(tempNPCData);
                        }
                    } 
               }
            }
        }
        result.PickUps = new();
        foreach(GameObject x in GetAllChests())
        {
            if(x != null)
            {
                if(x.TryGetComponent<PickUpScript>(out PickUpScript pickUp))
                {
                    PickUpData tempNPCData = GetPickUpData(pickUp);
                    if(tempNPCData != null)
                    {
                        result.PickUps.Add(tempNPCData);
                    }
                }
            }
        }
        result.Items = new();
        foreach(GameObject x in GetAllItems())
        {
            if(x != null)
            {
                if(x.TryGetComponent<InvItem>(out InvItem item))
                {
                    Transform t = x.transform;
                    bool alone = true;
                    while(t.parent != null)
                    {
                        t = t.parent;
                        if(t.TryGetComponent<Inventory>(out Inventory inv))
                        {
                            alone = false;
                            break;
                        }
                        else if(t.TryGetComponent<PickUpScript>(out PickUpScript pickUp))
                        {
                            alone = false;
                            break;
                        }
                        else if(t.TryGetComponent<Stats>(out Stats stats))
                        {
                            alone = false;
                            break;
                        }
                    }

                    if(alone)       //only Items that aren't already in an Inventory o.ä. are saved here
                    {
                        ItemData tempNPCData = GetItemData(x);
                        if(tempNPCData != null)
                        {
                            result.Items.Add(tempNPCData);
                            Debug.Log("Found individual Item: " + x.name);
                        }
                    }
                }
            }
        }

        return result;
    }

    List<GameObject> GetAllNPCs()
    {
        List<GameObject> result = new();
        foreach (NPCharacter npcScript in FindObjectsByType<NPCharacter>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            result.Add(npcScript.gameObject);
        }
        return result;
    }

    List<GameObject> GetAllItems()
    {
        List<GameObject> result = new();
        foreach (InvItem npcScript in FindObjectsByType<InvItem>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            result.Add(npcScript.gameObject);
        }
        return result;
    }

    List<GameObject> GetAllRestingGO()
    {
        List<GameObject> result = new();
        foreach (RestingStats npcScript in FindObjectsByType<RestingStats>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            result.Add(npcScript.gameObject);
        }
        return result;
    }

    List<GameObject> GetAllChests()
    {
        List<GameObject> result = new();
        foreach (PickUpScript npcScript in FindObjectsByType<PickUpScript>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            result.Add(npcScript.gameObject);
        }
        return result;
    }

    private PickUpData GetPickUpData(PickUpScript pickUp)
    {
        return new()
        {
            PrefabNumber = pickUp.PrefabNumber,
            Pos = new float[] { pickUp.transform.position.x, pickUp.transform.position.y, pickUp.transform.position.z },
            Rot = new float[] { pickUp.transform.rotation.x, pickUp.transform.rotation.y, pickUp.transform.rotation.z, pickUp.transform.rotation.w },
            LifeTime = pickUp.LifeTime,
            Name = pickUp.Name,
            Items = GetItemsData(pickUp.Items)
        };
    }

    public void SaveSceneData()
    {
        Debug.Log("Saving Scene!");
        currentScenesData[SceneManager.GetActiveScene().buildIndex] = GetSceneData();
    }

    void LoadSceneData(SceneData scene)
    {
        foreach(GameObject gO in GetAllItems())     //destroy all Items so there arent any duplicates
        {
            Debug.Log("1 " + gO.name + "; " + gO.GetInstanceID());
            Destroy(gO);
        }
        foreach(GameObject gO in GetAllNPCs())
        {
            Debug.Log("2 " + gO.name);
            Destroy(gO);
        }
        foreach(GameObject gO in GetAllChests())
        {
            Debug.Log("3 " + gO.name);
            Destroy(gO);
        }
        Debug.Log("Destroyed all other GameObjects!");

        LoadItemsData(scene.Items, null);
        Debug.Log("Loaded all independent Items!");
        foreach(NPCData npc in scene.NPCs)
        {
            if(npc != null)
            {
                LoadNPCData(npc, null);
            }
        }
        Debug.Log("Loaded all NPCs!");
        foreach(PickUpData pickUp in scene.PickUps)
        {
            if(pickUp != null)
            {
                LoadPickUpData(pickUp);
            }
        }
        Debug.Log("Loaded all PickUps!");
    }

    void LoadPickUpData(PickUpData pickUpData)
    {
        Quaternion Rot = new Quaternion(pickUpData.Rot[0], pickUpData.Rot[1], pickUpData.Rot[2], pickUpData.Rot[3]);
        Vector3 Pos = new Vector3(pickUpData.Pos[0], pickUpData.Pos[1], pickUpData.Pos[2]);
        PickUpScript PickUp = PickUpScript.CreatePickUp(PickUpPrefabs[pickUpData.PrefabNumber].transform, Pos, Rot, LoadItemsData(pickUpData.Items), pickUpData.Name, pickUpData.LifeTime);
        
        Debug.Log("Loaded PickUp: " + PickUp.Name);
    }

    public List<GameObject> GetEnemies(List<int> IDs)
    {
        List<GameObject> result = new();

        foreach (int id in IDs)
        {
            foreach(ListItem i in currentNPCs)
            {
                if(i.ID == id)
                {
                    result.Add(i.GO);
                }
            }
        }

        return result;
    }
}

[Serializable]
class SaveData
{
    public MenuData menuData;
    public PlayerData playerData;
    public SceneData[] scenesData;
    public int currentScene;
}

[Serializable]
class MenuData
{
    public float TimePassed;
    public List<List<int>> relations;
}

[Serializable]
class PlayerData
{
    public StatsData stats;
    public InventoryData inventory;
    public float[] Pos;
    public float[] Rot;
}

[Serializable]
class SceneData
{
    public List<NPCData> NPCs;
    public List<PickUpData> PickUps;
    public List<ItemData> Items;
}

[Serializable]
class StatsData
{
    public int Hitpoints;
    public int MaxHitpoints;
    public int Mana;
    public int MaxMana;
    public int[] Attributes;
    public int Experience;
    public int Level;
    public int Class;
    public List<float[]> StatusEffects;
}

[Serializable]
class InventoryData
{
    public List<ItemData> Inventory;
    public List<ItemData> PickUpList;
    public ItemData[] EquipList = new ItemData[10];
}

[Serializable]
class ItemData
{
    public int PrefabNumber;
    public int Count;
    public int Enchantment;
    public float[] Pos;
    public float[] Rot;
    public bool Collectable;
    public bool activeSelf;
    public string Name;
    public int ItemType;
}

[Serializable]
class NPCData
{
    public int ID;
    public int PrefabNumber;
    public StatsData stats;
    public InventoryData inventory;
    public float[] Pos;
    public float[] Rot;
    public bool InvSelling;
    public int Affiliation;
    public List<int> personalEnemies;
    public bool playerEnemy;
    public bool activeSelf;
    public GenerateItemsData generateItemsData;
}

[Serializable]
class GenerateItemsData
{
    public float[] ItemRate;
    public int[] ItemMinAmount;
    public int[] ItemMaxAmount;
    public ItemData[] WeaponPrefabs;
    public float[] WeaponRate;
    public ItemData[] EquipmentPrefabs;
    public float[] EquipmentRate;
    public ItemData[] DropPrefabs;
    public float[] DropRate;
    public int[] DropMinAmount;
    public int[] DropMaxAmount;
}

[Serializable]
class PickUpData
{
    public int PrefabNumber;
    public float[] Pos;
    public float[] Rot;
    public float LifeTime;
    public string Name;
    public List<ItemData> Items;
}

class ListItem
{
    public int ID;
    public GameObject GO;
}