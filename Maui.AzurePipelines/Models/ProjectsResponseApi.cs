using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineApproval.Models;

public class ProjectsResponseApi
{
    public int count { get; set; }
    public Project[] value { get; set; }
}
