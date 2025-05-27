using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPdbAccounts.Models
{
    public class RegistrationModel
    {
        public string StudentID { get; set; } = string.Empty;
        public string CourseID { get; set; } = string.Empty;
        public decimal? PracticeScore { get; set; }
        public decimal? ProcessScore { get; set; }
        public decimal? FinalScore { get; set; }
        public decimal? TotalScore { get; set; }

        // Display properties
        public string PracticeScoreDisplay => PracticeScore.HasValue ? PracticeScore.Value.ToString() : "-";
        public string ProcessScoreDisplay => ProcessScore.HasValue ? ProcessScore.Value.ToString() : "-";
        public string FinalScoreDisplay => FinalScore.HasValue ? FinalScore.Value.ToString() : "-";
        public string TotalScoreDisplay => TotalScore.HasValue ? TotalScore.Value.ToString() : "-";
    }
}
