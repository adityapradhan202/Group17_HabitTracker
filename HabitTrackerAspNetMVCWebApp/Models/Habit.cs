using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HabitTrackerAspNetMVCWebApp.Models
{
    public class Habit
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters.")]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters.")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Frequency")]
        public Frequency Frequency { get; set; }

        [Required]
        [Display(Name = "Status")]
        public HabitStatus Status { get; set; }

        [Required]
        [Display(Name = "Kanban Status")]
        public KanbanStatus KanbanStatus { get; set; } = KanbanStatus.Todo;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }

        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        public ICollection<HabitLog> HabitLogs { get; set; } = new List<HabitLog>();
    }
}