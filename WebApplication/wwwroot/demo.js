if (window.WebUploader) {
    WebUploader.Uploader.register({ "before-send": "beforeSend", "before-send-file": "beforeSendFile", "after-send-file": "afterSendFile" }, {
        //发送分片之前检查分片完整性
        beforeSend: function (block) {
            var file = block.file;
            var options = file.options;
            if (!options.chunk.chunked)
                return false;
            if (!file.chunks)
                file.chunks = block.chunks;
            var deferred = WebUploader.Deferred();
            (new WebUploader.Uploader()).md5File(file, block.start, block.end).then(function (md5) {
                $.ajax({
                    type: "post",
                    url: options.chunk.chunkCheckServerUrl,
                    data: {
                        chunk_md5: md5,
                        md5: file.md5,
                        chunk: block.chunk
                    },
                    success: function (res) {
                        if (res.data === 1)
                            deferred.reject();
                        else
                            deferred.resolve();
                    },
                    error: function (err) {
                        console.error(err);
                        deferred.reject();
                    }
                });

            });
            return deferred.promise();
        },
        //发送文件之前生成md5
        beforeSendFile: function (file) {
            var options = file.options;
            if (!options.chunk.chunked)
                return false;
            var deferred = WebUploader.Deferred();
            (new WebUploader.Uploader()).md5File(file).then(function (md5) {
                file.md5 = md5;
                deferred.resolve();
            });
            return deferred.promise();
        },
        //分片上传完成合并分片
        afterSendFile: function (file, res) {
            var options = file.options;
            if (!options.chunk.chunked) {
                options.container.find("#" + file.id).append('<input type="hidden" name="' + options.container.data("form-name") + '" value="' + res.data[0] + '" />');
            }
            else {
                var deferred = WebUploader.Deferred();
                $.ajax({
                    type: "post",
                    url: options.chunk.chunkMergeServerUrl,
                    data: {
                        md5: file.md5,
                        chunks: file.chunks
                    },
                    success: function (res) {
                        options.container.find("#" + file.id).append('<input type="hidden" name="' + options.container.data("form-name") + '" value="' + res.data + '" />');
                        deferred.resolve();
                    },
                    error: function (err) {
                        console.error(err);
                        deferred.reject();
                    }
                });
                return deferred.promise();
            }
        }
    });
}
(function ($) {
    $.fn.InitUploader = function (options) {
        options = $.extend({
            data: null,
            container: this,
            base_url: "",
            serverUrl: "/api/upload",
            multiple: false,
            chunk: {
                chunked: false,
                chunkSize: 1024 * 1024 * 2,
                chunkCheckServerUrl: "/api/upload?action=chunk",
                chunkMergeServerUrl: "/api/upload?action=merge"
            },
            accept: {
                title: 'Images',
                extensions: 'gif,jpg,jpeg,bmp,png,pdf,xls,xlsx,doc,docx,ppt,pptx',
                mimeTypes: 'Image/*'
            },
            translation: {
                uploadBtnText: "开始上传",
                addFileBtnText: "添加文件",
                pauseBtnText: "暂停上传",
                continueBtnText: "继续上传"
            },
            compress: false,
            formData: null,
            fileNumLimit: 1,
            fileSingleSizeLimit: 1024 * 1024 * 4,
            threads: 1,
            thumb: {
                width: 100, height: 100, quality: 60, crop: true, allowMagnify: false
            }
        }, options);


        var o;
        this.each(function () {
            var $ = jQuery,    // just in case. Make sure it's not an other libaray.
                $wrap = options.container,
                // 图片容器
                $queue = $wrap.find('.filelist'),

                // 状态栏，包括进度和控制按钮
                $statusBar = $wrap.find('.statusBar'),

                // 文件总体选择信息。
                $info = $statusBar.find('.info'),

                // 上传按钮
                $upload = $wrap.find('.uploadBtn'),

                // 没选择文件之前的内容。
                $placeHolder = $wrap.find('.placeholder'),

                // 总体进度条
                $progress = $statusBar.find('.progress').hide(),

                // 添加的文件数量
                fileCount = 0,

                // 添加的文件总大小
                fileSize = 0,

                // 优化retina, 在retina下这个值是2
                //ratio = window.devicePixelRatio || 1,

                // 缩略图大小
                thumbnailWidth = options.thumb.width, //* ratio,
                thumbnailHeight = options.thumb.height,// * ratio,

                // 可能有pedding, ready, uploading, confirm, done.
                state = 'pedding',

                // 所有文件的进度信息，key为file id
                percentages = {},

                supportTransition = (function () {
                    var s = document.createElement('p').style,
                        r = 'transition' in s ||
                            'WebkitTransition' in s ||
                            'MozTransition' in s ||
                            'msTransition' in s ||
                            'OTransition' in s;
                    s = null;
                    return r;
                })(),
                footerAddFile = $wrap.find('.footer-add-btn'),
                uploader;

            if (!WebUploader.Uploader.support()) {
                alert('WebUploader does not support the browser you are using.');
                throw new Error('WebUploader does not support the browser you are using.');
            }

            // 实例化
            o = uploader = WebUploader.create({
                pick: {
                    id: $wrap.find(".filePicker"),
                    label: options.translation.addFileBtnText,
                    multiple: options.multiple
                },
                dnd: $wrap.find('.queueList'),
                paste: options.container,

                accept: options.accept,

                //// swf文件路径
                //swf: options.base_url + '/js/Uploader.swf',
                formData: options.formData,
                disableGlobalDnd: true,
                thumb: options.thumb,
                compress: options.compress,
                chunked: options.chunk.chunked,
                chunkSize: options.chunk.chunkSize,
                server: options.serverUrl,
                fileNumLimit: options.fileNumLimit,
                //fileSizeLimit: options.fileSizeLimit,
                threads: options.threads,
                fileSingleSizeLimit: options.fileSingleSizeLimit
            });


            // 添加“添加文件”的按钮，
            uploader.addButton({
                id: footerAddFile,
                label: options.translation.addFileBtnText
            }).then(function () {
                footerAddFile.find("div:eq(1)").css({ "width": "100%", "height": "100%" });
            });

            $(options.data).each(function (_, item) {
                var sp = item.split('/');
                url2File(item, sp[sp.length - 1], "image/jpeg").then(function (f) {
                    f.uploaded = true;
                    f.remote_url = item;
                    uploader.addFiles(f);
                });
            });

            // 当有文件添加进来时执行，负责view的创建
            function addFile(file) {
                var $li = $('<li id="' + file.id + '">' +
                    '<p class="imgWrap"></p>' +
                    '<p class="progress"><span></span></p>' +
                    '</li>'),

                    $btns = $('<div class="file-panel">' +
                        '<span class="rotateLeft"></span>' +
                        '<span class="rotateRight"></span>' +
                        '<span class="cancel"></span></div>').appendTo($li),
                    source = file.source.source,

                    $prgress = $li.find('p.progress span'),
                    $wrap = $li.find('p.imgWrap'),
                    $info = $('<p class="error"></p>'),

                    showError = function (code) {
                        switch (code) {
                            case 'exceed_size':
                                text = '文件超出大小';
                                break;

                            case 'interrupt':
                                text = '上传暂停';
                                break;

                            default:
                                text = '上传失败，请重试';
                                break;
                        }

                        $info.text(text).appendTo($li);
                    };
                if (source.uploaded && source.remote_url) {
                    $li.append('<input type="hidden" name="' + options.container.data("form-name") + '" value="' + source.remote_url + '" />');
                }
                if (file.getStatus() === 'invalid') {
                    showError(file.statusText);
                } else {
                    // @todo lazyload
                    $wrap.text('Loading...');
                    uploader.makeThumb(file, function (error, src) {
                        if (error) {
                            //$wrap.text('不能预览');
                            $wrap.empty().append($('<img class="thumb" src="/auto_generate_html_control/resources/icons/' + file.ext + '.png">'));
                            return;
                        }
                        var img = $('<img src="' + src + '">');
                        $wrap.empty().append(img);
                    }, thumbnailWidth, thumbnailHeight);

                    percentages[file.id] = [file.size, 0];
                    file.rotation = 0;
                }

                file.on('statuschange', function (cur, prev) {

                    if (prev === 'progress') {
                        $prgress.hide().width(0);
                    } else if (prev === 'queued') {
                        //$li.off('mouseenter mouseleave');
                        //$btns.remove();
                    }

                    // 成功
                    if (cur === 'error' || cur === 'invalid') {
                        //console.log(file.statusText);
                        showError(file.statusText);
                        percentages[file.id][1] = 1;
                    } else if (cur === 'interrupt') {
                        showError('interrupt');
                    } else if (cur === 'queued') {
                        percentages[file.id][1] = 0;
                    } else if (cur === 'progress') {
                        $info.remove();
                        $prgress.css('display', 'block');
                    } else if (cur === 'complete') {
                        $li.append('<span class="success"></span>');
                    }

                    $li.removeClass('state-' + prev).addClass('state-' + cur);
                });

                $li.on('mouseenter', function () {
                    $btns.stop().animate({ height: 30 });
                });

                $li.on('mouseleave', function () {
                    $btns.stop().animate({ height: 0 });
                });

                $btns.on('click', 'span', function () {
                    var index = $(this).index(),
                        deg;

                    switch (index) {
                        case 0:
                            file.rotation -= 90;
                            break;
                        case 1:
                            file.rotation += 90;
                            break;

                        case 2:
                            uploader.removeFile(file);
                            return;
                    }

                    if (supportTransition) {
                        deg = 'rotate(' + file.rotation + 'deg)';
                        $wrap.css({
                            '-webkit-transform': deg,
                            '-mos-transform': deg,
                            '-o-transform': deg,
                            'transform': deg
                        });
                    } else {
                        $wrap.css('filter', 'progid:DXImageTransform.Microsoft.BasicImage(rotation=' + (~~((file.rotation / 90) % 4 + 4) % 4) + ')');
                    }
                });

                $li.appendTo($queue);

                if (file.source.source.uploaded) {
                    file.setStatus('complete');
                }
            }

            function removeFile(file) {
                var $li = $wrap.find('#' + file.id);
                delete percentages[file.id];
                updateTotalProgress();
                $li.off().find('.file-panel').off().end().remove();
            }

            function updateTotalProgress() {
                var loaded = 0,
                    total = 0,
                    spans = $progress.children();

                $.each(percentages, function (k, v) {
                    total += v[0];
                    loaded += v[0] * v[1];
                });

                var percent = total ? loaded / total : 0;

                spans.eq(0).text(Math.round(percent * 100) + '%');
                spans.eq(1).css('width', Math.round(percent * 100) + '%');
                updateStatus();
            }

            function updateStatus() {
                var text = '', stats;
                if (state === 'ready') {
                    text = '选中' + fileCount + '个文件，共' +
                        WebUploader.formatSize(fileSize) + '。';
                } else if (state === 'confirm') {
                    stats = uploader.getStats();
                    if (stats.uploadFailNum) {
                        text = '成功 <span class="text-warning">' +
                            stats.successNum +
                            '</span> 个，失败 <span class="text-success">' +
                            stats.uploadFailNum +
                            '</span> 个，点击<a class="retry font-weight-bold" href="javascript:;"> 重新上传 </a>失败文件';
                    }

                } else {
                    stats = uploader.getStats();
                    text = '共' + fileCount + '个（' +
                        WebUploader.formatSize(fileSize) +
                        '），已上传' + (stats.successNum - (options.data ? options.data.length : 0)) + '个';

                    if (stats.uploadFailNum) {
                        text += '，失败' + stats.uploadFailNum + '个';
                    }
                }

                $info.html(text);
            }

            function setState(val) {
                var stats;

                if (val === state) {
                    return;
                }

                $upload.removeClass('state-' + state);
                $upload.addClass('state-' + val);

                state = val;

                switch (state) {
                    case 'pedding':
                        $placeHolder.removeClass('element-invisible');
                        $queue.parent().removeClass('filled');
                        $queue.hide();
                        $statusBar.addClass('element-invisible');
                        uploader.refresh();
                        break;

                    case 'ready':
                        $placeHolder.addClass('element-invisible');
                        footerAddFile.removeClass('element-invisible');
                        $queue.parent().addClass('filled');
                        $queue.show();
                        $statusBar.removeClass('element-invisible');
                        uploader.refresh();
                        break;

                    case 'uploading':
                        footerAddFile.addClass('element-invisible');
                        $progress.show();
                        $upload.text(options.translation.pauseBtnText);
                        break;

                    case 'paused':
                        $progress.show();
                        $upload.text(options.translation.continueBtnText);
                        break;

                    case 'confirm':
                        $progress.hide();
                        $upload.text(options.translation.uploadBtnText).addClass('disabled');

                        stats = uploader.getStats();
                        if (stats.successNum && !stats.uploadFailNum) {
                            setState('finish');
                            return;
                        }
                        break;
                    case 'finish':
                        stats = uploader.getStats();
                        if (stats.successNum) {
                            //alert('上传成功');
                        } else {
                            // 没有成功的图片，重设
                            state = 'done';
                            location.reload();
                        }
                        break;
                }

                updateStatus();
            }

            uploader.onUploadProgress = function (file, percentage) {
                var $li = $('#' + file.id),
                    $percent = $li.find('.progress span');
                $percent.css('width', percentage * 100 + '%');
                percentages[file.id][1] = percentage;
                updateTotalProgress();
            };
            uploader.onBeforeFileQueued = function (file) {
                if (state === 'finish')
                    return false;
                //var len = uploader.getFiles().length;
                //return len < options.fileNumLimit;
            };

            uploader.on("uploadBeforeSend", function (block, data, headers) {
                var file = block.file;
                headers["file-md5"] = file.md5;
            });


            uploader.onFileQueued = function (file) {
                console.log(file);
                file.options = options;
                if (file.source.source.uploaded) {
                    $placeHolder.addClass('element-invisible');
                    $statusBar.show();
                }
                else {
                    fileCount++;
                    fileSize += file.size;
                }

                if (fileCount === 1) {
                    $placeHolder.addClass('element-invisible');
                    $statusBar.show();
                    $upload.removeClass("disabled");
                }
                addFile(file);
                setState('ready');
                updateTotalProgress();
            };

            uploader.onFileDequeued = function (file) {
                if (!file.source.source.uploaded) {
                    fileCount--;
                    fileSize -= file.size;
                }
                removeFile(file);
                updateTotalProgress();
                if (fileCount <= 0)
                    $upload.addClass("disabled");
                if (fileCount <= 0 && $queue.find("li").length === 0) {
                    setState('pedding');
                }
            };

            uploader.on('all', function (type) {
                switch (type) {
                    case 'uploadFinished':
                        setState('confirm');
                        break;

                    case 'startUpload':
                        setState('uploading');
                        break;

                    case 'stopUpload':
                        setState('paused');
                        break;
                }
            });

            uploader.onError = function (code) {
                alert('Error: ' + code);
            };

            $upload.on('click', function () {
                if ($(this).hasClass('disabled')) {
                    return false;
                }

                if (state === 'ready') {
                    uploader.upload();
                } else if (state === 'paused') {
                    uploader.upload();
                } else if (state === 'uploading') {
                    uploader.stop();
                }
            });
            $info.on('click', '.retry', function () {
                uploader.retry();
            });


            $upload.addClass('state-' + state);
            updateTotalProgress();

            function url2File(url, filename, mimeType) {
                return (fetch(url)
                    .then(function (res) { return res.arrayBuffer(); })
                    .then(function (buf) { return new File([buf], filename, { type: mimeType }); })
                );
            }
        });
        return o;
    };
})(jQuery);