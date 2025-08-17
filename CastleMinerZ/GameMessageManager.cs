﻿using System;
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
			foreach (GameMessageType gameMessageType in types)
			{
				List<WeakReference<IGameMessageHandler>> list = this._handlers[(int)gameMessageType];
				for (int j = 0; j < list.Count; j++)
				{
					IGameMessageHandler target = list[j].Target;
					if (target == null)
					{
						list.RemoveAt(j);
						j--;
					}
					else if (target == handler)
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
				IGameMessageHandler target = list[i].Target;
				if (target == null)
				{
					list.RemoveAt(i);
					i--;
				}
				else if (target == handler)
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
				IGameMessageHandler target = list[i].Target;
				if (target == null)
				{
					list.RemoveAt(i);
					i--;
				}
				else
				{
					target.HandleMessage(type, data, sender);
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
