using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LibMemCs.Primitives
{
	public readonly unsafe struct UnmanagedSpan<T> 
		: IEquatable<UnmanagedSpan<T>>, IEnumerable<T>
		where T : unmanaged
	{
		public readonly T* Pointer;
		public readonly UIntPtr Length;

		public unsafe ref T Reference => ref Unsafe.AsRef<T>(Pointer);

		public UnmanagedSpan() { }
		public unsafe UnmanagedSpan(ref T reference) : this((T*)Unsafe.AsPointer(ref reference)) { }

		public unsafe UnmanagedSpan(T* pointer)
		{
			this.Pointer = pointer;
			this.Length = pointer is not null ? 1u : 0u;
		}

		public unsafe UnmanagedSpan(T[]? array)
		{
			if (array is null)
			{
				this = default;
				return;
			}
			this.Pointer = (T*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(array));
			this.Length = (UIntPtr)array.Length;
		}

		public unsafe UnmanagedSpan(ReadOnlySpan<T> span) : this(
			(T*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span)),
			(UIntPtr)span.Length
		)
		{
		}

		public unsafe UnmanagedSpan(T* pointer, UIntPtr length)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(length);
			if (pointer is null)
			{
				ArgumentOutOfRangeException.ThrowIfNotEqual(length, 0u);
			}
			this.Pointer = pointer;
			this.Length = length;
		}

		public unsafe T this[UIntPtr idx]
		{
			get
			{
				ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(idx, Length);
				return Pointer[idx];
			}
			set
			{
				ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(idx, Length);
				Pointer[idx] = value;
			}
		}

		public Boolean IsEmpty => Length == 0;

		public unsafe Boolean Equals(UnmanagedSpan<T> other) => Pointer == other.Pointer && Length == other.Length;

		public override Boolean Equals([NotNullWhen(true)] Object? obj) => obj is UnmanagedSpan<T> objSpan && Equals(objSpan);

		public static bool operator ==(UnmanagedSpan<T> left, UnmanagedSpan<T> right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(UnmanagedSpan<T> left, UnmanagedSpan<T> right)
		{
			return !(left == right);
		}

		public override unsafe Int32 GetHashCode() => HashCode.Combine((IntPtr)Pointer, Length);

		public override String? ToString()
		{
			return base.ToString();
		}

		public unsafe ref readonly T GetPinnableReference()
		{
			ref T ret = ref Unsafe.NullRef<T>();
			if (Length != 0) ret = ref Reference;
			return ref ret;
		}

		public IEnumerator<T> GetEnumerator() => new UnmanagedSpanEnumerator(this);

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public static implicit operator UnmanagedSpan<T>(ReadOnlySpan<T> span) => new UnmanagedSpan<T>(span);

		public struct UnmanagedSpanEnumerator : IEnumerator<T>
		{
			private readonly UnmanagedSpan<T> _reference;
			private UIntPtr _index;

			public readonly T Current => _reference[_index];

			readonly Object IEnumerator.Current => Current;

			public UnmanagedSpanEnumerator(UnmanagedSpan<T> reference)
			{
				_reference = reference;
			}

			public readonly void Dispose()
			{

			}

			public Boolean MoveNext()
			{
				UIntPtr index = _index;
				if (index >= _reference.Length) return false;
				index++;
				_index = index;
				return true;
			}

			public void Reset() => _index = 0;
		}
	}
}
