using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eBAUsersHelper
{
    public class eBAUser
    {
        public eBAUser()
        {
            Groups = new List<GroupInfo>();
        }

        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public int Status { get; set; }
        public int? Category { get; set; }
        public int? Sex { get; set; }
        public string Manager { get; set; }
        public string DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string ProfessionId { get; set; }
        public string ProfessionName { get; set; }
        public string PositionId { get; set; }
        public string PositionName { get; set; }
        public string CompanyId { get; set; }
        public string CompanyName { get; set; }
        public List<GroupInfo> Groups { get; set; }

        public class GroupInfo
        {
            public string GroupId { get; set; }
            public string GroupDescription { get; set; }
        }

    }
}
