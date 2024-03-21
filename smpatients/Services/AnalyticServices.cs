using smpatients.Data;

namespace smpatients;

public class AnalyticServices
{
    private readonly ApplicationDbContext _dbContext;

    public AnalyticServices(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

      public async Task<List<(string Role, int PatientCount)>> GetCountPatientsByRolesAsync()
        {
            try
            {
                var countPatientsByRoles =  _dbContext.accessRoles
                    .GroupJoin(
                        _dbContext.patients,
                        role => role.Id,
                        patient => patient.AccessRoles.Any() ? patient.AccessRoles.First().Id : -1, // Assuming Patient has a collection of AccessRoles
                        (role, patients) => new
                        {
                            Role = role.Role,
                            PatientCount = patients.Count()
                        }).OrderByDescending(result => result.PatientCount).ToList();

                // Projecting the result to a list of tuples
                var result = countPatientsByRoles.Select(item => (item.Role, item.PatientCount)).ToList();
                return result;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it according to your application's requirements
                Console.WriteLine($"An error occurred while retrieving count of patients by roles: {ex.Message}");
                return null;
            }
        }
}
