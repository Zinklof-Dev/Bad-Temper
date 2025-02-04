using System.Data.SqlTypes;

namespace ZinklofDev.ConsoleV2
{
    public static class Console
    {
        public static string[] logs = new string[50];

        public delegate void ConsoleEventHandeler();
        public static event ConsoleEventHandeler OnNewLog;

        public static bool isOpen = false;
        public static readonly string ReleaseType = "Experimental Build"; // Should be "Release Build" or "Experiemental Build"
        public static readonly string ReleaseVersion = "E-2501B";
        public static int CommandsRegistered = 0;

        struct ConsoleLog
        {
            public string rawContent;
            public string hexColor;
            public float fontSize;
            public string? prefix;
            public bool bold;

            public ConsoleLog(string rawContent, string hexColor = "ffffff", bool bold = false, float fontSize = 17, string prefix = null)
            {
                this.rawContent = rawContent;
                this.hexColor = hexColor;
                this.bold = bold;
                this.fontSize = fontSize;
                this.prefix = prefix;
            }

            public override string ToString()
            {
                // Make new string
                string formatted = string.Empty;

                // If they put ffffff per say instead of #ffffff, make it #ffffff
                if (hexColor[1] != '#')
                {
                    string newHexColor = "#" + hexColor;
                    hexColor = newHexColor;
                }

                // Start this new string with the rich text color tag using our hex color
                formatted += $"<color={hexColor}>";

                // Now append the size tag with out font size
                formatted += $"<size={fontSize}>";

                // If the log should be bold append the bold tag
                if (bold)
                {
                    formatted += "<b>";
                }

                // If there is a prefix provided append a prefix
                if (prefix != "" && prefix != null && prefix != string.Empty)
                {
                    formatted += $"[{prefix}] ";
                }

                // Now append our raw content aka what the log should say
                formatted += rawContent;

                // End the color and size tag to avoid issues
                formatted += "</color></size>";

                // Lastly if the log was told to be bold, end that bold tag before it explodes
                if (bold)
                {
                    formatted += "</b>";
                }

                return formatted;
            }
        }

        public static void Log(object message, string prefix = null, string hexColor = "0fffff", bool bold = false, float fontSize = 17)
        {
            ConsoleLog log = new ConsoleLog(message.ToString(), hexColor, bold, fontSize, prefix);

            AppendLog(log);
            OnNewLog?.Invoke();
        }

        private static void AppendLog(ConsoleLog log)
        {
            string logContent = log.ToString();
            string[] newArray = new string[50];

            newArray[0] = logContent;

            for (int i = 0; i < 49; i++)
            {
                newArray[i + 1] = logs[i];
            }

            logs = newArray;
        }
    }
}
