using System;

namespace NetCore.Web.AutoGenerateHtmlControl.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class UploaderAttribute : FormControlsAttribute
    {
        public UploaderAttribute()
        {
            ControlType = HtmlControlType.Uploader;
        }

        public string PartialName { get; set; }

        public string FileBaseUrl { get; set; }

        public UploaderOptionEnum AutoUpload { get; set; } = UploaderOptionEnum.Inherit;

        public string ServerUrl { get; set; }

        public UploaderOptionEnum Multiple { get; set; } = UploaderOptionEnum.Inherit;

        public UploaderOptionEnum Chunked { get; set; } = UploaderOptionEnum.Inherit;

        public int ChunkSize { get; set; } = -1;

        public string ChunkCheckServerUrl { get; set; }

        public string ChunkMergeServerUrl { get; set; }

        //public string AcceptTitle { get; set; }

        public string AcceptExtensions { get; set; }

        public string AcceptMimeTypes { get; set; }

        public string Tips { get; set; }

        public string UploadBtnText { get; set; }

        public string AddFileBtnText { get; set; }

        public string PauseBtnText { get; set; }

        public string ContinueBtnText { get; set; }

        public string ExceedFileNumLimitAlert { get; set; }

        public string ExceedFileSizeLimitAlert { get; set; }

        public UploaderOptionEnum EnableCompress { get; set; } = UploaderOptionEnum.Inherit;

        public int CompressWidth { get; set; } = -1;

        public int CompressHeight { get; set; } = -1;

        /// <summary>
        /// 图片质量，只有type为`image/jpeg`的时候才有效。
        /// </summary>
        public int CompressQuality { get; set; } = -1;
        /// <summary>
        /// 是否允许放大，如果想要生成小图的时候不失真，此选项应该设置为false.
        /// </summary>
        public UploaderOptionEnum CompressAllowMagnify { get; set; } = UploaderOptionEnum.Inherit;

        /// <summary>
        /// 是否允许裁剪。
        /// </summary>
        public UploaderOptionEnum CompressCrop { get; set; } = UploaderOptionEnum.Inherit;

        /// <summary>
        /// 是否保留头部meta信息。
        /// </summary>
        public UploaderOptionEnum CompressPreserveHeaders { get; set; } = UploaderOptionEnum.Inherit;

        /// <summary>
        /// 如果发现压缩后文件大小比原来还大，则使用原来图片
        /// 此属性可能会影响图片自动纠正功能
        /// </summary>
        public UploaderOptionEnum CompressNoCompressIfLarger { get; set; } = UploaderOptionEnum.Inherit;

        /// <summary>
        /// 单位字节，如果图片大小小于此值，不会采用压缩。
        /// </summary>
        public int CompressSize { get; set; } = -1;


        public string FormData { get; set; }

        /// <summary>
        /// 允许上传文件最大数量,默认1
        /// </summary>
        public int FileNumLimit { get; set; } = -1;

        /// <summary>
        /// 允许上传的单个文件大小上限，默认4MB
        /// </summary>
        public int FileSingleSizeLimit { get; set; } = -1;

        /// <summary>
        /// 上传线程数,默认1
        /// </summary>
        public int Threads { get; set; } = -1;


        public int ThumbWidth { get; set; } = -1;

        public int ThumbHeight { get; set; } = -1;

        public int ThumbQuality { get; set; } = -1;

        /// <summary>
        /// 是否允许放大，如果想要生成小图的时候不失真，此选项应该设置为false.
        /// </summary>
        public UploaderOptionEnum ThumbAllowMagnify { get; set; } = UploaderOptionEnum.Inherit;

        /// <summary>
        /// 是否允许裁剪。
        /// </summary>
        public UploaderOptionEnum ThumbCrop { get; set; } = UploaderOptionEnum.Inherit;
    }


}
