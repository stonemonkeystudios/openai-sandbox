using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using HQDotNet.Unity;
namespace RPGPTV {
    public class RPGPTVMusicView : HQMonoView, ICommandReceivedListener {
        public AudioClip[] musicClips;
        public AudioSource audioSource1;
        public AudioSource audioSource2;
        public float crossFadeTime = 1f;

        private RPGPTVCommandModel? currentCommand = null;

        bool useSource1 = false;

        private AudioClip GetNamedClip(string name) {
            foreach (var music in musicClips) {
                if (name.Contains(music.name))
                    return music;
            }
            return null;
        }

        private IEnumerator CrossFade(AudioClip newClip, System.Action callback) {
            AudioSource fadeIn = useSource1 ? audioSource1 : audioSource2;
            AudioSource fadeOut = useSource1 ? audioSource2 : audioSource1;
            fadeIn.Stop();
            fadeIn.clip = newClip;
            fadeIn.Play();
            fadeIn.volume = 0f;
            float startTime = Time.time;
            while(Time.time < startTime + crossFadeTime) {
                float t = (Time.time - startTime) / crossFadeTime;
                fadeIn.volume = t;
                fadeOut.volume = 1f - t;
                yield return null;
            }
            fadeOut.Stop();
            useSource1 = !useSource1;
            callback?.Invoke();
        }

        void ICommandReceivedListener.CommandReceived(RPGPTVCommandModel command) {
            Assert.IsNotNull(audioSource1);
            if (command.command == RPGPTVPromptController.COMMANDS.PLAY_MUSIC.ToString()) {
                MainThreadSyncer.Instance.ExecuteOnMainThread(() => {
                    currentCommand = command;
                    var clip = GetNamedClip(command.data);
                    if (clip == null) {
                        Debug.LogError("Could not find clip " + command.data);
                        DispatchCompleteCallback();
                        return;
                    }
                    StartCoroutine(CrossFade(clip, DispatchCompleteCallback));
                });
            }
        }

        void DispatchCompleteCallback() {
            var temp = currentCommand;
            currentCommand = null;
            _session.Dispatcher.Dispatch<ICommandCompletedListener>(listener => listener.CommandCompleted(temp.Value));
        }
    }  
}