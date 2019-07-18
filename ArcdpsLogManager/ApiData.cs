using System;
using System.Collections.Concurrent;
using System.Threading;
using GW2Scratch.ArcdpsLogManager.GW2Api.V2;

namespace GW2Scratch.ArcdpsLogManager
{
	public class ApiData
	{
		private readonly GuildEndpoint guildEndpoint;

		private readonly ConcurrentDictionary<string, ApiGuild> guildDataCache =
			new ConcurrentDictionary<string, ApiGuild>();
		private readonly ConcurrentQueue<string> pendingGuildGuids = new ConcurrentQueue<string>();

		private Timer workerTimer;

		public event EventHandler GuildAdded;

		public int TotalRequestCount { get; private set; } = 0;
		public int CachedGuildCount => guildDataCache.Count;

		public ApiData(GuildEndpoint guildEndpoint)
		{
			this.guildEndpoint = guildEndpoint;
		}

		/// <summary>
		/// Starts the worker that fetches requested API data periodically.
		/// </summary>
		public void StartApiWorker()
		{
			if (workerTimer == null)
			{
				workerTimer = new Timer(_ => FetchApiData(), null, TimeSpan.Zero, TimeSpan.FromSeconds(65));
			}
		}

		/// <summary>
		/// Stops the worker that fetches requested API data periodically.
		/// </summary>
		public void StopApiWorker()
		{
			workerTimer?.Dispose();
			workerTimer = null;
		}

		private void FetchApiData()
		{
			int requests = 0;
			while (requests < 500)
			{
				if (!pendingGuildGuids.TryDequeue(out string guid))
				{
					break;
				}

				if (guildDataCache.ContainsKey(guid))
				{
					continue;
				}

				try
				{
					TotalRequestCount++;
					var guild = guildEndpoint.GetGuild(guid);
					guildDataCache[guid] = guild;
					OnGuildAdded();
					requests++;
				}
				catch
				{
					break;
				}
			}
		}

		private ApiGuild GetApiGuild(string guid)
		{
			if (guid == null)
			{
				throw new ArgumentNullException(nameof(guid));
			}

			if (guildDataCache.TryGetValue(guid, out var data))
			{
				return data;
			}

			return null;
		}

		/// <summary>
		/// Register a guild GUID, potentially scheduling it for retrieval of data from the API.
		/// </summary>
		/// <param name="guid">A guild GUID</param>
		public void RegisterGuild(string guid)
		{
			if (!guildDataCache.ContainsKey(guid))
			{
				pendingGuildGuids.Enqueue(guid);
			}
		}

		/// <summary>
		/// Get a guild name of a guild. In case the guild data has not been retrieved from the API,
		/// returns a name based on the GUID.
		/// </summary>
		/// <param name="guid">A guild GUID</param>
		/// <returns>The guild name, or an guid-based identifier if API data is not available.</returns>
		public string GetGuildName(string guid)
		{
			return GetApiGuild(guid)?.Name ?? $"Unknown ({guid.Substring(0, 7)})";
		}

		/// <summary>
		/// Get a guild tag of a guild.
		/// </summary>
		/// <param name="guid">A guild GUID</param>
		/// <param name="unavailableDefault">A value to be returned if the API data is not available.</param>
		/// <returns>The guild tag, or <paramref name="unavailableDefault"/> API data is not available.</returns>
		public string GetGuildTag(string guid, string unavailableDefault = "???")
		{
			return GetApiGuild(guid)?.Tag ?? unavailableDefault;
		}

		protected virtual void OnGuildAdded()
		{
			GuildAdded?.Invoke(this, EventArgs.Empty);
		}
	}
}