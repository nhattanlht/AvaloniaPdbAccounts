using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPdbAccounts.Models
{
    public class StudentModel
    {
        public string ID { get; set; }            
        public string NAME { get; set; }           
        public string GENDER { get; set; }         
        public DateTime BIRTHDAY { get; set; }    
        public string ADDRESS { get; set; }        
        public string PHONE { get; set; }         
        public string DEPARTMENT { get; set; }     
        public string STATUS { get; set; }

        // Hiển thị giới tính (1 → Nam, 0 → Nữ)
        public string GenderDisplay => GENDER == "1" ? "Nam" : "Nữ";
    }
}
