using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PhotoApplication.Models
{
    public class Photo
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is manadatory!")]
        [StringLength(20, ErrorMessage = "Title cannot be longer than 20 characters")]
        public string Title { get; set; }

        [DataType(DataType.DateTime, ErrorMessage = "The field must contain date and time")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "You must upload a picture!")]
        public byte[] Image { get; set; }

        [Required(ErrorMessage = "You must choose a category!")]
        public int CategoryId { get; set; }

        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public virtual Category Category{ get; set;}

        public IEnumerable<SelectListItem> Categories { get; set; }

        public virtual Album Album { get; set; }
        
        public IEnumerable<SelectListItem> Albums { get; set; }

    }
}