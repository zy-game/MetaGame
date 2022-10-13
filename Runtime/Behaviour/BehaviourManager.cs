using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourGame
{
    public class BehaviourManager 
    {
        private static BehaviourManager instance;

        public static BehaviourManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new BehaviourManager();
                }
                return instance;
            }
        }

        List<BehaviourEntity> behaviourDic = new List<BehaviourEntity>();


        public void AddBehaviour(BehaviourEntity entity)
        {
            if (entity == null)
            {
                return;
            }
            behaviourDic.Add(entity);
        }

        public void Trigger(string guid,string state,params object[] obj)
        {
            BehaviourEntity entity = behaviourDic.Find(temp => temp.guid == guid);
            if (entity == null)
            {
                return;
            }
            entity.OnTrigger(state, obj);
        }


        public void Update()
        {

            for (int i = behaviourDic.Count - 1; i >= 0; i--)
            {
                behaviourDic[i].Update();
            }
        }
    }
}
