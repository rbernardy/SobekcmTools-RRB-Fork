#region Using directives

using System;
using System.Data;
using System.IO;
using System.Net;

#endregion

namespace SobekCM.Management_Tool
{
    /// <summary> Enumeration defines the type of retrieval to be performed </summary>
    public enum Retrieval_Type_Enum
    {
        /// <summary> Retrieve the METS files for the selected items </summary>
        METS_Only = 1,

        /// <summary> Retrieve the complete package for the selected items </summary>
        Complete,

        /// <summary> Retrieve a MarcXML report with the information or all selected items </summary>
        MARC_XML
    }

    /// <summary> Delegate for the progress event during SobekCM item retrieval </summary>
    /// <param name="currentItem"> Index of the ccurrently being progressed item </param>
    public delegate void Retrieve_SobekCM_Items_Progress_Delegate( int currentItem );

    /// <summary> Delegate for the items complete event during SobekCM item retrieval </summary>
    /// <param name="successCount"> Number of items successfully downloaded </param>
    /// <param name="missingMetsCount"> Number of items with no METS file available </param>
    /// <param name="errorCount"> Number of actual errors encountered (file save failures, etc.) </param>
    public delegate void Retrieve_SobekCM_Items_Complete_Delegate( int successCount, int missingMetsCount, int errorCount );

    /// <summary> Delegate for the individual file progress during a complete SobekCM item retrieval </summary>
    /// <param name="fileCount"> Number of files processed </param>
    /// <param name="maxFiles"> Complete number of files to be processed </param>
    public delegate void Retrieve_SobekCM_Items_File_Progress_Delegate(int fileCount, int maxFiles );

    /// <summary> Processor class pulls packages (or portions of packages) from a SobekCM library </summary>
    public class Retrieve_SobekCM_Items_Processor
    {
        private readonly string destination;
        private readonly DataTable itemList;
        private readonly Retrieval_Type_Enum retrievalType;

        /// <summary> Constructor for a new instance of the Retrieve_SobekCM_Items_Processor </summary>
        /// <param name="Item_List"> List of items to retrieve </param>
        /// <param name="Destination"> Destination (directory) for the retrieved items </param>
        /// <param name="Retrieval_Type"> Type of retrieval to perform ( i.e, METS, complete package, MarcXML report, etc.. ) </param>
        public Retrieve_SobekCM_Items_Processor(DataTable Item_List, string Destination, Retrieval_Type_Enum Retrieval_Type )
        {
            itemList = Item_List;
            destination = Destination;
            retrievalType = Retrieval_Type;
        }

        /// <summary> Event is fired as each package's retrieval completes </summary>
        public event Retrieve_SobekCM_Items_Progress_Delegate New_Progress;

        /// <summary> Event is fired when the retrieval process is complete </summary>
        public event Retrieve_SobekCM_Items_Complete_Delegate Progress_Complete;

        /// <summary> Event is fired as each individual file is processed when retrieving complete packages </summary>
        public event Retrieve_SobekCM_Items_File_Progress_Delegate File_Progress;

        /// <summary> Start processing this item retrieval request </summary>
        public void Start()
        {
            int current_item = 0;
            int success_count = 0;
            int missing_mets_count = 0;
            int error_count = 0;

            StreamWriter marcReportWriter = null;

            if (retrievalType == Retrieval_Type_Enum.MARC_XML)
            {
                marcReportWriter = new StreamWriter(destination + "\\marc_report.xml", false);

                marcReportWriter.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
                marcReportWriter.WriteLine("<collection xmlns=\"http://www.loc.gov/MARC21/slim\">");
            }

            foreach (DataRow thisRow in itemList.Rows)
            {
                if ((retrievalType == Retrieval_Type_Enum.METS_Only) || ( retrievalType == Retrieval_Type_Enum.MARC_XML ))
                {
                    // Item_ID is in format "AA00039299_00006" - extract BibID and VID
                    string itemId = thisRow["Item_ID"].ToString();
                    string bibId = itemId;
                    string vid = "00001";
                    
                    // If itemId contains underscore, split into BibID and VID
                    if (itemId.Contains("_"))
                    {
                        int underscorePos = itemId.LastIndexOf('_');
                        bibId = itemId.Substring(0, underscorePos);
                        vid = itemId.Substring(underscorePos + 1);
                    }

                    string destination_folder = destination + "\\" + bibId + "\\" + vid;

                    // First, always try to download the METS file
                    // METS file naming convention: BibID_VID.mets.xml
                    string mets_file_url = thisRow["Web_Folder"] + "/" + bibId + "_" + vid + ".mets.xml";
                    string destination_file = destination_folder + "\\" + bibId + "_" + vid + ".mets.xml";
                    if (retrievalType == Retrieval_Type_Enum.MARC_XML)
                    {
                        mets_file_url = thisRow["Web_Folder"] + "/marc.xml";
                        destination_file = destination_folder + "\\marc.xml";
                    }
                    
                    LoggingWindow.Log("Attempting to download: " + mets_file_url);
                    string mets_file_data = Get_Html_Page(mets_file_url);

                    if (mets_file_data.Length > 0)
                    {
                        try
                        {
                            // Create the folder
                            if (!Directory.Exists(destination_folder))
                                Directory.CreateDirectory(destination_folder);

                            // Save the METS file
                            StreamWriter writer = new StreamWriter(destination_file, false);
                            writer.Write(mets_file_data);
                            writer.Flush();
                            writer.Close();
                            
                            LoggingWindow.Log("Successfully downloaded: " + bibId + "_" + vid);
                            success_count++;
                        }
                        catch (Exception ex)
                        {
                            LoggingWindow.Log("ERROR saving file for " + bibId + "_" + vid + ": " + ex.Message);
                            error_count++;
                        }

                        // Build the marc report
                        if ((retrievalType == Retrieval_Type_Enum.MARC_XML) && ( marcReportWriter != null ))
                        {
                            marcReportWriter.Write(mets_file_data.Replace("</collection>", "").Replace("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>","").Replace("<collection xmlns=\"http://www.loc.gov/MARC21/slim\">",""));
                        }
                    }
                    else
                    {
                        LoggingWindow.Log("No METS file available for " + bibId + "_" + vid + " (URL: " + mets_file_url + ")");
                        missing_mets_count++;
                    }
                }
                else
                {
                    // Complete package retrieval
                    // Item_ID is in format "AA00039299_00006" - extract BibID and VID
                    string itemId = thisRow["Item_ID"].ToString();
                    string bibId = itemId;
                    string vid = "00001";
                    
                    // If itemId contains underscore, split into BibID and VID
                    if (itemId.Contains("_"))
                    {
                        int underscorePos = itemId.LastIndexOf('_');
                        bibId = itemId.Substring(0, underscorePos);
                        vid = itemId.Substring(underscorePos + 1);
                    }

                    // First try network folder (for users with direct network access)
                    string resource_folder = thisRow["Network_Folder"].ToString();
                    bool useNetworkFolder = Directory.Exists(resource_folder);

                    if (useNetworkFolder)
                    {
                        // Copy from network folder
                        try
                        {
                            LoggingWindow.Log("Copying from network folder: " + resource_folder);

                            // Create the folder for this
                            string destination_folder2 = destination + "\\" + bibId + "\\" + vid;

                            if (!Directory.Exists(destination_folder2))
                            {
                                Directory.CreateDirectory(destination_folder2);
                            }

                            // Copy all the files over from the standard storage
                            string[] files = Directory.GetFiles(resource_folder);
                            LoggingWindow.Log("Found " + files.Length + " files to copy for " + bibId + "_" + vid);
                            
                            int file_count = 0;
                            foreach (string thisFile in files)
                            {
                                string new_file_name = destination_folder2 + "\\" + (new FileInfo(thisFile)).Name;
                                File.Copy(thisFile, new_file_name, true);

                                if (File_Progress != null)
                                {
                                    file_count++;
                                    File_Progress(file_count, files.Length);
                                }
                            }
                            
                            LoggingWindow.Log("Successfully copied " + files.Length + " files for " + bibId + "_" + vid);
                            success_count++;
                        }
                        catch (Exception ex)
                        {
                            LoggingWindow.Log("ERROR copying files for " + bibId + "_" + vid + ": " + ex.Message);
                            error_count++;
                        }
                    }
                    else
                    {
                        // Network folder not accessible - download via HTTP from web folder
                        try
                        {
                            string web_folder = thisRow["Web_Folder"].ToString().TrimEnd('/');
                            LoggingWindow.Log("Network folder not accessible, downloading via HTTP from: " + web_folder);

                            // Create the destination folder
                            string destination_folder2 = destination + "\\" + bibId + "\\" + vid;
                            if (!Directory.Exists(destination_folder2))
                            {
                                Directory.CreateDirectory(destination_folder2);
                            }

                            // Download the METS file first (required)
                            string mets_file_url = web_folder + "/" + bibId + "_" + vid + ".mets.xml";
                            string mets_data = Get_Html_Page(mets_file_url);
                            
                            if (mets_data.Length > 0)
                            {
                                // Save the METS file
                                string mets_dest = destination_folder2 + "\\" + bibId + "_" + vid + ".mets.xml";
                                using (StreamWriter writer = new StreamWriter(mets_dest, false))
                                {
                                    writer.Write(mets_data);
                                }
                                LoggingWindow.Log("Downloaded METS file for " + bibId + "_" + vid);

                                // Parse the METS to find all referenced files
                                int filesDownloaded = 1; // Count METS file
                                System.Collections.Generic.List<string> filesToDownload = new System.Collections.Generic.List<string>();
                                
                                // Extract file references from METS (look for FLocat elements with xlink:href)
                                try
                                {
                                    using (System.IO.StringReader sr = new System.IO.StringReader(mets_data))
                                    using (System.Xml.XmlTextReader xmlReader = new System.Xml.XmlTextReader(sr))
                                    {
                                        while (xmlReader.Read())
                                        {
                                            if (xmlReader.NodeType == System.Xml.XmlNodeType.Element && 
                                                (xmlReader.LocalName == "FLocat" || xmlReader.Name.EndsWith(":FLocat")))
                                            {
                                                // Try both with and without namespace prefix for xlink:href
                                                string href = xmlReader.GetAttribute("xlink:href");
                                                if (String.IsNullOrEmpty(href))
                                                {
                                                    href = xmlReader.GetAttribute("href", "http://www.w3.org/1999/xlink");
                                                }
                                                if (!String.IsNullOrEmpty(href) && !filesToDownload.Contains(href))
                                                {
                                                    filesToDownload.Add(href);
                                                    LoggingWindow.Log("Found file reference: " + href);
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception parseEx)
                                {
                                    LoggingWindow.Log("Warning: Could not parse METS for file list: " + parseEx.Message);
                                }

                                // Download each referenced file
                                LoggingWindow.Log("Found " + filesToDownload.Count + " files referenced in METS");
                                int file_count = 0;
                                foreach (string fileRef in filesToDownload)
                                {
                                    try
                                    {
                                        string file_url = web_folder + "/" + fileRef;
                                        string file_dest = destination_folder2 + "\\" + fileRef;
                                        
                                        // Download the file
                                        byte[] fileData = Get_Binary_File(file_url);
                                        if (fileData != null && fileData.Length > 0)
                                        {
                                            File.WriteAllBytes(file_dest, fileData);
                                            filesDownloaded++;
                                        }

                                        if (File_Progress != null)
                                        {
                                            file_count++;
                                            File_Progress(file_count, filesToDownload.Count);
                                        }
                                    }
                                    catch (Exception fileEx)
                                    {
                                        LoggingWindow.Log("Warning: Could not download " + fileRef + ": " + fileEx.Message);
                                    }
                                }

                                LoggingWindow.Log("Successfully downloaded " + filesDownloaded + " files for " + bibId + "_" + vid);
                                success_count++;
                            }
                            else
                            {
                                LoggingWindow.Log("No METS file available for " + bibId + "_" + vid + " (URL: " + mets_file_url + ")");
                                missing_mets_count++;
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggingWindow.Log("ERROR downloading files for " + bibId + "_" + vid + ": " + ex.Message);
                            error_count++;
                        }
                    }
                }

                // Fire the progress event
                current_item++;
                if (New_Progress != null)
                    New_Progress(current_item);
            }

            if ((retrievalType == Retrieval_Type_Enum.MARC_XML) && ( marcReportWriter != null ))
            {
                marcReportWriter.WriteLine("</collection>");
                marcReportWriter.Flush();
                marcReportWriter.Close();
            }

            // Fire the complete event
            if (Progress_Complete != null)
                Progress_Complete(success_count, missing_mets_count, error_count);
        }

        private string Get_Html_Page(string strURL)
        {
            try
            {
                // Enable TLS 1.2 for HTTPS connections
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

        private byte[] Get_Binary_File(string strURL)
        {
            try
            {
                // Enable TLS 1.2 for HTTPS connections
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                WebRequest objRequest = WebRequest.Create(strURL);
                using (WebResponse objResponse = objRequest.GetResponse())
                using (Stream objStream = objResponse.GetResponseStream())
                {
                    if (objStream != null)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            objStream.CopyTo(ms);
                            return ms.ToArray();
                        }
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
