namespace TestTaskUnrealEditor
{
    class Program
    {
        static void Main(string[] args)
        {
            IAssetCache assetCache = new AssetCacheImpl();
            assetCache.Build("Test01.unity", () => { });
        }
    }
}