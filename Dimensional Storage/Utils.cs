using System;
using System.IO;
using System.Reflection;
using FullSerializer;
using UnityEngine;

namespace Com.JiceeDev.DimensionalStorage
{
    public static class Utils
    {
        private readonly static string CurrentFolderAssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private readonly static string AssetsPath = Path.Combine(CurrentFolderAssemblyPath, "assets");

        public static Texture2D GetTexture(string name, int width = 32, int height = 32)
        {
            // var texture = new Texture2D(width, height);
            // texture.LoadImage(File.ReadAllBytes(Path.Combine(AssetsPath, name)));
            // Debug.Log("DS - Utils.GetTexture: " + Path.Combine(AssetsPath, name));
            var texture = new Texture2D(2, 2)
            {
                filterMode = FilterMode.Point
            };
            texture.LoadImage(File.ReadAllBytes(GetImagePath(name)));
            texture.name = name;
            return texture;
        }
        
        public static Sprite GetSprite(string name)
        {
            var texture = GetTexture(name);
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            sprite.name = name;
            return sprite;
        } 
        
        public static string GetImagePath(string name)
        {
            return Path.Combine(AssetsPath, name);
        }

    }
    
    public static class StringSerializationAPI {
        private static readonly fsSerializer _serializer = new fsSerializer();

        public static string Serialize(Type type, object value) {
            // serialize the data
            fsData data;
            _serializer.TrySerialize(type, value, out data).AssertSuccessWithoutWarnings();

            // emit the data via JSON
            return fsJsonPrinter.CompressedJson(data);
        }

        public static object Deserialize(Type type, string serializedState) {
            // step 1: parse the JSON data
            fsData data = fsJsonParser.Parse(serializedState);

            // step 2: deserialize the data
            object deserialized = null;
            _serializer.TryDeserialize(data, type, ref deserialized).AssertSuccessWithoutWarnings();

            return deserialized;
        }
    }
}