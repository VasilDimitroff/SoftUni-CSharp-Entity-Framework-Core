using P03_FootballBetting.Data;
using System;
using Microsoft.EntityFrameworkCore.SqlServer;

namespace P03_FootballBetting
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var db = new FootballBettingContext();

            db.Database.EnsureCreated();

            
        }
    }
}
