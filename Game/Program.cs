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
            Console.WriteLine("Les joueurs sont : " + names[0].Text + " et " + names[1].Text);
        }
    } 
    // Vérifier notre couleur
    string classNamwe = gameConfig.gameWebElement.GetAttribute("class");
    if (classNamwe == "board") {
        Console.WriteLine("Je suis les blancs");
    } else {
        Console.WriteLine("Je suis les noirs");
    }
    
    var position = gameConfig.gameWebElement.FindElements(By.ClassName("piece")).Select(x => x.GetAttribute("class")).ToList();
    int[][] pieces = new int[8][];
    var newchessPositionsPiece = position.Select<string, PositionPiece>(x => {
        var regex= MyRegex();
        var match = regex.Match(x);
        Piece piece = new(match.Groups[1].Value);
        var position = match.Groups[2].Value;
        short xPosition = Convert.ToInt16(int.Parse(position[0].ToString())-1);
        short yPosition = Convert.ToInt16(int.Parse(position[1].ToString())-1);
        return new(){
            Piece = piece,
            Position = new(xPosition, yPosition)
        };
    }).ToArray();

    List<PositionPiece> chessPostionPiece = new();
    for (short i = 0; i < 8; i++)
    {
        for (short j = 0; j < 8; j++)
        {
            var piece = gameConfig.ChessBoard[i,j];
            if(piece != null) {
                chessPostionPiece.Add(new PositionPiece(){
                    Piece = piece,
                    Position = new(i,j)
                });
            }
        }
    }
    var move = Function.GetMove(chessPostionPiece.ToArray(), newchessPositionsPiece, gameConfig.ChessBoard);
    Console.WriteLine("Mon mouvement est : " + move.ToString());
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



