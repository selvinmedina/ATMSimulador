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
    }
}
