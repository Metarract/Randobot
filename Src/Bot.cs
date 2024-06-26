using Randobot.Models;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Events;
using TwitchLib.Communication.Models;

namespace Randobot;
public sealed class Bot {
  private readonly TwitchClient Client;
  private readonly Config config;
  private bool ClientConfigured = false;

  private ConnectionCredentials? ClientCreds;
  public delegate void CheckCredValidityHandler ();
  public event CheckCredValidityHandler? CheckCredValidity;

  public Bot (Config appConfig) {
    config = appConfig;
    Client = GetClient();
    Log.Info("Bot Client initialized");
    Log.Info("Awaiting token initialization");
  }

  #region bot setup / configuration
  private static TwitchClient GetClient () {
    ClientOptions clientOptions = new() {
      MessagesAllowedInPeriod = 20,
      ThrottlingPeriod = TimeSpan.FromSeconds(30)
    };

    WebSocketClient customClient = new(clientOptions);
    TwitchClient newClient = new(customClient);

    return newClient;
  }

  public void SetClientCredentials (string newToken) {
    ClientCreds = new(config.ClientConfig.BotUsername, newToken);
    if (!ClientConfigured) ConfigureBotClient();
    if (!Client.IsConnected) {
      try {
        Log.Info("Setting creds and attempting to connect Bot Client...");
        Client.SetConnectionCredentials(ClientCreds);
        Client.Connect();
      } catch (Exception ex) {
        Log.Error("Ran into issues while attempting to reconnect, you may need to restart");
        Log.Error(ex.Message);
      }
    } else {
      Log.Info("Received new credential authorization, disconnecting temporarily to reup credentials...");
      Client.Disconnect();
    }
  }

  private void ConfigureBotClient () {
    Client.AddChatCommandIdentifier(config.CommandConfig.CommandCharacter);
    Client.OnLog += OnClientLog;
    Client.OnConnected += OnClientConnected;
    Client.OnDisconnected += OnClientDisconnected;
    Client.OnConnectionError += OnClientConnectionError;
    Client.OnJoinedChannel += OnClientJoined;
    Client.OnChatCommandReceived += OnClientCommandReceived;

    Client.Initialize(ClientCreds);

    ClientConfigured = true;
    Log.Info("Bot configuration completed");
  }

  private void JoinChannels () => config.ClientConfig.TwitchChannels.ForEach(channel => Client.JoinChannel(channel));
  #endregion

  #region bot client event handlers
  private void OnClientLog (object? sender, OnLogArgs e) {
    Log.Info($"[IRC][{e.BotUsername}] - {e.Data}");
  }

  private void OnClientConnected (object? sender, OnConnectedArgs e) {
    Log.Info("Bot Client Connected");
    JoinChannels();
  }

  private void OnClientDisconnected (object? sender, OnDisconnectedEventArgs e) {
    Log.Info("Bot Client Disconnected");
    CheckCredValidity?.Invoke();
  }

  private void OnClientConnectionError (object? sender, OnConnectionErrorArgs e) {
    Log.Info("Ran into connection issues, checking credential validity...");
    CheckCredValidity?.Invoke();
  }

  private void OnClientJoined (object? sender, OnJoinedChannelArgs e) {
    Log.Info($"Bot joined channel: {e.Channel}");
  }

  private async void OnClientCommandReceived (object? sender, OnChatCommandReceivedArgs e) {
    try {
      var response = await MessageHandler.GetCommandResponse(e.Command, config.CommandConfig);
      if (response != null) Client.SendMessage(e.Command.ChatMessage.Channel, response);
    } catch (Exception err) {
      Log.Error("Error received while processing commands");
      Log.Error(err.Message);
      if (err.StackTrace is not null) Log.Error($"Trace:\r\n{err.StackTrace}");
    }
  }
  #endregion
}

