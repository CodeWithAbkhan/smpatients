using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace smpatients
{
    public class PatientDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int pId { get; set; }
        public string email { get; set; } = String.Empty;

        public string anything_else { get; set; }= String.Empty;
        public string city { get; set; }= String.Empty;
        public string close_bot { get; set; }= String.Empty;
        public string consent { get; set; }= String.Empty;
        public string country { get; set; }= String.Empty;
        public string dob { get; set; }= String.Empty;
        public string med_report_file { get; set; }= String.Empty;
        public bool MedTrained { get; set; }
        public string MoreInfo { get; set; }= String.Empty;
        public bool OptIn { get; set; }
        public double height { get; set; } =0;
        public string hospitals_patient { get; set; }= String.Empty;
        public string mage_injury_file { get; set; }= String.Empty;
        public string medical_conditions { get; set; }= String.Empty;
        public string med_image { get; set; }= String.Empty;
        public string med_report { get; set; }= String.Empty;
        public string name { get; set; } = String.Empty;
        public string num_of_surgery { get; set; }= String.Empty;
        public string options { get; set; }= String.Empty;
        public string phone { get; set; }= String.Empty;
        public string scan_injury_file { get; set; }= String.Empty;
        public string state { get; set; }= String.Empty;
        public string summary_of_condition { get; set; }= String.Empty;
        public string url { get; set; }= String.Empty;
        public double weight { get; set; } = 0;
        public string welcome { get; set; }= String.Empty;
        public int id { get; set; }
        public double register_date {get; set;}
        public string opt_in {get; set;}
    }











}