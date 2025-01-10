using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace GlobalTypes
{
    public static class Asset
    {
        public static ContentManager Content { get; set; }

        public static string FontsFolderName { get; private set; } = "Fonts";
        public static string LevelsFolderName { get; private set; } = "Levels";
        public static string SoundsFolderName { get; private set; } = "Sounds";

        public static T Load<T>(string path) => Content.Load<T>(path);
        public static Dictionary<string, T> LoadFolder<T>(string contentFolder)
        {
            string folderPath = GetFolderPath(contentFolder);

            string[] filePaths = Directory.GetFiles(folderPath, "*.xnb");

            List<string> fileNames = new();

            foreach (string path in filePaths) 
            {
                fileNames.Add(Path.GetFileNameWithoutExtension(path));
            }

            Dictionary<string, T> assets = new();
            foreach (string name in fileNames)
            {
                T asset = Load<T>(Path.Combine(contentFolder, name));

                assets.Add(name, asset);
            }

            return assets;
        }

        public static string GetFolderPath(string path) 
        {
            return Path.Combine(Environment.CurrentDirectory, Content.RootDirectory, path);
        }

        public static SpriteFont LoadFont(string name) => Content.Load<SpriteFont>($"{FontsFolderName}/{name}");
        public static Texture2D LoadLevelPicture(int index) => Content.Load<Texture2D>($"{LevelsFolderName}/level_{index}");
    }
}