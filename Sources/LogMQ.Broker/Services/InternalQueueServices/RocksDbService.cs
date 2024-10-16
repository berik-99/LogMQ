using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogMQ.Broker.Services.InternalQueueServices
{
	using RocksDbSharp;
	using System;
	using System.Threading;
	using System.Threading.Tasks;

	public class RocksDbService : IDisposable
	{
		private readonly RocksDb db;
		private readonly SemaphoreSlim semaphoreSlim = new(1, 1);

		public RocksDbService()
		{
			try
			{
				var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "rocksdb_data");
				Directory.CreateDirectory(dbPath);
				var options = new DbOptions()
					.SetCreateIfMissing(true)
					.SetWriteBufferSize(64 * 1024 * 1024)
					.SetMaxWriteBufferNumber(3)
					.SetCompression(Compression.Snappy);
				db = RocksDb.Open(options, dbPath);
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		public async Task WriteLogMessage(string key, string message)
		{
			await semaphoreSlim.WaitAsync();
			try
			{
				db.Put(key, message);
			}
			finally
			{
				semaphoreSlim.Release();
			}
		}

		public void Dispose()
		{
			db?.Dispose();
			semaphoreSlim?.Dispose();
			GC.SuppressFinalize(this);
		}
	}

}
