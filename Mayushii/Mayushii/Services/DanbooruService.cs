﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace Mayushii.Services
{
    internal static class DanbooruService
    {
        private static JavaScriptSerializer json;
        private static Random random;

        static DanbooruService()
        {
            json = new JavaScriptSerializer();
            random = new Random();
        }

        public static string GetRandomImage(List<string> tags)
        {
            Post[] posts = GetPosts(tags).Where(post => post.ImageUrl != null).ToArray();
            if (posts.Length > 0)
            {
                return posts[random.Next(0, posts.Length)].ImageUrl;
            }
            else
            {
                return null;
            }
        }

        private static Post[] GetPosts(List<string> tags, int? page = null)
        {
            StringBuilder urlBuilder = new StringBuilder();
            urlBuilder.AppendFormat("https://danbooru.donmai.us/posts.json?limit=100&tags={0}", string.Join("*+*", tags));
            if (page != null)
            {
                urlBuilder.AppendFormat("&page={0}", page);
            }
            HttpWebRequest postRequest = WebRequest.CreateHttp(urlBuilder.ToString());
            try
            {
                WebResponse response = postRequest.GetResponse();
                string postResponse = new StreamReader(response.GetResponseStream()).ReadToEnd();
                return json.Deserialize<Post[]>(postResponse);
            }
            catch (WebException)
            {
                return new Post[0];
            }
        }

#pragma warning disable 0649

        private class Post
        {
            public string large_file_url;

            public string ImageUrl
            {
                get
                {
                    return string.IsNullOrEmpty(large_file_url) ? null : "https://danbooru.donmai.us" + large_file_url;
                }
            }
        }

#pragma warning restore 0649
    }
}