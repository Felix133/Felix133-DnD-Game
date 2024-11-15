public class MultiStepQuest
{
    public string QuestName;
    public string Description;
    public List<QuestStep> Steps = new List<QuestStep>();
    public bool IsCompleted;

    private int currentStepIndex = 0;

    public void CompleteCurrentStep()
    {
        if (currentStepIndex < Steps.Count)
        {
            Steps[currentStepIndex].IsCompleted = true;
            if (Steps[currentStepIndex].NextStepIndices.Count > 0)
            {
                // Wähle den nächsten Schritt basierend auf der Logik (z.B. zufällig oder basierend auf Bedingungen)
                currentStepIndex = Steps[currentStepIndex].NextStepIndices[0]; // Beispiel: Nimm den ersten nächsten Schritt
            }
            else
            {
                IsCompleted = true;
            }
        }
    }

    public QuestStep GetCurrentStep()
    {
        if (currentStepIndex < Steps.Count)
        {
            return Steps[currentStepIndex];
        }
        return null;
    }

    public float[] GetStepProgressOf(int StepIndex)
    {
        if(Steps.Count <= StepIndex)
        {
            return null;
        }

        if(Steps[StepIndex].IsCompleted)
        {
            return float{
                1,
                Steps[StepIndex].NeededAmount
            }
        }
        if(Steps[StepIndex].CurrentAmount > 0)
        {
            return float{
                Steps[StepIndex].CurrentAmount / Steps[StepIndex].NeededAmount,
                Steps[StepIndex].NeededAmount
            }
        }
    }
}