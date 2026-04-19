#region Using directives

using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using SobekCM.Engine_Library;
using SobekCM.Engine_Library.ApplicationState;
using SobekCM.Engine_Library.Settings;

#endregion

namespace SobekCM.Management_Tool
{
    /// <summary> Form is used to retrieve SobekCM_Items from a library, based on 
    /// a SobekCM search or browse URL  </summary>
    public partial class Retrieve_SobekCM_Items_Form : Form
    {
        private Thread processingThread;

        /// <summary> Constructor for a new instance of the Retrieve_SobekCM_Items_Form class </summary>
        public Retrieve_SobekCM_Items_Form()
        {
            InitializeComponent();
            BackColor = Color.FromArgb(240, 240, 240);

            folderBrowserDialog1.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            if (!Windows_Appearance_Checker.is_XP_Theme)
            {
                destinationTextBox.BorderStyle = BorderStyle.FixedSingle;
                sobekcmQueryTextBox.BorderStyle = BorderStyle.FixedSingle;
                browseButton.FlatStyle = FlatStyle.Flat;
                completeRadioButton.FlatStyle = FlatStyle.Flat;
                metsOnlyRadioButton.FlatStyle = FlatStyle.Flat;
                marcXmlRadioButton.FlatStyle = FlatStyle.Flat;
            }


            // Personalize several labels and controls now for the SobekCM Instance Name
            Text = "Retrieve " + Engine_ApplicationCache_Gateway.Settings.System.System_Abbreviation + " Items Form - (" + MainForm.CurrentDatabaseServer + ")";
            mainLabel.Text = "Retrieve " + Engine_ApplicationCache_Gateway.Settings.System.System_Abbreviation + " Items";
            queryLabel.Text = Engine_ApplicationCache_Gateway.Settings.System.System_Abbreviation + " Query:";

            // Apply developer mode defaults if applicable
            string devUrl = MainForm.GetDeveloperRetrieveUrl();
            if (!String.IsNullOrEmpty(devUrl))
            {
                sobekcmQueryTextBox.Text = devUrl;
            }

            string devFolder = MainForm.GetDeveloperOutputFolder();
            if (!String.IsNullOrEmpty(devFolder))
            {
                destinationTextBox.Text = devFolder;
                folderBrowserDialog1.SelectedPath = devFolder;
            }
        }

        public override sealed string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        public override sealed Color BackColor
        {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }

        #region Method to draw the form background

        /// <summary> Method is called whenever this form is resized. </summary>
        /// <param name="e"></param>
        /// <remarks> This redraws the background of this form </remarks>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            // Get rid of any current background image
            if (BackgroundImage != null)
            {
                BackgroundImage.Dispose();
                BackgroundImage = null;
            }

            if (ClientSize.Width > 0)
            {
                // Create the items needed to draw the background
                Bitmap image = new Bitmap(ClientSize.Width, ClientSize.Height);
                Graphics gr = Graphics.FromImage(image);
                Rectangle rect = new Rectangle(new Point(0, 0), ClientSize);

                // Create the brush
                LinearGradientBrush brush = new LinearGradientBrush(rect, BackColor, ControlPaint.Dark(BackColor), LinearGradientMode.Vertical);
                brush.SetBlendTriangularShape(0.33F);

                // Create the image
                gr.FillRectangle(brush, rect);
                gr.Dispose();

                // Set this as the backgroundf
                BackgroundImage = image;
            }
        }

        #endregion

        private void exitButton_Button_Pressed(object sender, EventArgs e)
        {
            Close();
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                destinationTextBox.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void okButton_Button_Pressed(object sender, EventArgs e)
        {
            if (sobekcmQueryTextBox.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please include a  " + Engine_ApplicationCache_Gateway.Settings.System.System_Abbreviation + " URL for a browse or search.       \n\nFor example: 'http://ufdc.ufl.edu/l/foto/results/?t=flint hall'    ", "Missing URL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (( destinationTextBox.Text.Trim().Length == 0 ) || ( !Directory.Exists( destinationTextBox.Text )))
            {
                MessageBox.Show("Please select a valid destination for the " + Engine_ApplicationCache_Gateway.Settings.System.System_Abbreviation + " packages.    ", "Missing or Invalid Destination", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string sobekcm_url = sobekcmQueryTextBox.Text.Trim();
            string destination = destinationTextBox.Text.Trim();

            sobekcmQueryTextBox.ReadOnly = true;
            destinationTextBox.ReadOnly = true;
            completeRadioButton.Enabled = false;
            metsOnlyRadioButton.Enabled = false;
            marcXmlRadioButton.Enabled = false;
            browseButton.Enabled = false;
            okButton.Button_Enabled = false;
            exitButton.Button_Enabled = false;

            // Handle URL transformation to XML endpoint
            sobekcm_url = sobekcm_url.Replace("/l/", "/xml/").Replace("/dataset/", "/xml/").Replace("/json/", "/xml/");
            
            // Check if URL has a protocol, if not add http://
            if (!sobekcm_url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && 
                !sobekcm_url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                sobekcm_url = "http://" + sobekcm_url;
            }
            
            // Insert /xml/ into the URL if not already present
            if (sobekcm_url.IndexOf("/xml/", StringComparison.OrdinalIgnoreCase) < 0)
            {
                string systemBaseUrl = Engine_ApplicationCache_Gateway.Settings.Servers.System_Base_URL;
                if (!String.IsNullOrEmpty(systemBaseUrl) && sobekcm_url.StartsWith(systemBaseUrl, StringComparison.OrdinalIgnoreCase))
                {
                    sobekcm_url = systemBaseUrl.TrimEnd('/') + "/xml/" + sobekcm_url.Substring(systemBaseUrl.Length).TrimStart('/');
                }
                else
                {
                    // Use simple string manipulation to insert /xml/ after the host
                    // This preserves the full hostname including all subdomains
                    int protocolEnd = sobekcm_url.IndexOf("://") + 3;
                    int firstSlash = sobekcm_url.IndexOf("/", protocolEnd);
                    if (firstSlash > 0)
                    {
                        sobekcm_url = sobekcm_url.Substring(0, firstSlash) + "/xml" + sobekcm_url.Substring(firstSlash);
                    }
                    else
                    {
                        // No path, just append /xml/
                        sobekcm_url = sobekcm_url + "/xml/";
                    }
                }
            }

            string web_stream = Get_Html_Page(sobekcm_url);

            if (web_stream.Length == 0)
            {
                MessageBox.Show("Invalid " + Engine_ApplicationCache_Gateway.Settings.System.System_Abbreviation + " Query URL was supplied.\n\nPerform requested search or browse directly in " + Engine_ApplicationCache_Gateway.Settings.System.System_Abbreviation + " and     \nthen copy the URL into the " + Engine_ApplicationCache_Gateway.Settings.System.System_Abbreviation + " query box.\n\nTransformed URL: " + sobekcm_url, "Invalid " + Engine_ApplicationCache_Gateway.Settings.System.System_Abbreviation + " Query Supplied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                sobekcmQueryTextBox.ReadOnly = false;
                destinationTextBox.ReadOnly = false;
                completeRadioButton.Enabled = true;
                metsOnlyRadioButton.Enabled = true;
                browseButton.Enabled = true;
                okButton.Button_Enabled = true;
                exitButton.Button_Enabled = true;
                marcXmlRadioButton.Enabled = true;
                return;
            }

            // Temporary folder
            string temp_folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            temp_folder = temp_folder + "\\SMaRT Temporary";
            try
            {
                if (!Directory.Exists(temp_folder))
                    Directory.CreateDirectory(temp_folder);
            }
            catch
            {
                MessageBox.Show("Unable to create necessary directory:\n\n\t" + temp_folder, "Unable to create temporary folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                sobekcmQueryTextBox.ReadOnly = false;
                destinationTextBox.ReadOnly = false;
                completeRadioButton.Enabled = true;
                metsOnlyRadioButton.Enabled = true;
                marcXmlRadioButton.Enabled = true;
                browseButton.Enabled = true;
                okButton.Button_Enabled = true;
                exitButton.Button_Enabled = true;
                return;
            }

            // Sanitize the system abbreviation for use in filename (remove invalid characters)
            string safeAbbreviation = Engine_ApplicationCache_Gateway.Settings.System.System_Abbreviation.ToLower();
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                safeAbbreviation = safeAbbreviation.Replace(c.ToString(), "_");
            }
            string temp_file = temp_folder + "\\" + safeAbbreviation + "_download.xml";

            // Save the data to the temp folder
            try
            {
                // Delete the file first if it exists (in case it's locked from a previous failed attempt)
                if (File.Exists(temp_file))
                {
                    try { File.Delete(temp_file); } catch { }
                }

                using (StreamWriter writer = new StreamWriter(temp_file, false))
                {
                    writer.Write(web_stream);
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to save the downloaded data to the temporary folder:\n\n\t" + temp_file + "\n\nError: " + ex.Message, "Unable to create temporary file", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                sobekcmQueryTextBox.ReadOnly = false;
                destinationTextBox.ReadOnly = false;
                completeRadioButton.Enabled = true;
                metsOnlyRadioButton.Enabled = true;
                marcXmlRadioButton.Enabled = true;
                browseButton.Enabled = true;
                okButton.Button_Enabled = true;
                exitButton.Button_Enabled = true;
                return;
            }

            // Load this data into a dataset
            DataTable itemList = null;
            try
            {
                itemList = Read_Item_Xml(temp_file);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to load the xml data into a dataset for processing.\n\nFile: " + temp_file + "\n\nError: " + ex.Message + "\n\nStack: " + ex.StackTrace, "Processing Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                sobekcmQueryTextBox.ReadOnly = false;
                destinationTextBox.ReadOnly = false;
                completeRadioButton.Enabled = true;
                metsOnlyRadioButton.Enabled = true;
                marcXmlRadioButton.Enabled = true;
                browseButton.Enabled = true;
                okButton.Button_Enabled = true;
                exitButton.Button_Enabled = true;
                return;
            }

            // If there were no matching items, show a message
            if (( itemList == null ) || ( itemList.Rows.Count == 0))
            {
                MessageBox.Show("No items match your query!    ", "Processing Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                sobekcmQueryTextBox.ReadOnly = false;
                destinationTextBox.ReadOnly = false;
                completeRadioButton.Enabled = true;
                metsOnlyRadioButton.Enabled = true;
                marcXmlRadioButton.Enabled = true;
                browseButton.Enabled = true;
                okButton.Button_Enabled = true;
                exitButton.Button_Enabled = true;
                return;
            }

            // Ensure the user knows what they are doing here
            DialogResult continue_test = MessageBox.Show("You are about to download " + itemList.Rows.Count + " packages from " + Engine_ApplicationCache_Gateway.Settings.System.System_Abbreviation.ToLower() + ".    \n\nAre you sure you would like to continue?", "Confirmation", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (continue_test != DialogResult.Yes)
            {
                sobekcmQueryTextBox.ReadOnly = false;
                destinationTextBox.ReadOnly = false;
                completeRadioButton.Enabled = true;
                metsOnlyRadioButton.Enabled = true;
                marcXmlRadioButton.Enabled = true;
                browseButton.Enabled = true;
                okButton.Button_Enabled = true;
                exitButton.Button_Enabled = true;
                return;
            }

            // Set the maximum on the progress bar
            progressBar1.Maximum = itemList.Rows.Count;

            // Determine the type of retrieval requested
            Retrieval_Type_Enum retrievalType = Retrieval_Type_Enum.METS_Only;
            if (completeRadioButton.Checked)
                retrievalType = Retrieval_Type_Enum.Complete;
            if (marcXmlRadioButton.Checked)
                retrievalType = Retrieval_Type_Enum.MARC_XML;

            // Show progress bars
            progressBar1.Show();
            if (retrievalType == Retrieval_Type_Enum.Complete) 
                progressBar2.Show();

            // Create the processor
            Retrieve_SobekCM_Items_Processor processor = new Retrieve_SobekCM_Items_Processor(itemList, destination, retrievalType);
            processor.New_Progress += processor_New_Progress;
            processor.File_Progress += processor_File_Progress;
            processor.Progress_Complete += processor_Progress_Complete;

            // Create the thread for this
            processingThread = new Thread(processor.Start);
            processingThread.Start();
        }

        private DataTable Read_Item_Xml( string fileName )
        {
            // Create the datatable to hold this data and define each column
            DataTable importItemsTable = new DataTable("Items");
            DataColumn titleIdColumn = importItemsTable.Columns.Add("TItle_ID");
            DataColumn itemIdColumn = importItemsTable.Columns.Add("Item_ID");
            DataColumn titleColumn = importItemsTable.Columns.Add("Title");
            DataColumn dateColumn = importItemsTable.Columns.Add("Date");
            DataColumn urlColumn = importItemsTable.Columns.Add("URL");
            DataColumn webColumn = importItemsTable.Columns.Add("Web_Folder");
            DataColumn networkColumn = importItemsTable.Columns.Add("Network_Folder");

            // Read the file content to determine format
            string fileContent = File.ReadAllText(fileName);
            
            // Remove any BOM or leading whitespace/control characters
            fileContent = fileContent.TrimStart('\uFEFF', '\u200B', ' ', '\t', '\r', '\n');

            // Check if this contains the expected XML elements for the old format
            // The old XML format specifically has <TitleResult> and <ItemResult> elements
            // If it doesn't have these, treat it as text format (even if it starts with < like HTML)
            bool isXmlFormat = fileContent.Contains("<TitleResult") || fileContent.Contains("<ItemResult");

            if (isXmlFormat)
            {
                // Use the original XML parsing logic
                Read_Item_Xml_Format(fileName, importItemsTable, titleIdColumn, itemIdColumn, titleColumn, dateColumn, urlColumn, webColumn, networkColumn);
            }
            else
            {
                // Use the new text-based parsing logic
                // Strip any HTML tags if present
                if (fileContent.Contains("<"))
                {
                    // Remove HTML tags - simple regex-free approach
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    bool inTag = false;
                    foreach (char c in fileContent)
                    {
                        if (c == '<') inTag = true;
                        else if (c == '>') inTag = false;
                        else if (!inTag) sb.Append(c);
                    }
                    fileContent = sb.ToString();
                }
                Read_Item_Text_Format(fileContent, importItemsTable, titleIdColumn, itemIdColumn, titleColumn, dateColumn, urlColumn, webColumn, networkColumn);
            }

            return importItemsTable;
        }

        private void Read_Item_Text_Format(string fileContent, DataTable importItemsTable, DataColumn titleIdColumn, DataColumn itemIdColumn, DataColumn titleColumn, DataColumn dateColumn, DataColumn urlColumn, DataColumn webColumn, DataColumn networkColumn)
        {
            // The text format has 4 values per item:
            // 1. URL (e.g., https://lib-builderdev.ad.ufl.edu/AA00039299/00006)
            // 2. Web folder (e.g., https://lib-builderdev.ad.ufl.edu/content/AA/00/03/92/99/00006)
            // 3. Network folder (e.g., C:\inetpub\wwwroot\content\AA\00\03\92\99\00006)
            // 4. Date (e.g., 2016)

            // Split by whitespace, but we need to be careful about paths with spaces
            // The pattern is: URL starts with http, web folder starts with http, network folder starts with drive letter, date is last
            string[] parts = fileContent.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            int i = 0;
            while (i < parts.Length)
            {
                // Find the URL (starts with http)
                if (!parts[i].StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    i++;
                    continue;
                }

                string url = parts[i];
                i++;

                // Skip if we don't have enough parts left
                if (i + 2 >= parts.Length) break;

                // Next should be web folder (also starts with http)
                string web = parts[i];
                i++;

                // Next should be network folder (starts with drive letter like C:\)
                string network = parts[i];
                i++;

                // Next should be date
                string date = parts[i];
                i++;

                // Extract title ID and item ID from the URL
                // URL format: https://lib-builderdev.ad.ufl.edu/AA00039299/00006
                string titleId = String.Empty;
                string itemId = String.Empty;

                try
                {
                    Uri uri = new Uri(url);
                    string[] pathParts = uri.AbsolutePath.Trim('/').Split('/');
                    if (pathParts.Length >= 2)
                    {
                        titleId = pathParts[0];
                        itemId = pathParts[1];
                    }
                    else if (pathParts.Length == 1)
                    {
                        titleId = pathParts[0];
                    }
                }
                catch
                {
                    // If URL parsing fails, try simple string parsing
                    int lastSlash = url.LastIndexOf('/');
                    if (lastSlash > 0)
                    {
                        itemId = url.Substring(lastSlash + 1);
                        int secondLastSlash = url.LastIndexOf('/', lastSlash - 1);
                        if (secondLastSlash > 0)
                        {
                            titleId = url.Substring(secondLastSlash + 1, lastSlash - secondLastSlash - 1);
                        }
                    }
                }

                // Create the new row and assign all the values
                DataRow newRow = importItemsTable.NewRow();
                newRow[titleIdColumn] = titleId;
                newRow[itemIdColumn] = itemId;
                newRow[titleColumn] = String.Empty; // Title not provided in text format
                newRow[dateColumn] = date;
                newRow[urlColumn] = url;
                newRow[webColumn] = web;
                newRow[networkColumn] = network;
                importItemsTable.Rows.Add(newRow);
            }
        }

        private void Read_Item_Xml_Format(string fileName, DataTable importItemsTable, DataColumn titleIdColumn, DataColumn itemIdColumn, DataColumn titleColumn, DataColumn dateColumn, DataColumn urlColumn, DataColumn webColumn, DataColumn networkColumn)
        {
            // Create the temporary values here
            string titleId = String.Empty;
            string itemId = String.Empty;
            string title = String.Empty;
            string date = String.Empty;
            string url = String.Empty;
            string web = String.Empty;
            string network = String.Empty;

            // Read the file content and remove any invalid control characters
            string xmlContent = File.ReadAllText(fileName);
            // Remove control characters that are invalid in XML (0x00-0x1F except tab, newline, carriage return)
            System.Text.StringBuilder cleanContent = new System.Text.StringBuilder();
            foreach (char c in xmlContent)
            {
                if (c == '\t' || c == '\n' || c == '\r' || c >= 0x20)
                {
                    cleanContent.Append(c);
                }
            }

            // Parse the cleaned XML content
            using (StringReader stringReader = new StringReader(cleanContent.ToString()))
            using (XmlTextReader reader = new XmlTextReader(stringReader))
            {
                while (reader.Read())
                {
                    // What type of XML node is this?
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "ItemResult":
                                if (reader.MoveToAttribute("ID"))
                                    itemId = reader.Value;
                                break;

                            case "Title":
                                reader.Read();
                                title = reader.Value;
                                break;

                            case "Date":
                                reader.Read();
                                date = reader.Value;
                                break;

                            case "URL":
                                reader.Read();
                                url = reader.Value;
                                break;

                            case "Folder":
                                if (reader.MoveToAttribute("type"))
                                {
                                    if (reader.Value == "web")
                                    {
                                        reader.Read();
                                        web = reader.Value;
                                    }
                                    else if (reader.Value == "network")
                                    {
                                        reader.Read();
                                        network = reader.Value;
                                    }
                                }
                                break;

                            case "TitleResult":
                                if (reader.MoveToAttribute("ID"))
                                    titleId = reader.Value;
                                break;
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        // Is this ending the title or an item within the title?
                        switch (reader.Name)
                        {
                            case "ItemResult":
                                // Create the new row and assign all the values
                                DataRow newRow = importItemsTable.NewRow();
                                newRow[titleIdColumn] = titleId;
                                newRow[itemIdColumn] = itemId;
                                newRow[titleColumn] = title;
                                newRow[dateColumn] = date;
                                newRow[urlColumn] = url;
                                newRow[webColumn] = web;
                                newRow[networkColumn] = network;
                                importItemsTable.Rows.Add(newRow);

                                // Now, clear out all the item-level data
                                itemId = String.Empty;
                                title = String.Empty;
                                date = String.Empty;
                                url = String.Empty;
                                web = String.Empty;
                                network = String.Empty;
                                break;


                            case "TitleResult":
                                // Clear out the last title bit of information
                                titleId = String.Empty;
                                break;

                        }
                    }
                }
            }
        }


        void processor_File_Progress(int fileCount, int maxFiles)
        {
            if (progressBar2 != null)
            {
                progressBar2.Maximum = maxFiles;
                progressBar2.Value = fileCount;
            }
        }

        void processor_Progress_Complete( int successCount, int missingMetsCount, int errorCount )
        {
            // Build informative completion message
            string message = "Process complete.\n\n";
            message += "Downloaded: " + successCount + " items\n";
            
            if (missingMetsCount > 0)
            {
                message += "No METS file available: " + missingMetsCount + " items\n";
            }
            
            if (errorCount > 0)
            {
                message += "Errors: " + errorCount + " items";
                MessageBox.Show(message, "Complete with Errors", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show(message.TrimEnd(), "Process Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // Show this folder
            Process process = new Process {StartInfo = {FileName = destinationTextBox.Text.Trim()}};
            process.Start();

            // Close the form
            Close();
        }

        void processor_New_Progress(int currentItem)
        {
            progressBar1.Value = currentItem;
        }

        private string Get_Html_Page(string strURL)
        {
            try
            {
                // Enable TLS 1.2 for HTTPS connections (required for modern servers)
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                // the html retrieved from the page
                WebRequest objRequest = WebRequest.Create(strURL);
                WebResponse objResponse = objRequest.GetResponse();
                Stream objStream = objResponse.GetResponseStream();

                if (objStream != null)
                {
                    // the using keyword will automatically dispose the object 
                    // once complete
                    string strResult;
                    using (StreamReader sr = new StreamReader(objStream))
                    {
                        strResult = sr.ReadToEnd();
                        // Close and clean up the StreamReader
                        sr.Close();
                    }
                    return strResult;
                }
                return String.Empty;
            }
            catch 
            {
                return String.Empty;
            }
        }

        private void completeRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (completeRadioButton.Checked)
            {
                try
                {
                   // string[] dirs = Directory.GetDirectories(@"\\cns-uflib-ufdc\UFDC");
                }
                catch
                {
                    MessageBox.Show("You do not appear to have appropriate read rights on the image share.    \n\nIf you should have read access, please put in a GROVER for read-only access.       ", "Insufficient Priviledges Detected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    metsOnlyRadioButton.Checked = true;
                }
            }
        }

        private void textBox_Enter(object sender, EventArgs e)
        {
            ((TextBox)sender).BackColor = Color.Khaki;
        }

        private void textBox_Leave(object sender, EventArgs e)
        {
            ((TextBox)sender).BackColor = Color.White;
        }
    }
}
