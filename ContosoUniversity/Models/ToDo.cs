using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Models
{
    public class ToDo
    {
        public int ID { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 1)]
        [Display(Name = "Title")]
        public required string Title { get; set; }

        [StringLength(1000)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Is Completed")]
        public bool IsCompleted { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Created Date")]
        [Column(TypeName = "datetime2")]
        public DateTime CreatedDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Completed Date")]
        [Column(TypeName = "datetime2")]
        public DateTime? CompletedDate { get; set; }
    }
}
