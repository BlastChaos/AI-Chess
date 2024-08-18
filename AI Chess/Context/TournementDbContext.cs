using Microsoft.EntityFrameworkCore;
namespace AI_Chess.Context
{

    public class TournementDbContext : BaseChessDbContext
    {

        public TournementDbContext(DbContextOptions<TournementDbContext> options) : base(options) { }
    }
}
