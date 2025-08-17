using System;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.Utils.Trace;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ
{
	public class ConstructionProbeClass : TraceProbe
	{
		public void Init(Vector3 start, Vector3 end, bool checkEnemies)
		{
			base.Init(start, end);
			this.HitZombie = false;
			this.HitPlayer = false;
			this.PlayerHit = null;
			this.EnemyHit = null;
			this.CheckEnemies = checkEnemies;
		}

		public override bool AbleToBuild
		{
			get
			{
				return base.AbleToBuild && !this.HitZombie && !this.HitPlayer;
			}
		}

		public void Trace()
		{
			this.HitZombie = false;
			this.EnemyHit = null;
			this.HitPlayer = false;
			this.PlayerHit = null;
			if (this.CheckEnemies)
			{
				if (CastleMinerZGame.Instance.CurrentNetworkSession != null && CastleMinerZGame.Instance.PVPState != CastleMinerZGame.PVPEnum.Off)
				{
					for (int i = 0; i < CastleMinerZGame.Instance.CurrentNetworkSession.RemoteGamers.Count; i++)
					{
						NetworkGamer networkGamer = CastleMinerZGame.Instance.CurrentNetworkSession.RemoteGamers[i];
						if (networkGamer.Tag != null)
						{
							Player player = (Player)networkGamer.Tag;
							if (player.ValidLivingGamer)
							{
								Vector3 worldPosition = player.WorldPosition;
								BoundingBox playerAABB = player.PlayerAABB;
								playerAABB.Min += worldPosition;
								playerAABB.Max += worldPosition;
								this.TestBoundBox(playerAABB);
								if (this._collides && this._inT < 0.5f)
								{
									this.PlayerHit = player;
									this.HitPlayer = true;
									break;
								}
							}
						}
					}
				}
				IShootableEnemy shootableEnemy = EnemyManager.Instance.Trace(this, true);
				if (shootableEnemy is BaseZombie)
				{
					this.EnemyHit = (BaseZombie)shootableEnemy;
					this.HitZombie = true;
					return;
				}
			}
			else
			{
				BlockTerrain.Instance.Trace(this);
			}
		}

		public bool HitZombie;

		public bool HitPlayer;

		public bool CheckEnemies;

		public BaseZombie EnemyHit;

		public Player PlayerHit;
	}
}
