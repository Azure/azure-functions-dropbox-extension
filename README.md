# Overview 
This is a Dropbox binding for Azure Functions / WebJobs. It relies on the Dropbox.Net sdk (see  https://github.com/dropbox/dropbox-sdk-dotnet/ ) 

This supports binding to the DropboxClient object and binding to streams. 

This extension is published on nuget as:  https://www.myget.org/feed/azure-appservice/package/nuget/Microsoft.Azure.WebJobs.Extensions.DropBox


# Examples
Set the Dropbox OAuth token in an appsetting. In the examples below, it uses the appsetting named 'cx1'. 
Here's how to get an Oauth token for your account: https://blogs.dropbox.com/developers/2014/05/generate-an-access-token-for-your-own-account/ 

See more examples in the Sample's project at https://github.com/Azure/azure-functions-dropbox-extension/blob/master/Samples/Program.cs

## Read 
        
        public static async Task SimpleRead(
          [Dropbox("/Folder/File", Connection = "cx1")] string str
          )
        {     
           // Reads contents as string.
           // Could also bind to Byte[], TextReader, Stream and other types as [Blob]
        }

## Write 
        public static async Task SimpleWrite(
          [Dropbox("/Folder/File", Connection = "cx1")] out string str
          )
        {     
           // Write  as string 
           // Could also bind to out byte[], TextWriter, Stream, and other types like [Blob]
        }

## Stream Operations with large files 
        public static async Task StreamCopy(
            [Blob("test/input.txt", FileAccess.Read)] Stream src,
            [Dropbox("/FuncTest7/w6.txt", Connection = "cx1", Access = FileAccess.Write)] Stream dest
            )
        {
            // Dropbox stream is chunking and supports large file copies. 
            await src.CopyToAsync(dest);
        }

## Trigger not provided
This binding does not have triggers. 
You can subscribe to a Dropbox Webhook (see https://www.dropbox.com/developers/reference/webhooks ) via HTTP Trigger and then use these bindings in conjuction with that. 

# Consuming from Functions v2
This does not work on Azure Functions v1.
To consume from Azure Functions, you must load the extension (see https://github.com/Azure/azure-webjobs-sdk-script/wiki/Binding-Extensions-Management ) 
The binding type is "Dropbox". Note that dropbox paths start with a '/' whereas Blob paths do not. Elsewise, this behaves just like blob. 


# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
