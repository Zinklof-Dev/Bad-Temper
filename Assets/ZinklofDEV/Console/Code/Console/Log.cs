using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace ZinklofDev.Console
{
    public class Log : MonoBehaviour
    {
        [SerializeField] private TMP_Text consoleText;
        [SerializeField] static private TMP_Text console;
        static List<string> lines = new List<string>();

        void Start()
        {
            Application.logMessageReceived += HandleUnityLog;

            console = consoleText;
        }

        void HandleUnityLog(string logString, string stackTrace, LogType type)
        {
            string transformedLog = string.Empty;

            if (type == LogType.Error)
            {
                transformedLog = "<color=#FF0000>" + logString + "</color>";
            }
            else if (type == LogType.Warning)
            {
                transformedLog = "<color=#FFEB04>" + logString + "</color>";
            }
            else
            {
                transformedLog = "<color=#FFFFFF>" + logString + "</color>";
            }

            AddLog(transformedLog);
        }

        static void HandleGSLog(ConsoleLogging.GSLog log)
        {
            string transformedLog = string.Empty;

            if (log.type == ConsoleLogging.GSLogType.Error)
            {
                transformedLog = "<color=#FF0000>" + log.log + "</color>";
            }
            else if (log.type == ConsoleLogging.GSLogType.Warning)
            {
                transformedLog = "<color=#FFEB04>" + log.log + "</color>";
            }
            else if (log.type == ConsoleLogging.GSLogType.Command)
            {
                transformedLog = "<color=#0FFFFF>" + log.log + "</color>";
            }
            else if (log.type == ConsoleLogging.GSLogType.Response)
            {
                transformedLog = "<color=#aaaaaa>" + log.log + "</color>";
            }
            else
            {
                transformedLog = "<color=#FFFFFF>" + log.log + "</color>";
            }

            AddLog(transformedLog);
        }

        static public void ClearLog()
        {
            lines.Clear();
        }

        static void AddLog(string logString)
        {
            lines.Add(logString);

            if (lines.Count > 100)
            {
                lines.RemoveAt(0);
            }

            console.text = null;
            foreach (string line in lines)
            {
                string temp = line;
                console.text += "<br>" + line;
            }
        }

        public static void LogWarning(string message)
        {
            ConsoleLogging.GSLog temp = new ConsoleLogging.GSLog(message, string.Empty, ConsoleLogging.GSLogType.Warning);
            HandleGSLog(temp);
        }
        public static void LogWarning(string message, string scriptNameAndLine)
        {
            var temp = new ConsoleLogging.GSLog(message, scriptNameAndLine, ConsoleLogging.GSLogType.Warning);
            HandleGSLog(temp);
        }
        public static void LogError(string message)
        {
            var temp = new ConsoleLogging.GSLog(message, string.Empty, ConsoleLogging.GSLogType.Error);
            HandleGSLog(temp);
        }
        public static void LogError(string message, string scriptNameAndLine)
        {
            var temp = new ConsoleLogging.GSLog(message, scriptNameAndLine, ConsoleLogging.GSLogType.Error);
            HandleGSLog(temp);
        }
        public static void LogMisc(string message)
        {
            var temp = new ConsoleLogging.GSLog(message, string.Empty, ConsoleLogging.GSLogType.Misc);
            HandleGSLog(temp);
        }
        public static void LogMisc(string message, string scriptNameAndLine)
        {
            var temp = new ConsoleLogging.GSLog(message, scriptNameAndLine, ConsoleLogging.GSLogType.Misc);
            HandleGSLog(temp);
        }
        public static void LogCommand(string message)
        {
            var temp = new ConsoleLogging.GSLog(message, string.Empty, ConsoleLogging.GSLogType.Command);
            HandleGSLog(temp);
        }
        public static void LogCommand(string message, string scriptNameAndLine)
        {
            var temp = new ConsoleLogging.GSLog(message, scriptNameAndLine, ConsoleLogging.GSLogType.Command);
            HandleGSLog(temp);
        }
        public static void LogResponse(string message)
        {
            var temp = new ConsoleLogging.GSLog(message, string.Empty, ConsoleLogging.GSLogType.Response);
            HandleGSLog(temp);
        }
        public static void LogResponse(string message, string scriptNameAndLine)
        {
            var temp = new ConsoleLogging.GSLog(message, scriptNameAndLine, ConsoleLogging.GSLogType.Response);
            HandleGSLog(temp);
        }
    } 
}
