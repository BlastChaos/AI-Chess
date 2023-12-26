using Chess;
using OpenQA.Selenium;
namespace Game {
    public class GameConfiguration {
        public required IWebDriver Driver {get; set;}
        public required ChessBoard ChessBoard {get; set;}
        public required  bool FirstTime {get; set;}
        public IWebElement? gameWebElement {get; set;}
        public PieceColor? Color {get; set;}
        public string? Username {get; set;}
        public string? Oppenentname {get; set;}

        public bool IsMyTurn {get {
            if(Username == null || Oppenentname == null || Color == null) {
                return false;
            }
            return ChessBoard.Turn.Value == Color.Value;
        }}
    }
}