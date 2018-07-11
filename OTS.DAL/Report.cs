public class ClientDetails
{
    public int Id{get;set;}
    public string ClientName { get; set; }
    public string HomeLanguage { get; set; }
}

public class Report
{
    public int Id{get;set;}

    public ClientDetails Client { get; set; }
}