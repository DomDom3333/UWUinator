using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Discord.WebSocket;

namespace UwUinator
{
    public class DynamicTriggerManager
    {
        private readonly int _defaultTimeWindowInSeconds; // Default time window
        private readonly ConcurrentQueue<DateTime> _triggerTimestamps; // Queue for tracking triggers
        private readonly ConcurrentQueue<DateTime> _messageTimestamps; // Queue for tracking all messages
        private readonly object _lock = new(); // Lock for thread safety
        private readonly Dictionary<ulong, DateTime> _lastMessageTimestamps = new(); // Tracks last messages per user
        private readonly Random _random = new();

        // Dynamic thresholds
        private int _maxTriggersPerWindow; // Dynamically adjusted max triggers
        private int _timeWindowInSeconds; // Timeframe for scaling

        /// <summary>
        /// Initializes the DynamicTriggerManager with default settings.
        /// </summary>
        public DynamicTriggerManager(int defaultTimeWindowInSeconds, int baseMaxTriggersPerWindow)
        {
            _defaultTimeWindowInSeconds = defaultTimeWindowInSeconds;
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
            {
                _messageTimestamps.Enqueue(DateTime.UtcNow);
                RemoveOldTimestamps(_messageTimestamps);
                AdjustScalingFactors(); // Adjust scaling based on activity
            }
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

                // Calculate the current trigger probability
                var triggerProbability = CalculateTriggerProbability();

                if (!ShouldConsiderMessage(message) || ContainsLink(message.Content) || HasAttachments(message))
                    return false;

                // Roll the chance based on the calculated probability
                return _random.NextDouble() < triggerProbability;
            }
        }

        /// <summary>
        /// Records a trigger event.
        /// </summary>
        public void RecordTrigger()
        {
            lock (_lock)
                if (_triggerTimestamps.Count > _maxTriggersPerWindow)
                    return;
            {
                _triggerTimestamps.Enqueue(DateTime.UtcNow);
                RemoveOldTimestamps(_triggerTimestamps);
            }
        }

        /// <summary>
        /// Removes outdated timestamps outside the time window.
        /// </summary>
        private void RemoveOldTimestamps(ConcurrentQueue<DateTime> timestamps)
        {
            while (timestamps.TryPeek(out var oldest) &&
                   (DateTime.UtcNow - oldest).TotalSeconds > _timeWindowInSeconds)
            {
                timestamps.TryDequeue(out _);
            }
        }

        /// <summary>
        /// Adjusts scaling factors dynamically based on recent activity.
        /// </summary>
        private void AdjustScalingFactors()
        {
            // Total messages in the current timeframe
            var totalMessageCount = _messageTimestamps.Count;

            // Dynamically scale the time window and max triggers
            _timeWindowInSeconds = _defaultTimeWindowInSeconds + totalMessageCount / 100; // Adjust time window
            _maxTriggersPerWindow = Math.Max(10, totalMessageCount / 10); // Scale max triggers
        }

        /// <summary>
        /// Calculates trigger probability based on activity and load.
        /// </summary>
        /// <returns>A double representing trigger likelihood.</returns>
        private double CalculateTriggerProbability()
        {
            var triggerCount = _triggerTimestamps.Count;
            var totalMessages = _messageTimestamps.Count;

            // Base chance with load scaling
            double baseProbability = totalMessages > 0
                ? Math.Max(0.3, 1.0 - (double)triggerCount / _maxTriggersPerWindow) // Scale with activity
                : 0.9; // Default high chance under no detected messages

            // Additional scaling factors for heavily loaded cases
            if (triggerCount >= _maxTriggersPerWindow)
            {
                return Math.Max(0.1, baseProbability * 0.5); // Reduce chances when limit approaches
            }

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
            return userMessage.Attachments != null && userMessage.Attachments.Count > 0;
        }

        private bool ShouldConsiderMessage(SocketMessage message)
        {
            if (message.Author.IsBot)
                return false;
            if (message.Source == Discord.MessageSource.System)
                return false;
            if (!message.MentionedUsers.Contains(message.Author))
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
                if (_lastMessageTimestamps.TryGetValue(message.Author.Id, out var lastTimestamp) &&
                    (now - lastTimestamp).TotalSeconds <= 10)
                {
                    return false;
                }

                _lastMessageTimestamps[message.Author.Id] = now;
            }

            return true;
        }
    }
}