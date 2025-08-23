using System;
using System.Threading;

namespace DNA.CastleMinerZ.Utils
{
	public class LockFreeStack<T> where T : class, ILinkedListNode
	{
		public T Root
		{
			get
			{
				return this._root;
			}
		}

		public void Push(T newNode)
		{
			if (newNode == null)
			{
				return;
			}
			T oldRoot = default(T);
			do
			{
				oldRoot = this._root;
				newNode.NextNode = oldRoot;
			}
			while (oldRoot != Interlocked.CompareExchange<T>(ref this._root, newNode, oldRoot));
		}

		public void PushList(T newList)
		{
			if (newList == null)
			{
				return;
			}
			ILinkedListNode i = newList;
			while (i.NextNode != null)
			{
				i = i.NextNode;
			}
			T oldRoot = default(T);
			do
			{
				oldRoot = this._root;
				i.NextNode = oldRoot;
			}
			while (oldRoot != Interlocked.CompareExchange<T>(ref this._root, newList, oldRoot));
		}

		public T Pop()
		{
			T result;
			do
			{
				result = this._root;
			}
			while (result != null && result != Interlocked.CompareExchange<T>(ref this._root, result.NextNode as T, result));
			if (result != null)
			{
				result.NextNode = null;
			}
			return result;
		}

		public T Clear()
		{
			T oldRoot = default(T);
			do
			{
				oldRoot = this._root;
			}
			while (oldRoot != Interlocked.CompareExchange<T>(ref this._root, default(T), oldRoot));
			return oldRoot;
		}

		public bool Empty
		{
			get
			{
				return this._root == null;
			}
		}

		private T _root = default(T);
	}
}
