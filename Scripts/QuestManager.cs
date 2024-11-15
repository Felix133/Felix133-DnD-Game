public class QuestManager : MonoBehaviour
{
    public List<QuestData> questDatabase = new List<QuestData>();

    public List<MultiStepQuest> activeQuests = new List<MultiStepQuest>();

    void Start()
    {
        foreach (var questData in questDatabase)
        {
            MultiStepQuest quest = new MultiStepQuest
            {
                QuestName = questData.QuestName,
                Description = questData.Description,
                Steps = questData.Steps.Select(step => new QuestStep
                {
                    Description = step.Description,
                    IsCompleted = step.IsCompleted,
                    NextStepIndices = step.NextStepIndices
                }).ToList()
            };
            activeQuests.Add(quest);
        }
    }

    public void CheckQuests()
    {
        foreach (var quest in activeQuests)
        {
            if (!quest.IsCompleted)
            {
                var currentStep = quest.GetCurrentStep();
                if (currentStep != null && currentStep.IsCompleted)
                {
                    quest.CompleteCurrentStep();
                }
            }
        }
    }

    public int GetCurrentStepOnQuest(int QuestIndex)
    {
        if(activeQuests.Count <= QuestIndex)
        {
            return null;
        }

        return activeQuests[QuestIndex].GetCurrentStep();
    }

    public float[] GetStepProgressonQuest(int QuestIndex; int StepIndex = -1)
    {
        if(activeQuests.Count <= QuestIndex)
        {
            return null;
        }

        if(Stepindex == -1)     //show Progress on current Step
        {
            return activeQuest[QuestIndex].GetStepProgressOf(GetCurrentStepOnQuest(QuestIndex));
        }
        return activeQuest[QuestIndex].GetStepProgressOf(StepIndex);
    }
}