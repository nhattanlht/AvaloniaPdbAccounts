using System.Collections.Generic;

namespace AvaloniaPdbAccounts.Models
{
    public class Employee
    {
        public string? MANLD { get; set; }
        public string? Role { get; set; }
    }

    public class Student
    {
        public string? MASV { get; set; }
    }

    public class UserAccount
    {
        public string? Username { get; set; }
        public string? Role { get; set; }
        public string? Password { get; set; }
    }

    public class Role
    {
        public string? RoleName { get; set; }      // Tên role, ví dụ "DBA", "SV"
        public string? Description { get; set; }   // Mô tả role (tuỳ chọn)
    }

    public class Roles
    {
        public List<Role> RolesList { get; set; } = new List<Role>();
    }
}
