using UnityEngine;
using UnityEngine.Assertions;
using HQDotNet;
using HQDotNet.Unity;
using SMS.OpenAI;

namespace RPGPTV {
    public class RPGPTVSession : HQSessionSetupMonoBehavior {

        public SMSOpenAIConfigurationSettings openAIConfigSettings;

        /// <summary>
        /// Set up Unity Session
        /// </summary>
        public override void Awake() {

            base.Awake();
            DontDestroyOnLoad(gameObject);

            //Register Controllers
            _session.RegisterController<SMSOpenAIController>();
            _session.RegisterController<RPGPTVPromptController>();

            SendModelUpdates();
        }

        private void SendModelUpdates() {
            Assert.IsNotNull(openAIConfigSettings);

            //Here, we are choosing to use our own OpenAI configuration settings, so we can control the flow of when we connect to openai
            _session.Dispatcher.Dispatch<IModelListener<SMSOpenAIConfigurationSettings>>(listener => listener.OnModelUpdated(openAIConfigSettings));
        }
    }
}