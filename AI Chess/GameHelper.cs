using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using AI_Chess.Activation;
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

    public static List<TurnInfo> GetGameInfo(string outputDirectory)
    {
        List<TurnInfo> turnInfos = new List<TurnInfo>();
        string[] filePaths = Directory.GetFiles(outputDirectory, "*.pgn",
                                         SearchOption.TopDirectoryOnly);

        foreach (string filePath in filePaths){
            string input =  File.ReadAllText(filePath);
            string datePattern = @"Date\s+""(\d{4}.\d{2}.\d{2})""";
            string whitePattern = @"White\s+""([^""]+)""";
            string blackPattern = @"Black\s+""([^""]+)""";
            string movesPattern = @"\d+(\.+)\s+([A-z0-9]+)";

            // Extraction de la date
            Match dateMatch = Regex.Match(input, datePattern);
            DateTime.TryParseExact(dateMatch.Groups[1].Value, "yyyy.mm.dd", null, DateTimeStyles.None, out DateTime parsedDate);

            // Extraction du joueur blanc
            Match whiteMatch = Regex.Match(input, whitePattern);
            string white = whiteMatch.Groups[1].Value;

            // Extraction du joueur noir
            Match blackMatch = Regex.Match(input, blackPattern);
            string black = blackMatch.Groups[1].Value;

            // Extraction de la liste des coups
            MatchCollection moveMatches = Regex.Matches(input, movesPattern);

            foreach (Match moveMatch in moveMatches)
            {
            }

            // Affichage des résultats
            Console.WriteLine("Date du jeu : " + parsedDate);
            Console.WriteLine("Joueur Blanc : " + white);
            Console.WriteLine("Joueur Noir : " + black);
            Console.WriteLine("Liste des coups :");
        }
        Console.WriteLine("Récupération des info terminée.");
        return turnInfos;
    }
}




