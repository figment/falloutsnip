using System;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TESsnip.Windows.Controls
{
	/// <summary>
	/// BallonToolTip
	/// </summary>
	/// <remarks>Original author: Eric Wilson</remarks>
	[
	ProvideProperty("ToolTip",typeof(Control)),
	ProvideProperty("TipTitle",typeof(Control)),
	ProvideProperty("IconType", typeof(Control)),
	ToolboxBitmap(typeof(System.Windows.Forms.ToolTip))
	]
	public class BallonToolTip : Component,IExtenderProvider
	{
        #region class NativeToolTipWindow
        /// <summary>
        /// Native ToolTip window for use with the BallonToolTip component.
        /// </summary>
        internal class NativeToolTipWindow : NativeWindow
        {
            private BallonToolTip m_toolTip;

            /// <summary>
            /// Initializes a new instance of the NativeToolTipWindow class.
            /// </summary>
            public NativeToolTipWindow(BallonToolTip toolTip)
            {
                m_toolTip = toolTip;
            }

            protected override void WndProc(ref Message m)
            {
                m_toolTip.WndProc(ref m);
            }
        }
        #endregion

		// Reference to the Native win32 window
		private NativeToolTipWindow m_window;

		// The keys are Control references and the values are the ToolTipInfo.
		private Hashtable m_controls;

		private ArrayList m_addedList;

		private ToolTipStyle m_style;

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the BallonToolTip class.
		/// </summary>
		public BallonToolTip()
		{
			m_window = new NativeToolTipWindow(this);
			m_controls = new Hashtable();
			m_addedList = new ArrayList();
			m_style = ToolTipStyle.Balloon;
		}

		/// <summary>
		/// Initializes a new instance of the BallonToolTip class.
		/// </summary>
		public BallonToolTip(System.ComponentModel.IContainer container) : this()
		{
			container.Add(this);
		}

		#endregion

		#region Dispose Override 

		/// <summary>
		/// Releases the unmanaged resources used by the Component. 
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (m_window != null && m_window.Handle != IntPtr.Zero)
			{
				m_window.DestroyHandle();
			}
			base.Dispose (disposing);
		}

		#endregion

		#region Public Properties/Methods

		#region ToolTip

		/// <summary>
		/// Retrieves the ToolTip text associated with the specified control.
		/// </summary>
		/// <param name="control">The Control for which to retrieves the ToolTip text</param>
		/// <returns>The ToolTip text for the specified control</returns>
		[DefaultValue("")]
		[Editor(typeof(GWinfoMultilineEditor),typeof(UITypeEditor))]
		public virtual string GetToolTip(Control control)
		{
			string text = "";
			if (this.m_controls.Contains(control))
			{
				text = ((ToolTipInfo)this.m_controls[control]).TipText;
			}
			return text;
		}

		/// <summary>
		/// Associates ToolTip text with the specified control.
		/// </summary>
		/// <param name="control">The Control whose ToolTip text you want to set</param>
		/// <param name="text">The ToolTip text</param>
		public virtual void SetToolTip(Control control, string text)
		{
			bool bNewControl,bValidText;

			if (control == null)
			{
				throw new ArgumentNullException("control");
			}

			bNewControl = !m_controls.ContainsKey(control);
			bValidText = (text != null && text.Length > 0);

			if (bNewControl)
			{
				if (bValidText)
				{
					ToolTipInfo info = new ToolTipInfo(text,"",ToolTipIconType.None);
					m_controls.Add(control,info);
					control.HandleCreated += new EventHandler(HandleCreated);
					control.HandleDestroyed += new EventHandler(HandleDestroyed);
					if (control.IsHandleCreated)
					{
						this.HandleCreated(control,EventArgs.Empty);
					}
				}
			}
			else
			{
				ToolTipInfo info = (ToolTipInfo)m_controls[control];
				info.TipText = text;
				if (info.TipText.Length == 0 && info.TipTitle.Length == 0 && info.IconType == ToolTipIconType.None)
				{
					// Remove the control.
					m_controls.Remove(control);
					// unhook events
					control.HandleCreated -= new EventHandler(HandleCreated);
					control.HandleDestroyed -= new EventHandler(HandleDestroyed);
					
					if (m_addedList.Contains(control))
					{
						DestroyRegion(control);
						m_addedList.Remove(control);
					}
				}
			}
		}

		#endregion

		#region TipTitle Ù–‘

		[DefaultValue("")]
		public virtual string GetTipTitle(Control control)
		{
			string text = "";
			if (this.m_controls.Contains(control))
			{
				text = ((ToolTipInfo)this.m_controls[control]).TipTitle;
			}
			return text;
		}

		public virtual void SetTipTitle(Control control,string title)
		{
			bool bNewControl,bValidText;

			if (control == null)
			{
				throw new ArgumentNullException("control");
			}

			bNewControl = !m_controls.ContainsKey(control);
			bValidText = (title != null && title.Length > 0);

			if (bNewControl)
			{
				if (bValidText)
				{
					ToolTipInfo info = new ToolTipInfo("",title,ToolTipIconType.None);
					m_controls.Add(control,info);
					control.HandleCreated += new EventHandler(HandleCreated);
					control.HandleDestroyed += new EventHandler(HandleDestroyed);
					if (control.IsHandleCreated)
					{
						this.HandleCreated(control,EventArgs.Empty);
					}
				}
			}
			else
			{
				ToolTipInfo info = (ToolTipInfo)m_controls[control];
				info.TipTitle = title;
				if (info.TipText.Length == 0 && info.TipTitle.Length == 0 && info.IconType == ToolTipIconType.None)
				{
					// Remove the control.
					m_controls.Remove(control);
					// unhook events
					control.HandleCreated -= new EventHandler(HandleCreated);
					control.HandleDestroyed -= new EventHandler(HandleDestroyed);
					
					if (m_addedList.Contains(control))
					{
						DestroyRegion(control);
						m_addedList.Remove(control);
					}
				}
			}
		}

		#endregion

		#region IconType Ù–‘

		[DefaultValue(ToolTipIconType.None)]
		public virtual ToolTipIconType GetIconType(Control control)
		{
			ToolTipIconType type = ToolTipIconType.None;
			if (this.m_controls.Contains(control))
			{
				type = ((ToolTipInfo)this.m_controls[control]).IconType;
			}
			return type;
		}

		public virtual void SetIconType(Control control,ToolTipIconType type)
		{
			bool bNewControl;

			if (control == null)
			{
				throw new ArgumentNullException("control");
			}

			bNewControl = !m_controls.ContainsKey(control);

			if (bNewControl)
			{
				if (type != ToolTipIconType.None)
				{
					ToolTipInfo info = new ToolTipInfo("","",type);
					m_controls.Add(control,info);
					control.HandleCreated += new EventHandler(HandleCreated);
					control.HandleDestroyed += new EventHandler(HandleDestroyed);
					if (control.IsHandleCreated)
					{
						this.HandleCreated(control,EventArgs.Empty);
					}
				}
			}
			else
			{
				ToolTipInfo info = (ToolTipInfo)m_controls[control];
				info.IconType = type;
				if (info.TipText.Length == 0 && info.TipTitle.Length == 0 && info.IconType == ToolTipIconType.None)
				{
					// Remove the control.
					m_controls.Remove(control);
					// unhook events
					control.HandleCreated -= new EventHandler(HandleCreated);
					control.HandleDestroyed -= new EventHandler(HandleDestroyed);
					
					if (m_addedList.Contains(control))
					{
						DestroyRegion(control);
						m_addedList.Remove(control);
					}
				}
			}
		}

		#endregion

		#endregion

		#region WndProc Method

		internal virtual void WndProc(ref Message m)
		{
			if (m.Msg == NativeMethods.WM_NOTIFY)
			{
				NativeMethods.NMHDR hdr = (NativeMethods.NMHDR)Marshal.PtrToStructure(m.LParam,typeof(NativeMethods.NMHDR));

				if (hdr.code == NativeMethods.TTN_NEEDTEXT)
				{
					Control control = Control.FromHandle(new IntPtr(hdr.idFrom));
					if (control != null)
					{
						string text = GetToolTip(control);
						string title = GetTipTitle(control);
						int icon = (int)GetIconType(control);
						NativeMethods.SendMessage(new HandleRef(this,this.Handle),NativeMethods.TTM_SETTITLE,icon,title);

						NativeMethods.NMTTDISPINFO info = (NativeMethods.NMTTDISPINFO)Marshal.PtrToStructure(m.LParam,typeof(NativeMethods.NMTTDISPINFO));
						info.lpszText = text;
						Marshal.StructureToPtr(info,m.LParam,true);
					}
					return;
				}
			}
			this.m_window.DefWndProc(ref m);
		}

		#endregion

		#region IExtenderProvider Members

		public bool CanExtend(object extendee)
		{
			if (extendee is Control)
			{
				return true;
			}
			return false;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Registers the control with the tooltip window when its handle is created.
		/// </summary>
		private void HandleCreated(object sender, EventArgs e)
		{
			this.CreateRegion((Control) sender);
		}

		private void HandleDestroyed(object sender, EventArgs e)
		{
			this.DestroyRegion((Control) sender);
		}

		/// <summary>
		/// Registers the specified control with the tooltip window.
		/// </summary>
		/// <param name="ctl"></param>
		private void CreateRegion(Control ctl)
		{
			if (!m_controls.ContainsKey(ctl))
			{
				return;
			}
			ToolTipInfo info = (ToolTipInfo)this.m_controls[ctl];

			bool flag = info.TipText.Length > 0 || info.TipTitle.Length > 0 || info.IconType != ToolTipIconType.None;
			if (flag && !this.m_addedList.Contains(ctl) && !base.DesignMode)
			{
				int num1 = (int) NativeMethods.SendMessage(new HandleRef(this, this.Handle), NativeMethods.TTM_ADDTOOL, 0, this.GetTOOLINFO(ctl));
				if (num1 == 0)
				{
					throw new InvalidOperationException("error:" + NativeMethods.GetLastError());
				}
				this.m_addedList.Add(ctl);
			}
		}

		private void DestroyRegion(Control control)
		{
			bool flag = control.IsHandleCreated && this.IsHandleCreated;
			if (this.m_addedList.Contains(control) && flag && !base.DesignMode)
			{
				NativeMethods.SendMessage(new HandleRef(this, this.Handle), NativeMethods.TTM_DELTOOL, 0, this.GetTOOLINFO(control));
				this.m_addedList.Remove(control);
			}
		}

		/// <summary>
		/// Creates and initializes a NativeMethods.TOOLINFO structure associate width the specified control
		/// </summary>
		private NativeMethods.TOOLINFO GetTOOLINFO(Control ctl)
		{
			NativeMethods.TOOLINFO toolinfo = new NativeMethods.TOOLINFO();
			toolinfo.cbSize = Marshal.SizeOf(typeof(NativeMethods.TOOLINFO));
			toolinfo.hwnd = this.Handle;
			toolinfo.uFlags = NativeMethods.TTF_TRANSPARENT | NativeMethods.TTF_SUBCLASS | NativeMethods.TTF_IDISHWND;
			toolinfo.uId = ctl.Handle;
			toolinfo.lpszText = NativeMethods.LPSTR_TEXTCALLBACKW;
			return toolinfo;
		}

		/// <summary>
		/// Creates the ToolTip window handle.
		/// </summary>
		private void CreateHandle()
		{
			if (!this.IsHandleCreated)
			{
				NativeMethods.INITCOMMONCONTROLSEX initcommoncontrolsex1 = new NativeMethods.INITCOMMONCONTROLSEX();
				initcommoncontrolsex1.dwICC = 8; // ICC_TAB_CLASSES : Load tab and ToolTip control classes. 
				NativeMethods.InitCommonControlsEx(initcommoncontrolsex1);
				this.m_window.CreateHandle(this.CreateParams);
				NativeMethods.SetWindowPos(new HandleRef(this, this.Handle), NativeMethods.HWND_TOPMOST, 0, 0, 0, 0, NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE);
				NativeMethods.SendMessage(new HandleRef(this, this.Handle), NativeMethods.TTM_SETMAXTIPWIDTH, 0, SystemInformation.MaxWindowTrackSize.Width);
				NativeMethods.SendMessage(new HandleRef(this, this.Handle), NativeMethods.TTM_ACTIVATE, 1,0);
			}
		}

		private System.Windows.Forms.CreateParams CreateParams
		{
			get
			{
				System.Windows.Forms.CreateParams params1 = new System.Windows.Forms.CreateParams();
				params1.Parent = IntPtr.Zero;
				params1.ClassName = NativeMethods.TOOLTIPS_CLASS;
				params1.Style |= NativeMethods.TTS_ALWAYSTIP;
				if (this.m_style == ToolTipStyle.Balloon)
				{
					params1.Style |= NativeMethods.TTS_BALLOON;
				}

				params1.ExStyle = 0;
				params1.Caption = null;
				return params1;
			}
		}

		private bool IsHandleCreated
		{
			get
			{
				return (this.m_window.Handle != IntPtr.Zero);
			}
		}

		private IntPtr Handle
		{
			get
			{
				if (!this.IsHandleCreated)
				{
					this.CreateHandle();
				}
				return this.m_window.Handle;
			}
		}

		#endregion
	}

	/// <summary>
	/// IconType
	/// </summary>
	public enum ToolTipIconType : int
	{
		None = 0,
		Information,
		Warning,
		Error
	}

	/// <summary>
	/// ToolTipStyle
	/// </summary>
	public enum ToolTipStyle
	{
		Standard,
		Balloon
	}

	#region class ToolTipInfo
	/// <summary>
	/// ToolTipInfo
	/// </summary>
	internal class ToolTipInfo
	{
		private string m_tooltip;
		private string m_title;
		private ToolTipIconType m_iconType;

		/// <summary>
		/// Initializes a new instance of the ToolTipInfo class.
		/// </summary>
		public ToolTipInfo() : this("","",ToolTipIconType.None)
		{

		}

		/// <summary>
		/// Initializes a new instance of the ToolTipInfo class.
		/// </summary>
		/// <param name="text">ToolTip text</param>
		/// <param name="title">ToolTip title</param>
		/// <param name="icon">ToolTip icon type</param>
		public ToolTipInfo(string text,string title,ToolTipIconType icon)
		{
			m_tooltip = text;
			m_title = title;
			m_iconType = icon;
		}

		/// <summary>
		/// Gets/sets the tooltip text.
		/// </summary>
		public string TipText
		{
			get
			{
				return m_tooltip;
			}
			set
			{
				m_tooltip = value;
			}
		}

		/// <summary>
		/// Gets/sets the tooltip title.
		/// </summary>
		public string TipTitle
		{
			get
			{
				return m_title;
			}
			set
			{
				m_title = value;
			}
		}

		/// <summary>
		/// Gets/sets the icon type.
		/// </summary>
		public ToolTipIconType IconType
		{
			get
			{
				return m_iconType;
			}
			set
			{
				m_iconType = value;
			}
		}
	}	
	#endregion

	#region class GWinfoMultilineEditor
	internal class GWinfoMultilineEditor : UITypeEditor
	{
		public override System.Drawing.Design.UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}

		public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, System.IServiceProvider provider, object value)
		{
			if (provider == null)
				return value;

			IWindowsFormsEditorService edSvc = provider.GetService( typeof(IWindowsFormsEditorService) ) as IWindowsFormsEditorService;

			if (edSvc == null)
				return value;

			MultilineTextBox textBox = new MultilineTextBox();
			textBox.BorderStyle = BorderStyle.None;
			textBox.Size = new System.Drawing.Size(150,80);
			textBox.Text = value == null ? string.Empty : value.ToString();

			edSvc.DropDownControl(textBox);

			if (!textBox.Cancelled)
				value = textBox.Text;

			textBox.Dispose();

			return value;
		}

		private class MultilineTextBox : TextBox
		{
			private bool cancelled = false;

			internal MultilineTextBox()
			{
				this.Multiline = true;
				this.AcceptsTab = true;
			}

			internal bool Cancelled
			{
				get { return this.cancelled; }
			}
			
			protected override bool ProcessDialogKey(System.Windows.Forms.Keys keyData)
			{
				if (keyData == Keys.Escape)
					this.cancelled = true;
				return base.ProcessDialogKey(keyData);
			}
		}
	}
	#endregion
}
