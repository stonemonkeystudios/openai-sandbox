using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HQDotNet.Unity;

namespace RPGPTV {

    public class RPGPTVSceneBackgroundView : HQMonoView, ICommandReceivedListener {
        public RPGPTVSceneBackgroundModel[] backgrounds;
        public Image fadeImage;
        public Image bgImage;
        public float fadeTime = 2f;
        public float waitToFadeTime = 1f;

        private RPGPTVCommandModel? currentCommand = null;

        public Sprite GetBackgroundForSceneModel(RPGPTVSceneChangeModel model) {
            foreach(var bg in backgrounds) {
                if (bg.sceneName == model.sceneName) {
                    if (model.isDay)
                        return bg.dayImage;
                    else
                        return bg.nightImage;
                }
            }
            return null;
        }

        private IEnumerator Fade(float from, float to, System.Action callback) {
            yield return new WaitForSeconds(waitToFadeTime);
            float startTime = Time.time;
            Color c = bgImage.color;
            while(startTime + fadeTime <= Time.time) {
                float t = (Time.time - startTime) / fadeTime;
                c.a = Mathf.Lerp(from, to, t);
                bgImage.color = c;
                yield return null;
            }
            callback?.Invoke();
        }

        void ICommandReceivedListener.CommandReceived(RPGPTVCommandModel command) {
            if (command.command == RPGPTVPromptController.COMMANDS.SCENE_CHANGE.ToString()) {
                currentCommand = command;
                try {
                    var model = JsonUtility.FromJson<RPGPTVSceneChangeModel>(command.data);
                    Debug.Log("Load Scene: " + model.sceneName);
                    MainThreadSyncer.Instance.ExecuteOnMainThread(() => {

                        StartCoroutine(Fade(0f, 1f, () => {
                            //Switch texture
                            var bgSprite = GetBackgroundForSceneModel(model);
                            if(bgSprite == null) {
                                Debug.LogError("Could not find texture for " + model.sceneName + " - isDay: " + model.isDay);
                            }
                            else {
                                bgImage.sprite = bgSprite;
                            }
                            StartCoroutine(Fade(1f, 0f, () => {

                                if (currentCommand.HasValue) {
                                    var oldCommand = currentCommand;
                                    currentCommand = null;
                                    _session.Dispatcher.Dispatch<ICommandCompletedListener>(listener => listener.CommandCompleted(oldCommand.Value));
                                }
                                else {
                                    //Make sure we don't lock up
                                    _session.Dispatcher.Dispatch<ICommandCompletedListener>(listener => listener.CommandCompleted(new RPGPTVCommandModel()));
                                }

                            }));
                        }));
                    });
                }
                catch (System.Exception e) {
                    Debug.LogException(e);
                }
            }
        }
    }
}