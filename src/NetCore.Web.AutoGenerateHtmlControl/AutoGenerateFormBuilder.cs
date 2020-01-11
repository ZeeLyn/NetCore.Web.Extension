using System;
using System.Collections.Generic;
using System.Text;

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
    }

    public class AutoGenerateFormUploaderConfigure
    {
        public string PartialName { get; set; }

        public string ServerUrl { get; set; } = "/api/upload";

        public bool Multiple { get; set; }

        public ChunkedOptions Chunked { get; } = new ChunkedOptions();

        public AcceptOptions Accept { get; } = new AcceptOptions();

        public TranslationOptions Translation { get; } = new TranslationOptions();

        public CompressOptions Compress { get; } = new CompressOptions();

        public object FormData { get; set; }

        /// <summary>
        /// 允许上传文件最大数量,默认1
        /// </summary>
        public int FileNumLimit { get; set; } = 1;

        /// <summary>
        /// 允许上传的单个文件大小上限，默认4MB
        /// </summary>
        public int FileSingleSizeLimit { get; set; } = 1024 * 1024 * 4;

        /// <summary>
        /// 上传线程数,默认1
        /// </summary>
        public int Threads { get; set; } = 1;

        /// <summary>
        /// 缩略图
        /// </summary>
        public ThumbOptions Thumb { get; set; }
    }
    public class ChunkedOptions
    {
        public bool Enable { get; set; }

        public int ChunkSize { get; set; } = 1024 * 1024 * 2;

        public string ChunkCheckServerUrl { get; set; } = "/api/upload?action=chunk";

        public string ChunkMergeServerUrl { get; set; } = "/api/upload?action=merge";
    }

    public class ThumbOptions
    {
        public int Width { get; set; } = 100;

        public int Height { get; set; } = 100;

        /// <summary>
        /// 是否允许放大，如果想要生成小图的时候不失真，此选项应该设置为false.
        /// </summary>
        public bool AllowMagnify { get; set; }

        /// <summary>
        /// 是否允许裁剪。
        /// </summary>
        public bool Crop { get; set; }

    }


    public class CompressOptions
    {
        public bool Enable { get; set; } = true;

        public int Width { get; set; } = 1280;

        public int Height { get; set; } = 900;

        /// <summary>
        /// 图片质量，只有type为`image/jpeg`的时候才有效。
        /// </summary>
        public int Quality { get; set; } = 60;
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
        public string UploadBtnText { get; set; } = "开始上传";

        public string AddFileBtnText { get; set; } = "添加文件";

        public string PauseBtnText { get; set; } = "暂停上传";

        public string ContinueBtnText { get; set; } = "继续上传";
    }

    public class AcceptOptions
    {
        public string Title { get; set; } = "Files";

        public string Extensions { get; set; } = "gif,jpg,jpeg,bmp,png";

        public string MimeTypes { get; set; } = "Images/*";
    }

    public enum UploaderOptionEnum
    {
        Inherit = -1,
        True = 1,
        False = 0
    }
}
