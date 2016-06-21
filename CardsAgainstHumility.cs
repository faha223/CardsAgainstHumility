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
using CardsAgainstHumility.Helpers;

namespace CardsAgainstHumility
{
    public enum Method
    {
        GET, POST
    }

    public static class CardsAgainstHumility
    {
        private static Random rand = new Random();

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

        public static string GameName { get; private set; }

        public static List<Player> Players { get; private set; }

        public static int MaxPlayers { get; private set; }

        public static int RequiredNumberOfPlayers { get; private set; }

        public static string PlayerName { get; set; }

        public static int AwesomePoints { get; set; }

        public static string SelectedCard { get; set; }

        public static string Host { get; set; }

        public static List<WhiteCard> PlayerHand { get; set; }

        public static List<WhiteCard> PlayedCards { get; set; }

        public static BlackCard CurrentQuestion { get; set; }

        public static bool IsReady { get; private set; }

        public static bool IsCardCzar { get; private set; }

        public static bool GameStarted { get; private set; }

        public static bool ReadyToSelectWinner { get; private set; }

        public static bool ReadyForReview { get; private set; }

        public static string RoundWinner { get; private set; }

        public static string WinningCard { get; private set; }

        public static List<string> PlayersNotYetSubmitted { get; private set; }

        #endregion Properties

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

        private static async Task<JsonValue> JsonRequestAsync(Method method, string route, object param, bool expectResponse = true)
        {
            string content = null;
            if(method == Method.POST && param != null)
            {
                content = JsonConvert.SerializeObject(param);
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri($"{Host}");

                HttpResponseMessage response = null;

                try
                {
                    switch (method)
                    {
                        case Method.GET:
                            response = await client.GetAsync(route).ConfigureAwait(false);
                            break;
                        case Method.POST:
                            if (expectResponse)
                                response = await client.PostAsync(route, new StringContent(content, Encoding.UTF8, "application/json")).ConfigureAwait(false);
                            else
                                client.PostAsync(route, new StringContent(content, Encoding.UTF8, "application/json")).Wait();
                            break;
                        default:
                            return null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An exception occurred: {0}", ex.Message);
                    return null;
                }

                if (response.IsSuccessStatusCode)
                {
                    return JsonValue.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                }
                return null;
            }
        }

        #endregion Helper Functions

        public static void InitDefaultValues(Activity activity)
        {
            // The Host defaults to my home IP address. I'm hosting a server on 16567.
            // This is just a modified NodeJS Against Humanity server updated to Socket.IO 1.4.6
            // found at https://github.com/amirrajan/nodejs-against-humanity
            Host = "https://polar-harbor-54061.herokuapp.com";
            PlayerName = UserInfo.GetUserName(activity);

            var settings = activity.GetSharedPreferences("CardsAgainstHumility", Android.Content.FileCreationMode.Private);

            if (settings != null)
            {
                if (settings.Contains("PlayerName"))
                    PlayerName = settings.GetString("PlayerName", PlayerName);
                if (settings.Contains("Host"))
                    Host = settings.GetString("Host", Host);
            }
        }

        private static void UpdateGameState(GameState gameState)
        {
            var player = gameState.players.SingleOrDefault(p => p.id == PlayerId);
            PlayerHand = new List<WhiteCard>();
            GameStarted = gameState.isStarted;
            GameName = gameState.name;

            if (player != null)
            {
                foreach (var card in player.cards)
                {
                    PlayerHand.Add(new WhiteCard(card, 20));
                }
                IsCardCzar = player.isCzar;
                IsReady = player.isReady;
                AwesomePoints = player.awesomePoints;
                SelectedCard = player.selectedWhiteCardId;
            }

            Players = new List<Player>();
            foreach(var p in gameState.players)
            {
                Players.Add(new Player()
                {
                    Id = p.id,
                    Name = p.name,
                    IsCardCzar = p.isCzar,
                    IsReady = p.isReady,
                    AwesomePoints = p.awesomePoints
                });
            }
            MaxPlayers = gameState.maxPlayers;

            PlayedCards = new List<WhiteCard>();
            if (gameState.isReadyForScoring)
            {
                foreach (var card in gameState.players.Where(c => !string.IsNullOrEmpty(c.selectedWhiteCardId)).Select(c => c.selectedWhiteCardId))
                {
                    PlayedCards.Add(new WhiteCard(card, 20));
                }
            }

            PlayersNotYetSubmitted = gameState.players.Where(c => string.IsNullOrEmpty(c.selectedWhiteCardId) && c.id != _playerId).Select(c => c.name).ToList();

            if (!string.IsNullOrWhiteSpace(gameState.currentBlackCard))
                CurrentQuestion = new BlackCard(gameState.currentBlackCard, 20, 1, 0);
            else
                CurrentQuestion = null;

            RequiredNumberOfPlayers = 3;
            ReadyToSelectWinner = gameState.isReadyForScoring;
            ReadyForReview = gameState.isReadyForReview;
            if(ReadyForReview)
            {
                Console.WriteLine("Round Winner: {0}", gameState.winnerId);
                WinningCard = gameState.winningCardId;
                var winner = gameState.players.SingleOrDefault(c => c.selectedWhiteCardId == WinningCard);
                if (winner != null)
                    RoundWinner = winner.name;
                else
                    RoundWinner = "Forgettable";
            }
            else
            {
                RoundWinner = string.Empty;
                WinningCard = string.Empty;
            }

            Game_Update?.Invoke(null, new GameUpdateEventArgs(gameState));
        }

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

        public static async Task<string> Add(string id)
        {
            return await Add(id, $"{PlayerName}'s Game", 10, 5);
        }

        public static async Task<string> Add(string id, string gameName, int maxPlayers, int pointsToWin)
        {
            var param = new
            {
                name = gameName,
                id = id,
                maxPlayers = maxPlayers,
                pointsToWin = 5
            };
            JsonValue value = await JsonRequestAsync(Method.POST, "add", param).ConfigureAwait(false);

            return JsonConvert.DeserializeObject<GameState>(value.ToString()).id;
        }

        public static async Task JoinGame(string id)
        {
            GameId = id;

            var param = new
            {
                gameId = id,
                playerId = PlayerId,
                playerName = PlayerName
            };

            var value = JsonRequestAsync(Method.POST, "joingame", param).ConfigureAwait(false);

            var gameState = JsonConvert.DeserializeObject<GameState>((await value).ToString());

            ConnectToGame();

            UpdateGameState(gameState);
        }

        public static void DepartGame()
        {
            if (GameId != null)
            {
                var param = new
                {
                    gameId = GameId,
                    playerId = PlayerId
                };

                JsonRequestAsync(Method.POST, "departgame", param, false).ConfigureAwait(false);
                GameId = null;

                DisconnectSocket();
            }
        }

        public static async void SelectCard(WhiteCard whiteCard)
        {
            if (GameId != null)
            {
                SelectedCard = whiteCard.Id;

                var param = new
                {
                    gameId = GameId,
                    playerId = PlayerId,
                    whiteCardId = whiteCard.Id
                };

                await JsonRequestAsync(Method.POST, "selectCard", param).ConfigureAwait(false);
            }
        }

        public static async void SelectWinner(WhiteCard card)
        {
            if (GameId != null)
            {
                var param = new
                {
                    gameId = GameId,
                    cardId = card.Id
                };

                await JsonRequestAsync(Method.POST, "selectWinner", param).ConfigureAwait(false);
            }
        }

        public static async void ReadyForNextRound()
        {
            if (GameId != null)
            {
                var param = new
                {
                    playerId = PlayerId,
                    gameId = GameId
                };

                await JsonRequestAsync(Method.POST, "readyForNextRound", param).ConfigureAwait(false);
            }
        }

        #endregion Api

        #region Socket.IO

        private static Socket Socket;

        private static Socket GetSocket(string path, IO.Options options = null)
        {
            Socket sock;
            if (options == null)
                sock = IO.Socket(path);
            else
                sock = IO.Socket(path, options);
            return sock;
        }

        public static void ConnectToLobby()
        {
            if (Socket != null)
                DisconnectSocket();

            var sock = GetSocket($"{Host}/lobby");
            sock.On(Socket.EventConnect, lobby_SocketConnected);
            sock.On(Socket.EventConnectError, lobby_SocketConnectError);
            sock.On(Socket.EventConnectTimeout, lobby_SocketConnectTimeout);
            sock.On("lobbyJoin", lobby_LobbyJoin);
            sock.On("gameAdded", lobby_GameAdded);
            Socket = sock;
            sock.Connect();
        }

        private static void ConnectToGame()
        {
            if (Socket != null)
                DisconnectSocket();

            var sock = GetSocket($"{Host}/game", new IO.Options()
            {
                Query = $"playerId={PlayerId}"
            });
            sock.On(Socket.EventConnect, game_SocketConnected);
            sock.On(Socket.EventConnectError, game_SocketConnectError);
            sock.On(Socket.EventConnectTimeout, game_SocketConnectTimeout);
            sock.On("updateGame", game_UpdateGame);
            sock.On("gameError", game_GameError);
            Socket = sock;
            sock.Connect();
        }

        public static void DisconnectSocket()
        {
            Socket.Close();
            Socket = null;
        }

        #endregion Socket.IO

        #region Lobby Event Handlers

        private static void lobby_SocketConnected(object[] obj)
        {
            Lobby_SocketConnected?.Invoke(null, new EventArgs());
            Socket.Emit("enterLobby");
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
            Socket.Emit("connectToGame", new connectToGameArgs()
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
            UpdateGameState(gameState);
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