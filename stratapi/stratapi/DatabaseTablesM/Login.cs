namespace stratapi.DatabaseTablesM
{
    // login table for DataBase
    public class Login
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public UserDetails UserDetails { get; set; }
    }

    // Responce for the login
    public class LoginResp
    {
        public string Token { get; set; }
        public bool Success { get; set; }
    }

    public class LoginReq
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}