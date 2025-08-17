using System;
using System.Collections.Generic;
using DNA.IO.Storage;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ
{
	public class WorldManager
	{
		public WorldInfo[] GetWorlds()
		{
			WorldInfo[] array = new WorldInfo[this.SavedWorlds.Count];
			this.SavedWorlds.Values.CopyTo(array, 0);
			return array;
		}

		public WorldManager(SignedInGamer gamer, SaveDevice device)
		{
			this._device = device;
			this._gamer = gamer;
			WorldInfo[] array = WorldInfo.LoadWorldInfo(this._device);
			foreach (WorldInfo worldInfo in array)
			{
				this.SavedWorlds[worldInfo.WorldID] = worldInfo;
			}
		}

		public void Delete(WorldInfo info)
		{
			this.SavedWorlds.Remove(info.WorldID);
			try
			{
				this._device.DeleteDirectory(info.SavePath);
			}
			catch
			{
			}
		}

		public void TakeOwnership(WorldInfo info)
		{
			if (this.SavedWorlds.ContainsKey(info.WorldID))
			{
				this.SavedWorlds.Remove(info.WorldID);
			}
			info.TakeOwnership(this._gamer, this._device);
			this.SavedWorlds[info.WorldID] = info;
		}

		public void RegisterNetworkWorld(WorldInfo newWorld)
		{
			WorldInfo worldInfo;
			if (this.SavedWorlds.TryGetValue(newWorld.WorldID, out worldInfo))
			{
				newWorld.LastPosition = worldInfo.LastPosition;
				newWorld.SavePath = worldInfo.SavePath;
			}
			else
			{
				newWorld.LastPosition = Vector3.Zero;
			}
			this.SavedWorlds[newWorld.WorldID] = newWorld;
			newWorld.SaveToStorage(this._gamer, CastleMinerZGame.Instance.SaveDevice);
		}

		private Dictionary<Guid, WorldInfo> SavedWorlds = new Dictionary<Guid, WorldInfo>(0);

		private SignedInGamer _gamer;

		private SaveDevice _device;
	}
}
