#region License
//////////////////////////////////////////////////////////////////////////
// RichTextBoxEx
// http://www.codeproject.com/KB/string/RTFBuilder.aspx
//
//  Release under CPOL License
//  http://www.codeproject.com/info/cpol10.aspx
//////////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace RichTextBoxLinks
{
	public class RichTextBoxEx : RichTextBox
	{
		#region Interop-Defines
		[ StructLayout( LayoutKind.Sequential )]
		private struct CHARFORMAT2_STRUCT
		{
			public UInt32	cbSize; 
			public UInt32   dwMask; 
			public UInt32   dwEffects; 
			public Int32    yHeight; 
			public Int32    yOffset; 
			public Int32	crTextColor; 
			public byte     bCharSet; 
			public byte     bPitchAndFamily; 
			[MarshalAs(UnmanagedType.ByValArray, SizeConst=32)]
			public char[]   szFaceName; 
			public UInt16	wWeight;
			public UInt16	sSpacing;
			public int		crBackColor; // Color.ToArgb() -> int
			public int		lcid;
			public int		dwReserved;
			public Int16	sStyle;
			public Int16	wKerning;
			public byte		bUnderlineType;
			public byte		bAnimation;
			public byte		bRevAuthor;
			public byte		bReserved1;
		}

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		private const int WM_USER			 = 0x0400;
		private const int EM_GETCHARFORMAT	 = WM_USER+58;
		private const int EM_SETCHARFORMAT	 = WM_USER+68;

		private const int SCF_SELECTION	= 0x0001;
		private const int SCF_WORD		= 0x0002;
		private const int SCF_ALL		= 0x0004;

		#region CHARFORMAT2 Flags
		private const UInt32 CFE_BOLD		= 0x0001;
		private const UInt32 CFE_ITALIC		= 0x0002;
		private const UInt32 CFE_UNDERLINE	= 0x0004;
		private const UInt32 CFE_STRIKEOUT	= 0x0008;
		private const UInt32 CFE_PROTECTED	= 0x0010;
		private const UInt32 CFE_LINK		= 0x0020;
		private const UInt32 CFE_AUTOCOLOR	= 0x40000000;
		private const UInt32 CFE_SUBSCRIPT	= 0x00010000;		/* Superscript and subscript are */
		private const UInt32 CFE_SUPERSCRIPT= 0x00020000;		/*  mutually exclusive			 */

		private const int CFM_SMALLCAPS		= 0x0040;			/* (*)	*/
		private const int CFM_ALLCAPS		= 0x0080;			/* Displayed by 3.0	*/
		private const int CFM_HIDDEN		= 0x0100;			/* Hidden by 3.0 */
		private const int CFM_OUTLINE		= 0x0200;			/* (*)	*/
		private const int CFM_SHADOW		= 0x0400;			/* (*)	*/
		private const int CFM_EMBOSS		= 0x0800;			/* (*)	*/
		private const int CFM_IMPRINT		= 0x1000;			/* (*)	*/
		private const int CFM_DISABLED		= 0x2000;
		private const int CFM_REVISED		= 0x4000;

		private const int CFM_BACKCOLOR		= 0x04000000;
		private const int CFM_LCID			= 0x02000000;
		private const int CFM_UNDERLINETYPE	= 0x00800000;		/* Many displayed by 3.0 */
		private const int CFM_WEIGHT		= 0x00400000;
		private const int CFM_SPACING		= 0x00200000;		/* Displayed by 3.0	*/
		private const int CFM_KERNING		= 0x00100000;		/* (*)	*/
		private const int CFM_STYLE			= 0x00080000;		/* (*)	*/
		private const int CFM_ANIMATION		= 0x00040000;		/* (*)	*/
		private const int CFM_REVAUTHOR		= 0x00008000;


		private const UInt32 CFM_BOLD		= 0x00000001;
		private const UInt32 CFM_ITALIC		= 0x00000002;
		private const UInt32 CFM_UNDERLINE	= 0x00000004;
		private const UInt32 CFM_STRIKEOUT	= 0x00000008;
		private const UInt32 CFM_PROTECTED	= 0x00000010;
		private const UInt32 CFM_LINK		= 0x00000020;
		private const UInt32 CFM_SIZE		= 0x80000000;
		private const UInt32 CFM_COLOR		= 0x40000000;
		private const UInt32 CFM_FACE		= 0x20000000;
		private const UInt32 CFM_OFFSET		= 0x10000000;
		private const UInt32 CFM_CHARSET	= 0x08000000;
		private const UInt32 CFM_SUBSCRIPT	= CFE_SUBSCRIPT | CFE_SUPERSCRIPT;
		private const UInt32 CFM_SUPERSCRIPT= CFM_SUBSCRIPT;

		private const byte CFU_UNDERLINENONE		= 0x00000000;
		private const byte CFU_UNDERLINE			= 0x00000001;
		private const byte CFU_UNDERLINEWORD		= 0x00000002; /* (*) displayed as ordinary underline	*/
		private const byte CFU_UNDERLINEDOUBLE		= 0x00000003; /* (*) displayed as ordinary underline	*/
		private const byte CFU_UNDERLINEDOTTED		= 0x00000004;
		private const byte CFU_UNDERLINEDASH		= 0x00000005;
		private const byte CFU_UNDERLINEDASHDOT		= 0x00000006;
		private const byte CFU_UNDERLINEDASHDOTDOT	= 0x00000007;
		private const byte CFU_UNDERLINEWAVE		= 0x00000008;
		private const byte CFU_UNDERLINETHICK		= 0x00000009;
		private const byte CFU_UNDERLINEHAIRLINE	= 0x0000000A; /* (*) displayed as ordinary underline	*/

		#endregion

		#endregion

		public RichTextBoxEx()
		{
			// Otherwise, non-standard links get lost when user starts typing
			// next to a non-standard link
			this.DetectUrls = false;
		}

		[DefaultValue(false)]
		public new bool DetectUrls
		{
			get { return base.DetectUrls; }
			set { base.DetectUrls = value; }
		}

		/// <summary>
		/// Insert a given text as a link into the RichTextBox at the current insert position.
		/// </summary>
		/// <param name="text">Text to be inserted</param>
		public void InsertLink(string text)
		{
			InsertLink(text, this.SelectionStart);
		}

		/// <summary>
		/// Insert a given text at a given position as a link. 
		/// </summary>
		/// <param name="text">Text to be inserted</param>
		/// <param name="position">Insert position</param>
		public void InsertLink(string text, int position)
		{
			if (position < 0 || position > this.Text.Length)
				throw new ArgumentOutOfRangeException("position");

			this.SelectionStart = position;
			this.SelectedText = text;
			this.Select(position, text.Length);
			this.SetSelectionLink(true);
			this.Select(position + text.Length, 0);
		}
		
		/// <summary>
		/// Insert a given text at at the current input position as a link.
		/// The link text is followed by a hash (#) and the given hyperlink text, both of
		/// them invisible.
		/// When clicked on, the whole link text and hyperlink string are given in the
		/// LinkClickedEventArgs.
		/// </summary>
		/// <param name="text">Text to be inserted</param>
		/// <param name="hyperlink">Invisible hyperlink string to be inserted</param>
		public void InsertLink(string text, string hyperlink)
		{
			InsertLink(text, hyperlink, this.SelectionStart);
		}

		/// <summary>
		/// Insert a given text at a given position as a link. The link text is followed by
		/// a hash (#) and the given hyperlink text, both of them invisible.
		/// When clicked on, the whole link text and hyperlink string are given in the
		/// LinkClickedEventArgs.
		/// </summary>
		/// <param name="text">Text to be inserted</param>
		/// <param name="hyperlink">Invisible hyperlink string to be inserted</param>
		/// <param name="position">Insert position</param>
		public void InsertLink(string text, string hyperlink, int position)
		{
			if (position < 0 || position > this.Text.Length)
				throw new ArgumentOutOfRangeException("position");

			this.SelectionStart = position;
			this.SelectedRtf = @"{\rtf1\ansi "+text+@"\v #"+hyperlink+@"\v0}";
			this.Select(position, text.Length + hyperlink.Length + 1);
			this.SetSelectionLink(true);
			this.Select(position + text.Length + hyperlink.Length + 1, 0);
		}

        /// <summary>
        /// Assign a link to the text at the given position
        /// </summary>
        /// <param name="hyperlink">link to assign</param>
        /// <param name="position"></param>
        /// <param name="length"></param>
        public void SetLink(string hyperlink, int position, int length)
        {
            var originalSelection = this.SelectionStart;
            this.SelectionStart = position;
            this.Select(position, length);
            string text = this.SelectedText;
            this.SelectedRtf = @"{\rtf1\ansi " + this.SelectedText + @"\v #" + hyperlink + @"\v0}";
            this.Select(position, text.Length + hyperlink.Length + 1);
            this.SetSelectionLink(true);
            this.Select(position + text.Length + hyperlink.Length + 1, 0);
            this.SelectionStart = originalSelection;
        }

		/// <summary>
		/// Set the current selection's link style
		/// </summary>
		/// <param name="link">true: set link style, false: clear link style</param>
		public void SetSelectionLink(bool link)
		{
			SetSelectionStyle(CFM_LINK, link ? CFE_LINK : 0);
		}
		/// <summary>
		/// Get the link style for the current selection
		/// </summary>
		/// <returns>0: link style not set, 1: link style set, -1: mixed</returns>
		public int GetSelectionLink()
		{
			return GetSelectionStyle(CFM_LINK, CFE_LINK);
		}


		private void SetSelectionStyle(UInt32 mask, UInt32 effect)
		{
			CHARFORMAT2_STRUCT cf = new CHARFORMAT2_STRUCT();
			cf.cbSize = (UInt32)Marshal.SizeOf(cf);
			cf.dwMask = mask;
			cf.dwEffects = effect;

			IntPtr wpar = new IntPtr(SCF_SELECTION);
			IntPtr lpar = Marshal.AllocCoTaskMem( Marshal.SizeOf( cf ) ); 
			Marshal.StructureToPtr(cf, lpar, false);

			IntPtr res = SendMessage(Handle, EM_SETCHARFORMAT, wpar, lpar);

			Marshal.FreeCoTaskMem(lpar);
		}

		private int GetSelectionStyle(UInt32 mask, UInt32 effect)
		{
			CHARFORMAT2_STRUCT cf = new CHARFORMAT2_STRUCT();
			cf.cbSize = (UInt32)Marshal.SizeOf(cf);
			cf.szFaceName = new char[32];

			IntPtr wpar = new IntPtr(SCF_SELECTION);
			IntPtr lpar = 	Marshal.AllocCoTaskMem( Marshal.SizeOf( cf ) ); 
			Marshal.StructureToPtr(cf, lpar, false);

			IntPtr res = SendMessage(Handle, EM_GETCHARFORMAT, wpar, lpar);

			cf = (CHARFORMAT2_STRUCT)Marshal.PtrToStructure(lpar, typeof(CHARFORMAT2_STRUCT));

			int state;
			// dwMask holds the information which properties are consistent throughout the selection:
			if ((cf.dwMask & mask) == mask) 
			{
				if ((cf.dwEffects & effect) == effect)
					state = 1;
				else
					state = 0;
			}
			else
			{
				state = -1;
			}
			
			Marshal.FreeCoTaskMem(lpar);
			return state;
		}


        public new string Rtf
        {
            get { return base.Rtf; }
            set { SetRtfText(value); }
        }

        public void BeginUpdate()
        {
            SendMessage(this.Handle, WM_SETREDRAW, (IntPtr)0, IntPtr.Zero);
        }
        public void EndUpdate()
        {
            SendMessage(this.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
        }
        private const int WM_SETREDRAW = 0x0b;
        
        static readonly System.Text.RegularExpressions.Regex linkRegex = new System.Text.RegularExpressions.Regex(
                @"\{\\field\s+\{\\\*\\fldinst\s+HYPERLINK\s+\u0022?(?<link>[^\u0022}]+)\u0022?\s*\}\s*\{\\fldrslt\s+(?<name>[^}]+)\s*\}\s*\}"
                );
        private void SetRtfText(string value)
        {

            try
            {
                this.BeginUpdate();
                var sb = new System.Text.StringBuilder(value);
                List<LinkMatch> matches = new List<LinkMatch>();
                int idx = 0;
                while (true)
                {
                    var match = linkRegex.Match(sb.ToString(), idx);
                    if (!match.Success) break;
                    var link = match.Groups["link"];
                    var name = match.Groups["name"];
                    var m2 = new LinkMatch(link.Value, name.Value, match.Index, name.Length);
                    sb.Remove(match.Index, match.Length);
                    string linkText = string.Format("#:@{0}@:#", matches.Count.ToString());
                    sb.Insert(match.Index, linkText);
                    matches.Add(m2);
                    idx = match.Index + linkText.Length;
                }
                base.Rtf = sb.ToString();
                idx = 0;
                while (true)
                {
                    int start = base.Find("#:@", idx, RichTextBoxFinds.NoHighlight);
                    if (start == -1) break;
                    int end = base.Find("@:#", start + 3, RichTextBoxFinds.NoHighlight);
                    if (end == -1) break; // Error?

                    base.Select(start, end - start + 3);
                    string text = base.SelectedText;
                    int link = int.Parse(text.Substring(3, text.Length - 6));
                    var match = matches[link];
                    base.SelectedText = "";
                    this.InsertLink(match.name, match.link, start);
                }
                base.SelectionStart = 0;
            }
            catch
            {
                base.Rtf = value;
            }
            finally
            {
                try
                {
                    this.EndUpdate();
                    this.Invalidate();
                    this.Update();
                }
                catch {}
            }
        }

        private struct LinkMatch
        {
            public LinkMatch(string link, string name, int index, int length)
            {
                this.link = link;
                this.name = name;
                this.index = index;
                this.length = length;
            }
            public readonly string link;
            public readonly string name;
            public readonly int index;
            public readonly int length;
        }

	}
}
