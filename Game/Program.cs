using System.Text.RegularExpressions;
using Chess;
using Game;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
// See https://aka.ms/new-console-template for more information

//ÉTAPE 1- Trouver le jeu d'échec
Console.WriteLine("Bienvenue dans le jeu d'échec !");
var gameConfig = new GameConfig(){
    FirstTime = true,
    ChessBoard = new ChessBoard(),
    Driver = new ChromeDriver()
};
gameConfig.Driver.Navigate().GoToUrl("https://www.google.com");
Console.WriteLine("Aller à votre partie et appuyer sur entrée lorsque le jeu est commencé");
Console.ReadLine(); 

while (true)
{
    // Attendre une courte période
    Thread.Sleep(1000);
    if(gameConfig.FirstTime) {
        gameConfig.gameWebElement = gameConfig.Driver.FindElement(By.TagName("wc-chess-board"));
        gameConfig.FirstTime = false;
        if(gameConfig.gameWebElement == null) 
            throw new Exception("Impossible de trouver le jeu d'échec");
        Console.WriteLine("jeu d'échec trouvé");
            // Vérifier si l'URL a changé
        IWebElement[] names =  gameConfig.Driver.FindElements(By.ClassName("user-username-component")).ToArray();
        if(names.Length == 2) {
            gameConfig.Oppenentname = names[0].Text;
            gameConfig.Username = names[1].Text;
            Console.WriteLine("Les joueurs sont : " + names[0].Text + " et " + names[1].Text);
        }
        // Vérifier notre couleur
        string classNamwe = gameConfig.gameWebElement.GetAttribute("class");
        if (classNamwe == "board") {
            Console.WriteLine("Je suis les blancs");
            gameConfig.Color = PieceColor.White;
        } else {
            Console.WriteLine("Je suis les noirs");
            gameConfig.Color = PieceColor.Black;
        }
    } 


    if(!gameConfig.IsMyTurn) {
        Console.WriteLine($"Tour de {gameConfig.Oppenentname}");
        Function.GetOpponentMove(gameConfig);
        var oppenentMove = gameConfig.ChessBoard.ExecutedMoves.Last();
        continue;
    } else {
        // Console.WriteLine($"Tour de {gameConfig.Username}");
        // var move = Function.GetMove(gameConfig.ChessBoard);
        // gameConfig.ChessBoard.Move(move);
        // Function.PlayMove(gameConfig, move);
        // Console.WriteLine($"Mouvement de {gameConfig.Oppenentname} : {oppenentMove}");
    }
}

/*
ÉTAPES
1. Trouver le jeu d'échec
2. Vérifier notre tour au début du jeu
    2.1 si c'est notre tour, on joue
    2.2 si ce n'est pas notre tour, on attend
3. Alterner
*/
public partial class Program {
    [GeneratedRegex("\\b(?:b?|w?)\\s*(\\b\\w+\\b)\\s*square-(\\d+)")]
    private static partial Regex MyRegex();
}



