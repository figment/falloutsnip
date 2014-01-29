using System.Collections.Generic;
using TESVSnip.Domain.Model;

namespace TESVSnip.Domain.Rendering
{
    public interface IRenderer
    {
        //string GetHeader(BaseRecord rec);
        //string GetDescription(BaseRecord rec);

        string Render(BaseRecord rec, Dictionary<string,object> kwargs);
    }
}
