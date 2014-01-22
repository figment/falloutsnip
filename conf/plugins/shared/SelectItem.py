
import System.Drawing
import System.Windows.Forms

from System.Drawing import *
from System.Windows.Forms import *

class SelectItem(Form):
	def __init__(self):
		self.InitializeComponent()
		self._btnOk.Enabled = False
	
	def InitializeComponent(self):
		self._listBox1 = System.Windows.Forms.ListBox()
		self._lblHeader = System.Windows.Forms.Label()
		self._btnOk = System.Windows.Forms.Button()
		self._btnCancel = System.Windows.Forms.Button()
		self.SuspendLayout()
		# 
		# listBox1
		# 
		self._listBox1.FormattingEnabled = True
		self._listBox1.Location = System.Drawing.Point(12, 35)
		self._listBox1.Name = "listBox1"
		self._listBox1.Size = System.Drawing.Size(201, 225)
		self._listBox1.TabIndex = 7
		self._listBox1.SelectedIndexChanged += self.ListBox1SelectedIndexChanged
		# 
		# lblHeader
		# 
		self._lblHeader.Location = System.Drawing.Point(12, 9)
		self._lblHeader.Name = "lblHeader"
		self._lblHeader.Size = System.Drawing.Size(201, 23)
		self._lblHeader.TabIndex = 6
		# 
		# btnOk
		# 
		self._btnOk.Anchor = System.Windows.Forms.AnchorStyles.Top
		self._btnOk.Location = System.Drawing.Point(24, 276)
		self._btnOk.Name = "btnOk"
		self._btnOk.Size = System.Drawing.Size(75, 23)
		self._btnOk.TabIndex = 8
		self._btnOk.Text = "OK"
		self._btnOk.UseVisualStyleBackColor = True
		self._btnOk.Click += self.BtnOkClick
		# 
		# btnCancel
		# 
		self._btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Top
		self._btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
		self._btnCancel.Location = System.Drawing.Point(125, 276)
		self._btnCancel.Name = "btnCancel"
		self._btnCancel.Size = System.Drawing.Size(75, 23)
		self._btnCancel.TabIndex = 9
		self._btnCancel.Text = "Cancel"
		self._btnCancel.UseVisualStyleBackColor = True
		self._btnCancel.Click += self.BtnCancelClick
		# 
		# SelectItem
		# 
		self.AcceptButton = self._btnOk
		self.CancelButton = self._btnCancel
		self.ClientSize = System.Drawing.Size(225, 311)
		self.Controls.Add(self._btnOk)
		self.Controls.Add(self._btnCancel)
		self.Controls.Add(self._listBox1)
		self.Controls.Add(self._lblHeader)
		self.MaximizeBox = False
		self.MinimizeBox = False
		self.Name = "SelectItem"
		self.ShowIcon = False
		self.ShowInTaskbar = False
		self.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
		self.Text = "Select Item"
		self.ResumeLayout(False)

	def SetLabel(self, text):
		self._lblHeader.Text = text
		
	def SetItems(self, items):
		for item in items:
			self._listBox1.Items.Add(item)
			
	def GetSelectedItem(self):
		return self._listBox1.SelectedItem

	def BtnOkClick(self, sender, e):
		self.DialogResult = DialogResult.OK
		self.Close()

	def BtnCancelClick(self, sender, e):
		self.DialogResult = DialogResult.Cancel
		self.Close()

	def ListBox1SelectedIndexChanged(self, sender, e):
		item = self._listBox1.SelectedItem
		if item:
			self._btnOk.Enabled = True
		else:
			self._btnOk.Enabled = False
		pass