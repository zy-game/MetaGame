using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Runtime.Behaviour
{
    public abstract class BehaviourHandle
    {

        public string stateName { get;  }
        public int priority { get; set; }
        public bool isPlay { get; set; }

        public BehaviourHandle(string stateName)
        {
            this.stateName = stateName;
        }

        public abstract void OnEntry(BehaviourEntity ai, params object[] list);

        //离开时执行逻辑
        public abstract void OnLeave();

        //每帧执行逻辑
        public abstract void OnExecute();
    }
}
