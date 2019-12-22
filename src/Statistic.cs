using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BotTerminator
{
	public class Statistic
	{
		public bool UseTotalValue { get; set; }
		public String MetricId { get; set; }
		public Int32 RecentValue { get => recentValue; set => this.recentValue = value; }

		private Int32 value = 0;
		private Int32 recentValue = 0;

		/// <summary>
		/// The value of the counter. You should not increment or
		/// decrement it directly, rather you should use the functions
		/// provided.
		/// </summary>
		public Int32 Value { get => value; set => this.value = value; }

		public void Increment()
		{
			Interlocked.Increment(ref value);
			Interlocked.Increment(ref recentValue);
		}

		public void Decrement()
		{
			Interlocked.Decrement(ref value);
			Interlocked.Decrement(ref recentValue);
		}

		public void Reset(bool recentValueOnly = true)
		{
			Interlocked.MemoryBarrier();
			recentValue = 0;
			if (!recentValueOnly) value = 0;
			Interlocked.MemoryBarrier();
		}

		public void PushMetric(BotTerminator bot, bool? recentValue = null)
		{
			bool pushRecentValue = recentValue == null ? !UseTotalValue : recentValue.Value;
			bot.StatusPageQueueData.Enqueue(new Models.MetricData()
			{
				MetricId = MetricId,
				Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
				Value = pushRecentValue ? RecentValue : Value,
			});
		}
	}
}
