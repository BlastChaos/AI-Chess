using System.Text.Json;
using AI_Chess.Activation;
using Chess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AI_Chess.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChessController : ControllerBase
    {
        private readonly ILogger<ChessController> _logger;
        private readonly HttpClient _httpClient;
        private readonly GameConfig _gameConfig;

        public ChessController(ILogger<ChessController> logger, IHttpClientFactory factory, IOptions<GameConfig> gameConfig)
        {
            _logger = logger;
            _httpClient = factory.CreateClient(nameof(ChessController));
            _gameConfig = gameConfig.Value;
        }

        [HttpGet("Download")]
        public async Task<ActionResult<string>> DownloadGames()
        {
            var directory = Path.Combine(Directory.GetCurrentDirectory(),_gameConfig.GamesOutputDirectory);
            //var dir = new DirectoryInfo(directory);
            //dir.Delete(true);
            foreach (var username in _gameConfig.Users){
                // Obtenir la liste des liens d'archives
                string archivesURL = $"https://api.chess.com/pub/player/{username}/games/archives";
                string archivesResponse = await _httpClient.GetStringAsync(archivesURL);
                var archives = JsonSerializer.Deserialize<ArchivesResponse>(archivesResponse);
                var selectedArchives = new List<string>(archives?.archives);
                foreach (var archive in selectedArchives)
                {
                    // Obtenir les parties de l'archive
                    string gamesResponse = await _httpClient.GetStringAsync(archive);
                    var gamesData = JsonSerializer.Deserialize<GamesResponse>(gamesResponse);
                    var games = gamesData?.games;
                    if(games == null) continue;
                    foreach (var game in games)
                    {

                        string pgn = game.pgn;
                        string gameUrl = game.url;

                        // Créer un nom de fichier unique pour chaque partie en utilisant la date
                        string date = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        string fileName = Path.Combine(directory, $"{username}-{date}.pgn");

                        // Écrire le PGN dans le fichier
                        await System.IO.File.WriteAllTextAsync(fileName, pgn);

                        _logger.LogInformation("Partie récupérée: {filename}", fileName);
                    }
                }

                _logger.LogInformation("Récupération du joueur {username} terminée.", username);
            }
            string[] filePaths = Directory.GetFiles(directory, "*.pgn",
                                            SearchOption.TopDirectoryOnly);
            return this.Ok(filePaths);
        }

        [HttpGet("Test")]
        public ActionResult<List<TurnInfo>> Test(int nbreIterations)
        {
            var result3 = (OkObjectResult)this.GetGameInfo().Result; 
            var result = ((List<TurnInfo>) result3.Value).ToArray();
            var input = result.Select(x => x.Input()).ToArray();
            var output = result.Select(x => new double[]{_gameConfig.SoftmaxPercents}).ToArray();
            List<Node> nodes = new()
            {
                new Node()
                {
                    Activation = new Relu(),
                    NbHiddenNode = input[0].Length
                },
                new Node()
                {
                    Activation = new Sigmoid(),
                    NbHiddenNode = 200
                },
                new Node()
                {
                    Activation = new Softmax(),
                    NbHiddenNode = output[0].Length
                }
            };
            var nn = new NeuralNetwork(input[0].Length,0.0001, nodes.ToArray());
            var debut = DateTime.Now;
            var loss = nn.Train(input,output,nbreIterations);
            var fin = DateTime.Now;
            Console.WriteLine("Dernier Loss generer: " + loss.Last());
            Console.WriteLine("Temps pour le générer: " + (fin-debut));
            Random random = new();
            double[][] inputTest = input.Take(10).ToArray();
            double[][] outputTest = output.Take(10).ToArray();
            var test = nn.Predict(inputTest);
            return this.Ok(inputTest + " " +  outputTest);
        }
    
        [HttpGet("GamesInfo")]
        public ActionResult<List<TurnInfo>> GetGameInfo()
        {
            List<TurnInfo> turnInfos = new();
            var directory = Path.Combine(Directory.GetCurrentDirectory(), _gameConfig.GamesOutputDirectory);
            string[] filePaths = Directory.GetFiles(directory, "*.pgn",
                                            SearchOption.TopDirectoryOnly);
            int gameCount = 0;
            foreach (string filePath in filePaths){
                string input =  System.IO.File.ReadAllText(filePath);
                if(!ChessBoard.TryLoadFromPgn(input, out ChessBoard board)){
                    _logger.LogWarning("{filePath} n'as pas pu être utilisé", filePath);
                    continue;

                }
                

                board.MoveIndex = -1;
                foreach (Move moveMatch in board.ExecutedMoves)
                {
                    int[][] pieces = new int[8][];
                    for(short i = 0; i < 8; i++){
                        pieces[i] = new int[8];
                        for(short j = 0; j< 8; j++) {
                            if(board[i,j] != null)
                            pieces[i][j] = board[i,j].Color.Value == PieceColor.Black.Value ? -board[i,j].Type.Value : board[i,j].Type.Value;
                        }
                    }
                    turnInfos.Add(new TurnInfo(){
                        OriginalPositions = pieces,
                        OriginalPositionX = moveMatch.OriginalPosition.X,
                        OriginalPositionY = moveMatch.OriginalPosition.Y,
                        NewPositionX = moveMatch.NewPosition.X,
                        NewPositionY = moveMatch.NewPosition.Y,
                        Turn = board.Turn.Value
                        //Se référer à PieceColor.Black OU white
                    });
                    board.Next();
                    gameCount++;
                    if(gameCount == _gameConfig.NumberData){
                        break;
                    }
                }
                if(gameCount == _gameConfig.NumberData){
                    break;
                }
            }
            return this.Ok(turnInfos);
        }
    }
}