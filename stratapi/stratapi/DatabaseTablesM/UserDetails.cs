namespace stratapi.DatabaseTablesM
{
    // Userdetails table for databse
    public class UserDetails
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public DateTime DateOfBirth { get; set; }

        public int LoginId { get; set; }

        public Login Login { get; set; }
    }

    // Responvse on the userdetails
    public class UserDetailResp
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int DateDifference { get; set; }
        public bool Success { get; set; }
    }
}