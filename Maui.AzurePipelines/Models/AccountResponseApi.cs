using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineApproval.Models;

public class AccountResponseApi
{
    public int count { get; set; }
    public AccountInfo[] value { get; set; }
}
