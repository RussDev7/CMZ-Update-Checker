using System;
using System.IO;
using DNA.IO;
using DNA.Net;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace DNA.CastleMinerZ.Net
{
	public class PlayerUpdateMessage : CastleMinerZMessage
	{
		public override bool Echo
		{
			get
			{
				return false;
			}
		}

		private PlayerUpdateMessage()
		{
		}

		public void Apply(Player player)
		{
			player.LocalPosition = this.LocalPosition;
			player.PlayerPhysics.WorldVelocity = this.WorldVelocity;
			player.LocalRotation = this.LocalRotation;
			if (!player.IsLocal)
			{
				player.Shouldering = this.Shouldering;
				player.Reloading = this.Reloading;
				player.UsingTool = this.Using;
				player.Dead = this.Dead;
				player._playerMode = this.PlayerMode;
				player.PlayGrenadeAnim = this.ThrowingGrenade;
				player.ReadyToThrowGrenade = this.ReadyToThrowGrenade;
			}
			if (player.Dead)
			{
				player.UpdateAnimation(0f, 0f, Angle.Zero, this.PlayerMode, this.Using);
				return;
			}
			player.UpdateAnimation(this.Movement.Y, this.Movement.X, this.TorsoPitch, this.PlayerMode, this.Using);
		}

		public static void Send(LocalNetworkGamer from, Player player, CastleMinerZControllerMapping input)
		{
			PlayerUpdateMessage sendInstance = Message.GetSendInstance<PlayerUpdateMessage>();
			sendInstance.LocalPosition = player.LocalPosition;
			sendInstance.WorldVelocity = player.PlayerPhysics.WorldVelocity;
			sendInstance.LocalRotation = player.LocalRotation;
			sendInstance.Movement = input.Movement;
			sendInstance.TorsoPitch = player.TorsoPitch;
			sendInstance.Using = player.UsingTool;
			sendInstance.Shouldering = player.Shouldering;
			sendInstance.Reloading = player.Reloading;
			sendInstance.PlayerMode = player._playerMode;
			sendInstance.Dead = player.Dead;
			sendInstance.ThrowingGrenade = player.PlayGrenadeAnim;
			sendInstance.ReadyToThrowGrenade = player.ReadyToThrowGrenade;
			sendInstance.DoSend(from);
		}

		protected override SendDataOptions SendDataOptions
		{
			get
			{
				if (this.Using || this.Reloading)
				{
					return SendDataOptions.ReliableInOrder;
				}
				return SendDataOptions.InOrder;
			}
		}

		protected override void RecieveData(BinaryReader reader)
		{
			this.LocalPosition = reader.ReadVector3();
			byte b = reader.ReadByte();
			byte b2 = reader.ReadByte();
			this.LocalRotation = Quaternion.CreateFromYawPitchRoll(Angle.FromRevolutions((float)b2 / 255f).Radians, 0f, 0f);
			this.TorsoPitch = Angle.FromRevolutions((float)b / 510f) - Angle.FromDegrees(90f);
			byte b3 = reader.ReadByte();
			this.PlayerMode = (PlayerMode)reader.ReadByte();
			this.Using = (b3 & 1) != 0;
			this.Dead = (b3 & 2) != 0;
			this.Shouldering = (b3 & 4) != 0;
			this.Reloading = (b3 & 8) != 0;
			this.ThrowingGrenade = (b3 & 16) != 0;
			this.ReadyToThrowGrenade = (b3 & 32) != 0;
			byte b4 = reader.ReadByte();
			byte b5 = b4 & 15;
			byte b6 = (byte)(b4 >> 4);
			this.Movement.X = (float)b5 / 14f * 2f - 1f;
			this.Movement.Y = (float)b6 / 14f * 2f - 1f;
			HalfSingle halfSingle = default(HalfSingle);
			halfSingle.PackedValue = reader.ReadUInt16();
			this.WorldVelocity.X = halfSingle.ToSingle();
			halfSingle.PackedValue = reader.ReadUInt16();
			this.WorldVelocity.Y = halfSingle.ToSingle();
			halfSingle.PackedValue = reader.ReadUInt16();
			this.WorldVelocity.Z = halfSingle.ToSingle();
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(this.LocalPosition);
			EulerAngle eulerAngle = new EulerAngle(this.LocalRotation);
			float num = eulerAngle.Yaw.Revolutions;
			num -= (float)Math.Floor((double)num);
			byte b = (byte)Math.Round((double)(255f * num));
			float num2 = (this.TorsoPitch + Angle.FromDegrees(90f)).Degrees / 180f;
			num2 -= (float)Math.Floor((double)num);
			byte b2 = (byte)Math.Round((double)(255f * num2));
			byte b3 = 0;
			if (this.Using)
			{
				b3 |= 1;
			}
			if (this.Dead)
			{
				b3 |= 2;
			}
			if (this.Shouldering)
			{
				b3 |= 4;
			}
			if (this.Reloading)
			{
				b3 |= 8;
			}
			if (this.ThrowingGrenade)
			{
				b3 |= 16;
			}
			if (this.ReadyToThrowGrenade)
			{
				b3 |= 32;
			}
			writer.Write(b2);
			writer.Write(b);
			writer.Write(b3);
			writer.Write((byte)this.PlayerMode);
			byte b4 = (byte)((this.Movement.X + 1f) / 2f * 14f);
			byte b5 = (byte)((this.Movement.Y + 1f) / 2f * 14f);
			byte b6 = (byte)(((int)b5 << 4) | (int)b4);
			writer.Write(b6);
			writer.Write(new HalfSingle(this.WorldVelocity.X).PackedValue);
			writer.Write(new HalfSingle(this.WorldVelocity.Y).PackedValue);
			writer.Write(new HalfSingle(this.WorldVelocity.Z).PackedValue);
		}

		public override CastleMinerZMessage.MessageTypes MessageType
		{
			get
			{
				return CastleMinerZMessage.MessageTypes.PlayerUpdate;
			}
		}

		public Vector3 LocalPosition;

		public Quaternion LocalRotation;

		public Vector3 WorldVelocity;

		public Vector2 Movement;

		public Angle TorsoPitch;

		public bool Using;

		public bool Dead;

		public bool Shouldering;

		public bool Reloading;

		public PlayerMode PlayerMode;

		public bool ThrowingGrenade;

		public bool ReadyToThrowGrenade;
	}
}
