using System;
using System.Collections.Generic;
using System.Threading;

namespace Blade.Razor
{
	internal class StringLock
	{
		private readonly Dictionary<string, LockObject> _keyLocks = new Dictionary<string, LockObject>();
		private readonly object _keyLocksLock = new object();

		public IDisposable AcquireLock(string key)
		{
			LockObject obj;
			lock (_keyLocksLock)
			{
				if (!_keyLocks.TryGetValue(key,
										  out obj))
				{
					_keyLocks[key] = obj = new LockObject(key);
				}
				obj.Withdraw();
			}
			Monitor.Enter(obj);
			return new DisposableToken(this,
									   obj);
		}

		private void ReturnLock(DisposableToken disposableLock)
		{
			var obj = disposableLock.LockObject;
			lock (_keyLocksLock)
			{
				if (obj.Return())
				{
					_keyLocks.Remove(obj.Key);
				}
				Monitor.Exit(obj);
			}
		}

		private class DisposableToken : IDisposable
		{
			private readonly LockObject _lockObject;
			private readonly StringLock _stringLock;
			private bool _disposed;

			public DisposableToken(StringLock stringLock, LockObject lockObject)
			{
				_stringLock = stringLock;
				_lockObject = lockObject;
			}

			public LockObject LockObject
			{
				get
				{
					return _lockObject;
				}
			}

			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			~DisposableToken()
			{
				Dispose(false);
			}

			private void Dispose(bool disposing)
			{
				if (disposing && !_disposed)
				{
					_stringLock.ReturnLock(this);
					_disposed = true;
				}
			}
		}

		private class LockObject
		{
			private readonly string _key;
			private int _leaseCount;

			public LockObject(string key)
			{
				_key = key;
			}

			public string Key
			{
				get
				{
					return _key;
				}
			}

			public void Withdraw()
			{
				Interlocked.Increment(ref _leaseCount);
			}

			public bool Return()
			{
				return Interlocked.Decrement(ref _leaseCount) == 0;
			}
		}
	}
}
