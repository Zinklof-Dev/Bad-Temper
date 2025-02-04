using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using ZinklofDev.ConsoleV2;

public class InputHandeler : MonoBehaviour
{
    [Header("TMP References")]
    [SerializeField] private TMPro.TextMeshProUGUI log;
    [SerializeField] private TMPro.TextMeshProUGUI possibleCommandsGUI;
    [SerializeField] private TMPro.TMP_InputField inputField;
    [Header("Console Anim")]
    [SerializeField] float targetYOpen;
    [SerializeField] float targetYClosed;
    [SerializeField] float lerp;
    [Header("Rect Transform References")]
    [SerializeField] RectTransform console;
    [SerializeField] RectTransform logContainer;
    [Header("Log Anim")]
    [SerializeField] float maxLogContainerY;
    [SerializeField] float minLogContainerY;
    [SerializeField] float logLerp;
    [SerializeField] float changeAmmount;

    private float targetLogContainerY;

    private string[] lastInputs = new string[50];
    private int currentIndex;

    private string[] possibleCommands;
    private string tabfill;

    private void Start()
    {
        Assembler.Initialize();

        Application.logMessageReceived += HandleUnityLog;
        Console.OnNewLog += UpdateLog;

        string knownProblems = "Currently known problems in this version:\n" +
            "Wrong Variables cause exceptions that sometimes kill the Shell\n" +
            "Missformating of () causes exceptions that sometimes kill the shell\n" +
            "GPU Driver may crash on particularly severe exceptions due to an issue on unity's end with texture math\n" +
            "Help command doesn't exist\n" +
            "Suggested commands don't tab auto fill, and freak out once you start entering variables\n" +
            "Console may spazz to the corner, zero clue why\n" +
            "Log container doesn't scroll, code is in place, variables are not set yet.\n" +
            "Console causes slowdown on start, increases exponentially with the ammount of assemblies, classes, and methods you have, this is an issue with using reflection, and can only be worked around in the future.";

        Console.Log("Welcome to Zinklof.DEV ConsoleV2! You are currently using " + Console.ReleaseType + " " + Console.ReleaseVersion + ", There are " + Console.CommandsRegistered + " commands registered in your project.", "Console", "0fffff", true, 22);
        Console.Log(knownProblems, "", "ff9b9b", true, 12);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tilde) || Input.GetKeyDown(KeyCode.BackQuote))
        {
            Console.isOpen = !Console.isOpen;

            if (Console.isOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                inputField.ActivateInputField();
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                inputField.DeactivateInputField();
                inputField.text = "";
                possibleCommandsGUI.text = "";
            }
        }

        if (Console.isOpen)
        {
            console.localPosition = Vector3.Lerp(console.localPosition, new Vector3(0, targetYOpen, 0), lerp);
            float y = Input.GetAxis("Mouse ScrollWheel") * changeAmmount * Time.deltaTime;
            targetLogContainerY = Mathf.Clamp(targetLogContainerY + y, minLogContainerY, maxLogContainerY);
            logContainer.localPosition = Vector3.Lerp(logContainer.localPosition, new Vector3(0,0,0), logLerp);

            if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            {
                AppendToLastInputs();
                Shell.CallCommand(inputField.text);
                inputField.text = "";
                inputField.ActivateInputField();
                currentIndex = -1;
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                inputField.text = tabfill;
                inputField.MoveTextEnd();
                inputField.Text += ")";
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (lastInputs[currentIndex + 1] == "" || lastInputs[currentIndex + 1] == string.Empty || lastInputs[currentIndex + 1] == null)
                {
                    return;
                }

                currentIndex++;
                if (currentIndex > 49)
                {
                    currentIndex = 49;
                }

                inputField.text = lastInputs[currentIndex];
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                currentIndex--;
                if (currentIndex < -1)
                {
                    currentIndex = -1;
                }

                if (currentIndex != -1)
                    inputField.text = lastInputs[currentIndex];
                else
                    inputField.text = "";
            }
        }
        else
            console.localPosition = Vector3.Lerp(console.localPosition, new Vector3(0, targetYClosed, 0), lerp);
    }

    public void OnInputChanged()
    {
        if (inputField.text == "")
        {
            possibleCommandsGUI.text = "";
            return;
        }

        string[] results = Shell.SearchForCommands(inputField.text);
        possibleCommands = results;

        tabfill = results[0].Split("(")[0];
        tabfill += "(";

        if (results.Length == 0)
        {
            possibleCommandsGUI.text = "";
            return;
        }

        for (int i = 0; i < results.Length; i++)
        {
            string partOne = "";
            string partTwo = "";
            string partThree = "";

            int index = results[i].ToLower().IndexOf(inputField.text.ToLower());

            if (index != -1)
            {
                partTwo = "<color=#ffffff>" + results[i].Substring(index, inputField.text.Length) + "</color>";
                partOne = results[i].Substring(0, index);
                partThree = results[i].Substring(index + inputField.text.Length);

                string total = partOne + partTwo + partThree;
                
                possibleCommands[i] = total;
            }
        }

        possibleCommandsGUI.text = "";

        foreach (string command in possibleCommands)
        {
            possibleCommandsGUI.text += command + "\n";
        }
    }

    private void AppendToLastInputs()
    {
        if (lastInputs[0] == inputField.text)
        {
            return;
        }

        string[] newArray = new string[50];

        newArray[0] = inputField.text;

        for (int i = 0; i < 49; i++)
        {
            newArray[i + 1] = lastInputs[i];
        }

        lastInputs = newArray;
    }

    private void UpdateLog()
    {
        log.text = "";

        for (int i = 49; i >= 0; i--)
        {
            if (Console.logs[i] != "" && Console.logs[i] != string.Empty && Console.logs[i] != null)
            log.text += Console.logs[i] + "\n";
        }
    }

    private void HandleUnityLog(string logString, string stackTrace, LogType type)
    {
        string hex = "";
        bool bold = false;
        float fontSize = 17;
        string prefix = "Unity Log";

        if (type == LogType.Exception) 
        {
            bold = true;
            fontSize = 22;
        }

        switch(type)
        {
            case LogType.Error:
                hex = "ff534a";
                break;
            case LogType.Warning:
                hex = "ffc107";
                break;
            case LogType.Log:
                hex = "f0f0f0";
                break;
            case LogType.Exception:
                hex = "ff534a";
                prefix = "EXCEPTION";
                break ;
            case LogType.Assert:
                hex = "ff534a";
                break;
        }

        Console.Log(logString, prefix, hex, bold, fontSize);
    }
}
