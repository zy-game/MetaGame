using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourGame
{
    public abstract class StateHandle
    {

        public string stateName { get;  }
        public int priority { get; set; }
        public bool isPlay { get; set; }

        public StateHandle(string stateName)
        {
            this.stateName = stateName;
        }

        public abstract void DoBeforeEnter(BehaviourEntity ai, params object[] list);

        //离开时执行逻辑
        public abstract void DoAfterLeave();

        //每帧执行逻辑
        public abstract void Act();
    
    }
}
