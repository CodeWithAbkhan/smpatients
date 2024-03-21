using Microsoft.EntityFrameworkCore;
using smpatients.Data;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Text;


namespace smpatients.Services;

public class PatientServices
{
    private readonly ApplicationDbContext _dbContext;

    public PatientServices(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        
        
    }
      public async Task<Patient> GetPatientbyAppId(int AppId)
    {
        try
        {
            var getPatient = await _dbContext.patients
            .Where(p => p.AppId == AppId)
            .Include(p => p.Files)
            .Include(p => p.Reviews) // Include Reviews
                .ThenInclude(c => c.Files) // Include files within Reviews
            .Include(p => p.AccessRoles)
            .Include(p => p.Images)
            .AsNoTracking()
            .FirstOrDefaultAsync();




            return getPatient;
        }
        catch (Exception ex)
        {
            // Log the exception or handle it according to your application's requirements
            Console.WriteLine($"An error occurred while retrieving patient by AppId: {ex.Message}");
            return null;
        }
    }
    
    public async Task<List<AccessRole>> AddRoleAccessToPatient(List<string> roles, int appId)
    {
        try
        {
           
            var patient = await _dbContext.patients
                .Include(p => p.AccessRoles)
                .FirstOrDefaultAsync(p => p.AppId == appId);
            if (patient != null && patient.AccessRoles != null)
            {
                    if(roles!=null)
                    {
                        patient.AccessRoles.Clear();
                        var rolesToAdd = roles.Distinct().ToList();
                        foreach (var role in rolesToAdd)
                        {
                            patient.AccessRoles.Add(new AccessRole { Role = role, appId = appId });
                        }
                        await _dbContext.SaveChangesAsync();
                        return patient.AccessRoles.ToList();
                    }
                    else{
                        patient.AccessRoles.Clear();
                        await _dbContext.SaveChangesAsync();
                        return patient.AccessRoles.ToList();
                    }
            }
            return null;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            Console.WriteLine($"Concurrency exception: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding roles to patient: {ex.Message}");
            throw;
        }
    }

  


    // public async Task UpdatePatientimagesbyAppId(List<string> imageList,int AppId)
    // {
    //     // Assuming your DbContext has a DbSet for Patient
    //     var existingPatient = await _dbContext.patients.FirstOrDefaultAsync(p => p.AppId ==AppId);

    //     if (existingPatient != null)
    //     {
    //         existingPatient.Images = imageList; // Update the images

    //         // Assuming you have some saving logic in your context
    //         await _dbContext.SaveChangesAsync();
    //     }
    //     else
    //     {
    //         // Handle case where patient with given AppId is not found
    //         // Maybe throw an exception or log an error
    //         Console.WriteLine("Patient with specified AppId not found.");
    //     }
    // }
    public async Task<List<Patient>> GetPatientsByRoleAsync(string userRole)
    {
        IQueryable<Patient> query = _dbContext.patients
            .Include(p => p.AccessRoles)
            .OrderByDescending(o=>o.DateRegistered);
        if (userRole != "admin")
        {
            query = query.Where(p => p.AccessRoles.Any(r => r.Role == userRole))
                .Include(p => p.AccessRoles);  // Include AccessRoles again for the filtered result.
        }
        return await query.ToListAsync();
    }
    public async Task<List<PatientImage>> getImagesbyFileName(string filename)
    {
        // Extract file name without extension and string characters after last hyphen
        var fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(filename);
        var fileNameWithoutSuffix = fileNameWithoutExtension.Substring(0, fileNameWithoutExtension.LastIndexOf("-"));

        // Query for images matching the extracted file name criteria
        var query = _dbContext.patientImages
            .Where(image => image.imgUrl.StartsWith(fileNameWithoutSuffix));

        // Execute the query and return the result as a list
        return await query.ToListAsync();
    }
    public async Task<List<Reviews>> GetReviewsAsync(int AppId)
    {
        var Reviews = await _dbContext.reviews
            .Where(p => p.appId == AppId )
            .Include(f=> f.Files)
            .ToListAsync();

        return Reviews;
    }
    public async Task<List<PatientFiles>> GetFilesByReviewsAsync(int id)
    {
        var patientFilesOnly = await _dbContext.patientFiles
            .Where(p => p.ReviewsId == id )
            .ToListAsync();

        return patientFilesOnly;
    }
    public async Task<List<PatientFiles>> GetFilesByPatientAsync(int appId)
    {
        var patientFilesOnly = await _dbContext.patients
            .Where(p => p.AppId == appId)
            .SelectMany(p => p.Files) // Flatten the Files collection
            .ToListAsync();

        return patientFilesOnly;
    }
    public async Task Addreview(string review, int appId, string CurrentUser)
    {
        Reviews preview = new();
        preview.ReviewsContent = review;
        preview.appId = appId;
        preview.Createdby = CurrentUser;
        // Assuming _dbContext.patients is your DbSet<Patient> in the DbContext
        var patient = await _dbContext.patients.FirstOrDefaultAsync(p => p.AppId == appId);
        if (patient != null)
        {
            patient.Reviews.Add(preview);
            await _dbContext.SaveChangesAsync();
        }
        else
        {
            Console.WriteLine("Reviews Issue");
        }
    }
    public bool IsPatientComplete(Patient patient)
    {
        return patient != null &&
            patient.Name != "" &&
            patient.Dob != null &&
            patient.PhoneNew != "" 
            || patient.Phone != 0 &&
            patient.State != "" &&
            patient.MageInjuryFile != "" &&
            patient.NumOfSurgery != "" &&
            patient.SummaryOfCondition != "" &&
            patient.HospitalsPatient != "" &&
            patient.Weight != 0;
    }

    
    public async Task AddFileByPatientAsync(int patientId, PatientFiles file, List<string> imglist)
    {
        
        var patient = await _dbContext.patients
            .Include(p => p.Files) 
            .FirstOrDefaultAsync(p => p.AppId == patientId);
        
        if (patient != null)
        {
            _dbContext.Entry(file).State = EntityState.Detached; // Detach the file entity
            patient.Files ??= new List<PatientFiles>(); 
            foreach(var img in imglist){
                patient.Images.Add(new PatientImage(){imgUrl=img,AppId=patient.AppId});
            }
            patient.Files.Add(file);
            await _dbContext.SaveChangesAsync();
        }
        else
        {
            throw new InvalidOperationException("Patient not found.");
        }
    }
    public async Task AddFileByReviewsAsync(int ReviewId, PatientFiles file)
    {
        var review = await _dbContext.reviews
            .Include(p => p.Files) 
            .FirstOrDefaultAsync(p => p.Id == ReviewId);
        
        if (review != null)
        {
            _dbContext.Entry(file).State = EntityState.Detached; // Detach the file entity
            review.Files ??= new List<PatientFiles>(); 
            
            review.Files.Add(file);
            await _dbContext.SaveChangesAsync();
        }
        else
        {
            throw new InvalidOperationException("review not found.");
        }
    }
    public string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        var stringBuilder = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            stringBuilder.Append(chars[random.Next(chars.Length)]);
        }

        return stringBuilder.ToString();
    }
    public async Task SaveFileToDirectoryAsync(string fileUrl, string directoryPath)
    {
        try
        {
            if (string.IsNullOrEmpty(fileUrl) || !File.Exists(fileUrl))
            {
                // Log or handle the case where the source file doesn't exist
                Console.WriteLine($"Source file '{fileUrl}' does not exist or is empty.");
                return;
            }
           Console.WriteLine($"directory path: {directoryPath}");
            var destinationPath = Path.Combine("app", directoryPath);
           Console.WriteLine($"destinationPath: {destinationPath}");      
            File.Copy(fileUrl, directoryPath , true);
        }
        catch (Exception ex)
        {
            // Log or handle the exception appropriately
            Console.WriteLine($"Error copying file: {ex.Message}");
        }
    }

    // public async Task SaveFileToDirectoryAsync(string fileUrl, string directoryPath)
    // {
    //     try
    //     {
    //         if (string.IsNullOrEmpty(fileUrl) || !File.Exists(fileUrl))
    //         {
    //             // Log or handle the case where the source file doesn't exist
    //             Console.WriteLine($"Source file '{fileUrl}' does not exist or is empty.");
    //             return;
    //         }

    //         Console.WriteLine($"directory path: {directoryPath}");
            
    //         // Ensure the destination directory exists
    //         Directory.CreateDirectory(directoryPath);

    //         var destinationPath = Path.Combine(directoryPath, Path.GetFileName(fileUrl));

    //         Console.WriteLine($"destinationPath: {destinationPath}");

    //         // Use asynchronous file copy
    //         using (var sourceStream = new FileStream(fileUrl, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
    //         using (var destinationStream = new FileStream(destinationPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
    //         {
    //             await sourceStream.CopyToAsync(destinationStream);
    //         }

    //         Console.WriteLine($"File copied successfully.");
    //     }
    //     catch (Exception ex)
    //     {
    //         // Log or handle the exception appropriately
    //         Console.WriteLine($"Error copying file: {ex.Message}");
    //     }
    // }

    public async Task<IQueryable<Patient>> GetAllPatientsAsync()
    {

        var Patients = await Task.FromResult(_dbContext.patients.AsQueryable());
        return Patients;
    }
    public bool isPDF(string fileName)
    {
        var pathpdf =Path.GetExtension(fileName)?.ToLower();
        if (pathpdf == ".pdf")
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool pdfFileExists(string file)
    {
        // Specify the file path based on the wwwroot/images folder
        // string pdfFolder = Path.Combine("files");
        // string[] matchingFiles = Directory.GetFiles(pdfFolder, $"{appId}-{Path.GetFileName(file)}");
        string pdfPath = Path.Combine( "wwwroot","files", $"{Path.GetFileNameWithoutExtension(file)}.pdf");
        // Check if any file with the specified base name exists
        // return matchingFiles.Length > 0;
        return File.Exists(pdfPath);
    }
    public bool ImageFileExists(string file)
    {
        // Specify the file path based on the wwwroot/images folder
        string imageFolder = Path.Combine("wwwroot", "images");
        string[] matchingFiles = Directory.GetFiles(imageFolder, $"{Path.GetFileName(file)}");

        // Check if any file with the specified base name exists
        return matchingFiles.Length > 0;
    }
    public bool VideoFileExists(string file)
    {
        // Specify the file path based on the wwwroot/video folder
        string videoFolder = Path.Combine("wwwroot", "videos");
        string[] matchingFiles = Directory.GetFiles(videoFolder, $"{Path.GetFileName(file)}");

        // Check if any file with the specified base name exists
        return matchingFiles.Length > 0;
    }
    public bool documentFileExists(string file)
    {
        // Specify the file path based on the wwwroot/video folder
        string videoFolder = Path.Combine("wwwroot", "documents");
        string[] matchingFiles = Directory.GetFiles(videoFolder, $"{Path.GetFileName(file)}");

        // Check if any file with the specified base name exists
        return matchingFiles.Length > 0;
    }
    public bool OtherFilesExists(string file)
    {
        // Specify the file path based on the wwwroot/images folder
        string imageFolder = Path.Combine("wwwroot", "others");
        string[] matchingFiles = Directory.GetFiles(imageFolder, $"{Path.GetFileName(file)}");

        // Check if any file with the specified base name exists
        return matchingFiles.Length > 0;
    }
      public bool IsImageFile(string filePath)
        {
            string[] imageExtensions = { ".gif", ".jpg", ".jpeg", ".jpe", ".jfif", ".png", ".tiff", ".tif", ".svg",".webp", ".ico", ".jfif", ".jif"  }; // Add more extensions if needed

            string fileExtension = Path.GetExtension(filePath);

            if (!string.IsNullOrEmpty(fileExtension))
            {
                string lowerCaseExtension = fileExtension.ToLowerInvariant();
                return Array.IndexOf(imageExtensions, lowerCaseExtension) != -1;
            }

            return false; // No extension or unknown extension
        }
    public bool IsDocumentFile(string filePath)
    {
        string[] documentExtensions = { ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".txt", ".rtf" }; // Add more document extensions if needed

        string fileExtension = Path.GetExtension(filePath);

        if (!string.IsNullOrEmpty(fileExtension))
        {
            string lowerCaseExtension = fileExtension.ToLowerInvariant();
            return Array.IndexOf(documentExtensions, lowerCaseExtension) != -1;
        }

        return false; // No extension or unknown extension
    }

    public bool IsVideoFile(string filePath)
    {
        string[] videoExtensions = { ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".3gp", ".mpeg", ".mpg", ".webm" }; // Add more video extensions if needed

        string fileExtension = Path.GetExtension(filePath);

        if (!string.IsNullOrEmpty(fileExtension))
        {
            string lowerCaseExtension = fileExtension.ToLowerInvariant();
            return Array.IndexOf(videoExtensions, lowerCaseExtension) != -1;
        }

        return false; // No extension or unknown extension
    }
    public async Task<string> DownloadotherFile(string otherfilesUrl,int appId,string others)
    {
        var _otherfilesSavePath = $"wwwroot/{others}/";
        try
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(otherfilesUrl);
                response.EnsureSuccessStatusCode();

                string fileName = Path.GetFileName(otherfilesUrl);
                string savePath = Path.Combine(_otherfilesSavePath, $"{appId}-{fileName}");

                using (FileStream fileStream = File.Create(savePath))
                {
                    await response.Content.CopyToAsync(fileStream);
                    fileStream.Flush();
                }

                return savePath;
            }
        }
        catch (HttpRequestException ex)
        {
            // Log the exception or handle it as needed
            Console.WriteLine("DownloadotherFile issue:" + ex.Message);

            // In this case, returning null to indicate a failure
            return null;
        }
    }
    public async Task<string> DownloadimgFile(string imageUrl,int appId)
    {
        var _imageSavePath = "wwwroot/images/";
        try
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(imageUrl);
                response.EnsureSuccessStatusCode();

                string fileNameWithExt = Path.GetFileName(imageUrl);
                string savePath = Path.Combine(_imageSavePath, $"{appId}-{fileNameWithExt}");

                using (FileStream fileStream = File.Create(savePath))
                {
                    await response.Content.CopyToAsync(fileStream);
                    fileStream.Flush();
                }

                return savePath;
            }
        }
        catch (HttpRequestException ex)
        {
            // Log the exception or handle it as needed
            Console.WriteLine("DownloadimgFile issue:" + ex.Message);
            // In this case, returning null to indicate a failure
            return null;
        }
    }
   public async Task<int> CountPdfPages(string pdfPath)
{
    try
    {
        byte[] pdfContent = await File.ReadAllBytesAsync(pdfPath);
        
        // Use a temporary directory instead of GetTempFileName
        string tempDirectory = Path.Combine(Path.GetTempPath(), "smpatients");
        if (!Directory.Exists(tempDirectory))
        {
            Directory.CreateDirectory(tempDirectory);
        }

        string tempPdfFilePath = Path.Combine(tempDirectory, "temp.pdf");

        try
        {
            await File.WriteAllBytesAsync(tempPdfFilePath, pdfContent);

            using (PdfDocument pdfDocument = PdfReader.Open(tempPdfFilePath, PdfDocumentOpenMode.Import))
            {
                return pdfDocument.PageCount;
            }
        }
        finally
        {
            File.Delete(tempPdfFilePath);
        }
    }
    catch (Exception ex)
    {
        // Log or handle the exception appropriately
        Console.WriteLine($"Error counting PDF pages: {ex.Message}" + pdfPath);
        return 0; // or another appropriate error handling strategy
    }
}


   
  }
   