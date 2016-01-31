﻿using IrcDotNet;
using IrcDotNet.Ctcp;
using Jackett.Irc.Models;
using Jackett.Irc.Models.Command;
using Jackett.Irc.Models.DTO;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Jackett2.Core;
using Jackett2.Core.Services;

namespace Jackett2.Irc.Services
{
    public interface IIRCService
    {
        List<NetworkDTO> GetSummary();
        List<Message> GetMessages(string network, string channel);
        List<User> GetUser(string network, string channel);
        void ProcessCommand(string networkId, string channelId, string command);
        void CreateNetworkFromProfile(IRCProfile profile);
    }

    public class IRCService : IIRCService, INotificationHandler<AddProfileEvent>
    {
        List<Network> networks = new List<Network>();
        IIDService idService = null;
        IMediator mediator;
        ILogger<IRCService> logger;

        public IRCService(IIDService i, IMediator m, ILogger<IRCService> logger)
        {
            idService = i;
            mediator = m;
            this.logger = logger;
        }

        public List<Message> GetMessages(string networkId, string channelId)
        {
            var network = networks.Where(n => n.Id == networkId).FirstOrDefault();
            if (network == null)
                return new List<Message>();
            if (string.IsNullOrEmpty(channelId))
            {
                return network.Messages.TakeLast(100).ToList();
            }

            var channel = network.Channels.Where(c => c.Id == channelId).FirstOrDefault();
            return channel.Messages.TakeLast(100).ToList();
        }

        public List<User> GetUser(string networkId, string channelId)
        {
            var network = networks.Where(n => n.Id == networkId).FirstOrDefault();
            if (network == null)
                return new List<User>();
            if (channelId == "server")
            {
                // Network room
                return new List<User>()
                {
                    new User()
                    {
                        Nickname = network.Client.LocalUser.NickName
                    }
                };
            }


            var channel = network.Channels.Where(c => c.Id == channelId).FirstOrDefault();
            if (channel == null)
                return new List<User>();
            var channelObj = network.Client.Channels.Where(c => c.Name == channel.Name).FirstOrDefault();
            if (channelObj == null)
                return new List<User>();
            return channelObj.Users.Select(u => new User() { Nickname = u.User.NickName }).ToList();
        }

        public void ProcessCommand(string networkId, string channelId, string command)
        {
            var network = networks.Where(n => n.Id == networkId).FirstOrDefault();
            Channel channel = null;
            if (null != network)
            {
                channel = network.Channels.Where(c => c.Id == channelId).FirstOrDefault();
            }

            if (string.IsNullOrWhiteSpace(command))
                return;

            if (command.StartsWith("/join"))
            {
                network.Client.Channels.Join(command.Split(" ".ToArray())[1]);
            }
            else if (command == "/leave" && channel != null)
            {
                network.Client.Channels.Leave(channel.Name);
            }
            else
            {
                if (channel == null)
                {
                    network.Client.SendRawMessage(command);
                }
                else
                {
                    network.Client.LocalUser.SendMessage(channel.Name, command);
                    channel.Messages.Add(new Message()
                    {
                        From = network.Client.LocalUser.NickName,
                        Text = command,
                        Type = MessageType.Message
                    });
                    NotifyChannelChange(channel);
                }
            }
        }

        private void NotifyChannelChange(Channel c)
        {
            mediator.Publish(new IRCMessageEvent() { Channel = c.Id });
        }

        private void NotifyNetworkChange(Network n)
        {
            mediator.Publish(new IRCMessageEvent() { Channel = n.Id });
        }

        public List<NetworkDTO> GetSummary()
        {
            var list = new List<NetworkDTO>();
            foreach (var network in networks)
            {
                var d = new NetworkDTO();
                d.Name = network.Name;
                d.Id = network.Id;
                foreach (var c in network.Channels)
                {
                    d.Channels.Add(new ChannelDTO()
                    {
                        Name = c.Name,
                        Id = c.Id
                    });
                }
                list.Add(d);
            }

            return list;
        }

        public void Start()
        {


            /*  foreach(var network in networks)
               {
                   SetupNetwork(network);
                   Connect(network);
               }*/
        }

        private void SetupNetwork(Network network)
        {
            var client = network.Client = new StandardIrcClient();
            client.FloodPreventer = new IrcStandardFloodPreventer(4, 2000);
            client.Registered += Client_Registered;
            client.Disconnected += Client_Disconnected;
            client.ClientInfoReceived += Client_ClientInfoReceived;
            client.Error += Client_Error;
            client.ErrorMessageReceived += Client_ErrorMessageReceived;
            client.MotdReceived += Client_MotdReceived;
            client.ProtocolError += Client_ProtocolError;
            client.ChannelListReceived += Client_ChannelListReceived;
            client.ConnectFailed += Client_ConnectFailed;
            client.NetworkInformationReceived += Client_NetworkInformationReceived;
            client.PingReceived += Client_PingReceived;
            client.PongReceived += Client_PongReceived;
            client.ServerBounce += Client_ServerBounce;
            client.ServerStatsReceived += Client_ServerStatsReceived;
            client.ServerTimeReceived += Client_ServerTimeReceived;
            client.ServerSupportedFeaturesReceived += Client_ServerSupportedFeaturesReceived;
            client.ServerVersionInfoReceived += Client_ServerVersionInfoReceived;
            client.WhoIsReplyReceived += Client_WhoIsReplyReceived;
            client.WhoReplyReceived += Client_WhoReplyReceived;
            client.WhoWasReplyReceived += Client_WhoWasReplyReceived;

            var ctcpClient = new CtcpClient(client);
            ctcpClient.ClientVersion = "Jackett2";// + Engine.ConfigService.GetVersion();
            ctcpClient.PingResponseReceived += CtcpClient_PingResponseReceived;
            ctcpClient.VersionResponseReceived += CtcpClient_VersionResponseReceived;
            ctcpClient.TimeResponseReceived += CtcpClient_TimeResponseReceived;
            ctcpClient.ActionReceived += CtcpClient_ActionReceived;

            networks.Add(network);
        }

        private void Client_Registered(object sender, EventArgs e)
        {
            var client = (IrcClient)sender;

            client.LocalUser.NoticeReceived += LocalUser_NoticeReceived;
            client.LocalUser.MessageReceived += LocalUser_MessageReceived;
            client.LocalUser.JoinedChannel += LocalUser_JoinedChannel;
            client.LocalUser.LeftChannel += LocalUser_LeftChannel;
            client.LocalUser.InviteReceived += LocalUser_InviteReceived;
            client.LocalUser.IsAwayChanged += LocalUser_IsAwayChanged;
            client.LocalUser.ModesChanged += LocalUser_ModesChanged;
            client.LocalUser.NickNameChanged += LocalUser_NickNameChanged;
            mediator.Publish(new IRCStateChangedEvent());
        }

        private void LocalUser_JoinedChannel(object sender, IrcChannelEventArgs e)
        {
            var localUser = (IrcLocalUser)sender;
            var details = GetChannelFromEvent(e);
            details.Channel.Joined = true;

            e.Channel.UserJoined += Channel_UserJoined;
            e.Channel.MessageReceived += Channel_MessageReceived;
            e.Channel.ModesChanged += Channel_ModesChanged;
            e.Channel.NoticeReceived += Channel_NoticeReceived;
            e.Channel.TopicChanged += Channel_TopicChanged;
            e.Channel.UserInvited += Channel_UserInvited;
            e.Channel.UserKicked += Channel_UserKicked;
            e.Channel.UserLeft += Channel_UserLeft;
            e.Channel.UsersListReceived += Channel_UsersListReceived;

            details.Network.Channels.Add(details.Channel);
            mediator.Publish(new IRCStateChangedEvent());
        }

        private Network NetworkFromIrcClient(StandardIrcClient client)
        {
            return networks.Where(n => n.Client.ToString() == client.ToString()).First();
        }

        private Network NetworkFromIrcClient(IrcLocalUser user)
        {
            return NetworkFromIrcClient((StandardIrcClient)user.Client);
        }

        private void BroadcastMessageToNetwork(Network network, Message message)
        {
            network.Messages.Add(message);
            NotifyNetworkChange(network);
            foreach (var channel in network.Channels)
            {
                channel.Messages.Add(message);
                NotifyChannelChange(channel);
            }
        }

        private void CtcpClient_ActionReceived(object sender, CtcpMessageEventArgs e)
        {
            var network = NetworkFromIrcClient(sender as StandardIrcClient);
            BroadcastMessageToNetwork(network, new Message()
            {
                From = e.Source.NickName,
                Text = e.Text,
                Type = MessageType.CTCP
            });
            NotifyNetworkChange(network);
        }

        private void CtcpClient_TimeResponseReceived(object sender, CtcpTimeResponseReceivedEventArgs e)
        {
            var network = NetworkFromIrcClient(sender as StandardIrcClient);
            BroadcastMessageToNetwork(network, new Message()
            {
                From = e.User.NickName,
                Text = $"Time received: {e.DateTime}",
                Type = MessageType.CTCP
            });
            NotifyNetworkChange(network);
        }

        private void CtcpClient_VersionResponseReceived(object sender, CtcpVersionResponseReceivedEventArgs e)
        {
            var network = NetworkFromIrcClient(sender as StandardIrcClient);
            BroadcastMessageToNetwork(network, new Message()
            {
                From = e.User.NickName,
                Text = $"Version received: {e.VersionInfo}",
                Type = MessageType.CTCP
            });
            NotifyNetworkChange(network);
        }

        private void CtcpClient_PingResponseReceived(object sender, CtcpPingResponseReceivedEventArgs e)
        {
            var network = NetworkFromIrcClient(sender as StandardIrcClient);
            network.Messages.Add(new Message()
            {
                From = e.User.NickName,
                Text = $"Ping time: {e.PingTime}",
                Type = MessageType.CTCP
            });
            NotifyNetworkChange(network);
        }

        private void Client_WhoWasReplyReceived(object sender, IrcUserEventArgs e)
        {
            var network = NetworkFromIrcClient(sender as StandardIrcClient);
            BroadcastMessageToNetwork(network, new Message()
            {
                From = e.User.NickName,
                Text = $"Who was: {e.User.NickName}",
                Type = MessageType.System
            });
            NotifyNetworkChange(network);
        }

        private void Client_WhoReplyReceived(object sender, IrcNameEventArgs e)
        {
            var network = NetworkFromIrcClient(sender as StandardIrcClient);
            BroadcastMessageToNetwork(network, new Message()
            {
                From = network.Client.ServerName,
                Text = $"Who was: {e.Name}",
                Type = MessageType.System
            });
            NotifyNetworkChange(network);
        }

        private void Client_WhoIsReplyReceived(object sender, IrcUserEventArgs e)
        {
            var network = NetworkFromIrcClient(sender as StandardIrcClient);
            BroadcastMessageToNetwork(network, new Message()
            {
                From = network.Client.ServerName,
                Text = $"Who is: {e.User.NickName}",
                Type = MessageType.System
            });
            NotifyNetworkChange(network);
        }

        private void Client_ServerVersionInfoReceived(object sender, IrcServerVersionInfoEventArgs e)
        {
            var network = NetworkFromIrcClient(sender as StandardIrcClient);
            network.Messages.Add(new Message()
            {
                From = e.ServerName,
                Text = $"Server version: {e.Version}",
                Type = MessageType.System
            });
            NotifyNetworkChange(network);
        }

        private void Client_ServerSupportedFeaturesReceived(object sender, EventArgs e)
        {
            var network = NetworkFromIrcClient(sender as StandardIrcClient);
            var msg = string.Join(", ", network.Client.ServerSupportedFeatures.Select(f => f.Key + "=" + f.Value));

            network.Messages.Add(new Message()
            {
                From = network.Client.ServerName,
                Text = "Server features: " + msg,
                Type = MessageType.System
            });
            NotifyNetworkChange(network);
        }

        private void Client_ServerTimeReceived(object sender, IrcServerTimeEventArgs e)
        {
            var network = NetworkFromIrcClient(sender as StandardIrcClient);
            network.Messages.Add(new Message()
            {
                From = e.ServerName,
                Text = $"Server time: {e.DateTime}",
                Type = MessageType.System
            });
            NotifyNetworkChange(network);
        }

        private void Client_ServerStatsReceived(object sender, IrcServerStatsReceivedEventArgs e)
        {
            var network = NetworkFromIrcClient(sender as StandardIrcClient);
            foreach (var entry in e.Entries)
            {
                network.Messages.Add(new Message()
                {
                    From = network.Client.ServerName,
                    Text = $"Server stats ({entry.Type}) : { string.Join(", ", entry.Parameters)}",
                    Type = MessageType.System
                });
            }
            NotifyNetworkChange(network);
        }

        private void Client_ServerBounce(object sender, IrcServerInfoEventArgs e)
        {
            var network = NetworkFromIrcClient(sender as StandardIrcClient);
            BroadcastMessageToNetwork(network, new Message()
            {
                From = network.Client.ServerName,
                Text = $"Server is requesting bounce to: {e.Address}:{e.Port}",
                Type = MessageType.System
            });
        }

        private void Client_PongReceived(object sender, IrcPingOrPongReceivedEventArgs e)
        {
            // Ignore
        }

        private void Client_PingReceived(object sender, IrcPingOrPongReceivedEventArgs e)
        {
            // Ignore
        }

        private void Client_NetworkInformationReceived(object sender, IrcCommentEventArgs e)
        {
            var network = NetworkFromIrcClient(sender as StandardIrcClient);
            network.Messages.Add(new Message()
            {
                From = network.Client.ServerName,
                Text = e.Comment,
                Type = MessageType.System
            });
            NotifyNetworkChange(network);
        }

        private void Client_ConnectFailed(object sender, IrcErrorEventArgs e)
        {
            var network = NetworkFromIrcClient(sender as StandardIrcClient);
            BroadcastMessageToNetwork(network, new Message()
            {
                From = network.Name,
                Text = $"Connect failed",
                Type = MessageType.System
            });
        }

        private void Client_ChannelListReceived(object sender, IrcChannelListReceivedEventArgs e)
        {
            var network = NetworkFromIrcClient(sender as StandardIrcClient);
            foreach (var channel in e.Channels)
            {
                BroadcastMessageToNetwork(network, new Message()
                {
                    From = network.Client.ServerName,
                    Text = $"Channel: \"{channel.Name}\" Users: {channel.VisibleUsersCount} Topic: \"{channel.Topic}\"",
                    Type = MessageType.System
                });
            }
        }

        private void Client_ClientInfoReceived(object sender, EventArgs e)
        {
            // Ignore  ServerName/ServerVersion/ServerAvailableUserModes/ServerAvailableChannelModes  is set
        }

        private void Client_ProtocolError(object sender, IrcProtocolErrorEventArgs e)
        {
            var network = NetworkFromIrcClient(sender as StandardIrcClient);
            network.Messages.Add(new Message()
            {
                From = network.Name,
                Text = $"Protocol error ({e.Code}): {e.Message} { string.Join(" ", e.Parameters)}",
                Type = MessageType.System
            });
            NotifyNetworkChange(network);
        }

        private void Client_MotdReceived(object sender, EventArgs e)
        {
            var network = NetworkFromIrcClient(sender as StandardIrcClient);
            network.Messages.Add(new Message()
            {
                From = network.Client.ServerName,
                Text = $"MOTD: {network.Client.MessageOfTheDay}",
                Type = MessageType.System
            });
            NotifyNetworkChange(network);
        }

        private void Client_ErrorMessageReceived(object sender, IrcErrorMessageEventArgs e)
        {
            var network = NetworkFromIrcClient(sender as StandardIrcClient);
            network.Messages.Add(new Message()
            {
                From = network.Client.ServerName,
                Text = $"Network Error: {e.Message}",
                Type = MessageType.System
            });
        }


        private void Client_Error(object sender, IrcErrorEventArgs e)
        {
            var network = NetworkFromIrcClient(sender as StandardIrcClient);
            network.Messages.Add(new Message()
            {
                From = network.Client.ServerName,
                Text = $"Client Error: {e.Error.Message} {e.Error.StackTrace}",
                Type = MessageType.System
            });
        }

        private void Client_Disconnected(object sender, EventArgs e)
        {
            var network = NetworkFromIrcClient(sender as StandardIrcClient);
            BroadcastMessageToNetwork(network, new Message()
            {
                From = network.Client.ServerName,
                Text = "Disconnected",
                Type = MessageType.System
            });

            foreach (var channel in network.Channels)
            {
                channel.Joined = false;
            }
        }

        private void LocalUser_NickNameChanged(object sender, EventArgs e)
        {
            var network = NetworkFromIrcClient(sender as StandardIrcClient);
            BroadcastMessageToNetwork(network, new Message()
            {
                From = network.Client.ServerName,
                Text = $"Nickname changed to {network.Client.LocalUser.NickName}",
                Type = MessageType.System
            });
        }

        private void LocalUser_ModesChanged(object sender, EventArgs e)
        {
            var user = sender as IrcLocalUser;
            var network = NetworkFromIrcClient((StandardIrcClient)user.Client);
            BroadcastMessageToNetwork(network, new Message()
            {
                From = network.Client.ServerName,
                Text = $"Your modes were set to: {String.Join("", network.Client.LocalUser.Modes)}",
                Type = MessageType.System
            });
        }

        private void LocalUser_IsAwayChanged(object sender, EventArgs e)
        {
            var network = NetworkFromIrcClient(sender as StandardIrcClient);
            network.Messages.Add(new Message()
            {
                From = network.Client.ServerName,
                Text = network.Client.LocalUser.IsAway ? "You were marked away" : "You are no longer marked as away",
                Type = MessageType.System
            });
            NotifyNetworkChange(network);
        }

        private void LocalUser_InviteReceived(object sender, IrcChannelInvitationEventArgs e)
        {
            var network = NetworkFromIrcClient(sender as StandardIrcClient);
            BroadcastMessageToNetwork(network, new Message()
            {
                From = e.Inviter.NickName,
                Text = $"You were invited to: {e.Channel} {e.Comment}",
                Type = MessageType.System
            });
        }

        private ChannelInfoResult GetChannelFromEvent(IrcChannelEventArgs e)
        {
            var network = networks.Where(n => n.Client == e.Channel.Client).First();
            var channel = network.Channels.Where(c => c.Name == e.Channel.Name).FirstOrDefault();
            if (channel == null)
            {
                channel = new Channel()
                {
                    Joined = true,
                    Name = e.Channel.Name,
                    Id = idService.NewId()
                };
            }

            return new ChannelInfoResult()
            {
                Network = network,
                Channel = channel
            };
        }

        private ChannelInfoResult GetChannelInfoFromClientChannel(IrcChannel ircchannel)
        {
            var network = networks.Where(n => n.Client == ircchannel.Client).First();
            return new ChannelInfoResult()
            {
                Network = network,
                Channel = network.Channels.Where(c => c.Name == ircchannel.Name).FirstOrDefault()
            };
        }


        private Network GetNetworkFromEvent(IrcChannelEventArgs e)
        {
            return networks.Where(n => n.Client == e.Channel.Client).First();
        }

        private void LocalUser_LeftChannel(object sender, IrcChannelEventArgs e)
        {
            e.Channel.UserJoined -= Channel_UserJoined;
            e.Channel.MessageReceived -= Channel_MessageReceived;
            e.Channel.ModesChanged -= Channel_ModesChanged;
            e.Channel.NoticeReceived -= Channel_NoticeReceived;
            e.Channel.TopicChanged -= Channel_TopicChanged;
            e.Channel.UserInvited -= Channel_UserInvited;
            e.Channel.UserKicked -= Channel_UserKicked;
            e.Channel.UserLeft -= Channel_UserLeft;
            e.Channel.UsersListReceived -= Channel_UsersListReceived;

            var network = GetNetworkFromEvent(e);
            var channel = GetChannelFromEvent(e);
            channel.Channel.Joined = false;
            channel.Channel.Messages.Add(new Message()
            {
                From = network.Client.ServerName,
                Text = "You left this channel",
                Type = MessageType.System
            });
            mediator.Publish(new IRCStateChangedEvent());
        }

        private void Channel_UsersListReceived(object sender, EventArgs e)
        {
            var info = GetChannelInfoFromClientChannel(sender as IrcChannel);
            mediator.Publish(new IRCUsersChangedEvent() { Id = info.Channel.Id });
        }

        private void Channel_UserLeft(object sender, IrcChannelUserEventArgs e)
        {
            var info = GetChannelInfoFromClientChannel(sender as IrcChannel);
            mediator.Publish(new IRCUsersChangedEvent() { Id = info.Channel.Id });
        }

        private void Channel_UserKicked(object sender, IrcChannelUserEventArgs e)
        {
            var info = GetChannelInfoFromClientChannel(sender as IrcChannel);
            info.Channel.Messages.Add(new Message()
            {
                From = info.Network.Client.ServerName,
                Text = $"{e.ChannelUser.User.NickName} was kicked.",
                Type = MessageType.System
            });
            NotifyChannelChange(info.Channel);
            mediator.Publish(new IRCUsersChangedEvent() { Id = info.Channel.Id });
        }

        private void Channel_UserInvited(object sender, IrcUserEventArgs e)
        {
            var info = GetChannelInfoFromClientChannel(sender as IrcChannel);
            info.Channel.Messages.Add(new Message()
            {
                From = info.Network.Client.ServerName,
                Text = $"{e.User.NickName} was invited.",
                Type = MessageType.System
            });
            NotifyChannelChange(info.Channel);
        }

        private void Channel_TopicChanged(object sender, IrcUserEventArgs e)
        {
            var info = GetChannelInfoFromClientChannel(sender as IrcChannel);
            info.Channel.Messages.Add(new Message()
            {
                From = info.Network.Client.ServerName,
                Text = $"Topic: {(sender as IrcChannel).Topic}",
                Type = MessageType.System
            });
            NotifyChannelChange(info.Channel);
        }

        private void Channel_NoticeReceived(object sender, IrcMessageEventArgs e)
        {
            var info = GetChannelInfoFromClientChannel(sender as IrcChannel);
            info.Channel.Messages.Add(new Message()
            {
                From = e.Source.Name,
                Text = e.Text,
                Type = MessageType.Notice
            });
            NotifyChannelChange(info.Channel);
        }

        private void Channel_ModesChanged(object sender, IrcUserEventArgs e)
        {
            var info = GetChannelInfoFromClientChannel(sender as IrcChannel);
            if (info.Channel != null)
            {
                info.Channel.Messages.Add(new Message()
                {
                    From = e.User == null ? string.Empty : e.User.NickName,
                    Text = $"Channel modes changed to: {(sender as IrcChannel).Modes}",
                    Type = MessageType.System
                });
                NotifyChannelChange(info.Channel);
            }
        }

        private void Channel_MessageReceived(object sender, IrcMessageEventArgs e)
        {
            var info = GetChannelInfoFromClientChannel(sender as IrcChannel);
            info.Channel.Messages.Add(new Message()
            {
                From = e.Source.Name,
                Text = e.Text,
                Type = MessageType.Message
            });

            mediator.Publish(new IRCMessageEvent()
            {
                Channel = info.Channel.Id,
                From = e.Source.Name,
                Message = e.Text,
                Network = info.Network.Id,
                Profile = info.Network.Id
            });
            // NotifyChannelChange(info.Channel);
        }

        private void Channel_UserJoined(object sender, IrcChannelUserEventArgs e)
        {
            var info = GetChannelInfoFromClientChannel(sender as IrcChannel);
            info.Channel.Messages.Add(new Message()
            {
                From = info.Network.Client.ServerName,
                Text = $"User joined: {e.ChannelUser.User.NickName}",
                Type = MessageType.System
            });
            NotifyChannelChange(info.Channel);
            mediator.Publish(new IRCUsersChangedEvent() { Id = info.Channel.Id });
        }

        private void LocalUser_MessageReceived(object sender, IrcMessageEventArgs e)
        {
            // TODO PM Implementation
            var network = NetworkFromIrcClient(sender as IrcLocalUser); // Confirmed
            BroadcastMessageToNetwork(network, new Message()
            {
                From = e.Source.Name,
                Text = $"Private message {e.Text}",
                Type = MessageType.Message
            });
        }

        private void LocalUser_NoticeReceived(object sender, IrcMessageEventArgs e)
        {
            var network = NetworkFromIrcClient(sender as IrcLocalUser);
            BroadcastMessageToNetwork(network, new Message()
            {
                From = e.Source.Name,
                Text = e.Text,
                Type = MessageType.Notice
            });
        }

        private void Connect(Network network)
        {
            if (network.Address.Count == 0)
            {
                logger.LogWarning($"Unable to connect to {network.Name} as it has no servers defined!");
                return;
            }

            if (network.AddressIndex + 1 > network.Address.Count)
            {
                network.AddressIndex = 0;
            }

            var address = network.Address[network.AddressIndex];

            // Bump index so we try the next server should we fail to connect
            network.AddressIndex++;

            var port = 6667;
            var addr = string.Empty;
            var split = address.Split(':');
            if (split.Length > 0)
                addr = split[0];
            if (split.Length > 1)
                port = ParseUtil.CoerceInt(split[1]);

            network.Client.Connect(addr, port, network.UseSSL, new IrcUserRegistrationInfo()
            {
                NickName = network.Username,
                UserName = network.Username,
                RealName = network.Username
            });

            BroadcastMessageToNetwork(network, new Message()
            {
                From = network.Name,
                Text = $"Connecting to {address} ..",
                Type = MessageType.System
            });
        }

        void INotificationHandler<AddProfileEvent>.Handle(AddProfileEvent notification)
        {
            CreateNetworkFromProfile(notification.Profile);
        }

        public void CreateNetworkFromProfile(IRCProfile profile)
        {
            var network = networks.Where(n => n.Id == profile.Id).FirstOrDefault();
            if (network == null)
            {
                network = new Network()
                {
                    Id = profile.Id,
                    Name = profile.Name,
                    Address = profile.Servers,
                    Username = profile.Username
                };

                SetupNetwork(network);
                Connect(network);
            }
        }
    }
}
