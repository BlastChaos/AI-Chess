using System.Text.Json;
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
        private readonly NeuralConfig _neuralConfig;
        private readonly NeuralNetwork _neuralNetwork;

        public ChessController(ILogger<ChessController> logger, IHttpClientFactory factory, IOptions<NeuralConfig> gameConfig, NeuralNetwork neuralNetwork)
        {
            _logger = logger;
            _httpClient = factory.CreateClient(nameof(ChessController));
            _neuralConfig = gameConfig.Value;
            _neuralNetwork = neuralNetwork;
        }

        [HttpGet("Download")]
        public async Task<ActionResult<string>> DownloadGames()
        {
            var directory = Path.Combine(Directory.GetCurrentDirectory(),_neuralConfig.GamesOutputDirectory);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            } else {
                Directory.Delete(directory, true);
                Directory.CreateDirectory(directory);
            }
            var gameCount = 0;
            foreach (var username in _neuralConfig.Users){
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
                        if(string.IsNullOrEmpty(pgn) || !ChessBoard.TryLoadFromPgn(pgn, out ChessBoard board)) {
                            _logger.LogWarning("Game {gameUrl} cannot be used", gameUrl);
                            continue;
                        }
                        // Créer un nom de fichier unique pour chaque partie en utilisant la date
                        string date = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        string fileName = Path.Combine(directory, $"{username}-{date}.pgn");

                        // Écrire le PGN dans le fichier
                        await System.IO.File.WriteAllTextAsync(fileName, pgn);
                        gameCount++;
                        if(gameCount == _neuralConfig.NumberData){
                            break;
                        }
                        //_logger.LogInformation("Partie récupérée: {filename}", fileName);
                    }
                    if(gameCount == _neuralConfig.NumberData){
                        break;
                    }
                }

                _logger.LogInformation("Récupération du joueur {username} terminée.", username);
                if(gameCount == _neuralConfig.NumberData){
                    break;
                }
            }
            string[] filePaths = Directory.GetFiles(directory, "*.pgn",
                                            SearchOption.TopDirectoryOnly);
            return this.Ok(filePaths);
        }

        [HttpGet("Train")]
        public async Task<ActionResult> Train(int nbreIterations, CancellationToken stoppingToken)
        {
            var gameInfos = GetGameInfos(); 
            var input = gameInfos.Select(x => x.GetNeuralInput()).ToArray();
            var output = gameInfos.Select(x => new double[]{x.Point}).ToArray();
            gameInfos.Clear();
            var debut = DateTime.Now;
            var loss = await  _neuralNetwork.Train(input,output,nbreIterations, stoppingToken);
            var fin = DateTime.Now;
            _logger.LogInformation("Ai trained with {nbreIterations} iterations in {seconds} seconds. Last loss: {lastLoss}", nbreIterations, (fin-debut).TotalSeconds, loss.Last());
            return this.Ok($"Ai trained with {nbreIterations} iterations in {(fin-debut).TotalSeconds} seconds. Last loss: {loss.Last()}");
        }

        [HttpGet("GetGameInfos")]
        public List<TurnInfo> GetGameInfos()
        {
            List<TurnInfo> turnInfos = new();
            var directory = Path.Combine(Directory.GetCurrentDirectory(), _neuralConfig.GamesOutputDirectory);
            string[] filePaths = Directory.GetFiles(directory, "*.pgn",
                                            SearchOption.TopDirectoryOnly);
            int gameCount = 0;
            foreach (string filePath in filePaths){
                string input =  System.IO.File.ReadAllText(filePath);
                if(!ChessBoard.TryLoadFromPgn(input, out ChessBoard board)){
                    _logger.LogWarning("{filePath} will not be used", filePath);
                    continue;
                }

                board.Headers.TryGetValue("Black", out var blackName);
                var playerBlack  = _neuralConfig.Users.Contains(blackName, StringComparer.OrdinalIgnoreCase);

                var opponentElo = playerBlack ? board.Headers["WhiteElo"] : board.Headers["BlackElo"];
                var playerTurn = playerBlack ? PieceColor.Black : PieceColor.White;

                board.MoveIndex = -1;
                var fenBoard = board.ToFen();
                var newBoard = ChessBoard.LoadFromFen(fenBoard);

                foreach (Move moveMatch in board.ExecutedMoves)
                {
                    if(playerTurn != board.Turn){
                        newBoard.Move(moveMatch);
                        continue;
                    }
                    var pieces = ChessBoardOp.TransformChessBoard(board);
                    // for (int i = 0; i < pieces.Length; i++)
                    // {
                    //     for (int j = 0; j < pieces[i].Length; j++)
                    //     {
                    //         Console.Write(pieces[i][j] + " ");
                    //     }
                    //     Console.WriteLine();
                    // }
                    var moves = newBoard.Moves();
                    for(int i = 0; i < Math.Min(moves.Length, 30); i++){
                        var possibleMove = moves[i];
                        var isPlayerMove = moveMatch.ToString() == possibleMove.ToString();
                        var point = CalculatePoint.CalculatePointOfBoard(newBoard, possibleMove, isPlayerMove);
                        turnInfos.Add(new TurnInfo(){
                            OriginalPositions = pieces,
                            Move = moveMatch,
                            PreviousMoves = newBoard.ExecutedMoves.ToArray(),
                            Point = point,
                            Turn = board.Turn,
                            OpponentElo = double.Parse(opponentElo),
                        });
                        gameCount++;
                        
                    }

                    newBoard.Move(moveMatch);


                    if(gameCount >= _neuralConfig.NumberData){
                        break;
                    }
                }
                if(gameCount >= _neuralConfig.NumberData){
                    break;
                }
            }
            return turnInfos;
        }
    }
}