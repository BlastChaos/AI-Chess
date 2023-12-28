using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AI_Chess.Model
{
    public class WContent
    {
        public int Position { get; set; }
        public int From { get; set; }
        public int To { get; set; }
        public double Value { get; set; }
    }
}
