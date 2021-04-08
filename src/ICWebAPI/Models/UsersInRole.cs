using System.Collections.Generic;

namespace ICWebAPI.Models
{
    public class UsersInRole
    {
        public string RoleId { get; set; }
        public List<string> EnrolledUsers { get; set; }
        public List<string> RemovedUsers { get; set; }
    }
}