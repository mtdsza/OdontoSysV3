using Microsoft.AspNetCore.Mvc;

namespace OdontoSys.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[ApiConventionType(typeof(DefaultApiConventions))]
public class ApiControllerBase : ControllerBase
{
}