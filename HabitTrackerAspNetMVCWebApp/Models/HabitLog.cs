using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HabitTrackerAspNetMVCWebApp.Models
{
    public class HabitLog
    {
        public int Id { get; set; }

        [Required]
        public int HabitId { get; set; }

        [ForeignKey("HabitId")]
        public Habit? Habit { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Log Date")]
        public DateTime LogDate { get; set; }

        [Required]
        [Display(Name = "Status")]
        public HabitLogStatus Status { get; set; } = HabitLogStatus.Completed;
    }
}