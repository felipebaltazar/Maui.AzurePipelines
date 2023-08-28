namespace PipelineApproval.Models;

public class AzureApiResult<T>
{
    public int count { get; set; }
    public T[] value { get; set; }
}
