using System.Collections;
using System.Collections.Generic;

namespace GameFramework.Runtime.Assets
{
    public class ResourceLoader : AssetLoad
    {
        public override List<AssetHandle> GetAssetHandleListByPackageName(string packageName)
        {
            return new List<AssetHandle>();
        }

        public override AssetHandle Load(string path)
        {
            throw new System.NotImplementedException();
        }

        public override AssetLoadAsync LoadAsync(string path)
        {
            throw new System.NotImplementedException();
        }

        public override void SetRefCount(AssetHandle assetHandle, bool isAdd)
        {
            throw new System.NotImplementedException();
        }
    }
}
