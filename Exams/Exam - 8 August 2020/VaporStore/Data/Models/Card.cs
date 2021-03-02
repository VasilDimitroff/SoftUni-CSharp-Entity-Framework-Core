using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VaporStore.Data.Models.Enums;

namespace VaporStore.Data.Models
{
    public class Card
    {
        public int Id { get; set; }

        [Required]
        public string Number { get; set; }

        [Required]
        [DataType("char(3)")]
        public string Cvc { get; set; }

        [Required]
        public virtual CardType Type { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }

        public virtual ICollection<Purchase> Purchases { get; set; } = new HashSet<Purchase>();
    }
}
