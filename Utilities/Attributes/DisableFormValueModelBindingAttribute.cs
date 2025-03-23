using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FileManager.Utilities.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class DisableFormValueModelBindingAttribute : Attribute, IResourceFilter
{
    public void OnResourceExecuted(ResourceExecutedContext context)
    {
    }

    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        IList<IValueProviderFactory> factories = context.ValueProviderFactories;

        factories.RemoveType<FormValueProviderFactory>();
        factories.RemoveType<FormFileValueProviderFactory>();
        factories.RemoveType<JQueryFormValueProviderFactory>();
    }
}
