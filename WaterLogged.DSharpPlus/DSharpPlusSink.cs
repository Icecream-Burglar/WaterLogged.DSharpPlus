using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using WaterLogged.Serialization.StringConversion;
using WaterLogged.Templating;

namespace WaterLogged.DSharpPlus.cs
{
    /// <summary>
    /// Handles outputting structured log messages to specific channels based on each message's tag.
    /// </summary>
    /// <inheritdoc />
    public class DSharpPlusSink : TemplatedMessageSink
    {
        /// <summary>
        /// Gets or sets the Discord Client object from which channels shall be resolved.
        /// </summary>
        public DiscordClient DiscordClient { get; set; }
        /// <summary>
        /// Gets or sets a dictionary mapping tags to output to specific channels.
        /// Add an item with an empty string as its key to send all messages that
        /// item's associated channel IDs.
        /// </summary>
        public Dictionary<string, ulong[]> OutputChannels { get; private set; }

        /// <summary>
        /// Instantiates a new instance of the DSharpPlusSink class.
        /// </summary>
        public DSharpPlusSink()
        {
            OutputChannels = new Dictionary<string, ulong[]>();
        }

        public override void ProcessMessage(StructuredMessage message, string tag)
        {
            if (DiscordClient == null || OutputChannels.Count == 0)
            {
                return;
            }
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();

            if (!string.IsNullOrWhiteSpace(message.EntryName))
            {
                builder.WithAuthor(message.EntryName);
            }
            builder.Timestamp = message.CreationDate;
            builder.Title = message.Template;
            builder.Description = TemplateProcessor.ProcessMessage(message);

            foreach (var messageValue in message.Values)
            {
                builder.AddField(messageValue.Key.Id.ToString(), new StringConverter().Convert(messageValue.Value.Value), true);
            }
            foreach (var contextValue in message.ContextValues)
            {
                builder.AddField($"[CONTEXT] {contextValue.Key}", new StringConverter().Convert(contextValue.Value), true);
            }
            
            foreach (var item in OutputChannels)
            {
                if (item.Key != tag && !string.IsNullOrEmpty(item.Key))
                {
                    continue;
                }
                foreach (var channelId in item.Value)
                {
                    var channel = DiscordClient.GetChannelAsync(channelId);
                    channel.Wait();
                    channel.Result.SendMessageAsync(embed: builder.Build());
                }
            }
        }
    }
}
