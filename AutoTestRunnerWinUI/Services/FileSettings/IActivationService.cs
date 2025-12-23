using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTestRunnerWinUI.Services.FileSettings
{
    public interface IActivationService
    {
        Task ActivateAsync(object activationArgs);
    }
}
