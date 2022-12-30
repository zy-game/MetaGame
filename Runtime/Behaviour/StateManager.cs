using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Runtime.Behaviour
{
    public class StateManager
    {
        private BehaviourHandle curState;

        private BehaviourEntity entity;

        private List<BehaviourHandle> fsmStates = new List<BehaviourHandle>();

        public StateManager()
        {

        }

        public void Update()
        {
            if (curState == null)
            {
                return;
            }
            curState.OnExecute();
        }

        //添加状态
        public void AddState(BehaviourHandle fsmState)
        {
            if (fsmStates.Contains(fsmState))
            {
                return;
            }
            fsmStates.Add(fsmState);
        }

        public void RemoveState(string stateName)
        {
            BehaviourHandle state = GetState(stateName);
            if (state == null)
            {
                return;
            }
            fsmStates.Remove(state);
        }

        public BehaviourHandle GetState(string stateName)
        {
            return fsmStates.Find(x => x.stateName == stateName);
        }

        public void OnTrigger(string stateName, params object[] obj)
        {
            if (string.IsNullOrEmpty(stateName))
            {
                Debug.LogError("为空");
                return;
            }

            BehaviourHandle state = GetState(stateName);
            if (state == null)
            {
                Debug.LogError("为空");
                return;
            }
            curState?.OnLeave();
            state.OnEntry(entity, obj);
            curState = state;
        }

    }
}
