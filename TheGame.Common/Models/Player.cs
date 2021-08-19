﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheGame.Common.Models
{
    [Table("Players")]
    public class Player
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public Guid DeviceId { get; set; }

        [Required]
        public bool IsOnline { get; set; }

        public virtual List<Resource> Resources { get; set; } = new List<Resource>();
    }

 

}