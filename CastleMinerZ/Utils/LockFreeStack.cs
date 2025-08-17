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
			T t = default(T);
			do
			{
				t = this._root;
				newNode.NextNode = t;
			}
			while (t != Interlocked.CompareExchange<T>(ref this._root, newNode, t));
		}

		public void PushList(T newList)
		{
			if (newList == null)
			{
				return;
			}
			ILinkedListNode linkedListNode = newList;
			while (linkedListNode.NextNode != null)
			{
				linkedListNode = linkedListNode.NextNode;
			}
			T t = default(T);
			do
			{
				t = this._root;
				linkedListNode.NextNode = t;
			}
			while (t != Interlocked.CompareExchange<T>(ref this._root, newList, t));
		}

		public T Pop()
		{
			T root;
			do
			{
				root = this._root;
			}
			while (root != null && root != Interlocked.CompareExchange<T>(ref this._root, root.NextNode as T, root));
			if (root != null)
			{
				root.NextNode = null;
			}
			return root;
		}

		public T Clear()
		{
			T t = default(T);
			do
			{
				t = this._root;
			}
			while (t != Interlocked.CompareExchange<T>(ref this._root, default(T), t));
			return t;
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
