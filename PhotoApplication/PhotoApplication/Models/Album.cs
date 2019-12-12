using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PhotoApplication.Models
{
    public class Album
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is manadatory!")]
        [StringLength(20, ErrorMessage = "Title cannot be longer than 20 characters")]
        public string AlbumName { get; set; }

        [DataType(DataType.DateTime, ErrorMessage = "Fields must contain date and time")]
        public DateTime Date { get; set; }

        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<Photo> Photo { get; set; }        
    }
}