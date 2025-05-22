using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPdbAccounts.Models
{
    public class RegistrationModel
    {
        public string StudentID { get; set; }
        public string CourseID { get; set; }
        public decimal PracticeScore { get; set; }
        public decimal ProcessScore { get; set; }
        public decimal FinalScore { get; set; }
        public decimal TotalScore { get; set; }
    }
}
