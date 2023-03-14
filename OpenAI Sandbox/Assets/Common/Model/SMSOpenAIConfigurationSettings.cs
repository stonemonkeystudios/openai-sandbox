using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SMS.OpenAI {

    [CreateAssetMenu(fileName = nameof(SMSOpenAIConfigurationSettings), menuName = "SMS/OpenAI/" + nameof(SMSOpenAIConfigurationSettings), order = 0)]
    [System.Serializable]
    public class SMSOpenAIConfigurationSettings : ScriptableObject {
        public string apiKey;
        public string orgId;
    }
}