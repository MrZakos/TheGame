using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGame.Common.Models
{
    public class GameLoginDTO
    {
        [Required]
        public Guid DeviceId { get; set; }
    }

    public class GameLoginResultDTO
    {
        public int PlayerId { get; set; }
    }
}