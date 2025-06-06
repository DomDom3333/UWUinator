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

        private readonly DynamicTriggerManager _triggerManager;

        public UwUifyMessageHandler(ILogger<UwUifyMessageHandler> logger)
        {
            _logger = logger;
            _triggerManager = new DynamicTriggerManager(defaultTimeWindowInSeconds: 60, baseMaxTriggersPerWindow: 10, logger);
        }

        public async Task HandleMessageAsync(SocketMessage message, DiscordSocketClient client)
        {
            _logger.LogInformation(
                $"Starting HandleMessageAsync for message ID: {message.Id}, Author: {message.Author}, Content: \"{message.Content}\"");
            if (message.Author.IsBot || message.Channel is not SocketTextChannel)
            {
                _logger.LogInformation(
                    $"Exiting HandleMessageAsync early: Message is from a bot or not a text channel.");
                return;
            }

            _triggerManager.RecordMessage(message);

            var channel = (SocketTextChannel)message.Channel;
            bool directedAtBot = message.MentionedUsers.Any(user => user.Id == client.CurrentUser.Id);

            if (IsAlreadyUwUified(message.Content))
            {
                _logger.LogInformation($"Exiting HandleMessageAsync early: Message is already UwUified.");
                return;
            }

            if (!_triggerManager.ShouldTrigger(message))
            {
                _logger.LogInformation($"Exiting HandleMessageAsync early: Conditions for triggering are not met.");
                return;
            }

            _triggerManager.RecordTrigger();

            // Pre-Typing Messages (introduce anticipation)
            _logger.LogInformation(
                $"Simulating typing for channel: {channel.Id}, directed at bot: {directedAtBot}, message length: {message.Content.Length}");
            await SimulateTyping(channel, directedAtBot, message.Content.Length);
            _logger.LogInformation($"Completed typing simulation for channel: {channel.Id}");

            _logger.LogInformation(
                $"Transforming message content to UwUified version. Original content: \"{message.Content}\"");
            string uwuifiedMessage = UwUify(message.Content, directedAtBot);
            _logger.LogInformation($"UwUified message content: \"{uwuifiedMessage}\"");

            // Optionally add extra message flair
            if (!directedAtBot && _random.NextDouble() < 0.1)
            {
                uwuifiedMessage = $"nyaaa~ {message.Author.Username}-san, I’m hewe 4 nya!! >w< *huggies* 💕" +
                                  Environment.NewLine + uwuifiedMessage;
                _logger.LogInformation($"Extra flair added to UwUified message for {message.Author.Username}.");
            }

            await channel.SendMessageAsync(uwuifiedMessage, messageReference: new MessageReference(message.Id));
            _logger.LogInformation(
                $"Sending UwUified message to channel: {channel.Id}, original message ID: {message.Id}");
            _logger.LogInformation($"UwUified message content: \"{uwuifiedMessage}\"");
            _logger.LogInformation($"Finishing HandleMessageAsync for message ID: {message.Id}");
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
            _logger.LogInformation(
                $"Starting UwUify method with input: \"{input}\" and directedAtBot: {directedAtBot}.");
            if (IsAlreadyUwUified(input))
            {
                _logger.LogInformation($"Exiting UwUify early: Message is already UwUified.");
                return input;
            }

            string uwuified = ReplaceWordsWithKawaiiPhrases(input);
            uwuified = AddSeasonalFlair(uwuified);
            uwuified = AddMoodEnhancements(uwuified);
            uwuified = AddRelevantEmojis(uwuified);

            uwuified = Regex.Replace(uwuified, "[rl]", "w");
            uwuified = Regex.Replace(uwuified, "[RL]", "W");

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
                featurePool.Add(AddThemeBasedWords);

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

            }
            _logger.LogInformation($"Final UwUified message: \"{uwuified}\"");
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
                input = Regex.Replace(input, $"\b{pair.Key}\b", $"{pair.Key} {pair.Value}",
                    RegexOptions.IgnoreCase);
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
            // Add different emotional categories
            if (Regex.IsMatch(input, @"\b(lonely|alone|isolated|by myself)\b", RegexOptions.IgnoreCase))
            {
                input += Environment.NewLine + "Nyaa~ yuu awe neber awone, OwO UwU bot iz hewe wif you~!! 💕 *cuddles*";
            }

            if (Regex.IsMatch(input, @"\b(anxious|worry|stressed|panic|pressure)\b", RegexOptions.IgnoreCase))
            {
                input += Environment.NewLine + "*deep bweafs* nya~ donchu wowwy!! UwU bot iz hewe to hewp >w<!! 🍵";
            }

            if (Regex.IsMatch(input, @"\b(angry|mad|frustrated|furious|annoyed)\b", RegexOptions.IgnoreCase))
            {
                input += Environment.NewLine +
                         "*pats head* uwu awe yuu anngwyyy~? Saiyan powaa 🤜!! bweak fwustwation wif UwU fight!!";
            }

            // Emotional enhancements based on message sentiment
            if (Regex.IsMatch(input, @"\b(sad|cry|upset|bad|mad|angry|frustrated)\b", RegexOptions.IgnoreCase))
            {
                input += Environment.NewLine + "qwq *huggies* pwease dun be sad... (✿´‿`) *hands cookie* 🍪.";
            }

            if (Regex.IsMatch(input, @"\b(happy|yay|excited|awesome|great|good news)\b", RegexOptions.IgnoreCase))
            {
                input += Environment.NewLine + "OwO!! Dat'z so nyice nya~! (✧∀✧)/ ♥";
            }

            if (Regex.IsMatch(input, @"\b(blush|shy|nervous)\b", RegexOptions.IgnoreCase))
            {
                input += Environment.NewLine + "uwu oh nyoo... I-I'm a bit bwushy >///< ♥";
            }

            if (Regex.IsMatch(input, @"\b(bot|help|question|problem|issue)\b", RegexOptions.IgnoreCase))
            {
                input += Environment.NewLine + "I’m hewe to h-help! nyaa~ nyaaa~... 🛠️ wat can I do fow you?? 🤔";
            }

            // Add life event-specific responses
            if (Regex.IsMatch(input, @"\b(birthday|anniversary)\b", RegexOptions.IgnoreCase))
            {
                input += Environment.NewLine + "✨ 🎂 *happy UwU bwirthday* nya~~!! Make a wish >///<!!";
            }

            if (Regex.IsMatch(input, @"\b(holiday|vacation)\b", RegexOptions.IgnoreCase))
            {
                input += Environment.NewLine + "Nyaa~ wish yuu da purrfect meowwidays UwU!!! 🎄☃️";
            }

            // Add responses based on message length
            if (input.Length > 500 && _random.NextDouble() < 0.1)
            {
                input += Environment.NewLine +
                         "UwU dat’s a wot of wowds nya~!! 😺 *twies 2 wead awe of it weawwy fast!*";
            }
            else if (input.Length < 10 && _random.NextDouble() < 0.1)
            {
                input += Environment.NewLine + "uwu such a sh- *ahhem*... concise meowssage 🐾 owo.";
            }

            // Add emojis based on detected keywords
            Dictionary<string, string> emojiMap = new()
            {
                { "sad", "💔 qwq" },
                { "happy", "✨🌟 owo~!" },
                { "excited", "🎉 OwO!!" },
                { "blush", ">///< ♥" },
                { "anxiety", "😰 nyaa~!" }
            };
            foreach (var pair in emojiMap)
            {
                if (Regex.IsMatch(input, $@"\b{pair.Key}\b", RegexOptions.IgnoreCase))
                {
                    input += $" {pair.Value}";
                    break;
                }
            }

            // Time-based responses
            var currentTime = DateTime.Now;
            if (Regex.IsMatch(input, @"\b(sad|cry)\b", RegexOptions.IgnoreCase) && currentTime.Hour >= 20)
            {
                input += " uwuwu... s-sweet dweams... I pwomise da dawn wiww bwing bettew feews >///< 🌙";
            }

            if (currentTime.Hour % 12 == 0 && currentTime.Minute == 0)
            {
                input += Environment.NewLine + "Hewwooo! UwU dis is a time-special magical weply OwO ♥!";
            }

            // Randomized flair
            var randomEnhancements = new[]
            {
                "OwO nya~ don't wowwy!!",
                "🐾 yuu haz kuma UwU bot hewp paws!!",
                "*huggies!*",
                "UwU bewy pwoudd ob yuu nya! =^.UwU.^="
            };
            if (_random.NextDouble() < 0.1)
            {
                input += $" {randomEnhancements[_random.Next(randomEnhancements.Length)]}";
            }

            return input;
        }

        private string AddSeasonalFlair(string input)
        {
            var currentMonth = DateTime.Now.Month;
            var currentDay = DateTime.Now.Day;

            // Example: Valentine's Day (February 14th)
            if (currentMonth == 2 && currentDay == 14)
            {
                input = "💕💌 Hewwo and Happy Wuv Day nya~ my uwuwu fewwoww!! 💖💋 uwu~"
                        + Environment.NewLine + input;
            }
            // Christmas Season
            else if (currentMonth == 12)
            {
                input = "🎅 Nyaa~ it’z Chwistmas UwU!! ❄❄ uwuu~" + Environment.NewLine + input;
            }
            // Halloween Season
            else if (currentMonth == 10)
            {
                input = "🎃 Spooky uwu!! hewwo fwom da UwU bot nya~ boo~" + Environment.NewLine + input;
            }
            // New Year
            else if (currentMonth == 1 && currentDay == 1)
            {
                input = "🎉✨ HAPPIE NEW YEAR OwO!!! 🥳💖 let's make it PAWSOME nya~ UwU!!!"
                        + Environment.NewLine + input;
            }

            // Default case, no seasonal flair.
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
            return Regex.IsMatch(input, @"[wW]uv|[oO]wo") || input.Contains("-w");
        }

        private void TrackFeature(string featureKey)
        {
            _recentFeatures.Add(featureKey);
            if (_recentFeatures.Count > 5)
                _recentFeatures.Remove(_recentFeatures.First());
        }

        private string AddThemeBasedWords(string input)
        {
            string[] bookResponses = { "nyyaa~ do u wike weading stowies?? UwU ♥", "Me wuv books too~ nya nya 📖!" };
            string[] gameResponses = { "OwO!! Gamers rise up nyaaa~!!! *pew pew*", "UwU I wanna pway too nyaaa! 🎮" };
            string[] catResponses = { "Nyaa~ *purrs* UwU nya!! 😺", "Nyaa nya~~ meta cat-herd UwU-chan!!" };
            string[] dogResponses = { "Woof woof!! 🐕 UwU doggos are kawaiii~!! ♥♥!!", "*wiggles tail* woof woof OwO" };
            string[] foodResponses = { "OwO!! I wuv fwood UwU nyaaa 🍩!", "Gimme some yummie snacks >///<!!" };
            string[] musicResponses =
                { "Wut song u listening OwO 🎵??", "UwU bot haz neber seen melodyz 🙁 bit feels 4 music" };

            if (input.Contains("book", StringComparison.OrdinalIgnoreCase))
            {
                input += Environment.NewLine + bookResponses[_random.Next(bookResponses.Length)];
            }
            else if (input.Contains("game", StringComparison.OrdinalIgnoreCase))
            {
                input += Environment.NewLine + gameResponses[_random.Next(gameResponses.Length)];
            }
            else if (input.Contains("cat", StringComparison.OrdinalIgnoreCase))
            {
                input += Environment.NewLine + catResponses[_random.Next(catResponses.Length)];
            }
            else if (input.Contains("dog", StringComparison.OrdinalIgnoreCase))
            {
                input += Environment.NewLine + dogResponses[_random.Next(dogResponses.Length)];
            }
            else if (input.Contains("food", StringComparison.OrdinalIgnoreCase))
            {
                input += Environment.NewLine + foodResponses[_random.Next(foodResponses.Length)];
            }
            else if (input.Contains("music", StringComparison.OrdinalIgnoreCase))
            {
                input += Environment.NewLine + musicResponses[_random.Next(musicResponses.Length)];
            }

            if (DateTime.Now.Hour % 12 == 0 && DateTime.Now.Minute == 0) // Example: Time-Based Enhancement
            {
                input += Environment.NewLine + "Hewwooo! UwU dis is a time-special magical weply OwO ♥!";
            }

            return input;
        }
    }
}