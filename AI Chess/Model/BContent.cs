using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace AI_Chess.Model
{
    public class BContent
    {
        public int Position { get; set; }
        public int To { get; set; }
        public double Value { get; set; }

    }
}
