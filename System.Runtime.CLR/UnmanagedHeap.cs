﻿using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Runtime.CLR
{
	public delegate void CtorDelegate(IntPtr obj);
	
	internal static class Stub
	{
		public static void Construct(object obj, int value)
		{
		}	
	}
	
	public class UnmanagedObject<T> : IDisposable where T : UnmanagedObject<T>
	{	
		internal IUnmanagedHeap<T> heap;
		
		#region IDisposable implementation
		void IDisposable.Dispose()
		{
			heap.Free(this);
		}
		#endregion
	}
	
	
	public interface IUnmanagedHeapBase  : IDisposable
	{
		int TotalSize { get; }		
		object Allocate();
		void Free(object obj);
		void Reset();
	}
	
	public interface IUnmanagedHeap<TPoolItem> : IUnmanagedHeapBase where TPoolItem : UnmanagedObject<TPoolItem>
	{
		int TotalSize { get; }		
		TPoolItem Allocate();
		void Free(TPoolItem obj);
		void Reset();
	}
	
	/// <summary>
	/// Description of UnmanagedHeap.
	/// </summary>
	public unsafe class UnmanagedHeap<TPoolItem> : IUnmanagedHeap<TPoolItem> where TPoolItem : UnmanagedObject<TPoolItem>
	{
		private readonly TPoolItem[] _freeObjects;
		private readonly TPoolItem[] _allObjects;
		private readonly int _totalSize;
		private int _freeSize;
	    private int startingPointer;
		private readonly ConstructorInfo _ctor;
		
		public UnmanagedHeap(int capacity)
		{
			_allObjects = new TPoolItem[capacity];
			_freeSize = capacity;
			
            // Getting type size and total pool size
			var objectSize = GCEx.SizeOf<TPoolItem>();
			_totalSize = objectSize * capacity;
			
			startingPointer = Marshal.AllocHGlobal(_totalSize).ToInt32();
			var mTable = (MethodTableInfo *)typeof(TPoolItem).TypeHandle.Value.ToInt32();
			
			var pFake = typeof(Stub).GetMethod("Construct", BindingFlags.Static|BindingFlags.Public);
			var pCtor = _ctor = typeof(TPoolItem).GetConstructor(new []{typeof(int)});
		
			MethodUtil.ReplaceMethod(pCtor, pFake, skip: true);
			
			for(int i = 0; i < capacity; i++)
			{
				var handler =  startingPointer + (objectSize * i);
			    var obj = EntityPtr.ToInstance<object>((IntPtr)handler);
                obj.SetType<TPoolItem>();
				
				var reference = (TPoolItem)obj;
				reference.heap = this;
				
				_allObjects[i] = reference;
			}			
			
			_freeObjects = (TPoolItem[])_allObjects.Clone();
		}
		
		public int TotalSize
		{
			get {
				return _totalSize;
			}
		}
				
		public TPoolItem Allocate()
		{
			_freeSize--;
			var obj = _freeObjects[_freeSize];
			Stub.Construct(obj, 123);			
			return obj;
		}
		
		public TPoolItem AllocatePure()
		{
            _freeSize--;
			var obj = _freeObjects[_freeSize]; 
			_ctor.Invoke(obj, new object[]{123});			
			return obj;
		}
		
		public void Free(TPoolItem obj)
		{
			_freeObjects[_freeSize] = obj;
			_freeSize++;
		}	
		
		public void Reset()
		{
			_allObjects.CopyTo(_freeObjects, 0);
			_freeSize = _freeObjects.Length;
		}

		object IUnmanagedHeapBase.Allocate()
		{
			return this.Allocate();
		}
		
		void IUnmanagedHeapBase.Free(object obj)
		{
			this.Free((TPoolItem)obj);
		}

        public void Dispose()
        {
            Marshal.FreeHGlobal((IntPtr)startingPointer);
        }
    }
}
