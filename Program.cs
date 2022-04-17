using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Newtonsoft.Json;

namespace HelloWorld
{
    class Program
    {
        static DriveService service;
        static void Main(string[] args)
        {
            Console.WriteLine("Authenticate to servic account");
            string serviceAccountEmail = "testservice@gdrivetesting-346305.iam.gserviceaccount.com";
            string credentialPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "credentials.json");
            service =  AuthenticateServiceAccount(serviceAccountEmail,credentialPath);
            //UploadFile("E:\\Testing\\GDriveIntegration\\sample.jpg");
            DownloadFile("example.jpg","E:\\Testing\\GDriveIntegration\\downloadedsample.jpg");
        }

        static DriveService AuthenticateServiceAccount(string serviceAccountEmail, string keyFilePath)
        {
            if (!File.Exists(keyFilePath))
            {
                Console.WriteLine("An Error occurred - Key file does not exist");
                return null;
            }

            string[] scopes = new string[]
            {
                DriveService.Scope.Drive,  // view and manage your files and documents
            };

            var json = File.ReadAllText(keyFilePath);
            Newtonsoft.Json.Linq.JObject cr = JsonConvert.DeserializeObject(json) as Newtonsoft.Json.Linq.JObject;
            string s = (string)cr.GetValue("private_key");
            
            try
            {
                ServiceAccountCredential credential = new ServiceAccountCredential(
                    new ServiceAccountCredential.Initializer(serviceAccountEmail)
                    {
                        Scopes = scopes
                    }.FromPrivateKey(s));

                // Create the service.
                DriveService service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "TestApp",
                });
                Console.WriteLine("Authentication Success");
                return service;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException);
                return null;
            }
        }

        public static string UploadFile(string uploadFilePath)
		{
			IDictionary<string, string> properties = new Dictionary<string, string>();
			properties.Add("FileName", "Test Photo");

			var fileMetadata2 = new Google.Apis.Drive.v3.Data.File();
			fileMetadata2.Name = "example.jpg";
            fileMetadata2.Parents = new List<string> { "1OKh0AFwkv2n5VLzB_mKUyx7WasURBfMb" }; // your parent folder ID
			fileMetadata2.AppProperties = properties;
			FilesResource.CreateMediaUpload request3;
			using (var stream = new System.IO.FileStream(uploadFilePath,System.IO.FileMode.Open))
			{
				request3 = service.Files.Create(
					fileMetadata2, stream, "image/jpeg");
				request3.Fields = "id";
				request3.Upload();
			}
			
			var file3 = request3.ResponseBody;
			Console.WriteLine("Upload success and fileID is " + file3.Id);
			return file3.Id;
		}

        static string DownloadFile(string filename,string saveFilePath)
		{
			string fileID = "";
			var requestSearch = service.Files.List();
			requestSearch.Q = $"mimeType = 'image/jpeg' and name = '{filename}'";
			requestSearch.Fields = "nextPageToken, files(*)";
			var res = requestSearch.Execute();
			if (res.Files.Count > 0) fileID = res.Files[0].Id;

			var request = service.Files.Get(fileID);
			var stream2 = new MemoryStream();
			request.Download(stream2);

			FileStream file = new FileStream(saveFilePath, FileMode.Create, FileAccess.Write);
			stream2.WriteTo(file);
			file.Close();
            Console.WriteLine("Successfully Downlaod file "+saveFilePath);
			return saveFilePath;
		}
    }
}