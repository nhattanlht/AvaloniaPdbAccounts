using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPdbAccounts.Models
{
    public class EmployeeModel
    {
        public string EmployeeID { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public decimal Salary { get; set; }
        public decimal Allowance { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public string Department { get; set; }
    }
}
