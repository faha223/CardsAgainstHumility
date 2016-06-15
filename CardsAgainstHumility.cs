using System;
using System.Collections.Generic;
using System.Text;

using Android.App;
using System.Net.Http;
using System.Json;
using CardsAgainstHumility.GameClasses;
using System.Threading.Tasks;
using SocketIO.Client;
using Newtonsoft.Json;
using CardsAgainstHumility.Events;
using CardsAgainstHumility.SocketComm;
using System.Linq;

namespace CardsAgainstHumility
{
    public enum Method
    {
        GET, POST
    }

    class CardsAgainstHumility
    {
        private static Random rand = new Random();

        private static DateTime epoch
        {
            get
            {
                return new DateTime(1970, 1, 1, 0, 0, 0);
            }
        }

        private static Socket GetSocket(string path, IO.Options options = null)
        {
            Socket sock;
            if (options == null)
                sock = IO.Socket(path);
            else
                sock = IO.Socket(path, options);
            return sock;
        }

        private static Socket _gameSocket;
        private static Socket GameSocket
        {
            get
            {
                return _gameSocket;
            }
            set
            {
                _gameSocket = value;
            }
        }

        private static Socket _lobbySocket;
        private static Socket LobbySocket
        {
            get
            {
                return _lobbySocket;
            }
            set
            {
                _lobbySocket = value;
            }
        }

        #region Properties
        private static string _playerId { get; set; }

        public static string PlayerId
        {
            get
            {
                if (_playerId == null)
                    _playerId = NewId();
                return _playerId;
            }
        }

        public static string GameId { get; private set; }

        public static string PlayerName { get; set; }

        public static string Host { get; set; }

        public static List<WhiteCard> PlayerHand { get; set; }

        public static BlackCard CurrentQuestion { get; set; }

        public static bool IsCardCzar { get; private set; }

        #endregion

        #region Helper Functions
        public static string NewId()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 42; i++)
            {
                switch (i)
                {
                    case 10:
                    case 16:
                    case 22:
                    case 27:
                        sb.Append('-');
                        break;
                    default:
                        sb.Append(rand.Next() % 10);
                        break;
                }
            }
            return sb.ToString();
        }
        #endregion

        #region Api
        public static async Task<List<GameInstance>> ListAsync()
        {
            JsonValue value = await JsonRequestAsync(Method.GET, "list", null).ConfigureAwait(false);
            var instances = new List<GameInstance>();
            foreach (JsonObject item in value)
            {
                if (item.ContainsKey("id"))
                {
                    instances.Add(new GameInstance
                    {
                        Id = item["id"],
                        Name = item.ContainsKey("name") ? (string)item["name"] : "(no name)",
                        Players = item.ContainsKey("players") ? (int)item["players"] : 0,
                        MaxPlayers = item.ContainsKey("maxPlayers") ? (int)item["maxPlayers"] : 10
                    });
                }
            }
            return instances;
        }

        public static async Task<GameInstance> Add(string id)
        {
            var param = new Dictionary<string, string>();
            param.Add("name", string.Format("{0}'s Game", PlayerName));
            param.Add("id", id);
            JsonValue value = await JsonRequestAsync(Method.POST, "add", param).ConfigureAwait(false);
            return new GameInstance()
            {
                Name = value["name"],
                Id = value["id"],
                Players = value["players"].Count,
                MaxPlayers = value["maxPlayers"]
            };
        }

        public static async Task<GameInstance> JoinGame(string id)
        {
            var param = new Dictionary<string, string>();
            GameId = id;
            param.Add("gameId", id);
            param.Add("playerId", PlayerId);
            param.Add("playerName", PlayerName);

            JsonValue value = await JsonRequestAsync(Method.POST, "joingame", param).ConfigureAwait(false);
            var gi = new GameInstance();

            if (value.ContainsKey("id"))
            {
                gi.Name = (value.ContainsKey("name") ? (string)value["name"] : string.Empty);
                gi.Players = (value.ContainsKey("players") ? value["players"].Count : 0);
                gi.MaxPlayers = (value.ContainsKey("maxPlayers") ? (int)value["maxPlayers"] : 10);
                gi.Id = value["id"];
            }

            UpdateGameState(value);
            ConnectToGame();

            return gi;
        }

        public static async void DepartGame()
        {
            if (GameId != null)
            {
                var parameters = new Dictionary<string, string>();
                parameters.Add("gameId", GameId);
                parameters.Add("playerId", PlayerId);

                await JsonRequestAsync(Method.POST, "departgame", parameters).ConfigureAwait(false);
                GameId = null;
                if(GameSocket != null)
                {
                    var sock = GameSocket;
                    sock.Close();
                    sock.Off(Socket.EventConnect, game_SocketConnected);
                    sock.Off(Socket.EventConnectError, game_SocketConnectError);
                    sock.Off(Socket.EventConnectTimeout, game_SocketConnectTimeout);
                    sock.Off("updateGame", game_UpdateGame);
                    sock.Off("gameError", game_GameError);
                    GameSocket = null;
                }
            }
        }

        public static async void SelectCard(WhiteCard whiteCard)
        {
            if (GameId != null)
            {
                var param = new Dictionary<string, string>();
                param.Add("gameId", GameId);
                param.Add("playerId", PlayerId);
                param.Add("whiteCardId", whiteCard.Id);

                JsonValue value = await JsonRequestAsync(Method.POST, "selectCard", param).ConfigureAwait(false);
                UpdateGameState(value);
            }
        }

        public static async void SelectWinner(string card)
        {
            if (GameId != null)
            {
                var param = new Dictionary<string, string>();
                param.Add("gameId", GameId);
                param.Add("cardId", card);

                JsonValue value = await JsonRequestAsync(Method.POST, "selectWinner", param).ConfigureAwait(false);
                UpdateGameState(value);
            }
        }

        public static async void ReadyForNextRound()
        {
            if (GameId != null)
            {
                var param = new Dictionary<string, string>();
                param.Add("playerId", PlayerId);
                param.Add("gameId", GameId);

                JsonValue value = await JsonRequestAsync(Method.POST, "readyForNextRound", param).ConfigureAwait(false);
                UpdateGameState(value);
            }
        }
        #endregion

        public static void InitDefaultValues(Activity activity)
        {
            // The Host defaults to my home IP address. I'm hosting a server on 16567.
            // This is just a modified NodeJS Against Humanity server updated to Socket.IO 1.4.6
            Host = "http://74.139.199.67:16567/";
            PlayerName = $"Player_{rand.Next()}";

            var settings = activity.GetSharedPreferences("CardsAgainstHumility", Android.Content.FileCreationMode.Private);

            if (settings != null)
            {
                if (settings.Contains("PlayerName"))
                    PlayerName = settings.GetString("PlayerName", PlayerName);
                if (settings.Contains("Host"))
                    Host = settings.GetString("Host", Host);
            }
        }

        private static async Task<JsonValue> JsonRequestAsync(Method method, string route, Dictionary<string, string> parameters)
        {
            string content = null;
            if ((parameters != null) && (parameters.Count > 0))
            {
                var sb = new StringBuilder();
                sb.Append("{");
                foreach (var item in parameters)
                {
                    sb.Append($"\"{item.Key}\":\"{item.Value}\",");
                }
                sb.Remove(sb.Length - 1, 1);
                sb.Append("}");
                content = sb.ToString();
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Host);

                HttpResponseMessage response;
                switch (method)
                {
                    case Method.GET:
                        response = await client.GetAsync(route).ConfigureAwait(false);
                        break;
                    case Method.POST:
                        response = await client.PostAsync(route, new StringContent(content, Encoding.UTF8, "application/json")).ConfigureAwait(false);
                        break;
                    default:
                        return null;
                }
                if (response.IsSuccessStatusCode)
                {
                    return JsonValue.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                }
                return null;
            }
        }

        private static void UpdateGameState(JsonValue value)
        {
            if (PlayerHand == null)
                PlayerHand = new List<WhiteCard>();
            PlayerHand.Clear();
            foreach (JsonValue player in value["players"])
            {
                if (player["id"] == PlayerId)
                {
                    foreach (JsonValue card in player["cards"])
                    {
                        PlayerHand.Add(new WhiteCard((string)card, 20));
                    }
                }
            }
            if (!string.IsNullOrEmpty(value["currentBlackCard"]))
            {
                CurrentQuestion = new BlackCard(value["currentBlackCard"], 20, 1, 0);
            }
            else
                CurrentQuestion = null;
        }

        public static void ConnectToLobby()
        {
            if (LobbySocket == null)
            {
                var sock = GetSocket($"{Host}lobby");
                sock.On(Socket.EventConnect, lobby_SocketConnected);
                sock.On(Socket.EventConnectError, lobby_SocketConnectError);
                sock.On(Socket.EventConnectTimeout, lobby_SocketConnectTimeout);
                sock.On("lobbyJoin", lobby_LobbyJoin);
                sock.On("gameAdded", lobby_GameAdded);
                LobbySocket = sock;

                sock.Connect();
            }
        }

        public static void DisconnectLobby()
        {
            if(LobbySocket != null)
            {
                var sock = LobbySocket;
                sock.Close();
                sock.Off(Socket.EventConnect, lobby_SocketConnected);
                sock.Off(Socket.EventConnectError, lobby_SocketConnectError);
                sock.Off(Socket.EventConnectTimeout, lobby_SocketConnectTimeout);
                sock.Off("lobbyJoin", lobby_LobbyJoin);
                sock.Off("gameAdded", lobby_GameAdded);
                LobbySocket = null;
            }
        }

        private static void ConnectToGame()
        {
            if (GameSocket == null)
            {
                var sock = GetSocket(Host, new IO.Options()
                {
                    Query = $"playerId={PlayerId}"
                });
                sock.On(Socket.EventConnect, game_SocketConnected);
                sock.On(Socket.EventConnectError, game_SocketConnectError);
                sock.On(Socket.EventConnectTimeout, game_SocketConnectTimeout);
                sock.On("updateGame", game_UpdateGame);
                sock.On("gameError", game_GameError);
                GameSocket = sock;

                sock.Connect();
            }
        }

        #region Lobby Event Handlers
        private static void lobby_SocketConnected(object[] obj)
        {
            Lobby_SocketConnected?.Invoke(null, new EventArgs());
            _lobbySocket.Emit("enterLobby");
        }

        public static event EventHandler<EventArgs> Lobby_SocketConnected;

        private static void lobby_SocketConnectError(object[] obj)
        {
            Lobby_SocketConnectError?.Invoke(null, new SocketConnectErrorEventArgs(obj[0].ToString()));
        }

        public static event EventHandler<SocketConnectErrorEventArgs> Lobby_SocketConnectError;

        private static void lobby_SocketConnectTimeout(object[] obj)
        {
            Lobby_SocketConnectTimeout?.Invoke(null, new SocketConnectErrorEventArgs(obj[0].ToString()));
        }

        public static event EventHandler<SocketConnectErrorEventArgs> Lobby_SocketConnectTimeout;

        private static void lobby_LobbyJoin(object[] obj)
        {
            Console.WriteLine(obj[0].ToString());
            Lobby_Join?.Invoke(null, new EventArgs());
        }

        public static event EventHandler<EventArgs> Lobby_Join;

        private static void lobby_GameAdded(object[] obj)
        {
            Console.WriteLine("GamesAdded");
            var games = JsonConvert.DeserializeObject<List<GameInstance>>(obj[0].ToString());
            Lobby_GameAdded?.Invoke(null, new GameAddedEventArgs(games));
        }

        public static event EventHandler<GameAddedEventArgs> Lobby_GameAdded;

        #endregion Lobby Event Handlers

        #region Game Event Handlers
        private static void game_SocketConnected(object[] obj)
        {
            Game_SocketConnected?.Invoke(null, new EventArgs());
            GameSocket.Emit("connectToGame", new connectToGameArgs()
            {
                gameId = GameId,
                playerId = PlayerId,
                playerName = PlayerName
            }.ToString());
        }

        public static event EventHandler<EventArgs> Game_SocketConnected;

        private static void game_SocketConnectError(object[] obj)
        {
            Game_SocketConnectError?.Invoke(null, new SocketConnectErrorEventArgs(obj[0].ToString()));
        }

        public static event EventHandler<SocketConnectErrorEventArgs> Game_SocketConnectError;

        private static void game_SocketConnectTimeout(object[] obj)
        {
            Game_SocketConnectTimeout?.Invoke(null, new SocketConnectErrorEventArgs(obj[0].ToString()));
        }

        public static event EventHandler<SocketConnectErrorEventArgs> Game_SocketConnectTimeout;

        private static void game_UpdateGame(object[] obj)
        {
            var gameState = JsonConvert.DeserializeObject<GameState>(obj[0].ToString());
            var player = gameState.players.SingleOrDefault(p => p.id == PlayerId);
            PlayerHand = new List<WhiteCard>();

            if (player != null)
            {
                foreach (var card in player.cards)
                {
                    PlayerHand.Add(new WhiteCard(card, 20));
                }
                IsCardCzar = player.isCzar;
            }

            if (!string.IsNullOrWhiteSpace(gameState.currentBlackCard))
                CurrentQuestion = new BlackCard(gameState.currentBlackCard, 20, 1, 0);
            else
                CurrentQuestion = null;

            Game_Update?.Invoke(null, new GameUpdateEventArgs(gameState));
        }

        public static event EventHandler<GameUpdateEventArgs> Game_Update;

        private static void game_GameError(object[] obj)
        {
            Game_Error?.Invoke(null, new EventArgs());
        }

        public static event EventHandler<EventArgs> Game_Error;
        #endregion Game Event Handlers
    }
}