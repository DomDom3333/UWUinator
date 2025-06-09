using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace UwUinator
{
    public class DynamicTriggerManager
    {
        private readonly ILogger<UwUifyMessageHandler> _logger; // Logger instance
        private readonly int _defaultTimeWindowInSeconds; // Default time window
        private readonly int _baseMaxTriggersPerWindow; // Base max triggers
        private readonly object _lock = new(); // Lock for thread safety

        private class GuildActivity
        {
            public ConcurrentQueue<DateTime> MessageTimestamps { get; } = new();
            public ConcurrentQueue<DateTime> TriggerTimestamps { get; } = new();
            public int TimeWindowInSeconds;
            public int MaxTriggersPerWindow;
            public DateTime LastTrigger = DateTime.MinValue;
            public int BackoffLevel;
        }

        private readonly Dictionary<ulong, GuildActivity> _guildActivities = new();

        private readonly Dictionary<ulong, (DateTime LastTimestamp, ulong LastMessageId)>
            _lastMessageTimestamps = new();

        private readonly Random _random = new();

        private GuildActivity GetGuildActivity(ulong guildId)
        {
            if (!_guildActivities.TryGetValue(guildId, out var activity))
            {
                activity = new GuildActivity
                {
                    TimeWindowInSeconds = _defaultTimeWindowInSeconds,
                    MaxTriggersPerWindow = _baseMaxTriggersPerWindow
                };
                _guildActivities[guildId] = activity;
            }

            return activity;
        }

        // All dynamic thresholds are now tracked per guild

        /// <summary>
        /// Initializes the DynamicTriggerManager with default settings.
        /// </summary>
        public DynamicTriggerManager(int defaultTimeWindowInSeconds, int baseMaxTriggersPerWindow,
            ILogger<UwUifyMessageHandler> logger)
        {
            _logger = logger; // Assign logger instance
            _defaultTimeWindowInSeconds = defaultTimeWindowInSeconds;
            _baseMaxTriggersPerWindow = baseMaxTriggersPerWindow;

            _logger.LogInformation(
                "[Initialization] Default time window: {TimeWindow}s, Base max triggers per window: {MaxTriggers}",
                _defaultTimeWindowInSeconds, _baseMaxTriggersPerWindow);
        }

        /// <summary>
        /// Records a received message for activity tracking.
        /// </summary>
        public void RecordMessage(SocketMessage message)
        {
            if (!ShouldConsiderMessage(message))
                return;

            var timestamp = DateTime.UtcNow;
            _logger.LogDebug("[RecordMessage] Message recorded at {Timestamp}. Content: \"{Content}\"",
                timestamp, message.Content);

            var guildId = (message.Channel as SocketTextChannel)?.Guild.Id;
            if (guildId == null)
                return;

            lock (_lock)
            {
                var activity = GetGuildActivity(guildId.Value);
                activity.MessageTimestamps.Enqueue(timestamp);
                AdjustScalingFactors(activity);
                RemoveOldTimestamps(activity.MessageTimestamps, activity.TimeWindowInSeconds);
                _logger.LogDebug("[RecordMessage] Guild {Guild} message count: {MessageCount}",
                    guildId, activity.MessageTimestamps.Count);
            }
        }

        /// <summary>
        /// Determines if the bot should trigger based on activity and load.
        /// </summary>
        /// <returns>True if the bot should trigger; otherwise, false.</returns>
        public bool ShouldTrigger(SocketMessage message)
        {
            var guildId = (message.Channel as SocketTextChannel)?.Guild.Id;
            if (guildId == null)
                return false;

            lock (_lock)
            {
                var activity = GetGuildActivity(guildId.Value);

                RemoveOldTimestamps(activity.TriggerTimestamps, activity.TimeWindowInSeconds);

                var now = DateTime.UtcNow;
                double cooldown = 30 * Math.Pow(2, activity.BackoffLevel);
                if ((now - activity.LastTrigger).TotalSeconds < cooldown)
                {
                    _logger.LogWarning("[ShouldTrigger] Guild {Guild} in backoff. Cooldown {Cooldown}s remaining", guildId,
                        cooldown - (now - activity.LastTrigger).TotalSeconds);
                    return false;
                }

                // Decay backoff over time
                if (activity.BackoffLevel > 0 && (now - activity.LastTrigger).TotalSeconds >= cooldown)
                    activity.BackoffLevel--;

                var triggerProbability = CalculateTriggerProbability(message.Content, activity);
                _logger.LogDebug("[ShouldTrigger] Calculated trigger probability: {TriggerProbability}",
                    triggerProbability);

                if (!ShouldConsiderMessage(message) || ContainsLink(message.Content) || HasAttachments(message))
                    return false;

                var result = _random.NextDouble() < triggerProbability;
                _logger.LogDebug("[ShouldTrigger] Should trigger: {Result}", result);
                return result;
            }
        }

        /// <summary>
        /// Records a trigger event.
        /// </summary>
        public void RecordTrigger(SocketMessage message)
        {
            var guildId = (message.Channel as SocketTextChannel)?.Guild.Id;
            if (guildId == null)
                return;

            lock (_lock)
            {
                var activity = GetGuildActivity(guildId.Value);

                if (activity.TriggerTimestamps.Count > activity.MaxTriggersPerWindow)
                    return;

                var timestamp = DateTime.UtcNow;
                activity.TriggerTimestamps.Enqueue(timestamp);
                activity.LastTrigger = timestamp;
                activity.BackoffLevel = Math.Min(activity.BackoffLevel + 1, 5);

                _logger.LogDebug(
                    "[RecordTrigger] Guild {Guild} trigger recorded at {Timestamp}. Total: {TriggerCount}",
                    guildId, timestamp, activity.TriggerTimestamps.Count);

                RemoveOldTimestamps(activity.TriggerTimestamps, activity.TimeWindowInSeconds);
            }
        }

        /// <summary>
        /// Removes outdated timestamps outside the time window.
        /// </summary>
        private void RemoveOldTimestamps(ConcurrentQueue<DateTime> timestamps, int windowInSeconds)
        {
            int removedCount = 0;
            while (timestamps.TryPeek(out var oldest) &&
                   (DateTime.UtcNow - oldest).TotalSeconds > windowInSeconds)
            {
                timestamps.TryDequeue(out _);
                removedCount++;
            }

            _logger.LogDebug(
                "[RemoveOldTimestamps] Removed {RemovedCount} outdated timestamps. Remaining: {RemainingCount}",
                removedCount, timestamps.Count);
        }

        /// <summary>
        /// Adjusts scaling factors dynamically based on recent activity.
        /// </summary>
        private void AdjustScalingFactors(GuildActivity activity)
        {
            var totalMessageCount = activity.MessageTimestamps.Count;

            if (totalMessageCount < 5)
            {
                activity.TimeWindowInSeconds = _defaultTimeWindowInSeconds;
                activity.MaxTriggersPerWindow = 1;
            }
            else
            {
                activity.TimeWindowInSeconds = _defaultTimeWindowInSeconds + totalMessageCount / 100;
                activity.MaxTriggersPerWindow = Math.Max(_baseMaxTriggersPerWindow,
                    totalMessageCount / 10);
            }

            _logger.LogInformation(
                "[AdjustScalingFactors] Guild scaling updated. Time window: {TimeWindow}s, Max triggers: {MaxTriggers}",
                activity.TimeWindowInSeconds, activity.MaxTriggersPerWindow);
        }

        /// <summary>
        /// Calculates trigger probability based on activity and load.
        /// </summary>
        /// <returns>A double representing trigger likelihood.</returns>
        private double CalculateTriggerProbability(string messageContent, GuildActivity activity)
        {
            var triggerCount = activity.TriggerTimestamps.Count;
            var totalMessages = activity.MessageTimestamps.Count;
            _logger.LogInformation("[CalculateTriggerProbability] Guild messages: {TotalMessages}", totalMessages);

            if (totalMessages == 0)
                return 0.0; // Don't trigger if no messages in the window

            // Count occurrences of 'l' and 'r' in the message
            int lrCount = Regex.Matches(messageContent ?? string.Empty, "[lr]", RegexOptions.IgnoreCase).Count;

            // Base probability
            double baseProbability = Math.Max(0.2, 0.5 - (double)triggerCount / activity.MaxTriggersPerWindow);

            // Increase probability based on 'l'/'r' count
            double lrFactor = Math.Min(0.2, lrCount * 0.01); // Cap additional probability at 0.2
            baseProbability += lrFactor;

            // Scale down probability if trigger limit is reached
            if (triggerCount >= activity.MaxTriggersPerWindow)
            {
                return Math.Max(0.1, baseProbability * 0.5);
            }

            _logger.LogDebug(
                "[CalculateTriggerProbability] Final Probability: {BaseProbability}, LR Factor: {LRFactor}",
                baseProbability, lrFactor);

            return baseProbability;
        }

        private bool ContainsLink(string messageContent)
        {
            // Regular expression to identify URLs in the message
            string urlPattern = @"((http|https):\/\/)?(\w+\.)?[\w\-]+\.\w+([\/\w\-_?=&.]*)?";
            return Regex.IsMatch(messageContent, urlPattern, RegexOptions.IgnoreCase);
        }

        private bool HasAttachments(SocketMessage userMessage)
        {
            var hasAttachments = userMessage.Attachments != null && userMessage.Attachments.Count > 0;
            _logger.LogDebug("[HasAttachments] Message has attachments: {HasAttachments}", hasAttachments);
            return hasAttachments;
        }

        private bool ShouldConsiderMessage(SocketMessage message)
        {
            if (message.Author.IsBot)
                return false;
            if (message.Source == Discord.MessageSource.System)
                return false;
            if (message.Content.Length < 3)
                return false;
            string[] disallowedWords = { "spamword1", "spamword2" }; // Placeholder
            foreach (var word in disallowedWords)
            {
                if (message.Content.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
                    return false;
            }

            lock (_lock)
            {
                var now = DateTime.UtcNow;

                // Check if user already has a last message
                if (_lastMessageTimestamps.TryGetValue(message.Author.Id, out var entry))
                {
                    var timeDifference = (now - entry.LastTimestamp).TotalSeconds;

                    // If the message being processed is the same one stored, skip the cooldown check
                    if (entry.LastMessageId == message.Id)
                    {
                        _logger.LogDebug(
                            "[ShouldConsiderMessage] Skipping cooldown check for the same message ID: {MessageId}",
                            message.Id);
                    }
                    else if (timeDifference <= 10)
                    {
                        _logger.LogWarning(
                            "[ShouldConsiderMessage] Rejected due to cooldown. Time difference: {TimeDifference}s",
                            timeDifference);
                        return false;
                    }
                }

                // Update the user’s timestamp and message ID
                _lastMessageTimestamps[message.Author.Id] = (now, message.Id);
                _logger.LogDebug("[ShouldConsiderMessage] Updated timestamp for user {UserId} to {Timestamp}.",
                    message.Author.Id, now);
            }

            _logger.LogInformation("[ShouldConsiderMessage] Message is considered valid for triggering.");
            return true;
        }
    }
}