﻿using System.ComponentModel.DataAnnotations;

namespace VaporStore.Data.Models
{
    public class GameTag
    {
        [Required]
        public int GameId { get; set; }
        public virtual Game Game { get; set; }
       
        [Required]
        public int TagId { get; set; }
        public virtual Tag Tag { get; set; }
    }
}