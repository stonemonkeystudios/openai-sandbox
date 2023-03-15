using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HQDotNet;

namespace RPGPTV {
    public interface ICommandCompletedListener : IDispatchListener {
        void CommandCompleted(RPGPTVCommandModel command);
    }
}