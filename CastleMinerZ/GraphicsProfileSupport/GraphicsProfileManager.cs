using System;
using DNA.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.GraphicsProfileSupport
{
	public class GraphicsProfileManager
	{
		public static GraphicsProfileManager Instance
		{
			get
			{
				return GraphicsProfileManager._instance;
			}
		}

		public GraphicsProfile Profile
		{
			get
			{
				return this._profile;
			}
		}

		public bool IsHiDef
		{
			get
			{
				return this.Profile == GraphicsProfile.HiDef;
			}
		}

		public bool IsReach
		{
			get
			{
				return this.Profile == GraphicsProfile.Reach;
			}
		}

		public void ExamineGraphicsDevices(object sender, PreparingDeviceSettingsEventArgs args)
		{
			bool forceReachProfile = CommandLineArgs.Get<CastleMinerZArgs>().ForceReachProfile;
			GraphicsAdapter graphicsAdapter = args.GraphicsDeviceInformation.Adapter;
			if (graphicsAdapter == null)
			{
				graphicsAdapter = GraphicsAdapter.DefaultAdapter;
			}
			if (graphicsAdapter != null)
			{
				if (!forceReachProfile && graphicsAdapter.IsProfileSupported(GraphicsProfile.HiDef))
				{
					this._profile = GraphicsProfile.HiDef;
				}
				else
				{
					this._profile = GraphicsProfile.Reach;
				}
				args.GraphicsDeviceInformation.Adapter = graphicsAdapter;
				args.GraphicsDeviceInformation.GraphicsProfile = this._profile;
				return;
			}
			if (!forceReachProfile)
			{
				for (int i = 0; i < GraphicsAdapter.Adapters.Count; i++)
				{
					if (GraphicsAdapter.Adapters[i].IsProfileSupported(GraphicsProfile.HiDef))
					{
						args.GraphicsDeviceInformation.Adapter = GraphicsAdapter.Adapters[i];
						args.GraphicsDeviceInformation.GraphicsProfile = GraphicsProfile.HiDef;
						this._profile = GraphicsProfile.HiDef;
						return;
					}
				}
			}
			for (int j = 0; j < GraphicsAdapter.Adapters.Count; j++)
			{
				if (GraphicsAdapter.Adapters[j].IsProfileSupported(GraphicsProfile.Reach))
				{
					args.GraphicsDeviceInformation.Adapter = GraphicsAdapter.Adapters[j];
					args.GraphicsDeviceInformation.GraphicsProfile = GraphicsProfile.Reach;
					this._profile = GraphicsProfile.Reach;
					return;
				}
			}
		}

		private static GraphicsProfileManager _instance = new GraphicsProfileManager();

		private GraphicsProfile _profile;
	}
}
