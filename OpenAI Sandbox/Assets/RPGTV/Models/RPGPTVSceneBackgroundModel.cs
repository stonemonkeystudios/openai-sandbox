using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPGPTV {
    [CreateAssetMenu(fileName = nameof(RPGPTVSceneBackgroundModel), menuName = "SMS/RPGPTV/" + nameof(RPGPTVSceneBackgroundModel), order = 0)]
    [System.Serializable]
    public class RPGPTVSceneBackgroundModel : ScriptableObject {
        public string sceneName;
        public Sprite nightImage;
        public Sprite dayImage;
    }
}
