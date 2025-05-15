using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WFT.Infra.Contracts.Interfaces
{
    public partial interface IWorkContext
    {
        long? UserId { get; }
        string Username { get; }
        List<string> Roles { get; }
        List<string> Permissions { get; }
    }
}
