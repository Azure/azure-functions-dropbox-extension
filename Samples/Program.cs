using Dropbox.Api;
using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WebJobs.DropboxExtension;

namespace Samples
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var config = new JobHostConfiguration();
            config.DashboardConnectionString = null;
            config.HostId = "sample";

            // Register the dropbox extension
            if (args.Length != 2)
            {
                Console.WriteLine(@"Set arg[0] to the folder name, arg[1] to Dropbox OAuth string");                
                return;
            }
            var folderName = args[0];
            var oauthToken = args[1];
            var ext = new DropboxExtension
            {
                Connection = oauthToken
            };
            config.AddExtension(ext);


            // Invoke some calls that bind to the extension. 
            var param = new Dictionary<string, object>
            {
                { "folder", folderName },
                { "name", "test" }
            };
            var host = new JobHost(config);
            host.CallAsync(nameof(SimpleClient)).Wait();


            host.CallAsync(nameof(SimpleWrite), param).Wait();

            host.CallAsync(nameof(SimpleWriteAsReturn), param).Wait();

            host.CallAsync(nameof(SimpleRead), param).Wait();

            host.CallAsync(nameof(CopyExample), param).Wait();
        }


        public static async Task SimpleClient([Dropbox] DropboxClient client, TextWriter log)
        {
            // Bind to client. 
            var full = await client.Users.GetCurrentAccountAsync();
            log.WriteLine("Email         : {0}", full.Email);
        }

        public static async Task SimpleWrite(
            [Dropbox("/{folder}/{name}.txt")] TextWriter output)
        {
            // Write can bind to the same thingas as Blob: 
            //     Stream, TextWriter, out string, out byte[]. 
            output.Write("Hello");
        }

        [return:Dropbox("/{folder}/{name}.txt")]
        public static async Task<string> SimpleWriteAsReturn()
        {
            // non-null return value will write to file.
            return "Hello again";
        }

        public static async Task SimpleRead(
            [Dropbox("/{folder}/{name}.txt")] string contents)
        {
            // Read can bind to the same things as Blob:
            //    Stream, TextReader, string, byte[] 
            Console.WriteLine(contents);
        }

        // Binding to stream requires that we set the Access parameter for direction. 
        public static async Task CopyExample(
            [Dropbox("/{folder}/{name}.txt", FileAccess.Read)] Stream src,
            [Dropbox("/{folder}/copy-{name}.txt", FileAccess.Write)] Stream dest)
        {
            // Stream writer will handle chunking APIs for us. 
            await src.CopyToAsync(dest);
        }
    }
}
