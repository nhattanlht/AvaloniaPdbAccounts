using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPdbAccounts.Models
{
    public class CourseOfferingModel
    {
        public string OfferingID { get; set; }
        public string ModuleID { get; set; }
        public string InstructorID { get; set; }
        public int Semester { get; set; }
        public int Year { get; set; }
    }
}
