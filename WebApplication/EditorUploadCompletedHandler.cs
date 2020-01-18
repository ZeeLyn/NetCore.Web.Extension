using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using UploadMiddleware.Core;
using UploadMiddleware.Core.Handlers;

namespace WebApplication
{
    public class EditorUploadCompletedHandler : IUploadCompletedHandler
    {
        public async Task<ResponseResult> OnCompleted(IQueryCollection query, IFormCollection form, IHeaderDictionary headers, IReadOnlyList<UploadFileResult> fileData)
        {
            if (query["target"] == "editor")
            {
                return await Task.FromResult(new ResponseResult
                {
                    Content = new
                    {
                        uploaded = 1,
                        url = "/upload" + fileData.FirstOrDefault()?.Url
                    }
                });
            }
            else
                return await Task.FromResult(new ResponseResult
                {
                    Content = fileData.Select(p => p.Url).ToArray()
                });
        }
    }
}
