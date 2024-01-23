using System.Text.RegularExpressions;
using AI_Chess;
using Chess;
using Game;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using ProgramAi = ProgramAI.Program;
// See https://aka.ms/new-console-template for more information

var emptyString = Array.Empty<string>();
Environment.CurrentDirectory = "../AI Chess/";
var app = ProgramAi.BuildWebHost(emptyString);

var neuralNetwork = app.Services.GetRequiredService<NeuralNetwork>();


Console.WriteLine("Welcome to the chess bot");
var chromeOptions = new ChromeOptions();
chromeOptions.AddArgument("--log-level=3");
chromeOptions.AddArgument("--silent");
chromeOptions.AddArgument("--disable-logging");

var driver = new ChromeDriver(chromeOptions);
GameConfig? gameConfig = null;
driver.Navigate().GoToUrl("https://www.chess.com/play/computer");
Console.WriteLine("Go to https://www.chess.com/play/computer and press enter when you are ready");
Console.ReadLine(); 

while (true)
{
    if(gameConfig == null) {
        var gameWebElement = driver.FindElement(By.TagName("wc-chess-board")) 
                                ?? throw new Exception("Impossible to find the chess board");

        var playerTopElement = driver.FindElement(By.Id("player-top"))
                                ?? throw new Exception("Impossible to find the player top element");

        var opponentEloText = playerTopElement.FindElement(By.ClassName("user-tagline-rating")).Text;

        var match = MyRegex1().Match(opponentEloText);
        if(!match.Success) throw new Exception("Impossible to find the opponent elo");
        var opponentElo = double.Parse(match.Groups[1].Value);
        

        Console.WriteLine("Chess board found");
        
        IWebElement[] playerNames =  driver.FindElements(By.ClassName("user-username-component")).ToArray();
        Console.WriteLine( playerNames[0].Text + " vs " + playerNames[1].Text);

        // Vérifier notre couleur
        string color = gameWebElement.GetAttribute("class");
        var pieceColor = color == "board" ? PieceColor.White : PieceColor.Black;
        Console.WriteLine("I am " + pieceColor);

        gameConfig = new GameConfig(){
            ChessBoard = new ChessBoard(),
            Driver = driver,
            GameWebElement = gameWebElement,
            Color = pieceColor,
            Oppenentname = playerNames[0].Text,
            Username = playerNames[1].Text,
            NeuralNetwork = neuralNetwork,
            OppenentElo = opponentElo
        };
    }

    Move move;
    Console.WriteLine("Current Board");
    Console.WriteLine(gameConfig.ChessBoard.ToAscii());
    if(gameConfig.ChessBoard.IsEndGame) {
        var winner = gameConfig.ChessBoard.EndGame.WonSide.ToString() == gameConfig.Color.ToString() ? gameConfig.Username : gameConfig.Oppenentname;
        Console.WriteLine($"Checkmate, {winner} won the game!");
        break;
    }
    if(!gameConfig.IsMyTurn) {
        Console.WriteLine($"Current turn: {gameConfig.Oppenentname}");
        move = Function.GetOpponentMove(gameConfig);
    } else {
        Console.WriteLine($"Current turn: {gameConfig.Username}");
        move = Function.GetOurMove(gameConfig);
    }

    var valideMove = gameConfig.ChessBoard.Move(move);
    if(!valideMove) {
        throw new Exception("Invalid move");
    }
    Console.WriteLine($"Move played: {move}");
}

Console.WriteLine("End of the game");



public partial class Program {
    [GeneratedRegex("\\b(?:b?|w?)\\s*(\\b\\w+\\b)\\s*square-(\\d+)")]
    private static partial Regex MyRegex();
}

partial class Program
{
    [GeneratedRegex(@"\((\d+)\)")]
    private static partial Regex MyRegex1();
}