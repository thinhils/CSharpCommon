﻿using System;
using System.IO.Compression;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Web;

namespace Core.Html
{
    /// <summary>
    ///1、获取HTML
    ///1.1获取指定页面的HTML代码 GetHtml(string url, string postData, bool isPost, CookieContainer cookieContainer)
    ///1.2获取HTMLGetHtml(string url, CookieContainer cookieContainer)
    ///2、获取字符流
    ///2.1获取字符流GetStream(string url, CookieContainer cookieContainer)
    ///3、清除HTML标记 
    ///3.1清除HTML标记  NoHTML(string Htmlstring)
    ///4、匹配页面的链接 
    ///4.1获取页面的链接正则 GetHref(string HtmlCode)
    ///5、匹配页面的图片地址
    /// 5.1匹配页面的图片地址 GetImgSrc(string HtmlCode, string imgHttp)
    ///5.2匹配<img src="" />中的图片路径实际链接  GetImg(string ImgString, string imgHttp)
    ///6、抓取远程页面内容
    /// 6.1以GET方式抓取远程页面内容 Get_Http(string tUrl)
    /// 6.2以POST方式抓取远程页面内容 Post_Http(string url, string postData, string encodeType)
    ///7、压缩HTML输出
    ///7.1压缩HTML输出 ZipHtml(string Html)
    ///8、过滤HTML标签
    /// 8.1过滤指定HTML标签 DelHtml(string s_TextStr, string html_Str)  
    /// 8.2过滤HTML中的不安全标签 RemoveUnsafeHtml(string content)
    /// HTML转行成TEXT HtmlToTxt(string strHtml)
    /// 字符串转换为 HtmlStringToHtml(string str)
    /// html转换成字符串HtmlToString(string strHtml)
    /// 获取URL编码
    /// 判断URL是否有效
    /// 返回 HTML 字符串的编码解码结果
    /// </summary>
    public class HtmlHelper
    {
        #region 私有字段
        private static CookieContainer cc = new CookieContainer();
        private static string contentType = "application/x-www-form-urlencoded";
        private static string accept = "image/gif, image/x-xbitmap, image/jpeg, image/pjpeg," +
                                       " application/x-shockwave-flash, application/x-silverlight, " +
                                       "application/vnd.ms-excel, application/vnd.ms-powerpoint, " +
                                       "application/msword, application/x-ms-application," +
                                       " application/x-ms-xbap," +
                                       " application/vnd.ms-xpsdocument, application/xaml+xml, application/x-silverlight-2-b1, */*";
        private static string userAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1;" +
                                          " .NET CLR 2.0.50727; .NET CLR 3.0.04506.648; .NET CLR 3.5.21022)";
        private static Encoding encoding = Encoding.GetEncoding("utf-8");
        private static int delay = 1000;
        private static int maxTry = 300;
        private static int currentTry = 0;
        #endregion

        #region 公有属性
        /// <summary> 
        /// Cookie容器
        /// </summary> 
        public static CookieContainer CookieContainer
        {
            get
            {
                return cc;
            }
        }

        /// <summary> 
        /// 获取网页源码时使用的编码
        /// </summary> 
        public static Encoding Encoding
        {
            get
            {
                return encoding;
            }
            set
            {
                encoding = value;
            }
        }

        public static int NetworkDelay
        {
            get
            {
                Random r = new Random();
                return (r.Next(delay, delay * 2));
                // return (r.Next(delay / 1000, delay / 1000 * 2)) * 1000;
            }
            set
            {
                delay = value;
            }
        }

        public static int MaxTry
        {
            get
            {
                return maxTry;
            }
            set
            {
                maxTry = value;
            }
        }
        #endregion

        #region 1、获取HTML
        /// <summary>
        /// 1.1获取指定页面的HTML代码
        /// </summary>
        /// <param name="url">指定页面的路径</param>
        /// <param name="postData">post 提交的字符串</param>
        /// <param name="isPost">是否以post方式发送请求</param>
        /// <param name="cookieContainer">Cookie集合</param>
        public static string GetHtml(string url, string postData, bool isPost, CookieContainer cookieContainer)
        {
            if (string.IsNullOrEmpty(postData))
            {
                return GetHtml(url, cookieContainer);
            }
            Thread.Sleep(NetworkDelay);
            currentTry++;
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;
            try
            {
                byte[] byteRequest = Encoding.Default.GetBytes(postData);

                httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                httpWebRequest.CookieContainer = cookieContainer;
                httpWebRequest.ContentType = contentType;
                httpWebRequest.ServicePoint.ConnectionLimit = maxTry;
                httpWebRequest.Referer = url;
                httpWebRequest.Accept = accept;
                httpWebRequest.UserAgent = userAgent;
                httpWebRequest.Method = isPost ? "POST" : "GET";
                httpWebRequest.ContentLength = byteRequest.Length;

                httpWebRequest.AllowAutoRedirect = false;

                Stream stream = httpWebRequest.GetRequestStream();
                stream.Write(byteRequest, 0, byteRequest.Length);
                stream.Close();

                try
                {
                    httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    //redirectURL = httpWebResponse.Headers["Location"];// Get redirected uri
                }
                catch (WebException ex)
                {
                    httpWebResponse = (HttpWebResponse)ex.Response;
                }
                //httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                Stream responseStream = httpWebResponse.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream, encoding);
                string html = streamReader.ReadToEnd();
                streamReader.Close();
                responseStream.Close();
                currentTry = 0;
                httpWebRequest.Abort();
                httpWebResponse.Close();
                return html;
            }
            catch (Exception e)
            {
                if (currentTry <= maxTry)
                {
                    GetHtml(url, postData, isPost, cookieContainer);
                }
                currentTry--;
                if (httpWebRequest != null) httpWebRequest.Abort();
                if (httpWebResponse != null) httpWebResponse.Close();
                return string.Empty;
            }
        }


        /// <summary>
        /// 1.2获取HTML
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="cookieContainer">Cookie集合</param>
        public static string GetHtml(string url, CookieContainer cookieContainer)
        {
            Thread.Sleep(NetworkDelay);
            currentTry++;
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;
            try
            {
                httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                httpWebRequest.CookieContainer = cookieContainer;
                httpWebRequest.ContentType = contentType;
                httpWebRequest.ServicePoint.ConnectionLimit = maxTry;
                httpWebRequest.Referer = url;
                httpWebRequest.Accept = accept;
                httpWebRequest.UserAgent = userAgent;
                httpWebRequest.Method = "GET";
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream responseStream = httpWebResponse.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream, encoding);
                string html = streamReader.ReadToEnd();
                streamReader.Close();
                responseStream.Close();
                currentTry--;
                httpWebRequest.Abort();
                httpWebResponse.Close();
                return html;
            }
            catch (Exception e)
            {
                if (currentTry <= maxTry) GetHtml(url, cookieContainer);
                currentTry--;
                if (httpWebRequest != null) httpWebRequest.Abort();
                if (httpWebResponse != null) httpWebResponse.Close();
                return string.Empty;
            }
        }
        #endregion

        #region 2、获取字符流
        /// <summary>
        /// 2.1获取字符流
        /// </summary>
        //---------------------------------------------------------------------------------------------------------------
        // 示例:
        // System.Net.CookieContainer cookie = new System.Net.CookieContainer(); 
        // Stream s = HttpHelper.GetStream("http://ptlogin2.qq.com/getimage?aid=15000102&0.43878429697395826", cookie);
        // picVerify.Image = Image.FromStream(s);
        //---------------------------------------------------------------------------------------------------------------
        /// <param name="url">地址</param>
        /// <param name="cookieContainer">cookieContainer</param>
        public static Stream GetStream(string url, CookieContainer cookieContainer)
        {
            currentTry++;

            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;

            try
            {
                httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                httpWebRequest.CookieContainer = cookieContainer;
                httpWebRequest.ContentType = contentType;
                httpWebRequest.ServicePoint.ConnectionLimit = maxTry;
                httpWebRequest.Referer = url;
                httpWebRequest.Accept = accept;
                httpWebRequest.UserAgent = userAgent;
                httpWebRequest.Method = "GET";

                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream responseStream = httpWebResponse.GetResponseStream();
                currentTry--;
                return responseStream;
            }
            catch (Exception e)
            {
                if (currentTry <= maxTry)
                {
                    GetHtml(url, cookieContainer);
                }

                currentTry--;

                if (httpWebRequest != null)
                {
                    httpWebRequest.Abort();
                } if (httpWebResponse != null)
                {
                    httpWebResponse.Close();
                }
                return null;
            }
        }
        #endregion

        #region 3、清除HTML标记
        ///<summary>   
        ///3.1清除HTML标记   
        ///</summary>   
        ///<param name="NoHTML">包括HTML的源码</param>   
        ///<returns>已经去除后的文字</returns>   
        public static string RemoveHTML(string Htmlstring)
        {
            //删除脚本   
            Htmlstring = Regex.Replace(Htmlstring, @"<script[^>]*?>.*?</script>", "", RegexOptions.IgnoreCase);

            //删除HTML   
            Regex regex = new Regex("<.+?>", RegexOptions.IgnoreCase);
            Htmlstring = regex.Replace(Htmlstring, "");
            Htmlstring = Regex.Replace(Htmlstring, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"-->", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"<!--.*", "", RegexOptions.IgnoreCase);

            Htmlstring = Regex.Replace(Htmlstring, @"&(quot|#34);", "\"", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(nbsp|#160);", "   ", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(iexcl|#161);", "\xa1", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(copy|#169);", "\xa9", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&#(\d+);", "", RegexOptions.IgnoreCase);

            Htmlstring.Replace("<", "");
            Htmlstring.Replace(">", "");
            Htmlstring.Replace("\r\n", "");

            return Htmlstring;
        }


        #endregion

        #region 4、匹配页面的链接
        #region 4.1获取页面的链接正则
        /// <summary>
        /// 4.1获取页面的链接正则
        /// </summary>
        public string GetHref(string HtmlCode)
        {
            string MatchVale = "";
            string Reg = @"(h|H)(r|R)(e|E)(f|F) *= *('|"")?((\w|\\|\/|\.|:|-|_)+)[\S]*";
            foreach (Match m in Regex.Matches(HtmlCode, Reg))
            {
                MatchVale += (m.Value).ToLower().Replace("href=", "").Trim() + "|";
            }
            return MatchVale;
        }
        #endregion

        #region  4.2取得所有链接URL
        /// <summary>
        /// 4.2取得所有链接URL
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string GetAllURL(string html)
        {
            StringBuilder sb = new StringBuilder();
            Match m = Regex.Match(html.ToLower(), "<a href=(.*?)>.*?</a>");

            while (m.Success)
            {
                sb.AppendLine(m.Result("$1"));
                m.NextMatch();
            }

            return sb.ToString();
        }
        #endregion

        #region 4.3获取所有连接文本
        /// <summary>
        /// 4.3获取所有连接文本
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string GetAllLinkText(string html)
        {
            StringBuilder sb = new StringBuilder();
            Match m = Regex.Match(html.ToLower(), "<a href=.*?>(1,100})</a>");

            while (m.Success)
            {
                sb.AppendLine(m.Result("$1"));
                m.NextMatch();
            }

            return sb.ToString();
        }
        #endregion
        #endregion

        #region  5、匹配页面的图片地址
        /// <summary>
        /// 5.1匹配页面的图片地址
        /// </summary>
        /// <param name="imgHttp">要补充的http://路径信息</param>
        public string GetImgSrc(string HtmlCode, string imgHttp)
        {
            string MatchVale = "";
            string Reg = @"<img.+?>";
            foreach (Match m in Regex.Matches(HtmlCode.ToLower(), Reg))
            {
                MatchVale += GetImg((m.Value).ToLower().Trim(), imgHttp) + "|";
            }

            return MatchVale;
        }


        /// <summary>
        /// 5.2匹配<img src="" />中的图片路径实际链接
        /// </summary>
        /// <param name="ImgString"><img src="" />字符串</param>
        public string GetImg(string ImgString, string imgHttp)
        {
            string MatchVale = "";
            string Reg = @"src=.+\.(bmp|jpg|gif|png|)";
            foreach (Match m in Regex.Matches(ImgString.ToLower(), Reg))
            {
                MatchVale += (m.Value).ToLower().Trim().Replace("src=", "");
            }
            if (MatchVale.IndexOf(".net") != -1 || MatchVale.IndexOf(".com") != -1 || MatchVale.IndexOf(".org") != -1 || MatchVale.IndexOf(".cn") != -1 || MatchVale.IndexOf(".cc") != -1 || MatchVale.IndexOf(".info") != -1 || MatchVale.IndexOf(".biz") != -1 || MatchVale.IndexOf(".tv") != -1)
                return (MatchVale);
            else
                return (imgHttp + MatchVale);
        }
        #endregion

        #region 6、抓取远程页面内容
        /// <summary>
        /// 6.1以GET方式抓取远程页面内容
        /// </summary>
        public static string Get_Http(string tUrl)
        {
            string strResult;
            try
            {
                HttpWebRequest hwr = (HttpWebRequest)HttpWebRequest.Create(tUrl);
                hwr.Timeout = 19600;
                HttpWebResponse hwrs = (HttpWebResponse)hwr.GetResponse();
                Stream myStream = hwrs.GetResponseStream();
                StreamReader sr = new StreamReader(myStream, Encoding.Default);
                StringBuilder sb = new StringBuilder();
                while (-1 != sr.Peek())
                {
                    sb.Append(sr.ReadLine() + "\r\n");
                }
                strResult = sb.ToString();
                hwrs.Close();
            }
            catch (Exception ee)
            {
                strResult = ee.Message;
            }
            return strResult;
        }

        /// <summary>
        /// 6.2以POST方式抓取远程页面内容
        /// </summary>
        /// <param name="postData">参数列表</param>
        public static string Post_Http(string url, string postData, string encodeType)
        {
            string strResult = null;
            try
            {
                Encoding encoding = Encoding.GetEncoding(encodeType);
                byte[] POST = encoding.GetBytes(postData);
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
                myRequest.Method = "POST";
                myRequest.ContentType = "application/x-www-form-urlencoded";
                myRequest.ContentLength = POST.Length;
                Stream newStream = myRequest.GetRequestStream();
                newStream.Write(POST, 0, POST.Length); //设置POST
                newStream.Close();
                HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
                StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.Default);
                strResult = reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                strResult = ex.Message;
            }
            return strResult;
        }
        #endregion

        #region 7、压缩HTML输出
        /// <summary>
        /// 7.1压缩HTML输出
        /// </summary>
        public static string ZipHtml(string Html)
        {
            Html = Regex.Replace(Html, @">\s+?<", "><");//去除HTML中的空白字符
            Html = Regex.Replace(Html, @"\r\n\s*", "");
            Html = Regex.Replace(Html, @"<body([\s|\S]*?)>([\s|\S]*?)</body>", @"<body$1>$2</body>", RegexOptions.IgnoreCase);
            return Html;
        }
        #endregion

        #region 8、过滤HTML标签
        #region 8.1过滤指定HTML标签

        /// <summary>
        /// 8.1过滤指定HTML标签
        /// </summary>
        /// <param name="s_TextStr">要过滤的字符</param>
        /// <param name="html_Str">a img p div</param>
        public static string DelHtml(string s_TextStr, string html_Str)
        {
            string rStr = "";
            if (!string.IsNullOrEmpty(s_TextStr))
            {
                rStr = Regex.Replace(s_TextStr, "<" + html_Str + "[^>]*>", "", RegexOptions.IgnoreCase);
                rStr = Regex.Replace(rStr, "</" + html_Str + ">", "", RegexOptions.IgnoreCase);
            }
            return rStr;
        }
        #endregion
        #region 8.2过滤HTML中的不安全标签

        /// <summary>
        /// 8.2过滤HTML中的不安全标签
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string RemoveUnsafeHtml(string content)
        {
            content = Regex.Replace(content, @"(\<|\s+)o([a-z]+\s?=)", "$1$2", RegexOptions.IgnoreCase);
            content = Regex.Replace(content, @"(script|frame|form|meta|behavior|style)([\s|:|>])+", "$1.$2", RegexOptions.IgnoreCase);
            return content;
        }
        #endregion
        #endregion

        #region 转换HTML操作

        #region HTML转行成TEXT
        /// <summary>
        /// HTML转行成TEXT HtmlToTxt(string strHtml)
        /// </summary>
        /// <param name="strHtml"></param>
        /// <returns></returns>
        public static string HtmlToTxt(string strHtml)
        {
            string[] aryReg ={
            @"<script[^>]*?>.*?</script>",
            @"<(\/\s*)?!?((\w+:)?\w+)(\w+(\s*=?\s*(([""'])(\\[""'tbnr]|[^\7])*?\7|\w+)|.{0})|\s)*?(\/\s*)?>",
            @"([\r\n])[\s]+",
            @"&(quot|#34);",
            @"&(amp|#38);",
            @"&(lt|#60);",
            @"&(gt|#62);", 
            @"&(nbsp|#160);", 
            @"&(iexcl|#161);",
            @"&(cent|#162);",
            @"&(pound|#163);",
            @"&(copy|#169);",
            @"&#(\d+);",
            @"-->",
            @"<!--.*\n"
            };

            string newReg = aryReg[0];
            string strOutput = strHtml;
            for (int i = 0; i < aryReg.Length; i++)
            {
                Regex regex = new Regex(aryReg[i], RegexOptions.IgnoreCase);
                strOutput = regex.Replace(strOutput, string.Empty);
            }

            strOutput.Replace("<", "");
            strOutput.Replace(">", "");
            strOutput.Replace("\r\n", "");


            return strOutput;
        }
        #endregion

        #region 字符串转换为 Html
        /// <summary>
        /// 字符串转换为 HtmlStringToHtml(string str)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string StringToHtml(string str)
        {

            str = str.Replace("&", "&amp;");
            str = str.Replace(" ", "&nbsp;");
            str = str.Replace("'", "''");
            str = str.Replace("\"", "&quot;");
            str = str.Replace("<", "&lt;");
            str = str.Replace(">", "&gt;");
            str = str.Replace("\n", "<br />");
            str = str.Replace("\r", "<br />");
            str = str.Replace("\r\n", "<br />"); 
            return str;

        }
        #endregion

        #region Html转换成字符串
        /// <summary>
        /// html转换成字符串
        /// </summary>
        /// <param name="strHtml"></param>
        /// <returns></returns>
        public static string HtmlToString(string strHtml)
        {

            strHtml = strHtml.Replace("<br>", "\r\n");
            strHtml = strHtml.Replace(@"<br />", "\r\n");
            strHtml = strHtml.Replace(@"<br/>", "\r\n");
            strHtml = strHtml.Replace("&gt;", ">");
            strHtml = strHtml.Replace("&lt;", "<");
            strHtml = strHtml.Replace("&nbsp;", " ");
            strHtml = strHtml.Replace("&quot;", "\"");
            strHtml = Regex.Replace(strHtml, @"<\/?[^>]+>", "", RegexOptions.IgnoreCase);

            return strHtml;

        }
        #endregion
        #endregion

        #region 获取URL编码
        /// <summary>
        /// 获取URL编码
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string GetEncoding(string url)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            StreamReader reader = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = 20000;
                request.AllowAutoRedirect = false;
                response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK && response.ContentLength < 1024 * 1024)
                {
                    if (response.ContentEncoding != null && response.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase))
                    {
                        reader = new StreamReader(new GZipStream(response.GetResponseStream(), CompressionMode.Decompress));
                    }
                    else
                    {
                        reader = new StreamReader(response.GetResponseStream(), Encoding.ASCII);
                    }

                    string html = reader.ReadToEnd();
                    Regex reg_charset = new Regex(@"charset\b\s*=\s*(?<charset>[^""]*)");
                    if (reg_charset.IsMatch(html))
                    {
                        return reg_charset.Match(html).Groups["charset"].Value;
                    }
                    else if (response.CharacterSet != string.Empty)
                    {
                        return response.CharacterSet;
                    }
                    else
                    {
                        return Encoding.Default.BodyName;
                    }
                }
            }
            catch
            {
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
                if (reader != null)
                    reader.Close();
                if (request != null)
                    request = null;
            }
            return Encoding.Default.BodyName;
        }
        #endregion

        #region 判断URL是否有效


        /// <summary>
        /// 判断URL是否有效
        /// </summary>
        /// <param name="url">待判断的URL，可以是网页以及图片链接等</param>
        /// <returns>200为正确，其余为大致网页错误代码</returns>
        public int GetUrlError(string url)
        {
            int num = 200;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(url));
                ServicePointManager.Expect100Continue = false;
                ((HttpWebResponse)request.GetResponse()).Close();
            }
            catch (WebException exception)
            {
                if (exception.Status != WebExceptionStatus.ProtocolError)
                {
                    return num;
                }
                if (exception.Message.IndexOf("500 ") > 0)
                {
                    return 500;
                }
                if (exception.Message.IndexOf("401 ") > 0)
                {
                    return 401;
                }
                if (exception.Message.IndexOf("404") > 0)
                {
                    num = 404;
                }
            }
            catch
            {
                num = 401;
            }
            return num;
        }
        #endregion

        #region 返回 HTML 字符串的编码解码结果
        /// <summary>
        /// 返回 HTML 字符串的编码结果
        /// </summary>
        /// <param name="inputData">字符串</param>
        /// <returns>编码结果</returns>
        public static string HtmlEncode(string inputData)
        {
            return HttpUtility.HtmlEncode(inputData);
        }

        /// <summary>
        /// 返回 HTML 字符串的解码结果
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>解码结果</returns>
        public static string HtmlDecode(string str)
        {
            return HttpUtility.HtmlDecode(str);
        }
        #endregion

        #region 加载文件块
        /// <summary>
        /// 加载文件块
        /// </summary>
        public static string File(string Path, System.Web.UI.Page p)
        {
            return @p.ResolveUrl(Path);
        }
        #endregion

        #region 加载CSS样式文件
        /// <summary>
        /// 加载CSS样式文件
        /// </summary>
        public static string CSS(string cssPath, System.Web.UI.Page p)
        {
            return @"<link href=""" + p.ResolveUrl(cssPath) + @""" rel=""stylesheet"" type=""text/css"" />" + "\r\n";
        }
        #endregion

        #region 加载JavaScript脚本文件
        /// <summary>
        /// 加载javascript脚本文件
        /// </summary>
        public static string JS(string jsPath, System.Web.UI.Page p)
        {
            return @"<script type=""text/javascript"" src=""" + p.ResolveUrl(jsPath) + @"""></script>" + "\r\n";
        }
        #endregion

        public CookieCollection GetCookieCollection(string cookieString)
        {
            CookieCollection ccc = new CookieCollection();
            //string cookieString = "SID=ARRGy4M1QVBtTU-ymi8bL6X8mVkctYbSbyDgdH8inu48rh_7FFxHE6MKYwqBFAJqlplUxq7hnBK5eqoh3E54jqk=;Domain=.google.com;Path=/,LSID=AaMBTixN1MqutGovVSOejyb8mVkctYbSbyDgdH8inu48rh_7FFxHE6MKYwqBFAJqlhCe_QqxLg00W5OZejb_UeQ=;Domain=www.google.com;Path=/accounts";
            Regex re = new Regex("([^;,]+)=([^;,]+);Domain=([^;,]+);Path=([^;,]+)", RegexOptions.IgnoreCase);
            foreach (Match m in re.Matches(cookieString))
            {
                //name,   value,   path,   domain   
                Cookie c = new Cookie(m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value, m.Groups[3].Value);
                ccc.Add(c);
            }
            return ccc;
        }

        #region 从HTML中获取文本,保留br,p,img
      
        /// <summary>
        /// 从HTML中获取文本,保留br,p,img
        /// </summary>
        /// <param name="HTML"></param>
        /// <returns></returns>
        public static string GetTextFromHTML(string HTML)
        {
            Regex regEx = new Regex(@"</?(?!br|/?p|img)[^>]*>", RegexOptions.IgnoreCase);

            return regEx.Replace(HTML, "");
        }
        #endregion

        #region 获取HTML页面内制定Key的Value内容

        /// <summary>
        /// 获取HTML页面内制定Key的Value内容
        /// </summary>
        /// <param name="html"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetHiddenKeyValue(string html, string key)
        {
            string result = "";
            string sRegex = string.Format("<input\\s*type=\"hidden\".*?name=\"{0}\".*?\\s*value=[\"|'](?<value>.*?)[\"|'^/]", key);
            Regex re = new Regex(sRegex, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
            Match mc = re.Match(html);
            if (mc.Success)
            {
                result = mc.Groups[1].Value;
            }
            return result;
        }
        #endregion


        /// <summary>
        /// 替换回车换行符为html换行符
        /// </summary>
        public static string StrFormat(string str)
        {
            string str2;

            if (str == null)
            {
                str2 = "";
            }
            else
            {
                str = str.Replace("\r\n", "<br />");
                str = str.Replace("\n", "<br />");
                str2 = str;
            }
            return str2;
        }
        /// <summary>
        /// 替换html字符
        /// </summary>
        public static string EncodeHtml(string strHtml)
        {
            if (strHtml != "")
            {
                strHtml = strHtml.Replace(",", "&def");
                strHtml = strHtml.Replace("'", "&dot");
                strHtml = strHtml.Replace(";", "&dec");
                return strHtml;
            }
            return "";
        }


        /// <summary>
        /// 为脚本替换特殊字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ReplaceStrToScript(string str)
        {
            str = str.Replace("\\", "\\\\");
            str = str.Replace("'", "\\'");
            str = str.Replace("\"", "\\\"");
            return str;
        }
    }
}