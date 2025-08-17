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
				int num = CastleMinerZArgs.Instance.TextureFolder.Length + 1;
				foreach (string text in ProfiledContentManager._cImageExtensions)
				{
					string[] array = PathTools.RecursivelyGetFiles(CastleMinerZArgs.Instance.TextureFolder, text);
					if (array != null && array.Length > 0)
					{
						if (!this._checkUserTextures)
						{
							this._userTexturesDefault = CastleMinerZArgs.Instance.TextureFolder;
							this._checkUserTextures = true;
							this._availableTextures = new Dictionary<string, string>();
							this._preLoadedTextures = new Dictionary<string, Texture2D>();
						}
						foreach (string text2 in array)
						{
							string text3 = Path.ChangeExtension(text2, null);
							text3 = text3.Substring(num);
							text3 = text3.ToLower();
							if (text3.EndsWith("_m"))
							{
								text3 = text3.Substring(0, text3.Length - 2);
							}
							if (!this._availableTextures.ContainsKey(text3))
							{
								this._availableTextures.Add(text3, text2);
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
			string text = Path.Combine(CastleMinerZArgs.Instance.TextureFolder, "terrain_" + (GraphicsProfileManager.Instance.IsHiDef ? "hidef.dat" : "reach.dat"));
			if (!File.Exists(text))
			{
				return null;
			}
			Texture[] array = null;
			using (Stream stream = new FileStream(text, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				BinaryReader binaryReader = new BinaryReader(stream);
				array = new Texture[5];
				int num = (GraphicsProfileManager.Instance.IsHiDef ? 2048 : 1024);
				uint[] array2 = new uint[num * num];
				byte[] array3 = binaryReader.ReadBytes(num * num * 4);
				Buffer.BlockCopy(array3, 0, array2, 0, array3.Length);
				Texture2D texture2D = new Texture2D(CastleMinerZGame.Instance.GraphicsDevice, num, num, false, SurfaceFormat.Color);
				texture2D.SetData<uint>(array2);
				array[0] = texture2D;
				array3 = binaryReader.ReadBytes(num * num * 4);
				Buffer.BlockCopy(array3, 0, array2, 0, array3.Length);
				texture2D = new Texture2D(CastleMinerZGame.Instance.GraphicsDevice, num, num, false, SurfaceFormat.Color);
				texture2D.SetData<uint>(array2);
				array[1] = texture2D;
				array3 = binaryReader.ReadBytes(num * num * 4);
				Buffer.BlockCopy(array3, 0, array2, 0, array3.Length);
				texture2D = new Texture2D(CastleMinerZGame.Instance.GraphicsDevice, num, num, false, SurfaceFormat.Color);
				texture2D.SetData<uint>(array2);
				array[2] = texture2D;
				Texture2D texture2D2 = new Texture2D(CastleMinerZGame.Instance.GraphicsDevice, num / 2, num / 2, true, SurfaceFormat.Color);
				array[4] = texture2D2;
				Texture2D texture2D3 = new Texture2D(CastleMinerZGame.Instance.GraphicsDevice, num / 2, num / 2, true, SurfaceFormat.Color);
				array[3] = texture2D3;
				int num2 = num / 2;
				int num3 = 0;
				do
				{
					array2 = new uint[num2 * num2];
					array3 = binaryReader.ReadBytes(num2 * num2 * 4);
					Buffer.BlockCopy(array3, 0, array2, 0, array3.Length);
					texture2D2.SetData<uint>(num3, null, array2, 0, array2.Length);
					array3 = binaryReader.ReadBytes(num2 * num2 * 4);
					Buffer.BlockCopy(array3, 0, array2, 0, array3.Length);
					texture2D3.SetData<uint>(num3, null, array2, 0, array2.Length);
					num3++;
					num2 >>= 1;
				}
				while (num2 != 0);
				binaryReader.Close();
			}
			return array;
		}

		private Texture2D TryLoadFromFile(string assetName)
		{
			Texture2D texture2D;
			if (this._preLoadedTextures.TryGetValue(assetName, out texture2D))
			{
				return texture2D;
			}
			string text = assetName.ToLower();
			string text2;
			bool flag = this._availableTextures.TryGetValue(text, out text2);
			if (!flag)
			{
				if (GraphicsProfileManager.Instance.IsHiDef)
				{
					flag = this._availableTextures.TryGetValue(Path.Combine(this._hiDefRootLower, text), out text2);
				}
				else
				{
					flag = this._availableTextures.TryGetValue(Path.Combine(this._reachRootLower, text), out text2);
				}
			}
			if (!flag)
			{
				return null;
			}
			bool flag2 = false;
			text = text2.ToLower();
			bool flag3 = Path.ChangeExtension(text, null).EndsWith("_m");
			bool flag4 = false;
			if (flag3)
			{
				flag4 = text.Contains("_nrm_") || text.Contains("_n_");
			}
			do
			{
				if (GraphicsDeviceLocker.Instance.TryLockDeviceTimed(ref this._loadingTimer))
				{
					flag2 = true;
					try
					{
						texture2D = TextureLoader.LoadFromFile(CastleMinerZGame.Instance.GraphicsDevice, text2, flag3, flag4);
					}
					finally
					{
						GraphicsDeviceLocker.Instance.UnlockDevice();
					}
				}
				if (!flag2)
				{
					Thread.Sleep(10);
				}
			}
			while (!flag2);
			this._preLoadedTextures.Add(assetName, texture2D);
			return texture2D;
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
			Type typeFromHandle = typeof(T);
			bool flag = typeFromHandle.IsSubclassOf(typeof(Texture)) || typeFromHandle == typeof(Texture);
			T t = default(T);
			this._loadingTimer.Restart();
			if (this._checkUserTextures && flag)
			{
				t = ProfiledContentManager.Cast<T>(this.TryLoadFromFile(assetName));
				if (t != null)
				{
					return t;
				}
			}
			List<string> list = new List<string>();
			if (this.TextureLevel > 1 && flag)
			{
				string text = "_L" + this.TextureLevel.ToString();
				list.Add(assetName + text);
				if (GraphicsProfileManager.Instance.IsHiDef)
				{
					list.Add(this._hiDefRoot + "\\" + assetName + text);
				}
				else
				{
					list.Add(this._reachRoot + "\\" + assetName + text);
				}
			}
			list.Add(assetName);
			if (GraphicsProfileManager.Instance.IsHiDef)
			{
				list.Add(this._hiDefRoot + "\\" + assetName);
			}
			else
			{
				list.Add(this._reachRoot + "\\" + assetName);
			}
			for (int i = 0; i < list.Count; i++)
			{
				try
				{
					return base.Load<T>(list[i]);
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
