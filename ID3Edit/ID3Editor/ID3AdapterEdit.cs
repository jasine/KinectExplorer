// Copyright(C) 2002-2003 Hugo Rumayor Montemayor, All rights reserved.
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using Id3Lib;
using Mp3Lib;
using Utils;

namespace TagEditor
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class ID3AdapterEdit : System.Windows.Forms.Form
    {
        #region fields

        private IContainer components;
		private System.Windows.Forms.Button _buttonOK;
        private System.Windows.Forms.Button _buttonCancel;
		private System.Windows.Forms.ErrorProvider _errorProvider;
        private System.Windows.Forms.OpenFileDialog _openFileDialog;
        private BindingSource _tagHandlerBindingSource;
        private BindingSource _mp3FileBindingSource;

        #endregion

        /// <summary>
        /// MP3.File reference
        /// encapsulates both the TagModel and the audio information
        /// </summary>
        private Mp3File _mp3File = null;
        private ToolTip toolTip1;
        private TabControl _tabControlLyrics;
        private TabPage _tabPageGeneric;
        private Button _addPicture;
        private Button _removePicture;
        private PictureBox _artPictureBox;
        private ComboBox _comboBoxGenre;
        private Label _labelGenre;
        private TextBox _textBoxYear;
        private TextBox _textBoxAlbum;
        private TextBox _textBoxArtist;
        private TextBox _textBoxSampleRate;
        private TextBox _textBoxKBitRate;
        private TextBox _textBoxDiscNo;
        private TextBox _textBoxTrackNo;
        private TextBox _textBoxTitle;
        private Label _labelYear;
        private Label _labelAlbum;
        private Label _labelArtist;
        private Label label10;
        private Label label4;
        private Label label9;
        private Label label3;
        private Label _labelDiscNo;
        private Label _labelTrackNo;
        private Label _labelTitle;

        /// <summary>
		/// Tag Handler reference
        /// encapsulates TagModel
		/// </summary>
        private TagHandler _tagHandler = null;


        public ID3AdapterEdit(Mp3File mp3File)
		{
            _mp3File = mp3File;
            _tagHandler = new TagHandler(_mp3File.TagModel);

			InitializeComponent();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ID3AdapterEdit));
            this._mp3FileBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this._tagHandlerBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this._errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this._buttonOK = new System.Windows.Forms.Button();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this._tabPageGeneric = new System.Windows.Forms.TabPage();
            this._labelTitle = new System.Windows.Forms.Label();
            this._textBoxTitle = new System.Windows.Forms.TextBox();
            this._labelTrackNo = new System.Windows.Forms.Label();
            this._labelDiscNo = new System.Windows.Forms.Label();
            this._textBoxTrackNo = new System.Windows.Forms.TextBox();
            this._textBoxDiscNo = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this._textBoxKBitRate = new System.Windows.Forms.TextBox();
            this._textBoxSampleRate = new System.Windows.Forms.TextBox();
            this._textBoxArtist = new System.Windows.Forms.TextBox();
            this._textBoxAlbum = new System.Windows.Forms.TextBox();
            this._labelArtist = new System.Windows.Forms.Label();
            this._labelAlbum = new System.Windows.Forms.Label();
            this._labelYear = new System.Windows.Forms.Label();
            this._textBoxYear = new System.Windows.Forms.TextBox();
            this._labelGenre = new System.Windows.Forms.Label();
            this._comboBoxGenre = new System.Windows.Forms.ComboBox();
            this._artPictureBox = new System.Windows.Forms.PictureBox();
            this._removePicture = new System.Windows.Forms.Button();
            this._addPicture = new System.Windows.Forms.Button();
            this._tabControlLyrics = new System.Windows.Forms.TabControl();
            ((System.ComponentModel.ISupportInitialize)(this._mp3FileBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._tagHandlerBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider)).BeginInit();
            this._tabPageGeneric.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._artPictureBox)).BeginInit();
            this._tabControlLyrics.SuspendLayout();
            this.SuspendLayout();
            // 
            // _mp3FileBindingSource
            // 
            this._mp3FileBindingSource.DataSource = typeof(Mp3Lib.Mp3File);
            // 
            // _tagHandlerBindingSource
            // 
            this._tagHandlerBindingSource.DataSource = typeof(Id3Lib.TagHandler);
            // 
            // _errorProvider
            // 
            this._errorProvider.ContainerControl = this;
            this._errorProvider.DataMember = "";
            // 
            // _buttonOK
            // 
            this._buttonOK.Location = new System.Drawing.Point(214, 282);
            this._buttonOK.Name = "_buttonOK";
            this._buttonOK.Size = new System.Drawing.Size(72, 22);
            this._buttonOK.TabIndex = 1;
            this._buttonOK.Text = "确定";
            this._buttonOK.Click += new System.EventHandler(this.OnOkClick);
            // 
            // _buttonCancel
            // 
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.Location = new System.Drawing.Point(294, 282);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(72, 22);
            this._buttonCancel.TabIndex = 2;
            this._buttonCancel.Text = "取消";
            this._buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // _tabPageGeneric
            // 
            this._tabPageGeneric.AutoScroll = true;
            this._tabPageGeneric.Controls.Add(this._addPicture);
            this._tabPageGeneric.Controls.Add(this._removePicture);
            this._tabPageGeneric.Controls.Add(this._artPictureBox);
            this._tabPageGeneric.Controls.Add(this._comboBoxGenre);
            this._tabPageGeneric.Controls.Add(this._labelGenre);
            this._tabPageGeneric.Controls.Add(this._textBoxYear);
            this._tabPageGeneric.Controls.Add(this._textBoxAlbum);
            this._tabPageGeneric.Controls.Add(this._textBoxArtist);
            this._tabPageGeneric.Controls.Add(this._textBoxSampleRate);
            this._tabPageGeneric.Controls.Add(this._textBoxKBitRate);
            this._tabPageGeneric.Controls.Add(this._textBoxDiscNo);
            this._tabPageGeneric.Controls.Add(this._textBoxTrackNo);
            this._tabPageGeneric.Controls.Add(this._textBoxTitle);
            this._tabPageGeneric.Controls.Add(this._labelYear);
            this._tabPageGeneric.Controls.Add(this._labelAlbum);
            this._tabPageGeneric.Controls.Add(this._labelArtist);
            this._tabPageGeneric.Controls.Add(this.label10);
            this._tabPageGeneric.Controls.Add(this.label4);
            this._tabPageGeneric.Controls.Add(this.label9);
            this._tabPageGeneric.Controls.Add(this.label3);
            this._tabPageGeneric.Controls.Add(this._labelDiscNo);
            this._tabPageGeneric.Controls.Add(this._labelTrackNo);
            this._tabPageGeneric.Controls.Add(this._labelTitle);
            this._tabPageGeneric.Location = new System.Drawing.Point(4, 22);
            this._tabPageGeneric.Name = "_tabPageGeneric";
            this._tabPageGeneric.Size = new System.Drawing.Size(544, 243);
            this._tabPageGeneric.TabIndex = 0;
            this._tabPageGeneric.Text = "MP3信息";
            this._tabPageGeneric.Click += new System.EventHandler(this._tabPageGeneric_Click);
            // 
            // _labelTitle
            // 
            this._labelTitle.Location = new System.Drawing.Point(9, 12);
            this._labelTitle.Name = "_labelTitle";
            this._labelTitle.Size = new System.Drawing.Size(56, 15);
            this._labelTitle.TabIndex = 0;
            this._labelTitle.Text = "Title:";
            this._labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _textBoxTitle
            // 
            this._textBoxTitle.DataBindings.Add(new System.Windows.Forms.Binding("Text", this._tagHandlerBindingSource, "Title", true));
            this._textBoxTitle.Location = new System.Drawing.Point(71, 6);
            this._textBoxTitle.Name = "_textBoxTitle";
            this._textBoxTitle.Size = new System.Drawing.Size(214, 21);
            this._textBoxTitle.TabIndex = 1;
            // 
            // _labelTrackNo
            // 
            this._labelTrackNo.Location = new System.Drawing.Point(171, 143);
            this._labelTrackNo.Name = "_labelTrackNo";
            this._labelTrackNo.Size = new System.Drawing.Size(68, 15);
            this._labelTrackNo.TabIndex = 2;
            this._labelTrackNo.Text = "Track #:";
            this._labelTrackNo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this._labelTrackNo.Click += new System.EventHandler(this._labelTrackNo_Click);
            // 
            // _labelDiscNo
            // 
            this._labelDiscNo.Location = new System.Drawing.Point(100, 143);
            this._labelDiscNo.Name = "_labelDiscNo";
            this._labelDiscNo.Size = new System.Drawing.Size(46, 15);
            this._labelDiscNo.TabIndex = 2;
            this._labelDiscNo.Text = "Disc #:";
            this._labelDiscNo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _textBoxTrackNo
            // 
            this._textBoxTrackNo.DataBindings.Add(new System.Windows.Forms.Binding("Text", this._tagHandlerBindingSource, "Track", true));
            this._textBoxTrackNo.Location = new System.Drawing.Point(245, 141);
            this._textBoxTrackNo.Name = "_textBoxTrackNo";
            this._textBoxTrackNo.Size = new System.Drawing.Size(40, 21);
            this._textBoxTrackNo.TabIndex = 3;
            // 
            // _textBoxDiscNo
            // 
            this._textBoxDiscNo.DataBindings.Add(new System.Windows.Forms.Binding("Text", this._tagHandlerBindingSource, "Disc", true));
            this._textBoxDiscNo.Location = new System.Drawing.Point(147, 141);
            this._textBoxDiscNo.Name = "_textBoxDiscNo";
            this._textBoxDiscNo.Size = new System.Drawing.Size(40, 21);
            this._textBoxDiscNo.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(-2, 206);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 16);
            this.label3.TabIndex = 2;
            this.label3.Text = "采样率:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(-2, 186);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(75, 16);
            this.label9.TabIndex = 2;
            this.label9.Text = "比特率:";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(139, 206);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(34, 16);
            this.label4.TabIndex = 2;
            this.label4.Text = "kHz";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(139, 186);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(34, 16);
            this.label10.TabIndex = 2;
            this.label10.Text = "kbit/s";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _textBoxKBitRate
            // 
            this._textBoxKBitRate.Location = new System.Drawing.Point(74, 185);
            this._textBoxKBitRate.Name = "_textBoxKBitRate";
            this._textBoxKBitRate.ReadOnly = true;
            this._textBoxKBitRate.Size = new System.Drawing.Size(60, 21);
            this._textBoxKBitRate.TabIndex = 3;
            this._textBoxKBitRate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // _textBoxSampleRate
            // 
            this._textBoxSampleRate.Location = new System.Drawing.Point(74, 205);
            this._textBoxSampleRate.Name = "_textBoxSampleRate";
            this._textBoxSampleRate.ReadOnly = true;
            this._textBoxSampleRate.Size = new System.Drawing.Size(60, 21);
            this._textBoxSampleRate.TabIndex = 3;
            this._textBoxSampleRate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // _textBoxArtist
            // 
            this._textBoxArtist.DataBindings.Add(new System.Windows.Forms.Binding("Text", this._tagHandlerBindingSource, "Artist", true));
            this._textBoxArtist.Location = new System.Drawing.Point(71, 43);
            this._textBoxArtist.Name = "_textBoxArtist";
            this._textBoxArtist.Size = new System.Drawing.Size(214, 21);
            this._textBoxArtist.TabIndex = 4;
            // 
            // _textBoxAlbum
            // 
            this._textBoxAlbum.DataBindings.Add(new System.Windows.Forms.Binding("Text", this._tagHandlerBindingSource, "Album", true));
            this._textBoxAlbum.Location = new System.Drawing.Point(71, 76);
            this._textBoxAlbum.Name = "_textBoxAlbum";
            this._textBoxAlbum.Size = new System.Drawing.Size(214, 21);
            this._textBoxAlbum.TabIndex = 5;
            // 
            // _labelArtist
            // 
            this._labelArtist.Location = new System.Drawing.Point(8, 45);
            this._labelArtist.Name = "_labelArtist";
            this._labelArtist.Size = new System.Drawing.Size(56, 15);
            this._labelArtist.TabIndex = 6;
            this._labelArtist.Text = "Artist:";
            this._labelArtist.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _labelAlbum
            // 
            this._labelAlbum.Location = new System.Drawing.Point(8, 78);
            this._labelAlbum.Name = "_labelAlbum";
            this._labelAlbum.Size = new System.Drawing.Size(56, 15);
            this._labelAlbum.TabIndex = 7;
            this._labelAlbum.Text = "Album:";
            this._labelAlbum.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _labelYear
            // 
            this._labelYear.Location = new System.Drawing.Point(22, 143);
            this._labelYear.Name = "_labelYear";
            this._labelYear.Size = new System.Drawing.Size(32, 15);
            this._labelYear.TabIndex = 8;
            this._labelYear.Text = "Year:";
            this._labelYear.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _textBoxYear
            // 
            this._textBoxYear.DataBindings.Add(new System.Windows.Forms.Binding("Text", this._tagHandlerBindingSource, "Year", true));
            this._textBoxYear.Location = new System.Drawing.Point(55, 141);
            this._textBoxYear.Name = "_textBoxYear";
            this._textBoxYear.Size = new System.Drawing.Size(44, 21);
            this._textBoxYear.TabIndex = 9;
            // 
            // _labelGenre
            // 
            this._labelGenre.Location = new System.Drawing.Point(8, 111);
            this._labelGenre.Name = "_labelGenre";
            this._labelGenre.Size = new System.Drawing.Size(56, 15);
            this._labelGenre.TabIndex = 10;
            this._labelGenre.Text = "Genre:";
            this._labelGenre.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _comboBoxGenre
            // 
            this._comboBoxGenre.DataBindings.Add(new System.Windows.Forms.Binding("Text", this._tagHandlerBindingSource, "Genre", true));
            this._comboBoxGenre.Location = new System.Drawing.Point(71, 109);
            this._comboBoxGenre.Name = "_comboBoxGenre";
            this._comboBoxGenre.Size = new System.Drawing.Size(214, 20);
            this._comboBoxGenre.TabIndex = 11;
            // 
            // _artPictureBox
            // 
            this._artPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._artPictureBox.DataBindings.Add(new System.Windows.Forms.Binding("Image", this._tagHandlerBindingSource, "Picture", true));
            this._artPictureBox.Location = new System.Drawing.Point(314, 6);
            this._artPictureBox.Name = "_artPictureBox";
            this._artPictureBox.Size = new System.Drawing.Size(198, 185);
            this._artPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this._artPictureBox.TabIndex = 12;
            this._artPictureBox.TabStop = false;
            // 
            // _removePicture
            // 
            this._removePicture.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._removePicture.Location = new System.Drawing.Point(462, 204);
            this._removePicture.Name = "_removePicture";
            this._removePicture.Size = new System.Drawing.Size(22, 21);
            this._removePicture.TabIndex = 13;
            this._removePicture.Text = "X";
            this._removePicture.Click += new System.EventHandler(this.removePicture_Click);
            // 
            // _addPicture
            // 
            this._addPicture.Location = new System.Drawing.Point(344, 204);
            this._addPicture.Name = "_addPicture";
            this._addPicture.Size = new System.Drawing.Size(91, 21);
            this._addPicture.TabIndex = 14;
            this._addPicture.Text = "添加图片";
            this._addPicture.Click += new System.EventHandler(this.addPicture_Click);
            // 
            // _tabControlLyrics
            // 
            this._tabControlLyrics.Controls.Add(this._tabPageGeneric);
            this._tabControlLyrics.Location = new System.Drawing.Point(8, 7);
            this._tabControlLyrics.Name = "_tabControlLyrics";
            this._tabControlLyrics.SelectedIndex = 0;
            this._tabControlLyrics.Size = new System.Drawing.Size(552, 269);
            this._tabControlLyrics.TabIndex = 0;
            // 
            // ID3AdapterEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(567, 316);
            this.Controls.Add(this._buttonCancel);
            this.Controls.Add(this._buttonOK);
            this.Controls.Add(this._tabControlLyrics);
            this.DataBindings.Add(new System.Windows.Forms.Binding("Text", this._mp3FileBindingSource, "FileName", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ID3AdapterEdit";
            this.ShowInTaskbar = false;
            this.Text = "详细信息";
            this.Load += new System.EventHandler(this.ID3Edit_Load);
            ((System.ComponentModel.ISupportInitialize)(this._mp3FileBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._tagHandlerBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider)).EndInit();
            this._tabPageGeneric.ResumeLayout(false);
            this._tabPageGeneric.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._artPictureBox)).EndInit();
            this._tabControlLyrics.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		private void ID3Edit_Load(object sender, System.EventArgs e)
		{
			// If there is no model
			if(_mp3File == null)
				throw new ApplicationException("No data to edit on load");

            // set up databindings
            this._tagHandlerBindingSource.DataSource = _tagHandler;
            this._mp3FileBindingSource.DataSource = _mp3File;

            //this._textBoxSampleRate.DataBindings.Add(new System.Windows.Forms.Binding("Text", this._mp3FileBindingSource, "Audio.AudioHeader.SamplesPerSecond", true));
            Binding bindSampleRate = new Binding("Text", this._mp3FileBindingSource, "Audio.Header.SamplesPerSecond", true, System.Windows.Forms.DataSourceUpdateMode.Never);
            // Add the delegates to the events.
            bindSampleRate.Format += new ConvertEventHandler(bit_to_kbit);
            this._textBoxSampleRate.DataBindings.Add(bindSampleRate);

            //this._textBoxKBitRateCalc.DataBindings.Add(new System.Windows.Forms.Binding("Text", this._mp3FileBindingSource, "KBitRate", true, System.Windows.Forms.DataSourceUpdateMode.Never, null, "N0"));
            Binding bindKBitRateCalc = new Binding("Text", this._mp3FileBindingSource, "Audio.BitRateCalc", true, System.Windows.Forms.DataSourceUpdateMode.Never);
            // Add the delegates to the events.
            bindKBitRateCalc.Format += new ConvertEventHandler(bit_to_kbit);

            //this._textBoxKBitRateMP3.DataBindings.Add(new System.Windows.Forms.Binding("Text", this._mp3FileBindingSource, "KBitRate", true, System.Windows.Forms.DataSourceUpdateMode.Never, null, "N0"));
            Binding bindKBitRateMP3 = new Binding("Text", this._mp3FileBindingSource, "Audio.BitRateMp3", true, System.Windows.Forms.DataSourceUpdateMode.Never);
            // Add the delegates to the events.
            bindKBitRateMP3.Format += new ConvertEventHandler(bit_to_kbit);

            //this._textBoxKBitRateVbr.DataBindings.Add(new System.Windows.Forms.Binding("Text", this._mp3FileBindingSource, "KBitRate", true, System.Windows.Forms.DataSourceUpdateMode.Never, null, "N0"));
            Binding bindKBitRateVbr = new Binding("Text", this._mp3FileBindingSource, "Audio.BitRateVbr", true, System.Windows.Forms.DataSourceUpdateMode.Never);
            // Add the delegates to the events.
            bindKBitRateVbr.Format += new ConvertEventHandler(bit_to_kbit);

            //this._textBoxKBitRate.DataBindings.Add(new System.Windows.Forms.Binding("Text", this._mp3FileBindingSource, "KBitRate", true, System.Windows.Forms.DataSourceUpdateMode.Never, null, "N0"));
            Binding bindKBitRate = new Binding("Text", this._mp3FileBindingSource, "Audio.BitRate", true, System.Windows.Forms.DataSourceUpdateMode.Never);
            // Add the delegates to the events.
            bindKBitRate.Format += new ConvertEventHandler(bit_to_kbit);
            this._textBoxKBitRate.DataBindings.Add(bindKBitRate);

            //this._labelDetails.DataBindings.Add(new System.Windows.Forms.Binding("Text", this._mp3FileBindingSource, "Audio", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            Binding bindDetails = new Binding("Text", this._mp3FileBindingSource, "Audio.DebugString", true, System.Windows.Forms.DataSourceUpdateMode.Never);

        }

		private void OnOkClick(object sender, System.EventArgs e)
		{
            // picturebox is not a control that can receive focus,
            // so it doesn't save its image with the databinding mechanism.
            _tagHandler.Picture  = this._artPictureBox.Image;

			DialogResult = DialogResult.OK;
			this.Close();
		}

		private void buttonCancel_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void addPicture_Click(object sender, System.EventArgs e)
		{
			_openFileDialog.Multiselect= false;
			_openFileDialog.CheckFileExists = true;
			_openFileDialog.CheckPathExists = true;
			_openFileDialog.Title = "Select a picture";
            _openFileDialog.Filter = "Picture Files(*.bmp;*.jpg;*.gif;*.png)|*.bpm;*.jpg;*.gif;*.png|Bitmap (*.bmp)|*.bmp|jpg (*.jpg)|*.jpg|jpeg (*.jpeg)|*.jpeg|gif (*.gif)|*.gif|gif (*.png)|*.png";
			if(_openFileDialog.ShowDialog() == DialogResult.OK)
			{ 
				using (FileStream stream = File.Open(_openFileDialog.FileName,FileMode.Open,FileAccess.Read,FileShare.Read))
                {
				    byte[] buffer = new Byte[stream.Length];
				    stream.Read(buffer,0,buffer.Length);
				    if(buffer != null)
				    {
					    MemoryStream memoryStream = new MemoryStream(buffer,false);
					    this._artPictureBox.Image = Image.FromStream(memoryStream);
				    }
			    }
			}
		}

		private void removePicture_Click(object sender, System.EventArgs e)
		{
			this._artPictureBox.Image = null;
		}

        private void bit_to_kbit(object sender, ConvertEventArgs cevent)
        {
            // The method converts only to string type. Test this using the DesiredType.
            if (cevent.DesiredType != typeof(string)) return;

            // Use the ToString method to format the bit/s value as kbit/s with one decimal place ("F1").
            //cevent.Value = (System.Convert.ToDouble(cevent.Value) / 1000).ToString("F1");
            double rate = System.Convert.ToDouble(cevent.Value);
            double ratek = rate / 1000;
            string result = ratek.ToString("F1");
            cevent.Value = result;
        }

        private void OnButtonScanWholeFile(object sender, EventArgs e)
        {
            using (new CursorKeeper(Cursors.WaitCursor))
            {
                _mp3File.Audio.ScanWholeFile();
            }

        }


        private void SHA_Click(object sender, EventArgs e)
        {
            using (new CursorKeeper(Cursors.WaitCursor))
            {
                byte[] sha1 = _mp3File.Audio.CalculateAudioSHA1();
                string txt = System.Convert.ToBase64String(sha1);

            }
        }

        private void _labelTrackNo_Click(object sender, EventArgs e)
        {

        }

        private void _tabPageGeneric_Click(object sender, EventArgs e)
        {

        }
	}
}
