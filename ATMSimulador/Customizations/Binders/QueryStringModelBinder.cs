using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ATMSimulador.Customizations.Binders
{
    public class QueryStringNullOrEmptyModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (result == ValueProviderResult.None)
            {
                bindingContext.Result = ModelBindingResult.Success(null);
            }
            else
            {
                bindingContext.ModelState.SetModelValue(bindingContext.ModelName, result);

                var rawValue = result.FirstValue;

                if (string.IsNullOrEmpty(rawValue))
                {
                    // Si el valor es vacio, interpreta un string vacio
                    bindingContext.Result = ModelBindingResult.Success(String.Empty);
                }
                else if (rawValue is string)
                {
                    // Si el valor es un string valido, usar ese valor
                    bindingContext.Result = ModelBindingResult.Success(rawValue);
                }
                else
                {
                    // El valor es algo mas, fallo
                    bindingContext.ModelState.TryAddModelError(
                        bindingContext.ModelName,
                        "El valor debe ser string o null"
                        );
                }

            }
            return Task.CompletedTask;
        }
    }

    public class QueryStringNullOrEmptyModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(string)
                && context.BindingInfo.BindingSource != null
                && context.BindingInfo.BindingSource.CanAcceptDataFrom(BindingSource.Query))
            {
                return new QueryStringNullOrEmptyModelBinder();
            }

            return null;
        }
    }
}
