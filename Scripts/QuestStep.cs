[System.Serializable]
public class QuestStep
{
    public string Description;
    public bool IsCompleted;
    public List<int> NextStepIndeces;   //List of next Steps after different outputs of the Quest
    public bool IsActive;           //if the Quest was already activated
    public float TimeToComplete;    //max time to complete the quest in 

    [Header("KillQuest")]
    public int NPCPrefab;
    public int RequiredAmount;
    public int CurrentAmount;

    [Header("GatheringQuest")]
    public int ItemPrefab;

    [Header("TalkingQuest")]
    public NPCharacter NPC;     //the NPC to talk to

}