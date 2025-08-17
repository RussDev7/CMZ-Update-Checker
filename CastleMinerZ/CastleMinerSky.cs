using System;
using DNA.CastleMinerZ.Terrain;
using DNA.Drawing;
using DNA.Drawing.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ
{
	public class CastleMinerSky : SkySphere
	{
		public float TimeOfDay
		{
			get
			{
				return this.Day - (float)Math.Floor((double)this.Day);
			}
		}

		public CastleMinerSky()
			: base(CastleMinerZGame.Instance.GraphicsDevice, 500f, Vector3.Zero, 20, CastleMinerSky._blendEffect, CastleMinerSky._dayTexture)
		{
			this.DrawPriority = -1000;
		}

		public void SetParameters(Effect effect)
		{
			float num = this.TimeOfDay * 24f;
			float num2 = num - (float)((int)num);
			int num3 = (int)num;
			effect.Parameters["Blender"].SetValue(num2);
			if (num3 <= 5 || num3 >= 21)
			{
				effect.Parameters["Sky1Texture"].SetValue(CastleMinerSky._nightTexture);
				effect.Parameters["Sky2Texture"].SetValue(CastleMinerSky._nightTexture);
			}
			else if (num3 >= 9 && num3 <= 17)
			{
				effect.Parameters["Sky1Texture"].SetValue(CastleMinerSky._dayTexture);
				effect.Parameters["Sky2Texture"].SetValue(CastleMinerSky._dayTexture);
			}
			else if (num3 == 6)
			{
				effect.Parameters["Sky1Texture"].SetValue(CastleMinerSky._nightTexture);
				effect.Parameters["Sky2Texture"].SetValue(CastleMinerSky._dawnTexture);
			}
			else if (num3 == 7)
			{
				effect.Parameters["Sky1Texture"].SetValue(CastleMinerSky._dawnTexture);
				effect.Parameters["Sky2Texture"].SetValue(CastleMinerSky._dawnTexture);
			}
			else if (num3 == 8)
			{
				effect.Parameters["Sky1Texture"].SetValue(CastleMinerSky._dawnTexture);
				effect.Parameters["Sky2Texture"].SetValue(CastleMinerSky._dayTexture);
			}
			else if (num3 == 18)
			{
				effect.Parameters["Sky1Texture"].SetValue(CastleMinerSky._dayTexture);
				effect.Parameters["Sky2Texture"].SetValue(CastleMinerSky._sunSetTexture);
			}
			else if (num3 == 19)
			{
				effect.Parameters["Sky1Texture"].SetValue(CastleMinerSky._sunSetTexture);
				effect.Parameters["Sky2Texture"].SetValue(CastleMinerSky._sunSetTexture);
			}
			else if (num3 == 20)
			{
				effect.Parameters["Sky1Texture"].SetValue(CastleMinerSky._sunSetTexture);
				effect.Parameters["Sky2Texture"].SetValue(CastleMinerSky._nightTexture);
			}
			Vector3 zero = Vector3.Zero;
			float num4 = (CastleMinerZGame.Instance.LocalPlayer.LocalPosition.Y + 32f) / 8f;
			if (num4 < 0f)
			{
				num4 = 0f;
			}
			if (num4 > 1f)
			{
				num4 = 1f;
			}
			num4 = 1f - num4;
			if (this.drawLightning)
			{
				zero = new Vector3(1f, 1f, 1f);
				num4 = 1f;
			}
			effect.Parameters["LerpColor"].SetValue(zero);
			effect.Parameters["LerpAmount"].SetValue(num4);
		}

		protected override bool SetEffectParams(DNAEffect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
		{
			this.SetParameters(effect);
			float num = this.TimeOfDay * 24f;
			int num2 = (int)num;
			if (num2 > 5 && num2 < 20)
			{
				this.rot += (float)gameTime.ElapsedGameTime.TotalSeconds / 320f;
			}
			effect.Parameters["CloudOffset"].SetValue(Matrix.CreateRotationY(this.rot));
			effect.Parameters["Offset"].SetValue(new Vector3(0f, -100f, 0f));
			return base.SetEffectParams(effect, gameTime, world, view, projection);
		}

		public override void Draw(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection)
		{
			if (CastleMinerZGame.Instance.DrawingReflection && BlockTerrain.Instance.EyePos.Y >= BlockTerrain.Instance.WaterLevel)
			{
				this.DrawReflection(device, gameTime, view, projection);
				return;
			}
			base.Draw(device, gameTime, view, projection);
		}

		public void DrawReflection(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection)
		{
			throw new NotImplementedException();
		}

		private static Effect _blendEffect = CastleMinerZGame.Instance.Content.Load<Effect>("Shaders\\TextureSky");

		private static TextureCube _dayTexture = CastleMinerZGame.Instance.Content.Load<TextureCube>("Textures\\Skys\\ClearSky");

		private static TextureCube _nightTexture = CastleMinerZGame.Instance.Content.Load<TextureCube>("Textures\\Skys\\NightSky");

		private static TextureCube _sunSetTexture = CastleMinerZGame.Instance.Content.Load<TextureCube>("Textures\\Skys\\SunSet");

		private static TextureCube _dawnTexture = CastleMinerZGame.Instance.Content.Load<TextureCube>("Textures\\Skys\\DawnSky");

		public bool drawLightning;

		public float Day = 0.41f;

		private float rot;
	}
}
