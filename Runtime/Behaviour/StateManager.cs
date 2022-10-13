using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourGame
{
    public class StateManager 
    {
        private StateHandle curState;

        private BehaviourEntity entity;

        private List<StateHandle> fsmStates = new List<StateHandle>();

        public StateManager()
        { 
    
        }

        public void Update()
        {
            if (curState == null)
            {
                return;
            }
            curState.Act();
        }

        //添加状态
        public void AddState(StateHandle fsmState)
        {
            if (fsmStates.Contains(fsmState))
            {
                return;
            }
            fsmStates.Add(fsmState);
        }

        public void RemoveState(string stateName)
        {
            StateHandle state = GetState(stateName);
            if (state == null)
            {
                return;
            }
            fsmStates.Remove(state);
        }

        public StateHandle GetState(string stateName)
        {
            return fsmStates.Find(x => x.stateName == stateName);
        }

        public void OnTrigger(string stateName,params object[] obj)
        {
            if (string.IsNullOrEmpty(stateName))
            {
                Debug.LogError("为空");
                return;
            }

            StateHandle state = GetState(stateName);
            if (state == null)
            {
                Debug.LogError("为空");
                return;
            }
            curState?.DoAfterLeave();
            state.DoBeforeEnter(entity, obj);
            curState = state;
        }

    }
}
