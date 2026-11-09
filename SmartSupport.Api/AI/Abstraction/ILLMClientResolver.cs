namespace SmartSupport.Api.AI.Abstraction
{
    public interface ILLMClientResolver
    {
        ILLMClient Resolve();
    }
}
