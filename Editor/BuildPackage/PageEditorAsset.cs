using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Linq;

namespace GameEditor.BuildAsset
{
    public static class EditorGUI
    {
        private static GUIContent mFolder;
        public static GUIContent Folder
        {
            get
            {
                if (mFolder == null)
                    mFolder = EditorGUIUtility.IconContent("d_Folder Icon");
                return mFolder;
            }
        }

        private static GUIContent mFolderEmpty;
        public static GUIContent FolderEmpty
        {
            get
            {
                if (mFolderEmpty == null)
                    mFolderEmpty = EditorGUIUtility.IconContent("d_FolderEmpty Icon");
                return mFolderEmpty;
            }
        }

        private static GUIContent mFolderOpened;
        public static GUIContent FolderOpened
        {
            get
            {
                if (mFolderOpened == null)
                    mFolderOpened = EditorGUIUtility.IconContent("d_FolderOpened Icon");
                return mFolderOpened;
            }
        }

        private static GUIContent mMaterial;
        public static GUIContent Material
        {
            get
            {
                if (mMaterial == null)
                    mMaterial = EditorGUIUtility.IconContent("d_Material Icon");
                return mMaterial;
            }
        }

        private static GUIContent mPrefab;
        public static GUIContent Prefab
        {
            get
            {
                if (mPrefab == null)
                    mPrefab = EditorGUIUtility.IconContent("d_Prefab Icon");
                return mPrefab;
            }
        }

        private static GUIContent mFont;
        public static GUIContent Font
        {
            get
            {
                if (mFont == null)
                    mFont = EditorGUIUtility.IconContent("d_Font Icon");
                return mFont;
            }
        }

        private static GUIContent mShader;
        public static GUIContent Shader
        {
            get
            {
                if (mShader == null)
                    mShader = EditorGUIUtility.IconContent("d_Shader Icon");
                return mShader;
            }
        }

        private static GUIContent mScriptableObject;
        public static GUIContent ScriptableObject
        {
            get
            {
                if (mScriptableObject == null)
                    mScriptableObject = EditorGUIUtility.IconContent("d_ScriptableObject Icon");
                return mScriptableObject;
            }
        }

        private static GUIContent mSceneAsset;
        public static GUIContent SceneAsset
        {
            get
            {
                if (mSceneAsset == null)
                    mSceneAsset = EditorGUIUtility.IconContent("d_SceneAsset Icon");
                return mSceneAsset;
            }
        }

        private static GUIContent mMeshFilter;
        public static GUIContent MeshFilter
        {
            get
            {
                if (mMeshFilter == null)
                    mMeshFilter = EditorGUIUtility.IconContent("d_MeshFilter Icon");
                return mMeshFilter;
            }
        }

        private static GUIContent mRawImage;
        public static GUIContent RawImage
        {
            get
            {
                if (mRawImage == null)
                    mRawImage = EditorGUIUtility.IconContent("d_RawImage Icon");
                return mRawImage;
            }
        }

        private static GUIContent mRenderTexture;
        public static GUIContent RenderTexture
        {
            get
            {
                if (mRenderTexture == null)
                    mRenderTexture = EditorGUIUtility.IconContent("d_RenderTexture Icon");
                return mRenderTexture;
            }
        }

        private static GUIContent mAnimationClip;
        public static GUIContent AnimationClip
        {
            get
            {
                if (mAnimationClip == null)
                    mAnimationClip = EditorGUIUtility.IconContent("d_AnimationClip Icon");
                return mAnimationClip;
            }
        }

        private static GUIContent mAnimatorController;
        public static GUIContent AnimatorController
        {
            get
            {
                if (mAnimatorController == null)
                    mAnimatorController = EditorGUIUtility.IconContent("d_AnimatorController Icon");
                return mAnimatorController;
            }
        }

        private static GUIContent mUnknownIcon;
        public static GUIContent UnknownIcon
        {
            get
            {
                if (mUnknownIcon == null)
                    mUnknownIcon = EditorGUIUtility.IconContent("d_HorizontalLayoutGroup Icon");
                return mUnknownIcon;
            }
        }

        private static GUIContent mShadersubgraph;
        public static GUIContent Shadersubgraph
        {
            get
            {
                if (mShadersubgraph == null)
                {
                    mShadersubgraph = new GUIContent();
                    mShadersubgraph.image = Resources.Load<Texture2D>("Icons/sg_subgraph_icon");

                }
                return mShadersubgraph;
            }
        }

        private static GUIContent mShadergraph;
        public static GUIContent Shadergraph
        {
            get
            {
                if (mShadergraph == null)
                {
                    mShadergraph = new GUIContent();
                    mShadergraph.image = Resources.Load<Texture2D>("Icons/sg_graph_icon");
                }
                return mShadergraph;
            }
        }

        private static GUIContent mINFoldoutAct;
        public static GUIContent INFoldoutAct
        {
            get
            {
                if (mINFoldoutAct == null)
                    mINFoldoutAct = EditorGUIUtility.IconContent("d_IN_foldout");
                return mINFoldoutAct;
            }
        }

        private static GUIContent mINFoldoutOn;
        public static GUIContent INFoldoutOn
        {
            get
            {
                if (mINFoldoutOn == null)
                    mINFoldoutOn = EditorGUIUtility.IconContent("d_IN_foldout_on");
                return mINFoldoutOn;
            }
        }

        private static GUIContent mSettings;
        public static GUIContent Settings
        {
            get
            {
                if (mSettings == null)
                    mSettings = EditorGUIUtility.IconContent("d_Settings Icon");
                return mSettings;
            }
        }

        private static GUIContent mMaskAssetBundle;
        public static GUIContent MaskAssetBundle
        {
            get
            {
                if (mMaskAssetBundle == null)
                    mMaskAssetBundle = EditorGUIUtility.IconContent("TestPassed");
                return mMaskAssetBundle;
            }
        }

        private static GUIContent mShurikenToggleNormal;
        public static GUIContent ShurikenToggleNormal
        {
            get
            {
                if (mShurikenToggleNormal == null)
                    mShurikenToggleNormal = EditorGUIUtility.IconContent("toggle mixed act@2x");
                return mShurikenToggleNormal;
            }
        }

        private static GUIContent mShurikenToggleMixed;
        public static GUIContent ShurikenToggleMixed
        {
            get
            {
                if (mShurikenToggleMixed == null)
                    mShurikenToggleMixed = EditorGUIUtility.IconContent("ShurikenToggleNormalMixed");
                return mShurikenToggleMixed;
            }
        }


        private static GUIContent mBox;
        public static GUIContent Box
        {
            get
            {
                if (mBox == null)
                    mBox = EditorGUIUtility.IconContent("OL box@2x");
                return mBox;
            }
        }

        private static GUIContent mAdd;
        public static GUIContent Add
        {
            get
            {
                if (mAdd == null)
                    mAdd = EditorGUIUtility.IconContent("d_Toolbar Plus");
                return mAdd;
            }
        }

        private static GUIContent mSub;
        public static GUIContent Sub
        {
            get
            {
                if (mSub == null)
                    mSub = EditorGUIUtility.IconContent("d_Toolbar Minus");
                return mSub;
            }
        }

        private static GUIStyle mIconStyle;
        public static GUIStyle IconStyle
        {
            get
            {
                if (mIconStyle == null) mIconStyle = new GUIStyle { normal = { }, alignment = TextAnchor.MiddleCenter };
                return mIconStyle;
            }
        }

        private static GUIStyle mTextStyle;
        public static GUIStyle TextStyle
        {
            get
            {
                if (mTextStyle == null) mTextStyle = new GUIStyle { normal = { textColor = Color.white }, fontSize = 12, padding = { left = 2 }, alignment = TextAnchor.MiddleLeft };
                return mTextStyle;
            }
        }

        private static GUIStyle mMinTextStyle;
        public static GUIStyle MinTextStyle
        {
            get
            {
                if (mMinTextStyle == null) mMinTextStyle = new GUIStyle { normal = { textColor = new Color(0.6f, 0.6f, 0.6f, 1) }, fontSize = 11, padding = { left = 2 }, alignment = TextAnchor.LowerLeft };
                return mMinTextStyle;
            }
        }

        private static GUIStyle mButtonToggle;
        public static GUIStyle ButtonToggle
        {
            get
            {
                if (mButtonToggle == null) mButtonToggle = new GUIStyle { normal = { background = (ShurikenToggleMixed.image as Texture2D) }, alignment = TextAnchor.MiddleCenter };
                return mButtonToggle;
            }
        }

        private static GUIStyle mSelectFile;
        public static GUIStyle SelectFile
        {
            get
            {
                if (mSelectFile == null)
                {
                    mSelectFile = new GUIStyle();
                    mSelectFile.alignment = TextAnchor.MiddleLeft;
                    Texture2D t2d = new Texture2D(1, 1);
                    t2d.SetPixel(0, 0, new Color(44f / 255, 93f / 255, 135f / 255, 1));
                    t2d.Apply();
                    mSelectFile.normal = new GUIStyleState { background = t2d };
                }
                return mSelectFile;
            }
        }
    }

    public class PageEditorAsset : BuildAssetPage
    {
        //打包资源数据
        private class BuildItem
        {
            public bool isNew;//是否是新加的资源
            public BuildAssetItem assetItem;//资源基础配置
        }

        //资源信息
        private class AssetItem
        {
            public bool isSelect;//是否选择了
            public bool isDisable;//是否被禁止操作
            public string ext;
            public Texture icon;
            public int nameLabelSize = -1;
            public AssetFloder parentFloder;//父文件夹
            public bool isClick;
            //---------------
            public bool isFile;
            public string name;
            public Object obj;
            public Object Obj {
                get 
                {
                    if (!obj)
                        obj = AssetDatabase.LoadAssetAtPath<Object>(objPath);
                    return obj;
                }
            }
            public string objPath;
            public string guid;
        }
        //资源文件夹
        private class AssetFloder : AssetItem
        {
            public bool isOpen;
            public bool isChildSelect;//子物体是否选择了
            public int childSelectCount;
            public List<AssetFloder> childFloder;
            public List<AssetItem> assetItems;

            public static void Foreach(Action<AssetItem> func, AssetFloder floder)
            {
                foreach (var file in floder.assetItems)
                {
                    func(file);
                }
                foreach (var f in floder.childFloder)
                {
                    func(f);
                    Foreach(func, f);
                }
            }
        }


        private const int fontSize = 12;
        private Font defFont;
        private AssetFloder rootFloder;//模块根目录
        private List<AssetItem> selectList;
        private BuildAssetData assetData;
        private Vector2 scrollView = Vector2.zero;
        private AssetItem curClickItem;

        private List<BuildAssetItem> defectList;//缺失的资源列表
        private List<BuildAssetItem> errorList;//不在当前模块下的资源列表

        private string[] codesPackage;
        private int selectCodeMask;

        private bool isShowList = false;//列表显示

        public PageEditorAsset(AssetBundleBuildSetting buildSetting, AssetBundleBuildSetting.Page page) : base(buildSetting, page)
        {
            defFont = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
        }

        public override void Enter(object param)
        {
            assetData = param as BuildAssetData;
            rootFloder = new();
            rootFloder.name = assetData.assetRoot.name;
            rootFloder.isOpen = true;
            SetFloderList(AssetDatabase.GetAssetPath(assetData.assetRoot), rootFloder);
            SetBuildItems();
            InitCodePackage();
            if (isShowList)
                UpdateSelectList();
        }

        //设置当前模块所有文件列表
        private void SetFloderList(string path, AssetFloder assetFloder)
        {
            assetFloder.childFloder = new List<AssetFloder>();

            assetFloder.assetItems = new List<AssetItem>();
            string[] files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                if (file.EndsWith(".meta")) continue;
                string pt = file.Replace(@"\", "/");
                AssetItem item = new();
                //item.obj = AssetDatabase.LoadAssetAtPath(pt, typeof(Object));
                item.objPath = pt;
                item.guid = AssetDatabase.AssetPathToGUID(pt);
                item.name = Path.GetFileNameWithoutExtension(file);
                item.ext = Path.GetExtension(file).ToLower();
                item.parentFloder = assetFloder;
                assetFloder.assetItems.Add(item);
            }

            string[] dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
            {
                string pt = dir.Replace(@"\", "/");
                AssetFloder floder = new();
                //floder.obj = AssetDatabase.LoadAssetAtPath(pt, typeof(Object));
                floder.objPath = pt;
                floder.guid = AssetDatabase.AssetPathToGUID(pt);
                floder.name = Path.GetFileName(pt);//floder.obj.name;
                floder.parentFloder = assetFloder;
                assetFloder.childFloder.Add(floder);
                SetFloderList(dir, floder);
            }
        }

        //获取字符串在label上的长度
        private int GetStringLabelWidth(string str)
        {
            defFont.RequestCharactersInTexture(str, fontSize);
            int size = 0;
            foreach (var c in str)
            {
                defFont.GetCharacterInfo(c, out CharacterInfo info, fontSize, FontStyle.Normal);
                size += info.advance;
            }
            return size;
        }

        private AssetItem FindAssetItem(AssetFloder floder, Object obj)
        {            
            if (!obj) return null;
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out string obj_guid, out long obj_loadlId)) return null;
            foreach (var item in floder.assetItems)
            {
                //if (item.obj == obj) return item;
                if (item.guid.Equals(obj_guid)) return item;
            }

            foreach (var f in floder.childFloder)
            {
                //if (f.obj == obj) return f;
                if (f.guid.Equals(obj_guid)) return f;
                var item = FindAssetItem(f, obj);
                if (item != null)
                    return item;
            }               
            return null;
        }

        //设置打包数据
        private void SetBuildItems()
        {
            defectList = new();
            errorList = new();

            foreach (var buildItem in assetData.buildAssets)
            {
                if (buildItem.asset)
                {
                    AssetItem item = FindAssetItem(rootFloder, buildItem.asset);
                    if (item == null)
                    {
                        errorList.Add(buildItem);
                    }
                    else
                    {
                        item.isSelect = true;
                        if (item is AssetFloder) SetFloderChildDisable(item as AssetFloder);
                    }
                }
                else
                {
                    defectList.Add(buildItem);
                }
            }
            ChildSelectParentFloderSate(rootFloder);
        }

        private void InitCodePackage()
        {
            selectCodeMask = 0;
            string luaConfigPath = "Assets/" + AssetDirConfig.configDirProjectPath + AssetDirConfig.luaDir + "/" + AssetDirConfig.luaConfigName;
            if (File.Exists(luaConfigPath))
            {
                var luaPackageConfig = AssetDatabase.LoadAssetAtPath<LuaPackageData>(luaConfigPath);
                codesPackage = new string[luaPackageConfig.items.Count];
                int index = 0;
                foreach (var luaItem in luaPackageConfig.items)
                {
                    string codePackageName = luaItem.name;
                    if (assetData.codesPackageName != null)
                    {
                        for (int i = 0; i < assetData.codesPackageName.Count; i++)
                        {
                            if (codePackageName.Equals(assetData.codesPackageName[i]))
                            {
                                selectCodeMask += 1 << index;
                            }
                        }
                    }
                    codesPackage[index++] = luaItem.name;
                }

            }
            else
            {
                codesPackage = new string[0];
                selectCodeMask = 0;
            }
        }

        private void UpdateSelectList()
        {
            selectList = new List<AssetItem>();
            AssetFloder.Foreach((item) =>
            {
                if (item.isSelect)
                    selectList.Add(item);
            }, rootFloder);
        }

        public override void Exit()
        {

        }

        public override void Run()
        {
            Title();
            scrollView = GUILayout.BeginScrollView(scrollView);
            if (errorList.Count > 0 || defectList.Count > 0) DrawErrorList();
            if (isShowList) DrawSelectList();
            else DrawFloderList(rootFloder, 0);
            GUILayout.Space(16);
            GUILayout.EndScrollView();
            GUILayout.Box("", selfGUIStyle.line, GUILayout.Width(buildSetting.position.width), GUILayout.Height(3));
        }

        //当前页面title
        private void Title()
        {
            GUILayout.Space(3);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("返回", GUILayout.Height(25), GUILayout.Width(75))) Return();
            if (GUILayout.Button("编辑关联包体", GUILayout.Height(25))) AddDependPackage();
            if (GUILayout.Button(isShowList ? "选择资源" : "列表显示", GUILayout.Height(25))) ChangeShowType();
            if (GUILayout.Button("保存", GUILayout.Height(25))) Save();
            GUILayout.EndHorizontal();
            GUILayout.Space(3);
            GUILayout.BeginHorizontal();
            GUILayout.Label("代码包:", EditorGUI.TextStyle, GUILayout.Width(45), GUILayout.Height(25));
            selectCodeMask = EditorGUILayout.MaskField("", selectCodeMask, codesPackage, GUILayout.Height(25));
            GUILayout.EndHorizontal();
            GUILayout.Space(3);
            GUILayout.Box("", selfGUIStyle.line, GUILayout.Width(buildSetting.position.width), GUILayout.Height(3));
            GUILayout.Space(3);
        }

        //返回到主界面
        private void Return()
        {
            buildSetting.ChangePage(AssetBundleBuildSetting.Page.Main);
        }

        //添加依赖包体
        private void AddDependPackage()
        {
            buildSetting.ChangePage(AssetBundleBuildSetting.Page.EditorModule, assetData);
        }

        private void ChangeShowType()
        {
            isShowList = !isShowList;
            if (isShowList)
                UpdateSelectList();
        }

        private void Save(bool tips = true)
        {
            assetData.buildAssets.Clear();

            AssetFloder.Foreach((assetItem) =>
            {
                if (assetItem.isSelect)
                {
                    BuildAssetItem buildItem = new();
                    buildItem.asset = assetItem.Obj;
                    buildItem.lastSavePath = AssetDatabase.GetAssetPath(assetItem.Obj);
                    assetData.buildAssets.Add(buildItem);
                }
            }, rootFloder);

            assetData.codesPackageName = new List<string>();
            for (int i = 0; i < codesPackage.Length; i++)
            {
                if (selectCodeMask == 0)
                    break;
                if ((selectCodeMask & (1 << i)) > 0 || selectCodeMask == -1)
                {
                    string codePackageName = codesPackage[i];
                    assetData.codesPackageName.Add(codePackageName);
                }
            }
            AssetDatabase.SaveAssetIfDirty(assetData);
            EditorUtility.SetDirty(assetData);
            buildSetting.Save();
            if (tips)
                EditorUtility.DisplayDialog("提示", "保存完成!", "确定");
            AssetDatabase.Refresh();
        }

        #region 文件列表绘制
        private void SetFloderChildDisable(AssetFloder floder)
        {
            foreach (var v in floder.assetItems)
            {
                v.isDisable = floder.isSelect;
                if (!v.isDisable) v.isDisable = floder.isDisable;
                if (floder.isSelect || floder.isDisable)
                    v.isSelect = false;
            }

            foreach (var v in floder.childFloder)
            {
                v.isDisable = floder.isSelect;
                if (!v.isDisable) v.isDisable = floder.isDisable;
                if (floder.isSelect || floder.isDisable)
                    v.isSelect = false;
                SetFloderChildDisable(v);
            }
        }

        //子节点是否有选择的
        private int ChildSelectSelectCount(AssetFloder floder)
        {
            int selectCount = 0;
            foreach (var v in floder.assetItems)
            {
                if (v.isSelect) selectCount++;
            }
            foreach (var v in floder.childFloder)
            {
                if (v.isSelect) selectCount++;
            }

            foreach (var v in floder.childFloder)
            {
                int count = ChildSelectSelectCount(v);
                v.childSelectCount = count;
                selectCount += count;
            }

            return selectCount;
        }

        private void ChildSelectParentFloderSate(AssetFloder floder)
        {
            if (floder.isSelect) floder.isChildSelect = false;
            else
            {
                int selectCount = ChildSelectSelectCount(floder);
                floder.isChildSelect = selectCount > 0;
                floder.childSelectCount = selectCount;
            }
            foreach (var v in floder.childFloder)
            {
                ChildSelectParentFloderSate(v);
            }
        }

        private void DrawFloderList(AssetFloder floder, int space)
        {
            if (floder.isClick) GUILayout.BeginHorizontal(EditorGUI.SelectFile);
            else GUILayout.BeginHorizontal();
            GUILayout.Space(space);

            if (floder.assetItems.Count != 0 || floder.childFloder.Count != 0)//是否是空文件夹
            {
                //---------------选择Toggle
                if (floder.isDisable)//是否被禁用
                {
                    GUILayout.Space(-1);
                    GUILayout.Box(EditorGUI.MaskAssetBundle, EditorGUI.IconStyle, GUILayout.Width(16), GUILayout.Height(16));
                }
                else
                {
                    if (floder.isChildSelect)
                    {
                        if (GUILayout.Button("", EditorGUI.ButtonToggle, GUILayout.Width(15), GUILayout.Height(15)))
                        {
                            floder.isChildSelect = false;
                            floder.isSelect = true;
                            SetFloderChildDisable(floder);
                            ChildSelectParentFloderSate(rootFloder);
                        }
                    }
                    else
                    {
                        bool isOn = GUILayout.Toggle(floder.isSelect, EditorGUI.ShurikenToggleNormal, GUILayout.Width(12), GUILayout.Height(12));
                        if (floder.isSelect != isOn)
                        {
                            floder.isSelect = isOn;
                            SetFloderChildDisable(floder);
                            ChildSelectParentFloderSate(rootFloder);
                        }
                    }
                }

                //--------文件夹打开折叠按钮
                if (GUILayout.Button(floder.isOpen ? EditorGUI.INFoldoutOn.image : EditorGUI.INFoldoutAct.image, EditorGUI.IconStyle, GUILayout.Width(16), GUILayout.Height(16))) floder.isOpen = !floder.isOpen;
                //文件夹图标
                GUILayout.Box(floder.isOpen ? EditorGUI.FolderOpened.image : EditorGUI.Folder.image, EditorGUI.IconStyle, GUILayout.Width(16), GUILayout.Height(16));
            }
            else
            {
                GUILayout.Space(31);
                GUILayout.Box(EditorGUI.FolderEmpty.image, EditorGUI.IconStyle, GUILayout.Width(16), GUILayout.Height(16));
            }

            if (floder.nameLabelSize == -1) floder.nameLabelSize = GetStringLabelWidth(floder.name);

            //文件夹名字按钮
            if (GUILayout.Button(floder.name, EditorGUI.TextStyle, GUILayout.Width(floder.nameLabelSize + 10))) ClickItem(floder);

            if (floder.childSelectCount > 0)
            {
                if (GUILayout.Button("(selected " + floder.childSelectCount.ToString() + ")", EditorGUI.MinTextStyle, GUILayout.Height(16))) ClickItem(floder);
            }
            else
            {
                if (GUILayout.Button("", EditorGUI.MinTextStyle, GUILayout.Height(16))) ClickItem(floder);
            }

            GUILayout.EndHorizontal();

            space += 16;

            foreach (var f in floder.childFloder)
            {
                if (floder.isOpen)
                    DrawFloderList(f, space);
            }

            if (floder.assetItems != null && floder.assetItems.Count > 0 && floder.isOpen)
                DrawItemList(floder.assetItems, space);

        }

        private void DrawItemList(List<AssetItem> items, int space)
        {
            foreach (var item in items)
            {
                if (item.isClick) GUILayout.BeginHorizontal(EditorGUI.SelectFile);
                else GUILayout.BeginHorizontal();
                GUILayout.Space(space);
                DrawItem(item);
                GUILayout.EndHorizontal();
            }
        }

        private void DrawItem(AssetItem item)
        {
            if (!item.icon)
                item.icon = GetAssetIcon(item);

            if (!item.isDisable)
            {
                bool isOn = GUILayout.Toggle(item.isSelect, EditorGUI.ShurikenToggleNormal, GUILayout.Width(12), GUILayout.Height(12));
                if (item.isSelect != isOn)
                {
                    item.isSelect = isOn;
                    ChildSelectParentFloderSate(rootFloder);
                }
            }
            else
            {
                GUILayout.Space(-1);
                GUILayout.Box(EditorGUI.MaskAssetBundle.image, EditorGUI.IconStyle, GUILayout.Width(16), GUILayout.Height(16));
            }

            GUILayout.Box(item.icon, EditorGUI.IconStyle, GUILayout.Width(16), GUILayout.Height(16));
            if (GUILayout.Button(item.name, EditorGUI.TextStyle, GUILayout.Height(16))) ClickItem(item);
        }

        private void ClickItem(AssetItem item)
        {
            if (item == curClickItem)
            {
                item.isClick = false;
                curClickItem = null;
            }
            else
            {
                if (curClickItem != null) curClickItem.isClick = false;
                item.isClick = true;
                curClickItem = item;
                Selection.activeObject = item.Obj;
            }
        }

        private Texture GetAssetIcon(AssetItem item)
        {
            if (item.Obj)
            {
                Texture defTex = EditorGUIUtility.GetIconForObject(item.Obj);
                if (defTex) return defTex;
                Type type = item.Obj.GetType();
                if (type == typeof(Texture2D)) return AssetPreview.GetMiniThumbnail(item.Obj);
            }

            switch (item.ext)
            {
                case ".prefab":
                    return EditorGUI.Prefab.image;
                case ".unity":
                    return EditorGUI.SceneAsset.image;
                case ".controller":
                    return EditorGUI.AnimatorController.image;
                case ".anim":
                    return EditorGUI.AnimationClip.image;
                case ".ttf":
                    return EditorGUI.Font.image;
                case ".shader":
                    return EditorGUI.Shader.image;
                case ".fbx":
                case ".3ds":
                case ".obj":
                    return EditorGUI.MeshFilter.image;
                case ".rendertexture":
                    return EditorGUI.RenderTexture.image;
                case ".mat":
                    return EditorGUI.Material.image;
                case ".asset":
                    return EditorGUI.ScriptableObject.image;
                case ".shadersubgraph":
                    return EditorGUI.Shadersubgraph.image;
                case ".shadergraph":
                    return EditorGUI.Shadergraph.image;
            }
            return AssetPreview.GetMiniTypeThumbnail(item.Obj.GetType());
        }
        #endregion

        private void DrawSelectList()
        {
            int index = 1;
            foreach (var item in selectList)
            {
                GUIStyle style = selfGUIStyle.item;
                if (!item.isSelect) style = selfGUIStyle.delItem;
                GUILayout.BeginHorizontal(style);
                item.isSelect = EditorGUILayout.Toggle(item.isSelect, GUILayout.Width(15));
                GUILayout.Label(index + ". " + item.name, GUILayout.Width(170));
                EditorGUILayout.ObjectField(item.Obj, typeof(Object), true, GUILayout.Width(280));
                GUILayout.EndHorizontal();
                GUILayout.Space(2);
                index++;
            }
        }

       // 错误的资源列表
        private void DrawErrorList()
        {
            int index = 1;
            if (defectList.Count > 0)
            {
                GUILayout.Label("缺失的资源:");
                foreach (var v in defectList)
                {
                    GUILayout.Space(3);
                    GUILayout.BeginHorizontal(selfGUIStyle.item, GUILayout.Height(20));
                    GUILayout.Label(index + "." + v.lastSavePath);
                    GUILayout.EndHorizontal();
                    index++;
                }
            }
            if (errorList.Count > 0)
            {
                GUILayout.Space(5);
                GUILayout.Label("不在当前模块下的资源:");
                index = 1;
                foreach (var v in errorList)
                {
                    GUILayout.Space(3);
                    GUILayout.BeginVertical(selfGUIStyle.item, GUILayout.Height(40));
                    GUILayout.Label(index + "." + v.lastSavePath);
                    EditorGUILayout.ObjectField(v.asset, typeof(Object), true);
                    GUILayout.Space(3);
                    GUILayout.EndVertical();
                    index++;
                }
            }

            if (GUILayout.Button("移除错误的资源"))
            {
                defectList.Clear();
                errorList.Clear();
                Save(false);
            }

        }                

    }
}
