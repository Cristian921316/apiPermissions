using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Security;

namespace apiPermissions.Models
{
	public class Permissions
	{
		[Key] 
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]  
		public int Id { get; set; }
		[Required]
		public string EmployeeForname { get; set; }
		[Required]
		public string EmployeeSurname { get; set; }
		[Required]
		public int  PermissionTypeId  { get; set;}
		[Required]
		public DateTime PermissionDate { get; set; }		


	}
}
