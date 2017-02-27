using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Serilog;
using Newtonsoft.Json;

using CWEBot.Interfaces;
namespace CWEBot
{
    public class Transform<T>
    {
        #region Constructors
        public Transform(FileInfo inputFile, FileInfo trainingOutputFile, FileInfo testOutputFile, FileInfo targetOutputFile)
        {
            InputFile = inputFile;
            TrainingOuputFile = trainingOutputFile;
            TestOuputFile = testOutputFile;
            TargetOuputFile = targetOutputFile;
        }
        #endregion

        #region Properties
        public FileInfo InputFile { get; protected set; }
        public List<Record> ExtractedRecords { get; protected set; }
        public List<Record> ModelDatasetRecords { get; protected set; }
        public List<Record> TargetDatasetRecords { get; protected set; }
        public FileInfo TrainingOuputFile { get; protected set; }
        public FileInfo TestOuputFile { get; protected set; }
        public FileInfo TargetOuputFile { get; protected set; }
        public List<Record> TrainingRecords { get; protected set; } = new List<Record>();
        public List<Record> TestRecords { get; protected set; } = new List<Record>();
        
        ILogger L { get; } = Log.ForContext<Transform<T>>();
        #endregion

        #region Methods
        public bool CreateModelDataset()
        {
            using (StreamReader f = new StreamReader(InputFile.OpenRead()))
            using (JsonTextReader reader = new JsonTextReader(f))
            {
                JsonSerializer serializer = new JsonSerializer();
                ExtractedRecords = serializer.Deserialize<List<Record>>(reader);
            }
            ModelDatasetRecords = ExtractedRecords.Select(r => TransformRecordWithCWE(r)).Where(r => r.CWEId.HasValue).ToList();
            TargetDatasetRecords = ExtractedRecords.Select(r => TransformRecordWithCWE(r)).Where(r => !r.CWEId.HasValue).ToList();
            foreach (Record r in ModelDatasetRecords)
            {
                if (r.VulnerabilityId % 10 <= 8)
                {
                    TestRecords.Add(r);
                }
                else
                {
                    TrainingRecords.Add(r);
                }
            }

            using (FileStream trfs = new FileStream(TrainingOuputFile.FullName, FileMode.Create))
            using (StreamWriter trswe = new StreamWriter(trfs))
            {
                try
                {
                    foreach (Record r in TrainingRecords)
                    {
                        trswe.WriteLine("{0}\t{1}\t{2}. {3}.", r.CWEId, r.VulnerabilityId, r.Title, r.Description);
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
                            teswe.WriteLine("{0}\t{1}\t{2}. {3}.", r.CWEId, r.VulnerabilityId, r.Title, r.Description);
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
                            tarswe.WriteLine("{0}\t{1}\t{2}. {3}.", r.CWEId, r.VulnerabilityId, r.Title, r.Description);
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

        public Record TransformRecordWithCWE(Record r)
        {
            if (r.References != null && r.References.Count() > 0)
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
