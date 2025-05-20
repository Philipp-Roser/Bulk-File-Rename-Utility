using System.Diagnostics;
using System.IO;


namespace BulkRename
{
    public class BulkRenameForm : Form
    {
        int _defaultWindowWidth = 800;
        int _defaultWindowHeight = 800;

        Label _titleLabel;

        Button _selectDirectoryButton;
        Label _selectedDirectoryDisplay;

        CheckBox _keepFileExtensionsCheckBox;
        Label _fileExtensionCheckBoxLabel;

        Panel _scrollPanel;

        Label _columnTitleOld;
        Label _columnTitleNew;

        Button _renameButton;
        Button _clearButton;

        List<Label> _fileLabels = new();
        List<TextBox> _newNameBoxes = new();
        List<Label> _fileExtensionLabels = new();


        int _fileListFirstYPos = 250;
        int _fileLabelSpacing = 20;
        int _fileLabelHeight = 30;
        int _fileLabelLength = 300;
        int _fileExtensionLength = 100;
        int _renameBoxX = 300;
        int _renameBoxWidth = 200;

        string _selectedDirectory = "";
        List<string> _filesInDirectory = new();


        public BulkRenameForm()
        {
            InitializeComponent();
            InitializeUI();
        }


        void InitializeUI()
        {
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = false;

            Width = _defaultWindowWidth;
            Height = _defaultWindowHeight;

            _titleLabel = WinformHelpers.Create.NewLabel(
                "--- Bulk File Rename Utility ---", 60, 20, 400, 40);
            _titleLabel.Font = new(_titleLabel.Font, FontStyle.Bold);
            this.Controls.Add(_titleLabel);

            _selectDirectoryButton = WinformHelpers.Create.NewButton(
                "SelectDirectory", 60, 80, 200, 40);
            this.Controls.Add(_selectDirectoryButton);
            _selectDirectoryButton.Click += OnClickSelectDirectory;

            _selectedDirectoryDisplay = WinformHelpers.Create.NewLabel(
                "No directory selected", 60, 150, 600, 40);
            this.Controls.Add(_selectedDirectoryDisplay);

            _columnTitleOld = WinformHelpers.Create.NewLabel("Old file name", 60, 220, 200, 40);
            _columnTitleOld.Font = new Font(_columnTitleOld.Font, FontStyle.Bold);
            this.Controls.Add(_columnTitleOld);
            _columnTitleOld.Visible = false;

            _columnTitleNew = WinformHelpers.Create.NewLabel(
                "New file name", 60 + _renameBoxX, 220, 200, 40);
            _columnTitleNew.Font = new Font(_columnTitleNew.Font, FontStyle.Bold);
            this.Controls.Add(_columnTitleNew);
            _columnTitleNew.Visible = false;

            _keepFileExtensionsCheckBox = WinformHelpers.Create.NewCheckBox(
                60 + _renameBoxX + _renameBoxWidth + 10, 220, 20, 30, true);
            this.Controls.Add(_keepFileExtensionsCheckBox);
            _keepFileExtensionsCheckBox.Visible = false;
            _keepFileExtensionsCheckBox.CheckedChanged += OnFileExtensionCheckBoxChanged;

            _fileExtensionCheckBoxLabel = WinformHelpers.Create.NewLabel(
                "Keep File Extensions", 60 + _renameBoxX + _renameBoxWidth + 30, 220, 200, 40);
            this.Controls.Add(_fileExtensionCheckBoxLabel);
            _fileExtensionCheckBoxLabel.Visible = false;

            _renameButton = WinformHelpers.Create.NewButton(
                "Rename", 60 + _renameBoxX, Height - 150, 200, 40);
            this.Controls.Add(_renameButton);
            _renameButton.Visible = false;
            _renameButton.Click += OnRenameButtonClicked;

            _clearButton = WinformHelpers.Create.NewButton(
                "Clear All", 60 + _renameBoxX + _renameBoxWidth + 30, Height - 150, 150, 40);
            this.Controls.Add(_clearButton);
            _clearButton.Visible = false;
            _clearButton.Click += OnClearButtonClicked;

            InitializeResizeTimer();
        }

        private void OnClickSelectDirectory(object? sender, EventArgs e)
        {
            _filesInDirectory = new();
            ClearDisplayedFileList();

            using ( FolderBrowserDialog dialog = new() )
            {
                dialog.Description = "Select Directory";
                dialog.UseDescriptionForTitle = true;

                DialogResult result = dialog.ShowDialog();

                if ( result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath) )
                {
                    {
                        _selectedDirectory = dialog.SelectedPath;
                        OnDirectorySelected();
                    }
                }
            }
        }

        private void OnFileExtensionCheckBoxChanged(object? sender, EventArgs e)
        {
            DisplayFileListEtc();
        }

        void OnRenameButtonClicked(object? sender, EventArgs e)
        {
            int fileCount = _filesInDirectory.Count;
            for(int i = 0; i < fileCount; i++ )
            {
                if ( _newNameBoxes[i].Text == "" )
                    continue;

                string oldPath = _filesInDirectory[i];
                string newPath = _keepFileExtensionsCheckBox.Checked ?
                                        _selectedDirectory + "\\" + _newNameBoxes[i].Text + _fileExtensionLabels[i].Text
                                    :   _selectedDirectory + "\\" + _newNameBoxes[i].Text;

                try 
                {
                    File.Move(oldPath, newPath);
                }
                catch (Exception ex)
                {
                     MessageBox.Show($"Error: Cannot rename {_filesInDirectory[i]}.\n\n" +
                         $"Possible suspects:\n" +
                         $"- Your file is in use by another application.\n" +
                         $"- You tried to have illegal symbols inside your file name.\n" +
                         $"- The file is read-only or you otherwise don't have write permission.\n" +
                         $"- Your file name is too long.\n\n" +
                         $"If it's none of those, you're on your own. Sorry.");

                }

            }
            
            DisplayFileListEtc();
        }

        void OnClearButtonClicked(object? sender, EventArgs e)
        {
            if ( _scrollPanel is null )
                return;

            for ( int i = 0; i < _newNameBoxes.Count; i++ )
                _newNameBoxes[i].Text = "";
        }




        void OnDirectorySelected()
        {
            _selectDirectoryButton.Text = "Change Directory";
            _selectedDirectoryDisplay.Text = _selectedDirectory;

            _fileExtensionCheckBoxLabel.Visible = true;
            _keepFileExtensionsCheckBox.Visible = true;

            DisplayFileListEtc();
        }



        void DisplayFileListEtc()
        {
            ClearDisplayedFileList();

            Debug.Assert(Directory.Exists(_selectedDirectory));
            _filesInDirectory = Directory.GetFiles(_selectedDirectory).ToList();
            int fileCount = _filesInDirectory.Count;

            _columnTitleOld.Visible = true;
            _columnTitleNew.Visible = true;

            _scrollPanel = new Panel();
            _scrollPanel.Location = new Point(60, _fileListFirstYPos);
            _scrollPanel.Size = new Size(Width - 80, Height - _fileListFirstYPos - 200);
            _scrollPanel.AutoScroll = true;
            this.Controls.Add(_scrollPanel);

            for(int i = 0; i < fileCount; i++ )
            {
                string fileNameNoExt = Path.GetFileNameWithoutExtension(_filesInDirectory[i]);
                string fileExtension = Path.GetExtension(_filesInDirectory[i]);
                string fileName = fileNameNoExt + fileExtension;

                int rowY = 20 + i * (_fileLabelHeight + _fileLabelSpacing);
                
                if(_keepFileExtensionsCheckBox.Checked)
                {
                    _fileLabels.Add(WinformHelpers.Create.NewLabel(fileName, 0, rowY, _fileLabelLength, _fileLabelHeight));
                    _scrollPanel.Controls.Add(_fileLabels[i]);
                    _newNameBoxes.Add(WinformHelpers.Create.NewTextBox
                        ("", _renameBoxX, rowY, _renameBoxWidth, 40));
                    _scrollPanel.Controls.Add(_newNameBoxes[i]);
                    _fileExtensionLabels.Add(WinformHelpers.Create.NewLabel(
                        fileExtension, _renameBoxX + _renameBoxWidth + 10, rowY, 
                        _fileExtensionLength, _fileLabelHeight));
                    _scrollPanel.Controls.Add(_fileExtensionLabels[i]);
                }
                else
                {
                    _fileLabels.Add(WinformHelpers.Create.NewLabel(fileName, 0, rowY, _fileLabelLength, _fileLabelHeight));
                    _scrollPanel.Controls.Add(_fileLabels[i]);
                    _newNameBoxes.Add(WinformHelpers.Create.NewTextBox("", _renameBoxX, rowY, _renameBoxWidth, 40));
                    _scrollPanel.Controls.Add(_newNameBoxes[i]);
                }

                _renameButton.Visible = true;
                _clearButton.Visible = true;

                _renameButton.Location = new Point (60 + _renameBoxX, Height - 150);
                _clearButton.Location = new Point(60 + _renameBoxX + _renameBoxWidth + 10, Height - 150);
            }
        }



        void ClearDisplayedFileList()
        {
            _fileLabels = new();
            _newNameBoxes = new();
            _fileExtensionLabels = new();

            if(_scrollPanel is not null)
            {
                this.Controls.Remove(_scrollPanel);
                _scrollPanel.Dispose();
            }
        }



        #region Resize Window Logic ----------

        System.Windows.Forms.Timer _resizeTimer;

        void InitializeResizeTimer()
        {
            _resizeTimer = new();
            this.Resize += OnResizeWindow;
            _resizeTimer.Interval = 100; // ms
            _resizeTimer.Tick += RedrawAfterResize;
        }

        void OnResizeWindow(object? sender, EventArgs e)
        {
            _resizeTimer.Stop();
            _resizeTimer.Start();
        }

        void RedrawAfterResize(object? sender, EventArgs e)
        {
            if ( Width < _defaultWindowWidth )
                Width = _defaultWindowWidth;

            _resizeTimer.Stop();
            if ( _selectedDirectory != "" )
                DisplayFileListEtc();
        }


        #endregion




        #region Boilerplate stuff ------------

        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if ( disposing && (components != null) )
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(_defaultWindowWidth, _defaultWindowHeight);
            this.Text = "Bulk Rename Utility";
        }


        #endregion

    }
}
