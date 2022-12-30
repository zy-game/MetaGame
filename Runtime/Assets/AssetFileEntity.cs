using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Runtime.Assets
{
    public class AssetFileEntity
    {
        public class FileItem
        {
            public string name;
            public string md5;
            public int size;

            public void Copy(FileItem item)
            {
                name = item.name;
                md5 = item.md5;
                size = item.size;
            }
        }

        public int version;
        public string moduleName;
        public List<FileItem> files;

        public static AssetFileEntity Get(string json)
        {
            return JsonObject.Deserialize<AssetFileEntity>(json);
        }

        public static AssetFileEntity Get(AssetFileEntity source)
        {
            var entity = new AssetFileEntity();
            entity.files = new List<FileItem>();
            entity.version = 0;
            entity.moduleName = source.moduleName;
            return entity;
        }

        public static AssetFileEntity Copy(AssetFileEntity source)
        {
            var entity = new AssetFileEntity();
            entity.files = new List<FileItem>();
            entity.version = source.version;
            entity.moduleName = source.moduleName;

            foreach (var v in source.files)
            {
                var newItem = new FileItem();
                newItem.Copy(v);
                entity.files.Add(newItem);
            }
            return entity;
        }

        public void UpdateItem(FileItem item)
        {
            var curItem = FindItem(item.name);
            if (curItem == null)
                files.Add(item);
            else
                curItem.Copy(item);
        }

        public bool ContainsMd5(FileItem item)
        {
            if (files == null) return false;
            foreach (var v in files)
            {
                if (item.md5.Equals(v.md5))
                    return true;
            }
            return false;
        }

        public bool ContainsName(FileItem item)
        {
            if (files == null) return false;
            foreach (var v in files)
            {
                if (item.name.Equals(v.name))
                    return true;
            }
            return false;
        }

        public FileItem FindItem(string name)
        {
            if (files == null) return null;
            foreach (var v in files)
            {
                if (name.Equals(v.name))
                    return v;
            }
            return null;
        }

    }
}
