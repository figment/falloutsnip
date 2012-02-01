using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TESVSnip.Collections;

namespace TESVSnip.Model
{
    enum SearchCondRecordType
    {
        Exists,
        Missing
    }
    enum SearchCondElementType
    {
        Equal,
        Not,
        Greater,
        Less,
        GreaterEqual,
        LessEqual,
        StartsWith,
        EndsWith,
        Contains,
        Exists,
        Missing
    }

    class SearchCriteriaSettings
    {
        public string Type;
        public IEnumerable<SearchCriteria> Items;
    }


    abstract class SearchCriteria
    {
        public bool Checked { get; set; }
        public string Name { get; set; }

        public abstract bool Match(Record r);
        public abstract bool Match(SubRecord sr);
        public abstract bool Match(Element  se);
    }

    class SearchSubrecord : SearchCriteria
    {
        public SubrecordStructure Record { get; set; }
        public ICollection<SearchElement> Children { get; set; }
        public SearchCondRecordType Type { get; set; }

        public override bool Match(Record r)
        {
            var sr = r.SubRecords.FirstOrDefault(x => x.Name == this.Record.name);
            return Match(sr);
        }
        public override bool Match(SubRecord sr)
        {
            return (this.Type == SearchCondRecordType.Exists ^ sr == null);
        }
        public override bool Match(Element se)
        {
            return false;
        }
    }
    class SearchElement : SearchCriteria
    {
        public ElementStructure Record { get; set; }
        public SearchSubrecord Parent { get; set; }
        public SearchCondElementType Type { get;set; }
        public object Value { get; set; }

        public override bool Match(Record r)
        {
            bool any = false;
            foreach (bool value in r.SubRecords.Where(x => x.Name == this.Parent.Record.name).Select(Match))
            {
                if (!value) return false;
                any = true;
            }
            return any;
        }
        public override bool Match(SubRecord sr)
        {
            bool any = false;
            foreach (bool value in sr.EnumerateElements().Where(x => x.Structure.name == this.Record.name).Select(Match))
            {
                if (!value) return false;
                any = true;
            }
            return any;
        }
        public override bool Match(Element se)
        {
            if (Type == SearchCondElementType.Exists && se != null)
                return true;
            if (Type == SearchCondElementType.Missing && se == null)
                return true;
            if (se == null)
                return false;

            int diff = ValueComparer.Compare(se.Value, this.Value);
            switch (this.Type)
            {
                case SearchCondElementType.Equal:
                    return diff == 0;
                case SearchCondElementType.Not:
                    return diff != 0;
                case SearchCondElementType.Greater:
                    return diff > 0;
                case SearchCondElementType.Less:
                    return diff < 0;
                case SearchCondElementType.GreaterEqual:
                    return diff >= 0;
                case SearchCondElementType.LessEqual:
                    return diff <= 0;
                case SearchCondElementType.StartsWith:
                    if (diff == 0) return true;
                    if (se.Value != null && this.Value != null)
                        return se.Value.ToString().StartsWith(this.Value.ToString());
                    break;
                case SearchCondElementType.EndsWith:
                    if (diff == 0) return true;
                    if (se.Value != null && this.Value != null)
                        return se.Value.ToString().EndsWith(this.Value.ToString());
                    break;
                case SearchCondElementType.Contains:
                    if (diff == 0) return true;
                    if (se.Value != null && this.Value != null)
                        return se.Value.ToString().Contains(this.Value.ToString());
                    break;
            }
            return false;
        }

    }
}
