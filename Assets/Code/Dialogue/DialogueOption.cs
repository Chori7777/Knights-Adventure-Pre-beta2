
using UnityEngine.Events;

public class DialogueOption
{
    public string text;
    public UnityAction callback;

    public DialogueOption(string text, UnityAction callback = null)
    {
        this.text = text;
        this.callback = callback;
    }
}
