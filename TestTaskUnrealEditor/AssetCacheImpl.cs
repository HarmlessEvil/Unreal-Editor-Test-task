using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace TestTaskUnrealEditor
{
    [Serializable]
    internal class NodeDescription
    {
        [YamlMember(Alias = "serializedVersion", ApplyNamingConventions = false)]
        public int SerializedVersion { get; set; }
    }

    [Serializable]
    internal class GameObjectDescription : NodeDescription
    {
        [YamlMember(Alias = "m_Name", ApplyNamingConventions = false)]
        public string Name { get; set; }
    }

    public class AssetCacheImpl : IAssetCache
    {
        private readonly int _checkFrequency = 100;

        public AssetCacheImpl()
        {
        }

        /// <summary>
        /// Using this constructor, user can define,
        /// how will often occur the check for interruption in <c>Build()</c> operation.
        /// </summary>
        /// <param name="checkFrequency">The frequency: <c>checkFrequency = n</c> means,
        /// that in <c>Build()</c> operation after every <c>n</c> deserialized nodes
        /// a check call to <c>interruptChecker()</c> will occur</param>
        public AssetCacheImpl(int checkFrequency)
        {
            _checkFrequency = checkFrequency;
        }

        /// <summary>
        /// In my implementation the cache object is a List of nodes meta-information.
        /// </summary>
        /// <param name="path">The path to an existing file on a disk</param>
        /// <param name="interruptChecker">The function, that checks, if cache building should be interrupted.
        ///     It throws <c>OperationCanceledException</c> in case of interrupt is necessary</param>
        /// <returns>Cache data for the given file,
        /// that is actually a <c>List&lt;NodeDescription&gt;</c></returns>
        public object Build(string path, Action interruptChecker)
        {
            var cache = new List<NodeDescription>();

            using var input = new StreamReader(path);
            var deserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithNodeTypeResolver(new UnityNodeTypeResolver())
                .Build();
            var parser = new Parser(input);

            parser.Consume<StreamStart>();
            var counter = 0;
            while (parser.Accept<DocumentStart>(out _))
            {
                ++counter;
                cache.AddRange(deserializer.Deserialize<Dictionary<string, NodeDescription>>(parser).Values);

                if (counter != _checkFrequency) continue;
                try
                {
                    interruptChecker();
                }
                catch (OperationCanceledException)
                {
                    return cache;
                }

                counter = 0;
            }

            return cache;
        }

        private DateTime deserializedAt_;
        private List<NodeDescription> cache_;

        /// <summary>
        /// <para>After successful cache build, the function serializes cache in a binary format to disk.
        /// It uses <c>path + ".cache"</c> as path to the cache file.</para>
        /// <para>Also, this method saves deserialized cache to the private <c>cache_</c> property for future use.
        /// Saves timestamp, when the cache has been deserialized to determine if the current cache has expired.</para>
        /// </summary>
        /// <param name="path">The path to an existing file on a disk</param>
        /// <param name="result">Cache data from function <c>Build()</c></param>
        public void Merge(string path, object result)
        {
            if (!(result is List<NodeDescription> cache)) return;

            using Stream stream = File.Open(path + ".cache", FileMode.Create);

            var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            formatter.Serialize(stream, cache);

            cache_ = cache;
            deserializedAt_ = DateTime.Now;
        }

        public int GetLocalAnchorUsages(ulong anchor)
        {
            throw new NotImplementedException();
        }

        public int GetGuidUsages(string guid)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ulong> GetComponentsFor(ulong gameObjectAnchor)
        {
            throw new NotImplementedException();
        }
    }
}