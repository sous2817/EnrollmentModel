namespace Semio.ClientService.Data.Intelligence
{
    public class Address
    {
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string City { get; set; }
        public string StateOrProvince { get; set; }
        public CountryInfo Country { get; set; }

        public Address()
        {
            Country = new CountryInfo();
        }
    }
}