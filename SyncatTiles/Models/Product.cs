using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SyncatTiles.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Display(Name ="Short Desc")]
        public string ShortDesc { get; set; }
        public string Description { get; set; }
        [Range(1, int.MaxValue)]
        public double Price { get; set; }
        public string Image { get; set; }
        [ForeignKey(nameof(Category))]
        [Display(Name="Category Type")]
        public int CategoryId { get; set; }
        public virtual Category Category {get;set;}

        [ForeignKey(nameof(ApplicationType))]
        [Display(Name = "Application Type")]
        public int ApplicationTypeId { get; set; }
        public virtual ApplicationType ApplicationType { get; set; }
    }
}
