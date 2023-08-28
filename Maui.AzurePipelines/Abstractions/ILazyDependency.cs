namespace PipelineApproval.Abstractions;

public interface ILazyDependency<T>
{
    T Value { get; }
}
