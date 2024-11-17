



using System.Text.Json;
using AI_Chess;
using Chess;


public class ArchivesResponse
{
    public required string[] archives { get; set; }
}


public class Game
{
    public required string pgn { get; set; }
    public required string url { get; set; }
}

public class GamesResponse
{
    public required Game[] games { get; set; }
}


public partial class Helper
{

    public static async Task DownloadGames(NeuralConfig neuralConfig, HttpClient httpClient, ILogger logger)
    {
        var directory = Path.Combine(Directory.GetCurrentDirectory(), neuralConfig.GamesOutputDirectory);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        else
        {
            Directory.Delete(directory, true);
            Directory.CreateDirectory(directory);
        }
        var gameCount = 0;
        foreach (var username in neuralConfig.Users)
        {
            // Obtenir la liste des liens d'archives
            string archivesURL = $"https://api.chess.com/pub/player/{username}/games/archives";
            string archivesResponse = await httpClient.GetStringAsync(archivesURL);
            var archives = JsonSerializer.Deserialize<ArchivesResponse>(archivesResponse);
            var selectedArchives = new List<string>(archives?.archives);
            foreach (var archive in selectedArchives)
            {
                // Obtenir les parties de l'archive
                string gamesResponse = await httpClient.GetStringAsync(archive);
                var gamesData = JsonSerializer.Deserialize<GamesResponse>(gamesResponse);
                var games = gamesData?.games;
                if (games == null) continue;
                foreach (var game in games)
                {

                    string pgn = game.pgn;
                    string gameUrl = game.url;
                    if (string.IsNullOrEmpty(pgn) || !ChessBoard.TryLoadFromPgn(pgn, out ChessBoard board))
                    {

                        logger.LogInformation("Game {gameUrl} cannot be used", gameUrl);
                        continue;
                    }
                    // Créer un nom de fichier unique pour chaque partie en utilisant la date
                    string date = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    string fileName = Path.Combine(directory, $"{username}-{date}.pgn");
                    // Écrire le PGN dans le fichier
                    await File.WriteAllTextAsync(fileName, pgn);
                    gameCount++;
                    if (gameCount == neuralConfig.NumberData)
                    {
                        break;
                    }
                    //_logger.LogInformation("Partie récupérée: {filename}", fileName);
                }
                if (gameCount == neuralConfig.NumberData)
                {
                    break;
                }
            }

            logger.LogInformation("Recuperation of player {username} done.", username);
            if (gameCount == neuralConfig.NumberData)
            {
                break;
            }
        }
    }



}

