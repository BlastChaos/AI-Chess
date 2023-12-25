using System.Text.RegularExpressions;
using Chess;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Bienvenue dans le jeu d'échec !");
IWebDriver driver = new ChromeDriver();
driver.Navigate().GoToUrl("https://www.google.com");
Console.WriteLine("Aller à votre partie et appuyer sur entrée lorsque le jeu est comencé");
Console.ReadLine(); 
var firstTime = true;
IWebElement element = null;
while (true)
{
    // Attendre une courte période
    Thread.Sleep(1000);
    if(firstTime) {
        element = driver.FindElement(By.TagName("wc-chess-board"));
        firstTime = false;
        if(element == null) 
            throw new Exception("Impossible de trouver le jeu d'échec");
        Console.WriteLine("jeu d'échec trouvé");
            // Vérifier si l'URL a changé
        IWebElement[] names =  driver.FindElements(By.ClassName("user-username-component")).ToArray();
        if(names.Length == 2) {
            Console.WriteLine("Les joueurs sont : " + names[0].Text + " et " + names[1].Text);
        }
    } 
    // Vérifier notre couleur
    string classNamwe = element.GetAttribute("class");
    if (classNamwe == "board") {
        Console.WriteLine("Je suis les blancs");
    } else {
        Console.WriteLine("Je suis les noirs");
    }
    var position = element.FindElements(By.ClassName("piece")).Select(x => x.GetAttribute("class")).ToList();
    int[][] pieces = new int[8][];
    var chessBoard = new ChessBoard();
    position.ForEach(x => {
        var regex=  new Regex(@"\b(?:b?|w?)\s*(\b\w+\b)\s*square-(\d+)");
        var match = regex.Match(x);
        if(match.Success) {
            Piece piece = new(match.Groups[1].Value);
            var position = match.Groups[2].Value;
            short xPosition = Convert.ToInt16(int.Parse(position[0].ToString())-1);
            short yPosition = Convert.ToInt16(int.Parse(position[1].ToString())-1);
            
        }
    });
}