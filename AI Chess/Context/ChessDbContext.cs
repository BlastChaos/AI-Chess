using Microsoft.EntityFrameworkCore;
namespace AI_Chess.Context
{

    public class ChessDbContext : BaseChessDbContext
    {
        public ChessDbContext(DbContextOptions<ChessDbContext> options) : base(options) { }
    }
}
