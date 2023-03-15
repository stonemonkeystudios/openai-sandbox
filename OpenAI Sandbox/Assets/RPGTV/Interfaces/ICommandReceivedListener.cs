using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HQDotNet;

namespace RPGPTV {
    public interface ICommandReceivedListener : IDispatchListener {
        void CommandReceived(RPGPTVCommandModel command);
    }
}