using System;
using UnityEngine;
using XLua;

namespace GameFramework
{
    /// <summary>
    /// UI脚本
    /// </summary>
    public interface IUIHandler : GObject
    {
        /// <summary>
        /// UI层级
        /// </summary>
        /// <value></value>
        int layer { get; }

        /// <summary>
        /// UI名称
        /// </summary>
        /// <value></value>
        string name { get; }

        /// <summary>
        /// UI游戏对象
        /// </summary>
        /// <value></value>
        GameObject gameObject { get; }

        /// <summary>
        /// UI所在摄像机
        /// </summary>
        /// <value></value>
        Camera UICamera { get; }

        /// <summary>
        /// UI所在画布
        /// </summary>
        /// <value></value>
        Canvas canvas { get; set; }



        /// <summary>
        /// 启动UI
        /// </summary>
        void Start();

        /// <summary>
        /// 显示UI
        /// </summary>
        void OnEnable();

        /// <summary>
        /// 隐藏UI
        /// </summary>
        void OnDisable();

        /// <summary>
        /// 设置UI层级
        /// </summary>
        /// <param name="layer"></param>
        void ToLayer(int layer);

        /// <summary>
        /// 获取UI子对象
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        GameObject GetChild(string name);

        /// <summary>
        /// 获取精灵图片
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Sprite GetSprite(string name);

        /// <summary>
        /// 设置精灵图片
        /// </summary>
        /// <param name="name"></param>
        /// <param name="spriteName"></param>
        void SetSprite(string name, string spriteName);

        /// <summary>
        /// 设置精灵图片
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sprite"></param>
        void SetSprite(string name, Sprite sprite);

        /// <summary>
        /// 获取文本
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string GetText(string name);

        /// <summary>
        /// 设置文本
        /// </summary>
        /// <param name="name"></param>
        /// <param name="info"></param>
        void SetText(string name, string info);

        /// <summary>
        /// 事件通知
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void OnNotify(string eventId, GameObject sender, object args);

        /// <summary>
        /// 克隆对象
        /// </summary>
        /// <param name="name"></param>
        GameObject Instantiate(string name);

        /// <summary>
        /// 克隆对象
        /// </summary>
        /// <param name="name"></param>
        GameObject Instantiate(string name, GameFrameworkAction<GameObject> OnClick);

        /// <summary>
        /// 清理克隆物体
        /// </summary>
        /// <param name="isCache"></param>
        void ClearClone(string name, bool isCache = true);

        /// <summary>
        /// 清理克隆物体
        /// </summary>
        /// <param name="isCache"></param>
        void ClearClone(bool isCache = true);
        
        /// <summary>
        /// 加载子级UI
        /// </summary>
        /// <param name="name"></param>
        /// <param name="path"></param>
        /// <param name="childName"></param>
        /// <returns></returns>
        GameObject GenerateSubUIHandler(string parentName, string path, string uiName);

        /// <summary>
        /// 加载子级UI
        /// </summary>
        /// <param name="parentName"></param>
        /// <param name="path"></param>
        /// <param name="uiName"></param>
        /// <param name="luaScript"></param>
        /// <returns></returns>
        GameObject GenerateSubUIHandler(string parentName, string path, string uiName, LuaTable luaScript);
    }
}