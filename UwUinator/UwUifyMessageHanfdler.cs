using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace UwUinator
{
    public class UwUifyMessageHandler : IMessageHandler
    {
        private readonly ILogger<UwUifyMessageHandler> _logger;
        private readonly Random _random = new();
        private readonly HashSet<string> _recentFeatures = new();
        private readonly DynamicTriggerManager _triggerManager =
            new DynamicTriggerManager(defaultTimeWindowInSeconds: 60, baseMaxTriggersPerWindow: 10);

        public UwUifyMessageHandler(ILogger<UwUifyMessageHandler> logger)
        {
            _logger = logger;
        }

        public async Task HandleMessageAsync(SocketMessage message, DiscordSocketClient client)
        {
            if (message.Author.IsBot || message.Channel is not SocketTextChannel)
                return;

            _triggerManager.RecordMessage(message);

            var channel = (SocketTextChannel)message.Channel;
            bool directedAtBot = message.MentionedUsers.Any(user => user.Id == client.CurrentUser.Id);

            if (IsAlreadyUwUified(message.Content))
                return;

            if (!_triggerManager.ShouldTrigger(message))
                return;

            _triggerManager.RecordTrigger();

            // Pre-Typing Messages (introduce anticipation)
            await SimulateTyping(channel, directedAtBot, message.Content.Length);

            string uwuifiedMessage = UwUify(message.Content, directedAtBot);

            // Optionally add extra message flair
            if (!directedAtBot && _random.NextDouble() < 0.1)
            {
                uwuifiedMessage = $"nyaaa~ {message.Author.Username}-san, I’m hewe 4 nya!! >w< *huggies* 💕" + Environment.NewLine + uwuifiedMessage;
            }
            
            await channel.SendMessageAsync(uwuifiedMessage, messageReference: new MessageReference(message.Id));
            _logger.LogInformation($"UwUified message: {uwuifiedMessage}");
        }

        private async Task SimulateTyping(SocketTextChannel channel, bool directedAtBot, int messageLength)
        {
            if (directedAtBot && _random.NextDouble() < 0.3)
            {
                string[] preTypingMessages =
                    { "hewwwo~ wait nya~!", "uhhh let me think nya!", "oh nyoo~ one sec pwease!! OwO" };
                await channel.SendMessageAsync(preTypingMessages[_random.Next(preTypingMessages.Length)]);
            }

            int typingDelay = Math.Clamp(messageLength * 50, directedAtBot ? 300 : 500, directedAtBot ? 800 : 2000);
            await channel.TriggerTypingAsync();
            await Task.Delay(typingDelay);
        }

        private string UwUify(string input, bool directedAtBot = false)
        {
            if (IsAlreadyUwUified(input))
                return input;

            string uwuified = ReplaceWordsWithKawaiiPhrases(input);
            uwuified = Regex.Replace(uwuified, "[rl]", "w");
            uwuified = Regex.Replace(uwuified, "[RL]", "W");

            // Add seasonal flair and mood enhancements early
            uwuified = AddSeasonalFlair(uwuified);
            uwuified = AddMoodEnhancements(uwuified);
            uwuified = AddRelevantEmojis(uwuified);

            // Apply stuttering early in the process
            uwuified = AddStuttering(uwuified);

            // Random feature pool selection
            List<Func<string, string>> featurePool = new()
            {
                AddEmotiveWords, // Apply emotive words
                AddKawaiiActions, // Add cute actions based on context
            };

            // Additional features if the message is directed at the bot
            if (directedAtBot)
            {
                featurePool.Add(AddOverexcitedMode);
                featurePool.Add(AddShynessLayer);
                featurePool.Add(AddThemeBasedWords);
            }

            featurePool = featurePool.OrderBy(_ => _random.Next()).Take(_random.Next(2, 5)).ToList();

            // Apply selected features progressively
            foreach (var feature in featurePool)
            {
                uwuified = feature(uwuified);
                TrackFeature(feature.Method.Name);
            }

            // Optional final enhancements
            if (_random.NextDouble() < 0.1)
                uwuified += " nyaa~ UwU bot is hewe fow yuu!! (✿~✿)";

            return uwuified;
        }

        private string AddStuttering(string input)
        {
            return Regex.Replace(input, @"\b(\w)", match =>
            {
                // Skip stuttering for user tags or numeric values
                if (match.Value.StartsWith("@") || Regex.IsMatch(match.Value, @"^\d+$"))
                {
                    return match.Value; // Leave as-is
                }

                return _random.NextDouble() < 0.1 ? $"{match.Value}-{match.Value}{match.Value}" : match.Value;
            });
        }

        private string ReplaceWordsWithKawaiiPhrases(string input)
        {
            Dictionary<string, string> replacements = new()
            {
                { "love", "wuv" },
                { "hello", "hewwo" },
                { "you", "yuu" },
                { "happy", "happeh" },
                { "friend", "fwiend" },
                { "sorry", "sowwy" }
            };

            foreach (var pair in replacements)
            {
                input = Regex.Replace(input, $"\b{pair.Key}\b", pair.Value, RegexOptions.IgnoreCase);
            }

            return input;
        }

        private string AddRelevantEmojis(string input)
        {
            Dictionary<string, string> emojiMap = new()
            {
                { "cat", "🐱" },
                { "dog", "🐶" },
                { "love", "❤️" },
                { "cute", "🥰" },
                { "sad", "😿" },
                { "happy", "😊" },
                { "food", "🍔" }
            };

            foreach (var pair in emojiMap)
            {
                input = Regex.Replace(input, $"\b{pair.Key}\b", $"{pair.Key} {pair.Value}", RegexOptions.IgnoreCase);
            }

            return input;
        }

        private string AddEmotiveWords(string input)
        {
            string[] emotiveWords = { "owo", "uwu", "(✿◕‿◕✿)", ":3", "〜(˚☐˚〜)" };
            string[] words = input.Split(' ');

            for (int i = 0; i < words.Length; i++)
            {
                if (_random.NextDouble() < 0.2)
                {
                    words[i] += " " + emotiveWords[_random.Next(emotiveWords.Length)];
                }
            }

            return string.Join(' ', words);
        }

        private string AddKawaiiActions(string input)
        {
            string[] actions = { "*blushes*", "*giggles*", "*rawr*", "*nuzzles*", "*paws at you*" };

            if (_random.NextDouble() < 0.3)
            {
                input += $" {actions[_random.Next(actions.Length)]}";
            }

            return input;
        }

        private string AddMoodEnhancements(string input)
        {
            if (Regex.IsMatch(input, @"\b(sad|cry|upset|bad|mad|angry|frustrated)\b", RegexOptions.IgnoreCase))
            {
                input += " qwq *huggies* pwease dun be sad... (✿´‿`) *hands cookie* 🍪.";
            }

            if (Regex.IsMatch(input, @"\b(happy|yay|excited|awesome|great|good news)\b", RegexOptions.IgnoreCase))
            {
                input += " OwO!! Dat'z so nyice nya~! (✧∀✧)/ ♥";
            }

            if (Regex.IsMatch(input, @"\b(bot|help|question|problem|issue)\b", RegexOptions.IgnoreCase))
            {
                input += " I’m hewe to h-help! nyaa~ nyaaa~... 🛠️ wat can I do fow you?? 🤔";
            }

            return input;
        }

        private string AddSeasonalFlair(string input)
        {
            if (DateTime.Now.Month == 12)
            {
                input += " 🎅 Nyaa~ it’z Chwistmas UwU!! ❄❄ uwuu~";
            }
            else if (DateTime.Now.Month == 10)
            {
                input += " 🎃 Spooky uwu!! hewwo fwom da UwU bot nya~ boo~";
            }

            return input;
        }

        private string AddShynessLayer(string input)
        {
            if (Regex.IsMatch(input, @"\b(blush|shy|nervous)\b", RegexOptions.IgnoreCase))
            {
                input += " uwu oh nyoo... I-I'm a bit bwushy >///< ♥";
            }

            return input;
        }

        private string AddOverexcitedMode(string input)
        {
            if (Regex.IsMatch(input, "!+$"))
            {
                input = Regex.Replace(input, "!+", "!!! owo!!! UwU!!!", RegexOptions.IgnoreCase);
            }

            return input;
        }

        private bool IsAlreadyUwUified(string input)
        {
            return Regex.IsMatch(input, @"[wW]uv|[oO]wo|[uU]wu") || input.Contains("-w");
        }

        private void TrackFeature(string featureKey)
        {
            _recentFeatures.Add(featureKey);
            if (_recentFeatures.Count > 5)
                _recentFeatures.Remove(_recentFeatures.First());
        }

        private string AddThemeBasedWords(string input)
        {
            if (input.Contains("book"))
            {
                input += " nyyaa~ do u wike weading stowies?? UwU ♥";
            }
            else if (input.Contains("game"))
            {
                input += " OwO!! Gamers rise up nyaaa~!!! *pew pew*";
            }
            else if (input.Contains("cat"))
            {
                input += " Nyaa~ *purrs* UwU nya!! 😺";
            }

            return input;
        }
    }
}
