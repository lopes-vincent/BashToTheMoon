using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    public Command[] commands;
    
    public Weather[] weathers;
    
    public string[] startMessages;

    public Text displayText;
    public Text nameText;
    public Text timeText;
    
    public InputField inputField;

    public Animator backgroundAnimator;
    public Animator foregroundAnimator;
    
    public Image rocketImage;
    public Image fireImage;
    
    public GameObject middlegroundObject;
    public GameObject moonObject;
    public GameObject lockObject;
    public GameObject launchTarget;

    public Sprite emptyRocketSprite;
    public Sprite filledRocketSprite;
    
    List<string> actionLog = new List<string>();
    
    private Weather _currentWeather;
    private string _playerName = "";
    private int _moonAngle = 0;
    private bool _askedToDo = false;
    private bool _askedHint = false;
    private int _rocketAngle = 90;
    private bool _tankFilled = false;
    private bool _engineStarted = false;
    private bool _unlocked = false;
    private bool _moveRocket = false;

    private bool _countDownInProgress = false;
    
    private Queue<string> _messageQueue = new Queue<string>();

    private void Start()
    {
        StaticClass.TimeElapsed = 0f;
        _playerName = StaticClass.PlayerName;

        if (null == _playerName)
        {
            foreach (var message in startMessages)
            {
                _messageQueue.Enqueue(message);
            }   
        }
        else
        {
            _messageQueue.Enqueue("Ok let's start again "+_playerName+" !" );
        }

        StartCoroutine(TreatMessageQueue());
        ChangeMoonAngle();
        ChangeWeather(2);
    }

    private void Update()
    {
        StaticClass.TimeElapsed += Time.deltaTime;
        timeText.text = StaticClass.TimeElapsed.ToString();
        
        if (!inputField.isFocused)
        {
            inputField.ActivateInputField();
        }

        if (_moveRocket)
        {
            float targetDistance = Vector3.Distance(rocketImage.transform.position, launchTarget.transform.position);
            float translateSpeed = targetDistance / 4 * Time.deltaTime;
            float scaleSpeed = targetDistance / 1000 * Time.deltaTime;
            rocketImage.transform.position = Vector3.MoveTowards(rocketImage.transform.position,  launchTarget.transform.position, translateSpeed);
            rocketImage.transform.localScale += new Vector3(-scaleSpeed, -scaleSpeed, 0);
        }
    }

    private IEnumerator TreatMessageQueue()
    {
        yield return new WaitWhile(() =>
        {
            return _messageQueue.Count < 1;
        });
        string message = _messageQueue.Dequeue();
        int stringLength = message.Length;
        LogStringWithReturn(message);
        DisplayLogText();
        float timeToWait = 0.25f + stringLength * 0.012f;
        yield return new WaitForSecondsRealtime(timeToWait);
        StartCoroutine(TreatMessageQueue());
    }

    public string GetPlayerName()
    {
        return _playerName;
    }

    public void SetPlayerName(string name)
    {
        _playerName = name;
        nameText.text = _playerName + " $";
        Canvas.ForceUpdateCanvases();
        int playerNameLength = CalculateLengthOfString(nameText.text, nameText);
        nameText.text = "<color=green>"+_playerName+"</color>" + " $";

        Vector3 currentInputPosition = inputField.transform.position;
        inputField.transform.position = new Vector3(currentInputPosition.x + playerNameLength * 1.08f, currentInputPosition.y, currentInputPosition.z);
    }

    private int CalculateLengthOfString(string String, Text text)
    {
        int totalLength = 0;
 
        Font textFont = text.font;
        CharacterInfo characterInfo = new CharacterInfo();
 
        char[] characters = String.ToCharArray();
 
        foreach(char character in characters)
        {
            textFont.RequestCharactersInTexture(character.ToString(), text.fontSize, text.fontStyle);
            textFont.GetCharacterInfo(character, out characterInfo, text.fontSize);  
 
            totalLength += characterInfo.advance;
        }
 
        return totalLength;
    }

    public void DisplayLogText()
    {
        string logAsText = string.Join("\n", actionLog.ToArray());
        displayText.text = logAsText;
        
        Canvas.ForceUpdateCanvases();
        float displayTextHeight = displayText.cachedTextGenerator.lineCount * 16;
        RectTransform displayTextRectTranform = displayText.GetComponent<RectTransform>();
        displayTextRectTranform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, displayTextHeight);
    }
    

    public void LogStringWithReturn(string stringToAdd, bool fromUser = false)
    {
        string text = stringToAdd + "\n";

        if (fromUser && _playerName != "")
        {
            text = System.DateTime.Now.ToShortTimeString() +" <color=green> " + _playerName + "</color> ~ " +text;
        }
        actionLog.Add(text);
    }

    public void ToDo(string[] separatedInputWords)
    {
        _askedToDo = true;
    }

    public void EnqueueMessage(string message)
    {
        _messageQueue.Enqueue(message);
    }

    public bool IsQueueEmpty()
    {
        return _messageQueue.Count < 1;
    }

    public void Checklist(string[] separatedInputWords)
    {
        if (_askedToDo)
        {
            EnqueueMessage("Oh what a good idea to read the check list it seems like you are very smart !");
        }
        
        EnqueueMessage("This is the checklist. She is very long, I hope you have a lot of time !");
        EnqueueMessage("[ ] Weather");
        EnqueueMessage("[ ] Angle");
        EnqueueMessage("[ ] Engine");
        EnqueueMessage("[ ] Unlock");
        EnqueueMessage("[ ] Launch");
    }

    public void Hint(string[] separatedInputWords)
    {
        if (!_askedHint)
        {
            _askedHint = true;
            EnqueueMessage("Are you very sure you want hint ? You do not want search by yourself ?");
            return;
        }
        
        EnqueueMessage("Okay...");
        EnqueueMessage("This is the hint : Launch the rocket !!!");
        _askedHint = false;
    }

    public void Weather(string[] separatedInputWords)
    {
        EnqueueMessage(_currentWeather.description);
    }

    public void Wait(string[] separatedInputWords)
    {
        ChangeWeather(null);
        ChangeMoonAngle();
    }

    public void Rotate(string[] separatedInputWords)
    {
        for (int i = 0; i < separatedInputWords.Length; i++)
        {
            string word = separatedInputWords[i].Replace("°", "");
            bool isInt = int.TryParse(word, out int angleAsked);
            if (isInt)
            {
                if (angleAsked != 45 && angleAsked != 90 && angleAsked != 135)
                {
                    EnqueueMessage("Sorry only this angle are possible : 45° | 90° | 135° ");
                    return;
                }
                
                _rocketAngle = angleAsked;
                int spriteAngle = 90 - angleAsked;
                middlegroundObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, spriteAngle));
                
                Vector3 launchTargetPosition = launchTarget.transform.localPosition;
                float launchTargetPositionX = -20 - (spriteAngle * 4);
                launchTarget.transform.localPosition = new Vector3(launchTargetPositionX, launchTargetPosition.y, launchTargetPosition.z);

                EnqueueMessage("Ok your rotate has been rocketed to "+angleAsked+" degree");
                return;
            }
        }
        
        EnqueueMessage("Please specify an angle for the rocket !");
    }

    public void Refuel(string[] separatedInputWords)
    {
        fillTank();;
    }

    public void Fire(string[] separatedInputWords)
    {
        //default text
        string text = "Humm humm... I think your forget to do something !";
        
        if (_engineStarted)
        {
            text = "It seems like the engine are already started ! ";
        }

        if (!_engineStarted && _tankFilled)
        {
            text = "Fire in the hole ! Starting the engines !";
            turnOnEngine();
        }

        EnqueueMessage(text);
    }

    public void Unlock(string[] separatedInputWords)
    {
        string text = _unlocked
            ? "The rocket is already unlocked and free as the air"
            : "All your base belong to us ! Heu no no sorry this is for another game... I would say all part of the rocket has been unlocked !";
        
        EnqueueMessage(text);
        lockObject.SetActive(false);

        _unlocked = true;
    }

    public void Launch(string[] separatedInputWords)
    {
        if (!_engineStarted)
        {
            StartCoroutine(LaunchWithoutEngineStarted());
            return;
        }

        LogStringWithReturn("Ok prepare yourself to an imminent launch !");
        DisplayLogText();

        if (!_unlocked)
        {
            StartCoroutine(LaunchWithoutUnlock());
            return;
        }

        StartCoroutine(LaunchSuccesful());
    }

    private void Reset()
    {
        Start();
        actionLog = new List<string>();
         _askedToDo = false;
        _askedHint = false;
        _rocketAngle = 90;
        emptyTank();
        turnOffEngine();
        _unlocked = false;
        
        turnOffEngine();
    }

    private void ChangeWeather(int? forceWeatherIndex)
    {
        int weatherIndex = forceWeatherIndex.HasValue ? forceWeatherIndex.Value : Random.Range(0, weathers.Length);
        _currentWeather = weathers[weatherIndex];
        TriggerAnimatorWeather(backgroundAnimator);
        TriggerAnimatorWeather(foregroundAnimator);
    }

    private void TriggerAnimatorWeather(Animator animator)
    {
        foreach (AnimatorControllerParameter parameter in  animator.parameters)
        {
            if (parameter.type != AnimatorControllerParameterType.Trigger)
            {
                continue;
            }
            
            if (_currentWeather.name.ToLower() == parameter.name.ToLower())
            {
                animator.SetTrigger(parameter.name);
                return;
            }
        }
        
        animator.SetTrigger("empty");
    }

    private void ChangeMoonAngle()
    {
        int[] possibleAngles = {45, 90, 135};
        _moonAngle = possibleAngles[Random.Range(0, possibleAngles.Length)];
        
        float moonPositionX = -20 + ((_moonAngle - 90) * 4);
        Vector3 moonObjectPosition = moonObject.transform.localPosition;
        moonObject.transform.localPosition = new Vector3(moonPositionX, moonObjectPosition.y, moonObjectPosition.z);;
    }

    private bool HasLaunchWeatherAccident()
    {
        int weatherSuccessRate = Random.Range(_currentWeather.minimumSuccessPercentage, _currentWeather.maximumSuccessPercentage);

        Debug.Log("ROLL");

        Debug.Log(weatherSuccessRate);
        int roll = Random.Range(0, 100);
        
        Debug.Log(roll);
        Debug.Log(roll > weatherSuccessRate);

        
        // If roll is > than success rate rocket have an accident
        return roll > weatherSuccessRate;
    }

    private IEnumerator CountDown()
    {
        _countDownInProgress = true;
        LogStringWithReturn("3");
        DisplayLogText();

        yield return new WaitForSecondsRealtime(1);
        LogStringWithReturn("2");
        DisplayLogText();

        yield return new WaitForSecondsRealtime(1);
        LogStringWithReturn("1");
        DisplayLogText();

        yield return new WaitForSecondsRealtime(1);
        LogStringWithReturn("0 !!!");
        DisplayLogText();
        
        yield return new WaitForSecondsRealtime(1);

        _countDownInProgress = false;
    }

    private IEnumerator LaunchWithoutEngineStarted()
    {
        LogStringWithReturn("You want launch a rocket without starting engines ?");
        LogStringWithReturn("Ok that's a concept, let's check...");
        DisplayLogText();
        yield return new WaitForSecondsRealtime(1);

        StartCoroutine(CountDown());

        yield return new WaitUntil(() => _countDownInProgress == false);
        LogStringWithReturn("...nothing happen, really weird... (－‸ლ)");
        DisplayLogText();
    }
    
    private IEnumerator LaunchWithoutUnlock()
    {
        StartCoroutine(CountDown());

        yield return new WaitUntil(() => _countDownInProgress == false);

        yield return new WaitForSecondsRealtime(1);
        LogStringWithReturn("Dayum the rocket stay stuck at the ground... I advise to check your checklist and verify everything is ok !");
        DisplayLogText();

        emptyTank();
        turnOffEngine();
    }
    
    private IEnumerator LaunchSuccesful()
    {
        StartCoroutine(CountDown());

        yield return new WaitUntil(() => _countDownInProgress == false);
        _moveRocket = true;
        
        yield return new WaitForSecondsRealtime(1);
        LogStringWithReturn("Woohooo it's ok the rocket is in the air !");
        DisplayLogText();
        
        yield return new WaitForSecondsRealtime(2);

        if (HasLaunchWeatherAccident())
        {
            StaticClass.LoseText = _currentWeather.accidentDescription;
            SceneManager.LoadScene("Lose");
        }

        Debug.Log("ANGLES");
        Debug.Log(_moonAngle);
        Debug.Log(_rocketAngle);
        if (_moonAngle != _rocketAngle)
        {
            LogStringWithReturn("Oh wait wait ! This is not the good direction ! The rocket is not going to the good direction !!!!");
            DisplayLogText();
            yield return new WaitForSecondsRealtime(1);

            LogStringWithReturn("Did you forget to set the good angle ???!!");
            DisplayLogText();
            yield return new WaitForSecondsRealtime(1);

            LogStringWithReturn("Congratulations you have lost a rocket in the infinite of space...");
            DisplayLogText();

            yield return new WaitForSecondsRealtime(3);

            // Todo display fail screen
            StaticClass.LoseText = "LOSE TO infinity";
            SceneManager.LoadScene("Lose");
        }
        
        // Todo display win screen
        SceneManager.LoadScene("Win");
    }

    private void turnOnEngine()
    {
        ChangeEngineState(true);
    }

    private void turnOffEngine()
    {
        ChangeEngineState(false);
    }

    private void ChangeEngineState(bool activated)
    {
        _engineStarted = activated;
        fireImage.gameObject.SetActive(activated);
    }

    private void fillTank()
    {
        _tankFilled = true;
        rocketImage.GetComponent<Image>().sprite = filledRocketSprite;
    }

    private void emptyTank()
    {
        _tankFilled = false;
        rocketImage.GetComponent<Image>().sprite = emptyRocketSprite;
    }
}
