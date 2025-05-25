using System.ComponentModel.DataAnnotations;

namespace Server.DTO
{
    public class CreateRoleDto
    {
        [Required(ErrorMessage ="Role Name is Required")]
        public string RoleName { get; set; } = string.Empty;
    }
}