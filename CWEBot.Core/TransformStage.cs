using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Serilog;
using Newtonsoft.Json;

using CWEBot.Interfaces;
namespace CWEBot
{
    public class TransformStage
    {
        #region Constructors
        public TransformStage(FileInfo inputFile, FileInfo trainingOutputFile, FileInfo testOutputFile, FileInfo targetOutputFile, Dictionary<string, object> options = null)
        {
            InputFile = inputFile;
            TrainingOuputFile = trainingOutputFile;
            TestOuputFile = testOutputFile;
            TargetOuputFile = targetOutputFile;
            if (options != null)
            {
                if (options.ContainsKey("WithDescription"))
                {
                    WithDescription = (bool)options["WithDescription"];
                }
                if (options.ContainsKey("VulnerabilitiesLimit"))
                {
                    VulnerabilitiesLimit = (int)options["VulnerabilitiesLimit"];
                }
                if (options.ContainsKey("Split"))
                {
                    Split = (int)options["Split"];
                }
            }
        }
        #endregion

        #region Properties
        ILogger L { get; } = Log.ForContext<TransformStage>();
        public FileInfo InputFile { get; protected set; }
        public List<Record> ExtractedRecords { get; protected set; }
        public List<Record> ModelDatasetRecords { get; protected set; }
        public List<Record> TargetDatasetRecords { get; protected set; }
        public FileInfo TrainingOuputFile { get; protected set; }
        public FileInfo TestOuputFile { get; protected set; }
        public FileInfo TargetOuputFile { get; protected set; }
        public List<Record> TrainingRecords { get; protected set; } = new List<Record>();
        public List<Record> TestRecords { get; protected set; } = new List<Record>();
        public int Split { get; protected set; } = 8;
        public bool WithDescription { get; protected set; } = false;
        public int VulnerabilitiesLimit { get; protected set; }
        #endregion

        #region Methods
        public bool CreateModelDataset()
        {
            if (InputFile.Extension == ".gz")
            {
                using (StreamReader f = new StreamReader(new GZipStream(InputFile.OpenRead(), CompressionMode.Decompress)))
                using (JsonTextReader reader = new JsonTextReader(f))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    ExtractedRecords = serializer.Deserialize<List<Record>>(reader);
                }
            }
            else
            {
                using (StreamReader f = new StreamReader(InputFile.OpenRead()))
                using (JsonTextReader reader = new JsonTextReader(f))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    ExtractedRecords = serializer.Deserialize<List<Record>>(reader);
                }

            }
            ModelDatasetRecords = ExtractedRecords.Select(r => TransformRecordWithAvailableCWE(r)).Where(r => r.CWEId.HasValue).ToList();
            TargetDatasetRecords = ExtractedRecords.Select(r => TransformRecordWithAvailableCWE(r)).Where(r => !r.CWEId.HasValue).ToList();
            int vuln_count = 0;
            foreach (Record r in ModelDatasetRecords)
            {
                if (r.VulnerabilityId % 10 < Split)
                {
                    TestRecords.Add(r);
                }
                else
                {
                    TrainingRecords.Add(r);
                }
                if (VulnerabilitiesLimit > 0 && ++vuln_count > VulnerabilitiesLimit) break;
            }

            using (FileStream trfs = new FileStream(TrainingOuputFile.FullName, FileMode.Create))
            using (StreamWriter trswe = new StreamWriter(trfs))
            {
                try
                {
                    foreach (Record r in TrainingRecords)
                    {
                        if (WithDescription && !string.IsNullOrEmpty(r.Description))
                        {
                            trswe.WriteLine("{0}\t{1} {2}", r.CWEId, r.Title, r.Description);
                        }
                        else
                        {
                            trswe.WriteLine("{0}\t{1}.", r.CWEId, r.Title);
                        }
                    }
                    trswe.Flush();
                    L.Information("Wrote {0} vulnerability records to training data file {1}.", TrainingRecords.Count, TrainingOuputFile.FullName);
                }
                catch (IOException ioe)
                {
                    L.Error(ioe, "I/O Error writing to training data file {0}.", TrainingOuputFile.FullName);
                    return false;
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error writing to training data file {0}.", TrainingOuputFile.FullName);
                    return false;
                }

                using (FileStream tefs = new FileStream(TestOuputFile.FullName, FileMode.Create))
                using (StreamWriter teswe = new StreamWriter(tefs))
                {
                    try
                    {
                        foreach (Record r in TestRecords)
                        {
                            if (WithDescription && !string.IsNullOrEmpty(r.Description))
                            {
                                teswe.WriteLine("{0}\t{1} {2}\t{3}", r.CWEId, r.Title, r.Description, r.VulnerabilityId);
                            }
                            else
                            {
                                teswe.WriteLine("{0}\t{1}\t{2}", r.CWEId, r.Title, r.VulnerabilityId);
                            }
                        }
                        teswe.Flush();
                        L.Information("Wrote {0} vulnerability records to test data file {1}.", TestRecords.Count, TestOuputFile.FullName);
                    }
                    catch (IOException ioe)
                    {
                        L.Error(ioe, "I/O Error writing to test data file {0}.", TestOuputFile.FullName);
                        return false;
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Error writing to test data file {0}.", TestOuputFile.FullName);
                        return false;
                    }
                }

                using (FileStream tarfs = new FileStream(TargetOuputFile.FullName, FileMode.Create))
                using (StreamWriter tarswe = new StreamWriter(tarfs))
                {
                    try
                    {
                        foreach (Record r in TargetDatasetRecords)
                        {

                            if (WithDescription && !string.IsNullOrEmpty(r.Description))
                            {
                                tarswe.WriteLine("{0}\t{1} {2}\t{3}", string.Empty, r.Title, r.Description, r.VulnerabilityId);
                            }
                            else
                            {
                                tarswe.WriteLine("{0}\t{1}\t{2}", string.Empty, r.Title, r.VulnerabilityId);
                            }
                        }
                        tarswe.Flush();
                        L.Information("Wrote {0} vulnerability records to target data file {1}.", TargetDatasetRecords.Count, TargetOuputFile.FullName);
                    }
                    catch (IOException ioe)
                    {
                        L.Error(ioe, "I/O Error writing to target data file {0}.", TargetOuputFile.FullName);
                        return false;
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Error writing to target data file {0}.", TargetOuputFile.FullName);
                        return false;
                    }
                }
                return true;
            }
        }

        public Record TransformRecordWithAvailableCWE(Record r)
        {
            if (!r.CWEId.HasValue && r.References != null && r.References.Count() > 0)
            {
                foreach( string reference in r.References)
                {
                    Uri uri = null;
                    if (!Uri.TryCreate(reference, UriKind.Absolute, out uri))
                    {
                        L.Warning("Reference {ref} is not a Uri.", reference);
                        continue;
                    }
                    if (uri.Host == "cwe.mitre.org")
                    {
                        int cwe_id = -1;
                        if (Int32.TryParse(uri.Segments.Last().Replace(".html", string.Empty), out cwe_id))
                        {
                            r.CWEId = cwe_id;
                            L.Information("Got CWE Id {id} from reference {uri} for vulnerability record {vid}.", cwe_id, uri, r.VulnerabilityId);
                            continue;
                        }
                    }

                }
            }
            return r;        
        }
        #endregion

    }
}
