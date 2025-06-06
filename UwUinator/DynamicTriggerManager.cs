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
        private readonly ConcurrentQueue<DateTime> _triggerTimestamps; // Queue for tracking triggers
        private readonly ConcurrentQueue<DateTime> _messageTimestamps; // Queue for tracking all messages
        private readonly object _lock = new(); // Lock for thread safety

        private readonly Dictionary<ulong, (DateTime LastTimestamp, ulong LastMessageId)>
            _lastMessageTimestamps = new();

        private readonly Random _random = new();

        // Dynamic thresholds
        private int _maxTriggersPerWindow; // Dynamically adjusted max triggers
        private int _timeWindowInSeconds; // Timeframe for scaling

        /// <summary>
        /// Initializes the DynamicTriggerManager with default settings.
        /// </summary>
        public DynamicTriggerManager(int defaultTimeWindowInSeconds, int baseMaxTriggersPerWindow,
            ILogger<UwUifyMessageHandler> logger)
        {
            _logger = logger; // Assign logger instance
            _defaultTimeWindowInSeconds = defaultTimeWindowInSeconds;

            _logger.LogInformation(
                "[Initialization] Default time window: {TimeWindow}s, Base max triggers per window: {MaxTriggers}",
                _defaultTimeWindowInSeconds, baseMaxTriggersPerWindow);
            _timeWindowInSeconds = defaultTimeWindowInSeconds;
            _maxTriggersPerWindow = baseMaxTriggersPerWindow;

            _triggerTimestamps = new ConcurrentQueue<DateTime>();
            _messageTimestamps = new ConcurrentQueue<DateTime>();
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
            _messageTimestamps.Enqueue(timestamp);
            AdjustScalingFactors();
            RemoveOldTimestamps(_messageTimestamps);
            _logger.LogDebug("[RecordMessage] Total messages after cleanup: {MessageCount}",
                _messageTimestamps.Count);
        }

        /// <summary>
        /// Determines if the bot should trigger based on activity and load.
        /// </summary>
        /// <returns>True if the bot should trigger; otherwise, false.</returns>
        public bool ShouldTrigger(SocketMessage message)
        {
            lock (_lock)
            {
                RemoveOldTimestamps(_triggerTimestamps);

                // Enforce cooldown for low activity (e.g., one trigger per minute)
                if (_triggerTimestamps.TryPeek(out var lastTrigger) &&
                    (DateTime.UtcNow - lastTrigger).TotalSeconds < 60)
                {
                    _logger.LogWarning(
                        "[ShouldTrigger] Rejected due to cooldown. Last trigger was {TimeSinceLastTrigger}s ago.",
                        (DateTime.UtcNow - lastTrigger).TotalSeconds);
                    return false;
                }

                var triggerProbability = CalculateTriggerProbability(message.Content);
                _logger.LogDebug("[ShouldTrigger] Calculated trigger probability: {TriggerProbability}",
                    triggerProbability);

                if (!ShouldConsiderMessage(message) || ContainsLink(message.Content) || HasAttachments(message))
                    return false;

                // Roll the chance based on the calculated probability
                var result = _random.NextDouble() < triggerProbability;
                _logger.LogDebug("[ShouldTrigger] Should trigger: {Result}", result);
                return result;
            }
        }

        /// <summary>
        /// Records a trigger event.
        /// </summary>
        public void RecordTrigger()
        {
            lock (_lock)
            {
                if (_triggerTimestamps.Count > _maxTriggersPerWindow)
                    return;

                var timestamp = DateTime.UtcNow;
                _triggerTimestamps.Enqueue(timestamp);
                _logger.LogDebug("[RecordTrigger] Trigger recorded at {Timestamp}. Total triggers: {TriggerCount}",
                    timestamp, _triggerTimestamps.Count);
                RemoveOldTimestamps(_triggerTimestamps);
            }
        }

        /// <summary>
        /// Removes outdated timestamps outside the time window.
        /// </summary>
        private void RemoveOldTimestamps(ConcurrentQueue<DateTime> timestamps)
        {
            int removedCount = 0;
            while (timestamps.TryPeek(out var oldest) &&
                   (DateTime.UtcNow - oldest).TotalSeconds > _timeWindowInSeconds)
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
        private void AdjustScalingFactors()
        {
            var totalMessageCount = _messageTimestamps.Count;

            // For very low activity, use minimum thresholds
            if (totalMessageCount < 5)
            {
                _timeWindowInSeconds = _defaultTimeWindowInSeconds; // Keep the default window
                _maxTriggersPerWindow = 1; // Allow only 1 trigger
            }
            else
            {
                _timeWindowInSeconds = _defaultTimeWindowInSeconds + totalMessageCount / 100;
                _maxTriggersPerWindow = Math.Max(10, totalMessageCount / 10); // Scale based on activity
            }

            _logger.LogInformation(
                "[AdjustScalingFactors] Adjusted time window: {TimeWindow}s, Max triggers per window: {MaxTriggers}",
                _timeWindowInSeconds, _maxTriggersPerWindow);
        }

        /// <summary>
        /// Calculates trigger probability based on activity and load.
        /// </summary>
        /// <returns>A double representing trigger likelihood.</returns>
        private double CalculateTriggerProbability(string messageContent)
        {
            var triggerCount = _triggerTimestamps.Count;
            var totalMessages = _messageTimestamps.Count;
            _logger.LogInformation("[CalculateTriggerProbability] Total Messages: {TotalMessages}", totalMessages);

            if (totalMessages == 0)
                return 0.0; // Don't trigger if no messages in the window

            // Count occurrences of 'l' and 'r' in the message
            int lrCount = Regex.Matches(messageContent ?? string.Empty, "[lr]", RegexOptions.IgnoreCase).Count;

            // Base probability
            double baseProbability = Math.Max(0.2, 0.5 - (double)triggerCount / _maxTriggersPerWindow);

            // Increase probability based on 'l'/'r' count
            double lrFactor = Math.Min(0.2, lrCount * 0.01); // Cap additional probability at 0.2
            baseProbability += lrFactor;

            // Scale down probability if trigger limit is reached
            if (triggerCount >= _maxTriggersPerWindow)
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