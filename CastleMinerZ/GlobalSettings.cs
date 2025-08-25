using System;
using System.IO;
using System.Text;
using DNA.CastleMinerZ.GraphicsProfileSupport;
using DNA.Drawing;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace DNA.CastleMinerZ
{
	public class GlobalSettings
	{
		public GlobalSettings()
		{
			if (GraphicsProfileManager.Instance.Profile == GraphicsProfile.Reach)
			{
				this.TextureQualityLevel = 2;
			}
		}

		public static string GetAppDataDirectory(string subdir)
		{
			string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			string dataPath = Path.Combine(appData, "CastleMinerZ");
			if (!string.IsNullOrEmpty(subdir))
			{
				dataPath = Path.Combine(dataPath, subdir);
			}
			return dataPath;
		}

		public static string GetAppDataDirectory()
		{
			return GlobalSettings.GetAppDataDirectory(null);
		}

		private static string GetSettingsPath()
		{
			return Path.Combine(GlobalSettings.GetAppDataDirectory(), "user.settings");
		}

		public void Save()
		{
			try
			{
				string dataPath = GlobalSettings.GetAppDataDirectory();
				if (!Directory.Exists(dataPath))
				{
					Directory.CreateDirectory(dataPath);
				}
				string json = JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore,
					DefaultValueHandling = DefaultValueHandling.Include
				});
				File.WriteAllText(GlobalSettings.GetSettingsPath(), json, Encoding.UTF8);
			}
			catch
			{
			}
		}

		public void Load()
		{
			try
			{
				string fullSavePath = GlobalSettings.GetSettingsPath();
				if (File.Exists(fullSavePath))
				{
					string json = File.ReadAllText(fullSavePath, Encoding.UTF8);
					GlobalSettings loaded = JsonConvert.DeserializeObject<GlobalSettings>(json);
					if (loaded != null)
					{
						this.FullScreen = loaded.FullScreen;
						this.AskForFacebook = loaded.AskForFacebook;
						this.ScreenSize = loaded.ScreenSize;
						this.TextureQualityLevel = loaded.TextureQualityLevel;
					}
				}
			}
			catch
			{
			}
		}

		public bool FullScreen;

		public bool AskForFacebook = true;

		public Size ScreenSize = new Size(1280, 720);

		public int TextureQualityLevel = 1;
	}
}
