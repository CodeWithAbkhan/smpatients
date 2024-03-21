using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace smpatients;


public class Patient
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int AppId { get; set; }

    [StringLength(255)]
    public string? City { get; set; } = "";

    [StringLength(255)]
    public string? consent { get; set; } = "";

    [StringLength(255)]
    public string? Country { get; set; } = "";

    [DataType(DataType.DateTime)]
    public DateTime DateRegistered { get; set; } = DateTime.Now;

    [DataType(DataType.Date)]
    public DateTime? Dob { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DateOfInjury { get; set; }

    [EmailAddress]
    [StringLength(255)]
    public string? Email { get; set; } = "";

    public double? Height { get; set; } = 0;

    [StringLength(255)]
    public string? HospitalsPatient { get; set; } = "";

    [StringLength(255)]
    public string? Ignore { get; set; } = "";

    [StringLength(255)]
    public string? MageInjuryFile { get; set; } = "";

    [StringLength(255)]
    public string? MedicalImage { get; set; } = "";

    [StringLength(255)]
    public string? MedReport { get; set; } = "";

    [StringLength(255)]
    public string? MedReportFile { get; set; } = "";

    [StringLength(255)]
    public string? MedicalConditions { get; set; } = "";

    public string? MoreInfo { get; set; } = "";

    [StringLength(255)]
    public string? Name { get; set; } = "";

    [StringLength(255)]
    public string? NumOfSurgery { get; set; } = "";

    [StringLength(255)]
    public string? OptIn { get; set; } = "";

    [StringLength(255)]
    public string? Options { get; set; } = "";

    public double? Phone { get; set; } 
    public string? PhoneNew { get; set; }  = "";

    [StringLength(255)]
    public string? ScanInjuryFile { get; set; } = "";

    [StringLength(255)]
    public string? State { get; set; } = "";

    public string? SummaryOfCondition { get; set; } = "";

    public string? Url { get; set; } = "";

    public double? Weight { get; set; } = 0;

    [StringLength(255)]
    public string? Welcome { get; set; } = "";

    public bool? Completed { get; set; } = false;
    public bool Released { get; set; } = false;

    public List<PatientFiles> Files { get; set; } = new List<PatientFiles>();
    public List<Reviews> Reviews { get; set; } = new List<Reviews>();
    public List<AccessRole> AccessRoles { get; set; } = new List<AccessRole>();
   public List<PatientImage> Images { get; set; } = new List<PatientImage>();


}
 public class AccessRole{
        [Key]
        public int Id { get; set; } 
        public int? appId { get; set; }
        public string Role { get; set; }
    }

    public class PatientImage
    {
        public int Id { get; set; }

        // You might need additional properties here, like FileName, ContentType, etc.
        public int AppId  { get; set; }
        public string imgUrl { get; set; }

    }
     
    
    public class Reviews{
        [Key]
        public int Id { get; set; } 
        public string ReviewsContent { get; set; }= "";
        public int appId { get; set; } =0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Createdby { get; set; }= "";
        public List<PatientFiles> Files { get; set; } = new List<PatientFiles>();        
    }
    public class PatientFiles
    {
        [Key]
        public int Id { get; set; }
        public string FileType { get; set; } = "";
        public int? AppId { get; set; }
        public string FileUrl { get; set; } = "";
        public int FilePages { get; set; } = 1;
        public string CreatedBy { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int? ReviewsId { get; set; } 
       
        public Reviews? Reviews { get; set; }

        // Navigation property to the Patient entity
        public int? PatientId { get; set; } 
        public Patient? Patient { get; set; }
    }



    public class DialogData
    {
        public string currentUser { get; set; } 
        public string currentRole { get; set; } 
        public string Name { get; set; } 
        public int AppId { get; set; } 
    }
    public class DialogUploadFile
    {
        public string? currentUser { get; set; } 
        public string? currentRole { get; set; } 
        public string? Name { get; set; } 
        public int ReviewId { get; set; } =0;
        public int AppId { get; set; } = 0;
        public bool IsPatient { get; set; } = false;
    }
    public class PatientImages
{
    [Key]
    public int Id { get; set; }=default!;
    public int appId { get; set; }

    public string City { get; set; }=default!;
    public string currentRole { get; set; }=default!;
    public string currentUser { get; set; }=default!;

    public DateTime DateRegistred { get; set; }  =default!;

    public DateTime Dob { get; set; }=default!;


    public string HospitalsPatient { get; set; }=default!;

    public string MageInjuryFile { get; set; }=default!;


    public string MedReport { get; set; }=default!;

    public string MedReportFile { get; set; }=default!;

    public string Name { get; set; }=default!;

    public string NumOfSurgery { get; set; }=default!;

    public string Phone { get; set; }="";

    public string ScanInjuryFile { get; set; }=default!;

    public string State { get; set; }=default!;

    public string SummaryOfCondition { get; set; } = default!;
    public bool Released { get; set; } = false;

    public double Weight { get; set; }=default!;
    public bool? Completed {get; set;} = default!;
    public List<string> Images = new List<string>();
    public List<PatientFiles> OtherFiles = new();
    public List<PatientFiles> Videos = new();
    public List<PatientFiles> Documents = new();

    public List<Reviews> Reviews { get; set; } = new();
    public List<AccessRole> AccessRoles { get; set; } = new();

}