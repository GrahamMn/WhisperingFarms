using UnityEngine;

public class PlantDialogue : MonoBehaviour
{
    [TextArea(3, 10)]
    public string[] dialogueLines;

    public void StartDialogue()
    {
        foreach (var line in dialogueLines)
        {
            Debug.Log(line); 
        }
    }
}
