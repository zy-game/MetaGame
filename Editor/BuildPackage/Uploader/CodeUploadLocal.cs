using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace GameEditor.BuildAsset
{
    public class CodeUploderLocal : AssetUploderLocal
    {
        public CodeUploderLocal(UploaderParam param) : base(param)
        {

        }

        public override void UploadAsset(Action<int, bool> uploadFunc)
        {
            List<FileItem> tempItems = new List<FileItem>();
            foreach (var item in uploadAssets)
            {
                if (item.isUpload) tempItems.Add(item);
            }
            uploadAssets = tempItems;

            tempItems = new List<FileItem>();
            foreach (var del in delItems)
            {
                if (del.isUpload) tempItems.Add(del);
            }
            delItems = tempItems;

            base.UploadAsset(uploadFunc);
        }
    }
}