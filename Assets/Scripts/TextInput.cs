using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextInput : MonoBehaviour
{
    public InputField inputField;
    
    private GameController _controller;
    private string PlayerNameChosen = "";
    private bool AlreadyTryOneName = false;
    
    int _logIndex = 1;
    List<string> _commandLog = new List<string>();
    

    private void Awake()
    {
        _controller = GetComponent<GameController>();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
        {
            AcceptStringInput(inputField.text);
            return;
        }
        
        if (Input.GetKeyUp(KeyCode.UpArrow) && _commandLog.Count > 0)
        {
            LoadLogInput();
            if (_commandLog.Count > _logIndex)
            {
                _logIndex++;
            }
        }
    
        if (Input.GetKeyUp(KeyCode.DownArrow) && _logIndex > 1)
        {
            _logIndex--;
            LoadLogInput();
        }
    }

    private void LoadLogInput()
    {
        inputField.text = _commandLog[_commandLog.Count - _logIndex];
        inputField.caretPosition = inputField.text.Length;
    }

    private void AcceptStringInput(string userInput)
    {
        if (userInput == "")
        {
            return;    
        }
        
        if (!_controller.IsQueueEmpty())
        {
            _controller.LogStringWithReturn("<color=red>Can you wait until I'm done talking?</color>");
            _controller.DisplayLogText();
            inputField.ActivateInputField();
            return;    
        }
        
        _logIndex = 1;
        userInput = userInput.ToLower();
        
        _controller.LogStringWithReturn(userInput, true);
        
        if (null == _controller.GetPlayerName())
        {
            InitPlayerName(userInput);
            // Stop here to prevent other command
            return;
        }
        
        char[] delimiterCharacters = {' '};
        string[] separatedInputWords = userInput.Split(delimiterCharacters);

        RespondFirstMatchingCommand(separatedInputWords);
        
        _commandLog.Add(userInput);
        InputComplete();
    }

    private void InitPlayerName(string userInput)
    {
        if (PlayerNameChosen == "")
        {
            if (userInput.Length > 20)
            {
                _controller.EnqueueMessage("Please choose a username with a maximum of 20 character. Otherwise the terminal renderer will be ugly. ");
                _commandLog.Add(userInput);
                InputComplete();
                return;
            }
            
            PlayerNameChosen = userInput;
            if (!AlreadyTryOneName)
            {
                AlreadyTryOneName = true;
                _controller.EnqueueMessage("Seriously this is really your name ???");
                _controller.EnqueueMessage("Ok specially for you and your weird name you can use another name and not necessarily your real name this time !");
                _controller.EnqueueMessage("Do you still want to keep this name ?");
            }
            else
            {
                _controller.EnqueueMessage("Ok... are you sure this time ?");
            }
        } else if (userInput == "yes")
        {
            _controller.SetPlayerName(PlayerNameChosen);
            _controller.EnqueueMessage("Ok let's go for "+PlayerNameChosen+" !");
        } else if (userInput == "no") {
            PlayerNameChosen = "";
            _controller.EnqueueMessage("Ok say me another name please.");
        } else
        {
            _controller.EnqueueMessage("I don't understand, do you want to use this name or not ?");
        }
        
        _commandLog.Add(userInput);
        InputComplete();
    }

    protected void RespondFirstMatchingCommand(string[] separatedInputWords)
    {
        for (int i = 0; i < _controller.commands.Length; i++)
        {
            Command command = _controller.commands[i];
            for (int x = 0; x < separatedInputWords.Length; x++)
            {
                bool exist = Array.Exists(command.keywords, element => element == separatedInputWords[x]);
                if (exist)
                {
                    command.RespondToInput(_controller, separatedInputWords);
                    return;
                }
            }
        }
        
        _controller.EnqueueMessage("Sorry I don't understand what do you mean...");
    }

    void InputComplete()
    {
        _controller.DisplayLogText();
        inputField.ActivateInputField();
        inputField.text = null;
    }
}
