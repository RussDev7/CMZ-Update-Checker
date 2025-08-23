using System;
using DNA.CastleMinerZ.Inventory;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ
{
	public class GunEntity : CastleMinerToolModel
	{
		public void ShowMuzzleFlash()
		{
			this._muzzleFlash.Visible = true;
		}

		public GunEntity(Model gunModel, ItemUse use, bool attachedToLocalPlayer)
			: base(gunModel, use, attachedToLocalPlayer)
		{
			this._muzzleFlash = new ModelEntity(GunEntity._muzzleFlashModel);
			this._muzzleFlash.BlendState = BlendState.Additive;
			this._muzzleFlash.DepthStencilState = DepthStencilState.DepthRead;
			ModelBone barrelTip = base.Model.Bones["BarrelTip"];
			if (barrelTip != null)
			{
				this.BarrelTipLocation = Vector3.Transform(Vector3.Zero, barrelTip.Transform);
			}
			else
			{
				this.BarrelTipLocation = new Vector3(0f, 0f, -0.5f);
			}
			this._muzzleFlash.Visible = false;
			base.Children.Add(this._muzzleFlash);
			base.EnableDefaultLighting();
		}

		public override void Draw(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection)
		{
			base.CalculateLighting();
			base.Draw(device, gameTime, view, projection);
			this._muzzleFlash.LocalToParent = Matrix.CreateScale(0.75f + (float)this.rand.NextDouble() / 2f) * Matrix.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)this.rand.NextDouble() * 6.2831855f)) * base.Skeleton["BarrelTip"].Transform;
			this._muzzleFlash.Visible = false;
		}

		private static Model _muzzleFlashModel = CastleMinerZGame.Instance.Content.Load<Model>("MuzzleFlash");

		public Vector3 BarrelTipLocation;

		private ModelEntity _muzzleFlash;

		private Random rand = new Random();
	}
}
