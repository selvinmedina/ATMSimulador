using System.Reflection;

namespace ATMSimulador.Domain.Security
{
    public class EncryptionHelper
    {
        private readonly EncryptionService _encryptionService;

        public EncryptionHelper(EncryptionService encryptionService)
        {
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
        }

        public T2 EncriptarPropiedades<T1, T2>(T1 originalObj) where T2 : new()
        {
            if (originalObj == null) return default(T2);

            var encryptedObj = new T2();
            var originalProperties = typeof(T1).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var encryptedProperties = typeof(T2).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var originalProperty in originalProperties)
            {
                var encryptedProperty = encryptedProperties.FirstOrDefault(p => p.Name == originalProperty.Name && p.PropertyType == typeof(string));
                if (encryptedProperty != null)
                {
                    var value = originalProperty.GetValue(originalObj);
                    if (value != null)
                    {
                        string encryptedValue = _encryptionService.Encrypt(value.ToString());
                        encryptedProperty.SetValue(encryptedObj, encryptedValue);
                    }
                }
                else
                {
                    encryptedProperty = encryptedProperties.FirstOrDefault(p => p.Name == originalProperty.Name && p.PropertyType.IsClass);
                    if (encryptedProperty != null)
                    {
                        var value = originalProperty.GetValue(originalObj);
                        if (value != null)
                        {
                            var encryptedValue = EncriptarPropiedades(value, Activator.CreateInstance(encryptedProperty.PropertyType));
                            encryptedProperty.SetValue(encryptedObj, encryptedValue);
                        }
                    }
                    else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(originalProperty.PropertyType) && originalProperty.PropertyType != typeof(string))
                    {
                        var listType = typeof(List<>).MakeGenericType(encryptedProperty.PropertyType.GenericTypeArguments[0]);
                        var encryptedList = (System.Collections.IList)Activator.CreateInstance(listType);

                        var originalList = (System.Collections.IEnumerable)originalProperty.GetValue(originalObj);
                        foreach (var item in originalList)
                        {
                            var encryptedItem = EncriptarPropiedades(item, Activator.CreateInstance(encryptedProperty.PropertyType.GenericTypeArguments[0]));
                            encryptedList.Add(encryptedItem);
                        }

                        encryptedProperty.SetValue(encryptedObj, encryptedList);
                    }
                }
            }

            return encryptedObj;
        }

        public Response<T2> EncriptarResponse<T1, T2>(Response<T1> originalResponse) where T2 : new()
        {
            var encryptedResponse = new Response<T2>
            {
                Ok = originalResponse.Ok,
                Message = originalResponse.Message
            };

            if (originalResponse.Data != null)
            {
                encryptedResponse.Data = EncriptarPropiedades<T1, T2>(originalResponse.Data);
            }

            return encryptedResponse;
        }

        private object EncriptarPropiedades(object originalObj, object encryptedObj)
        {
            if (originalObj == null) return encryptedObj;

            var originalProperties = originalObj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var encryptedProperties = encryptedObj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var originalProperty in originalProperties)
            {
                var encryptedProperty = encryptedProperties.FirstOrDefault(p => p.Name == originalProperty.Name && p.PropertyType == typeof(string));
                if (encryptedProperty != null)
                {
                    var value = originalProperty.GetValue(originalObj);
                    if (value != null)
                    {
                        string encryptedValue = _encryptionService.Encrypt(value.ToString());
                        encryptedProperty.SetValue(encryptedObj, encryptedValue);
                    }
                }
                else
                {
                    encryptedProperty = encryptedProperties.FirstOrDefault(p => p.Name == originalProperty.Name && p.PropertyType.IsClass);
                    if (encryptedProperty != null)
                    {
                        var value = originalProperty.GetValue(originalObj);
                        if (value != null)
                        {
                            var encryptedValue = EncriptarPropiedades(value, Activator.CreateInstance(encryptedProperty.PropertyType));
                            encryptedProperty.SetValue(encryptedObj, encryptedValue);
                        }
                    }
                    else if (typeof(System.Collections.IEnumerable).IsAssignableFrom(originalProperty.PropertyType) && originalProperty.PropertyType != typeof(string))
                    {
                        var listType = typeof(List<>).MakeGenericType(encryptedProperty.PropertyType.GenericTypeArguments[0]);
                        var encryptedList = (System.Collections.IList)Activator.CreateInstance(listType);

                        var originalList = (System.Collections.IEnumerable)originalProperty.GetValue(originalObj);
                        foreach (var item in originalList)
                        {
                            var encryptedItem = EncriptarPropiedades(item, Activator.CreateInstance(encryptedProperty.PropertyType.GenericTypeArguments[0]));
                            encryptedList.Add(encryptedItem);
                        }

                        encryptedProperty.SetValue(encryptedObj, encryptedList);
                    }
                }
            }

            return encryptedObj;
        }
    }
}
