using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using NetCore.Web.AutoGenerateHtmlControl.Attributes;
using Newtonsoft.Json.Linq;

namespace NetCore.Web.AutoGenerateHtmlControl
{
    public static class FormGeneratorExtension
    {
        private static readonly ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> ControlAttributes =
            new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();

        private static readonly ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> FormAttributes =
            new ConcurrentDictionary<Type, IEnumerable<PropertyInfo>>();



        public static async Task<IHtmlContent> GenerateFormAsync<TModel>(this IHtmlHelper html, HttpContext context, string url, FormMethod method, TModel model, object htmlAttributes = default, Func<TModel, IHtmlContent> appendHtmlContent = default, bool? antiforgery = default, string globalCssClass = "form-control")
        {
            var type = typeof(TModel);
            var serviceProvider = context.RequestServices;
            var options = serviceProvider.GetRequiredService<AutoGenerateFormBuilder>();

            var properties = ControlAttributes.GetOrAdd(type, t =>
            {
                return t.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead);
            });

            var form = new TagBuilder("form");
            form.MergeAttribute("method", method.ToString());
            form.MergeAttribute("action", url);
            var attribute = htmlAttributes.GetAttributeFromObject();
            if (attribute != null)
                form.MergeAttributes(attribute, true);
            var hasUploader = false;
            var hasEditor = false;
            var uploaderScripts = new StringBuilder();
            var editorScripts = new StringBuilder();
            foreach (var prop in properties)
            {
                var controlAttrs = prop.GetCustomAttributes<FormControlsAttribute>().ToList();
                if (!controlAttrs.Any())
                    continue;
                var name = prop.Name;
                var value = model == null ? null : prop.GetValue(model);
                var itemType = prop.PropertyType;
                var display = prop.GetCustomAttribute<DisplayNameAttribute>();
                var displayName = display == null ? name : display.DisplayName;

                var group = new TagBuilder("div");
                group.MergeAttribute("class", "form-group");
                if (prop.GetCustomAttribute<HideAttribute>() != null)
                    group.MergeAttribute("style", "display:none");

                var groupName = new TagBuilder("label");
                groupName.InnerHtml.AppendHtml(displayName);
                group.InnerHtml.AppendHtml(groupName);


                var controlContainer = new TagBuilder("div");
                //controlContainer.AddCssClass("input-group");

                controlContainer.MergeAttribute("id", $"{name.ToLower()}-input-group");

                foreach (var control in controlAttrs)
                {
                    switch (control.ControlType)
                    {
                        case HtmlControlType.Label:
                            controlContainer.InnerHtml.AppendHtml(html.ExLabel(name, value, control.GetAttributes(), globalCssClass));
                            break;

                        case HtmlControlType.Hidden:
                            controlContainer.InnerHtml.AppendHtml(html.ExHidden(name, value, control.GetAttributes()));
                            break;

                        case HtmlControlType.TextBox:
                            controlContainer.InnerHtml.AppendHtml(html.ExTextBox(name, value, control.GetAttributes(), globalCssClass));
                            break;

                        case HtmlControlType.Password:
                            controlContainer.InnerHtml.AppendHtml(html.ExPassword(name, value, control.GetAttributes(), globalCssClass));
                            break;

                        case HtmlControlType.TextArea:
                            controlContainer.InnerHtml.AppendHtml(html.ExTextArea(name, value, control.GetAttributes(), globalCssClass));
                            break;

                        case HtmlControlType.DropDownList:

                            var dropDownAttr = (DropDownListAttribute)control;
                            var dropDownDataSource = (IDataSource)serviceProvider.GetService(dropDownAttr.DataSource);
                            if (dropDownDataSource == null)
                            {
                                controlContainer.MergeAttribute("style", "color:red;");
                                controlContainer.InnerHtml.AppendHtml("Please bind the data source.");
                                break;
                            }
                            controlContainer.InnerHtml.AppendHtml(html.ExDropDownList(name,
                                await dropDownDataSource.GetAsync(value == null ? null : new[] { value }), dropDownAttr.OptionLabel,
                                control.GetAttributes(), globalCssClass));

                            break;

                        case HtmlControlType.ListBox:
                            var listBoxAttr = (ListBoxAttribute)control;
                            var listBoxDataSource = (IDataSource)serviceProvider.GetService(listBoxAttr.DataSource);
                            if (listBoxDataSource == null)
                            {
                                controlContainer.MergeAttribute("style", "color:red;");
                                controlContainer.InnerHtml.AppendHtml("Please bind the data source.");
                                break;
                            }

                            if (typeof(ICollection).IsAssignableFrom(itemType))
                            {
                                controlContainer.InnerHtml.AppendHtml(html.ExListBox(name, await listBoxDataSource.GetAsync(value as IEnumerable<object>), listBoxAttr.OptionLabel, control.GetAttributes(), globalCssClass));
                            }
                            else
                            {
                                controlContainer.MergeAttribute("style", "color:red;");
                                controlContainer.InnerHtml.AppendHtml($"ListBox does not support type {itemType}.");
                            }
                            break;

                        case HtmlControlType.RadioButton:
                            var radioButtonAttr = (RadioButtonAttribute)control;
                            var radioButtonDataSource = (IDataSource)serviceProvider.GetService(radioButtonAttr.DataSource);
                            if (radioButtonDataSource == null)
                            {
                                controlContainer.MergeAttribute("style", "color:red;");
                                controlContainer.InnerHtml.AppendHtml("Please bind the data source.");
                                break;
                            }
                            controlContainer.InnerHtml.AppendHtml(html.ExRadioButton(name,
                                await radioButtonDataSource.GetAsync(value == null ? null : new[] { value }), radioButtonAttr.GetAttributes(),
                                globalCssClass));
                            break;

                        case HtmlControlType.CheckBox:
                            var checkBoxAttr = (CheckBoxAttribute)control;
                            if (typeof(ICollection).IsAssignableFrom(itemType))
                            {
                                var checkBoxDataSource = (IDataSource)serviceProvider.GetService(checkBoxAttr.DataSource);
                                if (checkBoxDataSource == null)
                                {
                                    controlContainer.MergeAttribute("style", "color:red;");
                                    controlContainer.InnerHtml.AppendHtml("Please bind the data source.");
                                    break;
                                }
                                controlContainer.InnerHtml.AppendHtml(html.ExCheckBox(name,
                                    await checkBoxDataSource.GetAsync(value as IEnumerable<object>),
                                    checkBoxAttr.GetAttributes(), globalCssClass));
                            }
                            else if (typeof(bool).IsAssignableFrom(itemType))
                            {
                                controlContainer.InnerHtml.AppendHtml(html.ExSingleCheckBox(name, value != null && bool.Parse(value?.ToString()), checkBoxAttr.GetAttributes(), globalCssClass));
                            }
                            else
                            {

                                controlContainer.MergeAttribute("style", "color:red;");
                                controlContainer.InnerHtml.AppendHtml($"CheckBox does not support type {itemType}.");
                            }
                            break;

                        case HtmlControlType.Button:
                            var buttonAttr = (ButtonAttribute)control;
                            controlContainer.InnerHtml.AppendHtml(html.Button(buttonAttr.ButtonText, buttonAttr.GetAttributes(),
                                globalCssClass));
                            break;

                        case HtmlControlType.File:
                            controlContainer.InnerHtml.AppendHtml(html.ExFile(name, control.GetAttributes(), globalCssClass));
                            break;

                        case HtmlControlType.RichEditor:
                            var editorAttr = (RichEditorAttribute)control;
                            var editorPartialName = string.IsNullOrWhiteSpace(editorAttr.PartialName)
                                ? options.RichEditorOptions.PartialName
                                : editorAttr.PartialName;
                            controlContainer.InnerHtml.AppendHtml(await html.ExRichEditor(name, value?.ToString(), editorPartialName, editorAttr.GetAttributes()));
                            if (string.IsNullOrWhiteSpace(editorPartialName))
                            {
                                editorScripts.AppendFormat("ClassicEditor.create(document.querySelector(\"#{0}\")).catch(function(err){{alert(err)}}),", name);
                            }
                            if (!hasEditor)
                                hasEditor = true;
                            break;

                        case HtmlControlType.Uploader:
                            var uploaderAttr = (UploaderAttribute)control;
                            var uploaderPartialName = string.IsNullOrWhiteSpace(uploaderAttr.PartialName) ? options.UploaderOptions.PartialName : uploaderAttr.PartialName;
                            var global = options.UploaderOptions;
                            JToken uploaderOptions = null;
                            if (string.IsNullOrWhiteSpace(uploaderPartialName))
                            {
                                //合并参数

                                uploaderOptions = new JObject();
                                if (value != null)
                                    uploaderOptions["data"] = JToken.FromObject(value);

                                var fileBaseUrl = ChooseOptionString(global.FileBaseUrl, uploaderAttr.FileBaseUrl);
                                if (!string.IsNullOrWhiteSpace(fileBaseUrl))
                                {
                                    uploaderOptions["fileBaseUrl"] = fileBaseUrl;
                                }

                                var auto = ChooseOptionEnum(global.AutoUpload, uploaderAttr.AutoUpload);
                                if (auto)
                                {
                                    uploaderOptions["auto"] = auto;
                                }

                                var serverUrl = ChooseOptionString(global.ServerUrl, uploaderAttr.ServerUrl);
                                if (serverUrl != DefaultOptionValue.ServerUrl)
                                    uploaderOptions["serverUrl"] = serverUrl;


                                if (ChooseOptionEnum(global.Multiple, uploaderAttr.Multiple))
                                    uploaderOptions["multiple"] = true;

                                #region Chunked
                                if (ChooseOptionEnum(global.Chunked.Enable, uploaderAttr.Chunked))
                                {
                                    JToken chunk = new JObject
                                    {
                                        ["chunked"] = true
                                    };
                                    var chunkSize = ChooseOptionInt(global.Chunked.ChunkSize, uploaderAttr.ChunkSize);
                                    if (chunkSize != DefaultOptionValue.ChunkSize)
                                        chunk["chunkSize"] = chunkSize;

                                    var checkUrl = ChooseOptionString(global.Chunked.ChunkCheckServerUrl, uploaderAttr.ChunkCheckServerUrl);
                                    if (checkUrl != DefaultOptionValue.ChunkCheckServerUrl)
                                        chunk["chunkCheckServerUrl"] = checkUrl;

                                    var mergeUrl = ChooseOptionString(global.Chunked.ChunkMergeServerUrl, uploaderAttr.ChunkMergeServerUrl);
                                    if (mergeUrl != DefaultOptionValue.ChunkMergeServerUrl)
                                        chunk["chunkMergeServerUrl"] = mergeUrl;

                                    uploaderOptions["chunk"] = chunk;
                                }
                                #endregion

                                #region Accept
                                JToken accept = null;
                                //var acceptTitle = ChooseOptionString(global.Accept.Title, uploaderAttr.AcceptTitle);
                                //if (acceptTitle != DefaultOptionValue.AcceptTitle)
                                //{
                                //    accept ??= new JObject();
                                //    accept["title"] = acceptTitle;
                                //}

                                var acceptExtensions = ChooseOptionString(global.Accept.Extensions, uploaderAttr.AcceptExtensions);
                                if (acceptExtensions != DefaultOptionValue.AcceptExtensions)
                                {
                                    accept ??= new JObject();
                                    accept["extensions"] = acceptExtensions;
                                }

                                var acceptMineTypes = ChooseOptionString(global.Accept.MimeTypes, uploaderAttr.AcceptMimeTypes);
                                if (acceptMineTypes != DefaultOptionValue.AcceptMimeTypes)
                                {
                                    accept ??= new JObject();
                                    accept["mimeTypes"] = acceptMineTypes;
                                }

                                if (accept != null)
                                {
                                    uploaderOptions["accept"] = accept;
                                }
                                #endregion

                                #region Translation
                                JToken translation = null;
                                var uploadBtnText = ChooseOptionString(global.Translation.UploadBtnText, uploaderAttr.UploadBtnText);
                                if (uploadBtnText != DefaultOptionValue.UploadBtnText)
                                {
                                    translation ??= new JObject();
                                    translation["uploadBtnText"] = uploadBtnText;
                                }

                                var addBtnText = ChooseOptionString(global.Translation.AddFileBtnText, uploaderAttr.AddFileBtnText);
                                if (addBtnText != DefaultOptionValue.AddFileBtnText)
                                {
                                    translation ??= new JObject();
                                    translation["addFileBtnText"] = addBtnText;
                                }

                                var pauseBtnText = ChooseOptionString(global.Translation.PauseBtnText, uploaderAttr.PauseBtnText);
                                if (pauseBtnText != DefaultOptionValue.PauseBtnText)
                                {
                                    translation ??= new JObject();
                                    translation["pauseBtnText"] = pauseBtnText;
                                }

                                var continueBtnText = ChooseOptionString(global.Translation.ContinueBtnText, uploaderAttr.ContinueBtnText);
                                if (continueBtnText != DefaultOptionValue.ContinueBtnText)
                                {
                                    translation ??= new JObject();
                                    translation["continueBtnText"] = continueBtnText;
                                }

                                var exceedFileNumLimitAlert = ChooseOptionString(global.Translation.ExceedFileNumLimitAlert, uploaderAttr.ExceedFileNumLimitAlert);
                                if (exceedFileNumLimitAlert != DefaultOptionValue.ExceedFileNumLimitAlert)
                                {
                                    translation["ExceedFileNumLimitAlert"] = exceedFileNumLimitAlert;
                                }

                                var exceedFileSizeLimitAlert = ChooseOptionString(global.Translation.ExceedFileSizeLimitAlert, uploaderAttr.ExceedFileSizeLimitAlert);
                                if (exceedFileSizeLimitAlert != DefaultOptionValue.ExceedFileSizeLimitAlert)
                                {
                                    translation["ExceedFileSizeLimitAlert"] = exceedFileSizeLimitAlert;
                                }

                                if (translation != null)
                                {
                                    uploaderOptions["translation"] = translation;
                                }

                                #endregion

                                #region Compress

                                var enableCompress = ChooseOptionEnum(global.Compress.Enable, uploaderAttr.EnableCompress);
                                if (enableCompress)
                                {
                                    JToken compress = new JObject();
                                    var width = ChooseOptionInt(global.Compress.Width, uploaderAttr.CompressWidth);
                                    if (width != DefaultOptionValue.CompressWidth)
                                    {
                                        compress["width"] = width;
                                    }

                                    var height = ChooseOptionInt(global.Compress.Height, uploaderAttr.CompressHeight);
                                    if (height != DefaultOptionValue.CompressHeight)
                                    {
                                        compress["height"] = height;
                                    }

                                    var quality = ChooseOptionInt(global.Compress.Quality, uploaderAttr.CompressQuality);
                                    if (quality != DefaultOptionValue.CompressQuality)
                                    {
                                        compress["quality"] = quality;
                                    }

                                    var crop = ChooseOptionEnum(global.Compress.Crop, uploaderAttr.CompressCrop);
                                    if (crop)
                                    {
                                        compress["crop"] = crop;
                                    }

                                    var preserveHeaders = ChooseOptionEnum(global.Compress.PreserveHeaders, uploaderAttr.CompressPreserveHeaders);
                                    if (!preserveHeaders)
                                    {
                                        compress["preserveHeaders"] = preserveHeaders;
                                    }

                                    var noCompressIfLarger = ChooseOptionEnum(global.Compress.NoCompressIfLarger, uploaderAttr.CompressNoCompressIfLarger);
                                    if (noCompressIfLarger)
                                    {
                                        compress["noCompressIfLarger"] = noCompressIfLarger;
                                    }

                                    var compressSize = ChooseOptionInt(global.Compress.CompressSize, uploaderAttr.CompressSize);
                                    if (compressSize != 0)
                                    {
                                        compress["compressSize"] = compressSize;
                                    }

                                    uploaderOptions["compress"] = compress;
                                }

                                #endregion

                                #region FormData
                                if (uploaderAttr.FormData != null || global.FormData != null)
                                {
                                    uploaderOptions["formData"] = JObject.FromObject(uploaderAttr.FormData ?? global.FormData);
                                }
                                #endregion

                                #region FileNumLimit
                                var fileNumLimit = ChooseOptionInt(global.FileNumLimit, uploaderAttr.FileNumLimit);
                                if (fileNumLimit != DefaultOptionValue.FileNumLimit)
                                {
                                    uploaderOptions["fileNumLimit"] = fileNumLimit;
                                }
                                #endregion

                                #region FileSingleSizeLimit
                                var fileSingleSizeLimit = ChooseOptionInt(global.FileSingleSizeLimit, uploaderAttr.FileSingleSizeLimit);
                                if (fileSingleSizeLimit != DefaultOptionValue.FileSingleSizeLimit)
                                {
                                    uploaderOptions["fileSingleSizeLimit"] = fileSingleSizeLimit;
                                }
                                #endregion

                                #region Threads

                                var threads = ChooseOptionInt(global.Threads, uploaderAttr.Threads);
                                if (threads != DefaultOptionValue.Threads)
                                {
                                    uploaderOptions["threads"] = threads;
                                }

                                #endregion

                                #region Thumb

                                JToken thumb = null;
                                var thumbWidth = ChooseOptionInt(global.Thumb.Width, uploaderAttr.ThumbWidth);
                                if (thumbWidth != DefaultOptionValue.ThumbWidth)
                                {
                                    thumb ??= new JObject();
                                    thumb["width"] = thumbWidth;
                                }

                                var thumbHeight = ChooseOptionInt(global.Thumb.Height, uploaderAttr.ThumbHeight);
                                if (thumbHeight != DefaultOptionValue.ThumbHeight)
                                {
                                    thumb ??= new JObject();
                                    thumb["height"] = thumbHeight;
                                }

                                var thumbQuality = ChooseOptionInt(global.Thumb.Quality, uploaderAttr.ThumbQuality);
                                if (thumbQuality != DefaultOptionValue.ThumbQuality)
                                {
                                    thumb ??= new JObject();
                                    thumb["quality"] = thumbQuality;
                                }

                                var allowMagnify = ChooseOptionEnum(global.Thumb.AllowMagnify, uploaderAttr.ThumbAllowMagnify);
                                if (allowMagnify)
                                {
                                    thumb ??= new JObject();
                                    thumb["allowMagnify"] = allowMagnify;
                                }

                                var thumbCrop = ChooseOptionEnum(global.Thumb.Crop, uploaderAttr.ThumbCrop);
                                if (!thumbCrop)
                                {
                                    thumb ??= new JObject();
                                    thumb["crop"] = thumbCrop;
                                }

                                if (thumb != null)
                                {
                                    uploaderOptions["thumb"] = thumb;
                                }

                                #endregion
                            }

                            var tips = ChooseOptionString(global.Translation.Tips, uploaderAttr.Tips);


                            controlContainer.InnerHtml.AppendHtml(await html.Uploader(name, value?.ToString(), uploaderPartialName, uploaderAttr.GetAttributes(), new Dictionary<string, string> { { "Tips", tips }, { "UploadBtnText", ChooseOptionString(global.Translation.UploadBtnText, uploaderAttr.UploadBtnText) }, { "auto", uploaderOptions["auto"]?.ToObject<string>() } }));

                            uploaderScripts.AppendFormat("uploader_{0}=$(\"#{0}-container\").InitUploader({1}),", name, uploaderOptions.ToString(Newtonsoft.Json.Formatting.None));

                            if (!hasUploader)
                                hasUploader = true;
                            break;
                    }
                }

                group.InnerHtml.AppendHtml(controlContainer);
                group.InnerHtml.AppendHtml(html.ValidationMessage(name));
                form.InnerHtml.AppendHtml(group);
            }
            if (appendHtmlContent != null)
                form.InnerHtml.AppendHtml(appendHtmlContent(model));
            if (antiforgery.HasValue && antiforgery.Value)
                form.InnerHtml.AppendHtml(html.AntiForgeryToken());
            if (hasUploader || hasEditor)
            {
                var script = new TagBuilder("script");
                var scriptString = "document.addEventListener(\"DOMContentLoaded\",function(){if(" + hasUploader.ToString().ToLower() + "){var e=document.querySelectorAll(\".uploader-container\");if(!window.jQuery)return void e.forEach(function(e,t){e.innerHTML=\'<label style=\"color:red;\">Please install jQuery first.</label>\'});if(!window.WebUploader)return void e.forEach(function(e,t){e.innerHTML=\'<label style=\"color:red;\">Please install WebUploader first.</label>\'});var t=document.createElement(\"link\");t.setAttribute(\"rel\",\"stylesheet\"),t.setAttribute(\"href\",\"/auto-generate-html-control/resources/css/uploader.min.css\"),document.querySelector(\"header\").appendChild(t);var r=document.createElement(\"script\");r.setAttribute(\"src\",\"/auto-generate-html-control/resources/js/uploader.min.js\"),r.addEventListener(\"load\",function(){" + uploaderScripts.ToString().TrimEnd(',') + "}),document.querySelector(\"body\").appendChild(r)}" + hasEditor.ToString().ToLower() + "&&(window.ClassicEditor?(" + (hasEditor ? editorScripts.ToString().TrimEnd(',') : "a=0") + "):document.querySelectorAll(\".editor-container\").forEach(function(e,t){e.innerHTML=\'<label style=\"color:red;\">Please install CKEditor5 first.</label>\'}))});";
                script.InnerHtml.AppendHtml(scriptString);
                form.InnerHtml.AppendHtml(script);
            }
            return form;
        }

        private static bool ChooseOptionEnum(bool globalValue, UploaderOptionEnum attributeValue)
        {
            switch (attributeValue)
            {
                case UploaderOptionEnum.Inherit:
                    return globalValue;
                case UploaderOptionEnum.True:
                    return true;
                default:
                    return false;
            }
        }


        private static Dictionary<string, object> GetAttributeFromObject(this object obj)
        {
            if (obj == null)
                return null;
            var type = obj.GetType();
            var props = FormAttributes.GetOrAdd(type, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));
            return props.ToDictionary(k => k.Name, v => v.GetValue(obj));
        }


        private static int ChooseOptionInt(int globalValue, int attributeValue)
        {
            return attributeValue == -1 ? globalValue : attributeValue;
        }

        private static string ChooseOptionString(string globalValue, string attributeValue)
        {
            return string.IsNullOrWhiteSpace(attributeValue) ? globalValue : attributeValue;
        }
    }
}
