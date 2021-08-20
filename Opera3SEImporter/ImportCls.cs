using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Opera3SEImporter
{
    class ImportCls
    {
        public static bool IsStarted { get; private set; }
        public static CancellationTokenSource cts { get; private set; }

        private static CancellationTokenSource cst;

        internal static async Task DoWork(IProgress<int> fileCount, IProgress<int> fileCurrent, IProgress<int> errors, IProgress<string> completed, string fileMask, string fileIncPath, string headerMask, string detailsMask)
        {
            IsStarted = true;
            string logFile = Path.Combine(fileIncPath, "logfile.txt");
            Thread.Sleep(200);
            int count = 0;
            int fileNo = 0;
            int errCount = 0;
            cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            try
            {
                string[] fileHolder = Directory.GetFiles(fileIncPath, "*.*", SearchOption.TopDirectoryOnly);
                foreach (var file in fileHolder)
                {
                    if (Path.GetExtension(file).ToUpper() == ".CSV" &&
                        Path.GetFileNameWithoutExtension(file).ToUpper().Contains(fileMask.ToUpper()) &&
                        Path.GetFileNameWithoutExtension(file).ToUpper().Contains(headerMask.ToUpper()))
                    {
                        count++;                        
                        fileCount.Report(count);
                    }
                }
                foreach (var file in fileHolder)
                {
                    if (Path.GetExtension(file).ToUpper() == ".CSV" && 
                        Path.GetFileNameWithoutExtension(file).ToUpper().Contains(fileMask.ToUpper()) && 
                        Path.GetFileNameWithoutExtension(file).ToUpper().Contains(headerMask.ToUpper()))
                    {
                        Thread.Sleep(500);
                        fileNo++;
                        string pairedFile = GetPairedFile(fileIncPath, file, headerMask, detailsMask);
                        string docReturned = "";
                        string dataTimeString = Convert.ToString(DateTime.Now);
                        char[] invalidChars = Path.GetInvalidFileNameChars();
                        char[] invalidCharsRemoved = dataTimeString.Where(x => !invalidChars.Contains(x)).ToArray();
                        string auditFile = "myauditSE" + string.Join("", invalidCharsRemoved) + ".txt";
                        string auditPass = Path.Combine(fileIncPath, auditFile);
                        bool exists = Directory.Exists(fileIncPath);
                        if (!exists)
                            Directory.CreateDirectory(fileIncPath);
                        string archivePath = Path.Combine(fileIncPath, "Archive");
                        bool archiveexists = Directory.Exists(archivePath);
                        if (!archiveexists)
                            Directory.CreateDirectory(archivePath);
                        bool archiveFailedexists = Directory.Exists(Path.Combine(archivePath, "FAILED"));
                        if (!archiveFailedexists)
                            Directory.CreateDirectory(Path.Combine(archivePath, "FAILED"));
                        string lcExternalUserName = "ckm2";
                        string lcExternalUserPassword = "ckm21234!";
                        Thread.Sleep(1000);
                        operaimport_serveredition.clsOperaImportServerEdition clsOperaImportServerEdition;
                        clsOperaImportServerEdition = new operaimport_serveredition.clsOperaImportServerEdition();
                        bool vLblnSuccess = false;
                        try
                        {
                            vLblnSuccess = Convert.ToBoolean(clsOperaImportServerEdition.ImportData(
                                "IT" + "#" + lcExternalUserName + "#" + lcExternalUserPassword + "#", "Z",
                                file + ", " + pairedFile, 3, auditPass, "U", "IMPORT",
                                DateTime.Now, DateTime.Now.Month, DateTime.Now.Year, DateTime.Now));
                            docReturned = Convert.ToString(clsOperaImportServerEdition.InfoCollectionItem("1"));
                            docReturned = docReturned.Substring(docReturned.LastIndexOf('=') + 1);
                        }
                        catch (Exception exImport) 
                        {
                            File.WriteAllText(logFile, "exImport catch :  " + exImport.Message);
                            errors.Report(errCount++);
                           // clsOperaImportServerEdition = new operaimport_serveredition.clsOperaImportServerEdition();
                            vLblnSuccess = false;
                        }
                        if (vLblnSuccess)
                        {
                            File.Move(
                                sourceFileName: file,
                                destFileName: Path.Combine(archivePath, "SUCESS_" + string.Join("", invalidCharsRemoved) + "_" + docReturned + "_" + Path.GetFileName(file)));
                            File.Move(
                                pairedFile,
                                destFileName: Path.Combine(archivePath, "SUCESS_" + string.Join("", invalidCharsRemoved) + "_" + docReturned + "_" + Path.GetFileName(pairedFile)));
                            File.Move(
                                sourceFileName: auditPass,
                                destFileName: Path.Combine(archivePath, "SUCESS_" + string.Join("", invalidCharsRemoved) + "_" + docReturned + "_" + Path.GetFileName(auditPass)));
                        }
                        else
                        {
                            File.Move(
                                sourceFileName: file,
                                destFileName: Path.Combine(Path.Combine(archivePath, "FAILED"), "FAILED_" + string.Join("", invalidCharsRemoved) + "_" + docReturned + "_" + Path.GetFileName(file)));
                            File.Move(
                                sourceFileName: pairedFile,
                                destFileName: Path.Combine(Path.Combine(archivePath, "FAILED"), "FAILED_" + string.Join("", invalidCharsRemoved) + "_" + docReturned + "_" + Path.GetFileName(pairedFile)));
                            File.Move(
                                sourceFileName: auditPass,
                                destFileName: Path.Combine(Path.Combine(archivePath, "FAILED"), "FAILED_" + string.Join("", invalidCharsRemoved) + "_" + docReturned + "_" + Path.GetFileName(auditPass)));
                        }
                        fileCurrent.Report(fileNo);
                        completed.Report(pairedFile);
                    }
                }
            }
            catch (Exception ex)
            {
                File.WriteAllText(logFile, "ex catch :  " +  ex.Message);
                Console.WriteLine(ex.Message);
            }
        }
        private static string GetPairedFile(string path, string file, string headerMask, string detailsMask)
        {
            string returnFile = Path.GetFileNameWithoutExtension(file).ToUpper();
            string[] fileHolder = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
            foreach (string fileList in fileHolder)
            {
                string newFile = returnFile.ToUpper().Replace(headerMask.ToUpper(), detailsMask.ToUpper()) + ".CSV";
                //string filePath = Path.GetDirectoryName(file);
                if (Path.GetFileName(fileList.ToUpper()) == newFile.ToUpper())
                {
                    returnFile = fileList;
                    return returnFile;
                }
            }
            return "NF";
        }
    }
}
