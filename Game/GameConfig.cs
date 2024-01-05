using Chess;
using OpenQA.Selenium;
namespace Game {
    public class GameConfig {
        public required IWebDriver Driver {get; set;}
        public required ChessBoard ChessBoard {get; set;}
        public required IWebElement GameWebElement {get; set;}
        public required PieceColor Color {get; set;}
        public required string Username {get; set;}
        public required string Oppenentname {get; set;}

        public bool IsMyTurn {get {
            if(Username == null || Oppenentname == null || Color == null) {
                return false;
            }
            return ChessBoard.Turn.Value == Color.Value;
        }}
    }
}