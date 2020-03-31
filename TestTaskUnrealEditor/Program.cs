namespace TestTaskUnrealEditor
{
    class Program
    {
        static void Main(string[] args)
        {
            IAssetCache assetCache = new AssetCacheImpl(10);
            var cache = assetCache.Build("Test01.unity", () => { });
            assetCache.Merge("Test01.unity", cache);
        }
    }
}