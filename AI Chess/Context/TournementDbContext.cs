using AI_Chess.Model;
using Microsoft.EntityFrameworkCore;
namespace AI_Chess.Context
{

    public class TournementDbContext : ChessDbContext
    {

        public TournementDbContext(DbContextOptions<ChessDbContext> options) : base(options) { }
    }
}
