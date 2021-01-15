// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Script.WebHost.Management;
using Microsoft.Azure.WebJobs.Script.WebHost.Models;
using Microsoft.Azure.WebJobs.Script.WebHost.Security.Authorization.Policies;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Script.WebHost.Controllers
{
    public class KubernetesPodController : Controller
    {
        private readonly IEnvironment _environment;
        private readonly IInstanceManager _instanceManager;
        private readonly ILogger _logger;
        private readonly StartupContextProvider _startupContextProvider;

        public KubernetesPodController(IEnvironment environment, IInstanceManager instanceManager, ILoggerFactory loggerFactory, StartupContextProvider startupContextProvider)
        {
            _environment = environment;
            _instanceManager = instanceManager;
            _logger = loggerFactory.CreateLogger<KubernetesPodController>();
            _startupContextProvider = startupContextProvider;
        }

        [HttpPost]
        [Route("admin/pod/assign")]
        public async Task<IActionResult> Assign([FromBody] HostAssignmentContext assignmentContext)
        {
            _logger.LogDebug($"Starting container assignment for host : {Request?.Host}");

            // before starting the assignment we want to perform as much
            // up front validation on the context as possible
            string error = await _instanceManager.ValidateContext(assignmentContext);
            if (error != null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, error);
            }

            var succeeded = _instanceManager.StartAssignment(assignmentContext);

            return succeeded
                ? Accepted()
                : StatusCode(StatusCodes.Status409Conflict, "Instance already assigned");
        }
    }
}
