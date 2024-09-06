using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

namespace ZinklofDev.Console
{
    public static class ConsoleLogging
    {
        public enum GSLogType
        {
            Error,
            Warning,
            Command,
            Response,
            Misc,
        }

        public struct GSLog
        {
            public GSLogType type;
            public string log;
            public string locale;

            public GSLog(string message, string location, GSLogType logType)
            {
                this.log = message;
                this.type = logType;
                this.locale = location;
            }
        }
    }

    public class CommandBasic
    {
        private string commandId;
        private string commandFormat;
        private string commandDescription;
        public bool commandCheat;

        public string Id { get { return commandId; } }
        public string Format { get { return commandFormat; } }
        public string description { get { return commandDescription; } }

        public CommandBasic(string id, string format, string description, bool isCheat)
        {
            commandId = id;
            commandFormat = format;
            commandDescription = description;
            this.commandCheat = isCheat;
        }
    }

    public class Command : CommandBasic
    {
        private Action command;

        public Command(string id, string format, string description, bool isCheat, Action command) : base (id, format, description, isCheat)
        {
            this.command = command;
        }

        public void Invoke()
        {
            command.Invoke();
        }
    }

    public class Command<T1> : CommandBasic
    {
        private Action<T1> command;

        public Command(string id, string format, string description, bool isCheat, Action<T1> command) : base (id, format, description, isCheat)
        {
            this.command = command;
        }

        public void invoke(T1 t1)
        {
            command.Invoke(t1);
        }
    }

    public class Command<T1, T2> : CommandBasic
    {
        private Action<T1, T2> command;

        public Command(string id, string format, string description, bool isCheat, Action<T1, T2> command) : base(id, format, description, isCheat)
        {
            this.command = command;
        }

        public void invoke(T1 t1, T2 t2)
        {
            command.Invoke(t1, t2); 
        }
    }

    public class Command<T1, T2, T3> : CommandBasic
    {
        private Action<T1, T2, T3> command;

        public Command(string id, string format, string description, bool isCheat, Action<T1, T2, T3> command) : base(id, format, description, isCheat)
        {
            this.command = command;
        }

        public void invoke(T1 t1, T2 t2, T3 t3)
        {
            command.Invoke(t1, t2, t3);
        }
    }
}
