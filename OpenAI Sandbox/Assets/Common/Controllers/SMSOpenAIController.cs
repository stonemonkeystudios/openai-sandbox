using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HQDotNet;
using OpenAI;
using UnityEngine.Assertions;

namespace SMS.OpenAI {
    public class SMSOpenAIController : HQController, IModelListener<SMSOpenAIConfigurationSettings> {
        public OpenAIClient openAIClient;
        void IModelListener<SMSOpenAIConfigurationSettings>.OnModelUpdated(SMSOpenAIConfigurationSettings model) {
            Assert.IsNotNull(model);
            Assert.IsFalse(string.IsNullOrEmpty(model.apiKey));
            Assert.IsFalse(string.IsNullOrEmpty(model.orgId));

            OpenAIAuthentication auth = new OpenAIAuthentication(model.apiKey, model.orgId);
            openAIClient = new OpenAIClient(auth);
            Debug.Log("OpenAI Client created.");
        }
    }
}
