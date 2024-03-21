using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace smpatients
{
    public class UserModel
    {
    
            public string UserId { get; set; }
            public string UserName { get; set; }
            public string Email { get; set; }
            public List<IdentityRole> Roles { get; set; } 
    }
    public class User
    {
        public string Id { get; set; }
        public string Username { get; set; }
    }
    public class RoleDto
    {
        [Required(ErrorMessage = "Role Name is required.")]
        [MaxLength(50, ErrorMessage = "Role Name Should be at least of len 2 and at most 50")]
        [MinLength(2, ErrorMessage = "Role Name Should be at least of len 2 and at most 50")]
        public string RoleName { get; set; }
    }

}
