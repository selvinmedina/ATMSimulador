using ATMSimulador.Dominio.Enums;
using System;

namespace ATMSimulador.Dominio.Dtos
{
    public class SignalRClientDto
    {
        public TipoConexionCliente TipoConexionCliente { get; set; }
        public string TokenConnetionId { get; set; } = null!;

        public string TokenDocumentId { get; set; } = null!;
        public DateTime Fecha { get; set; } = DateTime.Today;
    }
}
