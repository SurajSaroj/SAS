using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;
namespace sas
{
    class Program
    {
        private static string GetBlobSasUri(CloudBlobContainer container, string blobName, string policyName = null)
        {
            string sasBlobToken;

            // Get a reference to a blob within the container.
            // Note that the blob may not exist yet, but a SAS can still be created for it.
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);

            if (policyName == null)
            {
                // Create a new access policy and define its constraints.
                // Note that the SharedAccessBlobPolicy class is used both to define the parameters of an ad-hoc SAS, and
                // to construct a shared access policy that is saved to the container's shared access policies.
                SharedAccessBlobPolicy adHocSAS = new SharedAccessBlobPolicy()
                {
                    // When the start time for the SAS is omitted, the start time is assumed to be the time when the storage service receives the request.
                    // Omitting the start time for a SAS that is effective immediately helps to avoid clock skew.
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24),
                    Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Create | SharedAccessBlobPermissions.List
                };

                // Generate the shared access signature on the blob, setting the constraints directly on the signature.
                sasBlobToken = blob.GetSharedAccessSignature(adHocSAS);

                Console.WriteLine("SAS for blob (ad hoc): {0}", sasBlobToken);
                Console.WriteLine();
            }
            else
            {
                // Generate the shared access signature on the blob. In this case, all of the constraints for the
                // shared access signature are specified on the container's stored access policy.
                sasBlobToken = blob.GetSharedAccessSignature(null, policyName);

                Console.WriteLine("SAS for blob (stored access policy): {0}", sasBlobToken);
                Console.WriteLine();
            }

            // Return the URI string for the container, including the SAS token.
            return blob.Uri + sasBlobToken;
        }

        static void Main(string[] args)
        {
            //Parse the connection string and return a reference to the storage account.
              CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

            //Create the blob client object.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            //Get a reference to a container to use for the sample code, and create it if it does not exist.
            CloudBlobContainer container = blobClient.GetContainerReference("scripts");
            container.CreateIfNotExists();

            //Generate a SAS URI for a blob within the container, without a stored access policy.
            Console.WriteLine("Blob SAS URI: " + GetBlobSasUri(container, "tstscript.ps1"));
            Console.WriteLine();

            //Require user input before closing the console window.
            Console.ReadLine();
        }

    }
}