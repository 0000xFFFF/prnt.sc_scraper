using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Collections;

namespace kraken20
{
    class kraken20
    {
        private static char[] chars = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        
        static int Main(string[] args)
        {
            Console.WriteLine("This program will download screenshots (from https://prnt.sc) by random...");
            Console.WriteLine("Ignore Image: \"The screenshot was removed\".png");
            Console.WriteLine("Chars: ASCII lower + numbers");
            Console.WriteLine("Author: 0xC0LD");
            Console.WriteLine("URL E.G.: https://prnt.sc/asd123");
            Console.WriteLine("");

            Hashtable hashtable = new Hashtable();

            while (true)
            {
                try
                {
                    Random rm = new Random();

                    string url = "https://prnt.sc/"
                        + chars[rm.Next(0, chars.Length - 1)]
                        + chars[rm.Next(0, chars.Length - 1)]
                        + chars[rm.Next(0, chars.Length - 1)]

                        + chars[rm.Next(0, chars.Length - 1)]
                        + chars[rm.Next(0, chars.Length - 1)]
                        + chars[rm.Next(0, chars.Length - 1)]
                    ;

                    if (!hashtable.Contains(url))
                    {
                        hashtable.Add(url, url);
                        download_ss(url);
                    }
                }
                catch(Exception e) { Console.WriteLine(e.Message);  }
            }
        }
        
        private static void download_ss(string url)
        {
            WebClient wc = new WebClient();
            wc.Headers.Add("user-agent", "kraken20.exe");

            foreach (string link in get_html_all(new Uri(url), wc.DownloadData(url)))
            {
                if (string.IsNullOrEmpty(link)) { continue; }
                if (!Uri.IsWellFormedUriString(link, UriKind.Absolute)) { continue; }
                if (!link.Contains(".png")) { continue; }
                if (link.Contains("favicon")) { continue; }
                if (link.Contains("footer-logo.png")) { continue; }
                if (link.Contains("icon")) { continue; }
                if (link.Contains("searchbyimage")) { continue; }

                string name = GetURLFilename(url);
                byte[] bytes = wc.DownloadData(link);

                Bitmap bmp;
                using (var ms = new MemoryStream(bytes)) { bmp = new Bitmap(ms); }

                if (!compare(bmp, Properties.Resources.ignore))
                {
                    Console.WriteLine(url + " -> " + link + " -> " + name);
                    File.WriteAllBytes(name, bytes);
                }
            }
        }

        private static bool compare(Bitmap bmp1, Bitmap bmp2)
        {
            bool equals = true;
            bool flag = true;  //Inner loop isn't broken

            //Test to see if we have the same size of image
            if (bmp1.Size == bmp2.Size)
            {
                for (int x = 0; x < bmp1.Width; ++x)
                {
                    for (int y = 0; y < bmp1.Height; ++y)
                    {
                        if (bmp1.GetPixel(x, y) != bmp2.GetPixel(x, y))
                        {
                            equals = false;
                            flag = false;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        break;
                    }
                }
            }
            else
            {
                equals = false;
            }
            return equals;
        }

        private static string GetURLFilename(string url)
        {
            string[] index = url.Split('/');
            string name = index[index.Length - 1];
            return name + ".png";
        }

        private static string time()
        {
            TimeSpan diff = (new DateTime(2011, 02, 10) - new DateTime(2011, 02, 01));
            return diff.TotalMilliseconds.ToString();
        }

        private static List<string> root_urls(Uri website, List<string> urls)
        {
            List<string> items = new List<string>();

            foreach (string url in urls)
            {
                string item = System.Web.HttpUtility.HtmlDecode(url);

                if (Uri.IsWellFormedUriString(item, UriKind.Absolute))
                {
                    items.Add(item);

                    continue;
                }

                if (item.StartsWith("//")) //add http or https
                {
                    items.Add(website.ToString().Split(':')[0] + ":" + item);
                    items.Add(website.ToString().Split(':')[0] + ":" + item);
                }
                else if (item.StartsWith("/")) //add website name
                {
                    if (website.ToString().EndsWith("/"))
                    {
                        items.Add(website.ToString() + item);
                    }
                    else
                    {
                        items.Add(website.ToString() + "/" + item);
                    }
                }
                else
                {
                    items.Add(item); //add junk
                }
            }

            return items;
        }
        private static List<string> get_html_all(Uri URL, byte[] html)
        {
            //GET ALL CHARS
            char[] htmlChars = System.Text.Encoding.Default.GetString(html).ToArray();

            //EXTRACT ALL STRINGS LIKE: 'asdsadas' "asdasdasd"
            List<string> html_quotes = html_get_everyobject_in_quotes(htmlChars);
            List<string> html_apostrophes = html_get_everyobject_in_apostrophes(htmlChars);

            //ADD
            List<string> non_rooted = new List<string>();
            foreach (string l in html_quotes) { non_rooted.Add(l); }
            foreach (string l in html_apostrophes) { non_rooted.Add(l); }


            /// Uri myUri = new Uri("http://www.contoso.com:8080/");
            /// string host = myUri.Host;  // host is "www.contoso.com"

            //ROOT
            List<string> html_all = root_urls(URL, non_rooted);

            //CLEAR DUPLICATES
            html_all = html_all.Distinct().ToList(); //REMOVE SAME URLS

            return html_all;
        }
        private static List<string> html_get_everyobject_in_apostrophes(char[] htmlChars)
        {
            //get urls like this: blablablablablablabla "some url we want" blablablablabla

            List<string> links = new List<string>();
            string link = "";
            bool afterQuote = false;
            foreach (char ch in htmlChars)
            {
                if (ch == '\'')
                {
                    afterQuote = !afterQuote;

                    if (!afterQuote)
                    {
                        links.Add(link);
                        link = "";
                    }
                }
                else if (afterQuote)
                {
                    link = link + ch; //add chars to string after quote
                }
            }

            return links;
        }
        private static List<string> html_get_everyobject_in_quotes(char[] htmlChars)
        {
            //get urls like this: blablablablablablabla "some url we want" blablablablabla

            List<string> links = new List<string>();
            string link = "";
            bool afterQuote = false;
            foreach (char ch in htmlChars)
            {
                if (ch == '"')
                {
                    afterQuote = !afterQuote;

                    if (!afterQuote)
                    {
                        links.Add(link);
                        link = "";
                    }
                }
                else if (afterQuote)
                {
                    link = link + ch; //add chars to string after quote
                }
            }

            return links;
        }
    }
}
