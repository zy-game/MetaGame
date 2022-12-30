using GameFramework.Runtime.Assets;
using UnityEngine;
using UnityEngine.UI;
using XLua;

namespace GameFramework
{
    /// <summary>
    /// UI管理器
    /// </summary>
    public interface IUIManager : GObject
    {
        /// <summary>
        /// UI摄像机
        /// </summary>
        /// <value></value>
        Camera UICamera { get; }

        /// <summary>
        /// 当前最上层的UI
        /// </summary>
        /// <value></value>
        IUIHandler current { get; }

        /// <summary>
        /// 打开UI
        /// </summary>
        /// <param name="name"></param>
        IUIHandler OpenUI(string name);

        IUIHandler OpenUI(string path, string name, LuaTable table);

        IUIHandler OpenUI(string name, LuaTable table);

        /// <summary>
        /// 关闭UI
        /// </summary>
        /// <param name="name"></param>
        void CloseUI(string name, bool isCache = false);

        /// <summary>
        /// 获取指定的UI
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IUIHandler GetUIHandler(string name);

        /// <summary>
        /// 设置管理器显示状态
        /// </summary>
        /// <param name="active"></param>
        void SetActive(bool active);

        /// <summary>
        /// 将UI设置到指定的层级
        /// </summary>
        /// <param name="handler"></param>
        Canvas ToLayer(IUIHandler handler, int layer);

        /// <summary>
        /// 将物体设置到指定的层级
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="layer"></param>
        Canvas ToLayer(GameObject handle, int layer);

        /// <summary>
        /// 显示进度界面
        /// </summary>
        /// <returns></returns>
        ILoading OnLoading();

        /// <summary>
        /// 显示等待
        /// </summary>
        /// <returns></returns>
        IAwaiting OnAwaitLoading();

        void CloseWait();

        /// <summary>
        /// 显示一个提示窗口
        /// </summary>
        /// <param name="text"></param>
        /// <param name="ok"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
        IMessageBox OnMsgBox(string text, GameFrameworkAction ok = null, GameFrameworkAction cancel = null);

        /// <summary>
        /// 清理所有弹窗消息
        /// </summary>
        void ClearMessageBox();

        /// <summary>
        /// 清理所有加载界面
        /// </summary>
        void ClearLoading();

        /// <summary>
        /// 清理所有UI
        /// </summary>
        void Clear();
    }

    public interface IMessageBox : GObject
    {
        void Show(Transform parent, string tilet, string message, GameFrameworkAction ok, GameFrameworkAction cancel);
    }

    public interface ILoading : GObject
    {
        string text { get; set; }
        string version { get; set; }
        GameObject gameObject { get; }

        void SetLoadingBackground(string textureName);
    }

    public interface IAwaiting : GObject
    {
        GameObject gameObject { get; }
    }
}