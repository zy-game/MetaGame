using System.Collections;
using System.Collections.Generic;
using GameFramework.Runtime;
using GameFramework.Runtime.Assets;
using GameFramework.Runtime.Game;
using UnityEngine;
using GameFramework;

public class EditorStart : MonoBehaviour
{
    public GameObject loading;
    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return AppConst.LoadLocalConfig();
        DontDestroyOnLoad(gameObject);
        GameWorld.CreateWorld<UpdateResourceWorld>(AppConst.config.mainModuleName, new Vector2(1920, 1080));
    }

    class UpdateResourceWorld : GameWorld
    {
        private CommonLoading common;
        public override void Awake()
        {
            base.Awake();
            common = (CommonLoading)this.UIManager.OnLoading();
            
            GlobalEvent<AssetUpdate>.AddEvent(EventName.NotifyUpdateAsset, NotifyUpdate);
            GlobalEvent<string[]>.AddEvent(EventName.AssetUpdateFinished, AssetUpdateFinished);
            CorManager.Instance.StartCoroutine(OpenMainWorld());
        }
        void NotifyUpdate(AssetUpdate args)
        {

            GlobalEvent<AssetUpdate>.Remove(EventName.NotifyUpdateAsset, NotifyUpdate);
            string needUpdateSize = (args.totalSize / 1024f / 1024f).ToString("N2");
            this.UIManager.OnMsgBox("Need Update Resource <color=#00FF00>" + needUpdateSize + "M</color>, To Update?", () =>
            {
                args.StartUpdate();
                args.onDownloadingAsset = (a, b) =>
                {
                    common.text = "Waiting Update Resource..." + (b / (float)a * 100f).ToString("N2") + "%";
                };
            }, () =>
            {
                ReactiveWorld(string.Empty);
                GlobalEvent<string>.Notify(EventName.AssetUpdateFail, "Update Resource Failur");
            });
        }
        void AssetUpdateFinished(string[] args)
        {
            GlobalEvent<string[]>.Remove(EventName.AssetUpdateFinished, AssetUpdateFinished);
        }
        private IEnumerator OpenMainWorld()
        {
            yield return new VersionManager().Init();
            ResourcesManager.Instance.Init();
            LuaManager.Instance.Init(OpenEditorWorld);
        }

        private void OpenEditorWorld()
        {
            RemoveWorld(this.name);
            IWorld gameWorld = GameWorld.CreateWorld("EditorWorld", new Vector2(1920, 1080));
            if (gameWorld == null)
            {
                return;
            }
            gameWorld.LoadLuaMap("GameEditor/EditorWorld");
        }
    }
}
