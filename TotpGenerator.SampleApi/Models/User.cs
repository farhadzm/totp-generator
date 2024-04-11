namespace TotpGenerator.SampleApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Guid SecurityStamp { get; set; }
    }
}
