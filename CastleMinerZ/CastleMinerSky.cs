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

		public static void LoadTextures()
		{
			CastleMinerSky._dayTexture = CastleMinerZGame.Instance.Content.Load<TextureCube>("Textures\\Skys\\ClearSky");
			CastleMinerSky._nightTexture = CastleMinerZGame.Instance.Content.Load<TextureCube>("Textures\\Skys\\NightSky");
			CastleMinerSky._sunSetTexture = CastleMinerZGame.Instance.Content.Load<TextureCube>("Textures\\Skys\\SunSet");
			CastleMinerSky._dawnTexture = CastleMinerZGame.Instance.Content.Load<TextureCube>("Textures\\Skys\\DawnSky");
			CastleMinerSky._blendEffect = CastleMinerZGame.Instance.Content.Load<Effect>("Shaders\\TextureSky");
		}

		public CastleMinerSky()
			: base(CastleMinerZGame.Instance.GraphicsDevice, 500f, Vector3.Zero, 20, CastleMinerSky._blendEffect, CastleMinerSky._dayTexture)
		{
			this.DrawPriority = -1000;
		}

		public void SetParameters(Effect effect)
		{
			float hourf = this.TimeOfDay * 24f;
			float blender = hourf - (float)((int)hourf);
			int hour = (int)hourf;
			effect.Parameters["Blender"].SetValue(blender);
			if (hour <= 5 || hour >= 21)
			{
				effect.Parameters["Sky1Texture"].SetValue(CastleMinerSky._nightTexture);
				effect.Parameters["Sky2Texture"].SetValue(CastleMinerSky._nightTexture);
			}
			else if (hour >= 9 && hour <= 17)
			{
				effect.Parameters["Sky1Texture"].SetValue(CastleMinerSky._dayTexture);
				effect.Parameters["Sky2Texture"].SetValue(CastleMinerSky._dayTexture);
			}
			else if (hour == 6)
			{
				effect.Parameters["Sky1Texture"].SetValue(CastleMinerSky._nightTexture);
				effect.Parameters["Sky2Texture"].SetValue(CastleMinerSky._dawnTexture);
			}
			else if (hour == 7)
			{
				effect.Parameters["Sky1Texture"].SetValue(CastleMinerSky._dawnTexture);
				effect.Parameters["Sky2Texture"].SetValue(CastleMinerSky._dawnTexture);
			}
			else if (hour == 8)
			{
				effect.Parameters["Sky1Texture"].SetValue(CastleMinerSky._dawnTexture);
				effect.Parameters["Sky2Texture"].SetValue(CastleMinerSky._dayTexture);
			}
			else if (hour == 18)
			{
				effect.Parameters["Sky1Texture"].SetValue(CastleMinerSky._dayTexture);
				effect.Parameters["Sky2Texture"].SetValue(CastleMinerSky._sunSetTexture);
			}
			else if (hour == 19)
			{
				effect.Parameters["Sky1Texture"].SetValue(CastleMinerSky._sunSetTexture);
				effect.Parameters["Sky2Texture"].SetValue(CastleMinerSky._sunSetTexture);
			}
			else if (hour == 20)
			{
				effect.Parameters["Sky1Texture"].SetValue(CastleMinerSky._sunSetTexture);
				effect.Parameters["Sky2Texture"].SetValue(CastleMinerSky._nightTexture);
			}
			Vector3 tintColor = Vector3.Zero;
			float tintAmount = (CastleMinerZGame.Instance.LocalPlayer.LocalPosition.Y + 32f) / 8f;
			if (tintAmount < 0f)
			{
				tintAmount = 0f;
			}
			if (tintAmount > 1f)
			{
				tintAmount = 1f;
			}
			tintAmount = 1f - tintAmount;
			if (this.drawLightning)
			{
				tintColor = new Vector3(1f, 1f, 1f);
				tintAmount = 1f;
			}
			effect.Parameters["LerpColor"].SetValue(tintColor);
			effect.Parameters["LerpAmount"].SetValue(tintAmount);
		}

		protected override bool SetEffectParams(DNAEffect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
		{
			this.SetParameters(effect);
			float hourf = this.TimeOfDay * 24f;
			int hour = (int)hourf;
			if (hour > 5 && hour < 20)
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

		private static Effect _blendEffect;

		private static TextureCube _dayTexture;

		private static TextureCube _nightTexture;

		private static TextureCube _sunSetTexture;

		private static TextureCube _dawnTexture;

		public bool drawLightning;

		public float Day = 0.41f;

		private float rot;
	}
}
