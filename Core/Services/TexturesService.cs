﻿using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Modules.Managers;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Models;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Kenedia.Modules.Core.Services
{    
    public static class TexturesService
    {
        private static readonly Dictionary<string, Texture2D> s_loadedTextures = [];
        private static ContentsManager s_contentsManager;

        public static void Initilize(ContentsManager contentsManager)
        {
            s_contentsManager = contentsManager;
        }

        public static void Dispose()
        {
            if (s_loadedTextures.Count > 0) s_loadedTextures.Select(e => e.Value).DisposeAll();
            s_loadedTextures.Clear();
        }

        public static Texture2D GetTextureFromRef(string path, string key)
        {
            if (s_contentsManager is not null)
            {
                if (s_loadedTextures.ContainsKey(key)) return s_loadedTextures[key];
                Texture2D texture = s_contentsManager.GetTexture(path);

                s_loadedTextures.Add(key, texture);
                return texture;
            }

            return null;
        }

        public static Texture2D GetTextureFromRef(Bitmap bitmap, string key)
        {
            if (s_loadedTextures.ContainsKey(key)) return s_loadedTextures[key];
            Texture2D texture;
            s_loadedTextures.Add(key, texture = bitmap.CreateTexture2D());
            return texture;
        }

        public static AsyncTexture2D GetTextureFromDisk(string path)
        {
            if (s_loadedTextures.ContainsKey(path)) return s_loadedTextures[path];
            AsyncTexture2D texture = new();

            if (File.Exists(path))
            {
                GameService.Graphics.QueueMainThreadRender((graphicsDevice) => texture.SwapTexture(TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))));
            }

            return texture;
        }

        public static AsyncTexture2D GetAsyncTexture(int? id = 0)
        {
            int assetId = id ?? 0;

            if (assetId is not 0 && AsyncTexture2D.TryFromAssetId(assetId, out var texture))
            {
                return texture;
            }

            return null;
        }

        public static AsyncTexture2D GetTextureFromRef(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return ContentService.Textures.Error;
            }

            string key = Path.GetFileNameWithoutExtension(path);

            if (string.IsNullOrEmpty(key))
            {
                return ContentService.Textures.Error;
            }


            return GetTextureFromRef(path, key);
        }
    }
}
