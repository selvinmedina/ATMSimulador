using System;
using System.Collections.Generic;
using System.Text;

namespace ATMSimulador.Dominio.Security
{
    public class EncryptionService
    {
        private readonly byte[] IVector = new byte[] { 27, 9, 45, 27, 0, 72, 171, 54 };

        private readonly string _key = "ATF#.345T4TIRNGFG8FDG888434R3";
    }
}
