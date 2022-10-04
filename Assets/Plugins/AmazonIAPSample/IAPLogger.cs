using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AmazonIAP
{
    public class IAPLogger
    {
        public static bool EnableLog { get; set; } = true;

        public static event System.Action<string> OnMessageReceived;

        public static void Log(string message)
        {
            if(EnableLog)
            {
                Debug.Log("[AmazonIAP] " + message);
                OnMessageReceived?.Invoke(message);
            }
        }
    }
}
