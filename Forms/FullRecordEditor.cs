using System.Windows.Forms;
using TESVSnip.Properties;

namespace TESVSnip.Forms
{
    public partial class FullRecordEditor : Form
    {
        public FullRecordEditor()
        {
            InitializeComponent();
        }

        public FullRecordEditor(Record rec) : this()
        {
            Record = rec;
        }


        public Record Record
        {
            get { return panelRecordEditor.Record; }
            set
            {
                panelRecordEditor.Record = value;
                if (value != null)
                {
                    Text = string.Format("{0} - [{1:X8}] {2}", Resources.FullRecordEditorTitle, value.FormID,
                                         value.DescriptiveName);
                }
            }
        }
    }
}