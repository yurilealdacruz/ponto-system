using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ponto.Domain
{
    public class LoginDto
    {
        public string Identificador { get; set; } // Pode ser ID ou CPF
        public string Senha { get; set; }
    }
}
