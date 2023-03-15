using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using HQDotNet;

namespace RPGPTV {
    public class RPGPTVSceneChangeController : HQController, ICommandReceivedListener {
        private RPGPTVCommandModel? currentCommand = null;
        public override bool Startup() {
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            return base.Startup();
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1) {
            if(currentCommand.HasValue)
                Session.Dispatcher.Dispatch<ICommandCompletedListener>(listener => listener.CommandCompleted(currentCommand.Value));
        }

        void ICommandReceivedListener.CommandReceived(RPGPTVCommandModel command) {
            if(command.command == RPGPTVPromptController.COMMANDS.SCENE_CHANGE.ToString()) {
                currentCommand = command;
                try {
                    var model = JsonUtility.FromJson<RPGPTVSceneChangeModel>(command.data);
                    Debug.Log("Load Scene: " + model.sceneName);
                    MainThreadSyncer.Instance.ExecuteOnMainThread(() => {
                        SceneManager.LoadSceneAsync(model.sceneName);
                    });
                }
                catch(System.Exception e) {
                    Debug.LogException(e);
                }
            }
        }
    }
}