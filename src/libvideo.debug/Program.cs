using System;
using System.Diagnostics;
using System.IO;
using VideoLibrary;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;

namespace VideoLibrary.Debug
{
    class Program
    {
        //static void Main()
        //{
        //    string[] queries =
        //    {
        //        //"https://www.youtube.com/watch?v=jfobiCq0YUc&ab_channel=EminemMusic"//1080
        //        "https://www.youtube.com/watch?v=LXb3EKWsInQ&ab_channel=Jacob%2BKatieSchwarz",//2060
        //    };



        //    //TestVideoLib(queries);
        //    test("");
        //    Console.WriteLine("Done.");
        //    Console.ReadKey();
        //}


        static async Task Main(string[] args)
        {

            var youtube = YouTube.Default;
            var video = youtube.GetVideo("");
            var videoInfos = Client.For(youtube).GetAllVideosAsync("").GetAwaiter().GetResult();
            var maxResolution = videoInfos.First(i => i.Resolution == videoInfos.Max(j => j.Resolution));
            
            var client = new HttpClient();
            long? totalByte = 0;
            using (Stream output = File.OpenWrite("C:\\Users\\[Something]\\[Folder]\\" + maxResolution.FullName))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Head, maxResolution.Uri))
                {
                    totalByte = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result.Content.Headers.ContentLength;
                }
                using (var input = await client.GetStreamAsync(maxResolution.Uri))
                {
                    byte[] buffer = new byte[16 * 1024];
                    int read;
                    int totalRead = 0;
                    Console.WriteLine("Download Started");
                    while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        output.Write(buffer, 0, read);
                        totalRead += read;
                        Console.Write($"\rDownloading {totalRead}/{totalByte} ...");
                    }
                    Console.WriteLine("Download Complete");
                }
            }
            Console.ReadLine();
        }


        public static void SaveVideoToDisk(string link)
        {
            var youTube = YouTube.Default; // starting point for YouTube actions
            var videoInfos = Client.For(YouTube.Default).GetAllVideosAsync(link).GetAwaiter().GetResult();           
            var maxResolution = videoInfos.First(i => i.Resolution == videoInfos.Max(j => j.Resolution));
            var video = youTube.GetVideo(maxResolution.Uri); // gets a Video object with info about the video
            int resolution = video.Resolution;
            File.WriteAllBytes(@"C:\Users\Floppa\Desktop\" + video.FullName, video.GetBytes());
        }

        public static void test(string link)
        {
            // Custom Youtube
            var youtube = new CustomYouTube();
            var videos = youtube.GetAllVideosAsync(link).GetAwaiter().GetResult();
            var maxResolution = videos.First(i => i.Resolution == videos.Max(j => j.Resolution));
            youtube.CreateDownloadAsync(
                new Uri(maxResolution.Uri),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), maxResolution.FullName),
                new Progress<Tuple<long, long>>((Tuple<long, long> v) =>
                {
                    var percent = (int)((v.Item1 * 100) / v.Item2);
                    Console.Write(string.Format("Downloading.. ( % {0} ) {1} / {2} MB\r", percent, (v.Item1 / (double)(1024 * 1024)).ToString("N"), (v.Item2 / (double)(1024 * 1024)).ToString("N")));
                })).GetAwaiter();
        }

        public static void TestVideoLib(string[] queries)
        {
            using (var cli = Client.For(YouTube.Default))
            {
                Console.WriteLine("Downloading...");
                for (int i = 0; i < queries.Length; i++)
                {
                    string uri = queries[i];
                    try
                    {
                        var videoInfos = cli.GetAllVideosAsync(uri).GetAwaiter().GetResult();
                        Console.WriteLine($"Link #{i + 1}");
                        foreach (YouTubeVideo v in videoInfos)
                        {
                            if (v.Resolution > 0 && v.AudioBitrate < 0)
                            {
                                Console.WriteLine(v.Uri);
                                Console.WriteLine(string.Format($"Full Title\t{v.Title + v.FileExtension}\nType\t{v.AdaptiveKind}\nResolution\t{v.Resolution}p\nFormat\t{v.FormatCode}\nFPS\t{v.Fps}\nBitrate\t{v.AudioBitrate}\n"));
                                Console.WriteLine("Success : " + v.Head().CanRead);
                                Console.WriteLine();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e);
                        Debugger.Break();
                    }
                }
            }
        }
    }
}