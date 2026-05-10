namespace Transcodarr.Core.Database.Enums;

public enum JobState
{
    Pending,      
    Assigned,     
    Processing,   
    Completed,    
    Failed,       
    Cancelled,    
    LeaseExpired  
}