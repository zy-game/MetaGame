using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Runtime.Behaviour
{
    public class BehaviourEntity
    {
        public string guid;
        private StateManager stateManager;


        public BehaviourEntity()
        {
            stateManager = new StateManager();
        }
        public void Awake()
        {
        
        }
        public void Update()
        {
            stateManager.Update();
        }

        public void AddState(BehaviourHandle fsmState)
        {
            stateManager.AddState(fsmState);
        }

        public void RemoveState(string stateName)
        {
            stateManager.RemoveState(stateName);
        }

        public BehaviourHandle GetState(string stateName)
        {
            return stateManager.GetState(stateName);
        }

        public void OnTrigger(string stateName,params object[] obj)
        {
            stateManager.OnTrigger(stateName, obj);
        }
    }
}
