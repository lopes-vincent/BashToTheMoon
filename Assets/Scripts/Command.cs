using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Command")]
public class Command : ScriptableObject
{
    public string[] keywords;
    
    [TextArea] 
    public string text;

    public void RespondToInput(GameController controller, string[] separatedInputWords)
    {
        controller.LogStringWithReturn(text);

        Type controllerType = controller.GetType();
        var method = controllerType.GetMethod(this.name);
        if (null != method)
        {
            object[] parameters = {separatedInputWords};
            method.Invoke(controller, parameters);
        }

        controller.DisplayLogText();
    }
}
