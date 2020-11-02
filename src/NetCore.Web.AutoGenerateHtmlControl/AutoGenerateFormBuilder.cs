namespace NetCore.Web.AutoGenerateHtmlControl
{
    public class AutoGenerateFormBuilder
    {
        public AutoGenerateFormEditorConfigure RichEditorOptions { get; } = new AutoGenerateFormEditorConfigure();

        public AutoGenerateFormUploaderConfigure UploaderOptions { get; } = new AutoGenerateFormUploaderConfigure();
    }

    public class AutoGenerateFormEditorConfigure
    {
        public string PartialName { get; set; }

        public object Options { get; set; } = new object();
    }

    public class AutoGenerateFormUploaderConfigure
    {
        public string PartialName { get; set; }

        public string FileBaseUrl { get; set; }

        /// <summary>
        /// 自动上传，不需要点击
        /// </summary>
        public bool AutoUpload { get; set; }

        public string ServerUrl { get; set; } = DefaultOptionValue.ServerUrl;

        public bool Multiple { get; set; }

        public ChunkedOptions Chunked { get; } = new ChunkedOptions();

        public AcceptOptions Accept { get; } = new AcceptOptions();

        public TranslationOptions Translation { get; } = new TranslationOptions();

        public CompressOptions Compress { get; } = new CompressOptions();

        public object FormData { get; set; }

        /// <summary>
        /// 允许上传文件最大数量,默认1
        /// </summary>
        public int FileNumLimit { get; set; } = DefaultOptionValue.FileNumLimit;

        /// <summary>
        /// 允许上传的单个文件大小上限，默认4MB
        /// </summary>
        public int FileSingleSizeLimit { get; set; } = DefaultOptionValue.FileSingleSizeLimit;

        /// <summary>
        /// 上传线程数,默认1
        /// </summary>
        public int Threads { get; set; } = DefaultOptionValue.Threads;

        /// <summary>
        /// 缩略图
        /// </summary>
        public ThumbOptions Thumb { get; } = new ThumbOptions();

        /// <summary>
        /// 上传的文件存储提供者，用于生成缩略图时直接加载缩略图而不下载整个文件
        /// </summary>
        public StoreProvider StoreProvider { get; set; } = StoreProvider.DISK;
    }

    /// <summary>
    /// 上传的文件存储提供者
    /// </summary>
    public enum StoreProvider
    {
        DISK,
        OSS,
        COS
    }

    public class ChunkedOptions
    {
        public bool Enable { get; set; }

        public int ChunkSize { get; set; } = DefaultOptionValue.ChunkSize;

        public string ChunkCheckServerUrl { get; set; } = DefaultOptionValue.ChunkCheckServerUrl;

        public string ChunkMergeServerUrl { get; set; } = DefaultOptionValue.ChunkMergeServerUrl;
    }

    public class ThumbOptions
    {
        public int Width { get; set; } = DefaultOptionValue.ThumbWidth;

        public int Height { get; set; } = DefaultOptionValue.ThumbHeight;


        public int Quality { get; set; } = DefaultOptionValue.ThumbQuality;
        /// <summary>
        /// 是否允许放大，如果想要生成小图的时候不失真，此选项应该设置为false.
        /// </summary>
        public bool AllowMagnify { get; set; }

        /// <summary>
        /// 是否允许裁剪。
        /// </summary>
        public bool Crop { get; set; } = true;

    }


    public class CompressOptions
    {
        public bool Enable { get; set; }

        public int Width { get; set; } = DefaultOptionValue.CompressWidth;

        /// <summary>
        /// 缩略图的高度，启用Crop时才生效，否则跟Width等比例缩放
        /// </summary>
        public int Height { get; set; } = DefaultOptionValue.CompressHeight;

        /// <summary>
        /// 图片质量，只有type为`image/jpeg`的时候才有效。
        /// </summary>
        public int Quality { get; set; } = DefaultOptionValue.CompressQuality;
        /// <summary>
        /// 是否允许放大，如果想要生成小图的时候不失真，此选项应该设置为false.
        /// </summary>
        public bool AllowMagnify { get; set; }

        /// <summary>
        /// 是否允许裁剪。
        /// </summary>
        public bool Crop { get; set; }

        /// <summary>
        /// 是否保留头部meta信息。
        /// </summary>
        public bool PreserveHeaders { get; set; } = true;

        /// <summary>
        /// 如果发现压缩后文件大小比原来还大，则使用原来图片
        /// 此属性可能会影响图片自动纠正功能
        /// </summary>
        public bool NoCompressIfLarger { get; set; }

        /// <summary>
        /// 单位字节，如果图片大小小于此值，不会采用压缩。
        /// </summary>
        public int CompressSize { get; set; }
    }

    public class TranslationOptions
    {

        public string Tips { get; set; } = DefaultOptionValue.Tips;

        public string UploadBtnText { get; set; } = DefaultOptionValue.UploadBtnText;

        public string AddFileBtnText { get; set; } = DefaultOptionValue.AddFileBtnText;

        public string PauseBtnText { get; set; } = DefaultOptionValue.PauseBtnText;

        public string ContinueBtnText { get; set; } = DefaultOptionValue.ContinueBtnText;

        public string ExceedFileNumLimitAlert { get; set; } = DefaultOptionValue.ExceedFileNumLimitAlert;

        public string ExceedFileSizeLimitAlert { get; set; } = DefaultOptionValue.ExceedFileSizeLimitAlert;
    }

    public class AcceptOptions
    {
        //public string Title { get; set; } = DefaultOptionValue.AcceptTitle;

        /// <summary>
        /// 允许上传的文件格式,默认：gif,jpg,jpeg,bmp,png
        /// </summary>
        public string Extensions { get; set; } = DefaultOptionValue.AcceptExtensions;

        /// <summary>
        /// 类型,默认：images/*
        /// </summary>
        public string MimeTypes { get; set; } = DefaultOptionValue.AcceptMimeTypes;
    }

    public enum UploaderOptionEnum
    {
        Inherit = -1,
        True = 1,
        False = 0
    }

    public class DefaultOptionValue
    {
        public const string ServerUrl = "/api/upload";

        public const string ChunkCheckServerUrl = "/api/upload?action=chunk";

        public const string ChunkMergeServerUrl = "/api/upload?action=merge";

        public const int FileNumLimit = 1;

        public const int FileSingleSizeLimit = 1024 * 1024 * 4;

        public const int Threads = 1;

        public const int ChunkSize = 1024 * 1024 * 2;

        public const int ThumbWidth = 100;

        public const int ThumbHeight = 100;

        public const int ThumbQuality = 60;

        public const int CompressWidth = 1280;

        public const int CompressHeight = 720;

        public const int CompressQuality = 70;

        public const string UploadBtnText = "开始上传";

        public const string AddFileBtnText = "添加文件";

        public const string PauseBtnText = "暂停上传";

        public const string ContinueBtnText = "继续上传";

        public const string AcceptTitle = "Files";

        public const string AcceptExtensions = "gif,jpg,jpeg,bmp,png";

        public const string AcceptMimeTypes = "image/*";

        public const string Tips = "或将文件拖拽 , 粘贴到这里";

        public const string ExceedFileNumLimitAlert = "超出允许上传文件个数,最多只允许上传{FileNumLimit}个。";

        public const string ExceedFileSizeLimitAlert = "超出允许上传的文件大小,单个文件大小不能超过{FileSizeLimit}";
    }
}
