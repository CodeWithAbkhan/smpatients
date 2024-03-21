using System.Globalization;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using smpatients.Data;
using smpatients.Services;
namespace smpatients;

public class BotApiService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly HttpClient _httpClient;
    private readonly PatientServices _patientServices;
    private readonly PdfImageConverter _pdfImageConverter;

    public BotApiService(ApplicationDbContext dbContext, HttpClient httpClient, PatientServices patientServices,PdfImageConverter pdfImageConverter)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _patientServices = patientServices;
         _pdfImageConverter =pdfImageConverter;

        // Set up the authorization header
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", "secret-key-here");
    }

    public async Task<bool> FetchAndInsertPatientsAsync()
    {
        try
        {
            var apiUrl = "https://api.landbot.io/v1/customers/";
            // Send a GET request to the API with the authorization header
            var jsonResponse = await _httpClient.GetStringAsync(apiUrl);
            ApiResponse apiResponse = JsonConvert.DeserializeObject<ApiResponse>(jsonResponse);

            // Deserialize the JSON response into a list of PatientDto objects
            bool success = apiResponse.Success;
            List<PatientDto> patients = apiResponse.customers;
            var existingPatients = await _dbContext.patients.AsNoTracking().ToListAsync();

            // Remove patients from the 'patients' list that exist in 'existingPatients'
            patients.RemoveAll(patient => existingPatients.Any(existingPatient => existingPatient.AppId == patient.id));
            var lastExistingPatient = existingPatients.OrderByDescending(p => p.Id == existingPatients.Count()).Last();

            // Get the Id of the last patient or set it to 1 if the list is empty
            var count = (lastExistingPatient != null) ? lastExistingPatient.Id : 1;
            count++;
            foreach (var dto in patients.Take(5))
            {
                count++;

                var existingPatient = _dbContext.patients.FirstOrDefault(p => p.AppId == dto.id);

                if (existingPatient != null)
                {
                  
                  
                }
                else
                {
                  
                  
                    double phoneValue;
                    string phoneString = double.TryParse(dto.phone, NumberStyles.Float, CultureInfo.InvariantCulture, out phoneValue) ? phoneValue.ToString(CultureInfo.InvariantCulture) : null;
                    var patient = new Patient
                    {


                        City = dto.city,
                        consent = dto.consent,
                        Country = dto.country,
                        DateRegistered = dto.register_date != null ? DateTimeOffset.FromUnixTimeSeconds((long)dto.register_date).UtcDateTime : DateTime.MinValue,
                        Dob = !string.IsNullOrEmpty(dto.dob)
                            ? DateTime.ParseExact(dto.dob, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                            : (DateTime?)null,
                        Name = dto.name,
                        Email = dto.email,
                        Height = dto.height,
                        HospitalsPatient = dto.hospitals_patient,
                        AppId = dto.id,
                        MageInjuryFile = dto.mage_injury_file,
                        MedicalImage = dto.med_image,
                        MedReport = dto.med_report,
                        MedReportFile = dto.med_report_file,
                        NumOfSurgery = dto.num_of_surgery,
                        OptIn = dto.opt_in,
                        PhoneNew = phoneString,
                        Completed = IsPatientComplete(dto),
                        ScanInjuryFile = dto.scan_injury_file,
                        State = dto.state,
                        SummaryOfCondition = dto.summary_of_condition,
                        Url = dto.url,
                        Weight = dto.weight,
                        Welcome = dto.welcome 
                    };
                    patient = await ProcessAllAsync(patient);

                    _dbContext.patients.Add(patient);
                }
            }

            // Save changes to the database
            await _dbContext.SaveChangesAsync();

            return true; // Successfully fetched and inserted patients
        }
        catch (Exception ex)
        {
            // Handle exception (log, throw, etc.)
            Console.WriteLine($"Error: {ex.Message}");
            return false;
        }
    }
    private bool IsPatientComplete(PatientDto patient)
    {
        return patient != null &&
            patient.name != "" &&
            patient.dob != null &&
            patient.phone != null &&
            patient.state != "" &&
            patient.mage_injury_file != "" &&
            patient.num_of_surgery != "" &&
            patient.summary_of_condition != "" &&
            patient.hospitals_patient != "" &&
            patient.weight != 0;
    }
    
   private async Task<Patient> ProcessAllAsync(Patient patient)
    {
        if (!string.IsNullOrEmpty(patient.MageInjuryFile))
        {
            patient = await ProcessFileAsync(patient.MageInjuryFile, patient.AppId, patient);
        }
        
        if (!string.IsNullOrEmpty(patient.ScanInjuryFile))
        {
            patient = await ProcessFileAsync(patient.ScanInjuryFile, patient.AppId, patient);
        }
        
        if (!string.IsNullOrEmpty(patient.MedReportFile))
        {
            patient = await ProcessFileAsync(patient.MedReportFile, patient.AppId, patient);
        }
        
        return patient;
    }

    private async Task<Patient> ProcessFileAsync(string filePath, int appId, Patient pt)
    {
      // if pdf
        if(_patientServices.isPDF(filePath))
        {
            if(_patientServices.pdfFileExists($"{appId}-{Path.GetFileName(filePath)}"))
            {
                if(_patientServices.ImageFileExists($"{appId}-{Path.GetFileNameWithoutExtension(filePath)}-1.png"))
                {
                var pages1 = await _patientServices.CountPdfPages(Path.Combine("wwwroot","files",$"{appId}-{Path.GetFileName(filePath)}"));
                int pages =pages1;
                var fnameWithoutEx = Path.Combine($"/images/{appId}-{Path.GetFileNameWithoutExtension(filePath)}");
                    AddImagesToPatient(pt.Images, fnameWithoutEx,appId, pages);
                 } else{
                       int pages = await _pdfImageConverter.ConvertPdfToImages($"{appId}-{Path.GetFileName(filePath)}", 1, appId);
                       Console.WriteLine($"{pages} pages converted");
                       await ProcessFileAsync(filePath,appId,pt);
                 }
            }
            else{
                        await _pdfImageConverter.DownloadPdfFile(filePath, appId);
                            await ProcessFileAsync(filePath,appId,pt);

            }
        }
        //is image
        else if(_patientServices.IsImageFile(filePath))
        {
            if(_patientServices.ImageFileExists($"{appId}-{Path.GetFileName(filePath)}")){
                string imgUrl = $"/images/{appId}-{Path.GetFileName(filePath)}";

                pt.Images.Add(new PatientImage(){imgUrl=imgUrl,AppId=appId});
            } else if(_patientServices.ImageFileExists($"{Path.GetFileName(filePath)}"))
                {
                    string imgUrl1  =$"{Path.GetFileName(filePath)}";
                    pt.Images.Add(new PatientImage(){imgUrl=imgUrl1,AppId=appId});
                }
                else{
                await _patientServices.DownloadimgFile(filePath, appId);
                        await ProcessFileAsync(filePath,appId,pt);
                }
        }
       // is video file 
        else if(_patientServices.IsVideoFile(filePath))
        {
            if(_patientServices.VideoFileExists($"{appId}-{Path.GetFileName(filePath)}")){
            PatientFiles pfile = new PatientFiles{
                AppId= pt.AppId,
                CreatedBy = pt.Name,
                FileUrl = $"/videos/{appId}-{Path.GetFileName(filePath)}",
                FileType = "video",
                FilePages = 1
            };
            pt.Files.Add(pfile);
            
            } else {
                    await _patientServices.DownloadotherFile(filePath, appId,"videos");
                            await ProcessFileAsync(filePath,appId,pt);

            }
        }
       // is Document file 
        else if(_patientServices.IsDocumentFile(filePath)){
            if(_patientServices.documentFileExists($"{appId}-{Path.GetFileName(filePath)}")){
             PatientFiles pfile = new PatientFiles{
                AppId= pt.AppId,
                CreatedBy = pt.Name,
                FileUrl = "/documents/"+ $"{appId}-{Path.GetFileName(filePath)}",
                FileType = "document",
                FilePages = 1
            };
            pt.Files.Add(pfile);
            
            } else {
                    await _patientServices.DownloadotherFile(filePath, appId,"documents");
                            await ProcessFileAsync(filePath,appId,pt);

            }
        }
        //is other file 
        else if(_patientServices.OtherFilesExists($"{appId}-{Path.GetFileName(filePath)}")){
             PatientFiles pfile = new PatientFiles{
                AppId= pt.AppId,
                CreatedBy = pt.Name,
                FileUrl =$"/others/{appId}-{Path.GetFileName(filePath)}",
                FileType = "others",
                FilePages = 1
            };
                pt.Files.Add(pfile);
                
        } else  if(!_patientServices.OtherFilesExists($"{appId}-{Path.GetFileName(filePath)}")){
                await _patientServices.DownloadotherFile(filePath, appId,"other");
                        await ProcessFileAsync(filePath,appId,pt);

        } 
        return pt;
    }
    private void AddImagesToPatient(List<PatientImage> imglist, string imagePathWithoutExt,int AppId, int pages)
    {
        for (int i = 1; i <= pages; i++)
        {
            string imageFilePath = Path.ChangeExtension($"{imagePathWithoutExt}-{i}", "png");
            imglist.Add(new PatientImage(){imgUrl=imageFilePath, AppId=AppId});
        }
    }
    public class ApiResponse
    {
        public bool Success { get; set; }
        public List<PatientDto> customers { get; set; }
    }

}
