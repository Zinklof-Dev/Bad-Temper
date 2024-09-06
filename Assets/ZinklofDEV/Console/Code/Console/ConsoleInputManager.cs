using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

namespace ZinklofDev.Console
{
    public class ConsoleInputManager : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputManager;

        private void Start()
        {
            inputManager.onValueChanged.AddListener(CheckInput);
        }

        private void CheckInput(string input)
        {
            if (input.EndsWith("\n"))
            {
                inputManager.SetTextWithoutNotify("");

                string alteredinput = input.Replace("\n", "");

                Log.LogCommand("> " + alteredinput);

                Shell.HandleCommand(alteredinput);
            }
        }
    }
}

