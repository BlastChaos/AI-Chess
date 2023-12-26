using Chess;
using OpenQA.Selenium;
namespace Game {
    public class GameConfig {
        public required IWebDriver Driver {get; set;}
        public required ChessBoard ChessBoard {get; set;}
        public required  bool FirstTime {get; set;}
        public IWebElement? gameWebElement {get; set;}
    }
}