using System.Collections.Generic;
using FalloutSnip.Domain.Model;

namespace FalloutSnip.Domain.Rendering
{
    public interface IRenderer
    {
        //string GetHeader(BaseRecord rec);
        //string GetDescription(BaseRecord rec);

        string Render(BaseRecord rec, Dictionary<string,object> kwargs);
    }
}
