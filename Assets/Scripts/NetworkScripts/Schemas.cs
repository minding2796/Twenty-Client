using System;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkScripts
{
    [Serializable]
    public class ErrorBody
    {
        public int errorId;
        public string message;
    }
    
    [Serializable]
    public class CommandMessage
    {
        public string command;
        public string data;
        
        public CommandMessage(string command, string data)
        {
            this.command = command;
            this.data = data;
        }
    }
    
    [Serializable]
    public class PlayerStatus
    {
        public long userId;
        public int currentFunds;
        public bool isReady;
        public int goStack;
        public int cooldown;
    }

    public class Void { }
}