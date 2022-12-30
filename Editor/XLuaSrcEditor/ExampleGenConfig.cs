using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using XLua;
using System.Reflection;
using GameFramework;
//配置的详细介绍请看Doc下《XLua的配置.doc》
public static class ExampleGenConfig
{
    //lua中要使用到C#库的配置，比如C#标准库，或者Unity API，第三方库等。
    [LuaCallCSharp]
    public static List<Type> LuaCallCSharp = GetGenerateTypes();

    private static List<Type> GetGenerateTypes()
    {
        List<Type> LuaCallCSharp = new List<Type>() {
        typeof(System.Object),
        typeof(UnityEngine.Object),
        typeof(Vector2),
        typeof(Vector3),
        typeof(Vector4),
        typeof(Quaternion),
        typeof(Color),
        typeof(Ray),
        typeof(Bounds),
        typeof(Ray2D),
        typeof(Time),
        typeof(GameObject),
        typeof(Component),
        typeof(Behaviour),
        typeof(Transform),
        typeof(Resources),
        typeof(TextAsset),
        typeof(Keyframe),
        typeof(AnimationCurve),
        typeof(Animator),
        typeof(Animation),
        typeof(AnimationClip),
        typeof(MonoBehaviour),
        typeof(ParticleSystem),
        typeof(SkinnedMeshRenderer),
        typeof(Renderer),
        typeof(Light),
        typeof(Mathf),
        typeof(System.Collections.Generic.List<int>),
        typeof(Action<string>),
        typeof(UnityEngine.Debug),
        typeof(DG.Tweening.AutoPlay),
        typeof(DG.Tweening.AxisConstraint),
        typeof(DG.Tweening.Ease),
        typeof(DG.Tweening.LogBehaviour),
        typeof(DG.Tweening.LoopType),
        typeof(DG.Tweening.PathMode),
        typeof(DG.Tweening.PathType),
        typeof(DG.Tweening.RotateMode),
        typeof(DG.Tweening.ScrambleMode),
        typeof(DG.Tweening.TweenType),
        typeof(DG.Tweening.UpdateType),
        typeof(DG.Tweening.DOTween),
        typeof(DG.Tweening.DOVirtual),
        typeof(DG.Tweening.EaseFactory),
        typeof(DG.Tweening.Tweener),
        typeof(DG.Tweening.Tween),
        typeof(DG.Tweening.Sequence),
        typeof(DG.Tweening.TweenParams),
        typeof(DG.Tweening.Core.ABSSequentiable),
        typeof(DG.Tweening.Core.TweenerCore<Vector3, Vector3, DG.Tweening.Plugins.Options.VectorOptions>),
        typeof(DG.Tweening.TweenCallback),
        typeof(DG.Tweening.TweenExtensions),
        typeof(DG.Tweening.TweenSettingsExtensions),
        typeof(DG.Tweening.ShortcutExtensions),
        typeof(GameFrameworkAction<bool>),
        typeof(Texture2D),
        typeof(ImageConversion),

        };
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (var i = 0; i < assemblies.Length; i++)
        {
            Type[] types = assemblies[i].GetTypes();
            foreach (var item in types)
            {

                if ((item.FullName.StartsWith("GameFramework") && item.FullName.StartsWith("GameFramework.Editor") == false) || item.FullName.StartsWith("DG.Tweening"))
                {
                    LuaCallCSharp.Add(item);
                }
            }
        }
        return LuaCallCSharp;
    }


    // [LuaCallCSharp]
    // public static List<Type> GameFrameworkRefrence
    // {
    //     get
    //     {
    //         List<Type> list = AppDomain.CurrentDomain.GetAssemblies().ToList().Find(x => x.FullName.StartsWith("GameFramework.Runtime")).GetExportedTypes()
    //              .Where(type => !type.IsGenericTypeDefinition)//去除泛型
    //             .Where(type => !type.IsNested)//去除嵌套类型
    //             .ToList();
    //         return list;
    //     }
    // }

    // [ReflectionUseAttribute]
    // public static List<Type> DefineXluaBlackList
    // {
    //     get
    //     {
    //         List<Type> list = new List<Type>();
    //         AppDomain.CurrentDomain.GetAssemblies().ToList().ForEach(assembly =>
    //         {
    //             list.AddRange(assembly.GetTypes().ToList()
    //                   .Where(type => type.FullName.StartsWith("XLua"))
    //                  .ToList());
    //         });
    //         UnityEngine.Debug.Log(list.Count);
    //         return list;
    //     }
    // }

    //C#静态调用Lua的配置（包括事件的原型），仅可以配delegate，interface
    [CSharpCallLua]
    public static List<Type> CSharpCallLua = new List<Type>() {
                typeof(Action),
                typeof(Func<double, double, double>),
                typeof(Action<string>),
                typeof(Action<double>),
                typeof(Action<bool>),
                typeof(UnityEngine.Events.UnityAction),
                typeof(System.Collections.IEnumerator),
                typeof(GameFrameworkAction<bool>),
                typeof(UnityEngine.Events.UnityAction<int>),
                typeof(UnityEngine.Events.UnityAction<string>),
                typeof(UnityEngine.Events.UnityAction<bool>),
                typeof(UnityEngine.Events.UnityAction<float>),
                typeof(GameFrameworkAction<Texture2D>),
                typeof(GameFrameworkAction<Sprite>),
            };

    //黑名单
    [BlackList]
    public static List<List<string>> BlackList = new List<List<string>>()  {
                new List<string>(){"System.Xml.XmlNodeList", "ItemOf"},
                new List<string>(){"UnityEngine.WWW", "movie"},
    #if UNITY_WEBGL
                new List<string>(){"UnityEngine.WWW", "threadPriority"},
    #endif
                new List<string>(){"UnityEngine.Texture2D", "alphaIsTransparency"},
                new List<string>(){"UnityEngine.Security", "GetChainOfTrustValue"},
                new List<string>(){"UnityEngine.CanvasRenderer", "onRequestRebuild"},
    #if UNITY_ANDROID||UNITY_EDITOR
                new List<string>(){"UnityEngine.Light", "areaSize"},
                new List<string>(){"UnityEngine.Light", "lightmapBakeType"},

                new List<string>(){"UnityEngine.Light", "SetLightDirty"},
                new List<string>(){"UnityEngine.Light", "shadowRadius"},
                new List<string>(){"UnityEngine.Light", "shadowAngle"},
    #endif
                new List<string>(){"UnityEngine.LightAnchor"},
                new List<string>(){"UnityEngine.WWW", "MovieTexture"},
                new List<string>(){"UnityEngine.WWW", "GetMovieTexture"},
                new List<string>(){"UnityEngine.AnimatorOverrideController", "PerformOverrideClipListCleanup"},
    #if !UNITY_WEBPLAYER
                new List<string>(){"UnityEngine.Application", "ExternalEval"},
    #endif
                new List<string>(){"TouchScript"},
                new List<string>(){"UnityEngine.GameObject", "networkView"}, //4.6.2 not support
                new List<string>(){"UnityEngine.Component", "networkView"},  //4.6.2 not support
                new List<string>(){"System.IO.FileInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
                new List<string>(){"System.IO.FileInfo", "SetAccessControl", "System.Security.AccessControl.FileSecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
                new List<string>(){"System.IO.DirectoryInfo", "SetAccessControl", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "CreateSubdirectory", "System.String", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "Create", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"UnityEngine.MonoBehaviour", "runInEditMode"},
            };
}