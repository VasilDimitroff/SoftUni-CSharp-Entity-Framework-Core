using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VaporStore.Data.Models.Enums;

namespace VaporStore.Data.Models
{
    public class Purchase
    {
        public int Id { get; set; }

        [Required]
        public virtual PurchaseType Type { get; set; }
       
        [Required]
        public string ProductKey { get; set; }

        public DateTime Date { get; set; }
        public int CardId { get; set; }
        public virtual Card Card { get; set; }
        public int GameId { get; set; }
        public virtual Game Game { get; set; }

    }
}