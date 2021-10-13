using System;
namespace Stormworks_Japanese_translation_downloader
{
    public class ResponseData
    {
        public string name { get; set; }
        public string zipball_url { get; set; }
        public string tarball_url { get; set; }
        public Commit commit { get; set; }
        public string node_id { get; set; }
    }

    public class Commit
    {
        public string sha { get; set; }
        public string url { get; set; }
    }

}
