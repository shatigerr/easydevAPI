namespace easydev.Models;

public class HomeDashboard
{
    public Project project { get; set; }
    public List<Log> logs { get; set; }
    public bool apiStatus { get; set; }
    public bool dbStatus { get; set; }
    public bool appStatus { get; set; }
    public int endpointsCount { get; set; }
    public string lastRequest { get; set; }
    public List<RequestsPerDay> requestsPerDay { get; set; }
    public Database database { get; set; }
}

public class RequestsPerDay
{
    public string Day { get; set; }
    public int Requests { get; set; }
}
