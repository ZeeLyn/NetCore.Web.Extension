using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NetCore.Web.AutoGenerateHtmlControl.Attributes;
using Newtonsoft.Json;

namespace WebApplication
{
    public class News
    {
        [Hidden] [Hide] public int Id { get; set; }


        [DisplayName("类型")]
        [DataListColumn(ValueMap = "{'1':'图文','2':'视频'}")]
        public int Type { get; set; } = 1;


        [DisplayName("标题")]
        [TextBox]
        [DataListColumn]
        [Required(ErrorMessage = "请输入标题"), MaxLength(100, ErrorMessage = "标题不能超过100个字符")]
        public string Title { get; set; }

        [DisplayName("副标题")]
        [TextBox]
        [MaxLength(100, ErrorMessage = "标题不能超过100个字符")]
        public string SubTitle { get; set; }


        [DisplayName("封面（支持jpg，png；图片比例为16:9（750x422)，最大不能超过1MB）")]
        [Uploader(AcceptExtensions = "jpg,png,jpeg,gif", FileSingleSizeLimit = 1024 * 1024 * 1,
            ChunkSize = 1024 * 1024 * 2, FileNumLimit = 1)]
        [Required(ErrorMessage = "请上传封面"), MaxLength(100, ErrorMessage = "封面不能超过100个字符")]
        [DataListColumn(Format = "<img src='{BaseUrl}{Cover}?x-oss-process=image/resize,m_fill,h_130,w_200' />")]
        public string Cover { get; set; }

        [DisplayName("是否在详情页展示封面")]
        [CheckBox(HtmlAttributes = "{'class':''}")]
        public bool ShowCover { get; set; }

        [DisplayName("标签")] public List<string> LabelArray { get; set; }


        public string Labels { get; set; }


        [DisplayName("视频(支持mp4,最大不能超过1GB)")]
        [Uploader(AcceptExtensions = "mp4", FileSingleSizeLimit = 1024 * 1024 * 1024, ChunkSize = 1024 * 1024 * 2)]
        [MaxLength(100, ErrorMessage = "URL不能超过100个字符")]
        public string Video { get; set; }


        [DisplayName("内容")]
        [RichEditor(HtmlAttributes = "{style:'height:400px'}")]
        public string Content { get; set; }

        [DisplayName("附件(支持pdf,doc,docx,xls,xlsx,ppt,pptx,zip;文档不超过10MB,压缩包最大不能超过1GB)")]
        [Uploader(AcceptExtensions = "pdf,doc,docx,xls,xlsx,ppt,pptx,zip", FileSingleSizeLimit = 1024 * 1024 * 1024,
            ChunkSize = 1024 * 1024 * 2)]
        [MaxLength(100, ErrorMessage = "附件URL不能超过100个字符")]
        public string Attachment { get; set; }


        [DisplayName("高清美图(支持jpg,png;单张图片不要超过5MB,文件名将作为图片的标题)")]
        [Uploader(PartialName = "CustomUploader")]
        public List<string> MorePics { get; set; }

        public List<string> MorePicsTitle { get; set; }

        public string MorePic { get; set; }


        [DisplayName("精彩视频(支持mp4,单个视频最大不能超过1GB,文件名将作为视频的标题)")]
        [Uploader(PartialName = "CustomUploader")]
        public List<string> MoreVideos { get; set; }

        public List<string> MoreVideosTitle { get; set; }

        public string MoreVideo { get; set; }

        public List<NewsMoreInfo> Pics { get; set; } = new List<NewsMoreInfo>();

        public List<NewsMoreInfo> Videos { get; set; } = new List<NewsMoreInfo>();


        [DisplayName("立即发布")]
        [CheckBox(HtmlAttributes = "{'class':''}")]
        public bool Published { get; set; }

        [DisplayName("发布")]
        [DataListColumn(ValueMap = "{'True':'已发布','False':'未发布'}")]
        public bool Status => Published;


        [DisplayName("定时发布")]
        [TextBox]
        [DataListColumn]
        //[DateTimeConverter("yyyy-MM-dd HH:mm")]
        public DateTime? TimedPublish { get; set; }

        [DisplayName("打开次数")] [DataListColumn] public int Pv { get; set; }

        [DisplayName("下载次数")] [DataListColumn] public int DownLoads { get; set; }


        [DisplayName("分享次数")] [DataListColumn] public int Shares { get; set; }


        [DisplayName("创建时间")]
        [DataListColumn]
        [DateTimeConverter("yyyy-MM-dd HH:mm")]
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public string BaseUrl { get; set; }
    }

    public class NewsMoreInfo
    {
        public string Url { get; set; }

        public string Title { get; set; }
    }

    public class NewsListViewModel
    {
        public int Id { get; set; }
        public int Type { get; set; }

        public string Title { get; set; }

        public string Cover { get; set; }

        public string Date => CreatedOn.ToString("yyyy-MM-dd");

        [JsonIgnore] public DateTime CreatedOn { get; set; }
    }


    public class VideoDetail
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string SubTitle { get; set; }

        public string Cover { get; set; }

        public bool ShowCover { get; set; }

        [JsonIgnore] public string Labels { get; set; }

        public string Content { get; set; }

        public string Date => CreatedOn.ToString("yyyy-MM-dd");

        [JsonIgnore] public DateTime CreatedOn { get; set; }

        public List<NewsListViewModel> Recommend { get; set; }
    }

    public class NewsDetail : VideoDetail
    {
        [JsonIgnore] public string MorePic { get; set; }
        [JsonIgnore] public string MoreVideo { get; set; }

        public string Attachment { get; set; }

        public MoreFirstInfo MorePicInfo
        {
            get
            {
                if (string.IsNullOrWhiteSpace(MorePic))
                    return new MoreFirstInfo
                    {
                        Count = 0
                    };
                var list = JsonConvert.DeserializeObject<List<NewsMoreInfo>>(MorePic);
                return new MoreFirstInfo
                {
                    Url = list.FirstOrDefault()?.Url,
                    Title = list.FirstOrDefault()?.Title,
                    Count = list.Count
                };
            }
        }

        public MoreFirstInfo MoreVideoInfo
        {
            get
            {
                if (string.IsNullOrWhiteSpace(MoreVideo))
                    return new MoreFirstInfo
                    {
                        Count = 0
                    };
                var list = JsonConvert.DeserializeObject<List<NewsMoreInfo>>(MoreVideo);
                return new MoreFirstInfo
                {
                    Url = list.FirstOrDefault()?.Url,
                    Title = list.FirstOrDefault()?.Title,
                    Count = list.Count
                };
            }
        }
    }

    public class MoreFirstInfo : NewsMoreInfo
    {
        public int Count { get; set; }
    }
}