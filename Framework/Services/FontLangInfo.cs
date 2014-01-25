namespace TESVSnip.Framework.Services
{
    internal struct FontLangInfo
    {
        public readonly static FontLangInfo Default = new FontLangInfo(1252, 1033, 0);
        public readonly ushort CodePage;
        public readonly ushort lcid;
        public readonly byte charset;

        public FontLangInfo(ushort CodePage, ushort lcid, byte charset)
        {
            this.CodePage = CodePage;
            this.lcid = lcid;
            this.charset = charset;
        }
    }
}