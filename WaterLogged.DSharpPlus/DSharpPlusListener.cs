﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;

namespace WaterLogged.DSharpPlus.cs
{
    /// <summary>
    /// Handles outputting log messages to specific channels based on each message's tag.
    /// </summary>
    /// <inheritdoc />
    public class DSharpPlusListener : Listener
    {
        /// <summary>
        /// Gets or sets the Discord Client object from which channels shall be resolved.
        /// </summary>
        public DiscordClient DiscordClient { get; set; }
        /// <summary>
        /// Gets or sets a dictionary mapping tags to output to specific channels.
        /// Add an item with an empty string as its key to send all messages to that
        /// item's associated array of channel IDs.
        /// </summary>
        public Dictionary<string, ulong[]> OutputChannels { get; private set; }

        /// <summary>
        /// Instantiates a new instance of the DSharpPlusListener class.
        /// </summary>
        public DSharpPlusListener()
        {
            OutputChannels = new Dictionary<string, ulong[]>();
        }

        public override void Write(string value, string tag)
        {
            if (DiscordClient == null || OutputChannels.Count == 0)
            {
                return;
            }
            foreach (var item in OutputChannels)
            {
                if (item.Key != tag && !string.IsNullOrEmpty(item.Key))
                {
                    continue;
                }
                foreach (var channelId in item.Value)
                {
                    Send(value, channelId);
                }
            }
        }

        private async Task Send(string message, ulong channelId)
        {
            var channel = await DiscordClient.GetChannelAsync(channelId);
            await channel.SendMessageAsync(message);
        }
    }
}
