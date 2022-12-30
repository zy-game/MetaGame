using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Runtime.Assets;
using GameFramework.Runtime.Game;
using UnityEngine.Networking;
namespace GameFramework.Runtime
{
    public class GameManager : MonoBehaviour
    {
        public Vector2 SceneSize;
        IEnumerator Start()
        {
            DontDestroyOnLoad(gameObject);
            yield return AppConst.LoadLocalConfig();
            ResourcesManager.Instance.Init();
            LuaManager.Instance.Init();
            GameWorld.CreateWorld<StarboottWorld>(AppConst.config.mainModuleName, SceneSize);
            yield return new VersionManager().Init();
        }
        public class StarboottWorld : GameWorld
        {
            public override void Awake()
            {

                GlobalEvent.Notify(EventName.EnterGame);
                base.Awake();
            }
        }
    }
}