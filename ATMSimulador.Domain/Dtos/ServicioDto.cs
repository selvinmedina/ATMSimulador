﻿namespace ATMSimulador.Domain.Dtos
{
    public class ServicioDto
    {
        public int ServicioId { get; set; }
        public string NombreServicio { get; set; } = null!;
        public string? Descripcion { get; set; }
    }
}
