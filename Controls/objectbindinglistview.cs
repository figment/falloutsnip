using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Reflection;
using BrightIdeasSoftware;

namespace TESVSnip.Windows.Controls
{
	/// <summary>
	/// Summary description for BindingListView.
	/// </summary>
    public class ObjectBindingListView : BrightIdeasSoftware.ObjectListView
	{
		IList _data = null;

        int lvcacheStart = -1;
        int lvcacheEnd = -1;
        private List<ListViewItem> lvcache = new List<ListViewItem>();

        public ObjectBindingListView()
		{
			InitializeComponent();

			this.FullRowSelect = true;
			this.HideSelection = false;
			this.View = System.Windows.Forms.View.Details;
            //base.QueryItemText += new QueryItemTextHandler(BindingListView_QueryItemText);
            base.CacheVirtualItems += new CacheVirtualItemsEventHandler(BindingListView_CacheVirtualItems);
            base.RetrieveVirtualItem += new RetrieveVirtualItemEventHandler(BindingListView_RetrieveVirtualItem);
		}
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern int SetWindowTheme(IntPtr hWnd, string appName, string partList);

        // You can subclass ListView and override this method
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            SetWindowTheme(this.Handle, "explorer", null);
        }

        private ListViewItem CreateBlankListViewItem()
        {
            var subItems = new OLVListSubItem[this.Columns.Count];
            for (int i=0; i<subItems.Length; ++i)
                subItems[i] = new OLVListSubItem();
            return new OLVListItem(null, subItems,-1);
        }

        void FillListViewItem(ListViewItem lvItem, int index)
        {
            if (index >= 0 && this._data != null && index < this._data.Count)
            {
                object item = this._data[index];
                for (int i = 0; i < lvItem.SubItems.Count; ++i)
                {
                    ColumnBinding header = this.Columns[i] as ColumnBinding;
                    var subItem = lvItem.SubItems[i];
                    if (header != null)
                    {
                        object value = header.GetPropertyValue(item);
                        if (value != null) subItem.Text = value.ToString();
                    }
                }
            }
        }

        void BindingListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex>=lvcacheStart && e.ItemIndex<=lvcacheEnd)
            {
                e.Item = lvcache[e.ItemIndex - lvcacheStart];
            }
            else
            {
                e.Item = CreateBlankListViewItem();
                FillListViewItem(e.Item, e.ItemIndex);
            }
        }

        void BindingListView_CacheVirtualItems(object sender, CacheVirtualItemsEventArgs e)
        {
            int len = e.EndIndex - e.StartIndex + 1;
            lvcacheStart = e.StartIndex;
            if (len != lvcache.Count)
            {
                if (lvcache.Capacity < len)
                    lvcache.Capacity = len;
                while (lvcache.Count < len)
                    lvcache.Add(CreateBlankListViewItem());
                if (lvcache.Count > len)
                    lvcache.RemoveRange(len, lvcache.Count - len);
            }
            for (int i = 0; i<lvcache.Count; ++i)
            {
                FillListViewItem(lvcache[i], e.StartIndex + i);
            }            
        }

        //private event QueryItemTextHandler _QueryItemText;
        //public new event QueryItemTextHandler QueryItemText
        //{
        //    add { _QueryItemText += value; }
        //    remove { _QueryItemText -= value; }
        //}
		

		public new IList Items
		{
			get{ return _data; }
		}


        public ColumnBinding AddBindingColumn<T,TResult>(string PropertyName, string DisplayName)
        {
            ColumnBinding column = new ColumnBinding();
            column.Text = DisplayName;
            column.Property = new ColumnPropertyDescriptor<T, TResult>(PropertyName, null);
            this.Columns.Add(column);
            return column;
        }

        public ColumnBinding AddBindingColumn<T, TResult>(string PropertyName, string DisplayName, int width)
        {
            ColumnBinding column = AddBindingColumn<T,TResult>(PropertyName, DisplayName);
            column.Width = width;
            return column;
        }

        public ColumnBinding AddBindingColumn<T, TResult>(string PropertyName, string DisplayName, int width, Delegate transform)
        {
            ColumnBinding column = AddBindingColumn<T, TResult>(PropertyName, DisplayName, width);
            column.Property = new ColumnPropertyDescriptor<T, TResult>(PropertyName, transform);
            return column;
        }

        public ColumnBinding AddBindingColumn<T, TResult>(string PropertyName, string DisplayName, int width, HorizontalAlignment textAlign)
        {
            ColumnBinding column = AddBindingColumn<T, TResult>(PropertyName, DisplayName, width);
            column.TextAlign = textAlign;
            return column;
        }

        public ColumnBinding AddBindingColumn<T, TResult>(string PropertyName, string DisplayName, int width, HorizontalAlignment textAlign, Delegate transform)
        {
            ColumnBinding column = AddBindingColumn<T, TResult>(PropertyName, DisplayName, width, textAlign);
            column.Property = new ColumnPropertyDescriptor<T, TResult>(PropertyName, transform);
            return column;
        }

        public ColumnBinding AddBindingColumn(string PropertyName, string DisplayName)
		{
			ColumnBinding column = new ColumnBinding();
			column.Text = DisplayName;
            column.Property = new ColumnPropertyDescriptor<object, string>(PropertyName, null);
			this.Columns.Add( column );
            return column;
		}

        public ColumnBinding AddBindingColumn(string PropertyName, string DisplayName, int width)
		{
			ColumnBinding column = AddBindingColumn(PropertyName, DisplayName);
			column.Width = width;
            return column;
		}

        public ColumnBinding AddBindingColumn(string PropertyName, string DisplayName, int width, Delegate transform)
        {
            ColumnBinding column = AddBindingColumn(PropertyName, DisplayName, width);
            column.Property = new ColumnPropertyDescriptor<object, string>(PropertyName, transform);
            return column;
        }

		public ColumnBinding AddBindingColumn(string PropertyName, string DisplayName, int width, HorizontalAlignment textAlign)
		{
			ColumnBinding column = AddBindingColumn(PropertyName, DisplayName, width);
			column.TextAlign = textAlign;
            return column;
		}

        public ColumnBinding AddBindingColumn(string PropertyName, string DisplayName, int width, HorizontalAlignment textAlign, Delegate transform)
        {
            ColumnBinding column = AddBindingColumn(PropertyName, DisplayName, width, textAlign);
            column.Property = new ColumnPropertyDescriptor<object, string>(PropertyName, transform);
            return column;
        }

        public void AddBindingColumn(PropertyDescriptor property)
        {
            ColumnBinding column = new ColumnBinding();
            column.Text = property.DisplayName;
            column.Property = property;
            this.Columns.Add(column);
        }
       
        public void AddBindingColumn(PropertyDescriptor property, string DisplayName)
		{
			ColumnBinding column = new ColumnBinding();
			column.Text = DisplayName;
			column.Property = property;
			this.Columns.Add( column );
		}

		public void AddBindingColumn(PropertyDescriptor property, string DisplayName, int width)
		{
			ColumnBinding column = new ColumnBinding();
			column.Text = DisplayName;
			column.Property = property;
			column.Width = width;
			this.Columns.Add( column );
		}

		public void AddBindingColumn(PropertyDescriptor property, string DisplayName, int width, HorizontalAlignment textAlign)
		{
			ColumnBinding column = new ColumnBinding();
			column.Text = DisplayName;
			column.Property = property;
			column.Width = width;
			column.TextAlign = textAlign;
			this.Columns.Add( column );
		}

		public void RemoveBindingColumns( string PropertyName )
		{
			foreach ( ColumnBinding column in this.Columns )
			{
				if ( (column.Property != null && column.Property.Name == PropertyName)
				  || (column.FieldName == PropertyName )
				   )
				{
					this.Columns.Remove(column);
					break;
				}
			}
		}
        public ColumnBinding GetColumnBinding(int column)
        {
            return ( column >= 0 && column < this.Columns.Count ) ? this.Columns[column] as ColumnBinding : null;
        }

		public void ClearBindingColumns( )
		{
			this.Columns.Clear();
		}

        public int ItemCount
        {
            get { return this.VirtualListSize; }
            set { this.VirtualListSize = value; }
        }

		public IList DataSource
		{
			get{ return _data; }		
			set
			{ 
				if (_data != value)
				{
					IBindingList list = _data as IBindingList;
					if (list != null) list.ListChanged -= new System.ComponentModel.ListChangedEventHandler(DataSource_ListChanged);

					_data = value; 

					list = _data as IBindingList;
					if (list != null) list.ListChanged += new System.ComponentModel.ListChangedEventHandler(DataSource_ListChanged);

					if (this.Columns.Count == 0 && _data is ITypedList)
					{
						ITypedList typedList = _data as ITypedList;
						PropertyDescriptorCollection properties = typedList.GetItemProperties(null);
						if (properties != null)
						{
							foreach( PropertyDescriptor pd in properties )
							{
								ColumnBinding column = new ColumnBinding();
								column.Text = pd.DisplayName;
								column.Property = pd;
								this.Columns.Add( column );
							}
						}
					}
					ItemCount = (_data == null) ? 0 : _data.Count;
				}
			}
		}

		private void BindingListView_QueryItemText(int item, int subItem, out string text)
		{
			text = string.Empty;
			try
			{
                //if (_QueryItemText != null)
                //{
                //    _QueryItemText(item, subItem, out text);
                //}
                //else 
                if( DataSource != null && item < DataSource.Count)
				{
					ColumnBinding header = this.Columns[subItem] as ColumnBinding;
					if (header != null)
					{
						object value = header.GetPropertyValue( DataSource[item] );
						if (value != null) text = value.ToString();
					}
				}
			}
			catch{}
		}

		private void DataSource_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
		{
			switch ( e.ListChangedType )
			{
				case ListChangedType.ItemAdded: goto case ListChangedType.Reset;
				case ListChangedType.ItemChanged: goto case ListChangedType.Reset;
				case ListChangedType.ItemDeleted: goto case ListChangedType.Reset;
				case ListChangedType.ItemMoved: goto case ListChangedType.Reset;
				case ListChangedType.Reset: 
					ItemCount = (_data == null) ? 0 : _data.Count;
                    Invalidate();
					break;
			}
		}

		private void InitializeComponent()
		{
            this.SuspendLayout();
            // 
            // BindingListView
            // 
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.BindingListView_KeyDown);
            //this.CustomSort += new CustomSortHandler(BindingListView_CustomSort);
            this.ResumeLayout(false);

		}
	
		protected override void Dispose(bool disposing)
		{
            //base.QueryItemText -= new QueryItemTextHandler(BindingListView_QueryItemText);
			DataSource = null;
			base.Dispose (disposing);
        }

        #region ColumnPropertyDescriptor
        /// <summary>
        /// Custom column property descriptor
        /// </summary>
        private class ColumnPropertyDescriptor<T, TResult> 
            : PropertyDescriptor
		{
			#region Fields
			/// <summary>
			/// Contains the field index.
			/// </summary>
			string _propName = string.Empty;
            Delegate _propValueCast = null;
			#endregion

			#region Constructors

			/// <summary>
			/// Initializes a new instance of the CsvPropertyDescriptor class.
			/// </summary>
			/// <param name="fieldName">The field name.</param>
			/// <param name="index">The field index.</param>
            public ColumnPropertyDescriptor(string fieldName, Delegate propertyFunction)
				: base(fieldName, null)
			{
				_propValueCast = propertyFunction;
			}

			#endregion

			#region Properties

			/// <summary>
			/// Gets the field index.
			/// </summary>
			/// <value>The field index.</value>
            public Delegate Function
			{
				get { return _propValueCast; }
			}

			#endregion

			#region Overrides

			public override bool CanResetValue(object component)
			{
				return false;
			}

			public override object GetValue(object item)
			{
				object value = null;
                if (_propValueCast != null)
                {
                    try { value = _propValueCast.DynamicInvoke(item); }
                    catch { }
                }
                else if (string.IsNullOrEmpty(this.Name))
                {
                    value = item;
                }
                else
                {
                    if (this.Name.IndexOf(".") > 0)
                    {
                        string[] fields = this.Name.Split('.');
                        object context = item;
                        foreach (string field in fields)
                        {
                            PropertyInfo prop = context.GetType().GetProperty(field);
                            if (prop == null)
                            {
                                context = null;
                                break;
                            }
                            context = prop.GetValue(context, new object[0]);
                        }
                        value = context;
                    }
                    else
                    {
                        PropertyInfo prop = item.GetType().GetProperty(this.Name);
                        if (prop != null)
                        {
                            value = prop.GetValue(item, new object[0]);
                        }
                        else
                        {
                            foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(item))
                            {
                                if (pd.Name == this.Name)
                                {
                                    value = pd.GetValue(item);
                                    break;
                                }
                            }
                        }
                    }
                }
				return value;
			}

			public override void ResetValue(object component)
			{
			}

			public override void SetValue(object component, object value)
			{
			}

			public override bool ShouldSerializeValue(object component)
			{
				return false;
			}

			public override Type ComponentType
			{
				get
				{
                    return typeof(T);
				}
			}

			public override bool IsReadOnly
			{
				get
				{
					return true;
				}
			}

			public override Type PropertyType
			{
				get
				{
					return typeof(TResult);
				}
			}

			#endregion
        }
        #endregion

        #region class ColumnBinding
        public class ColumnBinding : BrightIdeasSoftware.OLVColumn
		{
			PropertyDescriptor _propDesc = null;
            //string _propName = string.Empty;
            //Delegate _propValueCast = null;

			public string FieldName
			{
				get { return _propDesc.Name; }
			}

			public PropertyDescriptor Property
			{
				get { return _propDesc; }
				set { _propDesc = value; }
			}

			public object GetPropertyValue(object item)
			{
				object value = null;
				if (_propDesc != null)
				{
					object entry = item;
                    if (item is ICustomTypeDescriptor)
                    {
                        entry = (item as ICustomTypeDescriptor).GetPropertyOwner(_propDesc);
                        if (entry == null) 
                            entry = item;
                    }
					value = _propDesc.GetValue(entry);
				}
				return value;
			}
        }
        #endregion

        private void BindingListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && !e.Shift && !e.Alt && e.KeyValue == 'C')
            {
                //this.CopyObjectsToClipboard()
                //Copy();
                this.CopySelectionToClipboard();
            }
            else if (e.Control && !e.Shift && !e.Alt && e.KeyValue == 'A')
            {
                SelectAll();
            }
        }


        void BindingListView_CustomSort(int iColumn)
        {
            if (DataSource != null && iColumn >= 0 && iColumn < this.Columns.Count)
			{
                ColumnBinding header = this.Columns[iColumn] as ColumnBinding;
				if (header != null)
				{
                    IBindingListView view = this.DataSource as IBindingListView;
                    if (view != null)
                    {
                        string fieldName = header.Property != null ? header.Property.Name : header.FieldName;
                        if (!string.IsNullOrEmpty(fieldName))
                        {
                            // handle existing sorts
                            if (view.IsSorted)
                            {
                                ListSortDescription[] arr = new ListSortDescription[view.SortDescriptions.Count];
                                view.SortDescriptions.CopyTo(arr,0);
                                bool found = false;
                                for (int idx = 0; idx<arr.Length; ++idx)
                                {
                                    ListSortDescription desc = arr[idx];
                                    if (desc.PropertyDescriptor.Name == fieldName)
                                    {
                                        found = true;
                                        if (idx == 0)
                                        {
                                            if (desc.SortDirection == ListSortDirection.Descending)
                                            {
                                                desc.SortDirection = ListSortDirection.Ascending;
                                            }
                                            else
                                            {
                                                List<ListSortDescription> list = new List<ListSortDescription>(arr);
                                                list.Remove(desc);
                                                arr = list.ToArray();
                                            }
                                        }
                                        else
                                        {
                                            List<ListSortDescription> list = new List<ListSortDescription>(arr);
                                            list.Remove(desc);
                                            list.Insert(0, desc);
                                            desc.SortDirection = ListSortDirection.Ascending;
                                            arr = list.ToArray();
                                        }
                                        break;
                                    }
                                }
                                if (!found)
                                {
                                    List<ListSortDescription> list = new List<ListSortDescription>(arr);
                                    list.Insert(0, new ListSortDescription(header.Property, ListSortDirection.Descending));
                                    while (list.Count > 3)
                                        list.RemoveAt(list.Count - 1);
                                    arr = list.ToArray();
                                }
                                view.ApplySort(new ListSortDescriptionCollection(arr));
                            }
                            else
                            {
                                view.ApplySort(header.Property, ListSortDirection.Ascending);
                            }
                        }
                    }
				}
			}
        }

        #region SendMessage Variations
        [DllImport("user32.dll", SetLastError = true)]
        extern static int SendMessage(IntPtr hWnd, ListViewMessages Msg, int wParam, ref LVHITTESTINFO lvhti);

        [DllImport("user32.dll", SetLastError = true)]
        extern static int SendMessage(IntPtr hWnd, ListViewMessages Msg, int wParam, ref LVITEMA lvitem);

        [DllImport("user32.dll", SetLastError = true)]
        extern static int SendMessage(IntPtr hWnd, ListViewMessages Msg, int wParam, ref LVITEMW lvitem);

        [DllImport("user32.dll", SetLastError = true)]
        extern static int SendMessage(IntPtr hWnd, ListViewMessages Msg, int wParam, ref Rectangle lvitem);

        [DllImport("user32.dll", SetLastError = true)]
        extern static int SendMessage(IntPtr hWnd, ListViewMessages Msg, int wParam, int lvitem);

        [DllImport("user32.dll", SetLastError = true)]
        extern private static IntPtr SendMessage(IntPtr hWnd, uint Msg, UIntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        extern private static IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessageLVI(IntPtr hWnd, int msg,
            int wParam, ref LVITEMA lvi);

        [DllImport("User32.dll")]
        private static extern int SendMessage(IntPtr hWnd, HeaderMessageCodes uMsg, IntPtr wParam, ref HDHITTESTINFO lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg,
            int wParam, int lParam);

        [DllImport("User32.dll")]
        private static extern uint GetMessagePos();

        #endregion

        public bool IsItemSelected(int index)
        {
            return (0 != SendMessage(this.Handle, ListViewMessages.LVM_GETITEMSTATE, index, (int)ListViewItemStates.LVIS_SELECTED));
        }
        public bool IsItemFocused(int index)
        {
            return (0 != SendMessage(this.Handle, ListViewMessages.LVM_GETITEMSTATE, index, (int)ListViewItemStates.LVIS_FOCUSED));
        }

        internal void SetItemState(int index, uint data, ListViewItemStates mask)
        {
            LVITEMA _ms_lvi = new LVITEMA();
            _ms_lvi.stateMask = mask;
            _ms_lvi.state = data;
            SendMessage(this.Handle, ListViewMessages.LVM_SETITEMSTATE, index, ref _ms_lvi);
        }
        public void ClearSelection()
        {
            int next = -1;
            while (true)
            {
                next = SendMessage(this.Handle, ListViewMessages.LVM_GETNEXTITEM, next, (int)ListViewNextItemCodes.LVNI_SELECTED);
                if (next == -1) break;
                SetItemState(next, 0, ListViewItemStates.LVIS_SELECTED);
            }
        }
        public int[] GetSelectionIndices()
        {
            System.Collections.Generic.List<int> sel = new System.Collections.Generic.List<int>();
            int next = -1;
            while (true)
            {
                next = SendMessage(this.Handle, ListViewMessages.LVM_GETNEXTITEM, next, (int)ListViewNextItemCodes.LVNI_SELECTED);
                if (next == -1) break;
                sel.Add(next);
                SetItemState(next, 0, ListViewItemStates.LVIS_SELECTED);
            }
            return sel.ToArray();
        }
        public void SelectItem(int index)
        {
            SetItemState(index, (unchecked((uint)-1)), ListViewItemStates.LVIS_SELECTED);
        }
        public int GetFocusedItem()
        {
            return SendMessage(this.Handle, ListViewMessages.LVM_GETNEXTITEM, -1, 1);
        }
        public void FocusItem(int index)
        {
            SetItemState(index, (unchecked((uint)-1)), ListViewItemStates.LVIS_FOCUSED);
        }

        public bool HasSelection()
        {
            return (0 != SendMessage(this.Handle, ListViewMessages.LVM_GETSELECTEDCOUNT, 0, 0));
        }

	}

}
