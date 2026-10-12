using Microsoft.AspNetCore.Mvc;

namespace <%= $CLI_PARAM_Organization %>.<%= $CLI_PARAM_Product %>.<%= $CLI_PARAM_idSegment %>.Services
{
    /// <summary>
    /// <%= $CLI_PARAM_Tenant %> Services
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class <%= $CLI_PARAM_Tenant %>Controller : ControllerBase
    {
        private const string OBJECT_TYPE_NAME = "<%= $CLI_PARAM_Organization %>.<%= $CLI_PARAM_Product %>.<%= $CLI_PARAM_idSegment %>.Services.<%= $CLI_PARAM_Tenant %>Management";
    }
}