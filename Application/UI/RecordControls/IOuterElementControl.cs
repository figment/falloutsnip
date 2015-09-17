namespace FalloutSnip.UI.RecordControls
{
    internal interface IOuterElementControl : IElementControl
    {
        IElementControl InnerControl { get; set; }
    }
}