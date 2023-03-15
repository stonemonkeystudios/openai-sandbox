using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using HQDotNet.Unity;
using TMPro;

namespace RPGPTV {
    public class RPGPTVConversationView : HQMonoView, ICommandReceivedListener {

        public TMP_Text conversationText;
        public TMP_Text nameTextLeft;
        public float charactersPerSecondTick = 10f;
        public float waitBeforeClose = 3f;

        private float characterTickDuration;
        private RPGPTVCommandModel? currentCommand = null;

        protected override void Awake() {
            base.Awake();
            characterTickDuration = 1f / charactersPerSecondTick;
            DontDestroyOnLoad(gameObject);
            gameObject.SetActive(false);
        }

        void ICommandReceivedListener.CommandReceived(RPGPTVCommandModel command) {

            if(command.command == RPGPTVPromptController.COMMANDS.CHARACTER_SPEAK.ToString()) {
                Assert.IsNotNull(nameTextLeft);
                MainThreadSyncer.Instance.ExecuteOnMainThread(() => {
                    currentCommand = command;
                    gameObject.SetActive(true);
                    try {
                        var dialog = JsonUtility.FromJson<RPGPTVConversationCommandModel>(command.data);
                        Debug.Log("Dialog: " + dialog.characterName + " " + dialog.characterDialog);
                        nameTextLeft.text = dialog.characterName;
                        StartCoroutine(TickInText(dialog.characterDialog));
                    }
                    catch (System.Exception e) {
                        Debug.LogException(e);
                    }
                });
            }
        }

        IEnumerator TickInText(string text) {
            Assert.IsTrue(currentCommand.HasValue);
            Assert.IsNotNull(conversationText);

            string partialText = "";

            while(partialText.Length < text.Length) {
                partialText += text[partialText.Length];
                conversationText.text = partialText;
                yield return new WaitForSeconds(characterTickDuration);
            }
            yield return new WaitForSeconds(waitBeforeClose);
            gameObject.SetActive(false);
            var oldCommand = currentCommand;
            currentCommand = null;
            _session.Dispatcher.Dispatch<ICommandCompletedListener>(listener=>listener.CommandCompleted(oldCommand.Value));
        }
    }

}