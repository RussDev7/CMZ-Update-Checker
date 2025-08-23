using System;
using System.Collections.Generic;
using DNA.Drawing;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ
{
	public class GameMessageManager : Entity
	{
		public GameMessageManager()
		{
			for (int i = 0; i < 3; i++)
			{
				this._handlers[i] = new List<WeakReference<IGameMessageHandler>>();
			}
			GameMessageManager.Instance = this;
			this.Collider = false;
			this.Collidee = false;
		}

		public void Subscribe(IGameMessageHandler handler, params GameMessageType[] types)
		{
			foreach (GameMessageType type in types)
			{
				List<WeakReference<IGameMessageHandler>> list = this._handlers[(int)type];
				for (int i = 0; i < list.Count; i++)
				{
					IGameMessageHandler t = list[i].Target;
					if (t == null)
					{
						list.RemoveAt(i);
						i--;
					}
					else if (t == handler)
					{
						return;
					}
				}
				list.Add(new WeakReference<IGameMessageHandler>(handler));
			}
		}

		public void UnSubscribe(GameMessageType type, IGameMessageHandler handler)
		{
			List<WeakReference<IGameMessageHandler>> list = this._handlers[(int)type];
			for (int i = 0; i < list.Count; i++)
			{
				IGameMessageHandler t = list[i].Target;
				if (t == null)
				{
					list.RemoveAt(i);
					i--;
				}
				else if (t == handler)
				{
					list.RemoveAt(i);
					return;
				}
			}
		}

		public void Send(GameMessageType type, object data, object sender)
		{
			List<WeakReference<IGameMessageHandler>> list = this._handlers[(int)type];
			for (int i = 0; i < list.Count; i++)
			{
				IGameMessageHandler t = list[i].Target;
				if (t == null)
				{
					list.RemoveAt(i);
					i--;
				}
				else
				{
					t.HandleMessage(type, data, sender);
				}
			}
		}

		protected override void OnUpdate(GameTime gameTime)
		{
		}

		public static GameMessageManager Instance;

		private List<WeakReference<IGameMessageHandler>>[] _handlers = new List<WeakReference<IGameMessageHandler>>[3];
	}
}
