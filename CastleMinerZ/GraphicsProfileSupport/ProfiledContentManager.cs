using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using DNA.Drawing;
using DNA.Drawing.Imaging;
using DNA.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.GraphicsProfileSupport
{
	public class ProfiledContentManager : ContentManager
	{
		public ProfiledContentManager(IServiceProvider services, string reachRoot, string hiDefRoot, int textureLevel)
			: base(services)
		{
			this.TextureLevel = textureLevel;
			this._reachRoot = reachRoot;
			this._reachRootLower = reachRoot.ToLower();
			this._hiDefRoot = hiDefRoot;
			this._hiDefRootLower = hiDefRoot.ToLower();
			if (CastleMinerZArgs.Instance.TextureFolder != null)
			{
				this._checkUserTextures = false;
				int flength = CastleMinerZArgs.Instance.TextureFolder.Length + 1;
				foreach (string extension in ProfiledContentManager._cImageExtensions)
				{
					string[] textures = PathTools.RecursivelyGetFiles(CastleMinerZArgs.Instance.TextureFolder, extension);
					if (textures != null && textures.Length > 0)
					{
						if (!this._checkUserTextures)
						{
							this._userTexturesDefault = CastleMinerZArgs.Instance.TextureFolder;
							this._checkUserTextures = true;
							this._availableTextures = new Dictionary<string, string>();
							this._preLoadedTextures = new Dictionary<string, Texture2D>();
						}
						foreach (string name in textures)
						{
							string assetName = Path.ChangeExtension(name, null);
							assetName = assetName.Substring(flength);
							assetName = assetName.ToLower();
							if (assetName.EndsWith("_m"))
							{
								assetName = assetName.Substring(0, assetName.Length - 2);
							}
							if (!this._availableTextures.ContainsKey(assetName))
							{
								this._availableTextures.Add(assetName, name);
							}
						}
					}
				}
			}
			else
			{
				this._checkUserTextures = false;
			}
			this._loadingTimer = Stopwatch.StartNew();
		}

		internal Texture[] LoadTerrain()
		{
			string fname = Path.Combine(CastleMinerZArgs.Instance.TextureFolder, "terrain_" + (GraphicsProfileManager.Instance.IsHiDef ? "hidef.dat" : "reach.dat"));
			if (!File.Exists(fname))
			{
				return null;
			}
			Texture[] result = null;
			using (Stream stream = new FileStream(fname, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				BinaryReader reader = new BinaryReader(stream);
				result = new Texture[5];
				int imageWidth = (GraphicsProfileManager.Instance.IsHiDef ? 2048 : 1024);
				uint[] imgbyte4 = new uint[imageWidth * imageWidth];
				byte[] imgbytes = reader.ReadBytes(imageWidth * imageWidth * 4);
				Buffer.BlockCopy(imgbytes, 0, imgbyte4, 0, imgbytes.Length);
				Texture2D texture = new Texture2D(CastleMinerZGame.Instance.GraphicsDevice, imageWidth, imageWidth, false, SurfaceFormat.Color);
				texture.SetData<uint>(imgbyte4);
				result[0] = texture;
				imgbytes = reader.ReadBytes(imageWidth * imageWidth * 4);
				Buffer.BlockCopy(imgbytes, 0, imgbyte4, 0, imgbytes.Length);
				texture = new Texture2D(CastleMinerZGame.Instance.GraphicsDevice, imageWidth, imageWidth, false, SurfaceFormat.Color);
				texture.SetData<uint>(imgbyte4);
				result[1] = texture;
				imgbytes = reader.ReadBytes(imageWidth * imageWidth * 4);
				Buffer.BlockCopy(imgbytes, 0, imgbyte4, 0, imgbytes.Length);
				texture = new Texture2D(CastleMinerZGame.Instance.GraphicsDevice, imageWidth, imageWidth, false, SurfaceFormat.Color);
				texture.SetData<uint>(imgbyte4);
				result[2] = texture;
				Texture2D diffMips = new Texture2D(CastleMinerZGame.Instance.GraphicsDevice, imageWidth / 2, imageWidth / 2, true, SurfaceFormat.Color);
				result[4] = diffMips;
				Texture2D specMips = new Texture2D(CastleMinerZGame.Instance.GraphicsDevice, imageWidth / 2, imageWidth / 2, true, SurfaceFormat.Color);
				result[3] = specMips;
				int w = imageWidth / 2;
				int mip = 0;
				do
				{
					imgbyte4 = new uint[w * w];
					imgbytes = reader.ReadBytes(w * w * 4);
					Buffer.BlockCopy(imgbytes, 0, imgbyte4, 0, imgbytes.Length);
					diffMips.SetData<uint>(mip, null, imgbyte4, 0, imgbyte4.Length);
					imgbytes = reader.ReadBytes(w * w * 4);
					Buffer.BlockCopy(imgbytes, 0, imgbyte4, 0, imgbytes.Length);
					specMips.SetData<uint>(mip, null, imgbyte4, 0, imgbyte4.Length);
					mip++;
					w >>= 1;
				}
				while (w != 0);
				reader.Close();
			}
			return result;
		}

		private Texture2D TryLoadFromFile(string assetName)
		{
			Texture2D result;
			if (this._preLoadedTextures.TryGetValue(assetName, out result))
			{
				return result;
			}
			string lowerName = assetName.ToLower();
			string filename;
			bool foundName = this._availableTextures.TryGetValue(lowerName, out filename);
			if (!foundName)
			{
				if (GraphicsProfileManager.Instance.IsHiDef)
				{
					foundName = this._availableTextures.TryGetValue(Path.Combine(this._hiDefRootLower, lowerName), out filename);
				}
				else
				{
					foundName = this._availableTextures.TryGetValue(Path.Combine(this._reachRootLower, lowerName), out filename);
				}
			}
			if (!foundName)
			{
				return null;
			}
			bool loaded = false;
			lowerName = filename.ToLower();
			bool makeMipmaps = Path.ChangeExtension(lowerName, null).EndsWith("_m");
			bool normalizeMipmaps = false;
			if (makeMipmaps)
			{
				normalizeMipmaps = lowerName.Contains("_nrm_") || lowerName.Contains("_n_");
			}
			do
			{
				if (GraphicsDeviceLocker.Instance.TryLockDeviceTimed(ref this._loadingTimer))
				{
					loaded = true;
					try
					{
						result = TextureLoader.LoadFromFile(CastleMinerZGame.Instance.GraphicsDevice, filename, makeMipmaps, normalizeMipmaps);
					}
					finally
					{
						GraphicsDeviceLocker.Instance.UnlockDevice();
					}
				}
				if (!loaded)
				{
					Thread.Sleep(10);
				}
			}
			while (!loaded);
			this._preLoadedTextures.Add(assetName, result);
			return result;
		}

		public static T Cast<T>(object v)
		{
			if (v == null)
			{
				return default(T);
			}
			return (T)((object)v);
		}

		public override T Load<T>(string assetName)
		{
			Type templateType = typeof(T);
			bool isTexture = templateType.IsSubclassOf(typeof(Texture)) || templateType == typeof(Texture);
			T result = default(T);
			this._loadingTimer.Restart();
			if (this._checkUserTextures && isTexture)
			{
				result = ProfiledContentManager.Cast<T>(this.TryLoadFromFile(assetName));
				if (result != null)
				{
					return result;
				}
			}
			List<string> pathsToTry = new List<string>();
			if (this.TextureLevel > 1 && isTexture)
			{
				string levelSuffix = "_L" + this.TextureLevel.ToString();
				pathsToTry.Add(assetName + levelSuffix);
				if (GraphicsProfileManager.Instance.IsHiDef)
				{
					pathsToTry.Add(this._hiDefRoot + "\\" + assetName + levelSuffix);
				}
				else
				{
					pathsToTry.Add(this._reachRoot + "\\" + assetName + levelSuffix);
				}
			}
			pathsToTry.Add(assetName);
			if (GraphicsProfileManager.Instance.IsHiDef)
			{
				pathsToTry.Add(this._hiDefRoot + "\\" + assetName);
			}
			else
			{
				pathsToTry.Add(this._reachRoot + "\\" + assetName);
			}
			for (int i = 0; i < pathsToTry.Count; i++)
			{
				try
				{
					return base.Load<T>(pathsToTry[i]);
				}
				catch
				{
				}
			}
			throw new Exception("Asset not found " + assetName);
		}

		private static readonly string[] _cImageExtensions = new string[] { "*.dds", "*.tiff", "*.png", "*.jpg", "*.jpeg", "*.bmp" };

		private string _reachRoot;

		private string _hiDefRoot;

		private string _reachRootLower;

		private string _hiDefRootLower;

		private Stopwatch _loadingTimer;

		private bool _checkUserTextures;

		private string _userTexturesDefault;

		private Dictionary<string, string> _availableTextures;

		private Dictionary<string, Texture2D> _preLoadedTextures;

		private int TextureLevel;
	}
}
