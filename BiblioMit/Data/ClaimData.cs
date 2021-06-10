using System.Collections.Generic;

namespace BiblioMit.Data
{
    public static class ClaimData
    {
        internal static List<string> UserClaims { get; } = new List<string>
                                                            {
                                                                "Instituciones",
                                                                "Centros",
                                                                "Coordenadas",
                                                                "Producciones",
                                                                "Contactos",
                                                                "Usuarios",
                                                                "Foros",
                                                                "per",
                                                                "sernapesca",
                                                                "intemit",
                                                                "mitilidb",
                                                                "webmaster"
                                                            };
    }
}
