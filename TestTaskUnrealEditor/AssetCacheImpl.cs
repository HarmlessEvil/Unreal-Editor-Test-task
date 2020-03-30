using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TestTaskUnrealEditor
{
    [System.Serializable]
    class NodeDescription
    {
        [YamlMember(Alias = "serializedVersion", ApplyNamingConventions = false)]
        public int SerializedVersion { get; set; }
    }

    [System.Serializable]
    class GameObjectDescription : NodeDescription
    {
        [YamlMember(Alias = "m_Name", ApplyNamingConventions = false)]
        public string Name { get; set; }
    }

    class AssetCacheImpl : IAssetCache
    {
        public object Build(string path, Action interruptChecker)
        {
            List<NodeDescription> cache = new List<NodeDescription>();
            using (StreamReader input = new StreamReader(path))
            {
                var deserializer = new DeserializerBuilder()
                    .IgnoreUnmatchedProperties()
                    .WithNodeTypeResolver(new UnityNodeTypeResolver())
                    .Build();
                var parser = new Parser(input);

                parser.Consume<StreamStart>();
                while (parser.Accept<DocumentStart>(out _))
                {
                    cache.AddRange(deserializer.Deserialize<Dictionary<string, NodeDescription>>(parser).Values);
                }
            }

            return cache;
        }

        public void Merge(string path, object result)
        {
            throw new NotImplementedException();
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