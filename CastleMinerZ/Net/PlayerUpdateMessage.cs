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
			PlayerUpdateMessage Instance = Message.GetSendInstance<PlayerUpdateMessage>();
			Instance.LocalPosition = player.LocalPosition;
			Instance.WorldVelocity = player.PlayerPhysics.WorldVelocity;
			Instance.LocalRotation = player.LocalRotation;
			Instance.Movement = input.Movement;
			Instance.TorsoPitch = player.TorsoPitch;
			Instance.Using = player.UsingTool;
			Instance.Shouldering = player.Shouldering;
			Instance.Reloading = player.Reloading;
			Instance.PlayerMode = player._playerMode;
			Instance.Dead = player.Dead;
			Instance.ThrowingGrenade = player.PlayGrenadeAnim;
			Instance.ReadyToThrowGrenade = player.ReadyToThrowGrenade;
			Instance.DoSend(from);
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
			byte torso = reader.ReadByte();
			byte rot = reader.ReadByte();
			this.LocalRotation = Quaternion.CreateFromYawPitchRoll(Angle.FromRevolutions((float)rot / 255f).Radians, 0f, 0f);
			this.TorsoPitch = Angle.FromRevolutions((float)torso / 510f) - Angle.FromDegrees(90f);
			byte flags = reader.ReadByte();
			this.PlayerMode = (PlayerMode)reader.ReadByte();
			this.Using = (flags & 1) != 0;
			this.Dead = (flags & 2) != 0;
			this.Shouldering = (flags & 4) != 0;
			this.Reloading = (flags & 8) != 0;
			this.ThrowingGrenade = (flags & 16) != 0;
			this.ReadyToThrowGrenade = (flags & 32) != 0;
			byte movementByte = reader.ReadByte();
			byte moveX = movementByte & 15;
			byte moveY = (byte)(movementByte >> 4);
			this.Movement.X = (float)moveX / 14f * 2f - 1f;
			this.Movement.Y = (float)moveY / 14f * 2f - 1f;
			HalfSingle hv = default(HalfSingle);
			hv.PackedValue = reader.ReadUInt16();
			this.WorldVelocity.X = hv.ToSingle();
			hv.PackedValue = reader.ReadUInt16();
			this.WorldVelocity.Y = hv.ToSingle();
			hv.PackedValue = reader.ReadUInt16();
			this.WorldVelocity.Z = hv.ToSingle();
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(this.LocalPosition);
			EulerAngle rot = new EulerAngle(this.LocalRotation);
			float rotFP = rot.Yaw.Revolutions;
			rotFP -= (float)Math.Floor((double)rotFP);
			byte rot4Bit = (byte)Math.Round((double)(255f * rotFP));
			float tpFP = (this.TorsoPitch + Angle.FromDegrees(90f)).Degrees / 180f;
			tpFP -= (float)Math.Floor((double)rotFP);
			byte torso4Bit = (byte)Math.Round((double)(255f * tpFP));
			byte flags = 0;
			if (this.Using)
			{
				flags |= 1;
			}
			if (this.Dead)
			{
				flags |= 2;
			}
			if (this.Shouldering)
			{
				flags |= 4;
			}
			if (this.Reloading)
			{
				flags |= 8;
			}
			if (this.ThrowingGrenade)
			{
				flags |= 16;
			}
			if (this.ReadyToThrowGrenade)
			{
				flags |= 32;
			}
			writer.Write(torso4Bit);
			writer.Write(rot4Bit);
			writer.Write(flags);
			writer.Write((byte)this.PlayerMode);
			byte moveX = (byte)((this.Movement.X + 1f) / 2f * 14f);
			byte moveY = (byte)((this.Movement.Y + 1f) / 2f * 14f);
			byte movementByte = (byte)(((int)moveY << 4) | (int)moveX);
			writer.Write(movementByte);
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
