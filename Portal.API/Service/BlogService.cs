using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ServiceModel.Syndication;
using System.Xml;
using Portal.API.Interfaces.Services;
using Portal.API.Entities;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Xml.Linq;

namespace Portal.API.Service
{
    public class BlogService : IBlogService
    {
        public async Task<List<Post>> GetPosts()
        {
            var result = new List<Post>();
            await Task.Run(() =>
            {
                try
                {
                    var rssFeed = new Uri("https://www.bullla.com.br/categoria/recursos-humanos/feed/");
                    var request = (HttpWebRequest)WebRequest.Create(rssFeed);
                    request.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                    request.Method = "GET";
                    request.Timeout = 20000;
                    var response = (HttpWebResponse)request.GetResponse();
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        var feedContents = reader.ReadToEnd();
                        var document = XDocument.Parse(feedContents);

                        var posts = (from p in document.Descendants("item")
                                     select new
                                     {
                                         Title = p.Element("title").Value,
                                         Link = p.Element("link").Value,
                                         Category = p.Element("category").Value,
                                         Description = p.Element("description").Value,
                                         PubDate = DateTime.Parse(p.Element("pubDate").Value)
                                     }).ToList();

                        foreach (var post in posts)
                        {
                            string matchString = Regex.Match(post.Description, "<img.+?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase).Groups[1].Value;
                            result.Add(new Post()
                            {
                                Category = post.Category,
                                Link = post.Link,
                                Title = post.Title,
                                //Creator = post.Authors.FirstOrDefault().Name,
                                Description = post.Description,
                                PublishDate = post.PubDate,
                                Image = matchString,
                                Summary = GetFirstParagraph(post.Description)



                            });
                        }
                    }
                    //var url = "https://www.bullla.com.br/feed";
                    //using var reader = XmlReader.Create(url);
                    //var feed = SyndicationFeed.Load(reader);
                    //foreach (var post in feed.Items)
                    //{
                    //    string matchString = Regex.Match(post.Summary.Text, "<img.+?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase).Groups[1].Value;
                    //    posts.Add(new Post()
                    //    {
                    //        Category = post.Categories.FirstOrDefault().Name,
                    //        Link = post.Links.FirstOrDefault().Uri.ToString(),
                    //        Title = post.Title.Text,
                    //        //Creator = post.Authors.FirstOrDefault().Name,
                    //        Description = post.Summary.Text,
                    //        PublishDate = post.PublishDate.LocalDateTime,
                    //        Image = matchString,
                    //        Summary = GetFirstParagraph(post.Summary.Text)



                    //    });
                    //}
                }
                catch (Exception)
                {
                     
                }                
            });
            return result;
        }
        private string GetFirstParagraph(string htmltext)
        {
            Match m = Regex.Match(htmltext, @"<p>\s*(.+?)\s*</p>");
            if (m.Success)
            {
                return m.Groups[1].Value;
            }
            else
            {
                return htmltext;
            }
        }
    }
}
