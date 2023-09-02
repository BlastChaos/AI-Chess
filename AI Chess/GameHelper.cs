using System.Text.Json;
using Microsoft.Net.Http.Headers;

namespace AI_Chess;

public static class GameHelper
{
    public static async Task DownloadGames(string username, string outputDirectory)
    {
        HttpClient client = new();
        // Obtenir la liste des liens d'archives
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36");
        string archivesURL = $"https://api.chess.com/pub/player/{username}/games/archives";
        string archivesResponse = await client.GetStringAsync(archivesURL);
        var archives = JsonSerializer.Deserialize<ArchivesResponse>(archivesResponse);
        var selectedArchives = new List<string>(archives?.archives);
        selectedArchives.Reverse(); // Inverser l'ordre pour obtenir les archives les plus récentes en premier

        int gameCount = 1;

        foreach (var archive in selectedArchives)
        {
            // Obtenir les parties de l'archive
            string gamesResponse = await client.GetStringAsync(archive);
            var gamesData = JsonSerializer.Deserialize<GamesResponse>(gamesResponse);
            var games = gamesData?.games;
            if(games == null) continue;
            foreach (var game in games)
            {

                string pgn = game.pgn;
                string gameUrl = game.url;

                // Créer un nom de fichier unique pour chaque partie en utilisant la date
                string date = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                string fileName = Path.Combine(outputDirectory, $"{username}-{date}.pgn");

                // Écrire le PGN dans le fichier
                await File.WriteAllTextAsync(fileName, pgn);

                Console.WriteLine($"Partie récupérée : {fileName}");
                gameCount++;
            }
        }

        Console.WriteLine("Récupération terminée.");
    }
}
public class ArchivesResponse
{
    public string[] archives { get; set; }
}

public class GamesResponse
{
    public Game[] games { get; set; }
}

public class Game
{
    public string pgn { get; set; }
    public string url { get; set; }
}
