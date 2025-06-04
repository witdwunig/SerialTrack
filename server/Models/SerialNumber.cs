namespace server.Models
{
    public class SerialNumber
    {
        public int Id {get; set;}
        public required string Name {get; set;}
        public required string Number {get; set;}
        public DateTimeOffset CreatedAt {get; set;} = DateTime.UtcNow;
    }
}