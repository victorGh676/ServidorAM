using System;
using System.Collections.Generic;

namespace Crud.Server.Models;

public partial class ImgsProducto
{
    public int IdImagen { get; set; }

    public byte[]? Imagen { get; set; }

    public int? IdProPer { get; set; }

    public virtual Producto? IdProPerNavigation { get; set; }
}
