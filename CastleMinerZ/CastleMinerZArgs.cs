using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using DNA.Text;

namespace DNA.CastleMinerZ
{
	internal class CastleMinerZArgs : CommandLineArgs
	{
		protected void LobbyArgHandler(string flag, List<string> args)
		{
			string text = args[0];
			if (!ulong.TryParse(text, out this.InvitedLobbyID))
			{
				this.ErrorString = "Couldn't parse lobby id: " + text;
				this.ShowUsage = true;
				return;
			}
			if (this.InvitedLobbyID <= 0UL)
			{
				this.ErrorString = "Invalid lobby id: " + text;
				this.ShowUsage = true;
			}
		}

		protected void UserTexturesHandler(string flag, List<string> args)
		{
			if (args == null || args.Count != 1)
			{
				this.TextureFolder = GlobalSettings.GetAppDataDirectory("Textures");
				if (!Directory.Exists(this.TextureFolder))
				{
					Directory.CreateDirectory(this.TextureFolder);
					return;
				}
			}
			else
			{
				if (!Directory.Exists(args[0]))
				{
					this.ErrorString = "texture_folder argument is not a folder: " + args[0];
					this.ShowUsage = true;
					return;
				}
				this.TextureFolder = args[0];
			}
		}

		protected void DumpTexturesHandler(string flag, List<string> args)
		{
			if (args == null || args.Count != 1)
			{
				this.ErrorString = "Invalid argument for dump_textures (do you need quotes?):";
				this.ShowUsage = true;
				return;
			}
			if (!Directory.Exists(args[0]))
			{
				this.ErrorString = "dump_textures does not specify an existing folder: " + args[0];
				this.ShowUsage = true;
				return;
			}
			this.TextureDumpFolderName = args[0];
		}

		protected void ShowInventoryAtlasHandler(string flag, List<string> args)
		{
			this.ShowInventoryAtlas = true;
		}

		protected void ForceReachHandler(string flag, List<string> args)
		{
			this.ForceReachProfile = true;
		}

		protected void HelpArgHandler(string flag, List<string> args)
		{
			this.ShowUsage = true;
		}

		protected void TextureQualityHandler(string flag, List<string> args)
		{
			string text;
			if (args.Count > 0 && (text = args[0].ToLower()) != null)
			{
				if (text == "high")
				{
					this.TextureQualityLevel = 1;
					return;
				}
				if (text == "med")
				{
					this.TextureQualityLevel = 2;
					return;
				}
				if (!(text == "low"))
				{
					return;
				}
				this.TextureQualityLevel = 3;
			}
		}

		protected void ForceLanguageHandler(string flag, List<string> args)
		{
			string text;
			if (args.Count > 0 && (text = args[0].ToLower()) != null)
			{
				if (<PrivateImplementationDetails>{DF2B40B0-404E-4155-9011-DBEE096B8228}.$$method0x600031a-1 == null)
				{
					<PrivateImplementationDetails>{DF2B40B0-404E-4155-9011-DBEE096B8228}.$$method0x600031a-1 = new Dictionary<string, int>(8)
					{
						{ "english", 0 },
						{ "french", 1 },
						{ "german", 2 },
						{ "italian", 3 },
						{ "japanese", 4 },
						{ "portuguese", 5 },
						{ "russian", 6 },
						{ "spanish", 7 }
					};
				}
				int num;
				if (<PrivateImplementationDetails>{DF2B40B0-404E-4155-9011-DBEE096B8228}.$$method0x600031a-1.TryGetValue(text, out num))
				{
					switch (num)
					{
					case 0:
						Thread.CurrentThread.CurrentCulture = (Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US"));
						return;
					case 1:
						Thread.CurrentThread.CurrentCulture = (Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US"));
						return;
					case 2:
						Thread.CurrentThread.CurrentCulture = (Thread.CurrentThread.CurrentUICulture = new CultureInfo("de"));
						return;
					case 3:
						Thread.CurrentThread.CurrentCulture = (Thread.CurrentThread.CurrentUICulture = new CultureInfo("it"));
						return;
					case 4:
						Thread.CurrentThread.CurrentCulture = (Thread.CurrentThread.CurrentUICulture = new CultureInfo("ja"));
						return;
					case 5:
						Thread.CurrentThread.CurrentCulture = (Thread.CurrentThread.CurrentUICulture = new CultureInfo("pt"));
						return;
					case 6:
						Thread.CurrentThread.CurrentCulture = (Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru"));
						return;
					case 7:
						Thread.CurrentThread.CurrentCulture = (Thread.CurrentThread.CurrentUICulture = new CultureInfo("es"));
						break;
					default:
						return;
					}
				}
			}
		}

		public CastleMinerZArgs()
		{
			base.AddFlag("+connect_lobby", 1, new CommandLineArgs.ArgumentHandler(this.LobbyArgHandler), "Steam lobby ID to join on launch ", "[+connect_lobby steam_lobby_id]", StringComparison.OrdinalIgnoreCase);
			base.AddFlag("+reach", 0, new CommandLineArgs.ArgumentHandler(this.ForceReachHandler), "Force reach graphics profile", "[+reach]", StringComparison.OrdinalIgnoreCase);
			base.AddFlag("+texture_quality", 1, new CommandLineArgs.ArgumentHandler(this.TextureQualityHandler), "Texture quality (high,med,low)", "[+texture_quality [high|med|low]]", StringComparison.OrdinalIgnoreCase);
			base.AddFlag("+texture_folder", -2, new CommandLineArgs.ArgumentHandler(this.UserTexturesHandler), "Look for user supplied textures (default is AppData\\Local\\CastleMinerZ\\Textures)", "[+texture_folder [path_to_folder]]", StringComparison.OrdinalIgnoreCase);
			base.AddFlag("+language", -2, new CommandLineArgs.ArgumentHandler(this.ForceLanguageHandler), "Force the game into a language", "[+language [language]]", StringComparison.OrdinalIgnoreCase);
			base.AddFlag("+dump_textures", 1, new CommandLineArgs.ArgumentHandler(this.DumpTexturesHandler), "The game will dump all loaded textures to the given directory", "[+dump_textures path_to_folder]", StringComparison.OrdinalIgnoreCase);
			base.AddFlag("+show_inv_atlas", 0, new CommandLineArgs.ArgumentHandler(this.ShowInventoryAtlasHandler), null, null, StringComparison.OrdinalIgnoreCase);
			base.AddFlag("-h", 0, new CommandLineArgs.ArgumentHandler(this.HelpArgHandler), "Show this usage message", "[-h]", StringComparison.OrdinalIgnoreCase);
			base.AddFlag("-?", 0, new CommandLineArgs.ArgumentHandler(this.HelpArgHandler), "Show this usage message", "[-?]", StringComparison.OrdinalIgnoreCase);
			CastleMinerZArgs.Instance = this;
		}

		public static CastleMinerZArgs Instance;

		public ulong InvitedLobbyID;

		public bool ForceReachProfile;

		public string TextureFolder;

		public string TextureDumpFolderName;

		public bool ShowInventoryAtlas;

		public int TextureQualityLevel = 1;
	}
}
