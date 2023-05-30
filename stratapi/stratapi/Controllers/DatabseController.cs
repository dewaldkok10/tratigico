using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using stratapi.DatabaseTablesM;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    private readonly StratDbContext _context;

    public LoginController(StratDbContext context)
    {
        _context = context;
    }

    // Login handler
    // Should Generate Token when succesfully authenticated
    [HttpPost("login")]
    public async Task<ActionResult<LoginResp>> Login(LoginReq request)
    {
        var login = await _context.Logins
            .Include(l => l.UserDetails)
            .FirstOrDefaultAsync(l => l.Username == request.Username && l.Password == request.Password);

        if (login == null)
        {
            return BadRequest("Invalid username or password.");
        }

        string token = GenerateToken(login);

        var response = new LoginResp
        {
            Token = token,
            Success = true
        };

        return response;
    }

    // Go's and gets user details based on the token
    [HttpGet("user-details")]
    public async Task<ActionResult<UserDetailResp>> GetUserDetails([FromQuery] string token)
    {
        var username = GetUsernameFromToken(token);

        if (string.IsNullOrEmpty(username))
        {
            return BadRequest("Invalid token.");
        }

        var login = await _context.Logins
            .Include(l => l.UserDetails)
            .FirstOrDefaultAsync(l => l.Username == username);

        if (login == null)
        {
            return NotFound("User not found.");
        }

        var userDetails = login.UserDetails;
        int dateDifference = (DateTime.Now - userDetails.DateOfBirth).Days;

        var response = new UserDetailResp
        {
            Name = userDetails.Name,
            Surname = userDetails.Surname,
            DateOfBirth = userDetails.DateOfBirth,
            DateDifference = dateDifference,
            Success = true
        };

        return response;
    }

    // fetches a list of all the logins
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Login>>> GetLogins()
    {
        var logins = await _context.Logins.ToListAsync();
        return logins;
    }

    // Creates new login entries
    [HttpPost]
    public async Task<ActionResult<Login>> CreateLogin(Login login)
    {
        _context.Logins.Add(login);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLogin), new { id = login.Id }, login);
    }

    // Gets a specific loginID when succesfully logged
    [HttpGet("{id}")]
    public async Task<ActionResult<Login>> GetLogin(int id)
    {
        var login = await _context.Logins.FindAsync(id);

        if (login == null)
        {
            return NotFound();
        }

        return login;
    }

    // Keeps updating the log
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLogin(int id, Login login)
    {
        if (id != login.Id)
        {
            return BadRequest();
        }

        _context.Entry(login).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Keeps the logg clean by deleting
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLogin(int id)
    {
        var login = await _context.Logins.FindAsync(id);

        if (login == null)
        {
            return NotFound();
        }

        _context.Logins.Remove(login);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Supposed to genretae tokens...
    private string GenerateToken(Login login)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, login.Id.ToString()),
            new Claim(ClaimTypes.Name, login.Username)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(" "); // the secret key would need to be entered over here when you have it figured out.
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    // Retrieves the username from the token
    private string GetUsernameFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(" "); // Replace with key
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };

            var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            var usernameClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            return usernameClaim;
        }
        catch
        {
            return null;
        }
    }
}