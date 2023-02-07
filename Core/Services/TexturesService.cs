using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Graphics;
using Blish_HUD.Modules.Managers;
using Kenedia.Modules.Core.Extensions;
using Kenedia.Modules.Core.Res;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Kenedia.Modules.Core.Services
{
    public class TexturesService : IDisposable
    {
        private readonly Dictionary<string, Texture2D> _loadedTextures = new();
        private readonly ContentsManager _contentsManager;
        private bool _disposed;

        public TexturesService(ContentsManager contentsManager)
        {
            _contentsManager = contentsManager;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if(_loadedTextures.Count > 0) _loadedTextures.Select(e => e.Value).DisposeAll();
            _loadedTextures.Clear();
        }

        public Texture2D GetTexture(string path, string key)
        {
            if (_loadedTextures.ContainsKey(key)) return _loadedTextures[key];
            Texture2D texture = _contentsManager.GetTexture(path);

            _loadedTextures.Add(key, texture);
            return texture;
        }

        public Texture2D GetTexture(Bitmap bitmap, string key)
        {
            if (_loadedTextures.ContainsKey(key)) return _loadedTextures[key];

            using GraphicsDeviceContext device = GameService.Graphics.LendGraphicsDeviceContext();
            using MemoryStream s = new();
            bitmap.Save(s, System.Drawing.Imaging.ImageFormat.Png);
            var texture = Texture2D.FromStream(device.GraphicsDevice, s);

            _loadedTextures.Add(key, texture);
            return texture;
        }
    }
}
